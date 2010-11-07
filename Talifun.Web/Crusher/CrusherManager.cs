using System;
using System.Collections.Generic;
using System.Threading;
using Talifun.Web.Crusher.Config;

namespace Talifun.Web.Crusher
{
    /// <summary>
    /// We only want one instance of this running. It has file watchers that look for changes to js and css rules
    /// specified and will update them as neccessary.
    /// </summary>
    public sealed class CrusherManager : IDisposable
    {
        private const int BufferSize = 32768;
        private const string HashQueryStringKeyName = "etag=";
        private CssGroupElementCollection cssGroups = CurrentCrusherConfiguration.Current.CssGroups;
        private JsGroupElementCollection jsGroups = CurrentCrusherConfiguration.Current.JsGroups;
        private ICssCrusher cssCrusher;
        private IJsCrusher jsCrusher;

        private CrusherManager()
        {
            var retryableFileOpener = new RetryableFileOpener();
            var hasher = new Hasher(retryableFileOpener);
            var cssAssetsFileHasher = new CssAssetsFileHasher(HashQueryStringKeyName, hasher);
            var cssPathRewriter = new CssPathRewriter(cssAssetsFileHasher);
            cssCrusher = new CssCrusher(BufferSize, retryableFileOpener, hasher, cssPathRewriter);
            jsCrusher = new JsCrusher(BufferSize, retryableFileOpener, hasher);
            InitManager();
        }

        public static CrusherManager Instance
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

            internal static readonly CrusherManager instance = new CrusherManager();
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

            foreach (CssGroupElement group in cssGroups)
            {
                var files = new List<CssFile>();
                var outputPath = group.OutputFilePath;

                foreach (CssFileElement cssFile in group.Files)
                {
                    var file = new CssFile()
                                   {
                                       CompressionType = cssFile.CompressionType,
                                       FilePath = cssFile.FilePath
                                   };
                    files.Add(file);
                } 

                cssCrusher.AddFiles(outputPath, files);
            }

            foreach (JsGroupElement group in jsGroups)
            {
                var files = new List<JsFile>();
                var outputPath = group.OutputFilePath;

                foreach (JsFileElement cssFile in group.Files)
                {
                    var file = new JsFile()
                                   {
                                       CompressionType = cssFile.CompressionType,
                                       FilePath = cssFile.FilePath
                                   };
                    files.Add(file);
                }

                jsCrusher.AddFiles(outputPath, files);
            }
        }

        private void DisposeManager()
        {
            foreach (CssGroupElement group in cssGroups)
            {
                cssCrusher.RemoveFiles(group.OutputFilePath);
            }

            foreach (JsGroupElement group in jsGroups)
            {
                jsCrusher.RemoveFiles(group.OutputFilePath);
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