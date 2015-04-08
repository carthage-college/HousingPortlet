using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Jenzabar.Portal.Framework;
using Jenzabar.Portal.Framework.Web.UI;
using CUS.OdbcConnectionClass3;

namespace Housing
{
    public partial class Availability_Building_View : PortletViewBase
    {
        OdbcConnectionClass3 jicsConn = new OdbcConnectionClass3("JICSDataConnection.config");
        public override string ViewName { get { return "Choose A Building"; } }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (this.IsFirstLoad)
            {
                InitScreen();
            }
        }

        private void InitScreen()
        {
            Exception exBuilding = null;
            DataTable dtBuilding = null;

            try
            {
                string greekSquatterSQL = String.Format(@"
                    /* Select the greek rooms that correspond to the student's greek affiliation */
                    SELECT
	                    GreekBuildings.BuildingID, GreekBuildings.BuildingName, GreekBuildings.BuildingCode
                    FROM
	                    (
		                    SELECT
                                HB.BuildingID, HB.BuildingName, HB.BuildingCode
		                    FROM
			                    CUS_Housing_Room	HR	INNER JOIN  CUS_Housing_Building	    HB      ON  HR.BuildingID	    =   HB.BuildingID
									                    INNER JOIN	CUS_Housing_RoomSession     HRS     ON  HR.RoomID           =	HRS.RoomID
                                                                                                        AND	HRS.HousingYear 	=	YEAR(GETDATE())
									                    LEFT JOIN	CUS_Housing_RoomStudent	    HRStu	ON	HRS.RoomSessionID	=   HRStu.RoomSessionID
									                    LEFT JOIN	CUS_HousingSelectionGreek	HSG	    ON	HRS.GreekID         =	HSG.greekid
                            WHERE
                                HRS.Gender	IN	(?,'')
                            AND
                                HSG.invl    =   ?
		                    GROUP BY
                                HB.BuildingID, HB.BuildingName, HB.BuildingCode
	                    )	AS  GreekBuildings
                    UNION
                    /* Select the student's current room */
                    SELECT
                        HB.BuildingID, HB.BuildingName, HB.BuildingCode
                    FROM
	                    CUS_Housing_Room	HR  INNER JOIN  CUS_Housing_Building	    HB	    ON  HR.BuildingID	    =   HB.BuildingID
							                    INNER JOIN	CUS_Housing_RoomSession	    HRS	    ON	HR.RoomID	        =	HRS.RoomID
                                                                                                AND HRS.HousingYear     =	YEAR(GETDATE())
							                    INNER JOIN  CUS_Housing_SessionOccupant HSO     ON  HRS.RoomSessionID   =   HSO.RoomSessionID
							                    LEFT JOIN   CUS_Housing_RoomStudent     HRStu	ON	HRS.RoomSessionID	=	HRStu.RoomSessionID
							                    INNER JOIN	FWK_User	                FU	    ON  HSO.StudentID	    =	FU.ID
                    WHERE
                        FU.HostID = {0}
                    ORDER BY
                        BuildingName
                ", PortalUser.Current.HostID);

                //Get the building information
                string buildingSQL = @"
                    SELECT
		                HB.BuildingID, HB.BuildingName, HB.BuildingCode
	                FROM
		                CUS_Housing_Building	HB	INNER JOIN	CUS_Housing_Room		HR	ON	HB.BuildingID	=	HR.BuildingID
									                INNER JOIN	CUS_Housing_RoomSession	HRS	ON	HR.RoomID		=	HRS.RoomID
	                WHERE
		                HRS.Gender		IN	('', ?)
	                GROUP BY
		                HB.BuildingID, HB.BuildingName, HB.BuildingCode
	                ORDER BY
		                HB.BuildingName
                ";
                List<OdbcParameter> parameters = new List<OdbcParameter>
                {
                    new OdbcParameter("StudentGender", this.ParentPortlet.PortletViewState["Gender"].ToString())
                };

                if (this.ParentPortlet.PortletViewState["DayIndex"].ToString() == "0")
                {
                    buildingSQL = greekSquatterSQL;
                    parameters.Add(new OdbcParameter("GreekInvl", this.ParentPortlet.PortletViewState["GreekID"].ToString()));
                }

                //Perform the query
                dtBuilding = jicsConn.ConnectToERP(buildingSQL, ref exBuilding, parameters);
                if (exBuilding != null) { throw exBuilding; }

                if (dtBuilding != null)
                {
                    //Insert proper grammar based on whether a single record or multiple records were returned
                    this.ltlBuildingPlural.Text = dtBuilding.Rows.Count == 1 ? "" : "s";
                    this.ltlBuildingPlural2.Text = dtBuilding.Rows.Count == 1 ? "has" : "have";

                    //Bind the dataset to the bulleted list
                    this.bulletedBuildings.DataSource = dtBuilding;
                    this.bulletedBuildings.DataTextField = "BuildingName";
                    this.bulletedBuildings.DataValueField = "BuildingID";
                    this.bulletedBuildings.DataBind();
                }
            }
            catch(Exception ex)
            {
                //If there was an error when connecting to the database or executing the SQL, display the error
                this.ParentPortlet.ShowFeedback(FeedbackType.Error, String.Format("{0}<br /><br />{1}", ex.Message, ex.InnerException.ToString()));
            }
            finally
            {
                //At the end of the process, make sure that the database connection has been closed
                if (jicsConn.IsNotClosed()) { jicsConn.Close(); }
            }
        }

        protected void bulletedBuildings_Click(object sender, BulletedListEventArgs e)
        {
            //Identify which building was clicked
            ListItem clickedItem = this.bulletedBuildings.Items[e.Index];

            //Store the code of the building in the viewstate so it may be used on the next screen
            this.ParentPortlet.PortletViewState["Building"] = clickedItem.Value;

            //Go to the screen where the user may select their bed from a list of available rooms
            this.ParentPortlet.NextScreen("AvailabilityRoom");
        }
    }
}