<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Accept_Room_View.ascx.cs" Inherits="Housing.Accept_Room_View" %>
<%@ Register Assembly="Jenzabar.Common" Namespace="Jenzabar.Common.Web.UI.Controls" TagPrefix="common" %>

<script type="text/javascript">
    $(function () {
        $('#<%= chkAgree.ClientID %>').click(function () {
            $('#<%= btnSubmit.ClientID %>').prop('disabled', $(this).prop('checked') ? '' : 'disabled');
        });
        $('#<%= btnSubmit.ClientID %>').prop('disabled', $('#<%= chkAgree.ClientID %>').prop('checked') ? '' : 'disabled');
    });
</script>

<div class="pSection"><common:ErrorDisplay ID="errMsg" runat="server" /></div>

<div class="pSection">
    <p>You have selected <asp:Literal ID="ltlNewBuildingRoom" runat="server" Text="[FIX THIS]" />.</p>
    <p>If this is correct, please read and accept the conditions below. If not, <asp:LinkButton ID="lnkPickRoom" runat="server" Text="return to the room selection screen" OnClick="lnkPickRoom_Click" /> and pick your desired room.</p>
    <asp:BulletedList ID="bulletedRoommates" runat="server">
    </asp:BulletedList>
</div>

<div class="pSection">
    <p><asp:CheckBox ID="chkAgree" runat="server" Text="I agree to the terms and conditions listed below." /></p>
    <%---
    <common:ContentBox ID="contentWaitlist" runat="server">
        <p>You have chosen not to live in the Oaks. Would you like to be placed on a waiting list to live in the Oaks should a space become available?
            Please realize that there is an extra charge associated with living in this premium housing.</p>
        <p><asp:CheckBox ID="chkOaksWaitlist" runat="server" Text="Please put me on a waiting list for the Oaks." /></p>
    </common:ContentBox>
    ---%>
    <p><asp:Button ID="btnSubmit" runat="server" Text="Sign up for [FIX THIS]" OnClick="btnSubmit_Click" /></p>
</div>

<!--- Contract --->
<pre>##########
    
CARTHAGE COLLEGE
HOUSING CONTRACT FOR RESIDENCE HALLS
ACADEMIC YEAR <asp:Literal ID="ltlContractYear" runat="server" Text="[FIX THIS]" />

RETURNING STUDENTS
Carthage agrees to provide assigned space in the residence halls for the undersigned student for
the <asp:Literal ID="ltlContractYear2" runat="server" Text="[FIX THIS]" /> academic year and the undersigned student agrees to pay for said assigned living
space on the following terms:

DURATION
This contract shall be for three (3) consecutive terms of the academic year including 
The Term I, J-Term, and Term II The student shall be entitled to reside in the halls at the
designated time the day prior to classes and ending immediately after the final exam each term,
unless given permission otherwise. The only exception to this is J-Term. Students must be a full-
time Carthage student in order to be eligible for on-campus housing. If a student reduces to part-
time status during the academic year, they are still obligated to fulfill their housing contract for
the full academic year. The campus does not close between the end of J-Term and the beginning
of Term II. The campus will be closed for the following breaks:

    Thanksgiving November 26th after 5:00 p.m. through December 1st, <asp:Literal ID="ltlThisYear1" runat="server" /> at noon
    Christmas December 13th, <asp:Literal ID="ltlThisYear2" runat="server" /> after 5:00 p.m. through January 7th, <asp:Literal ID="ltlNextYear1" runat="server" /> at noon
    Spring Break March 6th, after 5:00 p.m. through March 15th, <asp:Literal ID="ltlNextYear2" runat="server" /> at noon

The student shall remove all personal possessions from their assigned space upon termination of
residence for any reason. Carthage does not store personal possessions for students. All such
personal possessions not removed will be disposed of by Carthage and a cleaning fee assessed to
the student.

TERMS
A student must be registered full time to reside in the residence halls during any semester. This
equates to a minimum of twelve (12) credits each term, and a minimum of four (4) credits during
J-Term, to be eligible to live in the residence halls. Exception to the above may be made by the
Office of Residential Life.

CHARGES
The per term charges for rooms will be determined by the Student Accounts Office at Carthage
College at a later date. Carthage reserves the right to change the amount of charge at any time.
All students living on campus are required to take a meal plan.

COMMUNITY CHARGES
If damages or vandalism occur and no individual(s) is directly identified as being responsible,
the cost of repairs will be equally charged to the members of that wing, floor, building or group
of people most closely related to the damages. These charges will be placed on the student’s
monthly statement from Carthage.

ADJUSTMENTS
No refund of room charges shall be made when a student withdraws, is dismissed from Carthage,
or is removed from housing for disciplinary reasons.

