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
        OdbcConnectionClass3 jicsConn = new OdbcConnectionClass3("JICSDataConnection.config", true);
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
                //Get RA rooms that are available based on the student's gender
                string raSQL = "EXECUTE [dbo].[CUS_spHousing_getBuildingsRA] @strGender = ?";

                //Get buildings that have available rooms based on the the student's greek affiliations or current room assignment
                string greekSquatterSQL = "EXECUTE [dbo].[CUS_spHousing_getBuildingsGreekSquatter] @strGender = ?, @strGreekOrg = ?, @guidStudentID = ?";

                //Get the building information
                string buildingSQL = "EXECUTE [dbo].[CUS_spHousing_getBuildings] @strGender = ?";

                List<OdbcParameter> parameters = new List<OdbcParameter>
                {
                    new OdbcParameter("StudentGender", this.ParentPortlet.PortletViewState["Gender"].ToString())
                };

                if (bool.Parse(this.ParentPortlet.PortletViewState["IsTodayRA"].ToString()))
                {
                    buildingSQL = raSQL;
                }
                else if (this.ParentPortlet.PortletViewState["DayIndex"].ToString() == "0")
                {
                    buildingSQL = greekSquatterSQL;
                    parameters.Add(new OdbcParameter("GreekInvl", this.ParentPortlet.PortletViewState["GreekID"].ToString()));
                    parameters.Add(new OdbcParameter("StudentID", PortalUser.Current.Guid));
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