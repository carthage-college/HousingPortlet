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
        public bool IsOaks = false;
        public bool IsTower = false;
        public bool IsTodayGreekSquatter = false;
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
            string debugUpdate = "<ul>";
            try
            {
                //Check to see if a building is stored in the viewstate. If not, the user is redirected to the building selection page before being allowed to proceed.
                if (this.ParentPortlet.PortletViewState["Building"] == null || this.ParentPortlet.PortletViewState["Building"].ToString().Length == 0)
                {
                    this.ParentPortlet.PortletViewState["Message"] = "Please make sure to select a building before proceeding through the housing sign-up.";
                    this.ParentPortlet.PreviousScreen("AvailabilityBuilding");
                }
                string buildingID = this.ParentPortlet.PortletViewState["Building"].ToString();

                debugUpdate = String.Format("{0}<li>Parsed buildingID from viewstate {1}</li>", debugUpdate, buildingID);

                //Find the name of the building selected by the user on the previous page
                string buildingSQL = "EXECUTE [dbo].[CUS_spHousing_getBuildingByID] @guidBuildingID = ?";
                //Initialize variables that will process results from the query
                Exception ex = null;
                DataTable dtBuilding = null;
                List<OdbcParameter> parameters = new List<OdbcParameter>
                {
                    //Get the buildingID variable from the viewstate
                    new OdbcParameter("buildingID", buildingID)
                };

                debugUpdate = String.Format("{0}<li>Initialized variables for query to retrieve building</li>", debugUpdate);

                try
                {
                    dtBuilding = jicsConn.ConnectToERP(buildingSQL, ref ex, parameters);
                    if (ex != null) { throw ex; }

                    debugUpdate = String.Format("{0}<li>Query to retrive building completed successfully</li>", debugUpdate);

                    debugUpdate = String.Format("{0}<li>dtBuilding is not null: {1}</li>", debugUpdate, (dtBuilding != null).ToString());

                    //If the query returned a row, assign the building's name to the appropriate textfield
                    if (dtBuilding != null && dtBuilding.Rows.Count == 1)
                    {
                        string buildingName = dtBuilding.Rows[0]["BuildingName"].ToString();

                        this.ltlBuildingName.Text = buildingName;
                        IsOaks = buildingName.StartsWith("Oaks");
                        debugUpdate = String.Format("{0}<li>Is Oaks: {1}</li>", debugUpdate, IsOaks.ToString());

                        //If the user is looking at rooms in the Oaks, display language differentiating Suites from Doubles
                        this.panelOaksDescription.Visible = IsOaks;
                        debugUpdate = String.Format("{0}<li>Updated Oaks panel visibility</li>", debugUpdate);

                        IsTower = buildingName.StartsWith("Residential");
                        debugUpdate = String.Format("{0}<li>Is Tower: {1}</li>", debugUpdate, IsTower.ToString());
                    }
                    else
                    {
                        this.ParentPortlet.ShowFeedback(FeedbackType.Error, "We were unable to find the details of the building you selected. Please pick a different building and try again.");
                    }
                    debugUpdate = String.Format("{0}<li>Successfully retrieved building name</li>", debugUpdate);
                }
                catch (Exception ee)
                {
                    this.ParentPortlet.ShowFeedback(FeedbackType.Error, String.Format("{0}<br /><br />{1}", ee.Message, ee.InnerException.ToString()));
                }

                //Params: [0] = BuildingID, [1] = Gender
                string raRoomSQL = "EXECUTE [dbo].[CUS_spHousing_getRoomsStandard] @guidBuildingID = ?, @strGender = ?, @bitIsRA = 1";

                // Params: [0] = GreekID (invl_table.invl), [1] = Gender, [2] = StudentID
                string greekSquatterSQL = "EXECUTE [dbo].[CUS_spHousing_getRoomsGreekSquatter] @strGreekID = ?, @strGender = ?, @guidStudentID = ?, @guidBuildingID = ?";

                //Params: [0] = BuildingID, [1] = Gender
                string standardRoomSQL = "EXECUTE [dbo].[CUS_spHousing_getRoomsStandard] @guidBuildingID = ?, @strGender = ?";

                //Params: [0] = BuildingID, [1] = Gender
                string oaksRoomSQL = "EXECUTE [dbo].[CUS_spHousing_getRoomsOaks] @guidBuildingID = ?, @strGender = ?";

                //Params: [0] = BuildingID, [1] = Gender
                string oaksRARoomSQL = "EXECUTE [dbo].[CUS_spHousing_getRoomsOaks] @guidBuildingID = ?, @strGender = ?, @bitIsRA = 1";

                //Params: [0] = Gender
                string towerRARoomSQL = "EXECUTE [dbo].[CUS_spHousing_getRoomsTower] @strGender = ?, @bitIsRA = 1";

                //Params: [0] = Gender
                string towerRoomSQL = "EXECUTE [dbo].[CUS_spHousing_getRoomsTower] @strGender = ?";

                ex = null;
                DataTable dtRooms = null;

                bool isTodayRA = bool.Parse(this.ParentPortlet.PortletViewState["IsTodayRA"].ToString());
                int dayIndex = int.Parse(this.ParentPortlet.PortletViewState["DayIndex"].ToString());
                IsTodayGreekSquatter = dayIndex == 0;
                //string roomSQL = dayIndex == 0 ? greekSquatterSQL : (IsOaks ? oaksRoomSQL : standardRoomSQL);

                OdbcParameter paramGreekID = new OdbcParameter("GreekInvl", this.ParentPortlet.PortletViewState["GreekID"].ToString());
                OdbcParameter paramGender = new OdbcParameter("Gender", this.ParentPortlet.PortletViewState["Gender"].ToString());
                OdbcParameter paramStudentID = new OdbcParameter("StudentID", PortalUser.Current.Guid);
                OdbcParameter paramBuildingID = new OdbcParameter("BuildingID", buildingID);

                debugUpdate = String.Format("{0}<li>Initialized OdbcParameters</li>", debugUpdate);


                List<OdbcParameter> roomParameters = new List<OdbcParameter>();
                string roomSQL = "";
                //If it is greek/squatter signup day
                if (IsTodayGreekSquatter)
                {
                    roomSQL = greekSquatterSQL;
                    //Pass parameters for greek organization and student/room gender
                    roomParameters.Add(paramGreekID);
                    roomParameters.Add(paramGender);
                    roomParameters.Add(paramStudentID);
                    roomParameters.Add(paramBuildingID);
                }
                //If today is RA signup or a valid general student signup day
                else
                {
                    if (IsOaks)
                    {
                        roomSQL = oaksRoomSQL;
                        roomParameters.Add(paramBuildingID);
                        roomParameters.Add(paramGender);
                    }
                    else if (IsTower)
                    {
                        roomSQL = towerRoomSQL;
                        roomParameters.Add(paramGender);
                    }
                    else
                    {
                        roomSQL = standardRoomSQL;
                        roomParameters.Add(paramBuildingID);
                        roomParameters.Add(paramGender);
                    }
                }

                debugUpdate = String.Format("{0}<li>Defined SQL and built parameterized lists", debugUpdate);

                ////If it is greek/squatter signup day
                //if(IsTodayGreekSquatter)
                //{
                //    //Pass parameters for greek organization and student/room gender
                //    //roomParameters.Add(new OdbcParameter("GreekInvl", this.ParentPortlet.PortletViewState["GreekID"].ToString()));
                //    //roomParameters.Add(new OdbcParameter("Gender", this.ParentPortlet.PortletViewState["Gender"].ToString()));
                //    //roomParameters.Add(new OdbcParameter("StudentID", PortalUser.Current.Guid));
                //    //roomParameters.Add(new OdbcParameter("BuildingID", buildingID));
                //}
                ////If today is RA signup or a valid general student signup day
                //else if (isTodayRA || (dayIndex > 0 && dayIndex < 4))
                //{
                //    if (isTodayRA) { roomSQL = IsOaks ? oaksRARoomSQL : raRoomSQL; }
                //    //roomParameters.Add(new OdbcParameter("RoomBuildingID", buildingID));
                //    //roomParameters.Add(new OdbcParameter("Gender", this.ParentPortlet.PortletViewState["Gender"].ToString()));
                //}

                try
                {
                    dtRooms = jicsConn.ConnectToERP(roomSQL, ref ex, roomParameters);
                    if (ex != null) { throw ex; }

                    //Complete placeholder that identifies how many rooms matched the search criteria (building and gender)
                    this.ltlBuildingName.Text += String.Format(" ({0} room{1} found)", dtRooms != null ? dtRooms.Rows.Count.ToString() : "0", (dtRooms != null && dtRooms.Rows.Count == 1 ? "" : "s"));

                    if (dtRooms != null)
                    {
                        this.rptRoomList.ItemDataBound += rptRoomList_ItemDataBound;
                        this.rptRoomList.DataSource = dtRooms;
                        this.rptRoomList.DataBind();
                    }
                }
                catch (Exception ee)
                {
                    this.ParentPortlet.ShowFeedback(FeedbackType.Error,
                        String.Format("<p>An error occurred while processing room search results.</p><p>{0}</p><p>{1}, {2}</p><pre>{3}</pre>", ee.Message, buildingID, this.ParentPortlet.PortletViewState["Gender"].ToString(), ee.InnerException.ToString()));
                }
                finally
                {
                    if (jicsConn.IsNotClosed()) { jicsConn.Close(); }
                }
            }
            catch (Exception exPrimaryFailure)
            {
                this.ParentPortlet.ShowFeedback(FeedbackType.Error, String.Format("<p>An error occurred in the init event of room selection. {0}</p><p>{1}</p><pre>{2}</pre>{3}</ul>", exPrimaryFailure.Message, exPrimaryFailure.InnerException, exPrimaryFailure.StackTrace, debugUpdate));
            }
        }

        private void rptRoomList_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            string bedDebug = "";
            //Make sure that the repeater element being affected is an item or alternating item (essentially a row of content)
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                try
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
                        //string rsID = isSuite ? row["RoomID1"].ToString() : row["RoomSessionID"].ToString();
                        string rsID = row["RoomID1"].ToString();
                        Control bedControl1 = BuildBedControl(rsID, (isSuite ? "A" : "1"));
                        if (bedControl1 != null)
                        {
                            phBeds.Controls.Add(bedControl1);
                        }

                        //rsID = isSuite ? row["RoomID2"].ToString() : row["RoomSessionID"].ToString();
                        string rsID2 = row["RoomID2"].ToString();
                        Control bedControl2 = BuildBedControl(rsID2, (isSuite ? "B" : "2"));
                        if (bedControl2 != null)
                        {
                            phBeds.Controls.Add(bedControl2);
                        }
                    }
                    else if (IsTower)
                    {
                        roomNumber.Text = String.Format("{0} {1}: ", row["BuildingCode"].ToString(), row["RoomNumberOnly"].ToString());

                        bedDebug = String.Format("{0}<p>Generating beds for {1}</p><ul>", bedDebug, roomNumber.Text);

                        string suiteA_ID = row["RoomID1"].ToString();
                        for (int ii = 1; ii <= int.Parse(row["SuiteA_Capacity"].ToString()); ii++)
                        {
                            bedDebug = String.Format("{0}<li>BuildBedControl({1}, {2}, 'Suite A')</li>", bedDebug, suiteA_ID, ii.ToString());
                            phBeds.Controls.Add(BuildBedControl(suiteA_ID, ii.ToString(), "Suite A"));
                        }
                        string suiteB_ID = row["RoomID2"].ToString();
                        for (int jj = 1; jj <= int.Parse(row["SuiteB_Capacity"].ToString()); jj++)
                        {
                            bedDebug = String.Format("{0}<li>BuildBedControl({1}, {2}, 'Suite B')</li>", bedDebug, suiteB_ID, jj.ToString());
                            phBeds.Controls.Add(BuildBedControl(suiteB_ID, jj.ToString(), "Suite B"));
                        }

                        bedDebug = String.Format("{0}</ul>", bedDebug);
                        //this.ParentPortlet.ShowFeedback(FeedbackType.Message, bedDebug);
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
                catch (Exception ex)
                {
                    this.ParentPortlet.ShowFeedback(FeedbackType.Error, String.Format("{0}<br />{1}", ex.Message, ex.InnerException));
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
        /// <param name="customLabel">Custom label for displaying suites in the Residential Tower</param>
        /// <returns></returns>
        public Control BuildBedControl(string RoomSessionID, string BedIndex, string customLabel = "")
        {
            //The object to be passed back (during initial development this was either a Button or Literal object)
            Control returnObj = null;

            if (RoomSessionID != "")
            {
                string invitationSQL = "EXECUTE [dbo].[CUS_spHousing_getInvitationBedDisplay] @guidRoomSessionID = ?";
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
                string getOccupantSQL = "EXECUTE [dbo].[CUS_spHousing_getStudentNamesByRoom] @guidRoomSessionID = ?";

                //Define parameter for query
                List<OdbcParameter> paramBed = new List<OdbcParameter> { new OdbcParameter("bedRoomSessionID", RoomSessionID) };

                Exception exBed = null;
                DataTable dtBed = null;
                int bedNumber = -1;
                bool bedIsNumber = int.TryParse(BedIndex, out bedNumber);
                //Make adjustments to bed number when dealing with Oaks' suites
                bedNumber = bedIsNumber ? bedNumber : (BedIndex == "A" ? 1 : 2);

                try
                {
                    //Execute SQL
                    dtBed = jicsConn.ConnectToERP(getOccupantSQL, ref exBed, paramBed);
                    //If the executed SQL or the attempt to establish a database connection generates an exception, throw it to be handled by the "catch" below
                    if (exBed != null) { throw exBed; }

                    //If no occupant records are found, the bed is empty so create a button the user may select to sign up for the bed
                    //If there are rows but the number of them is less than the current bed index, create the button
                    if (dtBed != null && (dtBed.Rows.Count == 0 || (bedIsNumber && dtBed.Rows.Count < bedNumber)))
                    {
                        Button btnBed = new Button();
                        btnBed.Click += chooseBed_Click;
                        btnBed.CommandArgument = RoomSessionID;
                        btnBed.CssClass = "bedOccupant";
                        btnBed.Text = String.Format("{0} Bed {1}", customLabel, BedIndex);

                        //The label for the button is the name of the invitee or the bed number/letter.
                        //if (dtInvitation != null && (dtInvitation.Rows.Count >= bedNumber || (!bedIsNumber && dtInvitation.Rows.Count > 0)))
                        int adjustedBedIndex = bedIsNumber ? bedNumber - (1 + dtBed.Rows.Count) : 0;
                        if (dtInvitation != null && dtInvitation.Rows.Count > adjustedBedIndex)
                        {
                            btnBed.CssClass += " bedReserved";
                            //If a bed is in a suite (bed is not a number) there will only be one record in the invitation DataTable.
                            //Otherwise the bed index needs to be decreased by (1 + room occupants) to get the correct position.
                            btnBed.Text = String.Format("{0} {1} invited", dtInvitation.Rows[adjustedBedIndex]["FirstName"].ToString(), dtInvitation.Rows[adjustedBedIndex]["LastName"].ToString());
                        }
                        returnObj = btnBed;
                    }
                    else
                    {
                        //If the bed is not in a suite, take the corresponding (0-index-based) row from the recordset. If the bed is a suite, grab the first (0) row
                        DataRow drBed = dtBed.Rows[(bedIsNumber ? bedNumber - 1 : 0)];
                        Label lblBed = new Label();
                        lblBed.Text = String.Format("{0} Bed {1}: {2} {3}", customLabel, BedIndex, drBed["FirstName"].ToString(), drBed["LastName"].ToString());
                        //lblBed.Text = String.Format("{0} Bed {1}: {2} {3}", customLabel, BedIndex, "Student", "Redacted");
                        lblBed.CssClass = "bedOccupant";
                        returnObj = lblBed;
                    }
                }
                catch (Exception ex)
                {
                    Literal ltlEx = new Literal();
                    ltlEx.Text = String.Format("Error querying residents of room:<br />{0}<br /><br />{1}<br ><br />{2}", ex.Message, ex.StackTrace, RoomSessionID);
                    returnObj = ltlEx;
                }
            }

            return returnObj;
        }
    }
}