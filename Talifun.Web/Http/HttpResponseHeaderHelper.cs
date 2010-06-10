using System;
using System.Web;
using System.Net;

namespace Talifun.Web
{
    public class HttpResponseHeaderHelper : IHttpResponseHeaderHelper
    {
        public const string HTTP_HEADER_ACCEPT_RANGES_BYTES = "bytes";
        protected WebServerType WebServerType { get; private set; }

        public HttpResponseHeaderHelper(WebServerType webServerType)
        {
            WebServerType = webServerType;
        }

        /// <summary>
        /// Set the Status Code and Status Description of the http response.
        /// </summary>
        /// <param name="response">The Http Response.</param>
        /// <param name="httpStatus">The status to set.</param>
        public void SendHttpStatusHeaders(HttpResponseBase response, HttpStatus httpStatus)
        {
            response.StatusCode = (int)StringifyHttpHeaders.HttpStatusCodeFromHttpStatus(httpStatus);
            response.StatusDescription = StringifyHttpHeaders.StringFromHttpStatus(httpStatus);
        }

        /// <summary>
        /// Append header to response
        /// </summary>
        /// <remarks>
        /// Seems like appendheader only works with IIS 7
        /// </remarks>
        /// <param name="response"></param>
        /// <param name="headerName"></param>
        /// <param name="headerValue"></param>
        public void AppendHeader(HttpResponseBase response, HttpResponseHeader httpResponseHeader, string headerValue)
        {
            var httpResponseHeaderString = StringifyHttpHeaders.StringFromResponseHeader(httpResponseHeader);
            AppendHeader(response, httpResponseHeaderString, headerValue);
        }

        /// <summary>
        /// Append header to response
        /// </summary>
        /// <remarks>
        /// Seems like appendheader only works with IIS 7
        /// </remarks>
        /// <param name="response"></param>
        /// <param name="httpResponseHeader"></param>
        /// <param name="headerValue"></param>
        public void AppendHeader(HttpResponseBase response, string httpResponseHeader, string headerValue)
        {
            switch (WebServerType)
            {
                case WebServerType.IIS7:
                    response.AppendHeader(httpResponseHeader, headerValue);
                    break;
                default:
                    response.AddHeader(httpResponseHeader, headerValue);
                    break;
            }
        }

        /// <summary>
        /// Set the compression type used in the response.
        /// </summary>
        /// <param name="response">An HTTP response.</param>
        /// <param name="responseCompressionType">The compression type to use in the response.</param>
        public void SetContentEncoding(HttpResponseBase response, ResponseCompressionType responseCompressionType)
        {
            if (responseCompressionType != ResponseCompressionType.None)
            {
                AppendHeader(response, HttpResponseHeader.ContentEncoding, responseCompressionType.ToString().ToLower());
            }
        }

        /// <summary>
        /// Tell the browser that it supports resumable requests.
        /// </summary>
        /// <param name="response">An HTTP response.</param>
        public void SetResponseResumable(HttpResponseBase response)
        {
            // Tell the client software that we accept Range request
            AppendHeader(response, HttpResponseHeader.AcceptRanges, HTTP_HEADER_ACCEPT_RANGES_BYTES);
        }

        /// <summary>
        /// Make the response cachable by browser and any proxies in between.
        /// </summary>
        /// <param name="response">An HTTP response.</param>
        /// <param name="now">The current date time of the system.</param>
        /// <param name="lastModified">The last modified date of the entity.</param>
        /// <param name="etag">The etag of the entity.</param>
        /// <param name="maxAge">The time the entity should live before browser will recheck the freshness of the entity.</param>
        public void SetResponseCachable(HttpResponseBase response, DateTime now, DateTime lastModified, string etag, TimeSpan maxAge)
        {
            //Set the expires header for HTTP 1.0 cliets
            response.Cache.SetExpires(now.Add(maxAge));

            //Proxy and browser can cache response
            response.Cache.SetCacheability(HttpCacheability.Public);

            //Proxy cache should check with orginal server once cache has expired
            response.Cache.AppendCacheExtension("must-revalidate, proxy-revalidate");

            //The date the entity was last modified
            response.Cache.SetLastModified(lastModified);

            //The unique identifier for the entity
            response.Cache.SetETag(etag);

            //How often the browser should check that it has the latest version
            response.Cache.SetMaxAge(maxAge);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="response">An HTTP response.</param>
        /// <param name="contentType">The content type of the entity.</param>
        public void SetContentType(HttpResponseBase response, string contentType)
        {
            response.ContentType = contentType;
        }
    }
}
