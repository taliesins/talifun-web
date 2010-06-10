using System;
using System.Collections.Generic;
using System.Net;
using System.Web;

namespace Talifun.Web
{
    public interface IHttpRequestHeaderHelper
    {
        /// <summary>
        /// Get the value for an http header. 
        /// </summary>
        /// <param name="request">The http request.</param>
        /// <param name="httpRequestHeader">The header to get the value for.</param>
        /// <param name="defaultValue">The value to return should the header not exist in the request.</param>
        /// <returns>If the header exists return the header value; else return the default value specified.</returns>
        string GetHttpHeaderValue(HttpRequestBase request, HttpRequestHeader httpRequestHeader, string defaultValue);

        /// <summary>
        /// Get the value for an http header. 
        /// </summary>
        /// <param name="request">The http request.</param>
        /// <param name="httpRequestHeader">The header to get the value for.</param>
        /// <param name="defaultValue">The value to return should the header not exist in the request.</param>
        /// <returns>If the header exists return the header value; else return the default value specified.</returns>
        string GetHttpHeaderValue(HttpRequestBase request, string httpRequestHeader, string defaultValue);

        /// <summary>
        /// Get the identites for an http header.
        /// </summary>
        /// <param name="request">The http request.</param>
        /// <param name="httpRequestHeader">The header to get the value for.</param>
        /// <returns>Identities.</returns>
        /// <remarks>Supports quoted identities.</remarks>
        List<string> GetHttpHeaderValues(HttpRequestBase request, HttpRequestHeader httpRequestHeader);

        /// <summary>
        /// Get the identites for an http header.
        /// </summary>
        /// <param name="request">The http request.</param>
        /// <param name="httpRequestHeader">The header to get the value for.</param>
        /// <returns>Identities.</returns>
        /// <remarks>Supports quoted identities.</remarks>
        List<string> GetHttpHeaderValues(HttpRequestBase request, string httpRequestHeader);

        /// <summary>
        /// Get the identities and q values for an http header.
        /// </summary>
        /// <param name="request">The http request.</param>
        /// <param name="httpRequestHeader">The header to get the value for.</param>
        /// <returns>Identies with QValues</returns>
        /// <remarks>Supports quoted identities.</remarks>
        List<HttpHeaderValue> GetHttpHeaderWithQValues(HttpRequestBase request, HttpRequestHeader httpRequestHeader);

        /// <summary>
        /// Get the value and q values for an http header.
        /// </summary>
        /// <param name="request">The http request.</param>
        /// <param name="httpRequestHeader">The header to get the value for.</param>
        /// <returns>Identies with QValues</returns>
        /// <remarks>Supports quoted identities.</remarks>
        List<HttpHeaderValue> GetHttpHeaderWithQValues(HttpRequestBase request, string httpRequestHeader);

        /// <summary>
        /// Get compression mode for the request.
        /// </summary>
        /// <param name="request">The http request.</param>
        /// <returns>The compression mode to use.</returns>
        ResponseCompressionType GetCompressionMode(HttpRequestBase request);

        /// <summary>
        /// Get the http method used for the request.
        /// </summary>
        /// <param name="request">The http request.</param>
        /// <returns>The http method to use.</returns>
        HttpMethod GetHttpMethod(HttpRequestBase request);

        /// <summary>
        /// Check if request is a range request.
        /// </summary>
        /// <param name="request">The http request.</param>
        /// <returns>True if it is a range request; else false</returns>
        bool IsRangeRequest(HttpRequestBase request);

        /// <summary>
        /// Checks the If-Modified header if it was sent with the request.
        /// </summary>
        /// <param name="request">An HTTP request.</param>
        /// <param name="lastModified">The last modified date for the file requested.</param>
        /// <returns>
        /// Returns Null, if no header was sent or unable to parse incoming date; 
        /// Returns True, if the file was modified since the indicated date (RFC 1123 format); 
        /// returns False, if the file was not modified since the indicated date.
        /// </returns>
        bool? CheckIfModifiedSince(HttpRequestBase request, DateTime lastModified);

