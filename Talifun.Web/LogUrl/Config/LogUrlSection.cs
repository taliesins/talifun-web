using System.Configuration;
using System.Diagnostics.CodeAnalysis;

namespace Talifun.Web.LogUrl.Config
{
    public sealed class LogUrlSection : ConfigurationSection
    {
        private static ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
        private static readonly ConfigurationProperty urlMatches = new ConfigurationProperty("urlMatches", typeof(UrlMatchElementCollection), null, ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsDefaultCollection);

        /// <summary>
        /// Perform static initialisation for this configuration section. This includes explicitly adding
        /// configured properties to the Properties collection, and so cannot be performed inline.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static LogUrlSection()
        {
            properties.Add(urlMatches);
        }

        /// <summary>
        /// Gets a <see cref="UrlMatchElementCollection" /> containing the configuration elements associated with this configuration section.
        /// </summary>
        /// <value>A <see cref="UrlMatchElementCollection" /> containing the configuration elements associated with this configuration section.</value>
        [ConfigurationProperty("urlMatches", DefaultValue = null, IsRequired = true, IsDefaultCollection = true)]
        public UrlMatchElementCollection UrlMatches
        {
            get { return ((UrlMatchElementCollection)base[urlMatches]); }
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