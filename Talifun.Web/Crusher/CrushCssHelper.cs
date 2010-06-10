using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Caching;
using System.Web.Hosting;
using Yahoo.Yui.Compressor;

namespace Talifun.Web.Crusher
{
    /// <summary>
    /// Manages the adding and removing of css files to crush. It also does the css crushing.
    /// </summary>
    public static class CrushCssHelper
    {
        private const int BufferSize = 32768;
        private static IRetryableFileOpener _retryableFileOpener = new RetryableFileOpener();
        private static IHasher _hasher = new Hasher(_retryableFileOpener);

        /// <summary>
        /// Add css files to be crushed.
        /// </summary>
        /// <param name="outputPath">The path for the crushed css file.</param>
        /// <param name="files">The css files to be crushed.</param>
        public static void AddFiles(string outputPath, IEnumerable<CssFile> files)
        {
            ProcessFiles(outputPath, files);
            AddFilesToCache(outputPath, files);
        }

        /// <summary>
        /// Compress the css files and store them in the specified css file.
        /// </summary>
        /// <param name="outputPath">The path for the crushed css file.</param>
        /// <param name="files">The css files to be crushed.</param>
        private static void ProcessFiles(string outputPath, IEnumerable<CssFile> files)
        {
            var uncompressedContents = new StringBuilder();
            var toBeStockYuiCompressedContents = new StringBuilder();
            var toBeMichaelAshRegexCompressedContents = new StringBuilder();
            var toBeHybridCompressedContents = new StringBuilder();

            foreach (var file in files)
            {
                var fileInfo = new FileInfo(HostingEnvironment.MapPath(file.FilePath));
                var fileContents = _retryableFileOpener.ReadAllText(fileInfo);

                switch (file.CompressionType)
                {
                    case CssCompressionType.None:
                        uncompressedContents.AppendLine(fileContents);
                        break;
                    case CssCompressionType.StockYuiCompressor:
                        toBeStockYuiCompressedContents.AppendLine(fileContents);
                        break;
                    case CssCompressionType.MichaelAshRegexEnhancements:
                        toBeMichaelAshRegexCompressedContents.AppendLine(fileContents);
                        break;
                    case CssCompressionType.Hybrid:
                        toBeHybridCompressedContents.AppendLine(fileContents);
                        break;
                }
            }

            var uncompressedContent = uncompressedContents.ToString();
            var stockYuiCompressedCompressedContent = toBeStockYuiCompressedContents.ToString();
            var michaelAshRegexCompressedContent = toBeMichaelAshRegexCompressedContents.ToString();
            var hybridCompressedContent = toBeHybridCompressedContents.ToString();

            if (!string.IsNullOrEmpty(stockYuiCompressedCompressedContent))
            {
                stockYuiCompressedCompressedContent = CssCompressor.Compress(stockYuiCompressedCompressedContent, 0, Yahoo.Yui.Compressor.CssCompressionType.StockYuiCompressor);
            }

            if (!string.IsNullOrEmpty(michaelAshRegexCompressedContent))
            {
                michaelAshRegexCompressedContent = CssCompressor.Compress(michaelAshRegexCompressedContent, 0, Yahoo.Yui.Compressor.CssCompressionType.MichaelAshRegexEnhancements);
            }

            if (!string.IsNullOrEmpty(hybridCompressedContent))
            {
                hybridCompressedContent = CssCompressor.Compress(hybridCompressedContent, 0, Yahoo.Yui.Compressor.CssCompressionType.Hybrid);
            }

            using (var writer = new MemoryStream())
            {
                var uniEncoding = Encoding.Default;
                if (!string.IsNullOrEmpty(uncompressedContent))
                {
                    writer.Write(uniEncoding.GetBytes(uncompressedContent), 0, uniEncoding.GetByteCount(uncompressedContent));
                }

                if (!string.IsNullOrEmpty(stockYuiCompressedCompressedContent))
                {
                    writer.Write(uniEncoding.GetBytes(stockYuiCompressedCompressedContent), 0, uniEncoding.GetByteCount(stockYuiCompressedCompressedContent));
                }

                if (!string.IsNullOrEmpty(michaelAshRegexCompressedContent))
                {
                    writer.Write(uniEncoding.GetBytes(michaelAshRegexCompressedContent), 0, uniEncoding.GetByteCount(michaelAshRegexCompressedContent));
                }

                if (!string.IsNullOrEmpty(hybridCompressedContent))
                {
                    writer.Write(uniEncoding.GetBytes(hybridCompressedContent), 0, uniEncoding.GetByteCount(hybridCompressedContent));
                }

                //We might be competing with the web server for the output file, so try to overwrite it at regular intervals
                using (var outputFile = _retryableFileOpener.OpenFileStream(new FileInfo(HostingEnvironment.MapPath(outputPath)), 5, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
                {
                    var overwrite = true;
                    if (outputFile.Length > 0)
                    {
                        var newOutputFileHash = _hasher.CalculateMd5Etag(writer);
                        var outputFileHash = _hasher.CalculateMd5Etag(outputFile);

                        overwrite = (newOutputFileHash != outputFileHash);
                    }

                    if (overwrite)
                    {
                        writer.Seek(0, SeekOrigin.Begin);
                        outputFile.SetLength(writer.Length); //Truncate current file
                        outputFile.Seek(0, SeekOrigin.Begin);

                        var bufferSize = Convert.ToInt32(Math.Min(writer.Length, BufferSize));
                        var buffer = new byte[bufferSize];

                        int bytesRead;
                        while ((bytesRead = writer.Read(buffer, 0, bufferSize)) > 0)
                        {
                            outputFile.Write(buffer, 0, bytesRead);
                        }
                        outputFile.Flush();
                    }
                }
            }
        }

        /// <summary>
        /// Remove all css files from being crushed
        /// </summary>
        /// <param name="outputPath">The path for the crushed css file.</param>
        public static void RemoveFiles(string outputPath)
        {
            HttpRuntime.Cache.Remove(GetKey(outputPath));
        }

        /// <summary>
        /// Add the css files to the cache so that they are monitored for any changes.
        /// </summary>
        /// <param name="outputPath">The path for the crushed css file.</param>
        /// <param name="files">The css files to be crushed.</param>
        private static void AddFilesToCache(string outputPath, IEnumerable<CssFile> files)
        {
            var fileNames = new List<string>
                                {
                                    HostingEnvironment.MapPath(outputPath)
                                };

            foreach (var file in files)
            {
                fileNames.Add(HostingEnvironment.MapPath(file.FilePath));
            }

            HttpRuntime.Cache.Insert(
                GetKey(outputPath),
                files,
                new CacheDependency(fileNames.ToArray(), System.DateTime.Now),
                Cache.NoAbsoluteExpiration,
                Cache.NoSlidingExpiration,
                CacheItemPriority.High,
                fileRemovedCallback);
        }

        private static readonly CacheItemRemovedCallback fileRemovedCallback = new CacheItemRemovedCallback(FileRemoved);

        /// <summary>
        /// When a file is removed from cache, keep it in the cache if it is unused or expired as we want to continue to monitor
        /// any changes to file. If it has been removed because the file has changed then regenerate the crushed css file and
        /// start the monitoring again.
        /// </summary>
        /// <param name="key">The key of the cache item.</param>
        /// <param name="value">The value of the cache item.</param>
        /// <param name="reason">The reason the file was removed from cache.</param>
        private static void FileRemoved(string key, object value, CacheItemRemovedReason reason)
        {
            var outputPath = GetOutputPathFromKey(key);
            var files = (List<CssFile>)value;
            switch (reason)
            {
                case CacheItemRemovedReason.DependencyChanged:
                    AddFiles(outputPath, files);
                    break;
                case CacheItemRemovedReason.Underused:
                case CacheItemRemovedReason.Expired:
                    AddFilesToCache(outputPath, files);
                    break;
            }
        }

        /// <summary>
        /// Get the cache key to use for caching.
        /// </summary>
        /// <param name="outputPath">The path for the crushed css file.</param>
        /// <returns>The cache key to use for caching.</returns>
        private static string GetKey(string outputPath)
        {
            var prefix = typeof (CrushCssHelper).ToString() + "|";
            return prefix + outputPath;
        }

        /// <summary>
        /// Get the output path from the cache key.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <returns>The path for the crushed css file.</returns>
        private static string GetOutputPathFromKey(string key)
        {
            var prefix = typeof(CrushCssHelper).ToString() + "|";
            return key.Substring(prefix.Length);
        }
    }
}