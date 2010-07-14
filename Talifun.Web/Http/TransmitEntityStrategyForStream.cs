using System;
using System.IO;
using System.Web;

namespace Talifun.Web
{
    /// <summary>
    /// Optimized strategy for sending a stream to http response stream.
    /// </summary>
    public class TransmitEntityStrategyForStream : ITransmitEntityStrategy
    {
        public IEntity Entity { get; private set; }
        protected readonly Stream Stream;
        protected readonly int BufferSize;

        public TransmitEntityStrategyForStream(IEntity entity, Stream stream, int bufferSize)
        {
            Entity = entity;
            this.Stream = stream;
            this.BufferSize = bufferSize;
        }

        #region ITransmitEntityStrategy Members

        public void Transmit(HttpResponseBase response)
        {
            TransmitFile(response, Stream, BufferSize);
        }

        public void Transmit(HttpResponseBase response, long offset, long length)
        {
            TransmitFile(response, Stream, BufferSize, offset, length);
        }

        public void TransmitComplete(HttpResponseBase response)
        {
        }

        /// <summary>
        /// Transmit stream to browser.
        /// </summary>
        /// <param name="response">The <see cref="HttpResponse" /> of the current HTTP request.</param>
        /// <param name="stream">A <see cref="FileInfo" /> we are going to transmit to the browser.</param>
        /// <param name="bufferSize">The buffer size to use when transmitting file to browser.</param>
        public virtual void TransmitFile(HttpResponseBase response, Stream stream, int bufferSize)
        {
            var buffer = new byte[bufferSize];
            var readCount = 0;
            while ((readCount = stream.Read(buffer, 0, bufferSize)) > 0)
            {
                response.OutputStream.Write(buffer, 0, readCount);
            }
        }

        /// <summary>
        /// Transmit stream range to browser.
        /// </summary>
        /// <param name="response">The <see cref="HttpResponse" /> of the current HTTP request.</param>
        /// <param name="stream">A <see cref="FileInfo" /> we are going to transmit to the browser.</param>
        /// <param name="bufferSize">The buffer size to use when transmitting file to browser.</param>
        /// <param name="startRange">Start range</param>
        /// <param name="endRange">End range</param>
        public virtual void TransmitFile(HttpResponseBase response, Stream stream, long bufferSize, long startRange, long endRange)
        {
            stream.Seek(startRange, SeekOrigin.Begin);

            var bytesToRead = endRange - startRange + 1;
            var buffer = new byte[bufferSize];
            while (bytesToRead > 0)
            {
                var lengthOfReadChunk = stream.Read(buffer, 0, (int)Math.Min(bufferSize, bytesToRead));

                // Write the data to the current output stream.
                response.OutputStream.Write(buffer, 0, lengthOfReadChunk);

                // Reduce BytesToRead
                bytesToRead -= lengthOfReadChunk;
            }
        }
        #endregion
    }
}
