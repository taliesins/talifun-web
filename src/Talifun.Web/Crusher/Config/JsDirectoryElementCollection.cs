using System.Configuration;
using Talifun.Web.Configuration;

namespace Talifun.Web.Crusher.Config
{
    /// <summary>
    /// Represents a configuration element containing a collection of <see cref="JsDirectoryElement" /> configuration elements.
    /// </summary>
    [ConfigurationCollection(typeof(JsFileElement))]
    public sealed class JsDirectoryElementCollection : CurrentConfigurationElementCollection<JsFileElement>
    {
        public JsDirectoryElementCollection()
        {
            AddElementName = "directory";
        }
    }
}