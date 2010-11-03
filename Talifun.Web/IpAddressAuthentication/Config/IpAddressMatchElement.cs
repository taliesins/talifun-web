using System;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using Talifun.Web.Configuration;

namespace Talifun.Web.IpAddressAuthentication.Config
{
    public class IpAddressMatchElement : NamedConfigurationElement
    {
        private static ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
        private static readonly ConfigurationProperty name = new ConfigurationProperty("name", typeof(string), null, ConfigurationPropertyOptions.IsRequired);
        private static readonly ConfigurationProperty ipAddress = new ConfigurationProperty("ipAddress", typeof(string), null, ConfigurationPropertyOptions.IsRequired);
        private static readonly ConfigurationProperty netMask = new ConfigurationProperty("netMask", typeof(string), null, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty access = new ConfigurationProperty("access", typeof(Boolean), null, ConfigurationPropertyOptions.IsRequired);

        /// <summary>
        /// Initializes the <see cref="RegexUrlAuthorization.Config.UrlMatchElement"/> class.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static IpAddressMatchElement()
        {
            properties.Add(name);
            properties.Add(ipAddress);
            properties.Add(netMask);
            properties.Add(access);
        }

        /// <summary>
        /// Gets or sets the name of the configuration element represented by this instance.
        /// </summary>
        [ConfigurationProperty("name", DefaultValue = null, IsRequired = true, IsKey = true)]
        public override string Name
        {
            get { return ((string)base[name]); }
            set { base[name] = value; }
        }

        /// <summary>
        /// Gets or sets the name of the configuration element represented by this instance.
        /// </summary>
        [ConfigurationProperty("ipAddress", DefaultValue = null, IsRequired = true, IsKey = false)]
        public IPAddress IpAddress
        {
            get
            {
                var ipAddressString = (string)base[ipAddress];
                return string.IsNullOrEmpty(ipAddressString) ? null : IPAddress.Parse(ipAddressString);
            }
            set { base[ipAddress] = value; }
        }

        /// <summary>
        /// Gets or sets the name of the configuration element represented by this instance.
        /// </summary>
        [ConfigurationProperty("netMask", DefaultValue = null, IsRequired = false, IsKey = false)]
        public IPAddress NetMask
        {
            get
            {
                var netMaskString = (string)base[netMask];
                return string.IsNullOrEmpty(netMaskString) ? null : IPAddress.Parse(netMaskString);
            }
            set { base[netMask] = value; }
        }

        /// <summary>
        /// Gets or sets the name of the configuration element represented by this instance.
        /// </summary>
        [ConfigurationProperty("access", DefaultValue = null, IsRequired = true, IsKey = false)]
        public bool Access
        {
            get { return ((bool)base[access]); }
            set { base[access] = value; }
        }

        /// <summary>
        /// Gets the collection of configuration properties contained by this configuration element.
        /// </summary>
        /// <returns>The <see cref="T:System.Configuration.ConfigurationPropertyCollection"></see> collection of properties for the element.</returns>
        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return properties;
            }
        }
    }
}
