<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Send_Invitations_View.ascx.cs" Inherits="Housing.Send_Invitations_View" %>
<%@ Register Assembly="Jenzabar.Common" Namespace="Jenzabar.Common.Web.UI.Controls" TagPrefix="common" %>

<div class="pSection">
    <p>You have selected <asp:Literal ID="ltlRoomSelected" runat="server" Text="[FIX THIS]" /></p>
    <%--
    <asp:Repeater ID="repeaterRoommates" runat="server">
        <HeaderTemplate><ul></HeaderTemplate>
        <ItemTemplate>
            <li><asp:Literal ID="ltlRoommate" runat="server" /></li>
        </ItemTemplate>
        <FooterTemplate></ul></FooterTemplate>
    </asp:Repeater>
    --%>

    <h4>Invite Roommates to <asp:Literal ID="ltlRoom" runat="server" /></h4>
    <asp:Repeater ID="repeaterRoommates" runat="server" OnItemDataBound="repeaterRoommates_ItemDataBound">
        <HeaderTemplate><ul></HeaderTemplate>
        <ItemTemplate>
            <li><asp:PlaceHolder ID="phRoommate" runat="server" /></li>
        </ItemTemplate>
        <FooterTemplate></ul></FooterTemplate>
    </asp:Repeater>
    <asp:Panel ID="panelStudentDetail" runat="server">
        <asp:Panel ID="panelStudentOK" runat="server">
            You are about to send an invitation to <asp:Literal ID="ltlStudentName" runat="server" /> at <asp:Literal ID="ltlStudentEmail" runat="server" />.
            <asp:Button ID="btnSendInvitation" runat="server" Text="Send Invitation" OnClick="btnSendInvitation_Click" />
        </asp:Panel>
        <asp:Panel ID="panelStudentNo" runat="server">
            We are unable to send an invitation to <asp:Literal ID="ltlStudentEmail2" runat="server" />.
            <asp:Literal ID="ltlReason" runat="server" />
        </asp:Panel>
    </asp:Panel>

</div>