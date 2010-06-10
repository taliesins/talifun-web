using System.Configuration;
using Talifun.Web.Configuration;

namespace Talifun.Web.CssSprite.Config
{
    /// <summary>
    /// Represents a configuration element containing a collection of <see cref="CssSpriteGroupElement" /> configuration elements.
    /// </summary>
    [ConfigurationCollection(typeof(CssSpriteGroupElement))]
    public sealed class CssSpriteGroupElementCollection : CurrentConfigurationElementCollection<CssSpriteGroupElement>
    {
        public CssSpriteGroupElementCollection()
        {
            AddElementName = "cssSpriteGroup";
        }
    }
}
