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
        OdbcConnectionClass3 jicsConn = new OdbcConnectionClass3("JICSDataConnection.config");
        public static string COMMAND_NAME_ROOM = "PickRoom";
        public bool IsOaks = false;

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

            string standardRoomSQL = @"
                SELECT
                    BuildingCode, RoomID, RoomNumber, [Floor], Wing, Capacity
                FROM
                    CUS_Housing_Room    INNER JOIN  CUS_Housing_Building    ON  CUS_Housing_Room.BuildingID =   CUS_Housing_Building.BuildingID
                WHERE
                    CUS_Housing_Room.BuildingID =   ?
                ORDER BY RoomNumber";

            string oaksRoomSQL = @"
                SELECT
	                B.BuildingCode, SUBSTRING(R.RoomNumber, 1, 3) AS RoomNumberOnly, R.[Floor], R.Wing, SUM(R.Capacity) AS Capacity,
	                CASE
		                WHEN	R.Capacity = 1	THEN	1
								                ELSE	0
	                END AS isSuite,
	                (SELECT TOP 1 Rsub.RoomID FROM CUS_Housing_Room Rsub WHERE Rsub.BuildingID = ? AND SUBSTRING(Rsub.RoomNumber, 1, 3) = SUBSTRING(R.RoomNumber, 1, 3) ORDER BY Rsub.RoomNumber) AS RoomID1,
	                CASE	R.Capacity
		                WHEN	1	THEN	(
								                SELECT
									                Rsub.RoomID
								                FROM (
									                SELECT Rsub2.RoomID, ROW_NUMBER() OVER (ORDER BY Rsub2.RoomNumber) AS RowNum
									                FROM CUS_Housing_Room Rsub2
									                WHERE Rsub2.BuildingID = ? AND SUBSTRING(Rsub2.RoomNumber, 1, 3) = SUBSTRING(R.RoomNumber, 1, 3)
								                ) AS Rsub
								                WHERE Rsub.RowNum = 2
					                )
					                ELSE	(
						                SELECT TOP 1 Rsub.RoomID FROM CUS_Housing_Room Rsub WHERE Rsub.BuildingID = ? AND SUBSTRING(Rsub.RoomNumber, 1, 3) = SUBSTRING(R.RoomNumber, 1, 3) ORDER BY Rsub.RoomNumber
					                )
	                END AS RoomID2
                FROM
	                CUS_Housing_Building	B	INNER JOIN	CUS_Housing_Room	R	ON	B.BuildingID	=	R.BuildingID
                WHERE
	                R.BuildingID	=	?
                GROUP BY
	                B.BuildingCode, SUBSTRING(R.RoomNumber, 1, 3), R.[Floor], R.Wing, Capacity
                ORDER BY
	                RoomNumberOnly
            ";
            ex = null;
            DataTable dtRooms = null;

            string roomSQL = standardRoomSQL;

            //Reusing the same OdbcParameter list ("parameters" from the query above) throws an exception so recreate the query parameters in a separate list.
            List<OdbcParameter> roomParameters = new List<OdbcParameter>
            {
                new OdbcParameter("roomBuildingID", buildingID)
            };
            if (IsOaks)
            {
                roomSQL = oaksRoomSQL;
                roomParameters.Add(new OdbcParameter("roomBuildingID2", buildingID));
                roomParameters.Add(new OdbcParameter("roomBuildingID3", buildingID));
                roomParameters.Add(new OdbcParameter("roomBuildingID4", buildingID));
            }

            try
            {
                dtRooms = jicsConn.ConnectToERP(roomSQL, ref ex, roomParameters);
                if (ex != null) { throw ex; }

                this.ltlBuildingName.Text += String.Format(" ({0} room{1} found)", dtRooms.Rows.Count.ToString(), (dtRooms.Rows.Count == 1 ? "" :"s"));
            }
            catch (Exception ee)
            {
                this.ParentPortlet.ShowFeedback(FeedbackType.Error, String.Format("{0}<br /><br />{1}", ee.Message, ee.InnerException.ToString()));
            }
            finally
            {
                if (jicsConn.IsNotClosed()) { jicsConn.Close(); }
            }

            if (dtRooms != null)
            {
                this.rptRoomList.ItemDataBound += rptRoomList_ItemDataBound;
                this.rptRoomList.DataSource = dtRooms;
                this.rptRoomList.DataBind();
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
                    Button bedFirst = new Button();
                    bedFirst.Click += chooseBed_Click;
                    bedFirst.CommandArgument = row["RoomID1"].ToString();
                    bedFirst.Text = String.Format("Bed {0}", (isSuite ? "A" : "1"));
                    phBeds.Controls.Add(bedFirst);

                    Button bedSecond = new Button();
                    bedSecond.Click += chooseBed_Click;
                    bedSecond.CommandArgument = row["RoomID2"].ToString();
                    bedSecond.Text = String.Format("Bed {0}", (isSuite ? "B" : "2"));
                    phBeds.Controls.Add(bedSecond);
                }
                else
                {
                    //Find the existing control to display the room number and assign the appropriate text
                    roomNumber.Text = String.Format("{0} {1}: ", row["BuildingCode"].ToString(), row["RoomNumber"].ToString());
                    for (int ii = 1; ii <= int.Parse(row["Capacity"].ToString()); ii++)
                    {
                        Button chooseBed = new Button();
                        chooseBed.Click += chooseBed_Click;
                        chooseBed.CommandArgument = row["RoomID"].ToString();
                        chooseBed.Text = String.Format("Bed {0}", ii.ToString());
                        phBeds.Controls.Add(chooseBed);
                    }
                }
            }
        }

        void chooseBed_Click(object sender, EventArgs e)
        {
            Button clicked = (sender as Button);
            this.ParentPortlet.PortletViewState["RoomID"] = clicked.CommandArgument;
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

        public class Room
        {
            private string roomID;
            private string roomNumber;
            private string floor;
            private string wing;
            private int capacity;

            public Room(string RoomID, string RoomNumber, string Floor, string Wing, int Capacity)
            {
                this.roomID = RoomID;
                this.roomNumber = RoomNumber;
                this.floor = Floor;
                this.wing = Wing;
                this.capacity = Capacity;
            }

            public string RoomID { get { return this.roomID; } }
            public string RoomNumber { get { return this.roomNumber; } }
            public string Floor { get { return this.floor; } }
            public string Wing { get { return this.wing; } }
            public int Capacity { get { return this.capacity; } }
        }

        protected void rptRoomList_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == COMMAND_NAME_ROOM)
            {
                this.ParentPortlet.ShowFeedback(FeedbackType.Message, String.Format("{0} : {1}", e.CommandArgument, e.CommandName));
                this.ParentPortlet.PortletViewState["RoomID"] = e.CommandArgument;
                this.ParentPortlet.NextScreen("AcceptRoom");
            }
        }
    }
}