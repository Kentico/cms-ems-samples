<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TeamUpSyncControl.ascx.cs" Inherits="TeamUpSyncControl" %>
<asp:UpdateProgress ID="upp1" runat="server" AssociatedUpdatePanelID="up1">
    <ProgressTemplate>
        Loading... 
    </ProgressTemplate>
</asp:UpdateProgress>
<asp:UpdatePanel ID="up1" runat="server">
    <ContentTemplate>
        <asp:Button ID="btnGetEvent" runat="server" Text="Refresh TeamUp Data" OnClick="btn_Click" CssClass="btn btn-primary" />
        <br />
        <br />
        <strong>TeamUp Event</strong><br />
        <asp:Label ID="lblEvent" runat="server"></asp:Label>
        <br />
        <br />
        <asp:Button ID="btnCreateEvent" runat="server" Text="Create Event" OnClick="btn_Click"  CssClass="btn btn-primary" />
        <asp:Button ID="btnDeleteEvent" runat="server" Text="Delete Event" OnClick="btn_Click" CssClass="btn btn-primary" OnClientClick="return confirm('Are you sure you want to delete this event?');" />
        <br />
        <asp:Label ID="lblMessage" runat="server"></asp:Label>
        <asp:HiddenField ID="hidEventID" runat="server" />
        <asp:HiddenField ID="hidEventVersion" runat="server" />
    </ContentTemplate>
</asp:UpdatePanel>
