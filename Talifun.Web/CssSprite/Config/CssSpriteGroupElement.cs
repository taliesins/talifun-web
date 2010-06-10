using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using Talifun.Web.Configuration;

namespace Talifun.Web.CssSprite.Config
{
    public sealed class CssSpriteGroupElement: NamedConfigurationElement
    {
        private static ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
        private static readonly ConfigurationProperty name = new ConfigurationProperty("name", typeof(string), null, ConfigurationPropertyOptions.IsRequired);
        private static readonly ConfigurationProperty cssOutputFilePath = new ConfigurationProperty("cssOutputFilePath", typeof(string), null, ConfigurationPropertyOptions.IsRequired);
        private static readonly ConfigurationProperty imageOutputFilePath = new ConfigurationProperty("imageOutputFilePath", typeof(string), null, ConfigurationPropertyOptions.IsRequired);
        private static readonly ConfigurationProperty cssUrl = new ConfigurationProperty("cssUrl", typeof(string), null, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty imageUrl = new ConfigurationProperty("imageUrl", typeof(string), null, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty files = new ConfigurationProperty("files", typeof(ImageFileElementCollection), null, ConfigurationPropertyOptions.None | ConfigurationPropertyOptions.IsDefaultCollection);

         /// <summary>
        /// Initializes the <see cref="CssSpriteGroupElement"/> class.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static CssSpriteGroupElement()
        {
            properties.Add(name);
            properties.Add(cssOutputFilePath);
            properties.Add(imageOutputFilePath);
            properties.Add(cssUrl);
            properties.Add(imageUrl);
            properties.Add(files);
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
        /// Gets or sets the file path for the output css file for the sprite
        /// </summary>
        [ConfigurationProperty("cssOutputFilePath", DefaultValue = null, IsRequired = true)]
        public string CssOutputFilePath
        {
            get { return ((string)base[cssOutputFilePath]); }
            set { base[cssOutputFilePath] = value; }
        }

        /// <summary>
        /// Gets or sets the file path for the output image file for the sprite
        /// </summary>
        [ConfigurationProperty("imageOutputFilePath", DefaultValue = null, IsRequired = true)]
        public string ImageOutputFilePath
        {
            get { return ((string)base[imageOutputFilePath]); }
            set { base[imageOutputFilePath] = value; }
        }

        /// <summary>
        /// Gets or sets the url for output css file for the sprite.
        /// </summary>
        /// <remarks>
        /// Use this to set the url to a CDN.
        /// </remarks>
        [ConfigurationProperty("cssUrl", DefaultValue = null, IsRequired = false)]
        public string CssUrl
        {
            get { return ((string)base[cssUrl]); }
            set { base[cssUrl] = value; }
        }

        /// <summary>
        /// Gets or sets the url for output image file for the sprite.
        /// </summary>
        /// <remarks>
        /// Use this to set the url to a CDN.
        /// </remarks>
        [ConfigurationProperty("imageUrl", DefaultValue = null, IsRequired = false)]
        public string ImageUrl
        {
            get { return ((string)base[imageUrl]); }
            set { base[imageUrl] = value; }
        }

        /// <summary>
        /// Gets a <see cref="ImageFileElementCollection" /> containing the <see cref="ProviderSettingsCollection" /> elements
        /// for the conversion type to run upon matching.
        /// </summary>
        /// <value>A <see cref="ImageFileElement" /> containing the configuration elements associated with this configuration section.</value>
        [ConfigurationProperty("files", DefaultValue = null, IsDefaultCollection = true)]
        public ImageFileElementCollection Files
        {
            get { return ((ImageFileElementCollection)base[files]); }
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
