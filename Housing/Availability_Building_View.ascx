<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Availability_Building_View.ascx.cs" Inherits="Housing.Availability_Building_View" %>
<%@ Register Assembly="Jenzabar.Common" Namespace="Jenzabar.Common.Web.UI.Controls" TagPrefix="common" %>

<div class="pSection"><common:ErrorDisplay ID="errMsg" runat="server" /></div>

<asp:Panel ID="panelBuildings" runat="server" CssClass="pSection">
    <p>The following building<asp:Literal ID="ltlBuildingPlural" runat="server" /> currently <asp:Literal ID="ltlBuildingPlural2" runat="server" /> rooms available for you to select:</p>
    <asp:BulletedList ID="bulletedBuildings" runat="server" DisplayMode="LinkButton" OnClick="bulletedBuildings_Click">
    </asp:BulletedList>
</asp:Panel>