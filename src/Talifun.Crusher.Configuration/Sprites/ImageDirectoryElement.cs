using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using Talifun.Web.Configuration;

namespace Talifun.Crusher.Configuration.Sprites
{
    public sealed class ImageDirectoryElement : NamedConfigurationElement
    {
        private static ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
        private static readonly ConfigurationProperty name = new ConfigurationProperty("name", typeof(string), null, ConfigurationPropertyOptions.IsRequired);
        private static readonly ConfigurationProperty directoryPath = new ConfigurationProperty("directoryPath", typeof(string), null, ConfigurationPropertyOptions.IsRequired);
        private static readonly ConfigurationProperty includeSubDirectories = new ConfigurationProperty("includeSubDirectories", typeof(bool), true, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty includeFilter = new ConfigurationProperty("includeFilter", typeof(string), ".*\\.js", ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty excludeFilter = new ConfigurationProperty("excludeFilter", typeof(string), "crushed\\..*\\.js", ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty pollTime = new ConfigurationProperty("pollTime", typeof(int), 2, ConfigurationPropertyOptions.None);

        /// <summary>
        /// Initializes the <see cref="ImageFileElement"/> class.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static ImageDirectoryElement()
        {
            properties.Add(name);
            properties.Add(directoryPath);
            properties.Add(includeSubDirectories);
            properties.Add(includeFilter);
            properties.Add(excludeFilter);
            properties.Add(pollTime);
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
        /// Gets or sets the directory path for the image file
        /// </summary>
        [ConfigurationProperty("directoryPath", DefaultValue = null, IsRequired = true)]
        public string DirectoryPath
        {
            get { return ((string)base[directoryPath]); }
            set { base[directoryPath] = value; }
        }

        /// <summary>
        /// Should sub directories be scanned for js files as well. 
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
        [ConfigurationProperty("includeFilter", DefaultValue = ".*\\.(png|gif|jpg)", IsRequired = false)]
        public string IncludeFilter
        {
            get { return ((string)base[includeFilter]); }
            set { base[includeFilter] = value; }
        }

        /// <summary>
        /// Filter to be used for excluding files in directories.
        /// </summary>
        /// <remarks>Applied after the include filter.</remarks>
        [ConfigurationProperty("excludeFilter", DefaultValue = "crushed\\..*\\.(png|gif|jpg)", IsRequired = false)]
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
