using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Linq;
using System.Web;
using System.Web.Mail;
using System.Web.UI;
using System.Web.UI.WebControls;
using ConfigSettings = Jenzabar.Common.Configuration.ConfigSettings;
using Jenzabar.Common;                          //ObjectFactoryWrapper
using Jenzabar.Portal.Framework;
using Jenzabar.Portal.Framework.EmailLogging;   //EmailLogger
using Jenzabar.Portal.Framework.Facade;         //IPortalUserFacade
using Jenzabar.Common.Mail;                     //ValidEmail()
using Jenzabar.Portal.Framework.Web.UI;
using CUS.OdbcConnectionClass3;

namespace Housing
{
    public partial class Send_Invitations_View : PortletViewBase
    {
        OdbcConnectionClass3 cxConn = new OdbcConnectionClass3("ERPDataConnection.config");
        OdbcConnectionClass3 jicsConn = new OdbcConnectionClass3("JICSDataConnection.config");

        public override string ViewName { get { return "Roommate Invitations"; } }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (this.IsFirstLoad)
            {
                InitScreen();
            }
            LoadRoommates();
        }

        private void InitScreen()
        {
            string roomSQL = @"
                SELECT
                    Building.BuildingName, Building.BuildingCode, SUBSTRING(Room.RoomNumber, 1, 3) AS RoomNumberOnly, SUM(Room.Capacity) AS Capacity
                FROM
                    CUS_Housing_Room    Room    INNER JOIN  CUS_Housing_Building    Building    ON  Room.BuildingID =   Building.BuildingID
												,
												(
													SELECT
														BuildingCode, SUBSTRING(RoomNumber, 1, 3) AS RoomNumberOnly
													FROM
														CUS_Housing_RoomSession	HRS	INNER JOIN	CUS_Housing_Room		HRsub	ON	HRS.RoomID			=	HRsub.RoomID
																					INNER JOIN	CUS_Housing_Building	HBsub	ON	HRsub.BuildingID	=	HBsub.BuildingID
													WHERE
														HRS.RoomSessionID	=	?
												)									Sess
				WHERE
					SUBSTRING(Room.RoomNumber, 1, 3)	=	Sess.RoomNumberOnly
				AND
					Building.BuildingCode	=	Sess.BuildingCode
				GROUP BY
					Building.BuildingName, Building.BuildingCode, SUBSTRING(Room.RoomNumber, 1, 3)
				ORDER BY
					Building.BuildingName, RoomNumberOnly
            ";
            Exception exRoomDetail = null;
            DataTable dtRoomDetail = null;
            List<OdbcParameter> parameters = new List<OdbcParameter>
            {
                new OdbcParameter("roomSessionID", this.ParentPortlet.PortletViewState["RoomSessionID"].ToString())
            };

            try
            {
                dtRoomDetail = jicsConn.ConnectToERP(roomSQL, ref exRoomDetail, parameters);
                DataRow room = dtRoomDetail.Rows[0];
                this.ltlRoomSelected.Text = this.ltlRoom.Text = String.Format("{0} {1}", room["BuildingName"].ToString(), room["RoomNumberOnly"].ToString());
                this.ParentPortlet.PortletViewState["BuildingCode"] = room["BuildingCode"].ToString();
                this.ParentPortlet.PortletViewState["RoomNumber"] = room["RoomNumberOnly"].ToString();
                this.ParentPortlet.PortletViewState["RoomCapacity"] = room["Capacity"].ToString();
            }
            catch (Exception ex)
            {
                this.ParentPortlet.ShowFeedback(FeedbackType.Error, String.Format("{0}<br /><br />{1}", ex.InnerException, ex.Message));
            }
        }

