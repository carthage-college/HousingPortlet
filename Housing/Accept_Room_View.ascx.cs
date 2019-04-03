using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ConfigSettings = Jenzabar.Common.Configuration.ConfigSettings;
using Jenzabar.Common;                          //ObjectFactoryWrapper
using Jenzabar.Common.Mail;                     //ValidEmail()
using Jenzabar.Portal.Framework;
using Jenzabar.Portal.Framework.EmailLogging;   //EmailLogger
using Jenzabar.Portal.Framework.Facade;         //IPortalUserFacade
using Jenzabar.Portal.Framework.Web.UI;
using CUS.OdbcConnectionClass3;

namespace Housing
{
    public partial class Accept_Room_View : PortletViewBase
    {
        #region Define Variables
        //IMPORTANT!! - Because the registration process uses a stored procedure the SQL command begins with EXEC or EXECUTE.
        //The ConnectToERP() method in OdbcConnectionClass3 checks the beginning of the SQL string and if it does not begin with "SELECT", the class does not send the results of the query to the DataTable.
        //The only way to force this behavior is to set the second argument of the constructor (bool? forceSelect) to "true".
        public OdbcConnectionClass3 jicsConn = new OdbcConnectionClass3("JICSDataConnection.config", true);

        public HousingHelper helper = new HousingHelper();
        public string CurrentYear { get { return DateTime.Now.Year.ToString(); } }
        public string NextYear { get { return (int.Parse(CurrentYear) + 1).ToString(); } }
        public string AcademicYear { get { return String.Format("{0} - {1}", CurrentYear, NextYear); } }
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

            //Define values of mailto link
            //this.aMail.Text = helper.HOUSING_ADMIN_EMAIL;
            //this.aMail.NavigateUrl = String.Format("mailto:{0}", helper.HOUSING_ADMIN_EMAIL);

            this.ltlHousingAdminName.Text = helper.HOUSING_ADMIN_NAME;

            //Get information about the selected room
            string roomSQL = "EXECUTE [dbo].[CUS_spHousing_getRoomDetails] @guidRoomSessionID = ?";
            Exception ex = null;
            DataTable dtRoom = null;
            
