using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using Talifun.Web.Configuration;

namespace Talifun.Web.CssSprite.Config
{
    public sealed class ImageFileElement : NamedConfigurationElement
    {
        private static ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
        private static readonly ConfigurationProperty name = new ConfigurationProperty("name", typeof(string), null, ConfigurationPropertyOptions.IsRequired);
        private static readonly ConfigurationProperty filePath = new ConfigurationProperty("filePath", typeof(string), null, ConfigurationPropertyOptions.IsRequired);

         /// <summary>
        /// Initializes the <see cref="ImageFileElement"/> class.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static ImageFileElement()
        {
            properties.Add(name);
            properties.Add(filePath);
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
        /// Gets or sets the file path for the image file
        /// </summary>
        [ConfigurationProperty("filePath", DefaultValue = null, IsRequired = true)]
        public string FilePath
        {
            get { return ((string)base[filePath]); }
            set { base[filePath] = value; }
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
