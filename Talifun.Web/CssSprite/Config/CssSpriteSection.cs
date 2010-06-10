using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using Talifun.Web.Crusher.Config;

namespace Talifun.Web.CssSprite.Config
{
    public sealed class CssSpriteSection : ConfigurationSection
    {
        private static ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
        private static readonly ConfigurationProperty cssSpriteGroups = new ConfigurationProperty("cssSpriteGroups", typeof(CssSpriteGroupElementCollection), null, ConfigurationPropertyOptions.IsRequired);

        /// <summary>
        /// Perform static initialisation for this configuration section. This includes explicitly adding
        /// configured properties to the Properties collection, and so cannot be performed inline.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static CssSpriteSection()
        {
            properties.Add(cssSpriteGroups);
        }

        /// <summary>
        /// Gets a <see cref="CssSpriteGroupElementCollection" /> containing the configuration elements associated with this configuration section.
        /// </summary>
        /// <value>A <see cref="CssSpriteGroupElementCollection" /> containing the configuration elements associated with this configuration section.</value>
        [ConfigurationProperty("cssSpriteGroups", DefaultValue = null, IsRequired = true)]
        public CssSpriteGroupElementCollection CssSpriteGroups
        {
            get { return ((CssSpriteGroupElementCollection)base[cssSpriteGroups]); }
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
