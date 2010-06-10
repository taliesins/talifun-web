using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using System.Collections.Generic;

namespace Talifun.Web
{
    public class HttpRequestHeaderHelper : IHttpRequestHeaderHelper
    {
        public const string DEFLATE = "deflate";
        public const string GZIP = "gzip";
        public const string XGZIP = "x-gzip";
        public const string HTTP_HEADER_UNLESS_MODIFIED_SINCE = "Unless-Modified-Since";

        protected HeaderValueQValueComparer headerValueQValueComparer = new HeaderValueQValueComparer();

        /// <summary>
        /// Get the value for an http header. 
        /// </summary>
        /// <param name="request">The http request.</param>
        /// <param name="httpRequestHeader">The header to get the value for.</param>
        /// <param name="defaultValue">The value to return should the header not exist in the request.</param>
        /// <returns>If the header exists return the header value; else return the default value specified.</returns>
        public string GetHttpHeaderValue(HttpRequestBase request, HttpRequestHeader httpRequestHeader, string defaultValue)
        {
            var httpRequestHeaderString = StringifyHttpHeaders.StringFromRequestHeader(httpRequestHeader);
            return GetHttpHeaderValue(request, httpRequestHeaderString, defaultValue);
        }

        /// <summary>
        /// Get the value for an http header. 
        /// </summary>
        /// <param name="request">The http request.</param>
        /// <param name="httpRequestHeader">The header to get the value for.</param>
        /// <param name="defaultValue">The value to return should the header not exist in the request.</param>
        /// <returns>If the header exists return the header value; else return the default value specified.</returns>
        public string GetHttpHeaderValue(HttpRequestBase request, string httpRequestHeader, string defaultValue)
        {
            var result = request.Headers[httpRequestHeader];

            if (String.IsNullOrEmpty(result))
            {
                return defaultValue;
            }

            return result.Replace("\"", "");
        }

        /// <summary>
        /// Get the identites for an http header.
        /// </summary>
        /// <param name="request">The http request.</param>
        /// <param name="httpRequestHeader">The header to get the value for.</param>
        /// <returns>Identities.</returns>
        /// <remarks>Supports quoted identities.</remarks>
        public List<string> GetHttpHeaderValues(HttpRequestBase request, HttpRequestHeader httpRequestHeader)
        {
            var httpRequestHeaderString = StringifyHttpHeaders.StringFromRequestHeader(httpRequestHeader);
            return GetHttpHeaderValues(request, httpRequestHeaderString);
        }

        /// <summary>
        /// Get the identites for an http header.
        /// </summary>
        /// <param name="request">The http request.</param>
        /// <param name="httpRequestHeader">The header to get the value for.</param>
        /// <returns>Identities.</returns>
        /// <remarks>Supports quoted identities.</remarks>
        public List<string> GetHttpHeaderValues(HttpRequestBase request, string httpRequestHeader)
        {
            var httpRequestHeaderValues = request.Headers[httpRequestHeader];

            var regex = new Regex(@"(\""(?<identity>[^\""]+|\""\"")*\""|(?<identity>[^,]*))",
                                  RegexOptions.Compiled
                                  | RegexOptions.Singleline
                                  | RegexOptions.ExplicitCapture
                                  | RegexOptions.IgnorePatternWhitespace);

            var matches = regex.Matches(httpRequestHeaderValues);

            return (from Match match in matches
                    let identity = match.Groups["identity"].Value.Trim()
                    where !string.IsNullOrEmpty(identity)
                    select identity
                    ).ToList();
        }

        /// <summary>
        /// Get the identities and q values for an http header.
        /// </summary>
        /// <param name="request">The http request.</param>
        /// <param name="httpRequestHeader">The header to get the value for.</param>
        /// <returns>Identies with QValues</returns>
        /// <remarks>Supports quoted identities.</remarks>
        public List<HttpHeaderValue> GetHttpHeaderWithQValues(HttpRequestBase request, HttpRequestHeader httpRequestHeader)
        {
            var httpRequestHeaderString = StringifyHttpHeaders.StringFromRequestHeader(httpRequestHeader);
            return GetHttpHeaderWithQValues(request, httpRequestHeaderString);
        }

