using System.Web;

namespace Talifun.Web
{
    public interface IByteRangeResponse
    {
        void SetContentEncoding(HttpResponseBase response, ResponseCompressionType compressionType);
        void SetContentLength(HttpResponseBase response, IEntity entity);
        void SetOtherHeaders(HttpResponseBase response, IEntity entity);
        void SendBody(HttpMethod requestHttpMethod, HttpResponseBase response, ITransmitEntityStrategy transmitEntity);
    }
}