using Talifun.Web.Configuration;

namespace Talifun.Web.LogUrl.Config
{
    /// <summary>
    /// Provides easy access to the current application configuration.
    /// </summary>
    public static class CurrentLogUrlConfiguration
    {
        private static LogUrlSection current;
        /// <summary>
        /// Gets the static instance of <see cref="LogUrlSection" /> representing the current application configuration.
        /// </summary>
        public static LogUrlSection Current
        {
            get
            {
                if (current == null)
                {
                    current = CurrentConfigurationManager.GetSection<LogUrlSection>();
                }
                return current;
            }
        }
    }
}