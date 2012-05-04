using System.Configuration;
using Talifun.Web.Configuration;

namespace Talifun.Web.Crusher.Config
{
    /// <summary>
    /// Represents a configuration element containing a collection of <see cref="CssDirectoryElement" /> configuration elements.
    /// </summary>
    [ConfigurationCollection(typeof(CssDirectoryElement))]
    public sealed class CssDirectoryElementCollection : CurrentConfigurationElementCollection<CssDirectoryElement>
    {
        public CssDirectoryElementCollection()
        {
            AddElementName = "directory";
        }
    }
}