        /// <summary>
        /// Checks the If-Unmodified header, if it was sent with the request.
        /// </summary>
        /// <param name="request">An HTTP request.</param>
        /// <param name="lastModified">The last modified date for the file requested.</param>
        /// <returns>
        /// Returns Null, if no header was sent or unable to parse incoming date;
        /// Returns True, if the file has not been modified since the indicated date (RFC 1123 format);
        /// Returns False, if the file has been modified since the indicated date or .
        /// </returns>
        bool? CheckIfUnmodifiedSince(HttpRequestBase request, DateTime lastModified);

        /// <summary>
        /// Checks the Unless-Modified-Since header, if it was sent with the request.
        /// </summary>
        /// <param name="request">An HTTP request.</param>
        /// <param name="lastModified">The last modified date for the file requested.</param>
        /// <returns>
        /// Returns Null, if no header was sent or unable to parse incoming date;
        /// Returns True, if the file has not been modified since the indicated date (RFC 1123 format);
        /// Returns False, if the file has been modified since the indicated date.
        /// </returns>
        bool? CheckUnlessModifiedSince(HttpRequestBase request, DateTime lastModified);

        /// <summary>
        /// Checks the If-Range header if it was sent with the request.
        /// </summary>
        /// <param name="request">An HTTP request.</param>
        /// <param name="entityTag">The entity tag for the file requested.</param>
        /// <param name="lastModified">The last modified date for the file requested.</param>
        /// <returns>
        /// Returns Null, if no header was sent or no range header was sent; 
        /// Returns True, if the header value matches the file's entity tag or if the file was 
        /// modified since the indicated date (RFC 1123 format);
        /// returns False, if the header values does not match the file's entity tag or if the file was 
        /// modified since the indicated date (RFC 1123 format);
        /// </returns>
        /// <remarks>
        /// The If-Range header SHOULD only be used together with a Range header, 
        /// and MUST be ignored if the request does not include a Range header, 
        /// or if the server does not support the sub-range operation. 
        /// </remarks>
        bool? CheckIfRange(HttpRequestBase request, string entityTag, DateTime lastModified);

        /// <summary>
        /// Checks the If-Match header if it was sent with the request.
        /// </summary>
        /// <param name="request">An HTTP request.</param>
        /// <param name="entityTag">The entity tag for the file requested.</param>
        /// <param name="doesEntityExist">Does the entity to serve exist. i.e. does file exist.</param>
        /// <returns>
        /// Returns Null, if no header was sent;
        /// Returns True, if one of the header values matches the file's entity tag; 
        /// Returns False, if none of the header values matches the file's entity tag
        /// header was sent.
        /// </returns>
        bool? CheckIfMatch(HttpRequestBase request, string entityTag, bool doesEntityExist);

        /// <summary>
        /// Checks the If-None-Match header if it was sent with the request.
        /// </summary>
        /// <param name="request">An HTTP request.</param>
        /// <param name="entityTag">The entity tag for the file requested.</param>
        /// <param name="doesEntityExist">Does the entity to serve exist. i.e. does file exist.</param>
        /// <returns>
        /// Returns Null, if no header was sent;
        /// Returns False, if one of the header values matches the file's entity tag, or if "*" was sent 
        /// Returns True, if it does not match the file;
        /// </returns>
        bool? CheckIfNoneMatch(HttpRequestBase request, string entityTag, bool doesEntityExist);

        /// <summary>
        /// Parses the Range Header from the http request.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="contentLength">The length of the content to serve.</param>
        /// <param name="ranges">A list of ranges</param>
        /// <returns>
        /// Returns Null, if there is no Range Header in the http request
        /// Return False, if there are unsatisfiable Range Headers in the http request (416 Requested Range Not Satisfiable)
        /// Returns True, if there are Range Headers in the http request
        /// </returns>
        bool? GetRanges(HttpRequestBase request, long contentLength, out IEnumerable<RangeItem> ranges);
    }
}