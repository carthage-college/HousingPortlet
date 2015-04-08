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

using System.Text;  //StringBuilder
using ConfigSettings = Jenzabar.Common.Configuration.ConfigSettings;
using Jenzabar.Common;  //ObjectFactoryWrapper
using Jenzabar.Portal.Framework.EmailLogging;   //EmailLogger
using Jenzabar.Portal.Framework.Facade; //IPortalUserFacade
using Jenzabar.Common.Mail; //ValidEmail()


namespace Housing
{
    public partial class Default_View : PortletViewBase
    {
        public override string ViewName { get { return "Housing Sign-up Application"; } }

        #region Public Members
        OdbcConnectionClass3 odbcConn = new OdbcConnectionClass3("ERPDataConnection.config");
        OdbcConnectionClass3 jicsConn = new OdbcConnectionClass3("JICSDataConnection.config");
        public string SESSION_SPRING { get { return "RC"; } }
        public string SESSION_FALL { get { return "RA"; } }
        public string CurrentYear { get { return DateTime.Now.Year.ToString(); } }
        public string NextYear { get { return (int.Parse(CurrentYear) + 1).ToString(); } }
        public string BeginInvolvement { get { return "1/1/" + (int.Parse(CurrentYear) - 3).ToString(); } }
        public int UserID { get { return int.Parse(PortalUser.Current.HostID); } }
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
            
            //If the student hasn't signed up for housing, retrieve and display the invitations extended to them
            if (!isRegisteredForHousing)
            {
                DataTable dtInvitations = GetInvitations(drCxData["sex"].ToString());
            }
            //If the student has signed up for housing, display their roommates
            else
            {
                this.panelInvitations.Visible = false;
                GetRoommates(drUpcomingHousing["RoomSessionID"].ToString());
            }
            GetExtendedInvitations();

