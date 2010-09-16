namespace StaticFileGenericHandler.Demo
{
    public partial class Default : System.Web.UI.Page
    {
        protected override void Render(System.Web.UI.HtmlTextWriter writer)
        {
            DownloadWithTimestampHyperLink.NavigateUrl = "~/StaticFile.ashx?time=" + System.DateTime.Now.Ticks;
            base.Render(writer);
        }
    }
}