using System;
using System.Text.RegularExpressions;
using System.Threading;
using Talifun.Web.RegexUrlAuthorization.Config;

namespace Talifun.Web.RegexUrlAuthorization
{
    public sealed class RegexUrlAuthorizationManager : IDisposable
    {
        private readonly UrlMatchElementCollection _urlMatches = CurrentRegexUrlAuthorizationConfiguration.Current.UrlMatches;
        private const RegexOptions RegxOptions = RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Singleline;

        private RegexUrlAuthorizationManager()
        {
            InitManager();
        }

        public static RegexUrlAuthorizationManager Instance
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

            internal static readonly RegexUrlAuthorizationManager instance = new RegexUrlAuthorizationManager();
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

        ~RegexUrlAuthorizationManager()
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

        public bool IsAuthorized(string rawUrl, System.Security.Principal.IPrincipal user, string requestType) 
        {
            UrlMatchElement urlMatched = null;
            foreach (UrlMatchElement urlMatch in _urlMatches)
            {
                if (!Regex.IsMatch(rawUrl, urlMatch.Expression, RegxOptions)) continue;
                urlMatched = urlMatch;
                break;
            }

            if (urlMatched == null) return true;

            return urlMatched.EveryoneAllowed || urlMatched.IsUserAllowed(user, requestType);
        }
    }
}
