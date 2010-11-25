using System;
using System.IO;
using System.Web;

namespace Talifun.Web
{
    /// <summary>
    /// Optimized strategy for sending a file to http response stream.
    /// </summary>
    public class TransmitEntityStrategyForFile : ITransmitEntityStrategy
    {
        protected readonly IRetryableFileOpener RetryableFileOpener;
        public IEntity Entity { get; private set;}
        protected readonly FileInfo File;
        protected readonly int BufferSize;
        
        public TransmitEntityStrategyForFile(IRetryableFileOpener retryableFileOpener, IEntity entity, FileInfo file, int bufferSize)
        {
            this.RetryableFileOpener = retryableFileOpener;
            Entity = entity;
            this.File = file;
            this.BufferSize = bufferSize;
        }

        public void Transmit(HttpResponseBase response)
        {
            TransmitFile(RetryableFileOpener, response, File, BufferSize);
        }

        public void Transmit(HttpResponseBase response, long offset, long length)
        {
            TransmitFile(RetryableFileOpener, response, File, BufferSize, offset, length);
        }

        public void TransmitComplete(HttpResponseBase response)
        {
        }

        /// <summary>
        /// Transmit stream to browser.
        /// </summary>
        /// <param name="retryableFileOpener"></param>
        /// <param name="response">The <see cref="HttpResponse" /> of the current HTTP request.</param>
        /// <param name="file"></param>
        /// <param name="bufferSize">The buffer size to use when transmitting file to browser.</param>
        public virtual void TransmitFile(IRetryableFileOpener retryableFileOpener, HttpResponseBase response, FileInfo file, int bufferSize)
        {
            using (var stream = retryableFileOpener.OpenFileStream(file, 5, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var buffer = new byte[bufferSize];
                var readCount = 0;
                while ((readCount = stream.Read(buffer, 0, bufferSize)) > 0)
                {
                    response.OutputStream.Write(buffer, 0, readCount);
                }
            }
        }

        /// <summary>
        /// Transmit stream range to browser.
        /// </summary>
        /// <param name="retryableFileOpener"></param>
        /// <param name="response">The <see cref="HttpResponse" /> of the current HTTP request.</param>
        /// <param name="file"></param>
        /// <param name="bufferSize">The buffer size to use when transmitting file to browser.</param>
        /// <param name="offset">Start range</param>
        /// <param name="length">End range</param>
        public virtual void TransmitFile(IRetryableFileOpener retryableFileOpener, HttpResponseBase response, FileInfo file, long bufferSize, long offset, long length)
        {
            using (var stream = retryableFileOpener.OpenFileStream(file, 5, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                stream.Seek(offset, SeekOrigin.Begin);

                var buffer = new byte[bufferSize];
                while (length > 0)
                {
                    var lengthOfReadChunk = stream.Read(buffer, 0, (int)Math.Min(bufferSize, length));

                    // Write the data to the current output stream.
                    response.OutputStream.Write(buffer, 0, lengthOfReadChunk);

                    // Reduce BytesToRead
                    length -= lengthOfReadChunk;
                }
            }
        }
    }
}