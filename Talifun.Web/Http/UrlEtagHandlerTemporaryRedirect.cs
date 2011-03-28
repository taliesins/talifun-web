using System;
using System.Web;

namespace Talifun.Web
{
    public class UrlEtagHandlerTemporaryRedirect : IUrlEtagHandler
    {
        protected readonly IHttpResponseHeaderHelper HttpResponseHeaderHelper;
        protected readonly string UrlEtagQuerystringName;

        public UrlEtagHandlerTemporaryRedirect(IHttpResponseHeaderHelper httpResponseHeaderHelper, string urlEtagQuerystringName)
        {
            HttpResponseHeaderHelper = httpResponseHeaderHelper;
            UrlEtagQuerystringName = urlEtagQuerystringName;
        }

        public bool UpdateEtag(HttpResponseBase response, Uri uri, string etag)
        {
            var newLocation = new UriBuilder(uri);
            newLocation.EditQueryArgument(UrlEtagQuerystringName, etag);
            HttpResponseHeaderHelper.SetTemporaryRedirect(response, newLocation.Uri);

            return true;
        }
    }
}
