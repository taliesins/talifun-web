using System.IO;
using System.Web;
using Talifun.Web.StaticFile;

namespace StaticFileGenericHandler.Demo
{
    /// <summary>
    /// Summary description for StaticFile
    /// </summary>
    public class StaticFile : IHttpHandler
    {

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

            var fileInfo = new FileInfo(HttpContext.Current.Server.MapPath("~/Static/test.zip"));

            httpContext.Response.AddHeader("Content-Disposition", string.Format("attachment;filename=\"{0}\";", fileInfo.Name));
            StaticFileManager.Instance.ProcessRequest(new HttpContextWrapper(httpContext), fileInfo);
        }
    }
}