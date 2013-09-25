using System.IO;
using System.Reflection;

namespace Talifun.Web
{
    public interface IEmbeddedResourceLoader
    {
        string LoadEmbeddedResource(Assembly assembly, string resourcePath);
        void LoadEmbeddedResource(Stream response, Assembly assembly, string resourcePath, int bufferSize);
        void LoadEmbeddedResource(Stream response, Assembly assembly, string resourcePath, int bufferSize, long offset, long length);
    }
}