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
        OdbcConnectionClass3 odbcConn = new OdbcConnectionClass3("ERPDataConnection.config");

        public override string ViewName { get { return "Housing Sign-up Application"; } }

        public string SpringSession { get { return "RC"; } }
        public string FallSession { get { return "RA"; } }
        //public string CurrentYear { get { return new DateTime().Year.ToString(); } }
        public string CurrentYear { get { return "2014"; } }
        public string NextYear { get { return (int.Parse(CurrentYear) + 1).ToString(); } }
        public string BeginInvolvement { get { return (int.Parse(CurrentYear) - 3).ToString() + "-01-01"; } }
        //public int UserID { get { return int.Parse(PortalUser.Current.HostID); } }
        public int UserID { get { return 1339128; } }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (this.IsFirstLoad)
            {
                InitScreen();
            }
        }

        protected void InitScreen()
        {
            string studentSQL = String.Format(@"
                    SELECT
                        id_rec.id, TRIM(id_rec.firstname) AS firstname, TRIM(id_rec.lastname) AS lastname, profile_rec.sex, TRIM(aa_rec.line1) AS email, NVL(stu_stat_rec.cum_earn_hrs, 0) AS career_hours,
                        TRIM(NVL(curbldg.txt,'')) AS bldg, TRIM(NVL(stu_serv_rec.room,'')) AS room, NVL(stu_acad_rec.reg_hrs, 0) AS registered_hours, NVL(ADVpay.hld,'') AS advpayhold,
                        NVL(UNBal.hld,'') AS unbal_hold, involve_rec.invl AS greekid, TRIM(involve_rec.org) AS greek_name, TRIM(NVL(newbldg.txt,'')) AS upcoming_bldg, TRIM(NVL(upcoming.room,'')) AS upcoming_room
                    FROM id_rec	INNER JOIN	profile_rec			        ON	id_rec.id			=	profile_rec.id
                                LEFT JOIN	aa_rec				        ON	id_rec.id			=	aa_rec.id
											                            AND	aa_rec.aa			=	'EML1'
                                LEFT JOIN	stu_stat_rec		        ON	id_rec.id			=	stu_stat_rec.id
                                LEFT JOIN	stu_serv_rec		        ON	id_rec.id			=	stu_serv_rec.id
											                            AND	stu_serv_rec.sess	=	?
											                            AND	stu_serv_rec.yr		=	{0}
                                LEFT JOIN   bldg_table      curbldg     ON  stu_serv_rec.bldg   =   curbldg.bldg
			                    LEFT JOIN	stu_serv_rec	upcoming	ON	id_rec.id	        =	upcoming.id
											                            AND	upcoming.sess		=	?
											                            AND	upcoming.yr			=	stu_serv_rec.yr
                                LEFT JOIN   bldg_table      newbldg     ON  upcoming.bldg       =   newbldg.bldg
			                    LEFT JOIN	stu_acad_rec		        ON	id_rec.id			=	stu_acad_rec.id
											                            AND	stu_acad_rec.sess	=	?
											                            AND	stu_acad_rec.yr		=	stu_serv_rec.yr
			                    LEFT JOIN	hold_rec	    ADVpay	    ON	id_rec.id			=	ADVpay.id
											                            AND	TODAY			BETWEEN	ADVpay.beg_date	AND	NVL(ADVpay.end_date, TODAY)
											                            AND	ADVpay.hld			=	'APAY'
			                    LEFT JOIN	hold_rec	UNBal	        ON	id_rec.id			=	UNBal.id
											                            AND	TODAY			BETWEEN	UNBal.beg_date	AND	NVL(UNBal.end_date, TODAY)
											                            AND	UNBal.hld			=	'UBAL'
			                    LEFT JOIN	(
				                    SELECT		invl, org, id, MAX(beg_date) AS beg_date, MAX(end_date) AS end_date
				                    FROM		involve_rec
				                    WHERE		id = {1}
				                    GROUP BY	invl, org, id
			                    )			involve_rec			ON	id_rec.id			=	involve_rec.id
											                    AND
											                    (
												                    NVL(involve_rec.invl,'')	IN	('','S007','S025','S045','S061','S063','S092','S141','S152','S165','S168','S189','S190','S192','S194')
											                    )
											                    AND	NVL(involve_rec.beg_date, '{2}')	>=	?
                                                                AND NVL(involve_rec.end_date, TODAY)    >=  TODAY
                    WHERE id_rec.id	=	{3}
                ", CurrentYear, UserID, BeginInvolvement, UserID);

            List<OdbcParameter> parameters = new List<OdbcParameter>
                {
                      new OdbcParameter("SpringSess", SpringSession)
                    , new OdbcParameter("FallSess", FallSession)
                    , new OdbcParameter("FallSess2", FallSession)
                    , new OdbcParameter("InvolveStartDate", BeginInvolvement)
                };
            Exception ex = null;
            DataTable dt = null;

            try
            {
                //Attempt to execute the query
                dt = odbcConn.ConnectToERP(studentSQL, ref ex, parameters);
                
                //If the SQL (with parameters) generated an error, throw an exception
                if (ex != null) { throw ex; }
            }
            catch(Exception ee)
            {
                //If an exception was thrown, display the message to the user.
                //TODO: Consider turning this off unless the user is an administrator. Alternatively, throw custom exceptions where possible with more readable errors.
                this.ParentPortlet.ShowFeedback(FeedbackType.Error, String.Format("{0}<br /><br />{1}", ee.Message, ee.InnerException.ToString()));
            }
            finally
            {
                //Regardless of whether the data loaded correctly or an error occurred, if the connection is still open, close it.
                if (odbcConn.IsNotClosed()) { odbcConn.Close(); }
                this.errMsg.ErrorMessage = studentSQL;
            }

            //If the query returned results, load the data.
            if (dt != null && dt.Rows.Count > 0)
            {
                DataRow dr = dt.Rows[0];
                
                this.ltlStudentName.Text = String.Format("{0} {1}", dr["firstname"].ToString(), dr["lastname"].ToString());

                this.ltlRegisteredHousing.Text = String.Format("{0} {1}", dr["upcoming_bldg"].ToString(), dr["upcoming_room"].ToString()).Trim();
                //this.panelRegistered.Visible = this.ltlRegisteredHousing.Text.Length > 0;

                DateTime registeredDateTime = DateTime.Now;
                this.ltlRegisteredDateTime.Text = String.Format("{0:hh:mm:ss tt on dddd MMMM d, yyyy}", registeredDateTime);
                //this.bulletedRoommates - load roommate list

                //this.panelCommuter.Visible = this.ltlRegisteredHousing.Text == "CMTR";

                //this.panelUnregistered.Visible = this.ltlRegisteredHousing.Text.Length == 0;

                //this.ltlGreekStatus.Text = dr["greekname"].ToString().Length > 0 ? String.Format("a member of {0}", dr["greekname"].ToString()) : "not a member of a residential fraternity or sorority";
                this.ltlGreekStatus.Text = dr["greek_name"].ToString();
                this.ltlHold.Text = ""; //"not"
                this.ltlHold.Text = String.Format("{0} {1}", dr["advPayHold"].ToString(), dr["unbal_hold"].ToString()).Trim();
                //this.contentHoldDetail.Visible = this.ltlHold.Text.Length > 0;
                //A student must be registered for 12 or more credits in the upcoming semester
                this.ltlRegistered.Text = int.Parse(dr["registered_hours"].ToString()) >= 12 ? "" : "not";
                
                //this.contentRegisteredDetail.Visible = this.ltlRegistered.Text.Length > 0;
                
                this.ltlRegisteredYear.Text = String.Format("{0} - {1}", CurrentYear, NextYear);
                //Determine the proper display of student's gender
                switch (dr["sex"].ToString())
                {
                    case "M":
                        this.ltlGender.Text = "You are male";
                        break;
                    case "F":
                        this.ltlGender.Text = "You are female";
                        break;
                    default:
                        this.ltlGender.Text = "Our records don't indicate your gender";
                        break;
                }
                //Display the total number of credit hours earned by the student over their academic career
                this.ltlCareerCredits.Text = dr["career_hours"].ToString();

                this.ltlCurrentHousing.Text = String.Format("{0} {1}", dr["bldg"].ToString(), dr["room"].ToString());
                //this.panelCurrentHousing.Visible = this.ltlCurrentHousing.Text.Length > 0;
                //this.ltlNotResident.Visible = this.ltlCurrentHousing.Text.Length == 0;

                bool isValidTime = true, mayRegister = true;
                //this.lnkAvailability.Visible = isValidTime && mayRegister;
                //this.ltlCannotRegister.Visible = isValidTime && !mayRegister;
                //this.ltlInvalidTime.Visible = !isValidTime;

                foreach(DataColumn dc in dt.Columns)
                {
                    this.ltlNotResident.Text += String.Format("<br />{0}: {1}", dc.ColumnName, dr[dc.ColumnName].ToString());
                }
            }
            else if (dt != null && dt.Rows.Count == 0)
            {
                /*
                this.ParentPortlet.ShowFeedback(
                    FeedbackType.Error,
                    "No records were found that matched your ID.";
                );
                */
            }
            else
            {
                /*
                this.ParentPortlet.ShowFeedback(
                    FeedbackType.Error,
                    "We were unable to retrieve your information. Please contact Nina Fleming <a href='mailto:nfleming@carthage.edu'>nfleming@carthage.edu</a> to resolve this issue."
                );
                */
            }
        }

        protected void lnkAvailability_Click(object sender, EventArgs e)
        {
            //Click on the "availability" link takes the user to the building/room selection screen
            this.ParentPortlet.NextScreen("AvailabilityBuilding");
        }
    }
}