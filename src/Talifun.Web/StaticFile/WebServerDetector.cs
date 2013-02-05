using System;
using System.Diagnostics;
using System.Reflection;
using System.Security.Permissions;
using System.Web;

namespace Talifun.Web.StaticFile
{
    public class WebServerDetector
    {
        protected static WebServerType WebServerType;
        protected static Type HttpWorkerRequestType = typeof(HttpWorkerRequest);

        static WebServerDetector()
        {
            WebServerType = WebServerType.NotSet;
        }

        /// <summary>
        /// Return the http worker request for the current request.
        /// </summary>
        /// <remarks>
        /// This is needed for when we manually create an HttpContext.
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [ReflectionPermission(SecurityAction.Assert, RestrictedMemberAccess = true)]
        public static HttpWorkerRequest GetWorkerRequestViaReflection(HttpRequestBase request)
        {
            const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic;

            // In Mono, the field has a different name.
            var wrField = request.GetType().GetField("_wr", bindingFlags) ?? request.GetType().GetField("worker_request", bindingFlags);

            if (wrField == null) return null;

            return (HttpWorkerRequest)wrField.GetValue(request);
        }

        /// <summary>
        /// Detect the web server being used to serve requests
        /// </summary>
        /// <param name="context">Http context</param>
        [SecurityPermission(SecurityAction.Assert, UnmanagedCode = true)]
        public static WebServerType DetectWebServerType(HttpContextBase context)
        {
            if (WebServerType != WebServerType.NotSet)
            {
                return WebServerType;
            }

            var provider = (IServiceProvider)context;
            var worker = (HttpWorkerRequest)provider.GetService(HttpWorkerRequestType) ?? GetWorkerRequestViaReflection(context.Request);
     
            if (worker != null)
            {
                var workerType = worker.GetType();
                if (workerType != null)
                {
                    switch (workerType.FullName)
                    {
                        case "System.Web.Hosting.ISAPIWorkerRequest":
                            //IIS 7 in Classic mode gets lumped in here too
                            WebServerType = WebServerType.IIS6orIIS7ClassicMode;
                            break;
                        case "Microsoft.VisualStudio.WebHost.Request":
                            {
                                var version = workerType.Assembly.GetName().Version.Major;
                                if (version >= 11)
                                {
                                    WebServerType = WebServerType.VisualStudio2012;
                                }
                                else if (version > 10)
                                {
                                    WebServerType = WebServerType.VisualStudio2010;
                                }
                                else if (version > 9)
                                {
                                    WebServerType = WebServerType.VisualStudio2008;
                                }
                                else
                                {
                                    WebServerType = WebServerType.Cassini;
                                }
                                break;
                            }
                        case "Cassini.Request":
                            {
                                var version = workerType.Assembly.GetName().Version.Major;
                                if (version > 9)
                                {
                                    WebServerType = WebServerType.IIS7;
                                }
                                else
                                {
                                    WebServerType = WebServerType.Cassini;
                                }
                                break;
                            }
                        case "System.Web.Hosting.IIS7WorkerRequest":
                            WebServerType = WebServerType.IIS7;
                            break;
                        default:
                            WebServerType = WebServerType.Unknown;
                            break;
                    }
                }
            }

            if (WebServerType == WebServerType.NotSet)
            {
                WebServerType = WebServerType.Unknown;
            }

            return WebServerType;
        }
    }
}
