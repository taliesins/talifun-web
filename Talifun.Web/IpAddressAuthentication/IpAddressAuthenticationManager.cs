using System;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using Talifun.Web.IpAddressAuthentication.Config;

namespace Talifun.Web.IpAddressAuthentication
{
    public sealed class IpAddressAuthenticationManager : IDisposable
    {
        private const RegexOptions RegxOptions = RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Singleline;
        private readonly UrlMatchElementCollection _urlMatches = CurrentIpAddressAuthenticationConfiguration.Current.UrlMatches;

        private IpAddressAuthenticationManager()
        {
            InitManager();
        }

        public static IpAddressAuthenticationManager Instance
        {
            get
            {
                return Nested.instance;
            }
        }

        private class Nested
        {
            // Explicit static constructor to tell C# compiler
            // not to mark type as beforefieldinit
            static Nested()
            {
            }

            internal static readonly IpAddressAuthenticationManager instance = new IpAddressAuthenticationManager();
        }

        /// <summary>
        /// We want to release the manager when app domain is unloaded. So we removed the reference, as nothing will then be referencing
        /// the manager, garbage collector will dispose it.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>
        /// We are using a sneaky little trick to keep manager alive for the duration of the appdomain.
        /// We are storing a delegate with a reference to the manager in a global area (AppDomain.CurrentDomain.UnhandledException),
        /// which means the garbage collector won't be able to dispose the manager.
        /// HttpModule life is shorter then AppDomain and can be unloaded at any time.
        /// </remarks>
        private void OnDomainUnload(object sender, EventArgs e)
        {
            if (AppDomain.CurrentDomain != null)
            {
                AppDomain.CurrentDomain.DomainUnload -= OnDomainUnload;
            }
        }

        private void InitManager()
        {
            AppDomain.CurrentDomain.DomainUnload += OnDomainUnload;
        }

        private void DisposeManager()
        {
            if (AppDomain.CurrentDomain != null)
            {
                AppDomain.CurrentDomain.DomainUnload -= OnDomainUnload;
            }
        }

        #region IDisposable Members
        private int alreadyDisposed = 0;

        ~IpAddressAuthenticationManager()
        {
            // call Dispose with false.  Since we're in the
            // destructor call, the managed resources will be
            // disposed of anyways.
            Dispose(false);
        }

        void IDisposable.Dispose()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (alreadyDisposed != 0) return;

            // dispose of the managed and unmanaged resources
            Dispose(true);

            // tell the GC that the Finalize process no longer needs
            // to be run for this object. 

            // it is called after Dispose(true) to ensure that GC.SuppressFinalize() 
            // only gets called if the Dispose operation completes successfully. 
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposeManagedResources)
        {
            var disposedAlready = Interlocked.Exchange(ref alreadyDisposed, 1);
            if (disposedAlready != 0) return;

            if (!disposeManagedResources) return;

            // Dispose managed resources.
            DisposeManager();
        }

        #endregion

        public bool IsAuthorized(string rawUrl, string userHostAddress)
        {
            UrlMatchElement urlMatched = null;
            foreach (UrlMatchElement urlMatch in _urlMatches)
            {
                if (!Regex.IsMatch(rawUrl, urlMatch.Expression, RegxOptions)) continue;
                urlMatched = urlMatch;
                break;
            }

            if (urlMatched == null) return true;

            var ipAddress = IPAddress.Parse(userHostAddress);
            return IsValidIpAddress(urlMatched, ipAddress);
        }

        private bool IsValidIpAddress(UrlMatchElement urlMatched, IPAddress ipAddress)
        {
            foreach (IpAddressMatchElement ipAddressMatch in urlMatched.IpAddressMatches)
            {
                if (ipAddressMatch.NetMask == null)
                {
                    if (ipAddressMatch.IpAddress == ipAddress)
                    {
                        return ipAddressMatch.Access;
                    }
                }
                else
                {
                    if (IsAddressOnSubnet(ipAddress, ipAddressMatch.IpAddress, ipAddressMatch.NetMask))
                    {
                        return ipAddressMatch.Access;
                    }
                }
            }

            return urlMatched.DefaultAccess;
        }

        private bool IsAddressOnSubnet(IPAddress address, IPAddress subnet, IPAddress mask)
        {
            var addrBytes = address.GetAddressBytes();
            var maskBytes = mask.GetAddressBytes();
            var maskedAddressBytes = new byte[addrBytes.Length];

            for (var i = 0; i < maskedAddressBytes.Length; ++i)
            {
                maskedAddressBytes[i] = (byte)(addrBytes[i] & maskBytes[i]);
            }

            var maskedAddress = new IPAddress(maskedAddressBytes);
            return subnet.Equals(maskedAddress);
        }
    }
}
