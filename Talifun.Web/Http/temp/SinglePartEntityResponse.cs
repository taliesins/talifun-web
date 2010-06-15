using System;
using System.IO.Compression;
using System.Web;

namespace Talifun.Web
{
    public class SinglePartEntityResponse : IEntityResponse
    {
        public const string HTTP_HEADER_CONTENT_LENGTH = "Content-Length";
        public const string HTTP_HEADER_CONTENT_RANGE = "Content-Range";
        public const string BYTES = "bytes";

        protected readonly RangeItem Range;
        protected readonly IHttpResponseHeaderHelper HttpResponseHeaderHelper;

        public SinglePartEntityResponse(IHttpResponseHeaderHelper httpResponseHeaderHelper, RangeItem range)
        {
            this.HttpResponseHeaderHelper = httpResponseHeaderHelper;
            this.Range = range;
        }

        public void SendHeaders(HttpResponseBase response, ResponseCompressionType compressionType, IEntity entity)
        {
            //Data must be in uncompressed format when responding partially
            if (entity.CompressionType != ResponseCompressionType.None)
            {
                //TODO: perhaps we could uncompress object, but for now we don't worry about it
                throw new Exception("Cannot do a partial response on compressed data");
            }

            HttpResponseHeaderHelper.SetContentEncoding(response, compressionType);

            switch (compressionType)
            {
                case ResponseCompressionType.None:
                    var contentLength = Range.EndRange - Range.StartRange + 1;
                    HttpResponseHeaderHelper.AppendHeader(response, HTTP_HEADER_CONTENT_LENGTH, contentLength.ToString());
                    break;
                case ResponseCompressionType.GZip:
                    response.Filter = new GZipStream(response.Filter, CompressionMode.Compress);
                    //This means that the output stream will be chunked, so we don't have to worry about content length
                    break;
                case ResponseCompressionType.Deflate:
                    response.Filter = new DeflateStream(response.Filter, CompressionMode.Compress);
                    //This means that the output stream will be chunked, so we don't have to worry about content length
                    break;
            }

            response.ContentType = entity.ContentType;
            HttpResponseHeaderHelper.AppendHeader(response, HTTP_HEADER_CONTENT_RANGE, BYTES + " " + Range.StartRange + "-" + Range.EndRange + "/" + entity.ContentLength);
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

            var bytesToRead = Range.EndRange - Range.StartRange + 1;
            transmitEntity.Transmit(response, Range.StartRange, bytesToRead);
        }
    }
}
