using System;

namespace Talifun.Web
{
    public struct HttpResponseHeader : IEquatable<HttpResponseHeader>
    {
        private readonly string _name;

        private HttpResponseHeader(string name)
        {
            this._name = name;
        }

        public bool Equals(HttpResponseHeader other)
        {
            return this == other;
        }

        public override bool Equals(Object obj)
        {
            return obj is HttpResponseHeader && this == (HttpResponseHeader)obj;
        }

        public override int GetHashCode()
        {
            return _name.GetHashCode();
        }

        public static bool operator ==(HttpResponseHeader x, HttpResponseHeader y)
        {
            return x._name == y._name;
        }

        public static bool operator !=(HttpResponseHeader x, HttpResponseHeader y)
        {
            return x._name != y._name;
        }

        public static explicit operator string(HttpResponseHeader value)
        {
            return value._name;
        }

        public override string ToString()
        {
            return _name;
        }

        public static readonly HttpResponseHeader CacheControl = new HttpResponseHeader("cache-control");
        public static readonly HttpResponseHeader Connection = new HttpResponseHeader("connection");
        public static readonly HttpResponseHeader Date = new HttpResponseHeader("date");
        public static readonly HttpResponseHeader KeepAlive = new HttpResponseHeader("keep-alive");
        public static readonly HttpResponseHeader Pragma = new HttpResponseHeader("pragma");
        public static readonly HttpResponseHeader Trailer = new HttpResponseHeader("trailer");
        public static readonly HttpResponseHeader TransferEncoding = new HttpResponseHeader("transfer-encoding");
        public static readonly HttpResponseHeader Upgrade = new HttpResponseHeader("upgrade");
        public static readonly HttpResponseHeader Via = new HttpResponseHeader("via");
        public static readonly HttpResponseHeader Warning = new HttpResponseHeader("warning");
        public static readonly HttpResponseHeader Allow = new HttpResponseHeader("allow");
        public static readonly HttpResponseHeader ContentLength = new HttpResponseHeader("content-length");
        public static readonly HttpResponseHeader ContentType = new HttpResponseHeader("content-type");
        public static readonly HttpResponseHeader ContentEncoding = new HttpResponseHeader("content-encoding");
        public static readonly HttpResponseHeader ContentLanguage = new HttpResponseHeader("content-language");
        public static readonly HttpResponseHeader ContentLocation = new HttpResponseHeader("content-location");
        public static readonly HttpResponseHeader ContentMd5 = new HttpResponseHeader("content-md5");
        public static readonly HttpResponseHeader ContentRange = new HttpResponseHeader("content-range");
        public static readonly HttpResponseHeader Expires = new HttpResponseHeader("expires");
        public static readonly HttpResponseHeader LastModified = new HttpResponseHeader("last-modified");
        public static readonly HttpResponseHeader AcceptRanges = new HttpResponseHeader("accept-ranges");
        public static readonly HttpResponseHeader Age = new HttpResponseHeader("age");
        public static readonly HttpResponseHeader ETag = new HttpResponseHeader("etag");
        public static readonly HttpResponseHeader Location = new HttpResponseHeader("location");
        public static readonly HttpResponseHeader ProxyAuthenticate = new HttpResponseHeader("proxy-authenticate");
        public static readonly HttpResponseHeader RetryAfter = new HttpResponseHeader("retry-after");
        public static readonly HttpResponseHeader Server = new HttpResponseHeader("server");
        public static readonly HttpResponseHeader SetCookie = new HttpResponseHeader("set-cookie");
        public static readonly HttpResponseHeader Vary = new HttpResponseHeader("vary");
        public static readonly HttpResponseHeader WwwAuthenticate = new HttpResponseHeader("www-authenticate");
    }
}
               