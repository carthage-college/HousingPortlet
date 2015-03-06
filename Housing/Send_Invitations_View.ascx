<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Send_Invitations_View.ascx.cs" Inherits="Housing.Send_Invitations_View" %>
<%@ Register Assembly="Jenzabar.Common" Namespace="Jenzabar.Common.Web.UI.Controls" TagPrefix="common" %>

<div class="pSection">
    <p>You have selected <asp:Literal ID="ltlRoomSelected" runat="server" Text="[FIX THIS]" /></p>
    <common:DataList ID="dlRoommates" runat="server" OnItemDataBound="dlRoommates_ItemDataBound">
        <ItemTemplate>
            <asp:Literal ID="ltlBed" runat="server" Text="[FIX THIS]" />
        </ItemTemplate>
    </common:DataList>
</div>