using System;
using System.Net;
using System.Web;

namespace Talifun.Web
{
    public interface IHttpResponseHeaderHelper
    {
        /// <summary>
        /// Set the Status Code and Status Description of the http response.
        /// </summary>
        /// <param name="response">The Http Response.</param>
        /// <param name="httpStatus">The status to set.</param>
        void SendHttpStatusHeaders(HttpResponseBase response, HttpStatus httpStatus);

        /// <summary>
        /// Append header to response
        /// </summary>
        /// <remarks>
        /// Seems like appendheader only works with IIS 7
        /// </remarks>
        /// <param name="response"></param>
        /// <param name="headerName"></param>
        /// <param name="headerValue"></param>
        void AppendHeader(HttpResponseBase response, HttpResponseHeader httpResponseHeader, string headerValue);

        /// <summary>
        /// Append header to response
        /// </summary>
        /// <remarks>
        /// Seems like appendheader only works with IIS 7
        /// </remarks>
        /// <param name="response"></param>
        /// <param name="httpResponseHeader"></param>
        /// <param name="headerValue"></param>
        void AppendHeader(HttpResponseBase response, string httpResponseHeader, string headerValue);

        /// <summary>
        /// Set the compression type used in the response.
        /// </summary>
        /// <param name="response">An HTTP response.</param>
        /// <param name="responseCompressionType">The compression type to use in the response.</param>
        void SetContentEncoding(HttpResponseBase response, ResponseCompressionType responseCompressionType);

        /// <summary>
        /// Tell the browser that it supports resumable requests.
        /// </summary>
        /// <param name="response">An HTTP response.</param>
        void SetResponseResumable(HttpResponseBase response);

        /// <summary>
        /// Make the response cachable by browser and any proxies in between.
        /// </summary>
        /// <param name="response">An HTTP response.</param>
        /// <param name="now">The current date time of the system.</param>
        /// <param name="lastModified">The last modified date of the entity.</param>
        /// <param name="etag">The etag of the entity.</param>
        /// <param name="maxAge">The time the entity should live before browser will recheck the freshness of the entity.</param>
        void SetResponseCachable(HttpResponseBase response, DateTime now, DateTime lastModified, string etag, TimeSpan maxAge);
    }
}