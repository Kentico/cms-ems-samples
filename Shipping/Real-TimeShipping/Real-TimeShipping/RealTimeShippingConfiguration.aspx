<%@ Page Title="" Language="C#" MasterPageFile="~/CMSMasterPages/UI/SimplePage.master" AutoEventWireup="true" CodeBehind="RealTimeShippingConfiguration.aspx.cs" Inherits="RealTimeShippingConfiguration" Theme="Default" %>
<%@ Register TagPrefix="cms" TagName="SettingsGroupViewer" Src="~/CMSModules/Settings/Controls/SettingsGroupViewer.ascx" %>

<asp:Content ID="plcConfiguration" ContentPlaceHolderID="plcContent" runat="server">
    <cms:SettingsGroupViewer ID="SettingsGroupViewer" runat="server" AllowGlobalInfoMessage="false" CategoryName="Real-TimeShipping" />
</asp:Content>
