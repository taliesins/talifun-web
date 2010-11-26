using System;

namespace Talifun.Web
{
    public struct HttpMethod : IEquatable<HttpMethod>
    {
        private readonly string _name;
        private readonly int _value;

        private HttpMethod(int value, string name)
        {
            this._name = name;
            this._value = value;
        }

        public bool Equals(HttpMethod other)
        {
            return this == other;
        }

        public override bool Equals(Object obj)
        {
            return obj is HttpMethod && this == (HttpMethod)obj;
        }

        public override int GetHashCode()
        {
            return _name.GetHashCode();
        }

        public static bool operator ==(HttpMethod x, HttpMethod y)
        {
            return x._name == y._name;
        }

        public static bool operator !=(HttpMethod x, HttpMethod y)
        {
            return x._name != y._name;
        }

        public static explicit operator string(HttpMethod value)
        {
            return value._name;
        }

        public static explicit operator HttpMethod(string value)
        {
            switch (value.ToUpperInvariant())
            {
                case "CONNECT":
                    return Connect;
                case "DELETE":
                    return Delete;
                case "GET":
                    return Get;
                case "HEAD":
                    return Head;
                case "OPTIONS":
                    return Options;
                case "OTHER":
                    return Other;
                case "POST":
                    return Post;
                case "PUT":
                    return Put;
                case "TRACE":
                    return Trace;
                default:
                    throw new FormatException("Unable to convert to HttpMethod");

            }
        }

        public static explicit operator HttpMethod(int value)
        {
            switch (value)
            {
                case 7:
                    return Connect;
                case 4:
                    return Delete;
                case 1:
                    return Get;
                case 0:
                    return Head;
                case 5:
                    return Options;
                case -1:
                    return Other;
                case 2:
                    return Post;
                case 3:
                    return Put;
                case 6:
                    return Trace;
                default:
                    throw new FormatException("Unable to convert to HttpMethod");

            }
        }

        public static explicit operator int(HttpMethod value)
        {
            return value._value;
        }

        public override string ToString()
        {
            return _name;
        }

        /// <summary>
        /// This specification reserves the method name CONNECT for use with a proxy that can dynamically switch to being a tunnel (e.g. SSL tunneling). 
        /// </summary>
        public static readonly HttpMethod Connect = new HttpMethod(7, "CONNECT");

        /// <summary>
        /// The DELETE method requests that the origin server delete the resource identified by the Request-URI. 
        /// </summary>
        public static readonly HttpMethod Delete = new HttpMethod(4, "DELETE");

        /// <summary>
        /// The GET method means retrieve whatever information (in the form of an entity) is identified by the Request-URI. If the Request-URI refers to a data-producing process, it is the produced data which shall be returned as the entity in the response and not the source text of the process, unless that text happens to be the output of the process. 
        /// </summary>
        public static readonly HttpMethod Get = new HttpMethod(1, "GET");

        /// <summary>
        /// response to a HEAD request SHOULD be identical to the information sent in response to a GET request. This method can be used for obtaining metainformation about the entity implied by the request without transferring the entity-body itself. This method is often used for testing hypertext links for validity, accessibility, and recent 
        /// </summary>
        public static readonly HttpMethod Head = new HttpMethod(0, "HEAD");
        public static readonly HttpMethod Options = new HttpMethod(5, "OPTIONS");
        public static readonly HttpMethod Other = new HttpMethod(-1, "OTHER");

        /// <summary>
        /// The POST method is used to request that the origin server accept the entity enclosed in the request as a new subordinate of the resource identified by the Request-URI in the Request-Line. 
        /// </summary>
        public static readonly HttpMethod Post = new HttpMethod(2, "POST");

        /// <summary>
        /// The PUT method requests that the enclosed entity be stored under the supplied Request-URI. 
        /// </summary>
        public static readonly HttpMethod Put = new HttpMethod(3, "PUT");

        /// <summary>
        /// The TRACE method is used to invoke a remote, application-layer loop- back of the request message. 
        /// </summary>
        public static readonly HttpMethod Trace = new HttpMethod(6, "TRACE");
    }
}