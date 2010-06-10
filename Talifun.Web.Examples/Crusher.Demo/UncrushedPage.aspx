<%@ Page Language="C#"  %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title></title>
    <link rel="stylesheet" type="text/css" href="<%=this.ResolveUrl("~/Static/Css/jquery-ui-1.8rc3.custom.css")%>" media="screen" /> 
    <link rel="stylesheet" type="text/css" href="<%=this.ResolveUrl("~/Static/Css/default.css")%>" media="screen" />
    <script language="javascript" type="text/javascript" src="<%=this.ResolveUrl("~/Static/Js/jquery-1.4.2.min.js")%>"></script>
    <script language="javascript" type="text/javascript" src="<%=this.ResolveUrl("~/Static/Js/jquery-ui-1.8rc3.custom.min.js")%>"></script>
    <script language="javascript" type="text/javascript" src="<%=this.ResolveUrl("~/Static/Js/jquery.flash.min.js")%>"></script>
    <script language="javascript" type="text/javascript" src="<%=this.ResolveUrl("~/Static/Js/jquery.validate.min.js")%>"></script>
    <script language="javascript" type="text/javascript" src="<%=this.ResolveUrl("~/Static/Js/additional-validation-methods.min.js")%>"></script>
    <script language="javascript" type="text/javascript" src="<%=this.ResolveUrl("~/Static/Js/test.js")%>"></script>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <ul>
            <li>Each css file is served from the orginal file.</li>
            <li>Each js file is served from the orginal file.</li>
        </ul>
    </div>
    
    <input type="button" onclick="TestMessage()" value="Test" />
    </form>
</body>
</html>