using System.Web;

namespace Talifun.Web
{
    /// <summary>
    /// Optimized strategy for sending a byte array to http response stream.
    /// </summary>
    public class TransmitEntityStrategyForByteArray : ITransmitEntityStrategy
    {
        public IEntity Entity { get; private set; }
        protected readonly byte[] EntityData;
        public TransmitEntityStrategyForByteArray(IEntity entity, byte[] entityData)
        {
            Entity = entity;
            this.EntityData = entityData;
        }

        #region ITransmitEntityStrategy Members

        public void Transmit(HttpResponseBase response)
        {
            response.OutputStream.Write(EntityData, 0, EntityData.Length);            
        }

        public void Transmit(HttpResponseBase response, long offset, long length)
        {

            response.OutputStream.Write(EntityData, (int)offset, (int)length);            
        }

        public void TransmitComplete(HttpResponseBase response)
        {
        }

        #endregion
    }
}
