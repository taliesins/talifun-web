using System.Configuration;
using Talifun.Web.Configuration;

namespace Talifun.Web.Crusher.Config
{
    /// <summary>
    /// Represents a configuration element containing a collection of <see cref="CssFileElement" /> configuration elements.
    /// </summary>
    [ConfigurationCollection(typeof(CssFileElement))]
    public sealed class CssFileElementCollection : CurrentConfigurationElementCollection<CssFileElement>
    {
        public CssFileElementCollection()
        {
            AddElementName = "file";
        }
    }
}