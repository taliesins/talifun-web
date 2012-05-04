using System;
using System.IO;

namespace Talifun.Web.Crusher
{
    public class JsFileProcessor
    {
        protected readonly IRetryableFileOpener RetryableFileOpener;
        protected readonly IPathProvider PathProvider;
        protected readonly FileInfo FileInfo;

        public JsFileProcessor(IRetryableFileOpener retryableFileOpener, IPathProvider pathProvider, string filePath, JsCompressionType compressionType)
        {
            RetryableFileOpener = retryableFileOpener;
            PathProvider = pathProvider;
            CompressionType = compressionType;
            var resolvedFilePath = PathProvider.MapPath(filePath);
            FileInfo = new FileInfo(resolvedFilePath);
        }

        public JsCompressionType CompressionType { get; protected set; }

        private string _contents;
        public string GetContents()
        {
            return _contents ?? (_contents = RetryableFileOpener.ReadAllText(FileInfo));
        }

        private DateTime? _lastModified;
        public DateTime GetLastModified()
        {
            if (!_lastModified.HasValue)
            {
                _lastModified = FileInfo.LastWriteTimeUtc;
            }
            return _lastModified.Value;
        }
    }
}
