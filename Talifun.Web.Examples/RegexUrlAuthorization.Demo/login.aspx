<%@ Page Language="C#" %>

<html>
<head>
    <title>Login</title>
    <script language="C#" runat="server">
        void Login_Click(Object sender, EventArgs e) 
        {
            if (FormsAuthentication.Authenticate(UsernameTextBox.Text, PasswordTextBox.Text))
                FormsAuthentication.RedirectFromLoginPage(UsernameTextBox.Text, true);
            else
                status.InnerHtml = "Invalid Login";
        }
    </script>
</head>
<body>
    <p class=title>Login</p> 
    <span id="status" Style="color:Red" runat="Server"/>
    
    <form ID="Form1" runat="server">
    Username: <asp:TextBox ID="UsernameTextBox" CssClass="text" runat="Server"/><br />
    Password: <asp:TextBox ID="PasswordTextBox" TextMode="Password" CssClass="text" runat="Server"/><br />
    <asp:Button ID="LoginButton" OnClick="Login_Click" Text="  Login  " CssClass="button" runat="Server"/>
    </form>
    
    <div>
        <h2>To download pdf files use:</h2><br />
        Default username: pdf<br />
        Default password: password<br />
    </div>
    
    <div>
        <h2>To download txt files use:</h2><br />
        Default username: txt<br />
        Default password: password<br />
    </div>
</body>
</html>
