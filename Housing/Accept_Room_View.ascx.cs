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

            //Get information about the selected room
            string roomSQL = @"
                SELECT
                    Building.BuildingName, Building.BuildingCode, Room.RoomNumber
                FROM
                    CUS_Housing_Room    Room    INNER JOIN  CUS_Housing_Building    Building    ON  Room.BuildingID         =   Building.BuildingID
                                                INNER JOIN  CUS_Housing_RoomSession RoomSession ON  Room.RoomID             =   RoomSession.RoomID
                                                                                                AND RoomSession.HousingYear =   YEAR(GETDATE())
                WHERE
                    RoomSession.RoomSessionID   =   ?";
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
                string registerSQL = "EXECUTE dbo.CUS_spHousingRegisterRoom @uuidStudentID = ?, @uuidRoomSessionID = ?, @strGender = ?";
                List<OdbcParameter> registerParameters = new List<OdbcParameter>
                {
                      new OdbcParameter("StudentID", PortalUser.Current.Guid)
                    , new OdbcParameter("RoomSessionID", this.ParentPortlet.PortletViewState["RoomSessionID"].ToString())
                    , new OdbcParameter("StudentGender", this.ParentPortlet.PortletViewState["Gender"].ToString())
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
                        string roomSQL = @"
                            SELECT
                                HB.BuildingName, HB.BuildingCode, HR.RoomNumber, HRS.Gender,
                                (
                                    SELECT
                                        COUNT(*)
                                    FROM
                                        CUS_Housing_RoomStudent HRStu
                                    WHERE
                                        HRStu.RoomSessionID =   HRS.RoomSessionID
                                )   AS  NewOccupants
                            FROM
                                CUS_Housing_Room    HR  INNER JOIN  CUS_Housing_Building    HB  ON  HR.BuildingID   =   HB.BuildingID
                                                        INNER JOIN  CUS_Housing_RoomSession HRS ON  HR.RoomID       =   HRS.RoomID
                                                                                                AND HRS.HousingYear =   YEAR(GETDATE())
                            WHERE
                                HRS.RoomSessionID   =   ?";
                        Exception ex = null;
                        DataTable dtRoom = null;

                        string smtpAddress = ConfigSettings.Current.SmtpDefaultEmailAddress;
                        PortalUser emailSender = ObjectFactoryWrapper.GetInstance<IPortalUserFacade>().FindByEmail("nfleming@carthage.edu");
                        smtpAddress = emailSender.EmailAddress;
                        string emailTo = "mkishline@carthage.edu", emailSubject = "Room Registration";

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
                                    <p>Thank you for using the Housing Selection Process for {3}.</p>"
                            , PortalUser.Current.FirstName, formattedRoom, AcademicYear, AcademicYear
                            );

                            //TODO: Change emailTo to PortalUser.Current.EmailAddress
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