            List<OdbcParameter> parameters = new List<OdbcParameter>
            {
                new OdbcParameter("roomSessionID", this.ParentPortlet.PortletViewState["RoomSessionID"].ToString())
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
                //this.contentWaitlist.Visible = !room["BuildingCode"].ToString().Contains("OAK");
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
                //Performs several steps as part of the registration process
                //  1) Checks that the user is not already in a room
                //  2) Ensures the room's current gender is undefined or matches the student
                //  3) Validates that there is at least one available bed remaining in the room
                //  4) Generates a response value denoting either success (0) or an error (-1 through -5)
                //Error codes are broken down as follows
                //  -1 [Gender mismatch]
                //  -2 [Capacity exceeded]
                //  -3 [The student is already in a room]
                //  -4 [Invalid student gender]
                //  -5 [Unknown error]
                string registerSQL = "EXECUTE dbo.CUS_spHousingRegisterRoom @uuidStudentID = ?, @uuidRoomSessionID = ?, @strGender = ?, @oaksWaitlist = ?";
                List<OdbcParameter> registerParameters = new List<OdbcParameter>
                {
                      new OdbcParameter("StudentID", PortalUser.Current.Guid)
                    , new OdbcParameter("RoomSessionID", this.ParentPortlet.PortletViewState["RoomSessionID"].ToString())
                    , new OdbcParameter("StudentGender", this.ParentPortlet.PortletViewState["Gender"].ToString())
                    //, new OdbcParameter("Waitlist", this.chkOaksWaitlist.Checked ? "Y" : "N")
                    , new OdbcParameter("Waitlist", "")
                };

                Exception exRegister = null;
                DataTable dtRegister = null;
                int responseCode = -6;

                try
                {
                    dtRegister = jicsConn.ConnectToERP(registerSQL, ref exRegister, registerParameters);
                    if (exRegister != null) { throw exRegister; }
                    if (dtRegister != null)
                    {
                        responseCode = int.Parse(dtRegister.Rows[0]["returnCode"].ToString());
                    }

                    if (responseCode == 0)
                    {
                        //Get information about the selected room
                        string roomSQL = "EXECUTE [dbo].[CUS_spHousing_getRoomDetails] @guidRoomSessionID = ?";
                        Exception ex = null;
                        DataTable dtRoom = null;

                        string smtpAddress = ConfigSettings.Current.SmtpDefaultEmailAddress;
                        PortalUser emailSender = ObjectFactoryWrapper.GetInstance<IPortalUserFacade>().FindByEmail(helper.HOUSING_ADMIN_EMAIL);
                        smtpAddress = Email.GetProperEMailAddress(emailSender.EmailAddress);
                        string emailSubject = "Room Registration";

                        //If the database settings indicate we can send to students, do so. Otherwise we are in test mode and the admin should receive the email.
                        string emailTo = helper.sendEmailOk() ? PortalUser.Current.EmailAddress : helper.GetHousingSetting(HousingHelper.SETTING_KEY_TEST_EMAIL_ADDRESS);

                        List<OdbcParameter> parameters = new List<OdbcParameter>
                        {
                            new OdbcParameter("roomSessionID", this.ParentPortlet.PortletViewState["RoomSessionID"].ToString())
                        };

                        try
                        {
                            //Get results from database
                            dtRoom = jicsConn.ConnectToERP(roomSQL, ref ex, parameters);
                            if (ex != null) { throw ex; }
                            DataRow room = dtRoom.Rows[0];
                            string formattedRoom = String.Format("{0} {1}", room["BuildingName"].ToString(), room["RoomNumber"].ToString());

                            string emailBody = String.Format(
                                @"<p>Congratulations {0}, your reservation for {1} has been entered into our database.</p>
                                    <p>If you have any questions pertaining to your housing for the {2} academic year please contact the Office of Student Life in the Todd Wehr Center.</p>
                                    <p>Thank you for using the Housing Selection Process for {2}.</p>
                                    <p>##########</p>
                                    <p>CARTHAGE COLLEGE<br />HOUSING CONTRACT FOR RESIDENCE HALLS<br />ACADEMIC YEAR {2}</p>
                                    <p>RETURNING STUDENTS<br />
                                    Carthage agrees to provide assigned space in the residence halls for the undersigned student for the {2} academic year and the undersigned student agrees to pay for
                                    said assigned living space on the following terms:</p>
                                    <p>DURATION<br />
                                    This contract shall be for three (3) consecutive terms of the academic year including Term I, J-Term, and Term II. The student shall be entitled to reside in the halls
                                    at the designated time the day prior to classes and ending immediately after the final exam each term, unless given permission otherwise. The only exception to this is
                                    J-Term. Students must be a full-time Carthage student in order to be eligible for on-campus housing. If a student reduces to part-time status during the academic year,
                                    they are still obligated to fulfill their housing contract for the full academic year. The campus does not close between the end of J-Term and the beginning of Term II.
                                    The campus will be closed for the following breaks:</p>
                                    <blockquote>
                                        Thanksgiving - November 26th after 5:00 p.m. through December 1st, 2019 at noon<br />
                                        Christmas - December 13th, 2019 after 5:00 p.m. through January 7, 2020 at noon<br />
                                        Spring Break - March 6th, after 5:00 p.m. through March 15th, 2020 at noon
                                    </blockquote>
                                    <p>The student shall remove all personal possessions from their assigned space upon termination of residence for any reason. Carthage does not store personal possessions
                                    for students. All such personal possessions not removed will be disposed of by Carthage and a cleaning fee assessed to the student.</p>
                                    <p>TERMS<br />
                                    A student must be registered full time to reside in the residence halls during any semester. This equates to a minimum of twelve (12) credits each term, and a minimum of
                                    four (4) credits during J-Term, to be eligible to live in the residence halls. Exception to the above may be made by thevOffice of Residential Life.</p>
                                    <p>CHARGES<br />
                                    The per term charges for rooms will be determined by the Student Accounts Office at Carthage College at a later date. Carthage reserves the right to change the amount of
                                    charge at any time. All students living on campus are required to take a meal plan.</p>
                                    <p>COMMUNITY CHARGES<br />
                                    If damages or vandalism occur and no individual(s) is directly identified as being responsible, the cost of repairs will be equally charged to the members of that wing, floor,
                                    building or group of people most closely related to the damages. These charges will be placed on the student’s monthly statement from Carthage.</p>
                                    <p>ADJUSTMENTS<br />
                                    No refund of room charges shall be made when a student withdraws, is dismissed from Carthage, or is removed from housing for disciplinary reasons.</p>
                                    <p>UNASSIGNED LIVING SPACE<br />
                                    The contract does not constitute a guarantee for a specific room or roommate. Carthage reserves the right to make any changes in room or roommate assignments at any time.</p>
                                    <p>EFFECTIVE DATE<br />
                                    This contract becomes binding when the student completes the on-line housing selection process or when students are administratively assigned to housing if not eligible for
                                    commuter status or a residency exemption. It continues for the entire academic year as long as the student remains in good standing with Carthage.</p>
                                    <p>NO SHOWS<br />
                                    Any student who does not report to their assigned living space 24 hours after the start of classes will automatically lose their assigned space.</p>
                                    <p>RULES AND REGULATIONS<br />
                                    Carthage reserves the right for its authorized personnel to enter student rooms to preserve and protect Carthage property and to assure compliance with state and local
                                    laws or Carthage rules and regulations. In signing this contract, the student agrees to adhere to all rules and regulations governing their behavior as outlined in the
                                    Carthage Student Community Code.</p>
                                    <p>PAYMENTS<br />
                                    All payments hereunder shall be made to the Student Accounts Office.</p>"
                            , PortalUser.Current.FirstName, formattedRoom, AcademicYear);

                            bool emailSuccess = !String.IsNullOrEmpty(emailTo) && (new ValidEmail(emailTo).IsValid) && Email.CreateAndSendMailMessage(smtpAddress, emailTo, emailSubject, emailBody);
                            //If the system is configured to log sent emails and the email was sent successfully create a record of it in the database 
                            if (ConfigSettings.Current.LogEmail && emailSuccess)
                            {
                                List<PortalUser> recipients = new List<PortalUser>();
                                recipients.Add(PortalUser.Current);
                                new EmailLogger().Log(emailSender, recipients, null, null, null, emailSubject, emailBody, null, null);
                            }

                            this.ParentPortlet.PortletViewState["Registered"] = true;
                            this.ParentPortlet.NextScreen("SendInvitations");
                        }
                        catch (Exception ee)
                        {
                            this.ParentPortlet.ShowFeedback(FeedbackType.Error, String.Format("{0}<br /><br />{1}", ee.Message, ee.InnerException));
                        }
                    }
                    else
                    {
                        //If the response code from the registration stored procedure returned an error code, decipher it and notify the user
                        string errorText = "";
                        switch (responseCode)
                        {
                            case -1:
                                errorText = "This room has been assigned to someone of the opposite gender. Please choose another room.";
                                break;
                            case -2:
                                errorText = "There are no more available beds in this room. Please choose another room.";
                                break;
                            case -3:
                                errorText = "You have already signed up for a room.";
                                break;
                            case -4:
                                errorText = "Please return to the main housing screen and ensure that the correct gender is associated with your account.";
                                break;
                            case -6:
                                if (dtRegister != null)
                                {
                                    foreach (DataRow dr in dtRegister.Rows)
                                    {
                                        errorText += "Row:<br />";
                                        foreach (DataColumn dc in dtRegister.Columns)
                                        {
                                            errorText += String.Format("{0}: {1}<br />", dc.ColumnName, dr[dc.ColumnName].ToString());
                                        }
                                    }
                                }
                                else
                                {
                                    errorText = "dtRegister was null";
                                }
                                break;
                            default:
                                errorText = String.Format("Response code {0}: An unknown error occurred while attempting to register you for your room. Please try again.", responseCode);
                                break;
                        }
                        this.ParentPortlet.ShowFeedback(
                            FeedbackType.Error,
                            String.Format(
                                "{0}<br />StudentID: {1}<br />RoomSessionID: {2}<br />Gender: {3}",
                                errorText,
                                PortalUser.Current.Guid,
                                this.ParentPortlet.PortletViewState["RoomSessionID"].ToString(),
                                this.ParentPortlet.PortletViewState["Gender"].ToString()
                            )
                        );
                    }
                }
                catch (Exception ee)
                {
                    this.ParentPortlet.ShowFeedback(FeedbackType.Error, String.Format("Error while registering:<br />{0}<br /><br />{1}", ee.Message, ee.InnerException));
                }
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
            this.ParentPortlet.PortletViewState["RoomSessionID"] = null;
            
            //Send the user to the room selection screen
            this.ParentPortlet.PreviousScreen("AvailabilityRoom");
        }
    }
}