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

        public string SpringSession { get { return "RA"; } }
        public string FallSession { get { return "RC"; } }
        //public string CurrentYear { get { return new DateTime().Year.ToString(); } }
        public string CurrentYear { get { return "2014"; } }
        public string NextYear { get { return (CurrentYear + 1).ToString(); } }
        public string BeginInvolvement { get { return (int.Parse(CurrentYear) - 3).ToString() + "-01-01"; } }
        //public int UserID { get { return int.Parse(PortalUser.Current.HostID); } }
        public int UserID { get { return 1339128; } }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (this.IsFirstLoad)
            {
                InitScreenStatic();
            }
        }

        protected void InitScreenStatic()
        {
            string studentSQL = @"
                    SELECT
                        id_rec.id, TRIM(id_rec.firstname) AS firstname, TRIM(id_rec.lastname) AS lastname, profile_rec.sex, TRIM(aa_rec.line1) AS email, stu_stat_rec.cum_earn_hrs AS careerhours,
                        TRIM(NVL(stu_serv_rec.bldg,'')) AS bldg, TRIM(NVL(stu_serv_rec.room,'')) AS room, stu_acad_rec.reg_hrs AS registered_hours, NVL(ADVpay.hld,'') AS advPayHold,
                        NVL(UNBal.hld,'') AS unbalHold, involve_rec.invl AS greekid, invl_table.txt AS greekname, TRIM(upcoming.bldg) AS upcomingBldg, TRIM(upcoming.room) AS upcomingRoom
                    FROM id_rec	INNER JOIN	profile_rec			        ON	id_rec.id			=	profile_rec.id
                                LEFT JOIN	aa_rec				        ON	id_rec.id			=	aa_rec.id
											                            AND	aa_rec.aa			=	'EML1'
                                LEFT JOIN	stu_stat_rec		        ON	id_rec.id			=	stu_stat_rec.id
                                LEFT JOIN	stu_serv_rec		        ON	id_rec.id			=	stu_serv_rec.id
											                            AND	stu_serv_rec.sess	=	?
											                            AND	stu_serv_rec.yr		=	YEAR(TODAY)
			                    LEFT JOIN	stu_serv_rec	upcoming	ON	id_rec.id	        =	upcoming.id
											                            AND	upcoming.sess		=	?
											                            AND	upcoming.yr			=	YEAR(TODAY)
			                    LEFT JOIN	stu_acad_rec		        ON	id_rec.id			=	stu_acad_rec.id
											                            AND	stu_acad_rec.sess	=	?
											                            AND	stu_acad_rec.yr		=	YEAR(TODAY)
			                    LEFT JOIN	hold_rec	    ADVpay	    ON	id_rec.id			=	ADVpay.id
											                            AND	TODAY			BETWEEN	ADVpay.beg_date	AND	NVL(ADVpay.end_date, TODAY)
											                            AND	ADVpay.hld			=	'APAY'
			                    LEFT JOIN	hold_rec	UNBal	        ON	id_rec.id			=	UNBal.id
											                            AND	TODAY			BETWEEN	UNBal.beg_date	AND	NVL(UNBal.end_date, TODAY)
											                            AND	UNBal.hld			=	'UBAL'
			                    LEFT JOIN	(
				                    SELECT		invl, id, MAX(beg_date) AS beg_date, MAX(end_date) AS end_date
				                    FROM		involve_rec
				                    WHERE		id = ?
				                    GROUP BY	invl, id
			                    )			involve_rec			ON	id_rec.id			=	involve_rec.id
											                    AND
											                    (
												                    NVL(involve_rec.invl,'')	IN	('','S007','S025','S045','S061','S063','S092','S141','S152','S165','S168','S189','S190','S192','S194')
											                    )
											                    AND	NVL(involve_rec.beg_date, ?)	>=	?
			                    LEFT JOIN	invl_table			ON	involve_rec.invl	=	invl_table.invl
                    WHERE id_rec.id	=	?
                ";

            List<OdbcParameter> parameters = new List<OdbcParameter>
                {
                    new OdbcParameter("SpringSess", SpringSession),
                    new OdbcParameter("FallSess", FallSession),
                    new OdbcParameter("FallSess2", FallSession),
                    new OdbcParameter("ID", UserID),
                    new OdbcParameter("InvolveStartDate", BeginInvolvement),
                    new OdbcParameter("InvolveStartDate2", BeginInvolvement),
                    new OdbcParameter("ID2", UserID)
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
                this.ParentPortlet.ShowFeedback(FeedbackType.Error, ee.Message);
            }
            finally
            {
                //Regardless of whether the data loaded correctly or an error occurred, if the connection is still open, close it.
                if (odbcConn.IsNotClosed()) { odbcConn.Close(); }
            }

            //If the query returned results, load the data.
            if (dt != null && dt.Rows.Count > 0)
            {
                DataRow dr = dt.Rows[0];
                
                this.ltlStudentName.Text = String.Format("{0} {1}", dt.Rows[0]["firstname"].ToString(), dt.Rows[0]["lastname"].ToString());

                this.panelRegistered.Visible = this.ltlRegisteredHousing.Text.Length > 0;
                this.ltlRegisteredHousing.Text = "Denhart 120";

                DateTime registeredDateTime = DateTime.Now;
                this.ltlRegisteredDateTime.Text = String.Format("{0:hh:mm:ss tt on dddd MMMM d, yyyy}", registeredDateTime);
                //this.bulletedRoommates - load roommate list

                this.panelCommuter.Visible = this.ltlRegisteredHousing.Text == "CMTR";

                this.panelUnregistered.Visible = this.ltlRegisteredHousing.Text.Length == 0;

                this.ltlGreekStatus.Text = "a member of [GreekOrgName]"; //"not a member of a residential fraternity or sorority"
                this.ltlHold.Text = ""; //"not"
                //this.ltlHoldDetail.Visible = this.ltlHold.Text.Length > 0;
                this.contentHoldDetail.Visible = this.ltlHold.Text.Length > 0;
                this.ltlRegistered.Text = ""; //"not"
                //this.panelRegisteredDetail.Visible = this.ltlRegistered.Text.Length > 0;
                this.contentRegisteredDetail.Visible = this.ltlRegistered.Text.Length > 0;
                this.ltlRegisteredYear.Text = String.Format("{0} - {1}", CurrentYear, NextYear);
                this.ltlGender.Text = "You are male"; //"female", "Our records don't indicate your gender"
                this.ltlCareerCredits.Text = "120";
                this.ltlCurrentHousing.Text = "Denhart 119";
                this.panelCurrentHousing.Visible = this.ltlCurrentHousing.Text.Length > 0;
                this.ltlNotResident.Visible = this.ltlCurrentHousing.Text.Length == 0;

                bool isValidTime = true, mayRegister = true;
                this.lnkAvailability.Visible = isValidTime && mayRegister;
                this.ltlCannotRegister.Visible = isValidTime && !mayRegister;
                this.ltlInvalidTime.Visible = !isValidTime;
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

        protected void InitScreen()
        {
            try
            {
                Exception ex = null;
                string studentSQL = @"
                    SELECT
                        id_rec.id, TRIM(id_rec.firstname) AS firstname, TRIM(id_rec.lastname) AS lastname, profile_rec.sex, TRIM(aa_rec.line1) AS email, stu_stat_rec.cum_earn_hrs AS careerhours,
                        TRIM(stu_serv_rec.bldg) AS bldg, TRIM(stu_serv_rec.room) AS room, stu_acad_rec.reg_hrs AS registered_hours, ADVpay.hld AS advPayHold, UNBal.hld AS unbalHold,
                        involve_rec.invl AS greekid, invl_table.txt AS greekname, TRIM(upcoming.bldg) AS upcomingBldg, TRIM(upcoming.room) AS upcomingRoom
                    FROM id_rec	INNER JOIN	profile_rec			        ON	id_rec.id			=	profile_rec.id
                                LEFT JOIN	aa_rec				        ON	id_rec.id			=	aa_rec.id
											                            AND	aa_rec.aa			=	'EML1'
                                LEFT JOIN	stu_stat_rec		        ON	id_rec.id			=	stu_stat_rec.id
                                LEFT JOIN	stu_serv_rec		        ON	id_rec.id			=	stu_serv_rec.id
											                            AND	stu_serv_rec.sess	=	@SpringSess
											                            AND	stu_serv_rec.yr		=	YEAR(TODAY)
			                    LEFT JOIN	stu_serv_rec	upcoming	ON	id_rec.id	        =	upcoming.id
											                            AND	upcoming.sess		=	@FallSess
											                            AND	upcoming.yr			=	YEAR(TODAY)
			                    LEFT JOIN	stu_acad_rec		        ON	id_rec.id			=	stu_acad_rec.id
											                            AND	stu_acad_rec.sess	=	@FallSess2
											                            AND	stu_acad_rec.yr		=	YEAR(TODAY)
			                    LEFT JOIN	hold_rec	    ADVpay	    ON	id_rec.id			=	ADVpay.id
											                            AND	TODAY			BETWEEN	ADVpay.beg_date	AND	NVL(ADVpay.end_date, TODAY)
											                            AND	ADVpay.hld			=	'APAY'
			                    LEFT JOIN	hold_rec	UNBal	        ON	id_rec.id			=	UNBal.id
											                            AND	TODAY			BETWEEN	UNBal.beg_date	AND	NVL(UNBal.end_date, TODAY)
											                            AND	UNBal.hld			=	'UBAL'
			                    LEFT JOIN	(
				                    SELECT		invl, id, MAX(beg_date) AS beg_date, MAX(end_date) AS end_date
				                    FROM		involve_rec
				                    WHERE		id = @ID
				                    GROUP BY	invl, id
			                    )			involve_rec			ON	id_rec.id			=	involve_rec.id
											                    AND
											                    (
												                    NVL(involve_rec.invl,'')	IN	('','S007','S025','S045','S061','S063','S092','S141','S152','S165','S168','S189','S190','S192','S194')
											                    )
											                    AND	NVL(involve_rec.beg_date,@InvolveStartDate)	>=	@InvolveStartDate2
			                    LEFT JOIN	invl_table			ON	involve_rec.invl	=	invl_table.invl
                    WHERE id_rec.id	=	@ID2
                ";

                List<OdbcParameter> parameters = new List<OdbcParameter>
                {
                    new OdbcParameter("@SpringSess", SpringSession),
                    new OdbcParameter("@FallSess", FallSession),
                    new OdbcParameter("@FallSess2", FallSession),
                    new OdbcParameter("@ID", int.Parse(PortalUser.Current.HostID)),
                    new OdbcParameter("@InvolveStartDate", BeginInvolvement),
                    new OdbcParameter("@InvolveStartDate2", BeginInvolvement),
                    new OdbcParameter("@ID2", int.Parse(PortalUser.Current.HostID))
                };

                DataTable dt = null;
                /*
                dt = odbcConn.ConnectToERP(studentSQL, ref ex, parameters);
                if (dt.Rows.Count > 1)
                {
                    this.errMsg.ErrorMessage = "Too many rows were found. Please check the SQL and verify the results.";
                }
                */
            }
            catch (Exception ee)
            {
                this.ParentPortlet.ShowFeedback(FeedbackType.Error, ee.Message);
            }
            finally
            {
                if (odbcConn.IsNotClosed())
                {
                    odbcConn.Close();
                }
            }
        }

        protected void lnkAvailability_Click(object sender, EventArgs e)
        {
            //Click on the "availability" link takes the user to the building/room selection screen
            this.ParentPortlet.NextScreen("AvailabilityBuilding");
        }
    }
}