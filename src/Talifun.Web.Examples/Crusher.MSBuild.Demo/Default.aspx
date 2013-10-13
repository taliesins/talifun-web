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
            <li><asp:HyperLink ID="ConventionCrushedHyperLink" runat="server" Text="Convention Crushed Page" NavigateUrl="~/ConventionPage.aspx"/></li>
            <li><asp:HyperLink ID="DebugConventionCrushedHyperLink" runat="server" Text="Debug Convention Crushed Page" NavigateUrl="~/DebugConventionPage.aspx"/></li>
        </ul>
    </div>
    <div>
        <ul>
            <li><asp:HyperLink ID="GoodCssSpriteHyperLink" runat="server" Text="Good Css Sprite" NavigateUrl="~/GoodCssSprite.aspx"/></li>
            <li><asp:HyperLink ID="BadCssSpriteHyperlink" runat="server" Text="Bad Css Sprite" NavigateUrl="~/BadCssSprite.aspx"/></li>
            <li><asp:HyperLink ID="ConventionCssSpriteHyperLink" runat="server" Text="Convention Css Sprite" NavigateUrl="~/ConventionCssSprite.aspx"/></li>
        </ul>      
        The generated sprite image has to be a png and it will be upsized to 32 bit colour to support transparency.
    </div>
    </form>
</body>
</html>
