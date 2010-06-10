using Talifun.Web.Module;

namespace Talifun.Web.Crusher
{
    /// <summary>
    /// Module that is used to crush css and js based on a configuration provided.
    /// </summary>
    public class CrusherModule : HttpModuleBase
    {
        /// <summary>
        /// We want to initialize the crusher manager.
        /// </summary>
        private static CrusherManager crusherManager = CrusherManager.Instance;

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