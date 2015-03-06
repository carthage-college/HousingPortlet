﻿using System;
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
            string buildingSQL = "SELECT BuildingID, BuildingName, BuildingCode FROM CUS_Housing_Building ORDER BY BuildingName";
            Exception ex = null;
            DataTable dt = null;

            try
            {
                //Perform the query
                dt = jicsConn.ConnectToERP(buildingSQL, ref ex);
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
                //TODO: Should there be a secondary step that disables buildings based on user-data or should those building be excluded from the list through the query?
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