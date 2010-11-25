using System;
using System.IO.Compression;
using System.Web;

namespace Talifun.Web
{
    public class FullEntityResponse : IEntityResponse
    {
        public const string HttpHeaderContentLength = "Content-Length";

        protected readonly IHttpResponseHeaderHelper HttpResponseHeaderHelper;

        public FullEntityResponse(IHttpResponseHeaderHelper httpResponseHeaderHelper)
        { 
            this.HttpResponseHeaderHelper = httpResponseHeaderHelper;
        }

        /// <summary>
        /// Sends headers required to send an entity.
        /// </summary>
        /// <param name="response">An HTTP response.</param>
        /// <param name="compressionType">The compression type that request wants it sent back in.</param>
        /// <param name="entity"></param>
        public void SendHeaders(HttpResponseBase response, ResponseCompressionType compressionType, IEntity entity)
        {
            if (!(entity.CompressionType == ResponseCompressionType.None || entity.CompressionType == compressionType))
            {
                throw new NotImplementedException("Need to decode in memory, and then stream it");
            }

            HttpResponseHeaderHelper.SetContentEncoding(response, compressionType);

            //How should data be compressed
            if (entity.CompressionType == compressionType)
            {
                //We have the entity stored in the correct compression format so just stream it    
                HttpResponseHeaderHelper.AppendHeader(response, HttpHeaderContentLength, entity.ContentLength.ToString());
            }
            else if (entity.CompressionType == ResponseCompressionType.None)
            {
                switch (compressionType)
                {
                    case ResponseCompressionType.GZip:
                        response.Filter = new GZipStream(response.Filter, CompressionMode.Compress);
                        //This means that the output stream will be chunked, so we don't have to worry about content length
                        break;
                    case ResponseCompressionType.Deflate:
                        response.Filter = new DeflateStream(response.Filter, CompressionMode.Compress);
                        //This means that the output stream will be chunked, so we don't have to worry about content length
                        break;
                }
            }

            response.ContentType = entity.ContentType;
        }

        /// <summary>
        /// Sends entity to response body.
        /// </summary>
        /// <param name="requestHttpMethod">The http method for the HTTP request.</param>
        /// <param name="response">An HTTP response.</param>
        /// <param name="transmitEntity">The strategy to use to transmit entity to http response.</param>
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

            transmitEntity.Transmit(response);
        }
    }
}
