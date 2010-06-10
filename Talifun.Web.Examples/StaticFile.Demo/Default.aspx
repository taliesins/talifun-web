<%@ Page Language="C#" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head id="Head1" runat="server">
    <title></title>
    <link id="Link1" runat="server" href="~/Static/Css/CssAdapter.css" rel="stylesheet" type="text/css" />
</head>
<body>
    <form id="form1" runat="server">
    <div>
        Resumable downloads will only work with web server that you can modify the headers on.
        So it will work on IIS but not on Cassini.
    
        <img src="/doesnotexist1.jpg" width="100px" height="100px" />
        <img src="/doesnotexist2.jpg" width="100px" height="100px" />
        <img src="/doesnotexist3.jpg" width="100px" height="100px" />
        <img src="/doesnotexist4.jpg" width="100px" height="100px" />
        <img src="/doesnotexist5.jpg" width="100px" height="100px" />
        <img src="/doesnotexist6.jpg" width="100px" height="100px" />
        <img src="/doesnotexist7.jpg" width="100px" height="100px" />
    
        <asp:Image ID="MainImage" runat="server" ImageUrl="~/Static/Images/Main.jpg" />
        <asp:HyperLink ID="DownloadHyperLink" runat="server" NavigateUrl="~/Static/test.zip" Text="Download" />
    </div>
    </form>
</body>
</html>