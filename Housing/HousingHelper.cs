using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using Jenzabar.Common;
using Jenzabar.Portal.Framework;
using Jenzabar.Portal.Framework.Facade;
using CUS.OdbcConnectionClass3;

namespace Housing
{
    public class HousingHelper
    {
        OdbcConnectionClass3 spConn = new OdbcConnectionClass3("JICSDataConnection.config", true);

        public static string SETTING_KEY_END_HOUR = "END_TIME";
        public static string SETTING_KEY_IS_PRODUCTION = "IS_PRODUCTION";
        public static string SETTING_KEY_SEND_EMAIL = "SEND_EMAIL";
        public static string SETTING_KEY_TEST_EMAIL_ADDRESS = "TEST_EMAIL";
        public static string SETTING_KEY_HOUSING_DIRECTOR_NAME = "HOUSING_DIRECTOR_NAME";
        public static string SETTING_KEY_HOUSING_DIRECTOR_EMAIL = "HOUSING_DIRECTOR_EMAIL";



        public bool IS_PRODUCTION
        {
            get
            {
                return bool.Parse(GetHousingSetting(SETTING_KEY_IS_PRODUCTION));
            }
        }

        public bool SEND_EMAIL
        {
            get
            {
                return bool.Parse(GetHousingSetting(SETTING_KEY_SEND_EMAIL));
            }
        }

        public string TEST_EMAIL_ADDRESS
        {
            get
            {
                return GetHousingSetting(SETTING_KEY_TEST_EMAIL_ADDRESS);
            }
        }

        public string HOUSING_ADMIN_NAME
        {
            get
            {
                //return "Amber Krusza";
                return GetHousingSetting(SETTING_KEY_HOUSING_DIRECTOR_NAME);
            }
        }

        public string HOUSING_ADMIN_EMAIL
        {
            get
            {
                //return "akrusza@carthage.edu";
                return GetHousingSetting(SETTING_KEY_HOUSING_DIRECTOR_EMAIL);
            }
        }

        public string GetHousingSetting(string settingKey)
        {
            string settingSQL = String.Format("EXECUTE [dbo].[CUS_spHousing_getHousingSetting] @strSettingKey = ?");

            string settingValue = "";
            Exception exSetting = null;
            DataTable dtSetting = null;
            List<OdbcParameter> settingParam = new List<OdbcParameter>
            {
                new OdbcParameter("SettingKey", settingKey)
            };

            try
            {
                dtSetting = spConn.ConnectToERP(settingSQL, ref exSetting, settingParam);
                if (exSetting != null) { throw exSetting; }
                if (dtSetting != null && dtSetting.Rows.Count > 0)
                {
                    settingValue = dtSetting.Rows[0]["SettingValue"].ToString();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return settingValue;
        }

        public bool sendEmailOk()
        {
            bool isProduction = false;
            bool.TryParse(GetHousingSetting(HousingHelper.SETTING_KEY_IS_PRODUCTION), out isProduction);

            bool sendEmail = false;
            bool.TryParse(GetHousingSetting(HousingHelper.SETTING_KEY_SEND_EMAIL), out sendEmail);

            return isProduction && sendEmail;
        }
    }
}