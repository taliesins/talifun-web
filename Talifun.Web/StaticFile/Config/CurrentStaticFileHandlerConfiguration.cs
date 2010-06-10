using Talifun.Web.Configuration;

namespace Talifun.Web.StaticFile.Config
{
    /// <summary>
    /// Provides easy access to the current application configuration.
    /// </summary>
    public static class CurrentStaticFileHandlerConfiguration
    {
        private static StaticFileHandlerSection current = null;
        /// <summary>
        /// Gets the static instance of <see cref="StaticFileHandlerSection" /> representing the current application configuration.
        /// </summary>
        public static StaticFileHandlerSection Current
        {
            get
            {
                if (current == null)
                {
                    current = CurrentConfigurationManager.GetSection<StaticFileHandlerSection>();
                }
                return current;
            }
        }
    }
}