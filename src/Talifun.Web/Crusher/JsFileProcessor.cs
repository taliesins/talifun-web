using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Talifun.Web.Crusher.JsModule;
using Talifun.Web.Helper.Pooling;

namespace Talifun.Web.Crusher
{
    public class JsFileProcessor
    {
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        private readonly IRetryableFileOpener _retryableFileOpener;
        private readonly IPathProvider _pathProvider;
        private readonly Uri _jsRootUri;
        private readonly FileInfo _fileInfo;
        private readonly List<IJsModule> _modules;

        public JsFileProcessor(Pool<Coffee.CoffeeCompiler> coffeeCompilerPool, Pool<IcedCoffee.IcedCoffeeCompiler> icedCoffeeCompilerPool, Pool<LiveScript.LiveScriptCompiler> liveScriptCompilerPool, Pool<Hogan.HoganCompiler> hoganCompilerPool, IRetryableFileOpener retryableFileOpener, IPathProvider pathProvider, string filePath, JsCompressionType compressionType, Uri jsRootUri)
        {
            _retryableFileOpener = retryableFileOpener;
            _pathProvider = pathProvider;
            _jsRootUri = jsRootUri;
            CompressionType = compressionType;
            _fileInfo = new FileInfo(new Uri(_pathProvider.MapPath(filePath)).LocalPath);
            _modules = new List<IJsModule>()
            {
                new CoffeeModule(coffeeCompilerPool),
                new IcedCoffeeModule(icedCoffeeCompilerPool),
                new LiveScriptModule(liveScriptCompilerPool),
                new HoganModule(pathProvider, hoganCompilerPool)
            };
        }

        public JsCompressionType CompressionType { get; protected set; }

        private string _contents;
        public string GetContents()
        {
             _lock.EnterUpgradeableReadLock();
            try
            {
                if (_contents == null)
                {
                    _lock.EnterWriteLock();
                    try
                    {
                        _contents = _retryableFileOpener.ReadAllText(_fileInfo);

                        foreach (var module in _modules)
                        {
                            _contents = module.Process(_jsRootUri, _fileInfo, _contents);
                        }
                    }
                    finally
                    {
                        _lock.ExitWriteLock();
                    }
                }

                return _contents;
            }
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }
        }
    }
}
