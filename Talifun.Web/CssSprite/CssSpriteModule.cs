using Talifun.Web.Module;

namespace Talifun.Web.CssSprite
{
    /// <summary>
    /// Module that is used to generate sprite images based on a configuration provided.
    /// </summary>
    public class CssSpriteModule : HttpModuleBase
    {
        /// <summary>
        /// We want to initialize the css sprite manager.
        /// </summary>
        protected static CssSpriteManager CssSpriteManager = CssSpriteManager.Instance;

        /// <summary>
        /// Determines whether the module will be registered for discovery
        /// in partial trust environments or not.
        /// </summary>
        protected override bool SupportDiscoverability
        {
            get { return true; }
        }
    }
}