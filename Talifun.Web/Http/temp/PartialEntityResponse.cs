using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Web;

namespace Talifun.Web
{
    public class PartialEntityResponse : IEntityResponse
    {
        protected readonly IHttpResponseHeaderHelper HttpResponseHeaderHelper;
        protected readonly IByteRangeResponse ByteRangeResponse;

        public PartialEntityResponse(IHttpResponseHeaderHelper httpResponseHeaderHelper, IEnumerable<RangeItem> ranges)
        {
            this.HttpResponseHeaderHelper = httpResponseHeaderHelper;

            if (ranges.Count() == 1)
            {
                //Single byte range request
                this.ByteRangeResponse = new SingleByteRangeResponse(HttpResponseHeaderHelper, ranges.Single());
            }
            else
            {
                //Multiple byte range request
                this.ByteRangeResponse = new MultipleByteRangeResponse(HttpResponseHeaderHelper, ranges);
            }
        }

        /// <summary>
        /// Setups the headers required to send a partial response.
        /// </summary>
        /// <param name="response">An HTTP response.</param>
        /// <param name="compressionType">The compression type that request wants it sent back in.</param>
        /// <param name="entity"></param>
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
                    ByteRangeResponse.SetContentLength(response, entity);
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

            ByteRangeResponse.SetOtherHeaders(response, entity);
        }

        /// <summary>
        /// Sends ranges of a stream to the browser.
        /// </summary>
        /// <param name="requestHttpMethod">The http method for the HTTP request.</param>
        /// <param name="response">An HTTP response.</param>
        /// <param name="transmitEntity"></param>
        public void SendBody(HttpMethod requestHttpMethod, HttpResponseBase response, ITransmitEntityStrategy transmitEntity)
        {
            ByteRangeResponse.SendBody(requestHttpMethod, response, transmitEntity);
        }
    }
}
