using System.Web;

namespace Talifun.Web.StaticFile
{
    /// <summary>
    /// A http handler to serve static content in an efficient way i.e. cached, compressed and resumable
    /// </summary>
    public class StaticFileHandler : IHttpHandler
    {
        #region IHttpHandler Members

        public bool IsReusable
        {
            get { return true; }
        }

        public void ProcessRequest(HttpContext httpContext)
        {
            if (HttpContext.Current == null)
            {
                HttpContext.Current = httpContext;
            }

            StaticFileManager.Instance.ProcessRequest(new HttpContextWrapper(httpContext));
        }

        #endregion
    }
}