using System;
using System.Web;

namespace Talifun.Web.Module
{
    public abstract class HttpModuleBase : IHttpModule
    {        
        void IHttpModule.Init(HttpApplication httpApplication)
        {
            if (httpApplication == null)
                throw new ArgumentNullException("httpApplication");

            if (SupportDiscoverability)
                HttpModuleRegistry.RegisterInPartialTrust(httpApplication, this);

            OnInit(httpApplication);
        }

        void IHttpModule.Dispose()
        {
            OnDispose();
        }

        /// <summary>
        /// Determines whether the module will be registered for discovery
        /// in partial trust environments or not.
        /// </summary>
        protected virtual bool SupportDiscoverability
        {
            get { return false; }
        }

        /// <summary>
        /// Initializes the module and prepares it to handle requests.
        /// </summary>

        protected virtual void OnInit(HttpApplication httpApplication) { }

        /// <summary>
        /// Disposes of the resources (other than memory) used by the module.
        /// </summary>

        protected virtual void OnDispose() { }
    }
}