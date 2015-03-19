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
using Jenzabar.Common;
using Jenzabar.Portal.Framework;
using Jenzabar.Portal.Framework.EmailLogging;   //EmailLogger
using Jenzabar.Portal.Framework.Facade; //IPortalUserFacade
using Jenzabar.Common.Mail; //ValidEmail()
using Jenzabar.Portal.Framework.Web.UI;
using CUS.OdbcConnectionClass3;

namespace Housing
{
    public partial class Send_Invitations_View : PortletViewBase
    {
        OdbcConnectionClass3 jicsConn = new OdbcConnectionClass3("JICSDataConnection.config");
        public override string ViewName { get { return "Roommate Invitations"; } }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (this.IsFirstLoad)
            {
                InitScreen();
            }
        }

        private void InitScreen()
        {
            string roomSQL = "SELECT Building.BuildingName, Building.BuildingCode, Room.* FROM CUS_Housing_Room Room INNER JOIN CUS_Housing_Building Building ON Room.BuildingID = Building.BuildingID WHERE Room.RoomID = ?";
            Exception ex = null;
            DataTable dt = null;
            List<OdbcParameter> parameters = new List<OdbcParameter>
            {
                new OdbcParameter("roomID", this.ParentPortlet.PortletViewState["RoomID"].ToString())
            };

            try
            {
                dt = jicsConn.ConnectToERP(roomSQL, ref ex, parameters);
                DataRow room = dt.Rows[0];
                this.ltlRoomSelected.Text = String.Format("{0} {1} ({2})", room["BuildingName"].ToString(), room["RoomNumber"].ToString(), this.ParentPortlet.PortletViewState["RoomID"].ToString());

                sendTestEmail();
            }
            catch(Exception ee)
            {
                this.ParentPortlet.ShowFeedback(FeedbackType.Error, String.Format("{0}<br /><br />{1}", ee.InnerException, ee.Message));
            }
            finally
            {
                if (jicsConn.IsNotClosed()) { jicsConn.Close(); }
            }
        }

        protected void dlRoommates_ItemDataBound(object sender, DataListItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {

            }
        }

        protected void sendTestEmail()
        {
            string smtpAddress = ConfigSettings.Current.SmtpDefaultEmailAddress;
            smtpAddress = ObjectFactoryWrapper.GetInstance<IPortalUserFacade>().FindByEmail("nfleming@carthage.edu").EmailAddress;
            /*
            string emailTo = "mkishline@carthage.edu", emailSubject = "Room Registration";
            string emailBody = String.Format(
                @"<p>Congratulations {0}, your reservation for #form.bldg# #form.room# has been entered into our database.</p>
                <p>If you have any questions pertaining to your housing for the #Year(Now())#-#Year(Now())+1# academic year please contact the Office of Student Life in the Todd Wehr Center.</p>
                <p>Thank you for using the Housing Selection Process for #Year(Now())#-#Year(Now())+1#.</p>", PortalUser.Current.FirstName
            );
            if (!String.IsNullOrEmpty(emailTo) && (new ValidEmail(emailTo).IsValid) && Email.CreateAndSendMailMessage(smtpAddress, emailTo, String.Empty, String.Empty, emailSubject, emailBody, System.Web.Mail.MailFormat.Html))
            {
                PortalUser user = ObjectFactoryWrapper.GetInstance<IPortalUserFacade>().FindByEmail("mkishline@carthage.edu");
                //PortalUser user2 = ObjectFactoryWrapper.GetInstance<IPortalUserFacade>().FindAllPortalOnlyUsers()
            }
            */
            /*
            if (!(new Jenzabar.Common.Mail.ValidEmail(emailTo)).IsValid)
            {
                //Take steps to process an invalid email address
                this.ParentPortlet.ShowFeedback(FeedbackType.Error, String.Format("Invalid email address: {0}", emailTo));
            }
            else if (!Jenzabar.Common.Mail.Email.CreateAndSendMailMessage(emailFrom, emailTo, null, null, emailSubject, emailBody, System.Web.Mail.MailFormat.Html))
            {
                //Unable to send email, log failure
                this.ParentPortlet.ShowFeedback(FeedbackType.Error, "Failure when attempting to send email");
            }
            else
            {
                //Show a success message and move forward
                this.ParentPortlet.ShowFeedback(FeedbackType.Message, "Email was sent successfully");
            }
            */
        }
    }
}