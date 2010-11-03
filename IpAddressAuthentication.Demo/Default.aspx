<%@ Page Language="C#" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head id="Head1" runat="server">
    </script>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <ul>
            <li>
                <asp:HyperLink ID="ShowPdfHyperLink" runat="server" NavigateUrl="~/test.pdf" Text="Show Pdf" />
            </li>
            <li>
                <asp:HyperLink ID="ShowTextHyperLink" runat="server" NavigateUrl="~/test.txt" Text="Show Txt" />            
            </li>
        </ul>        
    </div>
    </form>
</body>