        /// <summary>
        /// Get the value and q values for an http header.
        /// </summary>
        /// <param name="request">The http request.</param>
        /// <param name="httpRequestHeader">The header to get the value for.</param>
        /// <returns>Identies with QValues</returns>
        /// <remarks>Supports quoted identities.</remarks>
        public List<HttpHeaderValue> GetHttpHeaderWithQValues(HttpRequestBase request, string httpRequestHeader)
        {
            var httpRequestHeaderValues = request.Headers[httpRequestHeader];

            var regex = new Regex(@"\s*(\""(?<identity>[^\""]+|\""\"")*\""|(?<identity>[^;,]*))\s*((\;\s*[q|Q]\s*=\s*(?<qValue>[1|0](\.\d)?))?)\s*",
                                  RegexOptions.Compiled
                                  | RegexOptions.Singleline
                                  | RegexOptions.ExplicitCapture
                                  | RegexOptions.IgnorePatternWhitespace);

            var matches = regex.Matches(httpRequestHeaderValues);

            return (from Match match in matches
                    let identity = match.Groups["identity"].Value.Trim()
                    let qValue = match.Groups["qValue"].Value.Trim()
                    where !string.IsNullOrEmpty(identity)
                    select new HttpHeaderValue
                               {
                                   Identity = identity, QValue = string.IsNullOrEmpty(qValue) ? null : (float?) Convert.ToSingle(qValue)
                               }).ToList();
        }

        /// <summary>
        /// Get compression mode for the request.
        /// </summary>
        /// <param name="request">The http request.</param>
        /// <returns>The compression mode to use.</returns>
        public ResponseCompressionType GetCompressionMode(HttpRequestBase request)
        {
            var acceptEncodingValues = GetHttpHeaderWithQValues(request, HttpRequestHeader.AcceptEncoding).OrderByDescending(x => x, headerValueQValueComparer);

            if (!acceptEncodingValues.Any()) return ResponseCompressionType.None;

            var acceptEncodingToUse = acceptEncodingValues.Where(x => (x.Identity.Equals(DEFLATE, StringComparison.InvariantCultureIgnoreCase) || x.Identity.Equals(GZIP, StringComparison.InvariantCultureIgnoreCase) || x.Identity.Equals(XGZIP, StringComparison.InvariantCultureIgnoreCase) || x.Identity == "*") && (!x.QValue.HasValue || x.QValue.Value > 0)).FirstOrDefault();

            if (acceptEncodingToUse == null) return ResponseCompressionType.None;

            if (acceptEncodingToUse.Identity == "*")
            {
                //Wildcard logic is everything that is not in the list, so we are assuming they can handle what ever we can send to them
                if (!acceptEncodingValues.Any(x => x.Identity.Equals(DEFLATE, StringComparison.InvariantCultureIgnoreCase))) return ResponseCompressionType.Deflate;
                if (!acceptEncodingValues.Any(x => x.Identity.Equals(GZIP, StringComparison.InvariantCultureIgnoreCase) || x.Identity.Equals(XGZIP, StringComparison.InvariantCultureIgnoreCase))) return ResponseCompressionType.GZip;

                //We tried our best to use wild card but we got no results, so see if we can send based on any other acceptable identities
                acceptEncodingToUse = acceptEncodingValues.Where(x => (x.Identity.Equals(DEFLATE, StringComparison.InvariantCultureIgnoreCase) || x.Identity.Equals(GZIP, StringComparison.InvariantCultureIgnoreCase) || x.Identity.Equals(XGZIP, StringComparison.InvariantCultureIgnoreCase)) && (!x.QValue.HasValue || x.QValue.Value > 0)).FirstOrDefault();
                if (acceptEncodingToUse == null) return ResponseCompressionType.None;
            }

            switch (acceptEncodingToUse.Identity.ToLowerInvariant())
            {
                case DEFLATE:
                    return ResponseCompressionType.Deflate;
                case GZIP:
                case XGZIP:
                    return ResponseCompressionType.GZip;
            }

            return ResponseCompressionType.None;
        }

        /// <summary>
        /// Get the http method used for the request.
        /// </summary>
        /// <param name="request">The http request.</param>
        /// <returns>The http method to use.</returns>
        public HttpMethod GetHttpMethod(HttpRequestBase request)
        {
            return StringifyHttpHeaders.HttpMethodFromString(request.HttpMethod);
        }

        /// <summary>
        /// Check if request is a range request.
        /// </summary>
        /// <param name="request">The http request.</param>
        /// <returns>True if it is a range request; else false</returns>
        public bool IsRangeRequest(HttpRequestBase request)
        {
            var requestHeaderRange = GetHttpHeaderValue(request, HttpRequestHeader.Range, string.Empty);
            return !string.IsNullOrEmpty(requestHeaderRange);
        }

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
        public bool? CheckIfModifiedSince(HttpRequestBase request, DateTime lastModified)
        {
            var requestHeaderIfModifiedSince = GetHttpHeaderValue(request, HttpRequestHeader.IfModifiedSince, string.Empty);

            if (string.IsNullOrEmpty(requestHeaderIfModifiedSince))
            {
                return null;
            }

            DateTime incomingLastModified;

            if (!DateTime.TryParse(requestHeaderIfModifiedSince, out incomingLastModified))
            {
                return null;
            }

            return (lastModified.ToUniversalTime() > incomingLastModified.ToUniversalTime());
        }

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
        public bool? CheckIfUnmodifiedSince(HttpRequestBase request, DateTime lastModified)
        {
            var requestHeaderIfUnmodifiedSince = GetHttpHeaderValue(request, HttpRequestHeader.IfUnmodifiedSince, string.Empty);

            if (string.IsNullOrEmpty(requestHeaderIfUnmodifiedSince))
            {
                return null;
            }

            DateTime incomingLastModified;

            if (!DateTime.TryParse(requestHeaderIfUnmodifiedSince, out incomingLastModified))
            {
                return null;
            }

            return (lastModified.ToUniversalTime() <= incomingLastModified.ToUniversalTime());
        }

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
        public bool? CheckUnlessModifiedSince(HttpRequestBase request, DateTime lastModified)
        {
            var requestHeaderUnlessModifiedSince = GetHttpHeaderValue(request, HTTP_HEADER_UNLESS_MODIFIED_SINCE, string.Empty);

            if (string.IsNullOrEmpty(requestHeaderUnlessModifiedSince))
            {
                return null;
            }

            DateTime incomingLastModified;

            if (!DateTime.TryParse(requestHeaderUnlessModifiedSince, out incomingLastModified))
            {
                return null;
            }

            return (lastModified <= incomingLastModified.ToUniversalTime());
        }

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
        public bool? CheckIfRange(HttpRequestBase request, string entityTag, DateTime lastModified)
        {
            var isRangeRequest = IsRangeRequest(request);

            if (!isRangeRequest)
            {
                //Not a range request so ignore
                return null;
            }

            var requestHeaderIfRange = GetHttpHeaderValue(request, HttpRequestHeader.IfRange, string.Empty);
            if (string.IsNullOrEmpty(requestHeaderIfRange))
            {
                return null;
            }

            DateTime incomingLastModified;
            //Might be a date
            if (DateTime.TryParse(requestHeaderIfRange, out incomingLastModified))
            {
                return (lastModified <= incomingLastModified.ToUniversalTime());
            }

            //Its not a date so assume its an entity tag
            return (requestHeaderIfRange == entityTag);
        }

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
        public bool? CheckIfMatch(HttpRequestBase request, string entityTag, bool doesEntityExist)
        {
            var requestHeaderIfMatchValues = GetHttpHeaderValues(request, HttpRequestHeader.IfMatch);

            if (!requestHeaderIfMatchValues.Any())
            {
                return null;
            }

            //Can use this to only return etag information
            if (requestHeaderIfMatchValues.Contains("*") )
            {
                //If entity does not exist 
                return doesEntityExist;
            }

            // Loop through all entity IDs, finding one 
            // which matches the current file's etag will
            // be enough to satisfy the If-Match
            return requestHeaderIfMatchValues.Any(entityId => entityId == entityTag);
        }

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
        public bool? CheckIfNoneMatch(HttpRequestBase request, string entityTag, bool doesEntityExist)
        {
            var requestHeaderIfNoneMatchValues = GetHttpHeaderValues(request, HttpRequestHeader.IfNoneMatch);

            if (!requestHeaderIfNoneMatchValues.Any())
            {
                return null;
            }

            //Can use this to only return etag information
            if (requestHeaderIfNoneMatchValues.Contains("*"))
            {
                return !doesEntityExist;
            }

            return requestHeaderIfNoneMatchValues.All(entityId => entityId != entityTag);
        }

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
        public bool? GetRanges(HttpRequestBase request, long contentLength, out IEnumerable<RangeItem> ranges)
        {
            var requestHeaderRange = GetHttpHeaderValue(request, HttpRequestHeader.Range, String.Empty);

            var rangesResult = new List<RangeItem>();

            if (string.IsNullOrEmpty(requestHeaderRange))
            {
                ranges = rangesResult;
                return null;
            }

            var rangesString = requestHeaderRange.Replace(" ", "").Replace("bytes=", "").Split(",".ToCharArray());

            // Check each found Range request for consistency
            foreach (var rangeString in rangesString)
            {
                // Split this range request by the dash character, 
                // currentRange[0] contains the requested begin-value,
                // currentRange[1] contains the requested end-value...
                var currentRangeString = rangeString.Split("-".ToCharArray());

                if (currentRangeString.Length != 2)
                {
                    ranges = null;
                    return false;
                }

                var currentRange = new RangeItem();

                // Determine the end of the requested range
                if (string.IsNullOrEmpty(currentRangeString[1]))
                {
                    // No end was specified, take the entire range
                    currentRange.EndRange = contentLength - 1;
                }
                else
                {
                    // An end was specified...
                    int endRangeValue;
                    if (!int.TryParse(currentRangeString[1], out endRangeValue))
                    {
                        ranges = null;
                        return false;
                    }

                    currentRange.EndRange = endRangeValue;
                }

                // Determine the begin of the requested range
                if (string.IsNullOrEmpty(currentRangeString[0]))
                {
                    // No begin was specified, which means that
                    // the end value indicated to return the last n
                    // bytes of the file:

                    // Calculate the begin
                    currentRange.StartRange = contentLength - currentRange.EndRange;
                    // ... to the end of the file...
                    currentRange.EndRange = contentLength - 1;
                }
                else
                {
                    // A normal begin value was indicated...
                    int beginRangeValue;
                    if (!int.TryParse(currentRangeString[0], out beginRangeValue))
                    {
                        ranges = null;
                        return false;
                    }

                    currentRange.StartRange = beginRangeValue;
                }

                // Check if the requested range values are valid, 
                // return False if they are not.

                // Note:
                // Do not clean invalid values up by fitting them into
                // valid parameters using Math.Min and Math.Max, because
                // some download clients (like Go!Zilla) might send invalid 
                // (e.g. too large) range requests to determine the file limits!

                // Begin and end must not exceed the file size
                if ((currentRange.StartRange > (contentLength - 1)) | (currentRange.EndRange > (contentLength - 1)))
                {
                    ranges = null;
                    return false;
                }

                // Begin and end cannot be < 0
                if ((currentRange.StartRange < 0) | (currentRange.EndRange < 0))
                {
                    ranges = null;
                    return false;
                }

                // End must be larger or equal to begin value
                if (currentRange.EndRange < currentRange.StartRange)
                {
                    // The requested Range is invalid...
                    ranges = null;
                    return false;
                }

                //We reached here so its a valid range, so add it to the list of ranges
                rangesResult.Add(currentRange);
            }
            ranges = rangesResult;
            return true;
        }
    }
}