using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using Talifun.Web.Configuration;

namespace Talifun.Web.Crusher.Config
{
    /// <summary>
    /// Represents the configuration for a css file element.
    /// </summary>
    public sealed class CssDirectoryElement : NamedConfigurationElement
    {
        private static ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
        private static readonly ConfigurationProperty name = new ConfigurationProperty("name", typeof(string), null, ConfigurationPropertyOptions.IsRequired);
        private static readonly ConfigurationProperty directoryPath = new ConfigurationProperty("directoryPath", typeof(string), null, ConfigurationPropertyOptions.IsRequired);
        private static readonly ConfigurationProperty compressionType = new ConfigurationProperty("compressionType", typeof(CssCompressionType), CssCompressionType.YahooYui, ConfigurationPropertyOptions.None);
		private static readonly ConfigurationProperty includeSubDirectories = new ConfigurationProperty("includeSubDirectories", typeof(bool), true, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty includeFilter = new ConfigurationProperty("includeFilter", typeof(string), ".*\\.css", ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty excludeFilter = new ConfigurationProperty("excludeFilter", typeof(string), "crushed\\..*\\.css", ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty pollTime = new ConfigurationProperty("pollTime", typeof(int), 2, ConfigurationPropertyOptions.None);
		
        /// <summary>
        /// Initializes the <see cref="CssFileElement"/> class.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static CssDirectoryElement()
        {
            properties.Add(name);
            properties.Add(directoryPath);
            properties.Add(compressionType);
			properties.Add(includeSubDirectories);
			properties.Add(includeFilter);
            properties.Add(excludeFilter);
            properties.Add(pollTime);
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
        /// The directory path for the css file
        /// </summary>
        [ConfigurationProperty("directoryPath", DefaultValue = null, IsRequired = true)]
        public string DirectoryPath
        {
            get { return ((string)base[directoryPath]); }
            set { base[directoryPath] = value; }
        }

        /// <summary>
        /// The compression type to use for the css file
        /// </summary>
        [ConfigurationProperty("compressionType", DefaultValue = CssCompressionType.YahooYui, IsRequired = false)]
        public CssCompressionType CompressionType
        {
            get { return ((CssCompressionType)base[compressionType]); }
            set { base[compressionType] = value; }
        }

		/// <summary>
		/// Should sub directories be scanned for css files as well. 
		/// </summary>
		[ConfigurationProperty("includeSubDirectories", DefaultValue = true, IsRequired = false)]
		public bool IncludeSubDirectories
		{
			get { return ((bool)base[includeSubDirectories]); }
			set { base[includeSubDirectories] = value; }
		}

		/// <summary>
		/// Filter to be used for selecting files in directories.
		/// </summary>
        [ConfigurationProperty("includeFilter", DefaultValue = ".*\\.css", IsRequired = false)]
		public string IncludeFilter
		{
			get { return ((string)base[includeFilter]); }
			set { base[includeFilter] = value; }
		}

        /// <summary>
        /// Filter to be used for excluding files in directories.
        /// </summary>
        /// <remarks>Applied after the include filter.</remarks>
        [ConfigurationProperty("excludeFilter", DefaultValue = "crushed\\..*\\.css", IsRequired = false)]
        public string ExcludeFilter
        {
            get { return ((string)base[excludeFilter]); }
            set { base[excludeFilter] = value; }
        }

        /// <summary>
        /// Filter to be used for selecting files in directories.
        /// </summary>
        [ConfigurationProperty("pollTime", DefaultValue = 2, IsRequired = false)]
        public int PollTime
        {
            get { return ((int)base[pollTime]); }
            set { base[pollTime] = value; }
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