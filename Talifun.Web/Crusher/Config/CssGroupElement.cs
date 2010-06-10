using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using Talifun.Web.Configuration;

namespace Talifun.Web.Crusher.Config
{
    /// <summary>
    /// Represents the configuration for a css group element.
    /// </summary>
    public sealed class CssGroupElement : NamedConfigurationElement
    {
        private static ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
        private static readonly ConfigurationProperty name = new ConfigurationProperty("name", typeof(string), null, ConfigurationPropertyOptions.IsRequired);
        private static readonly ConfigurationProperty outputFilePath = new ConfigurationProperty("outputFilePath", typeof(string), null, ConfigurationPropertyOptions.IsRequired);
        private static readonly ConfigurationProperty url = new ConfigurationProperty("url", typeof(string), null, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty media = new ConfigurationProperty("media", typeof(string), "screen", ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty debug = new ConfigurationProperty("debug", typeof(bool), null, ConfigurationPropertyOptions.IsRequired);
        private static readonly ConfigurationProperty files = new ConfigurationProperty("files", typeof(CssFileElementCollection), null, ConfigurationPropertyOptions.None | ConfigurationPropertyOptions.IsDefaultCollection);
        
        /// <summary>
        /// Initializes the <see cref="CssGroupElement"/> class.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static CssGroupElement()
        {
            properties.Add(name);
            properties.Add(outputFilePath);
            properties.Add(url);
            properties.Add(media);
            properties.Add(debug);
            properties.Add(files);
        }

        /// <summary>
        /// The name of the configuration element represented by this instance.
        /// </summary>
        /// <remarks>
        /// This is used as the key for caching so make sure that its unique for each group of css files
        /// </remarks>
        [ConfigurationProperty("name", DefaultValue = null, IsRequired = true, IsKey = true)]
        public override string Name
        {
            get { return ((string)base[name]); }
            set { base[name] = value; }
        }

        /// <summary>
        /// The file path for the output css file
        /// </summary>
        /// <remarks>
        /// This is used as the unique identifier for the group so ensure that it is unique,
        /// as cache key is based on it.
        /// </remarks>
        [ConfigurationProperty("outputFilePath", DefaultValue = null, IsRequired = true)]
        public string OutputFilePath
        {
            get { return ((string)base[outputFilePath]); }
            set { base[outputFilePath] = value; }
        }

        /// <summary>
        /// The url for output css file.
        /// </summary>
        /// <remarks>
        /// Use this to set the url to a CDN.
        /// </remarks>
        [ConfigurationProperty("url", DefaultValue = null, IsRequired = false)]
        public string Url
        {
            get { return ((string)base[url]); }
            set { base[url] = value; }
        }

        /// <summary>
        /// The css media to use
        /// </summary>
        [ConfigurationProperty("media", DefaultValue = "screen", IsRequired = true)]
        public string Media
        {
            get { return ((string)base[media]); }
            set { base[media] = value; }
        }

        /// <summary>
        /// Should the crushed version of the css file should be used
        /// </summary>
        [ConfigurationProperty("debug", DefaultValue = null, IsRequired = true)]
        public bool Debug
        {
            get { return ((bool)base[debug]); }
            set { base[debug] = value; }
        }

        /// <summary>
        /// Gets a <see cref="CssFileElementCollection" /> containing the <see cref="ProviderSettingsCollection" /> elements
        /// for the conversion type to run upon matching.
        /// </summary>
        /// <value>A <see cref="CssFileElement" /> containing the configuration elements associated with this configuration section.</value>
        [ConfigurationProperty("files", DefaultValue = null, IsDefaultCollection = true)]
        public CssFileElementCollection Files
        {
            get { return ((CssFileElementCollection)base[files]); }
        }

        /// <summary>
        /// The collection of configuration properties contained by this configuration element.
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