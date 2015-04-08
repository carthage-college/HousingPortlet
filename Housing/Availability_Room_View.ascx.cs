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
    public partial class Availability_Room_View : PortletViewBase
    {
        #region Public Variables
        OdbcConnectionClass3 jicsConn = new OdbcConnectionClass3("JICSDataConnection.config", true);
        public static string COMMAND_NAME_ROOM = "PickRoom";
        public bool IsOaks = false;
        #endregion

        public override string ViewName { get { return "Choose A Room"; } }

        protected override void OnInit(EventArgs e)
        {
            InitScreen();
            base.OnInit(e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (this.IsFirstLoad)
            {
                //InitScreen();
            }

            if (this.ParentPortlet.PortletViewState["Message"] != null && this.ParentPortlet.PortletViewState["Message"].ToString().Length > 0)
            {
                //this.ParentPortlet.ShowFeedback(FeedbackType.Error, this.ParentPortlet.PortletViewState["Message"].ToString());
                this.errMsg.ErrorMessage = this.ParentPortlet.PortletViewState["Message"].ToString();
            }
        }

        protected void InitScreen()
        {
            //Check to see if a building is stored in the viewstate. If not, the user is redirected to the building selection page before being allowed to proceed.
            if (this.ParentPortlet.PortletViewState["Building"] == null || this.ParentPortlet.PortletViewState["Building"].ToString().Length == 0)
            {
                this.ParentPortlet.PortletViewState["Message"] = "Please make sure to select a building before proceeding through the housing sign-up.";
                this.ParentPortlet.PreviousScreen("AvailabilityBuilding");
            }

            string buildingID = this.ParentPortlet.PortletViewState["Building"].ToString();

            //Find the name of the building selected by the user on the previous page
            string buildingSQL = "SELECT BuildingName FROM CUS_Housing_Building WHERE BuildingID = ?";
            //Initialize variables that will process results from the query
            Exception ex = null;
            DataTable dtBuilding = null;
            List<OdbcParameter> parameters = new List<OdbcParameter>
            {
                //Get the buildingID variable from the viewstate
                new OdbcParameter("buildingID", buildingID)
            };

            try
            {
                dtBuilding = jicsConn.ConnectToERP(buildingSQL, ref ex, parameters);
                if (ex != null) { throw ex; }

                //If the query returned a row, assign the building's name to the appropriate textfield
                if (dtBuilding != null && dtBuilding.Rows.Count == 1)
                {
                    this.ltlBuildingName.Text = dtBuilding.Rows[0]["BuildingName"].ToString();
                    IsOaks = dtBuilding.Rows[0]["BuildingName"].ToString().StartsWith("Oaks");
                }
                else
                {
                    this.ParentPortlet.ShowFeedback(FeedbackType.Error, "We were unable to find the details of the building you selected. Please pick a different building and try again.");
                }
            }
            catch (Exception ee)
            {
                this.ParentPortlet.ShowFeedback(FeedbackType.Error, String.Format("{0}<br /><br />{1}", ee.Message, ee.InnerException.ToString()));
            }

            // Params: [0] = GreekID (invl_table.invl), [1] = Gender, [2] = StudentID
            string greekSquatterSQL = String.Format(@"
                SELECT
                    HB.BuildingCode, HRS.RoomSessionID, HR.RoomNumber, HR.[Floor], HR.[Wing], HR.[Capacity], COUNT(HRStu.RoomStudentID) AS Occupancy
                FROM
                    CUS_Housing_Room	HR	INNER JOIN  CUS_Housing_Building	    HB		ON  HR.BuildingID		=   HB.BuildingID
								            INNER JOIN	CUS_Housing_RoomSession	    HRS		ON	HR.RoomID			=	HRS.RoomID
																						    AND	HRS.HousingYear		=	YEAR(GETDATE())
											LEFT JOIN	CUS_Housing_RoomStudent	    HRStu	ON	HRS.RoomSessionID	=	HRStu.RoomSessionID
                                            LEFT JOIN	CUS_HousingSelectionGreek	HSG		ON	HRS.GreekID			=	HSG.greekid
                WHERE
                    HSG.invl     	=   ?
		            AND
		            HRS.Gender		IN	(?,'')
                    AND
                    HR.[Capacity]   >   0
                GROUP BY
					HB.BuildingCode, HRS.RoomSessionID, HR.RoomNumber, HR.[Floor], HR.Wing, HR.Capacity
                UNION
                SELECT
                    HB.BuildingCode, HRS.RoomSessionID, HR.RoomNumber, HR.[Floor], HR.[Wing], HR.[Capacity], COUNT(HRStu.RoomStudentID) AS Occupancy
                FROM
                    CUS_Housing_Room	HR	INNER JOIN  CUS_Housing_Building	    HB		ON  HR.BuildingID		=   HB.BuildingID
								            INNER JOIN	CUS_Housing_RoomSession	    HRS		ON	HR.RoomID			=	HRS.RoomID
																					    	AND	HRS.HousingYear		=	YEAR(GETDATE())
                                            INNER JOIN  CUS_Housing_SessionOccupant HSO     ON  HRS.RoomSessionID   =   HSO.RoomSessionID
											LEFT JOIN	CUS_Housing_RoomStudent	    HRStu	ON	HRS.RoomSessionID	=	HRStu.RoomSessionID
                                            INNER JOIN  FWK_User                    FU      ON  HSO.StudentID       =   FU.ID
                WHERE
                    FU.HostID       =   {0}
                    AND
                    HR.[Capacity]   >   0
                GROUP BY
					HB.BuildingCode, HRS.RoomSessionID, HR.RoomNumber, HR.[Floor], HR.Wing, HR.Capacity
                ORDER BY
		            HR.RoomNumber
            ", PortalUser.Current.HostID);

            //Params: [0] = BuildingID, [1] = Gender
            string standardRoomSQL = @"
                SELECT
                    HB.BuildingCode, HRS.RoomSessionID, HR.RoomNumber, HR.[Floor], HR.Wing, HR.Capacity, COUNT(HRStu.RoomStudentID) AS Occupancy
                FROM
                    CUS_Housing_Room	HR	INNER JOIN  CUS_Housing_Building	HB		ON  HR.BuildingID		=   HB.BuildingID
								            INNER JOIN	CUS_Housing_RoomSession	HRS		ON	HR.RoomID			=	HRS.RoomID
																						AND	HRS.HousingYear		=	YEAR(GETDATE())
											LEFT JOIN	CUS_Housing_RoomStudent	HRStu	ON	HRS.RoomSessionID	=	HRStu.RoomSessionID
                WHERE
                    HR.BuildingID	=   ?
		            AND
		            HRS.Gender		IN	(?,'')
                    AND
                    HR.Capacity     >   0
                GROUP BY
					HB.BuildingCode, HRS.RoomSessionID, HR.RoomNumber, HR.[Floor], HR.Wing, HR.Capacity
                ORDER BY
		            HR.RoomNumber";

            //Params: [0] = BuildingID, [1] = Gender
            string oaksRoomSQL = @"
                SELECT
	                B.BuildingCode, SubRoom.RoomNumberOnly, SubRoom.[Floor], SubRoom.Wing, SUM(SubRoom.Capacity) AS Capacity, COUNT(HRS.RoomStudentID) AS Occupancy,
	                CASE
		                WHEN	SubRoom.Capacity = 1	THEN	1
										                ELSE	0
	                END AS IsSuite,
	                (
		                SELECT TOP 1
			                HRSsub.RoomSessionID
		                FROM
			                CUS_Housing_Room	Rsub	INNER JOIN	CUS_Housing_RoomSession	HRSsub	ON	Rsub.RoomID			=	HRSsub.RoomID
																					                AND	HRSsub.HousingYear	=	YEAR(GETDATE())
		                WHERE
			                Rsub.BuildingID						=	B.BuildingID
			                AND
			                SUBSTRING(Rsub.RoomNumber, 1, 3)	=	SubRoom.RoomNumberOnly
		                ORDER BY
			                Rsub.RoomNumber
	                )	AS	RoomID1,
	                (
		                SELECT TOP 1
			                HRSsub.RoomSessionID
		                FROM
			                CUS_Housing_Room	Rsub	INNER JOIN	CUS_Housing_RoomSession	HRSsub	ON	Rsub.RoomID	=	HRSsub.RoomID
																					                AND	HRSsub.HousingYear	=	YEAR(GETDATE())
		                WHERE
			                Rsub.BuildingID	=	B.BuildingID
			                AND
			                SUBSTRING(Rsub.RoomNumber, 1, 3)	=	SubRoom.RoomNumberOnly
		                ORDER BY
			                Rsub.RoomNumber	DESC
	                )	AS	RoomID2
                FROM
	                CUS_Housing_Building	B	INNER JOIN	(
												                SELECT
													                HR.BuildingID, HR.RoomNumber, SUBSTRING(HR.RoomNumber, 1, 3) AS RoomNumberOnly, HR.[Floor], HR.Wing, HR.Capacity, HRS.RoomSessionID, HRS.Gender
												                FROM
													                CUS_Housing_Room	HR	INNER JOIN	CUS_Housing_RoomSession	HRS	ON	HR.RoomID		=	HRS.RoomID
																													                AND	HRS.HousingYear	=	YEAR(GETDATE())
												                WHERE
													                HR.BuildingID	=	?
											                )						SubRoom	ON	B.BuildingID			=	SubRoom.BuildingID
								                LEFT JOIN	CUS_Housing_RoomStudent	HRS		ON	SubRoom.RoomSessionID	=	HRS.RoomSessionID
                WHERE
	                SubRoom.Gender		IN	(?,'')
                    AND
                    SubRoom.Capacity    >   0
                GROUP BY
	                B.BuildingID, B.BuildingCode, SubRoom.RoomNumberOnly, SubRoom.[Floor], SubRoom.Wing, Capacity
                ORDER BY
	                Subroom.RoomNumberOnly";

            ex = null;
            DataTable dtRooms = null;

            int dayIndex = int.Parse(this.ParentPortlet.PortletViewState["DayIndex"].ToString());
            string roomSQL = dayIndex == 0 ? greekSquatterSQL : (IsOaks ? oaksRoomSQL : standardRoomSQL);

            List<OdbcParameter> roomParameters = new List<OdbcParameter>();
            if (dayIndex == 0)
            {
                roomParameters.Add(new OdbcParameter("GreekInvl", this.ParentPortlet.PortletViewState["GreekID"].ToString()));
                roomParameters.Add(new OdbcParameter("Gender", this.ParentPortlet.PortletViewState["Gender"].ToString()));
            }
            else if(dayIndex > 0 && dayIndex < 4)
            {
                roomParameters.Add(new OdbcParameter("RoomBuildingID", buildingID));
                roomParameters.Add(new OdbcParameter("Gender", this.ParentPortlet.PortletViewState["Gender"].ToString()));
            }

            //Reusing the same OdbcParameter list ("parameters" from the query above) throws an exception so recreate the query parameters in a separate list.
            //List<OdbcParameter> roomParameters = new List<OdbcParameter>
            //{
            //    new OdbcParameter("roomBuildingID", buildingID)
            //    , new OdbcParameter("gender", this.ParentPortlet.PortletViewState["Gender"].ToString())
            //};

            try
            {
                dtRooms = jicsConn.ConnectToERP(roomSQL, ref ex, roomParameters);
                if (ex != null) { throw ex; }

                //Complete placeholder that identifies how many rooms matched the search criteria (building and gender)
                this.ltlBuildingName.Text += String.Format(" ({0} room{1} found)", dtRooms.Rows.Count.ToString(), (dtRooms.Rows.Count == 1 ? "" :"s"));

                if (dtRooms != null)
                {
                    this.rptRoomList.ItemDataBound += rptRoomList_ItemDataBound;
                    this.rptRoomList.DataSource = dtRooms;
                    this.rptRoomList.DataBind();
                }
            }
            catch (Exception ee)
            {
                this.ParentPortlet.ShowFeedback(FeedbackType.Error, String.Format("{0}<br /><br />{1}", ee.Message, ee.InnerException.ToString()));
            }
            finally
            {
                if (jicsConn.IsNotClosed()) { jicsConn.Close(); }
            }
        }

        private void rptRoomList_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            //Make sure that the repeater element being affected is an item or alternating item (essentially a row of content)
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                //Get the current row of data being mapped to the controls
                DataRow row = (e.Item.DataItem as DataRowView).Row;

                PlaceHolder phBeds = (PlaceHolder)e.Item.FindControl("phSpots");
                Literal roomNumber = (Literal)e.Item.FindControl("ltlRoomNumber");

                if (IsOaks)
                {
                    bool isSuite = row["IsSuite"].ToString() == "1";
                    roomNumber.Text = String.Format("{0} {1} ({2}): ", row["BuildingCode"].ToString(), row["RoomNumberOnly"].ToString(), (isSuite ? "Suite" : "Double"));

                    //Add controls for Oaks beds, suites are differentiated by letters, double rooms by numbers
                    phBeds.Controls.Add(BuildBedControl(row["RoomID1"].ToString(), (isSuite ? "A" : "1")));
                    phBeds.Controls.Add(BuildBedControl(row["RoomID2"].ToString(), (isSuite ? "B" : "2")));
                }
                else
                {
                    //Find the existing control to display the room number and assign the appropriate text
                    roomNumber.Text = String.Format("{0} {1}: ", row["BuildingCode"].ToString(), row["RoomNumber"].ToString());
                    //Loop through the capacity of each room and create a bed for each spot
                    for (int ii = 1; ii <= int.Parse(row["Capacity"].ToString()); ii++)
                    {
                        phBeds.Controls.Add(BuildBedControl(row["RoomSessionID"].ToString(), ii.ToString()));
                    }
                }
            }
        }

        /// <summary>
        /// Store value of the clicked element and direct the user to the next screen (Accept terms and conditions to finish registering for a room).
        /// </summary>
        /// <param name="sender">Object representing the button that was clicked</param>
        /// <param name="e"></param>
        void chooseBed_Click(object sender, EventArgs e)
        {
            Button clicked = (sender as Button);
            this.ParentPortlet.PortletViewState["RoomSessionID"] = clicked.CommandArgument;
            this.ParentPortlet.NextScreen("AcceptRoom");
        }

        /// <summary>
        /// The user clicked the link to return to the screen where they may select a different building. In doing so, they void their previous building selection.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void lnkPickBuilding_Click(object sender, EventArgs e)
        {
            //Remove previously selected building
            this.ParentPortlet.PortletViewState["Building"] = null;
            //Return the user to the list of available buildings
            this.ParentPortlet.PreviousScreen("AvailabilityBuilding");
        }

        /// <summary>
        /// Based on the RoomSessionID and bed index, determine whether the space is already occupied for the current housing session.
        /// If so, display the name of the occupant. If not, provide a button the user may click to select the room.
        /// </summary>
        /// <param name="RoomSessionID">Unique identifier of the room for a specific session (primary key of CUS_Housing_RoomSession)</param>
        /// <param name="BedIndex">The numeric or alphabetical representation of a bed within the room</param>
        /// <returns></returns>
        public Control BuildBedControl(string RoomSessionID, string BedIndex)
        {
            string invitationSQL = @"
                SELECT
                    HRR.StudentID, FU.FirstName, FU.LastName
                FROM
                    CUS_Housing_RoomReservation HRR INNER JOIN  FWK_User    FU  ON  HRR.StudentID   =   FU.ID
                WHERE
                    HRR.RoomSessionID   =   ?
                ORDER BY
                    HRR.ReservationTime
            ";
            Exception exInvitation = null;
            DataTable dtInvitation = null;
            List<OdbcParameter> paramInvite = new List<OdbcParameter> { new OdbcParameter("RoomSessionID", RoomSessionID) };

            try
            {
                dtInvitation = jicsConn.ConnectToERP(invitationSQL, ref exInvitation, paramInvite);
                if (exInvitation != null) { throw exInvitation; }
            }
            catch (Exception ex)
            {
                this.ParentPortlet.ShowFeedback(FeedbackType.Error, String.Format("Error retrieving invitations:<br />{0}<br />{1}", ex.Message, ex.InnerException));
            }

            //Define SQL to get name details about the occupant
            string getOccupantSQL = @"
                SELECT
                    U.FirstName, U.LastName
                FROM
                    CUS_Housing_RoomStudent HRStu   INNER JOIN  FWK_User    U   ON  HRStu.StudentID =   U.ID
                WHERE
                    HRStu.RoomSessionID =   ?
            ";

            //Define parameter for query
            List<OdbcParameter> paramBed = new List<OdbcParameter> { new OdbcParameter("bedRoomSessionID", RoomSessionID) };

            //The object to be passed back (during initial phase this was either a Button or Literal object)
            Control returnObj = null;
            Exception exBed = null;
            DataTable dtBed = null;
            int bedNumber = -1;
            bool bedIsNumber = int.TryParse(BedIndex, out bedNumber);

            try
            {
                //Execute SQL
                dtBed = jicsConn.ConnectToERP(getOccupantSQL, ref exBed, paramBed);
                //If the executed SQL or the attempt to establish a database connection generates an exception, throw it to be handled by the "catch" below
                if (exBed != null) { throw exBed; }

                //If no occupant records are found, the bed is empty so create a button the user may select to sign up for the bed
                //If there are rows but the number of them is less than the current bed index, create the button
                if (dtBed != null && (dtBed.Rows.Count == 0 || dtBed.Rows.Count < bedNumber))
                {
                    Button btnBed = new Button();
                    btnBed.Click += chooseBed_Click;
                    btnBed.CommandArgument = RoomSessionID;
                    btnBed.CssClass = "bedOccupant";
                    btnBed.Text = String.Format("Bed {0}", BedIndex);

                    bedNumber = bedIsNumber ? bedNumber : (BedIndex == "A" ? 1 : 2);

                    //The label for the button is the name of the invitee or the bed number/letter.
                    if (dtInvitation != null && dtInvitation.Rows.Count >= bedNumber)
                    {
                        btnBed.CssClass += " bedReserved";
                        btnBed.Text = String.Format("{0} {1} invited", dtInvitation.Rows[bedNumber - 1]["FirstName"].ToString(), dtInvitation.Rows[bedNumber - 1]["LastName"].ToString());
                    }

                    returnObj = btnBed;
                }
                else
                {
                    DataRow drBed = dtBed.Rows[bedNumber];
                    Label lblBed = new Label();
                    lblBed.Text = String.Format("Bed {0}: {1} {2}", BedIndex, drBed["FirstName"].ToString(), drBed["LastName"].ToString());
                    lblBed.CssClass = "bedOccupant";
                    returnObj = lblBed;
                }
            }
            catch (Exception ee)
            {
                Literal ltlEx = new Literal();
                ltlEx.Text = String.Format("Error querying residents of room:<br />{0}<br /><br />{1}", ee.Message, ee.InnerException);
                returnObj = ltlEx;
            }
            return returnObj;
        }
    }
}