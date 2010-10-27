using Talifun.Web.Configuration;

namespace Talifun.Web.IpAddressAuthentication.Config
{
    /// <summary>
    /// Provides easy access to the current application configuration.
    /// </summary>
    public static class CurrentIpAddressAuthenticationConfiguration
    {
        private static IpAddressAuthenticationSection current;
        /// <summary>
        /// Gets the static instance of <see cref="IpAddressAuthenticationSection" /> representing the current application configuration.
        /// </summary>
        public static IpAddressAuthenticationSection Current
        {
            get
            {
                if (current == null)
                {
                    current = CurrentConfigurationManager.GetSection<IpAddressAuthenticationSection>();
                }
                return current;
            }
        }
    }
}
