using System.Configuration;
using Talifun.Web.Configuration;

namespace Talifun.Crusher.Configuration.Sprites
{
    /// <summary>
    /// Represents a configuration element containing a collection of <see cref="ImageFileElement" /> configuration elements.
    /// </summary>
    [ConfigurationCollection(typeof(ImageFileElement))]
    public sealed class ImageFileElementCollection : CurrentConfigurationElementCollection<ImageFileElement>
    {
        public ImageFileElementCollection()
        {
            AddElementName = "file";
        }
    }
}
