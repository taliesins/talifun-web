<%@ Page Language="C#" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title></title>
    <talifun:CssControl ID="UrlSiteCssControl" runat="server" GroupName="UrlSiteCss" />
    <talifun:JsControl ID="UrlSiteJsControl" runat="server" GroupName="UrlSiteJs" />      
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <ul>
            <li>All the css is served from one file that is hosted at a remote location.</li>
            <li>All the js is served from one file that is hosted at a remote location.</li>
            <li>A hash is appended via a querystring to ensure that updates to the files will result in the file being re-served to client.</li>
            <li>Updating any css or js file will result in the crushed file being regenerated and a new hash being created.</li>
            <li>You need to manually copy any newly created local crushed files to the remote location.</li>
            <li>Use the url property to host content on CDN. e.g. "http://cdn.net/1234/site.css" or "http://cdn.net/1234/site.js" the hash is appended to the end of the url, the hash is based off the local crushed file.</li>
        </ul>
    </div>
    
    <input type="button" onclick="TestMessage()" value="Test" />
    </form>
</body>
</html>
