using System.Web;

namespace Talifun.Web
{
    public interface ITransmitEntityStrategy
    {
        /// <summary>
        /// The entity to transmit.
        /// </summary>
        IEntity Entity { get; }

        /// <summary>
        /// Transmit entire entity to http response stream.
        /// </summary>
        /// <param name="response">Http response stream.</param>
        void Transmit(HttpResponseBase response);

        /// <summary>
        /// Transmit part of entity to http response stream.
        /// </summary>
        /// <param name="response">Http response stream.</param>
        /// <param name="offset">Offset to start from.</param>
        /// <param name="length">Length of bytes to serve.</param>
        void Transmit(HttpResponseBase response, long offset, long length);

        /// <summary>
        /// Method should block until transfer is complete. This is needed when serving multi byte ranges requests.
        /// </summary>
        /// <param name="response">Http response stream.</param>
        void TransmitComplete(HttpResponseBase response);
    }
}
