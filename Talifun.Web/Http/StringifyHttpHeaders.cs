using System;
using System.Collections.Generic;
using System.Net;

namespace Talifun.Web
{
    public sealed class StringifyHttpHeaders
    {
        private static readonly Dictionary<HttpRequestHeader, string> HttpRequestHeaderToString;
        private static readonly Dictionary<string, HttpRequestHeader> StringToHttpRequestHeader;

        private static readonly Dictionary<HttpResponseHeader, string> HttpResponseHeaderToString;
        private static readonly Dictionary<string, HttpResponseHeader> StringToHttpResponseHeader;

        private static readonly Dictionary<HttpMethod, string> HttpMethodToString;
        private static readonly Dictionary<string, HttpMethod> StringToHttpMethod;

        private static readonly Dictionary<HttpProtocolVersion, string> HttpProtocolVersionToString;
        private static readonly Dictionary<string, HttpProtocolVersion> StringToHttpProtocolVersion;

        private static readonly Dictionary<HttpStatus, string> HttpStatusToString;
        private static readonly Dictionary<HttpStatus, HttpStatusCode> HttpStatusToHttpStatusCode;
        private static readonly Dictionary<string, HttpStatus> StringToHttpStatus;

        private static readonly object[][] HttpRequestHeaders = new object[][]
                {
                        new object[] { HttpRequestHeader.CacheControl, "cache-control"},
                        new object[] { HttpRequestHeader.Connection, "connection"},
                        new object[] { HttpRequestHeader.Date, "date"},
                        new object[] { HttpRequestHeader.KeepAlive, "keep-alive"},
                        new object[] { HttpRequestHeader.Pragma, "pragma"},
                        new object[] { HttpRequestHeader.Trailer, "trailer"},
                        new object[] { HttpRequestHeader.TransferEncoding, "transfer-encoding"},
                        new object[] { HttpRequestHeader.Upgrade, "upgrade"},
                        new object[] { HttpRequestHeader.Via, "via"},
                        new object[] { HttpRequestHeader.Warning, "warning"},
                        new object[] { HttpRequestHeader.Allow, "allow"},
                        new object[] { HttpRequestHeader.ContentLength, "content-length"},
                        new object[] { HttpRequestHeader.ContentType, "content-type"},
                        new object[] { HttpRequestHeader.ContentEncoding, "content-encoding"},
                        new object[] { HttpRequestHeader.ContentLanguage, "content-language"},
                        new object[] { HttpRequestHeader.ContentLocation, "content-location"},
                        new object[] { HttpRequestHeader.ContentMd5, "content-md5"},
                        new object[] { HttpRequestHeader.ContentRange, "content-range"},
                        new object[] { HttpRequestHeader.Expires, "expires"},
                        new object[] { HttpRequestHeader.LastModified, "last-modified"},
                        new object[] { HttpRequestHeader.Accept, "accept"},
                        new object[] { HttpRequestHeader.AcceptCharset, "accept-charset"},
                        new object[] { HttpRequestHeader.AcceptEncoding, "accept-encoding"},
                        new object[] { HttpRequestHeader.AcceptLanguage, "accept-language"},
                        new object[] { HttpRequestHeader.Authorization, "authorization"},
                        new object[] { HttpRequestHeader.Cookie, "cookie"},
                        new object[] { HttpRequestHeader.Expect, "expect"},
                        new object[] { HttpRequestHeader.From, "from"},
                        new object[] { HttpRequestHeader.Host, "host"},
                        new object[] { HttpRequestHeader.IfMatch, "if-match"},
                        new object[] { HttpRequestHeader.IfModifiedSince, "if-modified-since"},
                        new object[] { HttpRequestHeader.IfNoneMatch, "if-none-match"},
                        new object[] { HttpRequestHeader.IfRange, "if-range"},
                        new object[] { HttpRequestHeader.IfUnmodifiedSince, "if-unmodified-since"},
                        new object[] { HttpRequestHeader.MaxForwards, "max-forwards"},
                        new object[] { HttpRequestHeader.ProxyAuthorization, "proxy-authorization"},
                        new object[] { HttpRequestHeader.Referer, "referer"},
                        new object[] { HttpRequestHeader.Range, "range"},
                        new object[] { HttpRequestHeader.Te, "te"},
                        new object[] { HttpRequestHeader.Translate, "translate"},
                        new object[] { HttpRequestHeader.UserAgent, "user-agent"}                          
                };

