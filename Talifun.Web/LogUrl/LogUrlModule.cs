using System;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using Talifun.Web.LogUrl.Config;
using Talifun.Web.Module;

namespace Talifun.Web.LogUrl
{
    /// <summary>
    /// Module that is used to raise events that can be handled when urls, based on a configuration provided, are requested.
    /// </summary>
    public class LogUrlModule : HttpModuleBase
    {
        private UrlMatchElementCollection urlMatches = CurrentLogUrlConfiguration.Current.UrlMatches;
        private const RegexOptions regxOptions = RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Singleline;

        protected override void OnInit(HttpApplication httpApplication)
        {
            httpApplication.EndRequest += new EventHandler(OnEndRequest);
        }

        protected virtual void OnEndRequest(object sender, EventArgs e)
        {
            var application = (HttpApplication)sender;

            //We only want to log request to url if they are successfull and lets also ignore partial requests
            if (application.Context.Error != null || !(application.Response.StatusCode == (int)HttpStatusCode.OK || application.Response.StatusCode == (int)HttpStatusCode.NotModified)) return;

            var rawUrl = application.Request.RawUrl;
            foreach (UrlMatchElement urlMatch in urlMatches)
            {
                if (!Regex.IsMatch(rawUrl, urlMatch.Expression, regxOptions)) continue;

                LogUrlManager.Instance.RaiseLogUrlEvent(application, urlMatch.Expression);

                break;
            }
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