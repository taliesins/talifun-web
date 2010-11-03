using System;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using Talifun.Web.IpAddressAuthentication.Config;
using Talifun.Web.Module;

namespace Talifun.Web.IpAddressAuthentication
{
    public class IpAddressAuthenticationModule : HttpModuleBase
    {
        #region IHttpModule Members
        private UrlMatchElementCollection urlMatches = CurrentIpAddressAuthenticationConfiguration.Current.UrlMatches;

        private const RegexOptions regxOptions = RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Singleline;
        internal static string ipAddressAuthenticationModuleType = typeof(IpAddressAuthenticationModule).ToString();

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

            var rawUrl = application.Request.RawUrl;
            UrlMatchElement urlMatched = null;
            foreach (UrlMatchElement urlMatch in urlMatches)
            {
                if (!Regex.IsMatch(rawUrl, urlMatch.Expression, regxOptions)) continue;
                urlMatched = urlMatch;
                break;
            }

            if (urlMatched == null) return;

            var ipAddress = IPAddress.Parse(context.Request.UserHostAddress);
            if (IsValidIpAddress(urlMatched, ipAddress)) return;

            context.Response.StatusCode = 403; // (Forbidden)
            this.WriteErrorMessage(context);
            application.CompleteRequest();
        }

        private static bool IsValidIpAddress(UrlMatchElement urlMatched, IPAddress ipAddress)
        {
            foreach (IpAddressMatchElement ipAddressMatch in urlMatched.IpAddressMatches)
            {
                if (ipAddressMatch.NetMask == null)
                {
                    if (ipAddressMatch.IpAddress == ipAddress)
                    {
                        return ipAddressMatch.Access;
                    }
                } else
                {
                    if (IsAddressOnSubnet(ipAddress, ipAddressMatch.IpAddress, ipAddressMatch.NetMask))
                    {
                        return ipAddressMatch.Access;
                    }
                }
            }

            return urlMatched.DefaultAccess;
        }

        public static bool IsAddressOnSubnet(IPAddress address, IPAddress subnet, IPAddress mask)
        {
            var addrBytes = address.GetAddressBytes();
            var maskBytes = mask.GetAddressBytes();
            var maskedAddressBytes = new byte[addrBytes.Length];

            for (var i = 0; i < maskedAddressBytes.Length; ++i)
            {
                maskedAddressBytes[i] = (byte)(addrBytes[i] & maskBytes[i]);
            }

            var maskedAddress = new IPAddress(maskedAddressBytes);
            return subnet.Equals(maskedAddress);
        }

        private static MethodInfo getErrorTextMethod =
typeof(System.Web.Configuration.UrlMapping).Assembly.GetType("System.Web.Configuration.UrlAuthFailedErrorFormatter").GetMethod("GetErrorText",
BindingFlags.Static | BindingFlags.NonPublic, null, new Type[] { }, null);
        private static MethodInfo generateResponseHeadersForHandlerMethod =
typeof(HttpResponse).GetMethod("GenerateResponseHeadersForHandler",
BindingFlags.Instance | BindingFlags.NonPublic);
        private void WriteErrorMessage(HttpContext context)
        {

            //context.Response.Write(UrlAuthFailedErrorFormatter.GetErrorText());
            //context.Response.GenerateResponseHeadersForHandler();

            context.Response.Write(getErrorTextMethod.Invoke(null, null));
            generateResponseHeadersForHandlerMethod.Invoke(context.Response,
null);
        }

        #endregion
    }
}
