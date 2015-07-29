<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AzureSearchBox.ascx.cs" Inherits="AzureSearchBox" %>

<h2>Azure Search</h2>
<asp:UpdateProgress ID="upp1" runat="server" AssociatedUpdatePanelID="up1">
    <ProgressTemplate>
        Working....
    </ProgressTemplate>
</asp:UpdateProgress>
<asp:UpdatePanel ID="up1" runat="server">
    <ContentTemplate>
        <h3>Index Actions</h3>
        <asp:Button ID="btnCreateIndex" runat="server" Text="Create Index" CssClass="btn btn-primary" OnClick="bt_Click" />
        <asp:Button ID="btnLoadIndex" runat="server" Text="Load Index"  CssClass="btn btn-primary" OnClick="bt_Click" />
        <h3>Search</h3>
        <asp:TextBox ID="txtSearch" runat="server"></asp:TextBox>
        <br />
        <br />
        <asp:Button ID="btnSearch" runat="server" Text="Search" OnClick="bt_Click"  CssClass="btn btn-primary" />
        <asp:Button ID="btnReset" runat="server" Text="Reset" OnClick="bt_Click"  CssClass="btn btn-primary" />
        <br />
        <br />
        <asp:Label ID="lblResults" runat="server"></asp:Label>
    </ContentTemplate>
</asp:UpdatePanel>
