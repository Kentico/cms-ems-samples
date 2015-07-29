<%@ Control Language="C#" AutoEventWireup="true"
    Inherits="CMSWebParts_SmartSearch_MySearchBox" CodeFile="~/CMSWebParts/SmartSearch/MySearchBox.ascx.cs" %>
<asp:Panel ID="pnlSearch" runat="server" DefaultButton="btnImageButton" CssClass="searchBox" EnableViewState="false">
    <cms:LocalizedLabel DisplayColon="true" ID="lblSearch" runat="server" AssociatedControlID="txtWord" EnableViewState="false" />
    <cms:CMSTextBox ID="txtWord" runat="server" EnableViewState="false"  MaxLength="1000" />
    <cms:CMSButton ID="btnSearch" runat="server" EnableViewState="false" ButtonStyle="Default" />
    <asp:ImageButton ID="btnImageButton" runat="server" Visible="false" EnableViewState="false" />
    <asp:Panel ID="pnlPredictiveResultsHolder" runat="server" CssClass="predictiveSearchHolder" EnableViewState="false" />
</asp:Panel>
