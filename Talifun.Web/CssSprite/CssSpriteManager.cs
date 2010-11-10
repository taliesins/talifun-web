using System;
using System.Collections.Generic;
using System.Threading;
using Talifun.Web.CssSprite.Config;

namespace Talifun.Web.CssSprite
{
    /// <summary>
    /// We only want one instance of this running. It has file watchers that look for changes to sprite component 
    /// images and will update the sprite image.
    /// </summary>
    public sealed class CssSpriteManager : IDisposable
    {
        private CssSpriteGroupElementCollection cssSpriteGroups = CurrentCssSpriteConfiguration.Current.CssSpriteGroups;
        protected readonly ICssSpriteCreator CssSpriteCreator;
        private CssSpriteManager()
        {
            InitManager();
            CssSpriteCreator = new CssSpriteCreator();
        }

        public static CssSpriteManager Instance
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

            internal static readonly CssSpriteManager instance = new CssSpriteManager();
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

            foreach (CssSpriteGroupElement group in cssSpriteGroups)
            {
                var files = new List<ImageFile>();
                var imageOutputPath = group.ImageOutputFilePath;
                var cssOutputPath = group.CssOutputFilePath;

                foreach (ImageFileElement imageFile in group.Files)
                {
                    var file = new ImageFile()
                    {
                        FilePath = imageFile.FilePath,
                        Name = imageFile.Name
                    };
                    files.Add(file);
                }

                CssSpriteCreator.AddFiles(imageOutputPath, group.ImageUrl, cssOutputPath, files);
            }
        }

        private void DisposeManager()
        {
            foreach (CssSpriteGroupElement group in cssSpriteGroups)
            {
                var imageOutputPath = group.ImageOutputFilePath;
                var cssOutputPath = group.CssOutputFilePath;

                CssSpriteCreator.RemoveFiles(imageOutputPath, group.ImageUrl, cssOutputPath);
            }

            if (AppDomain.CurrentDomain != null)
            {
                AppDomain.CurrentDomain.DomainUnload -= OnDomainUnload;
            }
        }

        #region IDisposable Members
        private int alreadyDisposed = 0;

        ~CssSpriteManager()
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
