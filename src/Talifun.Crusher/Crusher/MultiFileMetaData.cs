using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Talifun.Web;
using Talifun.Web.Helper;

namespace Talifun.Crusher.Crusher
{
    public class MultiFileMetaData : IMetaData
    {
        private readonly IRetryableFileOpener _retryableFileOpener;
        private readonly IRetryableFileWriter _retryableFileWriter;

        public MultiFileMetaData(IRetryableFileOpener retryableFileOpener, IRetryableFileWriter retryableFileWriter)
        {
            _retryableFileOpener = retryableFileOpener;
            _retryableFileWriter = retryableFileWriter;
        }

        /// <summary>
        /// Creates meta data about files to be crushed.
        /// </summary>
        /// <param name="outputPath"></param>
        /// <param name="filesToWatch"></param>
        public virtual void CreateMetaData(FileInfo outputPath, IEnumerable<FileInfo> filesToWatch)
        {
            var metaDataFile = new FileInfo(outputPath.FullName + ".metadata");
            var metaData = GetaMetaData((new FileInfo[] { outputPath }).Concat(filesToWatch));

            _retryableFileWriter.SaveContentsToFile(metaData, metaDataFile);
        }

        /// <summary>
        /// Checkes if meta data is still valid against files to be crushed.
        /// </summary>
        /// <param name="outputPath"></param>
        /// <param name="filesToWatch"></param>
        /// <returns></returns>
        public virtual bool IsMetaDataFresh(FileInfo outputPath, IEnumerable<FileInfo> filesToWatch)
        {
            if (!outputPath.Exists)
            {
                return false;
            }

            var metaDataFile = new FileInfo(outputPath.FullName + ".metadata");

            if (!metaDataFile.Exists)
            {
                return false;
            }

            var cachedMetaData = _retryableFileOpener.ReadAllText(metaDataFile);
            var metaData = GetaMetaData((new FileInfo[]{outputPath}).Concat(filesToWatch));

            return cachedMetaData == metaData;
        }

        /// <summary>
        /// Generate meta data
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        public virtual string GetaMetaData(IEnumerable<FileInfo> files)
        {
            var sb = files
                .Select(s => string.Format("{0} ({1})", s.FullName, s.LastWriteTimeUtc.Ticks))
                .Aggregate(new StringBuilder(), (ag, n) => ag.Append(", ").Append(n));

            return sb.ToString();
        }
    }
}