            //If the query returned results, load the data into the controls on the page
            if (drCxData != null)
            {
                this.ltlStudentName.Text = String.Format("{0} {1}", drCxData["firstname"].ToString(), drCxData["lastname"].ToString());

                this.panelRegistered.Visible = isRegisteredForHousing;

                this.panelUnregistered.Visible = !isRegisteredForHousing;

                this.ltlGreekStatus.Text = drCxData["greek_name"].ToString().Length > 0 ? String.Format("a member of {0}", drCxData["greek_name"].ToString()) : "not a member of a residential fraternity or sorority";
                this.ParentPortlet.PortletViewState["GreekID"] = drCxData["greekid"].ToString();

                bool hasHold = (drCxData["advPayHold"].ToString() + drCxData["unbal_hold"].ToString()).Length > 0;
                this.ltlHold.Text = hasHold ? "not" : "";
                this.contentHoldDetail.Visible = hasHold;

                bool isFallRegistered = int.Parse(drCxData["registered_hours"].ToString()) >= 12;
                //A student must be registered for 12 or more credits in the upcoming semester
                this.ltlRegistered.Text = isFallRegistered ? "" : "not";
                
                //this.contentRegisteredDetail.Visible = this.ltlRegistered.Text.Length > 0;
                
                this.ltlRegisteredYear.Text = String.Format("{0} - {1}", CurrentYear, NextYear);
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
                bool isCommuter = drCxData["bldg"].ToString().Trim() == "Commuter" || drCxData["bldg"].ToString().Trim().ToUpper() == "CMTR";
                this.ltlCurrentHousing.Text = String.Format("{0} {1}", drCxData["bldg"].ToString(), drCxData["room"].ToString());
                this.panelCommuter.Visible = this.ltlNotResident.Visible = isCommuter;
                this.panelCurrentHousing.Visible = !isCommuter;

                //Get the start dates for RAs and the general student population
                DateTime? raStartDate = null, generalStartDate = null;
                bool hasDates = GetStartDates(out raStartDate, out generalStartDate);

                DayIndex = DateDiff(generalStartDate.Value);
                IsTodayRA = DateDiff(raStartDate.Value) == 0;

                string message = "";
                bool isValidTime = IsValidTimeToRegister(DayIndex, int.Parse(drCxData["career_hours"].ToString()), out message);

                bool mayRegister = true;
                bool hasDefinedGender = this.ParentPortlet.PortletViewState["Gender"].ToString() != "";
                //bool mayRegister = isFallRegistered && !isCommuter && !hasHold && hasDefinedGender;

                this.panelAvailability.Visible = !isRegisteredForHousing;
                this.lnkAvailability.Visible = isValidTime && mayRegister;
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
            }
            else if (dtStudentData != null && dtStudentData.Rows.Count == 0)
            {
                this.ParentPortlet.ShowFeedback(FeedbackType.Error, "No records were found that matched your ID.");
            }
            else
            {
                this.ParentPortlet.ShowFeedback(FeedbackType.Error, "We were unable to retrieve your information. Please contact Nina Fleming <a href='mailto:nfleming@carthage.edu'>nfleming@carthage.edu</a> to resolve this issue.");
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
            string roommatesSQL = @"
                SELECT
                    FU.FirstName, FU.LastName
                FROM
                    CUS_Housing_RoomStudent HRStu   INNER JOIN  CUS_Housing_RoomSession HRS ON  HRStu.RoomSessionID =   HRS.RoomSessionID
                                                    INNER JOIN  FWK_User                FU  ON  HRStu.StudentID     =   FU.UserID
                WHERE
                    HRStu.RoomSessionID =   ?
                ORDER BY
                    HRStu.RegistrationTime
            ";
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
                        id_rec.id, TRIM(id_rec.firstname) AS firstname, TRIM(id_rec.lastname) AS lastname, profile_rec.sex, NVL(stu_stat_rec.cum_earn_hrs, 0) AS career_hours, TRIM(NVL(curbldg.txt,'')) AS bldg,
                        TRIM(NVL(stu_serv_rec.room,'')) AS room, NVL(stu_acad_rec.reg_hrs, 0) AS registered_hours, NVL(ADVpay.hld,'') AS advpayhold, NVL(UNBal.hld,'') AS unbal_hold,
                        NVL(invl_rec.invl,'') AS greekid, TRIM(NVL(invl_rec.org,'')) AS greek_name
                    FROM id_rec	INNER JOIN	profile_rec			        ON	id_rec.id			=	profile_rec.id
                                LEFT JOIN	stu_stat_rec		        ON	id_rec.id			=	stu_stat_rec.id
                                LEFT JOIN	stu_serv_rec		        ON	id_rec.id			=	stu_serv_rec.id
											                            AND	stu_serv_rec.sess	=	?
											                            AND	stu_serv_rec.yr		=	{0}
                                LEFT JOIN   bldg_table      curbldg     ON  stu_serv_rec.bldg   =   curbldg.bldg
			                    LEFT JOIN	stu_acad_rec		        ON	id_rec.id			=	stu_acad_rec.id
											                            AND	stu_acad_rec.sess	=	?
											                            AND	stu_acad_rec.yr		=	stu_serv_rec.yr
			                    LEFT JOIN	hold_rec	    ADVpay	    ON	id_rec.id			=	ADVpay.id
											                            AND	TODAY			BETWEEN	ADVpay.beg_date	AND	NVL(ADVpay.end_date, TODAY)
											                            AND	ADVpay.hld			=	'APAY'
			                    LEFT JOIN	hold_rec	    UNBal       ON	id_rec.id			=	UNBal.id
											                            AND	TODAY			BETWEEN	UNBal.beg_date	AND	NVL(UNBal.end_date, TODAY)
											                            AND	UNBal.hld			=	'UBAL'
                                LEFT JOIN   involve_rec     invl_rec    ON  id_rec.id           =   invl_rec.id
                                                                        AND NVL(invl_rec.invl,'')    IN  ('','S007','S025','S045','S061','S063','S092','S141','S152','S165','S168','S189','S190','S192','S194')
                                                                        AND NVL(invl_rec.end_date, TODAY)   >=  TODAY
                    WHERE id_rec.id	=	{1}
                ", CurrentYear, UserID.ToString());

            List<OdbcParameter> parameters = new List<OdbcParameter>
            {
                  new OdbcParameter("SpringSess", SESSION_SPRING)
                , new OdbcParameter("FallSess", SESSION_FALL)
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
            string getStartDateSQL = @"
                SELECT
                    startdate, 1 AS isRA
                FROM
                    CUS_HousingSelectionStartdate
                WHERE
                    id  =   1
                UNION
                SELECT
                    startdate, 0 AS isRA
                FROM
                    CUS_HousingSelectionStartdate
                WHERE
                    id  =   2
                ORDER BY
                    isRA DESC
            ";
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
                    //TODO: If there are not start dates for both RAs and the general population what should we do? Attempt to automatically determine the values? Email Nina or an administrator?
                }
            }
            catch (Exception ee)
            {
                this.ParentPortlet.ShowFeedback(FeedbackType.Error, String.Format("Start date exception: {0}<br /><br />{1}", ee.Message, ee.InnerException));
            }
            return raStartDate.HasValue && generalStartDate.HasValue;
        }

