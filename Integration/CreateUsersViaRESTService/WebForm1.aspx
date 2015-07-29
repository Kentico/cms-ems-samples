<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="WebForm1.aspx.cs" Inherits="WebApplication1.WebForm1" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <h2>Create User</h2>
            <strong>Password</strong>
            <br />
            <asp:TextBox ID="pwPassword" runat="server" TextMode="Password">
            </asp:TextBox>
            <br />
            <br />
            <strong>Roles</strong>
            <br />
            <asp:CheckBoxList ID="cblRoles" runat="server">
            </asp:CheckBoxList>
            <br />
            <br />
            <asp:Button ID="Button1" runat="server" Text="Create User" OnClick="Button1_Click" />
            <h3>Results</h3>
            <asp:TextBox ID="txtResults" runat="server" TextMode="MultiLine" Width="1000" Rows="50" Visible="true" Text="Results will be loaded here"></asp:TextBox>
            <asp:Label ID="lblResults" runat="server"></asp:Label>
        </div>
    </form>
</body>
</html>
