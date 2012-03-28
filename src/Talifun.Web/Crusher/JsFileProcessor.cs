using System;
using System.IO;

namespace Talifun.Web.Crusher
{
    public class JsFileProcessor
    {
        protected readonly IRetryableFileOpener RetryableFileOpener;
        protected readonly IPathProvider PathProvider;
        protected readonly FileInfo FileInfo;

        public JsFileProcessor(IRetryableFileOpener retryableFileOpener, IPathProvider pathProvider, JsFile jsFile)
        {
            RetryableFileOpener = retryableFileOpener;
            PathProvider = pathProvider;
            CompressionType = jsFile.CompressionType;
            var filePath = PathProvider.MapPath(jsFile.FilePath);
            FileInfo = new FileInfo(filePath);
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
