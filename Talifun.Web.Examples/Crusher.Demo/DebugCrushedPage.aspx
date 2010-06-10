<%@ Page Language="C#" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title></title>
    <talifun:CssControl ID="DebugSiteCssControl" runat="server" GroupName="DebugSiteCss" />
    <talifun:JsControl ID="DebugSiteJsControl" runat="server" GroupName="DebugSiteJs" />    
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <ul>
            <li>Each css file is served from the orginal file.</li>
            <li>Each js file is served from the orginal file.</li>
            <li>A hash is appended via a querystring to ensure that updates to the files will result in the file being re-served to client.</li>
            <li>Updating any css or js file will result in a new hash for the file.</li>
        </ul>    
    </div>
    
    <input type="button" onclick="TestMessage()" value="Test" />
    </form>
</body>
</html>
