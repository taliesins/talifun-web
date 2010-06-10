using System.Configuration;
using Talifun.Web.Configuration;

namespace Talifun.Web.CssSprite.Config
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
