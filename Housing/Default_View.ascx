<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Default_View.ascx.cs" Inherits="Housing.Default_View" %>
<%@ Register Assembly="Jenzabar.Common" Namespace="Jenzabar.Common.Web.UI.Controls" TagPrefix="common" %>
<div class="pSection">
    <common:ErrorDisplay ID="errMsg" runat="server" />
</div>
<h4>Welcome, <asp:Literal ID="ltlStudentName" runat="server" Text="[FIX THIS]" /></h4>
<asp:Panel ID="panelRegistered" runat="server" CssClass="pSection">
    <p>You have registered for <asp:Literal ID="ltlRegisteredHousing" runat="server" Text="[FIX THIS]" />.</p>
    <p>Your registration was completed at <asp:Literal ID="ltlRegisteredDateTime" runat="server" Text="[FIX THIS]" />.</p>
    <p>If you have not already purchased a parking permit, you may do so <a href="#">here</a>.</p>
    <asp:BulletedList ID="bulletedRoommates" runat="server">
    </asp:BulletedList>
</asp:Panel>
<asp:Panel ID="panelCommuter" runat="server" CssClass="pSection">
    <p>Our records indicate that you are currently a commuter. If you are interested in Carthage housing, please contact Nina Fleming at <a href="mailto:nfleming@carthage.edu">nfleming@carthage.edu</a>
    with your request.</p>
</asp:Panel>
<asp:Panel ID="panelUnregistered" runat="server" CssClass="pSection">
    <p>You have not yet registered for a room. Please verify that all the following information is correct.</p>
    <ul>
        <li>You are <asp:Literal ID="ltlGreekStatus" runat="server" Text="[FIX THIS]" />.</li>
        <li>
            Your account is <asp:Literal ID="ltlHold" runat="server" Text="[FIX THIS]" /> current.
            <common:ContentBox ID="contentHoldDetail" runat="server">
                <strong>You will not be able to sign up for housing until your outstanding balances are paid. Please contact the Business Office at x6333 about this.</strong>
            </common:ContentBox>
        </li>
        <li>
            You have <asp:Literal ID="ltlRegistered" runat="server" Text="[FIX THIS]" /> registered for classes in the fall.
            <common:ContentBox ID="contentRegisteredDetail" runat="server">
                <strong>You are unable to sign up for housing because you are not registered as a full-time student for the <asp:Literal ID="ltlRegisteredYear" runat="server" Text="[FIX THIS]" />
                academic year. Contact the Registrar's office at 262-551-6100 for additional information.</strong>
            </common:ContentBox>
        </li>
        <li><asp:Literal ID="ltlGender" runat="server" Text="[FIX THIS]" />.</li>
        <li>You have earned <asp:Literal ID="ltlCareerCredits" runat="server" Text="[FIX THIS]" /> credits.</li>
        <li>
            <asp:Panel ID="panelCurrentHousing" runat="server">
                You are currently listed as living in <asp:Literal ID="ltlCurrentHousing" runat="server" Text="[FIX THIS]" />.
            </asp:Panel>
            <asp:Literal ID="ltlNotResident" runat="server" Text="You are not currently listed as living on campus." />
        </li>
    </ul>

    <!--- Invitations --->

    <common:Subheader ID="subhdrAvailability" runat="server" Text="Room Availability" />
    <asp:LinkButton ID="lnkAvailability" runat="server" Text="View the rooms currently available." OnClick="lnkAvailability_Click" />
    <asp:Literal ID="ltlCannotRegister" runat="server" Text="Please make sure that your balance is paid and that you are registered for classes." />
    <asp:Literal ID="ltlInvalidTime" runat="server" Text="It is not your time to register. Check the housing <a href='overview.cfm'>overview page</a> for a complete breakdown of registration times." />
</asp:Panel>