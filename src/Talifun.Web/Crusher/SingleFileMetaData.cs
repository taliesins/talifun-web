using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Talifun.Web.Helper;

namespace Talifun.Web.Crusher
{
    public class SingleFileMetaData : IMetaData
    {
        private const int PollTime = 2000;
        private const string MetaDataFileName = "MetaData.meta";
        private readonly Dictionary<string, string> _metaDataForFiles;
        private readonly IRetryableFileWriter _retryableFileWriter;
        private readonly System.Timers.Timer _timer;

        public SingleFileMetaData(IRetryableFileOpener retryableFileOpener, IRetryableFileWriter retryableFileWriter)
        {
            _timer = new System.Timers.Timer();
            _timer.Elapsed += OnTimerElapsed;
            _timer.Interval = PollTime;
            _timer.Enabled = false;
            _timer.AutoReset = false;

            _retryableFileWriter = retryableFileWriter;

            var metaDataFile = new FileInfo(MetaDataFileName);

            if (metaDataFile.Exists)
            {

                _metaDataForFiles = retryableFileOpener.ReadAllText(metaDataFile)
                    .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                    .ToDictionary(k => k.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries)[0], v => v.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries)[1]);
            }
            else
            {
                _metaDataForFiles = new Dictionary<string, string>();
            }
        }

        /// <summary>
        /// When there are no more updates to meta data files, write file to disk
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            _timer.Stop();
            WriteMetaDataFiles();
        }

        /// <summary>
        /// Add/Update meta data
        /// </summary>
        /// <param name="outputPath"></param>
        /// <param name="filesToWatch"></param>
        public void CreateMetaData(FileInfo outputPath, IEnumerable<FileInfo> filesToWatch)
        {
            _timer.Stop();

            var metaData = GetaMetaData((new FileInfo[] { outputPath })
                .Concat(filesToWatch));

            _metaDataForFiles[outputPath.FullName] = metaData;

            _timer.Start();
        }

        /// <summary>
        /// Is meta data fresh
        /// </summary>
        /// <param name="outputPath"></param>
        /// <param name="filesToWatch"></param>
        /// <returns></returns>
        public bool IsMetaDataFresh(FileInfo outputPath, IEnumerable<FileInfo> filesToWatch)
        {
            var cachedMetaData = string.Empty;

            if (!_metaDataForFiles.TryGetValue(outputPath.FullName, out cachedMetaData))
            {
                return false;
            }
            var metaData = GetaMetaData((new FileInfo[] { outputPath }).Concat(filesToWatch));

            return cachedMetaData == metaData;
        }

        public virtual void WriteMetaDataFiles()
        {
            var metaDataFile = new FileInfo(MetaDataFileName);

            var metaDataForFiles = _metaDataForFiles
                .Select(s => string.Format("{0}|{1}", s.Key, s.Value))
                .Aggregate(new StringBuilder(), (ag, n) => ag.Append(Environment.NewLine).Append(n))
                .ToString();

            _retryableFileWriter.SaveContentsToFile(metaDataForFiles, metaDataFile);

        }

        /// <summary>
        /// Generate meta data
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        public virtual string GetaMetaData(IEnumerable<FileInfo> files)
        {
            var sb = files
                .Select(s => string.Format("{0}<{1}>", s.FullName, s.LastWriteTimeUtc.Ticks))
                .Aggregate(new StringBuilder(), (ag, n) => ag.Append("?").Append(n));

            return sb.ToString();
        }
    }
}
