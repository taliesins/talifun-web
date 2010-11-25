using System;
using System.Reflection;
using System.Web;
using Talifun.Web.Module;

namespace Talifun.Web.IpAddressAuthentication
{
    public class IpAddressAuthenticationModule : HttpModuleBase
    {
        #region IHttpModule Members

        /// <summary>
        /// We want to initialize the ip address authentication manager.
        /// </summary>
        private static readonly IpAddressAuthenticationManager IpAddressAuthenticationManager = IpAddressAuthenticationManager.Instance;

        private static readonly MethodInfo GetErrorTextMethod =
typeof(System.Web.Configuration.UrlMapping).Assembly.GetType("System.Web.Configuration.UrlAuthFailedErrorFormatter").GetMethod("GetErrorText",
BindingFlags.Static | BindingFlags.NonPublic, null, new Type[] { }, null);

        private static readonly MethodInfo GenerateResponseHeadersForHandlerMethod =
typeof(HttpResponse).GetMethod("GenerateResponseHeadersForHandler",
BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        /// Determines whether the module will be registered for discovery
        /// in partial trust environments or not.
        /// </summary>
        protected override bool SupportDiscoverability
        {
            get { return true; }
        }

        protected override void OnInit(HttpApplication httpApplication)
        {
            httpApplication.AuthenticateRequest += new EventHandler(this.OnEnter);
        }

        private void OnEnter(object source, EventArgs e)
        {
            var application = (HttpApplication)source;
            var context = application.Context;

            var rawUrl = context.Request.RawUrl;
            var userHostAddress = context.Request.UserHostAddress;

            if (IpAddressAuthenticationManager.IsAuthorized(rawUrl, userHostAddress))
            {
                return;
            }

            context.Response.StatusCode = 403; // (Forbidden)
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

        #endregion
    }
}
