using Talifun.Web.Configuration;

namespace Talifun.Web.IpAddressAuthentication.Config
{
    /// <summary>
    /// Provides easy access to the current application configuration.
    /// </summary>
    public static class CurrentIpAddressAuthenticationConfiguration
    {
        private static IpAddressAuthenticationSection _current;
        /// <summary>
        /// Gets the static instance of <see cref="IpAddressAuthenticationSection" /> representing the current application configuration.
        /// </summary>
        public static IpAddressAuthenticationSection Current
        {
            get
            {
                if (_current == null)
                {
                    _current = CurrentConfigurationManager.GetSection<IpAddressAuthenticationSection>();
                }
                return _current;
            }
        }
    }
}
