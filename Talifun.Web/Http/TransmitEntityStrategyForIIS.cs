using System.Web;

namespace Talifun.Web
{
    /// <summary>
    /// Optimized strategy for sending a file to http response stream when serving from IIS web server.
    /// </summary>
    public class TransmitEntityStrategyForIIS : ITransmitEntityStrategy
    {
        public IEntity Entity { get; private set; }
        protected readonly string FileName;

        public TransmitEntityStrategyForIIS(IEntity entity, string fileName)
        {
            Entity = entity;
            this.FileName = fileName;
        }

        #region ITransmitEntityStrategy Members

        public void Transmit(HttpResponseBase response)
        {
            response.TransmitFile(FileName);
        }

        public void Transmit(HttpResponseBase response, long offset, long length)
        {
            response.TransmitFile(FileName, offset, length);
        }

        public void TransmitComplete(HttpResponseBase response)
        {
            //TransmitFile is asynchronous make it syncronous
            //http://stackoverflow.com/questions/2275894/calling-response-transmitfile-from-static-method
            response.Flush();
        }

        #endregion
    }
}
