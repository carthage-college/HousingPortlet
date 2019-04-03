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
    public partial class Default_View : PortletViewBase
    {
        public override string ViewName { get { return "Housing Sign-up Application"; } }

        #region Public Members
        //Database connections
        OdbcConnectionClass3 odbcConn = new OdbcConnectionClass3("ERPDataConnection.config");
        OdbcConnectionClass3 jicsConn = new OdbcConnectionClass3("JICSDataConnection.config", true);
        
        //Library of helper functions
        HousingHelper helper = new HousingHelper();

        //Constants
        public string SESSION_SPRING { get { return "RC"; } }
        public string SESSION_FALL { get { return "RA"; } }
        public string CurrentYear { get { return DateTime.Now.Year.ToString(); } }
        public string NextYear { get { return (int.Parse(CurrentYear) + 1).ToString(); } }
        public string BeginInvolvement { get { return "1/1/" + (int.Parse(CurrentYear) - 3).ToString(); } }
        public int UserID
        {
            get
            {
                int userID = 0;
                int.TryParse(PortalUser.Current.HostID, out userID);
                return userID;
            }
        }
        public int CONFIG_END_HOUR
        {
            get
            {
                string CONFIG_END_TIME = helper.GetHousingSetting(HousingHelper.SETTING_KEY_END_HOUR);
                int endTime = 0;
                if (!int.TryParse(CONFIG_END_TIME, out endTime))
                {
                    endTime = 15;
                }
                return endTime;
            }
        }
        public int DayIndex;
        public bool IsTodayRA;
        #endregion

        protected override void OnInit(EventArgs e)
        {
            InitScreen();
            base.OnInit(e);
        }

        protected void InitScreen()
        {
            //Get the data about the student from CX
            DataTable dtStudentData = GetCXData();
            
            //Create a DataRow object to store the details retrieved from CX
            DataRow drCxData = null;

            //If a recordset was returned by GetCXData(), assign the first row (there should only be one) to the DataRow object
            if (dtStudentData != null && dtStudentData.Rows.Count > 0)
            {
                drCxData = dtStudentData.Rows[0];
            }

            //Get a DataRow object containing information about the room the student selected
            DataRow drUpcomingHousing = GetUpcomingHousing();

            //If the DataRow object is null, the student has not yet signed up for a room
            bool isRegisteredForHousing = drUpcomingHousing != null;

            //Retrieve invitations extended by this student
            GetExtendedInvitations();

            bool isCommuter = studentIsCommuter(drCxData);

            //If the query returned results, load the data into the controls on the page
            if (drCxData != null && drCxData["include"].ToString().Trim() != "GPS")
            {
                //Load the student's name
                this.ltlStudentName.Text = String.Format("{0} {1}", drCxData["firstname"].ToString(), drCxData["lastname"].ToString());

                //If the student has already registered for housing, show the registered panel
                this.panelRegistered.Visible = isRegisteredForHousing;

                //If the student has not yet registered for housing, show the unregistered panel
                this.panelUnregistered.Visible = !isRegisteredForHousing;

                //Display the name of the greek organization to which the student belongs
                this.ltlGreekStatus.Text = drCxData["greek_name"].ToString().Length > 0 ? String.Format("a member of {0}", drCxData["greek_name"].ToString()) : "not a member of a residential fraternity or sorority";
                
                //Store the CX "invl" field in the viewstate
                this.ParentPortlet.PortletViewState["GreekID"] = drCxData["greekid"].ToString();

                //bool hasHold = (drCxData["advPayHold"].ToString() + drCxData["unbal_hold"].ToString()).Length > 0;
                bool hasHold = drCxData["unbal_hold"].ToString().Length > 0;
                this.ltlHold.Text = hasHold ? "not" : "";
                this.contentHoldDetail.Visible = hasHold;

                //By moving housing sign-up to a period before fall course registration, checking whether as student signed up for courses becomes an invalid condition
                //bool isFallRegistered = int.Parse(drCxData["registered_hours"].ToString()) >= 12;
                bool isFallRegistered = true;
                
                //A student must be registered for 12 or more credits in the upcoming semester
                //this.ltlRegistered.Text = isFallRegistered ? "" : "not";
                
                //If the student has not registered for the fall, display an explanation
                //this.contentRegisteredDetail.Visible = !isFallRegistered;
                
                //this.ltlRegisteredYear.Text = String.Format("{0}", CurrentYear);
                //Determine the proper display of student's gender
                switch (drCxData["sex"].ToString())
                {
                    case "M":
                        this.ltlGender.Text = "You are male";
                        this.ParentPortlet.PortletViewState["Gender"] = "M";
                        break;
                    case "F":
                        this.ltlGender.Text = "You are female";
                        this.ParentPortlet.PortletViewState["Gender"] = "F";
                        break;
                    default:
                        this.ltlGender.Text = "Our records don't indicate your gender";
                        this.ParentPortlet.PortletViewState["Gender"] = "";
                        break;
                }
                //Display the total number of credit hours earned by the student over their academic career
                this.ltlCareerCredits.Text = drCxData["career_hours"].ToString();

                //Show the building and room number the student currently occupies
                this.ltlCurrentHousing.Text = String.Format("{0} {1}", drCxData["bldg"].ToString(), drCxData["room"].ToString());
                this.panelCommuter.Visible = this.ltlNotResident.Visible = isCommuter;
                //this.panelCurrentHousing.Visible = this.panelUnregistered.Visible = !isCommuter;
                this.panelCurrentHousing.Visible = !isCommuter;
                if (helper.IS_PRODUCTION && this.panelUnregistered.Visible)
                {
                    this.panelUnregistered.Visible = !isCommuter;
                }

                //Get the start dates for RAs and the general student population
                DateTime? raStartDate = null, generalStartDate = null;
                bool hasDates = GetStartDates(out raStartDate, out generalStartDate);

                DayIndex = DateDiff(generalStartDate.Value);
                IsTodayRA = DateDiff(raStartDate.Value) == 0;

                string message = "";
                bool isStudentRA = studentIsRA();
                //bool isValidTime = (isStudentRA && IsTodayRA) || IsValidTimeToRegister(DayIndex, int.Parse(drCxData["career_hours"].ToString()), drCxData["greek_name"].ToString(), out message);
                //2019-03-20 (MK) - Amber has requested RA's not be able to sign up for their room. She will manually assign them after housing has concluded.
                bool isValidTime = !isStudentRA && IsValidTimeToRegister(DayIndex, int.Parse(drCxData["career_hours"].ToString()), drCxData["greek_name"].ToString(), out message);

                bool hasDefinedGender = this.ParentPortlet.PortletViewState["Gender"].ToString() != "";
                //A student must meet the following criteria to register:
                //  1. Must be registered for >= 12 credits in the fall term
                //  2. May not currently be a commuter
                //  3. Has no holds on their account
                //  4. Has a defined gender listed in CX (must be "M" or "F")
                bool mayRegister = isFallRegistered && !isCommuter && !hasHold && hasDefinedGender;

                this.panelAvailability.Visible = !isRegisteredForHousing;

                //Only limit access to availability screens in production
                this.lnkAvailability.Visible = helper.IS_PRODUCTION ? isValidTime && mayRegister : true;

                this.ltlCannotRegister.Visible = isValidTime && !mayRegister;
                this.panelOverview.Visible = !isValidTime;
                //If it is not the student's time to sign up for a room, calculate when their signup time begins
                if (!isValidTime)
                {
                    this.ltlGreekSquatterDay.Text = String.Format("{0:dddd MMMM d}", generalStartDate.Value);
                    this.ltlRegisterGreek.Visible = drCxData["greekid"].ToString().Trim().Length > 0;
                    DateTime firstRegister = GetCreditHourStartTime(generalStartDate.Value, int.Parse(drCxData["career_hours"].ToString()));
                    this.ltlFirstRegisterDateTime.Text = String.Format("{0:h:mm tt} on {1:dddd, MMMM d}", firstRegister, firstRegister);
                }

                this.ParentPortlet.PortletViewState["maySignUp"] = isValidTime && mayRegister;
                //If the student hasn't signed up for housing, retrieve and display the invitations extended to them
                if (!isRegisteredForHousing)
                {
                    if (drCxData != null)
                    {
                        DataTable dtInvitations = GetInvitations(drCxData["sex"].ToString(), mayRegister);
                    }
                }
                //If the student has signed up for housing, display their roommates
                else
                {
                    this.panelInvitations.Visible = false;
                    GetRoommates(drUpcomingHousing["RoomSessionID"].ToString());
                }
            }
            else
            {
                //Suppress the display of all the various responsive components of the page
                this.panelAvailability.Visible = this.panelCommuter.Visible = this.panelCurrentHousing.Visible = this.panelExtendedInvitations.Visible = this.panelExtendedInvitations2.Visible =
                    this.panelInvitations.Visible = this.panelOverview.Visible = this.panelRegistered.Visible = this.panelUnregistered.Visible = this.welcome.Visible = false;

                //Initialize message to be displayed
                string noSignupMessage = "";

                //Is the student in one of the GPS programs?
                bool isGPS = drCxData != null && drCxData["include"].ToString().Trim() == "GPS";
                
                //GPS students receive a message, other scenarios display as an error
                FeedbackType fbType = isGPS ? FeedbackType.Message : FeedbackType.Error;

                if (isGPS)
                {
                    noSignupMessage = String.Format("This portlet is only needed by undergraduate residents. If you require housing, please contact {0} <a href='mailto:{1}'>{1}</a>.", helper.HOUSING_ADMIN_NAME, helper.HOUSING_ADMIN_EMAIL);
                }
                else if (dtStudentData != null && dtStudentData.Rows.Count == 0)
                {
                    noSignupMessage = "No records were found that matched your ID.";
                }
                else
                {
                    noSignupMessage = String.Format("We were unable to retrieve your information. Please contact {0} <a href='mailto:{1}'>{1}</a> to resolve this issue.", helper.HOUSING_ADMIN_NAME, helper.HOUSING_ADMIN_EMAIL);
                }
                this.ParentPortlet.ShowFeedback(fbType, noSignupMessage);
            }
            /*
            else if (drCxData != null && drCxData["include"].ToString().Trim() == "GPS")
            {
                this.panelAvailability.Visible = this.panelCommuter.Visible = this.panelCurrentHousing.Visible = this.panelExtendedInvitations.Visible =
                    this.panelInvitations.Visible = this.panelOverview.Visible = this.panelRegistered.Visible = this.panelUnregistered.Visible = this.welcome.Visible = false;
                this.ParentPortlet.ShowFeedback(FeedbackType.Message, String.Format("This portlet is only needed by undergraduate residents. If you require housing, please contact {0} <a href='mailto:{1}'>{1}</a>.", helper.HOUSING_ADMIN_NAME, helper.HOUSING_ADMIN_EMAIL));
            }
            else if (dtStudentData != null && dtStudentData.Rows.Count == 0)
            {
                this.panelAvailability.Visible = this.panelCommuter.Visible = this.panelCurrentHousing.Visible = this.panelExtendedInvitations.Visible =
                    this.panelInvitations.Visible = this.panelOverview.Visible = this.panelRegistered.Visible = this.panelUnregistered.Visible = this.welcome.Visible = false;
                this.ParentPortlet.ShowFeedback(FeedbackType.Error, "No records were found that matched your ID.");
            }
            else
            {
                this.panelAvailability.Visible = this.panelCommuter.Visible = this.panelCurrentHousing.Visible = this.panelExtendedInvitations.Visible =
                    this.panelInvitations.Visible = this.panelOverview.Visible = this.panelRegistered.Visible = this.panelUnregistered.Visible = this.welcome.Visible = false;
                this.ParentPortlet.ShowFeedback(FeedbackType.Error, String.Format("We were unable to retrieve your information. Please contact {0} <a href='mailto:{1}'>{1}</a> to resolve this issue.", helper.HOUSING_ADMIN_NAME, helper.HOUSING_ADMIN_EMAIL));
            }
            */

            if (!helper.IS_PRODUCTION)
            {
                bool isProduction = false;
                bool.TryParse(helper.GetHousingSetting(HousingHelper.SETTING_KEY_IS_PRODUCTION), out isProduction);

                bool sendEmail = false;
                bool.TryParse(helper.GetHousingSetting(HousingHelper.SETTING_KEY_SEND_EMAIL), out sendEmail);

                DateTime? generalStart = null, raStart = null;
                bool startDateSuccess = GetStartDates(out raStart, out generalStart);

                string emailTo = helper.GetHousingSetting(HousingHelper.SETTING_KEY_TEST_EMAIL_ADDRESS);
                string msg = String.Format("Found the following:<br />Is production: {0}<br />Send email: {1}<br />Email to: {2}<br />Email determination: {3}<br />Day: {4}; Index: {5} (isRA: {6})",
                    isProduction.ToString(), sendEmail.ToString(), emailTo, (helper.sendEmailOk() ? "Okay to email students" : "Only email admins"), generalStart, DayIndex.ToString(), IsTodayRA.ToString());

                //this.ParentPortlet.ShowFeedback(FeedbackType.Message, msg);
                this.errMsg.ErrorMessage = msg;
            }
        }

        protected void lnkAvailability_Click(object sender, EventArgs e)
        {
            //Set session variable for the current day
            this.ParentPortlet.PortletViewState["DayIndex"] = DayIndex;

            //Set boolean value tracking whether today is the RA signup day
            this.ParentPortlet.PortletViewState["IsTodayRA"] = IsTodayRA;
            
            //Clicking on the "availability" link takes the user to the building/room selection screen
            this.ParentPortlet.NextScreen("AvailabilityBuilding");
        }

        private void GetRoommates(string roomSessionID)
        {
            string roommatesSQL = "EXECUTE [dbo].[CUS_spHousing_getRoommates] @guidRoomSessionID = ?";
            Exception exRoommates = null;
            DataTable dtRoommates = null;
            List<OdbcParameter> roommatesParam = new List<OdbcParameter>
            {
                new OdbcParameter("RoomSessionID", roomSessionID)
            };

            try
            {
                dtRoommates = jicsConn.ConnectToERP(roommatesSQL, ref exRoommates, roommatesParam);
                if (exRoommates != null) { throw exRoommates; }
                if (dtRoommates != null && dtRoommates.Rows.Count > 0)
                {
                    dtRoommates.Columns.Add("DisplayRoommate", typeof(string), "FirstName + ' ' + LastName");
                    this.bulletedRoommates.DataSource = dtRoommates;
                    this.bulletedRoommates.DataTextField = "DisplayRoommate";
                    this.bulletedRoommates.DataBind();
                }
            }
            catch (Exception ex)
            {
                this.ParentPortlet.ShowFeedback(FeedbackType.Error, String.Format("{0}<br /><br />{1}", ex.Message, ex.InnerException));
            }
        }

        private DataTable GetCXData()
        {
            string studentSQL = String.Format(@"
                    SELECT
                        id_rec.id, TRIM(id_rec.firstname) AS firstname, TRIM(id_rec.lastname) AS lastname, profile_rec.sex, ROUND(NVL(stu_stat_rec.cum_earn_hrs, 0)) AS career_hours, TRIM(NVL(curbldg.txt,'CMTR')) AS bldg,
                        TRIM(NVL(stu_serv_rec.room,'CMTR')) AS room, NVL(stu_acad_rec.reg_hrs, 0) AS registered_hours, NVL(ADVpay.hld,'') AS advpayhold, NVL(UNBal.hld,'') AS unbal_hold,
                        NVL(invl_rec.invl,'') AS greekid, TRIM(NVL(invl_rec.org,'')) AS greek_name,
                        CASE	sprAcad.subprog
    	                    WHEN	'TRAD'	THEN	'UNDG'
    	                    WHEN	'TRAP'	THEN	'UNDG'
    					                    ELSE	'GPS'
                        END	AS	include
                    FROM id_rec	INNER JOIN	profile_rec			        ON	id_rec.id			=	profile_rec.id
                                LEFT JOIN	stu_stat_rec		        ON	id_rec.id			=	stu_stat_rec.id
                                										AND	stu_stat_rec.prog	=	'UNDG'
                                LEFT JOIN	stu_serv_rec		        ON	id_rec.id			=	stu_serv_rec.id
											                            AND	stu_serv_rec.sess	=	?
											                            AND	stu_serv_rec.yr		=	{0}
                                LEFT JOIN   bldg_table      curbldg     ON  stu_serv_rec.bldg   =   curbldg.bldg
			                    LEFT JOIN	stu_acad_rec		        ON	id_rec.id			=	stu_acad_rec.id
											                            AND	stu_acad_rec.sess	=	?
											                            AND	stu_acad_rec.yr		=	{0}
								LEFT JOIN	stu_acad_rec	sprAcad		ON	id_rec.id			=	sprAcad.id
																		AND	sprAcad.sess		=	?
																		AND	sprAcad.yr			=	{0}
			                    LEFT JOIN	hold_rec	    ADVpay	    ON	id_rec.id			=	ADVpay.id
											                            AND	TODAY			BETWEEN	ADVpay.beg_date	AND	NVL(ADVpay.end_date, TODAY)
											                            AND	ADVpay.hld			=	'APAY'
			                    LEFT JOIN	hold_rec	    UNBal       ON	id_rec.id			=	UNBal.id
											                            AND	TODAY			BETWEEN	UNBal.beg_date	AND	NVL(UNBal.end_date, TODAY)
											                            AND	UNBal.hld			=	'UBAL'
                                LEFT JOIN   involve_rec     invl_rec    ON  id_rec.id           =   invl_rec.id
                                                                        AND NVL(invl_rec.invl,'')    IN  ('','S007','S025','S045','S061','S063','S092','S141','S152','S165','S168','S189','S190','S192','S194','S276','S277','S282')
                                                                        AND NVL(invl_rec.end_date, TODAY)   >=  TODAY
                                                                        AND YEAR(invl_rec.beg_date)   =  {0}
                    WHERE id_rec.id	=	{1}
                ", CurrentYear, UserID.ToString());

            List<OdbcParameter> parameters = new List<OdbcParameter>
            {
                  new OdbcParameter("SpringSess", SESSION_SPRING)
                , new OdbcParameter("FallSess", SESSION_FALL)
                , new OdbcParameter("SpringSess2", SESSION_SPRING)
            };

            Exception exCxData = null;
            DataTable dtCxData = null;

            try
            {
                //Attempt to execute the query
                dtCxData = odbcConn.ConnectToERP(studentSQL, ref exCxData, parameters);

                //If the SQL (with parameters) generated an error, throw an exception
                if (exCxData != null) { throw exCxData; }
            }
            catch (Exception ee)
            {
                //If an exception was thrown, display the message to the user.
                //TODO: Consider turning this off unless the user is an administrator. Alternatively, throw custom exceptions where possible with more readable errors.
                this.ParentPortlet.ShowFeedback(FeedbackType.Error, String.Format("Student data exception: {0}<br /><br />{1}", ee.Message, ee.InnerException.ToString()));
            }
            finally
            {
                //Regardless of whether the data loaded correctly or an error occurred, if the connection is still open, close it.
                if (odbcConn.IsNotClosed()) { odbcConn.Close(); }
            }
            return dtCxData;
        }

        private bool GetStartDates(out DateTime? raStartDate, out DateTime? generalStartDate)
        {
            raStartDate = null;
            generalStartDate = null;

            //Select start date for RAs (isRA = 1) and general student population (isRA = 0)
            string getStartDateSQL = "EXECUTE [dbo].[CUS_spHousing_getStartDates]";
            Exception exStartDate = null;
            DataTable dtStartDate = null;

            try
            {
                dtStartDate = jicsConn.ConnectToERP(getStartDateSQL, ref exStartDate);
                if (exStartDate != null) { throw exStartDate; }
                if (dtStartDate.Rows.Count == 2)
                {
                    raStartDate = DateTime.Parse(dtStartDate.Rows[0]["startdate"].ToString());
                    generalStartDate = DateTime.Parse(dtStartDate.Rows[1]["startdate"].ToString());
                }
                else
                {
                    //TODO: If there are not start dates for both RAs and the general population what should we do? Attempt to automatically determine the values? Email housing admin or an administrator?
                }
            }
            catch (Exception ee)
            {
                this.ParentPortlet.ShowFeedback(FeedbackType.Error, String.Format("Start date exception: {0}<br /><br />{1}", ee.Message, ee.InnerException));
            }
            return raStartDate.HasValue && generalStartDate.HasValue;
        }

        private DataTable GetInvitations(string studentGender, bool mayRegister)
        {
            //Get any invitations associated with the user
            string invitationSQL = "EXECUTE [dbo].[CUS_spHousing_getInvitations] @guidStudentID = ?, @strGender = ?";
            Exception exInvitation = null;
            DataTable dtInvitation = null;
            List<OdbcParameter> invitationParams = new List<OdbcParameter>
            {
                  new OdbcParameter("StudentID", PortalUser.Current.Guid)
                , new OdbcParameter("StudentGender", studentGender)
            };
            try
            {
                dtInvitation = jicsConn.ConnectToERP(invitationSQL, ref exInvitation, invitationParams);
                if (exInvitation != null) { throw exInvitation; }
                //If invitations exist for the user, bind the data to the repeater
                if (dtInvitation != null && dtInvitation.Rows.Count > 0)
                {
                    if (mayRegister)
                    {
                        this.repeaterInvites.DataSource = dtInvitation;
                        this.repeaterInvites.DataBind();
                    }
                    else
                    {
                        this.repeaterInvitesReadOnly.DataSource = dtInvitation;
                        this.repeaterInvitesReadOnly.DataBind();
                    }
                    this.repeaterInvites.Visible = mayRegister;
                    this.repeaterInvitesReadOnly.Visible = !mayRegister;
                }
                else
                {
                    //If there are no invitations for the user, hide the entire invitation panel
                    this.panelInvitations.Visible = false;
                }
            }
            catch (Exception ee)
            {
                this.ParentPortlet.ShowFeedback(FeedbackType.Error, String.Format("Error retrieving invitations:<br />{0}<br /><br />{1}", ee.Message, ee.InnerException));
            }
            return dtInvitation;
        }

        /// <summary>
        /// Retrieve all the invitations extended by the student after they completed the signup process for their room and loads the data into the repeater
        /// </summary>
        /// <returns>DataTable dtExtendedInvite - contains Building Name, Room Number, RoomSessionID, Student ID (guid), First Name, and Last Name</returns>
        private DataTable GetExtendedInvitations()
        {
            string extendedInviteSQL = "EXECUTE [dbo].[CUS_spHousing_getExtendedInvitations] @guidStudentID = ?";
            Exception exExtendedInvite = null;
            DataTable dtExtendedInvite = null;
            List<OdbcParameter> extendedInviteParams = new List<OdbcParameter>
            {
                new OdbcParameter("StudentID", PortalUser.Current.Guid)
            };

            try
            {
                dtExtendedInvite = jicsConn.ConnectToERP(extendedInviteSQL, ref exExtendedInvite, extendedInviteParams);
                if (exExtendedInvite != null) { throw exExtendedInvite; }
                this.repeaterExtendedInvites.DataSource = dtExtendedInvite;
                this.repeaterExtendedInvites.DataBind();

                this.repeaterExtendedInvites2.DataSource = dtExtendedInvite;
                this.repeaterExtendedInvites2.DataBind();

                if (dtExtendedInvite == null || dtExtendedInvite.Rows.Count == 0)
                {
                    this.panelExtendedInvitations.Visible = this.panelExtendedInvitations2.Visible = false;
                }
                else
                {
                    this.panelExtendedInvitations.Visible = this.panelExtendedInvitations2.Visible = true;
                }
            }
            catch (Exception ex)
            {
                this.ParentPortlet.ShowFeedback(FeedbackType.Message, String.Format("{0}<br /><br />{1}", ex.Message, ex.InnerException));
            }
            return dtExtendedInvite;
        }

        /// <summary>
        /// Retrieves the student's room assignment for the upcoming session. This is only available after a student has completed the housing sign-up process.
        /// </summary>
        /// <returns></returns>
        private DataRow GetUpcomingHousing()
        {
            //Check if the student has already signed up for a room in the current housing period
            string upcomingHousingSQL = "EXECUTE [dbo].[CUS_spHousing_isStudentInRoomByID] @guidStudentID = ?";

            //Initialize variables to execute query
            Exception exUpcomingHousing = null;
            DataTable dtUpcomingHousing = null;
            DataRow drUpcomingHousing = null;
            List<OdbcParameter> upcomingHousingParams = new List<OdbcParameter>
            {
                new OdbcParameter("StudentID", PortalUser.Current.Guid)
            };

            try
            {
                dtUpcomingHousing = jicsConn.ConnectToERP(upcomingHousingSQL, ref exUpcomingHousing, upcomingHousingParams);
                if (exUpcomingHousing != null) { throw exUpcomingHousing; }
                //If the student has signed up for a room, load a DataRow object with the information about their registration (Building, Room, Registration Time)
                if (dtUpcomingHousing != null && dtUpcomingHousing.Rows.Count > 0)
                {
                    drUpcomingHousing = dtUpcomingHousing.Rows[0];

                    //Populate the controls with the corresponding information
                    this.ltlRegisteredHousing.Text = String.Format("{0}: {1}", drUpcomingHousing["BuildingName"].ToString(), drUpcomingHousing["RoomNumber"].ToString());
                    this.ltlRegisteredDateTime.Text = String.Format("{0:hh:mm:ss tt on dddd MMMM d, yyyy}", drUpcomingHousing["RegistrationTime"].ToString());
                }
            }
            catch (Exception ee)
            {
                this.ParentPortlet.ShowFeedback(FeedbackType.Error, String.Format("{0}<br /><br />{1}", ee.Message, ee.InnerException));
            }
            return drUpcomingHousing;
        }

        /// <summary>
        /// Determine the date and time at which the student will be permitted to access the general room selection screens.
        /// This is calculated based on the application's start date and the number of credit hours earned by the student.
        /// </summary>
        /// <param name="generalStartDate">The date that the general (non-greek, non-squatter) signup period begins</param>
        /// <param name="careerCreditHours">The total number of credits earned by the student</param>
        /// <returns></returns>
        private DateTime GetCreditHourStartTime(DateTime generalStartDate, int careerCreditHours)
        {
            int daysToAdd = 3, startHour = 11;
            if (careerCreditHours >= 61)
            {
                daysToAdd = 1;
                if (careerCreditHours >= 126) { startHour = 8; }
                else if (careerCreditHours >= 118) { startHour = 9; }
                else if (careerCreditHours >= 106) { startHour = 10; }
                else if (careerCreditHours >= 98) { startHour = 11; }
                else if (careerCreditHours >= 92) { startHour = 12; }
                else if (careerCreditHours >= 85) { startHour = 13; }
                else if (careerCreditHours >= 74) { startHour = 14; }
                else if (careerCreditHours >= 66) { startHour = 15; }
                else if (careerCreditHours >= 61) { startHour = 16; }
            }
            else if (careerCreditHours >= 21)
            {
                daysToAdd = 2;
                if (careerCreditHours >= 57) { startHour = 8; }
                else if (careerCreditHours >= 53) { startHour = 9; }
                else if (careerCreditHours >= 45) { startHour = 10; }
                else if (careerCreditHours >= 36) { startHour = 11; }
                else if (careerCreditHours >= 32) { startHour = 12; }
                else if (careerCreditHours >= 28) { startHour = 13; }
                else if (careerCreditHours >= 24) { startHour = 14; }
                else if (careerCreditHours >= 21) { startHour = 15; }
            }
            else
            {
                //Days to add is defaulted to 3 so no additional changes need to be specified
                if (careerCreditHours >= 20) { startHour = 8; }
                else if (careerCreditHours >= 17) { startHour = 9; }
                else if (careerCreditHours >= 12) { startHour = 10; }
                //0 credit hours begin at 11 so no additional changes need to be specified
            }

            return generalStartDate.AddDays(daysToAdd).AddHours(startHour);
        }

        /// <summary>
        /// Determines whether the student is permitted to register at the current time. Examines day, current time, career credit hours and, if the student may not register, passes back a message explaining why
        /// </summary>
        /// <param name="dayIndex">The (0-based) index of today based on the greek/squatter start date</param>
        /// <param name="careerCreditHours">The total completed credit hours over the student's entire career</param>
        /// <param name="message">A user-readable explanation why they are not permitted to sign-up</param>
        /// <returns></returns>
        private bool IsValidTimeToRegister(int dayIndex, int careerCreditHours, string greekName, out string message)
        {
            string ExpiredMessage = String.Format("The housing sign-up period has expired. If you have any questions or problems, contact {0} at <a href='mailto:{1}'>{1}</a>, or call the Dean of Students office at 551-5800.", helper.HOUSING_ADMIN_NAME, helper.HOUSING_ADMIN_EMAIL);
            message = "";
            bool isValidTime = false;
            if (dayIndex < 0)
            {
                message = "The housing sign-up period has not started yet. Please check the housing <a href='https://www.carthage.edu/housing/selection/overview.cfm'>overview page</a> to verify start times.";
                return false;
            }
            else
            {
                //Get the current time
                int currentHour = DateTime.Now.Hour;
                switch (dayIndex)
                {
                    case 0:
                        //TODO: Add additional constraint to ensure user is a member of a Greek organization. The blanket "true" status only applied when the first day included squatters.
                        //isValidTime = true;
                        isValidTime = !String.IsNullOrWhiteSpace(greekName);
                        break;
                    case 1:
                        //Students meeting the credit hours for the day may begin signing up at 8 a.m.
                        if (currentHour >= 8 && careerCreditHours >= 126) { isValidTime = true; }
                        else if (currentHour >= 9 && careerCreditHours >= 118) { isValidTime = true; }
                        else if (currentHour >= 10 && careerCreditHours >= 106) { isValidTime = true; }
                        else if (currentHour >= 11 && careerCreditHours >= 98) { isValidTime = true; }
                        else if (currentHour >= 12 && careerCreditHours >= 92) { isValidTime = true; }
                        else if (currentHour >= 13 && careerCreditHours >= 85) { isValidTime = true; }
                        else if (currentHour >= 14 && careerCreditHours >= 74) { isValidTime = true; }
                        else if (currentHour >= 15 && careerCreditHours >= 66) { isValidTime = true; }
                        else if (currentHour >= 16 && careerCreditHours >= 61) { isValidTime = true; }
                        break;
                    case 2:
                        //Anyone from the previous day may sign up at any time on this day
                        if (careerCreditHours >= 61) { isValidTime = true; }
                        //Students meeting the credit hours for the day may begin signing up at 8 a.m.
                        else if (currentHour >= 8 && careerCreditHours >= 57) { isValidTime = true; }
                        else if (currentHour >= 9 && careerCreditHours >= 53) { isValidTime = true; }
                        else if (currentHour >= 10 && careerCreditHours >= 45) { isValidTime = true; }
                        else if (currentHour >= 11 && careerCreditHours >= 36) { isValidTime = true; }
                        else if (currentHour >= 12 && careerCreditHours >= 32) { isValidTime = true; }
                        else if (currentHour >= 13 && careerCreditHours >= 28) { isValidTime = true; }
                        else if (currentHour >= 14 && careerCreditHours >= 24) { isValidTime = true; }
                        else if (currentHour >= 15 && careerCreditHours >= 21) { isValidTime = true; }
                        break;
                    case 3:
                        //The housing signup period ends at 2 p.m. but we build in a one hour grace period
                        if (currentHour >= CONFIG_END_HOUR) { message = ExpiredMessage; isValidTime = false; }
                        //Anyone from the previous day may sign up at any time before the close of housing on this day
                        else if (currentHour >= 0 && careerCreditHours >= 21) { isValidTime = true; }
                        //Students meeting the credit hours for the day may begin signing up at 8 a.m.
                        else if (currentHour >= 8 && careerCreditHours >= 20) { isValidTime = true; }
                        else if (currentHour >= 9 && careerCreditHours >= 17) { isValidTime = true; }
                        else if (currentHour >= 10 && careerCreditHours >= 12) { isValidTime = true; }
                        else if (currentHour >= 11 && careerCreditHours >= 0) { isValidTime = true; }
                        break;
                    default:
                        message = ExpiredMessage;
                        isValidTime = false;
                        break;
                }
            }
            return isValidTime;
        }

        /// <summary>
        /// Implements the functionality for every invitation extended to the student
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void repeaterInvites_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            //The following code is only executed when the row being bound is an Item or AlternatingItem
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                //Cast the row as a DataRow object so information can be extracted
                DataRow invitation = (e.Item.DataItem as DataRowView).Row;

                //Find the Button control
                Button btnInvite = e.Item.FindControl("btnRoomInvite") as Button;

                //Is the student permitted to register? This represents a combination of if it is a valid time (based on day and credit hours) and the absence of any holds (billing or registered hours)
                bool okToRegister = this.ParentPortlet.PortletViewState["maySignUp"] != null ? bool.Parse(this.ParentPortlet.PortletViewState["maySignUp"].ToString()) : false;

                //If the student is permitted to register, make their invitation a button
                if (okToRegister)
                {
                    //Update the properties of the button control
                    btnInvite.Text = String.Format("{0} {1}", invitation["BuildingName"].ToString(), invitation["RoomNumber"].ToString());
                    btnInvite.CommandArgument = invitation["RoomSessionID"].ToString();
                    //Attach a definition to the click event of the button
                    btnInvite.Click += btnInvite_Click;
                }
                //If the student is not permitted to register yet, display the building and room of their invitation
                else
                {
                    btnInvite.Visible = false;
                    Literal ltlInvite = e.Item.FindControl("ltlInvite") as Literal;
                    ltlInvite.Text = String.Format("{0} {1}", invitation["BuildingName"].ToString(), invitation["RoomNumber"].ToString());
                }

                //Find the Literal control
                Literal ltlInviter = e.Item.FindControl("ltlInviteBy") as Literal;
                
                //Update the text of the Literal control
                ltlInviter.Text = invitation["InvitedBy"].ToString();
            }
        }

        /// <summary>
        /// When a button from the invitation repeater is clicked, store the RoomSessionID in the viewstate and skip the user straight to the "Accept Room" screen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnInvite_Click(object sender, EventArgs e)
        {
            //Get the button object that was clicked
            Button clicked = sender as Button;
            
            //Save the RoomSessionID in the viewstate to be used on subsequent screens
            this.ParentPortlet.PortletViewState["RoomSessionID"] = clicked.CommandArgument;
            
            //Skip the building and room screens and go straight to the screen where the user may accept the room
            this.ParentPortlet.NextScreen("AcceptRoom");
        }

        protected void repeaterExtendedInvites_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                //As the row is bound to the item, cast the item as a DataRow object so the values may be accessed
                DataRow invitation = (e.Item.DataItem as DataRowView).Row;

                //Get the control to store the name of the invitee
                Literal ltlInviteName = e.Item.FindControl("ltlInvitedName") as Literal;

                //Load the control with the name of the invitee
                ltlInviteName.Text = String.Format("{0} {1}", invitation["FirstName"].ToString(), invitation["LastName"].ToString());
            }
        }

        /// <summary>
        /// Calculates the difference in days between two dates. A negative value indicates that compareDate comes before variable date.
        /// A positive value indicates that compareDate comes after variable date. If the method returns 0, the dates are the same.
        /// </summary>
        /// <param name="variableDate">The date to drive the comparison</param>
        /// <param name="compareDate">Optional argument, defaults to DateTime.Now.Date</param>
        /// <returns></returns>
        private int DateDiff(DateTime variableDate, DateTime? compareDate = null)
        {
            //If compareDate is null, set it to use DateTime.Now.Date
            compareDate = compareDate ?? DateTime.Now.Date;

            //Calculate the difference, in days, between the two dates
            return int.Parse((compareDate.Value - variableDate).TotalDays.ToString());
        }

        /// <summary>
        /// Based on the student's current building assignment in CX, determine whether they are a commuter
        /// </summary>
        /// <param name="cxData">DataRow from the CX resultset</param>
        /// <returns></returns>
        private bool studentIsCommuter(DataRow cxData)
        {
            //Initialize student's commuter status
            bool returnVal = false;
            if (cxData != null)
            {
                string cxBldg = cxData["bldg"].ToString().Trim().ToUpper();
                return cxBldg == "CMTR" || cxBldg == "COMMUTER";
            }
            return returnVal;
        }

        private bool studentIsRA()
        {
            int[] arrayRA = {
                                //2019 RA List
                                1488830,1463347,1494233,1482856,1492063,1474291,1481102,1467645,1485547,1474140,
                                1496120,1493724,1513707,1533265,1479143,1476038,1496108,1489791,1500739,1484413,
                                1474951,1519395,1438088,1507525,1463926,1499755,1478288,1509159,1475502,1480905,
                                1479431,1501837,1470914,1511820,1501121,1489377,1342846,1481215,1470360,1370385,
                                1480225,1490256,1459495,1465187,1482808,1493323,1511835,1466121,1476715,1487771,
                                1472245,1480123,1491440,1438223,1435049

                                //2018 RA List
                                //1465187,1480905,1476038,1479424,1464587,1412101,1459495,1498090,1438088,1427013,
                                //1463974,1489791,1485547,1488830,1436401,1438223,1415363,1399018,1383296,1499263,
                                //1474951,1423374,1496120,1472245,1435049,1474291,1405587,1493945,1463926,1470360,
                                //1438508,1463087,1463347,1478017,1427991,1496108,1480123,1412177,891143,1389046,
                                //1428007,1466121,1469657,1487709,1519395,1427269,1435756,1342846,1467645,1411876,
                                //1479431,1425977,1480225,1398715,1361143,1419945

                                //2017 RA List
                                //1407002,1470448,1425977,1428007,1425939,1456418,1464587,1463974,1361143,1342846,
                                //1470421,1421227,1416008,1425317,1478017,1395196,1411876,1463926,1399018,1361489,
                                //1438223,1427013,1389046,1405587,1412101,1385027,1474291,1469657,1396941,1427991,
                                //1423374,1409414,1476038,1418195,1389751,1336700,1424855,1429353,1430793,1463313,
                                //1427269,1380208,1487709,1436182,1448268,1383296,1359246,
                                //1398715,1335444,1412177,1415363,1363736,1403984

                                //2016 RA List
                                //1381978,1421978,1339127,1405988,1383296,1396160,1389751,1338341,1425939,1392260,
                                //1425650,1385927,1383799,1409414,1392996,1427269,1362600,1427013,1427991,1389046,
                                //1409586,1385027,1427196,1389396,1415363,1325094,1361489,1403984,1384091,1387527,
                                //1423490,1380442,1363736,1404734,1425627,1305016,1380321,1416008,1412101,1396941,
                                //1428007,1398715,1418195,1400663,1335444,1397143,1412177,1320805,1369894,1253677,
                                //1328822,1390994,1358662,
                                //RA roommates sign up the same day as the RAs so we include their IDs as well
                                //1425977,1416751,1359246,1423115,1356365,1418198,1345259,1414334

                                //2015 RA List
                                //1313163,1245108,1362954,1382806,1334627,1381978,1253677,1392260,1360349,1396160,
                                //1326209,853586,1348991,1335444,1392091,1338427,1377417,1389396,1349912,1316931,
                                //1359544,1393681,1356926,1390994,1339127,1333211,1380321,1372750,1338766,1380442,
                                //1302343,1321415,1385261,1328382,1325094,1397143,1320805,1348908,1353112,1366791,
                                //1369894,1255003,1363112,798485,1352443,1388740,1343203,1299902,1367698,1366533,
                                //1359411,1358662,833023,1379393,1384058,1314645,1378875,1315014,1305909,1328822,
                                //1207964,1286565,1292888,1366157
                            };
            return arrayRA.Contains(UserID);
        }
    }
}