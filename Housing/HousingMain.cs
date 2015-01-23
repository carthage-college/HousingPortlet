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
                case "Main":
                    screen = this.LoadPortletView("ICS/Portlet.Housing/Main_View.ascx");
                    break;
                case "Default"
                default:
                    screen = this.LoadPortletView("ICS/Portelt.Housing/Default_View.ascx");
                    break;
            }
            return screen;
            //throw new NotImplementedException();
        }
    }
}