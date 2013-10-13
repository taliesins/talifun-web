using System.Configuration;
using Talifun.Web.Configuration;

namespace Talifun.Crusher.Configuration.Sprites
{
    /// <summary>
    /// Represents a configuration element containing a collection of <see cref="ImageDirectoryElement" /> configuration elements.
    /// </summary>
    [ConfigurationCollection(typeof(ImageFileElement))]
    public sealed class ImageDirectoryElementCollection : CurrentConfigurationElementCollection<ImageDirectoryElement>
    {
        public ImageDirectoryElementCollection()
        {
            AddElementName = "directory";
        }
    }
}