        private static readonly object[][] HttpResponseHeaders = new object[][]
                {
                        new object[] { HttpResponseHeader.CacheControl, "cache-control" }, 
                        new object[] { HttpResponseHeader.Connection, "connection" }, 
                        new object[] { HttpResponseHeader.Date, "date" }, 
                        new object[] { HttpResponseHeader.KeepAlive, "keep-alive" }, 
                        new object[] { HttpResponseHeader.Pragma, "pragma" }, 
                        new object[] { HttpResponseHeader.Trailer, "trailer" }, 
                        new object[] { HttpResponseHeader.TransferEncoding, "transfer-encoding" }, 
                        new object[] { HttpResponseHeader.Upgrade, "upgrade" }, 
                        new object[] { HttpResponseHeader.Via, "via" }, 
                        new object[] { HttpResponseHeader.Warning, "warning" }, 
                        new object[] { HttpResponseHeader.Allow, "allow" }, 
                        new object[] { HttpResponseHeader.ContentLength, "content-length" }, 
                        new object[] { HttpResponseHeader.ContentType, "content-type" }, 
                        new object[] { HttpResponseHeader.ContentEncoding, "content-encoding" }, 
                        new object[] { HttpResponseHeader.ContentLanguage, "content-language" }, 
                        new object[] { HttpResponseHeader.ContentLocation, "content-location" }, 
                        new object[] { HttpResponseHeader.ContentMd5, "content-md5" }, 
                        new object[] { HttpResponseHeader.ContentRange, "content-range" }, 
                        new object[] { HttpResponseHeader.Expires, "expires" }, 
                        new object[] { HttpResponseHeader.LastModified, "last-modified" }, 
                        new object[] { HttpResponseHeader.AcceptRanges, "accept-ranges" }, 
                        new object[] { HttpResponseHeader.Age, "age" }, 
                        new object[] { HttpResponseHeader.ETag, "etag" }, 
                        new object[] { HttpResponseHeader.Location, "location" }, 
                        new object[] { HttpResponseHeader.ProxyAuthenticate, "proxy-authenticate" }, 
                        new object[] { HttpResponseHeader.RetryAfter, "RetryAfter" }, 
                        new object[] { HttpResponseHeader.Server, "server" }, 
                        new object[] { HttpResponseHeader.SetCookie, "set-cookie" }, 
                        new object[] { HttpResponseHeader.Vary, "vary" }, 
                        new object[] { HttpResponseHeader.WwwAuthenticate, "www-authenticate" }
                };

        private static readonly object[][] HttpMethods = new object[][]
                {
                        new object[] { HttpMethod.Get, "GET" },
                        new object[] { HttpMethod.Put, "PUT" },
                        new object[] { HttpMethod.Connect, "CONNECT" },
                        new object[] { HttpMethod.Head, "HEAD" },
                        new object[] { HttpMethod.Options, "OPTIONS" },
                        new object[] { HttpMethod.Post, "POST" },
                        new object[] { HttpMethod.Trace, "TRACE"},
                        new object[] { HttpMethod.Delete, "DELETE"}
                };

        private static readonly object[][] HttpProtocolVersions = new object[][]
                {
                        new object[] { HttpProtocolVersion.Http10, "HTTP/1.0" },
                        new object[] { HttpProtocolVersion.Http11, "HTTP/1.1" }
                };

