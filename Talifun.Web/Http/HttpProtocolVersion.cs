using System;

namespace Talifun.Web
{
    public struct HttpProtocolVersion : IEquatable<HttpProtocolVersion>
    {
        private readonly string _name;
        private readonly int _value;

        private HttpProtocolVersion(int value, string name)
        {
            this._name = name;
            this._value = value;
        }

        public bool Equals(HttpProtocolVersion other)
        {
            return this == other;
        }

        public override bool Equals(Object obj)
        {
            return obj is HttpProtocolVersion && this == (HttpProtocolVersion)obj;
        }

        public override int GetHashCode()
        {
            return _name.GetHashCode();
        }

        public static bool operator ==(HttpProtocolVersion x, HttpProtocolVersion y)
        {
            return x._name == y._name;
        }

        public static bool operator !=(HttpProtocolVersion x, HttpProtocolVersion y)
        {
            return x._name != y._name;
        }

        public static explicit operator string(HttpProtocolVersion value)
        {
            return value._name;
        }

        public static explicit operator int(HttpProtocolVersion value)
        {
            return value._value;
        }

        public override string ToString()
        {
            return _name;
        }

        public static readonly HttpProtocolVersion Http10 = new HttpProtocolVersion(0, "HTTP/1.0");
        public static readonly HttpProtocolVersion Http11 = new HttpProtocolVersion(1, "HTTP/1.1");
    }
}