        private void LoadRoommates()
        {
            this.panelStudentDetail.Visible = false;
            string roommateSQL = @"
                SELECT
	                HB.BuildingName, HR.RoomNumber, HR.Capacity, ISNULL(FU.FirstName,'') AS FirstName, ISNULL(FU.LastName,'') AS LastName, HRS.RoomSessionID, 'Occupant' AS OccupancyStatus
                FROM
	                CUS_Housing_RoomSession	HRS	INNER JOIN	CUS_Housing_Room			HR		ON	HRS.RoomID			=	HR.RoomID
								                INNER JOIN	CUS_Housing_Building		HB		ON	HR.BuildingID		=	HB.BuildingID
								                INNER JOIN	CUS_Housing_RoomStudent		HRStu	ON	HRS.RoomSessionID	=	HRStu.RoomSessionID
								                INNER JOIN	FWK_User					FU		ON	HRStu.StudentID		=	FU.ID
                WHERE
					HRS.HousingYear	=	YEAR(GETDATE())
				AND
	                HB.BuildingCode	=	?
				AND
					SUBSTRING(HR.RoomNumber, 1, 3)	=	?
				UNION
                SELECT
	                HB.BuildingName, HR.RoomNumber, HR.Capacity, ISNULL(FU.FirstName,'') AS FirstName, ISNULL(FU.LastName,'') AS LastName, HRS.RoomSessionID, 'Invited' AS OccupancyStatus
                FROM
	                CUS_Housing_RoomSession	HRS	INNER JOIN	CUS_Housing_Room			HR		ON	HRS.RoomID			=	HR.RoomID
								                INNER JOIN	CUS_Housing_Building		HB		ON	HR.BuildingID		=	HB.BuildingID
								                INNER JOIN	CUS_Housing_RoomReservation	HRR		ON	HRS.RoomSessionID	=	HRR.RoomSessionID
								                INNER JOIN	FWK_User					FU		ON	HRR.StudentID		=	FU.ID
                WHERE
					HRS.HousingYear	=	YEAR(GETDATE())
				AND
	                HB.BuildingCode	=	?
				AND
					SUBSTRING(HR.RoomNumber, 1, 3)	=	?
				ORDER BY
					RoomNumber, LastName
            ";
            Exception exRoommates = null;
            DataTable dtRoommates = null;
            List<OdbcParameter> param = new List<OdbcParameter>
            {
                //  new OdbcParameter("RoomSessionID1", this.ParentPortlet.PortletViewState["RoomSessionID"].ToString())
                //, new OdbcParameter("RoomSessionID2", this.ParentPortlet.PortletViewState["RoomSessionID"].ToString())
                  new OdbcParameter("BuildingCode1", this.ParentPortlet.PortletViewState["BuildingCode"].ToString())
                , new OdbcParameter("RoomNumber1", this.ParentPortlet.PortletViewState["RoomNumber"].ToString())
                , new OdbcParameter("BuildingCode2", this.ParentPortlet.PortletViewState["BuildingCode"].ToString())
                , new OdbcParameter("RoomNumber2", this.ParentPortlet.PortletViewState["RoomNumber"].ToString())
            };

            this.ParentPortlet.PortletViewState["IsAtCapacity"] = false;
            try
            {
                dtRoommates = jicsConn.ConnectToERP(roommateSQL, ref exRoommates, param);
                if (exRoommates != null) { throw exRoommates; }
                if (dtRoommates != null)
                {
                    int capacity = int.Parse(this.ParentPortlet.PortletViewState["RoomCapacity"].ToString());
                    bool isAtCapacity = dtRoommates.Rows.Count >= capacity;
                    this.ParentPortlet.PortletViewState["IsAtCapacity"] = isAtCapacity;
                    if (!isAtCapacity)
                    {
                        dtRoommates.Rows.Add(
                            this.ParentPortlet.PortletViewState["BuildingCode"].ToString(),
                            this.ParentPortlet.PortletViewState["RoomNumber"].ToString(),
                            this.ParentPortlet.PortletViewState["RoomCapacity"].ToString(),
                            "", "",
                            dtRoommates.Rows[0]["RoomSessionID"].ToString(),"Occupant"
                        );
                    }
                    this.repeaterRoommates.DataSource = dtRoommates;
                    this.repeaterRoommates.DataBind();
                }
            }
            catch (Exception ex)
            {
                this.ParentPortlet.ShowFeedback(FeedbackType.Error, String.Format("Error while binding repeater:<br />{0}<br /><br />{1}<br />{2}", ex.Message, ex.InnerException, ex.ToString()));
            }
            finally
            {
                if (jicsConn.IsNotClosed()) { jicsConn.Close(); }
            }
        }

