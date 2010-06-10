using System;
using UrlReWriter.Demo.CustomUrlRewriter;

namespace UrlRewriter.Demo
{
    public partial class _Default : System.Web.UI.Page
    {
        protected void ClearCacheButton_Click(object sender, EventArgs e)
        {
            StaticUrlTransform.RefreshCache();
        }
    }
}
