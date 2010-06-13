using System;
using System.Web;

namespace Talifun.Web
{
    public class SingleByteRangeResponse : IByteRangeResponse
    {
        public const string HTTP_HEADER_CONTENT_LENGTH = "Content-Length";
        public const string HTTP_HEADER_CONTENT_RANGE = "Content-Range";
        public const string BYTES = "bytes";

        private readonly RangeItem range;
        protected readonly IHttpResponseHeaderHelper HttpResponseHeaderHelper;

        public SingleByteRangeResponse(IHttpResponseHeaderHelper httpResponseHeaderHelper, RangeItem range)
        {
            this.HttpResponseHeaderHelper = httpResponseHeaderHelper;
            this.range = range;
        }

        public void SetContentLength(HttpResponseBase response, IEntity entity)
        {
            var contentLength = range.EndRange - range.StartRange + 1;
            HttpResponseHeaderHelper.AppendHeader(response, HTTP_HEADER_CONTENT_LENGTH, contentLength.ToString());
        }

        public void SetOtherHeaders(HttpResponseBase response, IEntity entity)
        {
            response.ContentType = entity.ContentType;
            HttpResponseHeaderHelper.AppendHeader(response, HTTP_HEADER_CONTENT_RANGE, BYTES + " " + range.StartRange + "-" + range.EndRange + "/" + entity.ContentLength);
        }

        public void SendBody(HttpMethod requestHttpMethod, HttpResponseBase response, ITransmitEntityStrategy transmitEntity)
        {
            if (requestHttpMethod == HttpMethod.Head)
            {
                return;
            }

            if (requestHttpMethod != HttpMethod.Get)
            {
                throw new Exception("Unsupported http method");
            }

            var bytesToRead = range.EndRange - range.StartRange + 1;
            transmitEntity.Transmit(response, range.StartRange, bytesToRead);
        }
    }
}
