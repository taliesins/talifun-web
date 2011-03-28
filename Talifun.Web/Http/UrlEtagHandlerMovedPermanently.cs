using System;
using System.Web;

namespace Talifun.Web
{
    public class UrlEtagHandlerMovedPermanently : IUrlEtagHandler
    {
        protected readonly IHttpResponseHeaderHelper HttpResponseHeaderHelper;
        protected readonly string UrlEtagQuerystringName;

        public UrlEtagHandlerMovedPermanently(IHttpResponseHeaderHelper httpResponseHeaderHelper, string urlEtagQuerystringName)
        {
            HttpResponseHeaderHelper = httpResponseHeaderHelper;
            UrlEtagQuerystringName = urlEtagQuerystringName;
        }

        public bool UpdateEtag(HttpResponseBase response, Uri uri, string etag)
        {
            var newLocation = new UriBuilder(uri);
            newLocation.EditQueryArgument(UrlEtagQuerystringName, etag);
            HttpResponseHeaderHelper.SetMovedPermanently(response, newLocation.Uri);

            return true;
        }
    }
}
