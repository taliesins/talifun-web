<%@ Page Language="C#" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title></title>
    <link id="Link1" runat="server" href="~/Static/Css/crushed.css-sprite.css" rel="stylesheet" type="text/css" />
</head>
<body>
    <form id="form1" runat="server">
    <div>
        The images that make up the sprite are similar in canvas size and there are quite a few component images, making it a 
        good candidate for a css sprite. The generated image is smaller then the compressed image.
    
        Good:
        Image 1
        <div class="_01_png"></div>
        Image 3
        <div class="_03_png"></div>
        Image 2
        <div class="_02_png"></div>
        Image 4
        <div class="_04_png"></div>
        Image 5
        <div class="_05_png"></div>
        Image 6
        <div class="_06_png"></div>
        Image 7
        <div class="_07_png"></div>
        Image 8
        <div class="_08_png"></div>
        Image 9
        <div class="_09_png"></div>
        Image 10
        <div class="_10_png"></div>
        
        
        Bad:
        Image 1
        <div class="background-nav-ages-11-14_gif"></div>
        Image 3
        <div class="logo_gif"></div>
        Image 2
        <div class="Main_jpg"></div>
    </div>
    </form>
</body>
</html>
