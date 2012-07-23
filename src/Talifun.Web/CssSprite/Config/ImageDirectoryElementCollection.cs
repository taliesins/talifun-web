using System.Configuration;
using Talifun.Web.Configuration;

namespace Talifun.Web.CssSprite.Config
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
