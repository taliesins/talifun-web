using System;
using System.Reflection;
using System.Web;
using Talifun.Web.Module;

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
        /// <summary>
        /// We want to initialize the ip address authentication manager.
        /// </summary>
        private static readonly RegexUrlAuthorizationManager RegexUrlAuthorizationManager = RegexUrlAuthorizationManager.Instance;

        private static readonly MethodInfo GetErrorTextMethod = typeof(System.Web.Configuration.UrlMapping).Assembly.GetType("System.Web.Configuration.UrlAuthFailedErrorFormatter").GetMethod("GetErrorText", BindingFlags.Static | BindingFlags.NonPublic, null, new Type[] { }, null);
        private static readonly MethodInfo GenerateResponseHeadersForHandlerMethod = typeof(HttpResponse).GetMethod("GenerateResponseHeadersForHandler", BindingFlags.Instance | BindingFlags.NonPublic);
        
        protected override void OnInit(HttpApplication httpApplication)
        {
            httpApplication.AuthorizeRequest += new EventHandler(OnEnter);
        }

        private static void OnEnter(object source, EventArgs eventArgs)
        {
            var application = (HttpApplication)source;
            var context = application.Context;
            if (context.SkipAuthorization) return;

            var rawUrl = context.Request.RawUrl;
            var user = context.User;
            var requestType = context.Request.RequestType;

            if (RegexUrlAuthorizationManager.IsAuthorized(rawUrl, user, requestType))
            {
                return;
            }

            context.Response.StatusCode = 401;
            WriteErrorMessage(context);
            application.CompleteRequest();
        }

        private static void WriteErrorMessage(HttpContext context)
        {
            //context.Response.Write(UrlAuthFailedErrorFormatter.GetErrorText());
            //context.Response.GenerateResponseHeadersForHandler();

            context.Response.Write(GetErrorTextMethod.Invoke(null, null));
            GenerateResponseHeadersForHandlerMethod.Invoke(context.Response, null);
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
