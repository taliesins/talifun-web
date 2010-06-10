using System.Configuration;
using Talifun.Web.Configuration;

namespace Talifun.Web.Crusher.Config
{
    /// <summary>
    /// Represents a configuration element containing a collection of <see cref="JsFileElement" /> configuration elements.
    /// </summary>
    [ConfigurationCollection(typeof(JsFileElement))]
    public sealed class JsFileElementCollection : CurrentConfigurationElementCollection<JsFileElement>
    {
        public JsFileElementCollection()
        {
            AddElementName = "file";
        }
    }
}