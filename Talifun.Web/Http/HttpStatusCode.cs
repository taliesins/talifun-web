using System;

namespace Talifun.Web
{
    public struct HttpStatusCode : IEquatable<HttpStatusCode>
    {
        private readonly string _name;
        private readonly int _value;

        private HttpStatusCode(int value, string name)
        {
            this._name = name;
            this._value = value;
        }

        public bool Equals(HttpStatusCode other)
        {
            return this == other;
        }

        public override bool Equals(Object obj)
        {
            return obj is HttpStatusCode && this == (HttpStatusCode)obj;
        }

        public override int GetHashCode()
        {
            return _name.GetHashCode();
        }

        public static bool operator ==(HttpStatusCode x, HttpStatusCode y)
        {
            return x._name == y._name;
        }

        public static bool operator !=(HttpStatusCode x, HttpStatusCode y)
        {
            return x._name != y._name;
        }

        public static explicit operator string(HttpStatusCode value)
        {
            return value._name;
        }

        public static explicit operator int(HttpStatusCode value)
        {
            return value._value;
        }

        public override string ToString()
        {
            return _name;
        }

        public static readonly HttpStatusCode Continue = new HttpStatusCode(100, "Continue");
        public static readonly HttpStatusCode SwitchingProtocols = new HttpStatusCode(101, "Switching Protocols");
        public static readonly HttpStatusCode Ok = new HttpStatusCode(200, "Ok");
        public static readonly HttpStatusCode Created = new HttpStatusCode(201, "Created");
        public static readonly HttpStatusCode Accepted = new HttpStatusCode(202, "Accepted");
        public static readonly HttpStatusCode NonAuthoritativeInformation = new HttpStatusCode(203, "Non Authoritative Information");
        public static readonly HttpStatusCode NoContent = new HttpStatusCode(204, "No Content");
        public static readonly HttpStatusCode ResetContent = new HttpStatusCode(205, "Reset Content");
        public static readonly HttpStatusCode PartialContent = new HttpStatusCode(206, "Partial Content");
        public static readonly HttpStatusCode Ambiguous = new HttpStatusCode(206, "Ambiguous");
        public static readonly HttpStatusCode MultipleChoices = new HttpStatusCode(300, "Multiple Choices");
        public static readonly HttpStatusCode Moved = new HttpStatusCode(300, "Moved");
        public static readonly HttpStatusCode MovedPermanently = new HttpStatusCode(301, "Moved Permanently");
        public static readonly HttpStatusCode Found = new HttpStatusCode(302, "Found");
        public static readonly HttpStatusCode Redirect = new HttpStatusCode(302, "Redirect");
        public static readonly HttpStatusCode RedirectMethod = new HttpStatusCode(303, "Redirect Method");
        public static readonly HttpStatusCode SeeOther = new HttpStatusCode(303, "See Other");
        public static readonly HttpStatusCode NotModified = new HttpStatusCode(304, "Not Modified");
        public static readonly HttpStatusCode UseProxy = new HttpStatusCode(305, "Use Proxy");
        public static readonly HttpStatusCode Unused = new HttpStatusCode(306, "Unused");
        public static readonly HttpStatusCode RedirectKeepVerb = new HttpStatusCode(307, "Redirect Keep Verb");
        public static readonly HttpStatusCode TemporaryRedirect = new HttpStatusCode(307, "Temporary Redirect");
        public static readonly HttpStatusCode BadRequest = new HttpStatusCode(400, "Bad Request");
        public static readonly HttpStatusCode Unauthorized = new HttpStatusCode(401, "Unauthorized");
        public static readonly HttpStatusCode PaymentRequired = new HttpStatusCode(402, "Payment Required");
        public static readonly HttpStatusCode Forbidden = new HttpStatusCode(403, "Forbidden");
        public static readonly HttpStatusCode NotFound = new HttpStatusCode(404, "Not Found");
        public static readonly HttpStatusCode MethodNotAllowed = new HttpStatusCode(405, "Method Not Allowed");
        public static readonly HttpStatusCode NotAcceptable = new HttpStatusCode(406, "Not Acceptable");
        public static readonly HttpStatusCode ProxyAuthenticationRequired = new HttpStatusCode(407, "Proxy Authentication Required");
        public static readonly HttpStatusCode RequestTimeout = new HttpStatusCode(408, "Request Timeout");
        public static readonly HttpStatusCode Conflict = new HttpStatusCode(409, "Conflict");
        public static readonly HttpStatusCode Gone = new HttpStatusCode(410, "Gone");
        public static readonly HttpStatusCode LengthRequired = new HttpStatusCode(411, "Length Required");
        public static readonly HttpStatusCode PreconditionFailed = new HttpStatusCode(412, "Precondition Failed");
        public static readonly HttpStatusCode RequestEntityTooLarge = new HttpStatusCode(413, "Request Entity Too Large");
        public static readonly HttpStatusCode RequestUriTooLong = new HttpStatusCode(414, "Request Uri Too Long");
        public static readonly HttpStatusCode UnsupportedMediaType = new HttpStatusCode(415, "Unsupported MediaType");
        public static readonly HttpStatusCode RequestedRangeNotSatisfiable = new HttpStatusCode(416, "Requested Range Not Satisfiable");
        public static readonly HttpStatusCode ExpectationFailed = new HttpStatusCode(417, "Expectation Failed");
        public static readonly HttpStatusCode InternalServerError = new HttpStatusCode(500, "Internal Server Error");
        public static readonly HttpStatusCode NotImplemented = new HttpStatusCode(501, "Not Implemented");
        public static readonly HttpStatusCode BadGateway = new HttpStatusCode(502, "Bad Gateway");
        public static readonly HttpStatusCode ServiceUnavailable = new HttpStatusCode(503, "Service Unavailable");
        public static readonly HttpStatusCode GatewayTimeout = new HttpStatusCode(504, "Gateway Timeout");
        public static readonly HttpStatusCode HttpVersionNotSupported = new HttpStatusCode(505, "Http Version Not Supported");
    }
}
