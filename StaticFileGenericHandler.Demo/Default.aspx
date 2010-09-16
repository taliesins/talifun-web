<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="StaticFileGenericHandler.Demo.Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html>
<head id="Head" runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <ul>
            <li>
                <asp:HyperLink ID="DownloadHyperLink" runat="server" Text="Download" NavigateUrl="~/StaticFile.ashx"/>
            </li>
            <li>
                <asp:HyperLink ID="DownloadWithTimestampHyperLink" runat="server" Text="Download with timestamp for cache busting"/>
            </li>
        </ul>
    </div>
    </form>
</body>
</html>
