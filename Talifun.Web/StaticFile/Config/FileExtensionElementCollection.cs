using System.Configuration;
using Talifun.Web.Configuration;

namespace Talifun.Web.StaticFile.Config
{
    /// <summary>
    /// Represents a configuration element containing a collection of <see cref="FileExtensionElement" /> configuration elements.
    /// </summary>
    [ConfigurationCollection(typeof(FileExtensionElement))]
    public sealed class FileExtensionElementCollection : CurrentConfigurationElementCollection<FileExtensionElement>
    {
        public FileExtensionElementCollection()
        {
            AddElementName = "fileExtension";
        }
    }
}