using System;
using System.Web;
using Talifun.Web.Module;

namespace Talifun.Web.LogUrl
{
    /// <summary>
    /// Module that is used to raise events that can be handled when urls, based on a configuration provided, are requested.
    /// </summary>
    public class LogUrlModule : HttpModuleBase
    {
        /// <summary>
        /// We want to initialize the crusher manager.
        /// </summary>
        protected static LogUrlManager LogUrlManager = LogUrlManager.Instance;

        protected override void OnInit(HttpApplication httpApplication)
        {
            httpApplication.EndRequest += new EventHandler(OnEndRequest);
        }

        protected virtual void OnEndRequest(object sender, EventArgs e)
        {
            var application = (HttpApplication)sender;

            //We only want to log request to url if they are successfull and lets also ignore partial requests
            if (application.Context.Error != null || !(application.Response.StatusCode == (int)HttpStatusCode.Ok || application.Response.StatusCode == (int)HttpStatusCode.NotModified)) return;

            LogUrlManager.LogUrl(application);
        }

        /// <summary>
        /// Determines whether the module will be registered for discovery
        /// in partial trust environments or not.
        /// </summary>
        protected override bool SupportDiscoverability
        {
            get { return true; }
        }
    }
}