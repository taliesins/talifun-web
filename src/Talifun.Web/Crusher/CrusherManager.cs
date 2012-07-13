using System;
using System.Collections.Generic;
using System.Threading;
using Talifun.Web.Crusher.Config;
using Talifun.Web.Helper;

namespace Talifun.Web.Crusher
{
    /// <summary>
    /// We only want one instance of this running. It has file watchers that look for changes to js and css rules
    /// specified and will update them as neccessary.
    /// </summary>
    public sealed class CrusherManager : IDisposable
    {
        private const int BufferSize = 32768;
        private readonly string _hashQueryStringKeyName;
        private readonly CssGroupElementCollection _cssGroups;
        private readonly JsGroupElementCollection _jsGroups;
        private readonly ICacheManager _cacheManager;
        private readonly IPathProvider _pathProvider;
        private readonly ICssCrusher _cssCrusher;
        private readonly IJsCrusher _jsCrusher;

        private CrusherManager()
        {
            var crusherConfiguration = CurrentCrusherConfiguration.Current;
            _hashQueryStringKeyName = crusherConfiguration.QuerystringKeyName;
            _cssGroups = crusherConfiguration.CssGroups;
            _jsGroups = crusherConfiguration.JsGroups;

            var retryableFileOpener = new RetryableFileOpener();
            var hasher = new Hasher(retryableFileOpener);
            var retryableFileWriter = new RetryableFileWriter(BufferSize, retryableFileOpener, hasher);
            _pathProvider = new PathProvider();
            var cssAssetsFileHasher = new CssAssetsFileHasher(_hashQueryStringKeyName, hasher, _pathProvider);
            var cssPathRewriter = new CssPathRewriter(cssAssetsFileHasher, _pathProvider);

            _cacheManager = new HttpCacheManager();
            _cssCrusher = new CssCrusher(_cacheManager, _pathProvider, retryableFileOpener, retryableFileWriter, cssPathRewriter);
            _jsCrusher = new JsCrusher(_cacheManager, _pathProvider, retryableFileOpener, retryableFileWriter);

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

        	ConfigureJs();
        	ConfigureCss();
        }

		private void ConfigureJs()
		{
			foreach (CssGroupElement group in _cssGroups)
			{
				var files = new List<CssFile>();

				foreach (CssFileElement cssFile in group.Files)
				{
					var file = new CssFile()
					{
						CompressionType = cssFile.CompressionType,
						FilePath = cssFile.FilePath
					};
					files.Add(file);
				}

				var directories = new List<CssDirectory>();

				foreach (CssDirectoryElement cssDirectory in group.Directories)
				{
					var directory = new CssDirectory()
					{
						CompressionType = cssDirectory.CompressionType,
						FilePath = cssDirectory.FilePath,
                        IncludeSubDirectories = cssDirectory.IncludeSubDirectories,
                        PollTime = cssDirectory.PollTime,
                        Filter = cssDirectory.Filter
					};
					directories.Add(directory);
				}

				var outputUri = new Uri(_pathProvider.ToAbsolute(group.OutputFilePath), UriKind.Relative);

				_cssCrusher.CreateGroup(outputUri, files, directories, group.AppendHashToCssAsset);
			}
		}

		private void ConfigureCss()
		{
			foreach (JsGroupElement group in _jsGroups)
			{
				var files = new List<JsFile>();

				foreach (JsFileElement jsFile in group.Files)
				{
					var file = new JsFile()
					{
						CompressionType = jsFile.CompressionType,
						FilePath = jsFile.FilePath
					};
					files.Add(file);
				}

				var directories = new List<JsDirectory>();

				foreach (JsDirectoryElement jsDirectory in group.Directories)
				{
					var directory = new JsDirectory()
					{
						CompressionType = jsDirectory.CompressionType,
						FilePath = jsDirectory.FilePath,
                        IncludeSubDirectories = jsDirectory.IncludeSubDirectories,
                        PollTime = jsDirectory.PollTime,
                        Filter = jsDirectory.Filter
					};
					directories.Add(directory);
				}

				var outputUri = new Uri(_pathProvider.ToAbsolute(group.OutputFilePath), UriKind.Relative);
				_jsCrusher.AddGroup(outputUri, files, directories);
			}
		}

        private void DisposeManager()
        {
            foreach (CssGroupElement group in _cssGroups)
            {
                var outputUri = new Uri(_pathProvider.ToAbsolute(group.OutputFilePath), UriKind.Relative);
                _cssCrusher.RemoveGroup(outputUri);
            }

            foreach (JsGroupElement group in _jsGroups)
            {
                var outputUri = new Uri(_pathProvider.ToAbsolute(group.OutputFilePath), UriKind.Relative);
                _jsCrusher.RemoveGroup(outputUri);
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