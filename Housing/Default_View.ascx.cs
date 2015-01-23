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

        public string SpringSession { get { return "RC"; } }
        public string FallSession { get { return "RA"; } }
        public string CurrentYear { get { return new DateTime().Year.ToString(); } }
        public string NextYear { get { return (new DateTime().Year + 1).ToString(); } }
        public string BeginInvolvement { get { return (new DateTime().Year - 3).ToString() + "-01-01"; } }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (this.IsFirstLoad)
            {
                InitScreen();
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
    }
}