UNASSIGNED LIVING SPACE
The contract does not constitute a guarantee for a specific room or roommate. Carthage reserves
the right to make any changes in room or roommate assignments at any time.

EFFECTIVE DATE
This contract becomes binding when the student completes the on-line housing selection process
or when students are administratively assigned to housing if not eligible for commuter status or a
residency exemption. It continues for the entire academic year as long as the student remains in
good standing with Carthage.

NO SHOWS
Any student who does not report to their assigned living space 24 hours after the start of classes
will automatically lose their assigned space.

RULES AND REGULATIONS
Carthage reserves the right for its authorized personnel to enter student rooms to preserve and
protect Carthage property and to assure compliance with state and local laws or Carthage rules
and regulations. In signing this contract, the student agrees to adhere to all rules and regulations
governing their behavior as outlined in the Carthage Student Community Code.

PAYMENTS
All payments hereunder shall be made to the Student Accounts Office.
###############################################################################
</pre>

<asp:Panel ID="panelApartmentContract" runat="server">
    <!--- Apartment --->
<pre>Dear Apartment resident,

You have been assigned to the On-Campus Apartments at Carthage College for the <asp:Literal ID="ltlApartmentYear" runat="server" Text="[FIX THIS]" />
academic year. The street address for the apartments is: 1625 Sheridan Road, in the town of
Somers, WI. This letter details important information as you prepare for your return. Please
browse the information below.

Utilities -  All utilities are included in the cost.

Cable service - Cable service is not provided.  A possible service provider is
listed below:

Time-Warner Customer Service: 800-933-4662

Internet service - Internet service is not included. Some possible suggestions
for internet providers are listed below.

ASAP virtual networks: 888-700-8710     Ameritech: 800-638-8775
Broadwing: 800-276-2394                 Wisconsin Internet: 888-782-1454

Parking - The apartment parking lot has limited parking available for students. Every parking lot
is first come, first served. Students who have a vehicle on campus must obtain a valid parking
permit for a Carthage parking lot. Apartment dwellers cannot park on the lawn, the entrance
drive or the street. Contact Campus Security (262-551- 5911) for more information about
parking.

Laundry facilities - Laundry machines are located on the basement levels of the building.
Laundry can be paid for with quarters or by debit card using the provided card machine.

Mail Delivery - You will continue to receive your mail in the Todd Wehr Center (TWC) via your
campus mailbox.

Break Periods - All campus housing facilities, including the residence halls and on-campus
apartments close during some break periods. Students who would like to stay during closed
break periods must qualify and apply for &quot;extended stay.&quot; Information about the &quot;extended stay&quot;
process is organized, managed, and promoted by the Office of Residential Life.

Periodic Facilities Inspections - The Office of Residential Life reserves the right to conduct
facilities inspections throughout the year, including prior to checkout. For maintenance concerns,
please file a work order or contact the Maintenance Department at <a href="mailto:maintenance@carthage.edu">maintenance@carthage.edu</a>

Room Condition Reports - All residential students will be responsible for completing a room
condition report when moving into their room. More information will be available at check-in.

You are responsible for being familiar with and abiding by the Carthage College
Community Code (<a href="http://www.carthage.edu/campus-life/code/">http://www.carthage.edu/campus-life/code/</a>). All information in the Community Code
regarding room decoration(s) applies to the on-campus apartments. No structural changes may be
made to the building. In addition, nails should not be used to attach fixtures. Questions regarding
policies should be directed to the Dean of Students office.

Sincerely,
<asp:Literal ID="ltlHousingAdminName" runat="server" />
Director of Residential Life
###############################################################################</pre>

    <!--- Parking --->
<pre>ATTENTION APARTMENT AND HOUSE RESIDENTS:

Please review the following parking information regarding the Parkway Apartments
for the <asp:Literal ID="ltlParkingYear" runat="server" Text="[FIX THIS]" /> Academic Year.

The campus apartment complex (1625 Sheridan Road) is part of the parking
lottery for the <asp:Literal ID="ltlParkingYear2" runat="server" Text="[FIX THIS]" /> academic year. The 13 space apartment lot will be
awarded via the lottery. Other apartment dwellers may purchase Lot V permits if
available. You must be a resident of the apartment complex to apply for this lot.

Permits for parking in the drives of the two 17th Street campus homes (729
and 815 17th Street) will be required starting with the <asp:Literal ID="ltlParkingYear3" runat="server" Text="[FIX THIS]" /> academic
year. The College will award three permits for each home driveway via the
lottery. Other campus home dwellers may purchase Lot V permits if available.
You must be a resident of the specific campus home to apply for a
driveway-parking permit.

For further information regarding the parking lottery, please contact the
Business Office at x6200.
###############################################################################</pre>
</asp:Panel>