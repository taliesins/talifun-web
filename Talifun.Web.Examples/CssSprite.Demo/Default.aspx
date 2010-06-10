<%@ Page Language="C#" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title></title>
    
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <ul>
            <li><asp:HyperLink ID="GoodCssSpriteHyperLink" runat="server" Text="Good Css Sprite" NavigateUrl="~/GoodCssSprite.aspx"/></li>
            <li><asp:HyperLink ID="BadCssSpriteHyperlink" runat="server" Text="Bad Css Sprite" NavigateUrl="~/BadCssSprite.aspx"/></li>
        </ul>
        
        The generated sprite image has to be a png and it will be upsized to 32 bit colour to support transparency.
    </div>
    </form>
</body>
</html>
