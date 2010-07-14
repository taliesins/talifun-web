using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Text;
using System.Web;

namespace Talifun.Web
{
    public class MultiPartEntityResponse : IEntityResponse
    {
        public const string MULTIPART_BOUNDARY = "<q1w2e3r4t5y6u7i8o9p0>";
        public const string MULTIPART_CONTENTTYPE = "multipart/byteranges; boundary=" + MULTIPART_BOUNDARY;

        public const string HTTP_HEADER_CONTENT_TYPE = "Content-Type";
        public const string HTTP_HEADER_CONTENT_RANGE = "Content-Range";
        public const string HTTP_HEADER_CONTENT_LENGTH = "Content-Length";
        public const string BYTES = "bytes";

        protected readonly IHttpResponseHeaderHelper HttpResponseHeaderHelper;
        protected readonly IEnumerable<RangeItem> Ranges;

        public MultiPartEntityResponse(IHttpResponseHeaderHelper httpResponseHeaderHelper, IEnumerable<RangeItem> ranges)
        { 
            this.HttpResponseHeaderHelper = httpResponseHeaderHelper;
            this.Ranges = ranges;
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
                    var partialContentLength = GetMultipartPartialRequestLength(Ranges, entity.ContentType, entity.ContentLength);
                    HttpResponseHeaderHelper.AppendHeader(response, HTTP_HEADER_CONTENT_LENGTH, partialContentLength.ToString());
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

            response.ContentType = MULTIPART_CONTENTTYPE;
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

            TransmitMultiPartFile(response, transmitEntity.Entity.ContentType, transmitEntity.Entity.ContentLength, Ranges, transmitEntity);
        }

        /// <summary>
        /// Generate a multiple part header for multipart byte range request.
        /// </summary>
        /// <param name="contentType"></param>
        /// <param name="contentLength"></param>
        /// <param name="startRange"></param>
        /// <param name="endRange"></param>
        /// <returns></returns>
        protected virtual string GenerateMultiPartHeader(string contentType, long contentLength, long startRange, long endRange)
        {
            var multiPartHeader = new StringBuilder();

            multiPartHeader.AppendLine();
            multiPartHeader.AppendLine("--" + MULTIPART_BOUNDARY);
            multiPartHeader.AppendLine(HTTP_HEADER_CONTENT_TYPE + ": " + contentType);
            multiPartHeader.AppendLine(HTTP_HEADER_CONTENT_RANGE + ": " + BYTES + " " +
                                      startRange + "-" +
                                      endRange + "/" +
                                      contentLength);
            multiPartHeader.AppendLine();

            return multiPartHeader.ToString();
        }

        /// <summary>
        /// Get the multipart footer.
        /// </summary>
        /// <returns></returns>
        protected virtual string GenerateMultiPartFooter()
        {
            var multiPartFooter = new StringBuilder();
            multiPartFooter.AppendLine();
            multiPartFooter.AppendLine("--" + MULTIPART_BOUNDARY + "--");
            return multiPartFooter.ToString();
        }

        /// <summary>
        /// Calculate the content length of a partial response
        /// </summary>
        /// <param name="ranges">The ranges that must be sent to the browser</param>
        /// <param name="contentType">The mime type of the entity</param>
        /// <param name="contentLength">The length of the entity</param>
        /// <returns></returns>
        protected virtual long GetMultipartPartialRequestLength(IEnumerable<RangeItem> ranges, string contentType, long contentLength)
        {
            var partialContentLength = 0L;

            foreach (var range in ranges)
            {
                var multiPartHeader = GenerateMultiPartHeader(contentType, contentLength, range.StartRange, range.EndRange);
                partialContentLength += multiPartHeader.Length;
                partialContentLength += range.EndRange - range.StartRange + 1;
            }
            var multiPartFooter = GenerateMultiPartFooter();
            partialContentLength += multiPartFooter.Length;
            return partialContentLength;
        }

        /// <summary>
        /// Transmit file ranges to browser,
        /// </summary>
        /// <param name="response">The <see cref="HttpResponse" /> of the current HTTP request.</param>
        /// <param name="contentType">The content type of the entity.</param>
        /// <param name="contentLength">The length of the entity.</param>
        /// <param name="ranges">A list of ranges to send to the browser.</param>
        /// <param name="transmitEntity"></param>
        protected virtual void TransmitMultiPartFile(HttpResponseBase response, string contentType, long contentLength, IEnumerable<RangeItem> ranges, ITransmitEntityStrategy transmitEntity)
        {
            foreach (var range in ranges)
            {
                var multiPartHeader = GenerateMultiPartHeader(contentType, contentLength, range.StartRange, range.EndRange);
                response.Output.Write(multiPartHeader);

                var bytesToRead = range.EndRange - range.StartRange + 1;
                transmitEntity.Transmit(response, range.StartRange, bytesToRead);
                transmitEntity.TransmitComplete(response);
            }
            var multiPartFooter = GenerateMultiPartFooter();
            response.Output.Write(multiPartFooter);
        }
    }
}