        protected void repeaterRoommates_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.AlternatingItem || e.Item.ItemType == ListItemType.Item)
            {
                DataRow roommate = (e.Item.DataItem as DataRowView).Row;

                PlaceHolder phRow = e.Item.FindControl("phRoommate") as PlaceHolder;
                
                //If the bed is available, allow the user to invite a friend
                if (String.IsNullOrEmpty(roommate["FirstName"].ToString() + roommate["LastName"].ToString()))
                {
                    if (!bool.Parse(this.ParentPortlet.PortletViewState["IsAtCapacity"].ToString()))
                    {
                        TextBox tbEmail = new TextBox();
                        tbEmail.ID = "tbFindEmail";
                        phRow.Controls.Add(tbEmail);

                        Literal ltlEmailSuffix = new Literal();
                        ltlEmailSuffix.Text = "@carthage.edu";
                        phRow.Controls.Add(ltlEmailSuffix);

                        Button btnCheckEmail = new Button();
                        btnCheckEmail.Text = "Check Email";
                        btnCheckEmail.UseSubmitBehavior = false;
                        btnCheckEmail.CommandArgument = roommate["RoomSessionID"].ToString();
                        btnCheckEmail.Click += btnCheckEmail_Click;
                        phRow.Controls.Add(btnCheckEmail);
                    }
                }
                else
                {
                    Literal ltlRoommate = new Literal();
                    ltlRoommate.Text = String.Format("{0}: {1} {2}", roommate["OccupancyStatus"].ToString(), roommate["FirstName"].ToString(), roommate["LastName"].ToString());
                    phRow.Controls.Add(ltlRoommate);
                }
            }
        }

        private void btnCheckEmail_Click(object sender, EventArgs e)
        {
            //Flag indicating whether the invitee has satisfied all the criteria
            bool okToEmail = false;

            //If the invitee fails a condition, describe the reason to the user
            string reasonForFailure = "";

            //The "Check Email" button
            Button btn = (Button)sender;

            //The textbox containing the email value entered by the user
            TextBox tbEmail = btn.NamingContainer.FindControl("tbFindEmail") as TextBox;

            //Remove the domain from the email address (if entered) so the username can be checked
            string cleanEmail = tbEmail.Text + (tbEmail.Text.EndsWith("@carthage.edu") ? "" : "@carthage.edu");

            PortalUser invited = ObjectFactoryWrapper.GetInstance<IPortalUserFacade>().FindByEmail(cleanEmail);
            if (invited == null)
            {
                reasonForFailure = "There is no information about that student. Please double-check the email address you entered.";
            }
            else
            {
                //If the user put in their own email address...
                if (invited.Guid == PortalUser.Current.Guid)
                {
                    reasonForFailure = "You cannot reserve a bed for yourself, you've already signed up for a bed.";
                }
                else
                {
                    //Has the invitee already registered for a room?
                    string inRoomSQL = String.Format(@"
                        SELECT
                            StudentID
                        FROM
                            CUS_Housing_RoomStudent HRStu   INNER JOIN  CUS_Housing_RoomSession HRS ON  HRStu.RoomSessionID =   HRS.RoomSessionID
                                                                                                    AND HRS.HousingYear     =   YEAR(GETDATE())
                                                            INNER JOIN  FWK_User                FU  ON  HRStu.StudentID     =   FU.ID
                        WHERE
                            FU.HostID =   {0}
                        ", invited.HostID);
                    Exception exInRoom = null;
                    DataTable dtInRoom = null;

                    try
                    {
                        dtInRoom = jicsConn.ConnectToERP(inRoomSQL, ref exInRoom);
                        if (exInRoom != null) { throw exInRoom; }

                        //If the invited user has already registered for a room...
                        if (dtInRoom != null && dtInRoom.Rows.Count > 0)
                        {
                            reasonForFailure = String.Format("{0} {1} has already registered for a room.", invited.FirstName, invited.LastName);
                        }
                        else
                        {
                            //Query to determine the gender of the invited user
                            string genderSQL = String.Format("SELECT sex FROM profile_rec WHERE profile_rec.id = {0}", invited.HostID);
                            Exception exGender = null;
                            DataTable dtGender = null;
                            //Flag to track whether the inviting and invited students are of the same gender
                            bool genderMatch = false;
                            try
                            {
                                dtGender = cxConn.ConnectToERP(genderSQL, ref exGender);
                                if (exGender != null) { throw exGender; }
                                if (dtGender != null && dtGender.Rows.Count > 0)
                                {
                                    genderMatch = this.ParentPortlet.PortletViewState["Gender"].ToString().ToUpper() == dtGender.Rows[0]["sex"].ToString().ToUpper();
                                    if (!genderMatch)
                                    {
                                        reasonForFailure = "Your roommate must be the same gender as yourself.";
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                this.ParentPortlet.ShowFeedback(FeedbackType.Error, String.Format("{0}<br />{1}", ex.Message, ex.InnerException));
                            }

                            //If all other criteria have been satisfied, set the okToEmail flag to the last criteria (all others must have been TRUE to get to this point)
                            okToEmail = genderMatch;
                        }
                    }
                    catch (Exception ex)
                    {
                        this.ParentPortlet.ShowFeedback(FeedbackType.Error, String.Format("{0}<br /><br />{1}", ex.Message, ex.InnerException));
                    }
                }
            }

            //Set the visibility of the page elements based on the determination of the above criteria
            this.panelStudentDetail.Visible = true;
            this.panelStudentOK.Visible = okToEmail;
            this.panelStudentNo.Visible = !okToEmail;
            if (okToEmail)
            {
                this.ltlStudentName.Text = invited.ToFirstLastNameDisplay().ToString();
                this.ltlStudentEmail.Text = invited.EmailAddress;
                this.btnSendInvitation.CommandArgument = invited.HostID;
                this.ParentPortlet.PortletViewState["InvitedRoomSessionID"] = btn.CommandArgument;
            }
            else
            {
                this.ltlStudentEmail2.Text = cleanEmail;
                this.ltlReason.Text = reasonForFailure;
                this.btnSendInvitation.CommandArgument = "0";
                this.ParentPortlet.PortletViewState["InvitedRoomSessionID"] = null;
            }
        }

        protected void btnSendInvitation_Click(object sender, EventArgs e)
        {
            Button btnInvite = sender as Button;

            PortalUser invitee = ObjectFactoryWrapper.GetInstance<IPortalUserFacade>().FindByHostID(btnInvite.CommandArgument);

            string invitationSQL = String.Format(@"
                INSERT INTO CUS_Housing_RoomReservation (RoomSessionID, StudentID, ReservationTime, CreatedByID)
                VALUES('{0}', '{1}', GETDATE(), '{2}')
            ", this.ParentPortlet.PortletViewState["InvitedRoomSessionID"].ToString(), invitee.Guid.ToString(), PortalUser.Current.Guid.ToString());
            Exception exInvite = null;
            List<OdbcParameter> paramInvite = new List<OdbcParameter>
            {
                  new OdbcParameter("RoomSessionID", this.ParentPortlet.PortletViewState["InvitedRoomSessionID"].ToString())
                , new OdbcParameter("InviteeID", invitee.Guid.ToString())
                , new OdbcParameter("InviterID", PortalUser.Current.Guid.ToString())
            };

            try
            {
                jicsConn.ConnectToERP(invitationSQL, ref exInvite, paramInvite);
                if (exInvite != null) { throw exInvite; }
            }
            catch (Exception ex)
            {
                this.ParentPortlet.ShowFeedback(FeedbackType.Error, String.Format("Could not insert invitation:<br />{0}<br />{1}<br />{2}", ex.Message, ex.InnerException, invitationSQL));
            }

            string emailSQL = String.Format(@"
                SELECT
	                FU.FirstName, HB.BuildingName + ' ' + HR.RoomNumber AS RoomLocation, CAST(YEAR(GETDATE()) AS CHAR(4)) + '-' + CAST(YEAR(GETDATE()) + 1 AS CHAR(4)) AS HousingYear,
                    CONVERT(VARCHAR(15), DATEADD(d,3,HSS.startdate), 107) AS EndDate, FU.Email
                FROM
	                FWK_User	FU, CUS_HousingSelectionStartdate	HSS,
					                CUS_Housing_RoomSession			HRS	INNER JOIN	CUS_Housing_Room		HR	ON	HRS.RoomID		=	HR.RoomID
														                INNER JOIN	CUS_Housing_Building	HB	ON	HR.BuildingID	=	HB.BuildingID
                WHERE
	                FU.HostID	=	{0}
	                AND
	                HSS.id		=	2
	                AND
	                HRS.RoomSessionID	=	?
            ", btnInvite.CommandArgument);

            Exception exEmail = null;
            DataTable dtEmail = null;
            List<OdbcParameter> param = new List<OdbcParameter> { new OdbcParameter("RoomSessionID", this.ParentPortlet.PortletViewState["RoomSessionID"].ToString()) };

            try
            {
                dtEmail = jicsConn.ConnectToERP(emailSQL, ref exEmail, param);
                if (exEmail != null) { throw exEmail; }
                if (dtEmail != null && dtEmail.Rows.Count > 0)
                {
                    DataRow drEmail = dtEmail.Rows[0];

                    string emailBody = String.Format(@"
                        <p>{0},</p>
				        <p>A roommate request has been created for you in {1}. When it is your credit hour time to register for a room, you will see a button on your profile page once
                        you've logged into the housing sign-up application to accept this roommate invitation.</p>
				        <p>You must log in and select a bed before 2pm on {2:MMMM d, yyyy} to secure housing. This roommate invitation does not secure your housing for the {3}
                        academic year. You must log in and either accept the invitation or sign up for a different room to secure housing for the {4} academic year.</p>
				        <p>Please contact Nina Fleming, Assistant Dean of Students at <a mailto='nfleming@carthage.edu'>nfleming@carthage.edu</a> or in the Office of
                        Student Life if you have any questions about this invitation.</p>
				        <p><a href='https://www.carthage.edu/housing/'>https://www.carthage.edu/housing/</a></p>
                    ", drEmail["FirstName"].ToString(), drEmail["RoomLocation"].ToString(), drEmail["EndDate"].ToString(), drEmail["HousingYear"].ToString(), drEmail["HousingYear"].ToString());

                    PortalUser recipient = ObjectFactoryWrapper.GetInstance<IPortalUserFacade>().FindByEmail(drEmail["Email"].ToString());

                    string emailTo = recipient.EmailAddress, emailSubject = "Housing Roommate Invitation";

                    string smtpAddress = ConfigSettings.Current.SmtpDefaultEmailAddress;
                    PortalUser emailSender = ObjectFactoryWrapper.GetInstance<IPortalUserFacade>().FindByEmail("nfleming@carthage.edu");
                    smtpAddress = Email.GetProperEMailAddress(emailSender.EmailAddress);

                    bool emailSuccess = !String.IsNullOrEmpty(emailTo) && (new ValidEmail(emailTo).IsValid) && Email.CreateAndSendMailMessage(smtpAddress, emailTo, emailSubject, emailBody);
                    if (ConfigSettings.Current.LogEmail && emailSuccess)
                    {
                        List<PortalUser> recipients = new List<PortalUser>();
                        recipients.Add(recipient);
                        new EmailLogger().Log(emailSender, recipients, null, null, null, emailSubject, emailBody, null, null);
                    }
                    LoadRoommates();
                }
            }
            catch (Exception ex)
            {
                this.ParentPortlet.ShowFeedback(FeedbackType.Error, String.Format("Error when emailing invitee<br />{0}<br /><br />{1}", ex.Message, ex.InnerException.ToString()));
            }
        }
    }
}