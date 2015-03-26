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
            Exception ex = null;
            DataTable dt = null;
            List<OdbcParameter> parameters = new List<OdbcParameter>
            {
                new OdbcParameter("StudentGender", this.ParentPortlet.PortletViewState["Gender"].ToString())
            };

            try
            {
                //Perform the query
                dt = jicsConn.ConnectToERP(buildingSQL, ref ex, parameters);
                if (ex != null) { throw ex; }
            }
            catch(Exception ee)
            {
                //If there was an error when connecting to the database or executing the SQL, display the error
                this.ParentPortlet.ShowFeedback(FeedbackType.Error, String.Format("{0}<br /><br />{1}", ee.Message, ee.InnerException.ToString()));
            }
            finally
            {
                //At the end of the process, make sure that the database connection has been closed
                if (jicsConn.IsNotClosed()) { jicsConn.Close(); }
            }

            if (dt != null)
            {
                //Bind the dataset to the bulleted list
                this.bulletedBuildings.DataSource = dt;
                this.bulletedBuildings.DataTextField = "BuildingName";
                this.bulletedBuildings.DataValueField = "BuildingID";
                this.bulletedBuildings.DataBind();
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