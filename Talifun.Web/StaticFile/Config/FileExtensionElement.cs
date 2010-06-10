using System;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using Talifun.Web.Configuration;

namespace Talifun.Web.StaticFile.Config
{
    public sealed class FileExtensionElement : NamedConfigurationElement
    {
        private static ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
        private static readonly ConfigurationProperty name = new ConfigurationProperty("name", typeof(string), null, ConfigurationPropertyOptions.IsKey | ConfigurationPropertyOptions.IsRequired);
        private static readonly ConfigurationProperty extension = new ConfigurationProperty("extension", typeof(string), null, ConfigurationPropertyOptions.IsRequired);
        private static readonly ConfigurationProperty serveFromMemory = new ConfigurationProperty("serveFromMemory", typeof(bool), true, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty maxMemorySize = new ConfigurationProperty("maxMemorySize", typeof(long), 50000L, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty compress = new ConfigurationProperty("compress", typeof(bool), true, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty memorySlidingExpiration = new ConfigurationProperty("memorySlidingExpiration", typeof(TimeSpan), new TimeSpan(0,30,0), ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty expires = new ConfigurationProperty("expires", typeof(TimeSpan), new TimeSpan(7,0,0,0), ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty etagMethod = new ConfigurationProperty("etagMethod", typeof(EtagMethodType), EtagMethodType.MD5, ConfigurationPropertyOptions.None);

        /// <summary>
        /// Initializes the <see cref="FileExtensionElement"/> class.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static FileExtensionElement()
        {
            properties.Add(name);
            properties.Add(extension);
            properties.Add(serveFromMemory);
            properties.Add(maxMemorySize);
            properties.Add(compress);
            properties.Add(memorySlidingExpiration);
            properties.Add(expires);
            properties.Add(etagMethod);
        }

        /// <summary>
        /// Gets or sets the name of the configuration element represented by this instance.
        /// </summary>
        [ConfigurationProperty("name", DefaultValue = null, IsRequired = false, IsKey = true)]
        public override string Name
        {
            get { return ((string)base[name]); }
            set { base[name] = value; }
        }

        /// <summary>
        /// Gets or sets all the extensions of files that rule must be applied to.
        /// </summary>
        [ConfigurationProperty("extension", IsRequired = false)]
        public string Extension
        {
            get { return ((string)base[extension]); }
            set { base[extension] = value; }
        }

        /// <summary>
        /// Should content be served from memory. Compressed version of the file will also be stored in memory for these file type if specified.
        /// </summary>
        /// <remarks>
        /// Serving content from memory and skipping the hard drive can significantly improve perfomance.
        /// 
        /// Even if a file is not stored in memory, its meta information is (file size, etag). As generating 
        /// the etag can take some time on larger files.
        /// </remarks>
        [ConfigurationProperty("serveFromMemory", IsRequired = false, DefaultValue = true)]
        public bool ServeFromMemory
        {
            get { return ((bool)base[serveFromMemory]); }
            set { base[serveFromMemory] = value; }
        }

        /// <summary>
        /// Gets or sets the maximum size a file can be before it is no longer served from memory.
        /// </summary>
        [ConfigurationProperty("maxMemorySize", IsRequired = false, DefaultValue = true)]
        public long MaxMemorySize
        {
            get { return ((long)base[maxMemorySize]); }
            set { base[maxMemorySize] = value; }
        }

        /// <summary>
        /// Gets or sets if the content served is compressible
        /// </summary>
        /// <remarks>
        /// The default types that should be compressed are:
        /// css, js, htm, html, swf, xml, xslt, txt
        /// The default types that should not be compressed are:
        /// pdf, 
        /// png, jpg, jpeg, gif, ico,
        /// wav, mp3, m4a, aac,
        /// 3gp, 3g2, asf, avi, dv, flv, mov, mp4, mpg, mpeg, wmv,
        /// zip, rar, 7z, arj
        /// </remarks>
        [ConfigurationProperty("compress", IsRequired = false, DefaultValue = 50000L)]
        public bool Compress
        {
            get { return ((bool)base[compress]); }
            set { base[compress] = value; }
        }

        /// <summary>
        /// The amount of time a file should be cached in memory.
        /// </summary>
        /// <remarks>
        /// Even if a file is not stored in memory, its meta information is (file size, etag). As generating 
        /// the etag can take some time on larger files.
        /// </remarks>
        [ConfigurationProperty("memorySlidingExpiration", IsRequired = false)]
        public TimeSpan MemorySlidingExpiration
        {
            get { return ((TimeSpan)base[memorySlidingExpiration]); }
            set { base[memorySlidingExpiration] = value; }
        }

        /// <summary>
        /// Gets or sets the amount of time a browser request is valid for. Once the page has expired
        /// it will re-check to see if it has the latest content. On the recheck, if the browser has the latest version
        /// already a 304 will be returned and not the entire file.
        /// </summary>
        /// <remarks>
        /// Set this to a large value like 1 week for content that does not change often.
        /// </remarks>
        [ConfigurationProperty("expires", IsRequired = false)]
        public TimeSpan Expires
        {
            get { return ((TimeSpan)base[expires]); }
            set { base[expires] = value; }
        }

        /// <summary>
        /// Gets or sets the type of method used to calculate the etag.
        /// </summary>
        /// <remarks>
        /// Calculating a consistant etag is very import if using load balancers. So set it to MD 5 if this is the case,
        /// as files may be created at different times if it is on load balancers.
        /// </remarks>
        [ConfigurationProperty("etagMethod", IsRequired = false)]
        public EtagMethodType EtagMethod
        {
            get { return ((EtagMethodType)base[etagMethod]); }
            set { base[etagMethod] = value; }
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