using System;

namespace Talifun.Web
{
    public struct HttpRequestHeader : IEquatable<HttpRequestHeader>
    {
        private readonly string _name;

        private HttpRequestHeader(string name)
        {
            this._name = name;
        }

        public bool Equals(HttpRequestHeader other)
        {
            return this == other;
        }

        public override bool Equals(Object obj)
        {
            return obj is HttpRequestHeader && this == (HttpRequestHeader)obj;
        }

        public override int GetHashCode()
        {
            return _name.GetHashCode();
        }

        public static bool operator ==(HttpRequestHeader x, HttpRequestHeader y)
        {
            return x._name == y._name;
        }

        public static bool operator !=(HttpRequestHeader x, HttpRequestHeader y)
        {
            return x._name != y._name;
        }

        public static explicit operator string(HttpRequestHeader value)
        {
            return value._name;
        }

        public override string ToString()
        {
            return _name;
        }

        public static readonly HttpRequestHeader CacheControl = new HttpRequestHeader("cache-control");
        public static readonly HttpRequestHeader Connection = new HttpRequestHeader("connection");
        public static readonly HttpRequestHeader Date = new HttpRequestHeader("date");
        public static readonly HttpRequestHeader KeepAlive = new HttpRequestHeader("keep-alive");
        public static readonly HttpRequestHeader Pragma = new HttpRequestHeader("pragma");
        public static readonly HttpRequestHeader Trailer = new HttpRequestHeader("trailer");
        public static readonly HttpRequestHeader TransferEncoding = new HttpRequestHeader("transfer-encoding");
        public static readonly HttpRequestHeader Upgrade = new HttpRequestHeader("upgrade");
        public static readonly HttpRequestHeader Via = new HttpRequestHeader("via");
        public static readonly HttpRequestHeader Warning = new HttpRequestHeader("warning");
        public static readonly HttpRequestHeader Allow = new HttpRequestHeader("allow");
        public static readonly HttpRequestHeader ContentLength = new HttpRequestHeader("content-length");
        public static readonly HttpRequestHeader ContentType = new HttpRequestHeader("content-type");
        public static readonly HttpRequestHeader ContentEncoding = new HttpRequestHeader("content-encoding");
        public static readonly HttpRequestHeader ContentLanguage = new HttpRequestHeader("content-language");
        public static readonly HttpRequestHeader ContentLocation = new HttpRequestHeader("content-location");
        public static readonly HttpRequestHeader ContentMd5 = new HttpRequestHeader("content-md5");
        public static readonly HttpRequestHeader ContentRange = new HttpRequestHeader("content-range");
        public static readonly HttpRequestHeader Expires = new HttpRequestHeader("expires");
        public static readonly HttpRequestHeader LastModified = new HttpRequestHeader("last-modified");
        public static readonly HttpRequestHeader Accept = new HttpRequestHeader("accept");
        public static readonly HttpRequestHeader AcceptCharset = new HttpRequestHeader("accept-charset");
        public static readonly HttpRequestHeader AcceptEncoding = new HttpRequestHeader("accept-encoding");
        public static readonly HttpRequestHeader AcceptLanguage = new HttpRequestHeader("accept-language");
        public static readonly HttpRequestHeader Authorization = new HttpRequestHeader("authorization");
        public static readonly HttpRequestHeader Cookie = new HttpRequestHeader("cookie");
        public static readonly HttpRequestHeader Expect = new HttpRequestHeader("expect");
        public static readonly HttpRequestHeader From = new HttpRequestHeader("from");
        public static readonly HttpRequestHeader Host = new HttpRequestHeader("host");
        public static readonly HttpRequestHeader IfMatch = new HttpRequestHeader("if-match");
        public static readonly HttpRequestHeader IfModifiedSince = new HttpRequestHeader("if-modified-since");
        public static readonly HttpRequestHeader IfNoneMatch = new HttpRequestHeader("if-none-match");
        public static readonly HttpRequestHeader IfRange = new HttpRequestHeader("if-range");
        public static readonly HttpRequestHeader IfUnmodifiedSince = new HttpRequestHeader("if-unmodified-since");
        public static readonly HttpRequestHeader MaxForwards = new HttpRequestHeader("max-forwards");
        public static readonly HttpRequestHeader ProxyAuthorization = new HttpRequestHeader("proxy-authorization");
        public static readonly HttpRequestHeader Referer = new HttpRequestHeader("referer");
        public static readonly HttpRequestHeader Range = new HttpRequestHeader("range");
        public static readonly HttpRequestHeader Te = new HttpRequestHeader("te");
        public static readonly HttpRequestHeader Translate = new HttpRequestHeader("translate");
        public static readonly HttpRequestHeader UserAgent = new HttpRequestHeader("user-agent");
        public static readonly HttpRequestHeader UnlessModifiedSince = new HttpRequestHeader("unless-modified-since");
    }
}

