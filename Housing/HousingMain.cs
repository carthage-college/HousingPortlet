using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Jenzabar.Portal.Framework.Web;
using Jenzabar.Portal.Framework.Web.UI;

namespace Housing
{
    public class HousingMain : PortletBase
    {
        
        protected override PortletViewBase GetCurrentScreen()
        {
            PortletViewBase screen = null;
            switch(this.CurrentPortletScreenName)
            {
                //Choose from a list of available buildings
                case "AvailabilityBuilding":
                    screen = this.LoadPortletView("ICS/Portlet.Housing/Availability_Building_View.ascx");
                    break;
                //Based on a pre-selected building, choose the desired bed from a list of available rooms
                case "AvailabilityRoom":
                    screen = this.LoadPortletView("ICS/Portlet.Housing/Availability_Room_View.ascx");
                    break;
                //Accept the terms and conditions of the room
                case "AcceptRoom":
                    screen = this.LoadPortletView("ICS/Portlet.Housing/Accept_Room_View.ascx");
                    break;
                case "SendInvitations":
                    screen = this.LoadPortletView("ICS/Portlet.Housing/Send_Invitations_View.ascx");
                    break;
                //Start screen
                case "Default":
                default:
                    screen = this.LoadPortletView("ICS/Portlet.Housing/Default_View.ascx");
                    break;
            }
            return screen;
        }
    }
}