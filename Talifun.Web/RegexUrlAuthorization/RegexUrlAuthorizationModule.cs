using System;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using Talifun.Web.Module;
using Talifun.Web.RegexUrlAuthorization.Config;

namespace Talifun.Web.RegexUrlAuthorization
{
    /// <summary>
    /// Module that is used to provide authorization on urls, that match regular expressions, based on a configuration provided.
    /// </summary>
    /// <remarks>
    /// It uses the asp.net "authorization" web.config rules, to check if a user is authorized. 
    /// </remarks>
    public class RegexUrlAuthorizationModule : HttpModuleBase
    {
        private UrlMatchElementCollection urlMatches = CurrentRegexUrlAuthorizationConfiguration.Current.UrlMatches;
        private const RegexOptions regxOptions = RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Singleline;

        protected override void OnInit(HttpApplication httpApplication)
        {
            httpApplication.AuthorizeRequest += new EventHandler(this.OnEnter);
        }

        private void OnEnter(object source, EventArgs eventArgs)
        {
            var application = (HttpApplication)source;
            var context = application.Context;
            if (context.SkipAuthorization) return;

            var rawUrl = application.Request.RawUrl;
            UrlMatchElement urlMatched = null;
            foreach (UrlMatchElement urlMatch in urlMatches)
            {
                if (!Regex.IsMatch(rawUrl, urlMatch.Expression, regxOptions)) continue;
                urlMatched = urlMatch;
                break;
            }

            if (urlMatched == null) return;

            if (urlMatched.EveryoneAllowed || urlMatched.IsUserAllowed(context.User, context.Request.RequestType)) return;

            context.Response.StatusCode = 401;
            this.WriteErrorMessage(context);
            application.CompleteRequest();
        }

        private static MethodInfo getErrorTextMethod = typeof(System.Web.Configuration.UrlMapping).Assembly.GetType("System.Web.Configuration.UrlAuthFailedErrorFormatter").GetMethod("GetErrorText", BindingFlags.Static | BindingFlags.NonPublic, null, new Type[] { }, null);
        private static MethodInfo generateResponseHeadersForHandlerMethod = typeof(HttpResponse).GetMethod("GenerateResponseHeadersForHandler", BindingFlags.Instance | BindingFlags.NonPublic);
        private void WriteErrorMessage(HttpContext context)
        {
            //context.Response.Write(UrlAuthFailedErrorFormatter.GetErrorText());
            //context.Response.GenerateResponseHeadersForHandler();

            context.Response.Write(getErrorTextMethod.Invoke(null, null));
            generateResponseHeadersForHandlerMethod.Invoke(context.Response, null);
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
