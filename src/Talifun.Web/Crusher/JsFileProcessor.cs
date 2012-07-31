using System.Collections.Generic;
using System.IO;
using System.Threading;
using Talifun.Web.Crusher.JsModule;

namespace Talifun.Web.Crusher
{
    public class JsFileProcessor
    {
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        private readonly IRetryableFileOpener _retryableFileOpener;
        private readonly IPathProvider _pathProvider;
        private readonly FileInfo _fileInfo;
        private readonly List<IJsModule> _modules;

        public JsFileProcessor(IRetryableFileOpener retryableFileOpener, IPathProvider pathProvider, string filePath, JsCompressionType compressionType)
        {
            _retryableFileOpener = retryableFileOpener;
            _pathProvider = pathProvider;
            CompressionType = compressionType;
            var resolvedFilePath = _pathProvider.MapPath(filePath);
            _fileInfo = new FileInfo(resolvedFilePath);
            _modules = new List<IJsModule>();
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
                            _contents = module.Process(_fileInfo, _contents);
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
