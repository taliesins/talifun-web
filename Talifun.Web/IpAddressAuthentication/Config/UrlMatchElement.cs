using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using Talifun.Web.Configuration;

namespace Talifun.Web.IpAddressAuthentication.Config
{
    public sealed class UrlMatchElement : NamedConfigurationElement
    {
        private static ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
        private static readonly ConfigurationProperty name = new ConfigurationProperty("name", typeof(string), null, ConfigurationPropertyOptions.IsRequired);
        private static readonly ConfigurationProperty expression = new ConfigurationProperty("expression", typeof(string), null, ConfigurationPropertyOptions.IsRequired);
        private static readonly ConfigurationProperty defaultAccess = new ConfigurationProperty("defaultAccess", typeof(bool), false, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty ipAddressMatches = new ConfigurationProperty("ipAddressMatches", typeof(IpAddressMatchElementCollection), null, ConfigurationPropertyOptions.None | ConfigurationPropertyOptions.IsDefaultCollection);
       
        /// <summary>
        /// Initializes the <see cref="UrlMatchElement"/> class.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static UrlMatchElement()
        {
            properties.Add(name);
            properties.Add(expression);
            properties.Add(defaultAccess);
            properties.Add(ipAddressMatches);
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
        /// Gets or sets the expression to use for matching against the url.
        /// </summary>
        [ConfigurationProperty("expression", DefaultValue = null, IsRequired = true)]
        public string Expression
        {
            get { return ((string)base[expression]); }
            set { base[expression] = value; }
        }

        /// <summary>
        /// Gets or sets the default access for ip addresses that do not match any rules.
        /// </summary>
        [ConfigurationProperty("defaultAccess", DefaultValue = false, IsRequired = true)]
        public bool DefaultAccess
        {
            get { return ((bool)base[defaultAccess]); }
            set { base[defaultAccess] = value; }
        }

        /// <summary>
        /// Gets a <see cref="IpAddressMatchElementCollection" /> containing the <see cref="ProviderSettingsCollection" /> elements
        /// for the conversion type to run upon matching.
        /// </summary>
        /// <value>A <see cref="IpAddressMatchElement" /> containing the configuration elements associated with this configuration section.</value>
        [ConfigurationProperty("ipAddressMatches", DefaultValue = null, IsDefaultCollection = true)]
        public IpAddressMatchElementCollection IpAddressMatches
        {
            get { return ((IpAddressMatchElementCollection)base[ipAddressMatches]); }
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
