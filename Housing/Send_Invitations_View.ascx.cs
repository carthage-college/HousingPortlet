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
                    Building.BuildingName, Building.BuildingCode, Room.RoomNumber
                FROM
                    CUS_Housing_Room    Room    INNER JOIN  CUS_Housing_Building    Building    ON  Room.BuildingID =   Building.BuildingID
                                                INNER JOIN  CUS_Housing_RoomSession HRS         ON  Room.RoomID     =   HRS.RoomID
                                                                                                AND HRS.HousingYear =   YEAR(GETDATE())
                WHERE
                    HRS.RoomSessionID = ?";
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
                this.ltlRoomSelected.Text = String.Format("{0} {1}", room["BuildingName"].ToString(), room["RoomNumber"].ToString());
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
	                HB.BuildingName, HR.RoomNumber, HR.Capacity, ISNULL(FU.FirstName,'') AS FirstName, ISNULL(FU.LastName,'') AS LastName
                FROM
	                CUS_Housing_RoomSession	HRS	INNER JOIN	CUS_Housing_Room		HR		ON	HRS.RoomID			=	HR.RoomID
								                INNER JOIN	CUS_Housing_Building	HB		ON	HR.BuildingID		=	HB.BuildingID
								                LEFT JOIN	CUS_Housing_RoomStudent	HRStu	ON	HRS.RoomSessionID	=	HRStu.RoomSessionID
								                LEFT JOIN	FWK_User				FU		ON	HRStu.StudentID		=	FU.ID
                WHERE
	                HRS.RoomSessionID	=	?
            ";
            Exception exRoommates = null;
            DataTable dtRoommates = null;
            List<OdbcParameter> param = new List<OdbcParameter>
            {
                new OdbcParameter("RoomSessionID", this.ParentPortlet.PortletViewState["RoomSessionID"].ToString())
            };

            try
            {
                dtRoommates = jicsConn.ConnectToERP(roommateSQL, ref exRoommates, param);
                if (exRoommates != null) { throw exRoommates; }
                if (dtRoommates != null)
                {
                    this.repeaterRoommates.DataSource = dtRoommates;
                    this.repeaterRoommates.DataBind();
                    if (dtRoommates.Rows.Count > 0)
                    {
                        this.ltlRoom.Text = String.Format("{0} {1}", dtRoommates.Rows[0]["BuildingName"].ToString(), dtRoommates.Rows[0]["RoomNumber"].ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                this.ParentPortlet.ShowFeedback(FeedbackType.Error, String.Format("{0}<br /><br />{1}", ex.Message, ex.InnerException));
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
                //If the bed is available, allow the user to invite a friend
                if (String.IsNullOrEmpty(roommate["FirstName"].ToString() + roommate["LastName"].ToString()))
                {
                    PlaceHolder phRow = e.Item.FindControl("phRoommate") as PlaceHolder;
                    TextBox tbEmail = new TextBox();
                    tbEmail.ID = "tbFindEmail";
                    phRow.Controls.Add(tbEmail);

                    Literal ltlEmailSuffix = new Literal();
                    ltlEmailSuffix.Text = "@carthage.edu";
                    phRow.Controls.Add(ltlEmailSuffix);

                    Button btnCheckEmail = new Button();
                    btnCheckEmail.Text = "Check Email";
                    btnCheckEmail.UseSubmitBehavior = false;
                    btnCheckEmail.Click += btnCheckEmail_Click;
                    phRow.Controls.Add(btnCheckEmail);
                }
            }
        }

        private void btnCheckEmail_Click(object sender, EventArgs e)
        {
            bool okToEmail = false;
            string reasonForFailure = "";

            Button btn = (Button)sender;
            TextBox tbEmail = btn.NamingContainer.FindControl("tbFindEmail") as TextBox;
            string cleanEmail = tbEmail.Text + (tbEmail.Text.EndsWith("@carthage.edu") ? "" : "@carthage.edu");

            PortalUser invited = ObjectFactoryWrapper.GetInstance<IPortalUserFacade>().FindByEmail(cleanEmail);
            if (invited == null)
            {
                reasonForFailure = "There is no information about that student. Please double-check the email address you entered.";
            }
            else
            {
                if (invited.Guid == PortalUser.Current.Guid)
                {
                    reasonForFailure = "You cannot reserve a bed for yourself, you've already signed up for a bed.";
                }
                else
                {
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
                    }
                    catch (Exception ex)
                    {
                        this.ParentPortlet.ShowFeedback(FeedbackType.Error, String.Format("{0}<br /><br />{1}", ex.Message, ex.InnerException));
                    }

                    if (dtInRoom != null && dtInRoom.Rows.Count > 0)
                    {
                        reasonForFailure = String.Format("{0} {1} has already registered for a room.", invited.FirstName, invited.LastName);
                    }
                    else
                    {
                        string genderSQL = String.Format("SELECT sex FROM profile_rec WHERE profile_rec.id = {0}", invited.HostID);
                        Exception exGender = null;
                        DataTable dtGender = null;
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
                        okToEmail = genderMatch;
                    }
                }
            }


            this.panelStudentDetail.Visible = true;
            this.panelStudentOK.Visible = okToEmail;
            this.panelStudentNo.Visible = !okToEmail;
            if (okToEmail)
            {
                this.ltlStudentName.Text = invited.ToFirstLastNameDisplay().ToString();
                this.ltlStudentEmail.Text = invited.EmailAddress;
                this.btnSendInvitation.CommandArgument = invited.HostID;
            }
            else
            {
                this.ltlStudentEmail2.Text = cleanEmail;
                this.ltlReason.Text = reasonForFailure;
                this.btnSendInvitation.CommandArgument = "0";
            }
        }

        protected void btnSendInvitation_Click(object sender, EventArgs e)
        {
            Button btnInvite = sender as Button;

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

                    string emailTo = recipient.EmailAddress;
                    string emailSubject = "Housing Roommate Invitation";

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

                }
            }
            catch (Exception ex)
            {
                this.ParentPortlet.ShowFeedback(FeedbackType.Error, String.Format("{0}<br /><br />{1}", ex.Message, ex.InnerException.ToString()));
            }
        }
    }
}