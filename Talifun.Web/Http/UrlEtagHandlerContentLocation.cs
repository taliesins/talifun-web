using System;
using System.Web;

namespace Talifun.Web
{
    public class UrlEtagHandlerContentLocation : IUrlEtagHandler
    {
        protected readonly IHttpResponseHeaderHelper HttpResponseHeaderHelper;
        protected readonly string UrlEtagQuerystringName;

        public UrlEtagHandlerContentLocation(IHttpResponseHeaderHelper httpResponseHeaderHelper, string urlEtagQuerystringName)
        {
            HttpResponseHeaderHelper = httpResponseHeaderHelper;
            UrlEtagQuerystringName = urlEtagQuerystringName;
        }

        public bool UpdateEtag(HttpResponseBase response, Uri uri, string etag)
        {
            var newLocation = new UriBuilder(uri);
            newLocation.EditQueryArgument(UrlEtagQuerystringName, etag);
            HttpResponseHeaderHelper.SetContentLocation(response, newLocation.Uri);

            return false;
        }
    }
}