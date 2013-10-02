<%@ Page Language="C#" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title></title>
    <talifun:CssControl ID="SiteCssControl" runat="server" GroupName="ConventionSiteCss" />
    <talifun:JsControl ID="SiteJsControl" runat="server" GroupName="ConventionSiteJs" />
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <ul>
            <li>All the css is served from one file.</li>
            <li>All the js is served from one file.</li>
            <li>A hash is appended via a querystring to ensure that updates to the files will result in the file being re-served to client.</li>
            <li>Updating any css or js file will result in the crushed file being regenerated and a new hash being created.</li>
        </ul>
    </div>
    
    <input type="button" onclick="TestMessage()" value="Test" />
    </form>
    
    <script language="javascript" type="text/javascript">
        Templates['/Static/Js/test.moustache'](moustacheModel);
    </script>
</body>
</html>