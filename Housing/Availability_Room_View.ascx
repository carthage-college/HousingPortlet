<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Availability_Room_View.ascx.cs" Inherits="Housing.Availability_Room_View" %>
<%@ Register Assembly="Jenzabar.Common" Namespace="Jenzabar.Common.Web.UI.Controls" TagPrefix="common" %>

<div class="pSection"><common:ErrorDisplay ID="errMsg" runat="server" /></div>

<asp:LinkButton ID="lnkPickBuilding" runat="server" Text="Pick A Different Building" OnClick="lnkPickBuilding_Click" />

<h4><asp:Literal ID="ltlBuildingName" runat="server" Text="[FIX THIS]" /></h4>
<asp:Panel ID="panelOaksDescription" runat="server" Visible="false">
    <div class="pSection">
        Suites - offer two separate single living spaces with a shared bathroom<br />
        Double - one shared living space with a private bathroom
    </div>
</asp:Panel>
<asp:Repeater ID="rptRoomList" runat="server">
    <HeaderTemplate>
        <table cellpadding="3" cellspacing="0" border="0" class="tableRoomAvailability">
    </HeaderTemplate>
    <ItemTemplate>
        <tr>
            <td><asp:Literal ID="ltlRoomNumber" runat="server" /></td>
            <td>
                <asp:PlaceHolder ID="phSpots" runat="server" />
            </td>
        </tr>
    </ItemTemplate>
    <FooterTemplate>
        </table>
    </FooterTemplate>
</asp:Repeater>