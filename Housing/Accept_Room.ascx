<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Accept_Room.ascx.cs" Inherits="Housing.Accept_Room" %>
<%@ Register Assembly="Jenzabar.Common" Namespace="Jenzabar.Common.Web.UI.Controls" TagPrefix="common" %>

<script type="text/javascript">
    $(function () {
        $('#<%= chkAgree.ClientID %>').click(function () {
            $('#<%= btnSubmit.ClientID %>').prop('disabled', $(this).prop('checked') ? '' : 'disabled');
        });
        $('#<%= btnSubmit.ClientID %>').prop('disabled', $(this).prop('checked') ? '' : 'disabled');
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
    <common:ContentBox ID="contentWaitlist" runat="server">
        <p>You have chosen not to live in the Oaks. Would you like to be placed on a waiting list to live in the Oaks should a space become available?
            Please realize that there is an extra charge associated with living in this premium housing.</p>
        <p><asp:CheckBox ID="chkOaksWaitlist" runat="server" Text="Please put me on a waiting list for the Oaks." /></p>
    </common:ContentBox>
    <p><asp:Button ID="btnSubmit" runat="server" Text="Sign up for [FIX THIS]" OnClick="btnSubmit_Click" /></p>
</div>

<!--- Contract --->
<pre>##########
    
CARTHAGE COLLEGE
HOUSING CONTRACT FOR RESIDENCE HALLS
ACADEMIC YEAR <asp:Literal ID="ltlContractYear" runat="server" Text="[FIX THIS]" />

RETURNING STUDENTS
Carthage agrees to provide assigned space in the residence halls for the undersigned 
student for the <asp:Literal ID="ltlContractYear2" runat="server" Text="[FIX THIS]" /> academic year and the undersigned student agrees to pay for 
said unassigned living space on the following terms:

DURATION
This contract shall be for three (3) consecutive terms of the academic year including 
Term I, J-Term, and Term II. The student shall be entitled to reside in the halls starting 
twenty-four (24) hours before classes begin and ending immediately after the final exam 
each term. The only exception to this is J-Term. The campus does not close between the 
end of J-Term and the beginning of Term II. The campus will be closed for the following 
breaks:

    Thanksgiving November 21 after 6:00 p.m. through November 30, <asp:Literal ID="ltlThisYear1" runat="server" /> at noon
    Christmas December 12, <asp:Literal ID="ltlThisYear2" runat="server" /> after 6:00 p.m. through January 5, <asp:Literal ID="ltlNextYear1" runat="server" /> at noon
    Spring Break March 20, after 6:00 p.m. through March 29, <asp:Literal ID="ltlNextYear2" runat="server" /> at noon

The student shall remove all personal possessions from their assigned space upon 
termination of residence for any reason. All such personal possessions not removed will 
be disposed of by Carthage and a cleaning fee assessed to the student.

TERMS
A student must be registered full time to reside in the residence halls during any semester. 
This equates to a minimum of twelve (12) credits each term, and a minimum of four (4) 
credits during J-Term, to be eligible to live in the residence halls. Exception to the above 
may be made by the Dean of Students.

CHARGES
The per term charges for rooms will be determined by Carthage at a later date. Carthage 
reserves the right to change the amount of charge at any time. All students living on 
campus are required to take a meal plan.

COMMUNITY CHARGES
If damages or vandalism occur and no individual(s) is directly identified as being 
responsible, the cost of repairs will be equally charged to the members of that wing, floor, 
building or group of people most closely related to the damages. These charges will be 
placed on the student’s monthly statement from Carthage.

ADJUSTMENTS
No refund of room charges shall be made when a student withdraws, is dismissed from 
Carthage, or is removed from housing for disciplinary reasons.

UNASSIGNED LIVING SPACE
The contract does not constitute a guarantee for a specific room or roommate. Carthage 
reserves the right to make any changes in room or roommate assignments at any time.

EFFECTIVE DATE
This contract becomes binding (1) when the student pays applicable advance payment 
and (2) completes the on-line housing selection process. It continues for the academic 
year as long as the student remains in good standing with Carthage.

NO SHOWS
Any student who does not report to their assigned living space 24 hours after the start of 
classes will automatically lose their assigned space.

RULES AND REGULATIONS
Carthage reserves the right for its authorized personnel to enter student rooms to preserve 
and protect Carthage property and to assure compliance with state and local laws or 
Carthage rules and regulations. In signing this contract, the student agrees to adhere to 
all rules and regulations governing his/her behavior as outlined in the Carthage Student 
Community Code. 

PAYMENTS
All payments hereunder shall be made to the Business Office
###############################################################################
</pre>

<asp:Panel ID="panelApartmentContract" runat="server">
    <!--- Apartment --->
<pre>Dear Apartment resident,

You have been assigned to the Apartments at Carthage College for the <asp:Literal ID="ltlApartmentYear" runat="server" Text="[FIX THIS]" />
academic year. The street address for the apartments is: 1625 Sheridan Road,
in the town of Somers, WI. This letter details important information as you
prepare for your return. Please browse the information below.

Utilities -  All utilities are included in the cost.

Telephone service - You are responsible for obtaining your own local and long
distance service. Some possible service providers are listed below:

Ameritech sales: 800-924-1000
McLeodUSA sales: 800-909-3012
National Prepaid sales: 877-606-6066

Cable service - Cable service is not provided.  A possible service provider is
listed below:

Time-Warner Customer Service: 800-933-4662

Internet service - Internet service is not included. Some possible suggestions
for internet providers are listed below.

ASAP virtual networks: 888-700-8710     Ameritech: 800-638-8775
Broadwing: 800-276-2394                 Wisconsin Internet: 888-782-1454

Parking - The apartment parking lot has a maximum capacity of 13 vehicles.
Overflow parking for the apartment building is in Lot V. Apartment dwellers
cannot park on the lawn, the entrance drive or the street. If the apartment lot
is full you must park in Lot V, located right across the street. Contact the
business office (262-551-6200) for more information about parking.

Laundry facilities - Laundry machines are located in the building. Laundry
machines are card access. Cards will be distributed at check-in.

Mail Delivery - You will continue to receive your mail in the TWC via your
campus mailbox.

Break Periods - All campus housing facilities, including the Houses and
Apartments, close during college break periods. Students who would like to stay
during break periods must qualify for an &quot;extended stay.&quot;
&quot;Extended stay&quot; paperwork is available from the Dean of Students
office.

Periodic Facilities Inspections - The Dean of Students Office reserves the
right to conduct facilities inspections throughout the year, including prior
to checkout. For maintenance concerns, contact the Dean of Students office at
262-551-5800 or <a href="mailto:nfleming@carthage.edu">nfleming@carthage.edu</a>.

Campus Directory Information - If you would like to have your phone number
listed in the campus directory, please submit this information to the Dean of
Students office as soon as possible once you return.

Check-In Inventories - All apartment residents will be responsible for
completing an apartment inventory. More information will be available at
check-in.


You are responsible for being familiar with and abiding by the Carthage College
Community Code (<a href="http://www.carthage.edu/campus-life/code/">http://www.carthage.edu/campus-life/code/</a>). All
information in the Community Code regarding room decoration(s) applies to the
Houses. No structural changes may be made to the building. In addition, nails
should not be used to attach fixtures. Questions regarding policies should be
directed to the Dean of Students office.

Sincerely,
Nina Fleming
Assistant Dean of Students
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