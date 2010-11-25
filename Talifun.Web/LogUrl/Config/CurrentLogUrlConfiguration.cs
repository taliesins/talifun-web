using Talifun.Web.Configuration;

namespace Talifun.Web.LogUrl.Config
{
    /// <summary>
    /// Provides easy access to the current application configuration.
    /// </summary>
    public static class CurrentLogUrlConfiguration
    {
        private static LogUrlSection _current;
        /// <summary>
        /// Gets the static instance of <see cref="LogUrlSection" /> representing the current application configuration.
        /// </summary>
        public static LogUrlSection Current
        {
            get
            {
                if (_current == null)
                {
                    _current = CurrentConfigurationManager.GetSection<LogUrlSection>();
                }
                return _current;
            }
        }
    }
}