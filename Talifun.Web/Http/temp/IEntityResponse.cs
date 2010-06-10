using System.Web;

namespace Talifun.Web
{
    public interface IEntityResponse
    {
        /// <summary>
        /// Sends headers required to send an entity.
        /// </summary>
        /// <param name="response">An HTTP response.</param>
        /// <param name="compressionType">The compression type that request wants it sent back in.</param>
        /// <param name="entity">Entity to transmit.</param>
        void SendHeaders(HttpResponseBase response, ResponseCompressionType compressionType, IEntity entity);

        /// <summary>
        /// Sends entity to response body.
        /// </summary>
        /// <param name="requestHttpMethod">The http method for the HTTP request.</param>
        /// <param name="response">An HTTP response.</param>
        /// <param name="transmitEntity">The strategy to use to transmit entity to http response.</param>
        void SendBody(HttpMethod requestHttpMethod, HttpResponseBase response, ITransmitEntityStrategy transmitEntity);
    }
}