using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using Talifun.Web.LogUrl.Config;

namespace Talifun.Web.StaticFile.Config
{
    public sealed class StaticFileHandlerSection : ConfigurationSection
    {
        private static ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
        private static readonly ConfigurationProperty webServerType = new ConfigurationProperty("webServerType", typeof(WebServerType), WebServerType.NotSet, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty fileExtensionDefault = new ConfigurationProperty("fileExtensionDefault", typeof(FileExtensionDefaultElement), null, ConfigurationPropertyOptions.IsRequired);
        private static readonly ConfigurationProperty fileExtensions = new ConfigurationProperty("fileExtensions", typeof(FileExtensionElementCollection), null, ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsDefaultCollection);

        /// <summary>
        /// Perform static initialisation for this configuration section. This includes explicitly adding
        /// configured properties to the Properties collection, and so cannot be performed inline.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static StaticFileHandlerSection()
        {
            properties.Add(webServerType);
            properties.Add(fileExtensionDefault);
            properties.Add(fileExtensions);
        }

        /// <summary>
        /// Gets or sets the web server type used to serve requests.
        /// </summary>
        /// <remarks>
        /// Set this value if you need to run in medium trust, as it needs to use reflection to work out the web server type.
        /// </remarks>
        [ConfigurationProperty("webServerType", DefaultValue = WebServerType.NotSet, IsRequired = false)]
        public WebServerType WebServerType
        {
            get { return ((WebServerType)base[webServerType]); }
            set { base[webServerType] = value; }
        }

        /// <summary>
        /// Gets or sets the default values to use if the extension is not found.
        /// </summary>
        [ConfigurationProperty("fileExtensionDefault", DefaultValue = null, IsRequired = true)]
        public FileExtensionDefaultElement FileExtensionDefault
        {
            get { return ((FileExtensionDefaultElement)base[fileExtensionDefault]); }
            set { base[fileExtensionDefault] = value; }
        }

        /// <summary>
        /// Gets a <see cref="UrlMatchElementCollection" /> containing the configuration elements associated with this configuration section.
        /// </summary>
        /// <value>A <see cref="UrlMatchElementCollection" /> containing the configuration elements associated with this configuration section.</value>
        [ConfigurationProperty("fileExtensions", DefaultValue = null, IsRequired = true, IsDefaultCollection = true)]
        public FileExtensionElementCollection FileExtensions
        {
            get { return ((FileExtensionElementCollection)base[fileExtensions]); }
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