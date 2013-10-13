using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Talifun.Crusher.Configuration;
using Talifun.Crusher.Configuration.Css;
using Talifun.Crusher.Configuration.Js;
using Talifun.Web;
using Talifun.Web.Helper;

namespace Talifun.Crusher.Crusher
{
    /// <summary>
    /// We only want one instance of this running. It has file watchers that look for changes to js and css rules
    /// specified and will update them as neccessary.
    /// </summary>
    public sealed class CrusherManager : IDisposable
    {
        private const int BufferSize = 32768;
		private readonly Encoding _encoding = Encoding.UTF8;
        private readonly IPathProvider _pathProvider;

        private readonly IRetryableFileOpener _retryableFileOpener;
        private readonly IHasher _hasher;
        private readonly IRetryableFileWriter _retryableFileWriter;
        private readonly ICacheManager _cacheManager;
        private readonly IMetaData _fileMetaData;
        private readonly CrusherSection _crusherConfiguration;

        private CrusherManager()
        {
            _crusherConfiguration = CurrentCrusherConfiguration.Current;

            _retryableFileOpener = new RetryableFileOpener();
            _hasher = new Md5Hasher(_retryableFileOpener);
			_retryableFileWriter = new RetryableFileWriter(BufferSize, _encoding, _retryableFileOpener, _hasher);
            _pathProvider = new PathProvider();

            _cacheManager = new HttpCacheManager();
            _fileMetaData = new MultiFileMetaData(_retryableFileOpener, _retryableFileWriter);
 
            InitManager();
        }

        public static CrusherManager Instance
        {
            get
            {
                return Nested.Instance;
            }
        }

        private class Nested
        {
            // Explicit static constructor to tell C# compiler
            // not to mark type as beforefieldinit
            static Nested()
            {
            }

            internal static readonly CrusherManager Instance = new CrusherManager();
        }

        /// <summary>
        /// We want to release the manager when app domain is unloaded. So we removed the reference, as nothing will be referencing
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

            var jsExceptions = new List<JsException>(); 
            var cssExceptions = new List<CssException>();
            var countdownEvents = new CountdownEvent(2);

            ThreadPool.QueueUserWorkItem(data =>
                {
                    var manualResetEvent = (CountdownEvent)data;
                    try
                    {
                        var jsGroups = _crusherConfiguration.JsGroups;
                        var jsCrusher = new JsCrusher(_cacheManager, _pathProvider, _retryableFileOpener, _retryableFileWriter, _fileMetaData);
                        var groupsProcessor = new JsGroupsProcessor();
                        groupsProcessor.ProcessGroups(_pathProvider, jsCrusher, jsGroups);
                    }
                    catch (Exception exception)
                    {
                        jsExceptions.Add(new JsException(exception));
                    }
                    manualResetEvent.Signal();

                }, countdownEvents);

            ThreadPool.QueueUserWorkItem(data =>
                {
                    var manualResetEvent = (CountdownEvent)data;
                    try
                    {
                        var hashQueryStringKeyName = _crusherConfiguration.QuerystringKeyName;
                        var cssGroups = _crusherConfiguration.CssGroups;
                        var cssAssetsFileHasher = new CssAssetsFileHasher(hashQueryStringKeyName, _hasher, _pathProvider);
                        var cssPathRewriter = new CssPathRewriter(cssAssetsFileHasher, _pathProvider);
                        var cssCrusher = new CssCrusher(_cacheManager, _pathProvider, _retryableFileOpener, _retryableFileWriter, cssPathRewriter, _fileMetaData, _crusherConfiguration.WatchAssets);
                        var groupsProcessor = new CssGroupsProcessor();
                        groupsProcessor.ProcessGroups(_pathProvider, cssCrusher, cssGroups);
                    }
                    catch (Exception exception)
                    {
                        cssExceptions.Add(new CssException(exception));
                    }
                    manualResetEvent.Signal();
                }, countdownEvents);

            countdownEvents.Wait();

            var exceptions = cssExceptions.Cast<Exception>().Concat(jsExceptions.Cast<Exception>()).ToList();

            if (exceptions.Any())
            {
                throw new AggregateException(exceptions);
            }
        }

        private void DisposeManager()
        {
            var hashQueryStringKeyName = _crusherConfiguration.QuerystringKeyName;
            var cssGroups = _crusherConfiguration.CssGroups;
            var cssAssetsFileHasher = new CssAssetsFileHasher(hashQueryStringKeyName, _hasher, _pathProvider);
            var cssPathRewriter = new CssPathRewriter(cssAssetsFileHasher, _pathProvider);
            var cssCrusher = new CssCrusher(_cacheManager, _pathProvider, _retryableFileOpener, _retryableFileWriter, cssPathRewriter, _fileMetaData, _crusherConfiguration.WatchAssets);
            foreach (CssGroupElement group in cssGroups)
            {
                var outputUri = new Uri(_pathProvider.ToAbsolute(group.OutputFilePath), UriKind.Relative);
                cssCrusher.RemoveGroup(outputUri);
            }

            var jsGroups = _crusherConfiguration.JsGroups;
            var jsCrusher = new JsCrusher(_cacheManager, _pathProvider, _retryableFileOpener, _retryableFileWriter, _fileMetaData);
            foreach (JsGroupElement group in jsGroups)
            {
                var outputUri = new Uri(_pathProvider.ToAbsolute(group.OutputFilePath), UriKind.Relative);
                jsCrusher.RemoveGroup(outputUri);
            }

            if (AppDomain.CurrentDomain != null)
            {
                AppDomain.CurrentDomain.DomainUnload -= OnDomainUnload;
            }
        }
       
        #region IDisposable Members
        private int alreadyDisposed = 0;

        ~CrusherManager()
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
    }
}