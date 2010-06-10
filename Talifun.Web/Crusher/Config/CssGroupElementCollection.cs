using System.Configuration;
using Talifun.Web.Configuration;

namespace Talifun.Web.Crusher.Config
{
    /// <summary>
    /// Represents a configuration element containing a collection of <see cref="CssGroupElement" /> configuration elements.
    /// </summary>
    [ConfigurationCollection(typeof(CssGroupElement))]
    public sealed class CssGroupElementCollection : CurrentConfigurationElementCollection<CssGroupElement>
    {
        public CssGroupElementCollection()
        {
            AddElementName = "cssGroup";
        }
    }
}