        private static readonly object[][] HttpStatusCodes = new object[][]
                {
                        new object[] { HttpStatus.Accepted, HttpStatusCode.Accepted, "Accepted" },
                        new object[] { HttpStatus.Ambiguous, HttpStatusCode.Ambiguous, "Ambiguous" },
                        new object[] { HttpStatus.BadGateway, HttpStatusCode.BadGateway, "Bad Gatway" },
                        new object[] { HttpStatus.BadRequest, HttpStatusCode.BadRequest, "Bad Request" },
                        new object[] { HttpStatus.Conflict, HttpStatusCode.Conflict, "Conflict" },
                        new object[] { HttpStatus.Continue, HttpStatusCode.Continue, "Continue" },
                        new object[] { HttpStatus.Created, HttpStatusCode.Created, "Created" },
                        new object[] { HttpStatus.ExpectationFailed, HttpStatusCode.ExpectationFailed, "Expectation Failed" },
                        new object[] { HttpStatus.Forbidden, HttpStatusCode.Forbidden, "Path Forbidden" },
                        new object[] { HttpStatus.Found, HttpStatusCode.Found, "Found" },
                        new object[] { HttpStatus.GatewayTimeout, HttpStatusCode.GatewayTimeout, "Gateway Timout" },
                        new object[] { HttpStatus.Gone, HttpStatusCode.Gone, "Gone" },
                        new object[] { HttpStatus.HttpVersionNotSupported, HttpStatusCode.HttpVersionNotSupported, "Http Version Not Supported" },
                        new object[] { HttpStatus.InternalServerError, HttpStatusCode.InternalServerError, "Internal Server Error" },
                        new object[] { HttpStatus.LengthRequired, HttpStatusCode.LengthRequired, "Length Required" },
                        new object[] { HttpStatus.MethodNotAllowed, HttpStatusCode.MethodNotAllowed, "Method Not Allowed" },
                        new object[] { HttpStatus.Moved, HttpStatusCode.Moved, "Moved" },
                        new object[] { HttpStatus.MovedPermanently, HttpStatusCode.MovedPermanently, "Moved Permanently" },
                        new object[] { HttpStatus.MultipleChoices, HttpStatusCode.MultipleChoices, "Multiple Choices" },
                        new object[] { HttpStatus.NoContent, HttpStatusCode.NoContent, "No Content" },
                        new object[] { HttpStatus.NonAuthoritativeInformation, HttpStatusCode.NonAuthoritativeInformation, "Non Authoritative Information" },
                        new object[] { HttpStatus.NotAcceptable, HttpStatusCode.NotAcceptable, "Not Acceptable" },
                        new object[] { HttpStatus.NotFound, HttpStatusCode.NotFound, "Not Found" },
                        new object[] { HttpStatus.NotImplemented, HttpStatusCode.NotImplemented, "Not Implemented" },
                        new object[] { HttpStatus.NotModified, HttpStatusCode.NotModified, "Not Modified" },
                        new object[] { HttpStatus.Ok, HttpStatusCode.OK, "OK" },
                        new object[] { HttpStatus.PartialContent, HttpStatusCode.PartialContent, "Partial content" },
                        new object[] { HttpStatus.PaymentRequired, HttpStatusCode.PaymentRequired, "Payment Required" },
                        new object[] { HttpStatus.PreconditionFailed, HttpStatusCode.PreconditionFailed, "Precondition Failed" },
                        new object[] { HttpStatus.ProxyAuthenticationRequired, HttpStatusCode.ProxyAuthenticationRequired, "Proxy Authentication Required" },
                        new object[] { HttpStatus.Redirect, HttpStatusCode.Redirect, "Redirect" },
                        new object[] { HttpStatus.RedirectKeepVerb, HttpStatusCode.RedirectKeepVerb, "Redirect Keep Verb" },
                        new object[] { HttpStatus.RedirectMethod, HttpStatusCode.RedirectMethod, "Redirect Method" },
                        new object[] { HttpStatus.RequestedRangeNotSatisfiable, HttpStatusCode.RequestedRangeNotSatisfiable, "Requested Range Not Satisfiable" },
                        new object[] { HttpStatus.RequestEntityTooLarge, HttpStatusCode.RequestEntityTooLarge, "Request Entity Too Large" },
                        new object[] { HttpStatus.RequestTimeout, HttpStatusCode.RequestTimeout, "Request Timeout" },
                        new object[] { HttpStatus.RequestUriTooLong, HttpStatusCode.RequestUriTooLong, "Request Uri Too Long" },
                        new object[] { HttpStatus.ResetContent, HttpStatusCode.ResetContent, "Reset Content" },
                        new object[] { HttpStatus.SeeOther, HttpStatusCode.SeeOther, "See Other" },
                        new object[] { HttpStatus.ServiceUnavailable, HttpStatusCode.ServiceUnavailable, "Service Unavailable" },
                        new object[] { HttpStatus.SwitchingProtocols, HttpStatusCode.SwitchingProtocols, "Switching Protocols" },
                        new object[] { HttpStatus.TemporaryRedirect, HttpStatusCode.TemporaryRedirect, "Temporary Redirect" },
                        new object[] { HttpStatus.Unauthorized, HttpStatusCode.Unauthorized, "Unauthorized" },
                        new object[] { HttpStatus.UnsupportedMediaType, HttpStatusCode.UnsupportedMediaType, "Unsupported Media Type" },
                        new object[] { HttpStatus.Unused, HttpStatusCode.Unused, "Unused" },
                        new object[] { HttpStatus.UseProxy, HttpStatusCode.UseProxy, "UseProxy" }
                };