        private DataTable GetInvitations(string studentGender)
        {
            //Get any invitations associated with the user
            string invitationSQL = @"
                SELECT
	                HB.BuildingName, HR.RoomNumber, HRR.RoomSessionID,
	                CASE
		                WHEN	FU.ID	IS	NULL	THEN	''
									                ELSE	FU.FirstName + ' ' + FU.LastName
	                END	AS	InvitedBy
                FROM
	                CUS_Housing_RoomReservation	HRR	INNER JOIN	CUS_Housing_RoomSession	HRS	ON	HRR.RoomSessionID	=	HRS.RoomSessionID
																			                AND	HRS.HousingYear		=	YEAR(GETDATE())
									                INNER JOIN	CUS_Housing_Room		HR	ON	HRS.RoomID			=	HR.RoomID
									                INNER JOIN	CUS_Housing_Building	HB	ON	HR.BuildingID		=	HB.BuildingID
									                LEFT JOIN	FWK_User				FU	ON	HRR.CreatedByID		=	FU.ID
                WHERE
	                HRR.StudentID	=	?
                    AND
                    HRS.Gender      IN  ('', ?)
                ORDER BY
	                HB.BuildingID, HR.RoomNumber
            ";
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
                    this.repeaterInvites.DataSource = dtInvitation;
                    this.repeaterInvites.DataBind();
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
            string extendedInviteSQL = @"
                SELECT
                    HB.BuildingName, HR.RoomNumber, HRR.RoomSessionID, HRR.StudentID, FU.FirstName, FU.LastName
                FROM
                    CUS_Housing_RoomReservation HRR INNER JOIN  FWK_User                FU  ON  HRR.StudentID       =   FU.ID
                                                    INNER JOIN  CUS_Housing_RoomSession HRS ON  HRR.RoomSessionID   =   HRS.RoomSessionID
                                                                                            AND HRS.HousingYear     =   YEAR(GETDATE())
                                                    INNER JOIN  CUS_Housing_Room        HR  ON  HRS.RoomID          =   HR.RoomID
                                                    INNER JOIN  CUS_Housing_Building    HB  ON  HR.BuildingID       =   HB.BuildingID
                WHERE
                    HRR.CreatedByID =   ?
            ";
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
            }
            catch (Exception ex)
            {
                this.ParentPortlet.ShowFeedback(FeedbackType.Message, String.Format("{0}<br /><br />{1}", ex.Message, ex.InnerException));
            }
            return dtExtendedInvite;
        }

        private DataRow GetUpcomingHousing()
        {
            //Check if the student has already signed up for a room in the current housing period
            string upcomingHousingSQL = @"
                SELECT
                    HB.BuildingName, HR.RoomNumber, HRStu.RegistrationTime, HRStu.RoomSessionID
                FROM
                    CUS_Housing_Building    HB  INNER JOIN  CUS_Housing_Room        HR      ON  HB.BuildingID       =   HR.BuildingID
                                                INNER JOIN  CUS_Housing_RoomSession HRS     ON  HR.RoomID           =   HRS.RoomID
                                                                                            AND HRS.HousingYear     =   YEAR(GETDATE())
                                                LEFT JOIN   CUS_Housing_RoomStudent HRStu   ON  HRS.RoomSessionID   =   HRStu.RoomSessionID
                WHERE
                    HRStu.StudentID =   ?
            ";
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

        private bool IsValidTimeToRegister(int dayIndex, int careerCreditHours, out string message)
        {
            string ExpiredMessage = "The housing sign-up period has expired. If you have any questions or problems, contact Nina Fleming at <a href='mailto:nfleming@carthage.edu'>nfleming@carthage.edu</a>, or call the Dean of Students office at 551-5800.";
            message = "";
            bool isValidTime = false;
            if (dayIndex < 0)
            {
                message = "The housing sign-up period has not started yet. Please check the housing <a href='https://www.carthage.edu/housing/selection/overview.cfm'>overview page</a> to verify start times.";
                return false;
            }
            else
            {
                int currentHour = DateTime.Now.Hour;
                switch (dayIndex)
                {
                    case 0:
                        isValidTime = true;
                        break;
                    case 1:
                        if (currentHour > 8 && careerCreditHours >= 126) { isValidTime = true; }
                        else if (currentHour > 9 && careerCreditHours >= 118) { isValidTime = true; }
                        else if (currentHour > 10 && careerCreditHours >= 106) { isValidTime = true; }
                        else if (currentHour > 11 && careerCreditHours >= 98) { isValidTime = true; }
                        else if (currentHour > 12 && careerCreditHours >= 92) { isValidTime = true; }
                        else if (currentHour > 13 && careerCreditHours >= 85) { isValidTime = true; }
                        else if (currentHour > 14 && careerCreditHours >= 74) { isValidTime = true; }
                        else if (currentHour > 15 && careerCreditHours >= 66) { isValidTime = true; }
                        else if (currentHour > 16 && careerCreditHours >= 61) { isValidTime = true; }
                        break;
                    case 2:
                        if (currentHour > 8 && careerCreditHours >= 57) { isValidTime = true; }
                        else if (currentHour > 9 && careerCreditHours >= 53) { isValidTime = true; }
                        else if (currentHour > 10 && careerCreditHours >= 45) { isValidTime = true; }
                        else if (currentHour > 11 && careerCreditHours >= 36) { isValidTime = true; }
                        else if (currentHour > 12 && careerCreditHours >= 32) { isValidTime = true; }
                        else if (currentHour > 13 && careerCreditHours >= 28) { isValidTime = true; }
                        else if (currentHour > 14 && careerCreditHours >= 24) { isValidTime = true; }
                        else if (currentHour > 15 && careerCreditHours >= 21) { isValidTime = true; }
                        break;
                    case 3:
                        if (currentHour > 15) { message = ExpiredMessage; isValidTime = false; }
                        else if (currentHour > 8 && careerCreditHours >= 20) { isValidTime = true; }
                        else if (currentHour > 9 && careerCreditHours >= 17) { isValidTime = true; }
                        else if (currentHour > 10 && careerCreditHours >= 12) { isValidTime = true; }
                        else if (currentHour > 11 && careerCreditHours >= 0) { isValidTime = true; }
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

                //Update the properties of the button control
                btnInvite.Text = String.Format("{0} {1}", invitation["BuildingName"].ToString(), invitation["RoomNumber"].ToString());
                btnInvite.CommandArgument = invitation["RoomSessionID"].ToString();
                //Attach a definition to the click event of the button
                btnInvite.Click += btnInvite_Click;

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
                DataRow invitation = (e.Item.DataItem as DataRowView).Row;

                Literal ltlInviteName = e.Item.FindControl("ltlInvitedName") as Literal;
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
    }
}