<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Default_View.ascx.cs" Inherits="Housing.Default_View" %>
<%@ Register Assembly="Jenzabar.Common" Namespace="Jenzabar.Common.Web.UI.Controls" TagPrefix="common" %>
<div class="pSection">
    <common:ErrorDisplay ID="errMsg" runat="server" />
</div>
<h4 id="welcome" runat="server">Welcome, <asp:Literal ID="ltlStudentName" runat="server" Text="[FIX THIS]" /></h4>
<asp:Panel ID="panelRegistered" runat="server" CssClass="pSection">
    <p>You have registered for <asp:Literal ID="ltlRegisteredHousing" runat="server" Text="[FIX THIS]" />.</p>
    <p>Your registration was completed at <asp:Literal ID="ltlRegisteredDateTime" runat="server" Text="[FIX THIS]" />.</p>
    <%--- <p>If you have not already purchased a parking permit, you may do so <a href="http://www.carthage.edu/parking/">here</a>.</p> ---%>
    <p>Occupants of your room:</p>
    <asp:BulletedList ID="bulletedRoommates" runat="server" CssClass="roommateList">
    </asp:BulletedList>

    <!-- Extended Invitations -->
    <asp:Panel ID="panelExtendedInvitations2" runat="server">
        You have extended the following invitation(s):
        <asp:Repeater ID="repeaterExtendedInvites2" runat="server" OnItemDataBound="repeaterExtendedInvites_ItemDataBound">
            <HeaderTemplate><ul class="roommateList"></HeaderTemplate>
            <ItemTemplate>
                <li>Invited <asp:Literal ID="ltlInvitedName" runat="server" /></li>
            </ItemTemplate>
            <FooterTemplate></ul></FooterTemplate>
        </asp:Repeater>
    </asp:Panel>
</asp:Panel>
<asp:Panel ID="panelCommuter" runat="server" CssClass="pSection">
    <p>Our records indicate that you are currently a commuter. If you are interested in Carthage housing, please contact Amber Krusza at <a href="mailto:akrusza@carthage.edu">akrusza@carthage.edu</a>
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
        <%---
        <li>
            You have <asp:Literal ID="ltlRegistered" runat="server" Text="[FIX THIS]" /> registered for classes in the fall.
            <common:ContentBox ID="contentRegisteredDetail" runat="server">
                <strong>You are unable to sign up for housing because you are not registered for classes in the <asp:Literal ID="ltlRegisteredYear" runat="server" Text="[FIX THIS]" />
                Fall term. Contact the Registrar's office at 262-551-6100 for additional information.</strong>
            </common:ContentBox>
        </li>
        ---%>
        <li><asp:Literal ID="ltlGender" runat="server" Text="[FIX THIS]" />.</li>
        <li>You have earned <asp:Literal ID="ltlCareerCredits" runat="server" Text="[FIX THIS]" /> credits.</li>
        <li>
            <asp:Panel ID="panelCurrentHousing" runat="server">
                You are currently listed as living in <asp:Literal ID="ltlCurrentHousing" runat="server" Text="[FIX THIS]" />.
            </asp:Panel>
            <asp:Literal ID="ltlNotResident" runat="server" Text="You are not currently listed as living on campus." />
        </li>
    </ul>

    <!-- Invitations -->
    <asp:Panel ID="panelInvitations" runat="server">
        You have been invited to select the following room(s):
        <asp:Repeater ID="repeaterInvites" runat="server" OnItemDataBound="repeaterInvites_ItemDataBound">
            <HeaderTemplate><ul class="roommateList"></HeaderTemplate>
            <ItemTemplate>
                <li><asp:Literal ID="ltlInvite" runat="server" /><asp:Button ID="btnRoomInvite" runat="server" /> (Invited by <asp:Literal ID="ltlInviteBy" runat="server" />)</li>
            </ItemTemplate>
            <FooterTemplate></ul></FooterTemplate>
        </asp:Repeater>

        <asp:Repeater ID="repeaterInvitesReadOnly" runat="server">
            <HeaderTemplate><ul class="roommateList"></HeaderTemplate>
            <ItemTemplate>
                <li><asp:Literal ID="ltlInvite" runat="server" /> (Invited by <asp:Literal ID="ltlInviteBy" runat="server" />)</li>
            </ItemTemplate>
            <FooterTemplate></ul></FooterTemplate>
        </asp:Repeater>
    </asp:Panel>

    <!-- Extended Invitations -->
    <asp:Panel ID="panelExtendedInvitations" runat="server">
        You have extended the following invitation(s):
        <asp:Repeater ID="repeaterExtendedInvites" runat="server" OnItemDataBound="repeaterExtendedInvites_ItemDataBound">
            <HeaderTemplate><ul class="roommateList"></HeaderTemplate>
            <ItemTemplate>
                <li>Invited <asp:Literal ID="ltlInvitedName" runat="server" /></li>
            </ItemTemplate>
            <FooterTemplate></ul></FooterTemplate>
        </asp:Repeater>
    </asp:Panel>

    <asp:Panel ID="panelAvailability" runat="server">
        <common:Subheader ID="subhdrAvailability" runat="server" Text="Room Availability" />
        <asp:LinkButton ID="lnkAvailability" runat="server" Text="View the rooms currently available." OnClick="lnkAvailability_Click" />
        <asp:Literal ID="ltlCannotRegister" runat="server" Text="Please make sure that your balance is paid and that you are registered for classes." />
        <asp:Panel ID="panelOverview" runat="server">
            <p>It is not your time to register. You may:</p>
            <ul class="registrationOptions">
                <%--- <li>Register for <asp:Literal ID="ltlRegisterGreek" runat="server" Text="a room affiliated with your greek organization" /> on <asp:Literal ID="ltlGreekSquatterDay" runat="server" />.</li> ---%>
                <li>Register for a room affiliated with your greek organization<asp:Literal ID="ltlRegisterGreek" runat="server" Text="" /> on <asp:Literal ID="ltlGreekSquatterDay" runat="server" />.</li>
                <li>Register for any room starting at <asp:Literal ID="ltlFirstRegisterDateTime" runat="server" />.</li>
            </ul>
            <p>For a complete breakdown of registration days and times, please check the housing <a href="https://www.carthage.edu/housing/selection/overview.cfm" target="_blank">overview page</a>.</p>
        </asp:Panel>
    </asp:Panel>
</asp:Panel>