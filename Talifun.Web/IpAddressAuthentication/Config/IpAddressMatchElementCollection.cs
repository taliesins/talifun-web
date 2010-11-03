using System.Configuration;
using Talifun.Web.Configuration;

namespace Talifun.Web.IpAddressAuthentication.Config
{
    /// <summary>
    /// Represents a configuration element containing a collection of <see cref="IpAddressMatchElement" /> configuration elements.
    /// </summary>
    [ConfigurationCollection(typeof(IpAddressMatchElement))]
    public sealed class IpAddressMatchElementCollection : CurrentConfigurationElementCollection<IpAddressMatchElement>
    {
        public IpAddressMatchElementCollection()
        {
            AddElementName = "ipAddressMatch";
        }
    }
}