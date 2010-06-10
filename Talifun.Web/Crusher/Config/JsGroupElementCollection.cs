using System.Configuration;
using Talifun.Web.Configuration;

namespace Talifun.Web.Crusher.Config
{
    /// <summary>
    /// Represents a configuration element containing a collection of <see cref="JsGroupElement" /> configuration elements.
    /// </summary>
    [ConfigurationCollection(typeof(JsGroupElement))]
    public sealed class JsGroupElementCollection : CurrentConfigurationElementCollection<JsGroupElement>
    {
        public JsGroupElementCollection()
        {
            AddElementName = "jsGroup";
        }
    }
}