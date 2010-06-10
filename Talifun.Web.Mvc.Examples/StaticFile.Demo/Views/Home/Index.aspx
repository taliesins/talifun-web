<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ID="title" ContentPlaceHolderID="TitleContentPlaceHolder" runat="server"></asp:Content>
<asp:Content ID="head" ContentPlaceHolderID="HeadContentPlaceHolder" runat="server">
    <link id="Link1" href="<%= Url.Action("GetImage") %>" rel="stylesheet" type="text/css" />
</asp:Content>

<asp:Content ID="body" ContentPlaceHolderID="MainContentPlaceHolder" runat="server">
Resumable downloads will only work with web server that you can modify the headers on.
        So it will work on IIS but not on Cassini.
    
        <img src="/doesnotexist1.jpg" width="100px" height="100px" />
        <img src="/doesnotexist2.jpg" width="100px" height="100px" />
        <img src="/doesnotexist3.jpg" width="100px" height="100px" />
        <img src="/doesnotexist4.jpg" width="100px" height="100px" />
        <img src="/doesnotexist5.jpg" width="100px" height="100px" />
        <img src="/doesnotexist6.jpg" width="100px" height="100px" />
        <img src="/doesnotexist7.jpg" width="100px" height="100px" />
        
        <img src="<%= Url.Action("GetImage") %>" />
        
        <img src="<%= Url.Content("~/Static/Images/Main.jpg") %>" />
        
        <%=Html.ActionLink("Download", "GetTestZip")%>
</asp:Content>
