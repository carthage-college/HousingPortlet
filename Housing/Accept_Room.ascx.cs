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
    public partial class Accept_Room : PortletViewBase
    {
        #region Define Variables
        public OdbcConnectionClass3 jicsConn = new OdbcConnectionClass3("JICSDataConnection.config");
        public string CurrentYear { get { return DateTime.Now.Year.ToString(); } }
        public string NextYear { get { return (int.Parse(CurrentYear) + 1).ToString(); } }
        #endregion

        //Set page title
        public override string ViewName { get { return "Accept Terms and Conditions"; } }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (this.IsFirstLoad)
            {
                InitScreen();
            }
        }

        protected void InitScreen()
        {
            //Populate label placeholders with academic year
            this.ltlApartmentYear.Text = this.ltlContractYear.Text = this.ltlContractYear2.Text =
                this.ltlParkingYear.Text = this.ltlParkingYear2.Text = this.ltlParkingYear3.Text = String.Format("{0} - {1}", CurrentYear, NextYear);
            //Populate label placeholders with current or next year
            this.ltlThisYear1.Text = this.ltlThisYear2.Text = CurrentYear;
            this.ltlNextYear1.Text = this.ltlNextYear2.Text = NextYear;
            //Disable the submit button until the student checks the box agreeing to the terms and conditions
            //this.btnSubmit.Enabled = false;

            //Get information about the selected room
            string roomSQL = "SELECT Building.BuildingName, Building.BuildingCode, Room.RoomNumber FROM CUS_Housing_Room Room INNER JOIN CUS_Housing_Building Building ON Room.BuildingID = Building.BuildingID WHERE RoomID = ?";
            Exception ex = null;
            DataTable dtRoom = null;
            
            List<OdbcParameter> parameters = new List<OdbcParameter>
            {
                new OdbcParameter("roomID", this.ParentPortlet.PortletViewState["RoomID"].ToString())
            };
            try
            {
                //Get results from database
                dtRoom = jicsConn.ConnectToERP(roomSQL, ref ex, parameters);
                DataRow room = dtRoom.Rows[0];
                
                //Load screen with data from query results
                string formattedRoomNumber = String.Format("{0}, {1}", room["BuildingName"].ToString(), room["RoomNumber"].ToString());
                this.ltlNewBuildingRoom.Text = formattedRoomNumber;
                this.btnSubmit.Text = String.Format("Sign up for {0}", formattedRoomNumber);

                //Set visibility of controls based on query results
                //Only show the apartment contract if the student signed up for the apartments
                this.panelApartmentContract.Visible = room["BuildingCode"].ToString() == "APT";
                //Only show the Oaks waitlist if the student is not signing up for the Oaks
                this.contentWaitlist.Visible = !room["BuildingCode"].ToString().Contains("OAK");
            }
            catch (Exception ee)
            {
                //Display exception if caught
                this.ParentPortlet.ShowFeedback(FeedbackType.Error, String.Format("{0}<br /><br />{1}", ee.InnerException, ee.Message));
            }
            finally
            {
                //Always close the database connection
                if (jicsConn.IsNotClosed()) { jicsConn.Close(); }
            }
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            if (this.chkAgree.Checked)
            {
                this.ParentPortlet.PortletViewState["Registered"] = true;
                this.ParentPortlet.NextScreen("SendInvitations");
            }
            else
            {
                this.ParentPortlet.ShowFeedback(FeedbackType.Message, "You must agree to the terms and conditions before you can sign up for your room.");
            }
        }

        /// <summary>
        /// The user wishes to return to the room selection screen where they will pick a different room.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void lnkPickRoom_Click(object sender, EventArgs e)
        {
            //Remove the currently selected room from the viewstate
            this.ParentPortlet.PortletViewState["RoomID"] = null;
            
            //Send the user to the room selection screen
            this.ParentPortlet.PreviousScreen("AvailabilityRoom");
        }
    }
}