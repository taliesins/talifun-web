<%@ Page Language="C#"   %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <ul>
            <li><asp:HyperLink ID="CrushedHyperlink" runat="server" Text="Crushed Page" NavigateUrl="~/CrushedPage.aspx"/></li>
            <li><asp:HyperLink ID="DebugCrushedHyperlink" runat="server" Text="Debug Crushed Page" NavigateUrl="~/DebugCrushedPage.aspx"/></li>
            <li><asp:HyperLink ID="UrlCrushedHyperLink" runat="server" Text="Url Crushed Page" NavigateUrl="~/UrlCrushedPage.aspx"/></li>
            <li><asp:HyperLink ID="UncrushedHyperLink" runat="server" Text="Uncrushed Page" NavigateUrl="~/UncrushedPage.aspx"/></li>
        </ul>
    </div>
    </form>
</body>
</html>
