using System;
using Talifun.Web.LogUrl;

namespace LogModule.Example
{
    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            LogUrlManager.Instance.LogUrlEvent += OnLogUrlEvent;
        }

        protected void Application_End(object sender, EventArgs e)
        {
            LogUrlManager.Instance.LogUrlEvent -= OnLogUrlEvent;
        }

        protected static void OnLogUrlEvent(object sender, LogUrlEventArgs args)
        {
            //Usually log request to database. Information to include is User, Url, Date/Time etc
            var request = args.HttpApplication.Request;

            var referrer = string.Empty;
            if (request.UrlReferrer != null)
            {
                referrer = request.UrlReferrer.PathAndQuery;
            }

            var url = request.Path;
            var querystring = request.QueryString;

            var userAgent = request.UserAgent;
        }
    }
}