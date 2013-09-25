using System;
using System.IO;
using System.Reflection;
using System.Web;

namespace Talifun.Web
{
    public class TransmitEntityStrategyForEmbeddedResource : ITransmitEntityStrategy
    {
        private readonly IEmbeddedResourceLoader _embeddedResourceLoader;
        public IEntity Entity { get; private set; }
        protected readonly Assembly Assembly;
        protected readonly string ResourcePath;
        protected readonly int BufferSize;

        public TransmitEntityStrategyForEmbeddedResource(IEmbeddedResourceLoader embeddedResourceLoader, IEntity entity, Assembly assembly, string resourcePath, int bufferSize)
        {
            Entity = entity;
            _embeddedResourceLoader = embeddedResourceLoader;
            Assembly = assembly;
            ResourcePath = resourcePath;
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
            _embeddedResourceLoader.LoadEmbeddedResource(response.OutputStream, assembly, resourcePath, bufferSize);
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
            _embeddedResourceLoader.LoadEmbeddedResource(response.OutputStream, assembly, resourcePath, (int)bufferSize, offset, length);
        }
        #endregion
    }
}
