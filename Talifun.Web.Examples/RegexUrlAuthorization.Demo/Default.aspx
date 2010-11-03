<%@ Page Language="C#" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head id="Head1" runat="server">
    <title></title>
    <script language="C#" runat="server">
        
        void Logout_Click(Object sender, EventArgs e) 
        {
            FormsAuthentication.SignOut();
            Response.Redirect("~/Default.aspx");
        }

        void Login_Click(Object sender, EventArgs e)
        {
            Response.Redirect("~/login.aspx?ReturnUrl=%2fDefault.aspx");
        }
        
    </script>
</head>
<body>
    <form id="form1" runat="server">
    <div>
<%
            if (this.Request.IsAuthenticated)
            {
                %>
                <asp:Button ID="LogoutButton" runat="server" Text="Logout" OnClick="Logout_Click" /> (<%=this.User.Identity.Name %>)
                <%  
            } 
            else
            {
                %>
                <asp:Button ID="LoginButton" runat="server" Text="Login" OnClick="Login_Click" />
                <%
            }
%>
    
        <ul>
            <li>
                <asp:HyperLink ID="ShowPdfHyperLink" runat="server" NavigateUrl="~/test.pdf" Text="Show Pdf" />
            </li>
            <li>
                <asp:HyperLink ID="ShowTextHyperLink" runat="server" NavigateUrl="~/test.txt" Text="Show Txt" />            
            </li>
        </ul>
    </div>
    </form>
</body>
</html>