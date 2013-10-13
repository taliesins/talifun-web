using System.Configuration;
using Talifun.Web.Configuration;

namespace Talifun.Crusher.Configuration.Js
{
    /// <summary>
    /// Represents a configuration element containing a collection of <see cref="JsDirectoryElement" /> configuration elements.
    /// </summary>
    [ConfigurationCollection(typeof(JsDirectoryElement))]
    public sealed class JsDirectoryElementCollection : CurrentConfigurationElementCollection<JsDirectoryElement>
    {
        public JsDirectoryElementCollection()
        {
            AddElementName = "directory";
        }
    }
}