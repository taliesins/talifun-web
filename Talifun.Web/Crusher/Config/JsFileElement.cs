using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using Talifun.Web.Configuration;

namespace Talifun.Web.Crusher.Config
{
    /// <summary>
    /// Represents the configuration for a js file element.
    /// </summary>
    public sealed class JsFileElement : NamedConfigurationElement
    {
        private static ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
        private static readonly ConfigurationProperty name = new ConfigurationProperty("name", typeof(string), null, ConfigurationPropertyOptions.IsRequired);
        private static readonly ConfigurationProperty filePath = new ConfigurationProperty("filePath", typeof(string), null, ConfigurationPropertyOptions.IsRequired);
        private static readonly ConfigurationProperty compressionType = new ConfigurationProperty("compressionType", typeof(JsCompressionType), JsCompressionType.Min, ConfigurationPropertyOptions.None);

        /// <summary>
        /// Initializes the <see cref="JsFileElement"/> class.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static JsFileElement()
        {
            properties.Add(name);
            properties.Add(filePath);
            properties.Add(compressionType);
        }

        /// <summary>
        /// The name of the configuration element represented by this instance.
        /// </summary>
        [ConfigurationProperty("name", DefaultValue = null, IsRequired = true, IsKey = true)]
        public override string Name
        {
            get { return ((string)base[name]); }
            set { base[name] = value; }
        }

        /// <summary>
        /// The file path for the css file
        /// </summary>
        [ConfigurationProperty("filePath", DefaultValue = null, IsRequired = true)]
        public string FilePath
        {
            get { return ((string)base[filePath]); }
            set { base[filePath] = value; }
        }

        /// <summary>
        /// The compression type to use for the css file
        /// </summary>
        [ConfigurationProperty("compressionType", DefaultValue = JsCompressionType.Min, IsRequired = false)]
        public JsCompressionType CompressionType
        {
            get { return ((JsCompressionType)base[compressionType]); }
            set { base[compressionType] = value; }
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