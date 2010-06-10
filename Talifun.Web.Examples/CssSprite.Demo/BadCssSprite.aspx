<%@ Page Language="C#" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title></title>
    <link id="Link1" runat="server" href="~/Static/Css/bad-css-sprite.css" rel="stylesheet" type="text/css" />
</head>
<body>
    <form id="form1" runat="server">
    <div>
        The images that make up the sprite are really different in canvas size and there are only a few component images, making it a 
        bad candidate for a css sprite. The generated image is far larger then the compressed image.
    
        Image 1
        <div class="BadImage1"></div>
        Image 3
        <div class="BadImage3"></div>
        Image 2
        <div class="BadImage2"></div>
    </div>
    </form>
</body>
</html>
