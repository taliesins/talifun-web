using System;
using System.IO;
using System.Reflection;
using System.Web;

namespace Talifun.Web
{
    public class TransmitEntityStrategyForEmbeddedResource : ITransmitEntityStrategy
    {
        public IEntity Entity { get; private set; }
        protected readonly Assembly Assembly;
        protected readonly string ResourcePath;
        protected readonly int BufferSize;

        public TransmitEntityStrategyForEmbeddedResource(IEntity entity, Assembly assembly, string resourcePath, int bufferSize)
        {
            Entity = entity;
            Assembly = assembly;
            ResourcePath = string.Format("{0}.{1}", assembly.GetName().Name, resourcePath.Replace("/", "."));
            BufferSize = bufferSize;
        }

        #region ITransmitEntityStrategy Members

        public void Transmit(HttpResponseBase response)
        {
            TransmitFile(response, Assembly, ResourcePath, BufferSize);
        }

        public void Transmit(HttpResponseBase response, long offset, long length)
        {
            TransmitFile(response, Assembly, ResourcePath, BufferSize, offset, length);
        }

        public void TransmitComplete(HttpResponseBase response)
        {
        }

        /// <summary>
        /// Transmit stream to browser.
        /// </summary>
        /// <param name="response">The <see cref="HttpResponse" /> of the current HTTP request.</param>
        /// <param name="resourcePath"> </param>
        /// <param name="bufferSize">The buffer size to use when transmitting file to browser.</param>
        /// <param name="assembly"> </param>
        public virtual void TransmitFile(HttpResponseBase response, Assembly assembly, string resourcePath, int bufferSize)
        {
            using (var stream = assembly.GetManifestResourceStream(resourcePath))
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
        /// <param name="response">The <see cref="HttpResponse" /> of the current HTTP request.</param>
        /// <param name="resourcePath"> </param>
        /// <param name="bufferSize">The buffer size to use when transmitting file to browser.</param>
        /// <param name="offset">Start range</param>
        /// <param name="length">End range</param>
        /// <param name="assembly"> </param>
        public virtual void TransmitFile(HttpResponseBase response, Assembly assembly, string resourcePath, long bufferSize, long offset, long length)
        {
            using (var stream = assembly.GetManifestResourceStream(resourcePath))
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
        #endregion
    }
}
