using Talifun.Web.Configuration;

namespace Talifun.Web.Crusher.Config
{
    /// <summary>
    /// Current crusher module configuration.
    /// </summary>
    public static class CurrentCrusherConfiguration
    {
        private static CrusherSection current;
        /// <summary>
        /// Gets the static instance of <see cref="CrusherSection" /> representing the current application configuration.
        /// </summary>
        public static CrusherSection Current
        {
            get
            {
                if (current == null)
                {
                    current = CurrentConfigurationManager.GetSection<CrusherSection>();
                }
                return current;
            }
        }
    }
}