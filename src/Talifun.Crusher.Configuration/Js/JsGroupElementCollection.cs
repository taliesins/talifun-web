using System.Collections;
using System.Configuration;
using Talifun.Web.Configuration;

namespace Talifun.Crusher.Configuration.Js
{
    /// <summary>
    /// Represents a configuration element containing a collection of <see cref="JsGroupElement" /> configuration elements.
    /// </summary>
    [ConfigurationCollection(typeof(JsGroupElement))]
    public sealed class JsGroupElementCollection : CurrentConfigurationElementCollection<JsGroupElement>
    {
        public JsGroupElementCollection() : base(new CaseInsensitiveComparer())
        {
            AddElementName = "jsGroup";
        }
    }
}