        static StringifyHttpHeaders()
        {
            HttpRequestHeaderToString = new Dictionary<HttpRequestHeader, string>();
            StringToHttpRequestHeader = new Dictionary<string, HttpRequestHeader>();

            HttpResponseHeaderToString = new Dictionary<HttpResponseHeader, string>();
            StringToHttpResponseHeader = new Dictionary<string, HttpResponseHeader>();

            HttpMethodToString = new Dictionary<HttpMethod, string>();
            StringToHttpMethod = new Dictionary<string, HttpMethod>();

            HttpProtocolVersionToString = new Dictionary<HttpProtocolVersion, string>();
            StringToHttpProtocolVersion = new Dictionary<string, HttpProtocolVersion>();

            HttpStatusToString = new Dictionary<HttpStatus, string>();
            HttpStatusToHttpStatusCode = new Dictionary<HttpStatus, HttpStatusCode>();
            StringToHttpStatus = new Dictionary<string, HttpStatus>();

            foreach (var pair in HttpRequestHeaders)
            {
                var httpRequestHeader = (HttpRequestHeader)pair[0];
                var httpRequestHeaderName = (String)pair[1];

                HttpRequestHeaderToString.Add(httpRequestHeader, httpRequestHeaderName);
                StringToHttpRequestHeader.Add(httpRequestHeaderName, httpRequestHeader);
            }

            foreach (var pair in HttpResponseHeaders)
            {
                var httpResponseHeader = (HttpResponseHeader)pair[0];
                var httpResponseHeaderName = (String)pair[1];

                HttpResponseHeaderToString.Add(httpResponseHeader, httpResponseHeaderName);
                StringToHttpResponseHeader.Add(httpResponseHeaderName, httpResponseHeader);
            }

            foreach (var pair in HttpMethods)
            {
                var httpMethod = (HttpMethod)pair[0];
                var httpMethodName = (String)pair[1];

                HttpMethodToString.Add(httpMethod, httpMethodName);
                StringToHttpMethod.Add(httpMethodName, httpMethod);
            }

            foreach (var pair in HttpProtocolVersions)
            {
                var httpProtocolVersion = (HttpProtocolVersion)pair[0];
                var httpProtocolVersionName = (String)pair[1];

                HttpProtocolVersionToString.Add(httpProtocolVersion, httpProtocolVersionName);
                StringToHttpProtocolVersion.Add(httpProtocolVersionName, httpProtocolVersion);
            }

            foreach (var pair in HttpStatusCodes)
            {
                var httpStatus = (HttpStatus) pair[0];
                var httpStatusCode = (HttpStatusCode) pair[1];
                var httpStatusCodeName = (String) pair[2];

                HttpStatusToString.Add(httpStatus, httpStatusCodeName);
                HttpStatusToHttpStatusCode.Add(httpStatus, httpStatusCode);
                StringToHttpStatus.Add(httpStatusCodeName, httpStatus);
            }
        }

        public static HttpRequestHeader RequestHeaderFromString(string httpRequestHeaderName)
        {
            return StringToHttpRequestHeader[httpRequestHeaderName.ToLower()];
        }

        public static string StringFromRequestHeader(HttpRequestHeader httpRequestHeader)
        {
            return HttpRequestHeaderToString[httpRequestHeader];
        }

        public static HttpResponseHeader ResponseHeaderFromString(string httpResponseHeaderName)
        {
            return StringToHttpResponseHeader[httpResponseHeaderName.ToLower()];
        }

        public static string StringFromResponseHeader(HttpResponseHeader httpResponseHeader)
        {
            return HttpResponseHeaderToString[httpResponseHeader];
        }

        public static HttpMethod HttpMethodFromString(string httpMethodName)
        {
            return StringToHttpMethod[httpMethodName];
        }

        public static string StringFromHttpMethod(HttpMethod httpMethod)
        {
            return HttpMethodToString[httpMethod];
        }

        public static HttpProtocolVersion ProtocolVersionFromString(string httpProtocolVersionName)
        {
            return StringToHttpProtocolVersion[httpProtocolVersionName];
        }

        public static string StringFromProtocolVersion(HttpProtocolVersion httpProtocolVersion)
        {
            return HttpProtocolVersionToString[httpProtocolVersion];
        }

        public static HttpStatus HttpStatusFromString(string httpStatusName)
        {
            return StringToHttpStatus[httpStatusName];
        }

        public static HttpStatusCode HttpStatusCodeFromHttpStatus(HttpStatus httpStatus)
        {
            return HttpStatusToHttpStatusCode[httpStatus];
        }

        public static string StringFromHttpStatus(HttpStatus httpStatus)
        {
            return HttpStatusToString[httpStatus];
        }
    }
}
