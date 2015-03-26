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
            string roomSQL = @"
                SELECT
                    Building.BuildingName, Building.BuildingCode, Room.RoomNumber
                FROM
                    CUS_Housing_Room    Room    INNER JOIN  CUS_Housing_Building    Building    ON  Room.BuildingID =   Building.BuildingID
                                                INNER JOIN  CUS_Housing_RoomSession HRS         ON  Room.RoomID     =   HRS.RoomID
                                                                                                AND HRS.HousingYear =   YEAR(GETDATE())
                WHERE
                    HRS.RoomSessionID = ?";
            Exception ex = null;
            DataTable dt = null;
            List<OdbcParameter> parameters = new List<OdbcParameter>
            {
                new OdbcParameter("roomSessionID", this.ParentPortlet.PortletViewState["RoomSessionID"].ToString())
            };

            try
            {
                dt = jicsConn.ConnectToERP(roomSQL, ref ex, parameters);
                DataRow room = dt.Rows[0];
                this.ltlRoomSelected.Text = String.Format("{0} {1}", room["BuildingName"].ToString(), room["RoomNumber"].ToString());
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
    }
}