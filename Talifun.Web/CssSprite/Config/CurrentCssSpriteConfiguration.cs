using Talifun.Web.Configuration;

namespace Talifun.Web.CssSprite.Config
{
    /// <summary>
    /// Provides easy access to the current application configuration.
    /// </summary>
    public static class CurrentCssSpriteConfiguration
    {
        private static CssSpriteSection current;
        /// <summary>
        /// Gets the static instance of <see cref="CssSpriteSection" /> representing the current application configuration.
        /// </summary>
        public static CssSpriteSection Current
        {
            get
            {
                if (current == null)
                {
                    current = CurrentConfigurationManager.GetSection<CssSpriteSection>();
                }
                return current;
            }
        }
    }
}
