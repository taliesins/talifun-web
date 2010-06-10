using System;
using System.IO.Compression;
using System.Web;
using System.Web.UI;
using Talifun.Web.Module;

namespace Talifun.Web.Compress
{
    /// <summary>
    /// A module that can be used to compress the output from web pages. It will work for MVC too.
    /// </summary>
    public class PageCompressionModule : HttpModuleBase
    {
        protected override void OnInit(HttpApplication httpApplication)
        {
            httpApplication.PostAcquireRequestState += OnPostAcquireRequestState;
            httpApplication.EndRequest += OnEndRequest;
        }

        protected static void OnEndRequest(object sender, EventArgs e)
        {
            var httpApplication = (HttpApplication)sender;

            httpApplication.PostAcquireRequestState -= OnPostAcquireRequestState;
            httpApplication.EndRequest -= OnEndRequest;
        }

        protected static void OnPostAcquireRequestState(object sender, EventArgs e)
        {
            var httpContext = HttpContext.Current;

            // This part of the module compress only handlers from type System.Web.UI.Page
            // Other types such JavaScript or CSS files will be compressed in an httpHandler.
            // Here we check if the current handler if a Page, if so, we compress it.
            // Because there is a problem with async postbacks compression, we check here if the current request if an 'MS AJAX' call.
            // If so, we will not compress it.
            // Important !!! : I didn't check this module with another Ajax frameworks such 'infragistics' or 'SmartClient'.
            // probably you will have to change the IsAjaxPostBackRequest method.

            if (!(httpContext.CurrentHandler is Page || httpContext.CurrentHandler.GetType().BaseType.FullName == "System.Web.Mvc.MvcHandler") || CompressionModuleHelper.IsAjaxPostBackRequest(httpContext)) return;

            if (!CompressionModuleHelper.IsCompressionSupported(httpContext, true)) return;

            var response = httpContext.Response;

            // Check if GZIP is supported by the client
            if (CompressionModuleHelper.IsSpecificEncodingSupported(httpContext, CompressionModuleHelper.GZIP))
            {
                response.Filter = new GZipStream(response.Filter, CompressionMode.Compress);
                CompressionModuleHelper.SetEncodingType(response, CompressionModuleHelper.GZIP);
            }
                // If GZIP is not supported, so only DEFLATE is.
            else
            {
                response.Filter = new DeflateStream(response.Filter, CompressionMode.Compress);
                CompressionModuleHelper.SetEncodingType(response, CompressionModuleHelper.DEFLATE);
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