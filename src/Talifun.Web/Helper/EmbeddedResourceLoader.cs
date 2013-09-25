using System;
using System.IO;
using System.Reflection;

namespace Talifun.Web
{
    public class EmbeddedResourceLoader : IEmbeddedResourceLoader
    {
        public virtual string LoadEmbeddedResource(Assembly assembly, string resourcePath)
        {
            var assemblyName = assembly.GetName().Name;
            var embeddedResourcePath = string.Format("{0}.{1}", assemblyName, (resourcePath.StartsWith("/") ? resourcePath.Substring(1) : resourcePath).Replace("/", "."));

            using (var stream = assembly.GetManifestResourceStream(embeddedResourcePath))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        public virtual void LoadEmbeddedResource(Stream response, Assembly assembly, string resourcePath, int bufferSize)
        {
            var assemblyName = assembly.GetName().Name;
            var embeddedResourcePath = string.Format("{0}.{1}", assemblyName, (resourcePath.StartsWith("/") ? resourcePath.Substring(1) : resourcePath).Replace("/", "."));

            using (var stream = assembly.GetManifestResourceStream(embeddedResourcePath))
            {
                var buffer = new byte[bufferSize];
                var readCount = 0;
                while ((readCount = stream.Read(buffer, 0, bufferSize)) > 0)
                {
                    response.Write(buffer, 0, readCount);
                }
            }
        }

        public virtual void LoadEmbeddedResource(Stream response, Assembly assembly, string resourcePath, int bufferSize, long offset, long length)
        {
            var assemblyName = assembly.GetName().Name;
            var embeddedResourcePath = string.Format("{0}.{1}", assemblyName, (resourcePath.StartsWith("/") ? resourcePath.Substring(1) : resourcePath).Replace("/", "."));

            using (var stream = assembly.GetManifestResourceStream(embeddedResourcePath))
            {
                stream.Seek(offset, SeekOrigin.Begin);

                var buffer = new byte[bufferSize];
                while (length > 0)
                {
                    var lengthOfReadChunk = stream.Read(buffer, 0, (int)Math.Min(bufferSize, length));

                    // Write the data to the current output stream.
                    response.Write(buffer, 0, lengthOfReadChunk);

                    // Reduce BytesToRead
                    length -= lengthOfReadChunk;
                }
            }
        }
    }
}
