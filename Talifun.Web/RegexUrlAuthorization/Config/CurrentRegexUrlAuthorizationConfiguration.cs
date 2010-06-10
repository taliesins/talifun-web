using Talifun.Web.Configuration;

namespace Talifun.Web.RegexUrlAuthorization.Config
{
    /// <summary>
    /// Provides easy access to the current application configuration.
    /// </summary>
    public static class CurrentRegexUrlAuthorizationConfiguration
    {
        private static RegexUrlAuthorizationSection current;
        /// <summary>
        /// Gets the static instance of <see cref="RegexUrlAuthorizationSection" /> representing the current application configuration.
        /// </summary>
        public static RegexUrlAuthorizationSection Current
        {
            get
            {
                if (current == null)
                {
                    current = CurrentConfigurationManager.GetSection<RegexUrlAuthorizationSection>();
                }
                return current;
            }
        }
    }
}
