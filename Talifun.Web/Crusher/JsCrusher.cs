using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Caching;
using System.Web.Hosting;
using Talifun.Web.Helper;
using Yahoo.Yui.Compressor;

namespace Talifun.Web.Crusher
{
    /// <summary>
    /// Manages the adding and removing of js files to crush. It also does the js crushing.
    /// </summary>
    public class JsCrusher : IJsCrusher
    {
        protected readonly IRetryableFileOpener RetryableFileOpener;
        protected readonly IRetryableFileWriter RetryableFileWriter;

        public JsCrusher(IRetryableFileOpener retryableFileOpener, IRetryableFileWriter retryableFileWriter)
        {
            RetryableFileOpener = retryableFileOpener;
            RetryableFileWriter = retryableFileWriter;
        }

        /// <summary>
        /// Add js files to be crushed
        /// </summary>
        /// <param name="outputPath">The path for the crushed js file</param>
        /// <param name="files">The js files to be crushed</param>
        public virtual void AddFiles(string outputPath, IEnumerable<JsFile> files)
        {
            var crushedContent = ProcessFiles(outputPath, files);
            RetryableFileWriter.SaveContentsToFile(crushedContent, outputPath);
            AddFilesToCache(outputPath, files);
        }

        /// <summary>
        /// Compress the js files and store them in the specified js file.
        /// </summary>
        /// <param name="outputPath">The path for the crushed js file.</param>
        /// <param name="files">The js files to be crushed.</param>
        public virtual StringBuilder ProcessFiles(string outputPath, IEnumerable<JsFile> files)
        {
            var uncompressedContents = new StringBuilder();
            var toBeCompressedContents = new StringBuilder();

            foreach (var file in files)
            {
                var filePath = HostingEnvironment.MapPath(file.FilePath);
                var fileInfo = new FileInfo(filePath);
                var fileContents = RetryableFileOpener.ReadAllText(fileInfo);

                switch (file.CompressionType)
                {
                    case JsCompressionType.None:
                        uncompressedContents.AppendLine(fileContents);
                        break;
                    case JsCompressionType.Min:
                        toBeCompressedContents.AppendLine(fileContents);
                        break;
                }
            }

            if (toBeCompressedContents.Length > 0)
            {
                uncompressedContents.Append(JavaScriptCompressor.Compress(toBeCompressedContents.ToString()));
            }

            return uncompressedContents;
        }

        /// <summary>
        /// Remove all js files from being crushed
        /// </summary>
        /// <param name="outputPath">The path for the crushed js file</param>
        public virtual void RemoveFiles(string outputPath)
        {
            HttpRuntime.Cache.Remove(GetKey(outputPath));
        }

        /// <summary>
        /// Add the js files to the cache so that they are monitored for any changes.
        /// </summary>
        /// <param name="outputPath">The path for the crushed js file.</param>
        /// <param name="files">The js files to be crushed.</param>
        public virtual void AddFilesToCache(string outputPath, IEnumerable<JsFile> files)
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
                FileRemoved);
        }

        /// <summary>
        /// When a file is removed from cache, keep it in the cache if it is unused or expired as we want to continue to monitor
        /// any changes to file. If it has been removed because the file has changed then regenerate the crushed js file and
        /// start the monitoring again.
        /// </summary>
        /// <param name="key">The key of the cache item</param>
        /// <param name="value">The value of the cache item</param>
        /// <param name="reason">The reason the file was removed from cache.</param>
        public virtual void FileRemoved(string key, object value, CacheItemRemovedReason reason)
        {
            var outputPath = GetOutputPathFromKey(key);
            var files = (List<JsFile>)value;
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
        /// <param name="outputPath">The path for the crushed js file.</param>
        /// <returns>The cache key to use for caching.</returns>
        public virtual string GetKey(string outputPath)
        {
            var prefix = typeof(JsCrusher).ToString() + "|";
            return prefix + outputPath;
        }

        /// <summary>
        /// Get the output path from the cache key.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <returns>The path for the crushed js file.</returns>
        public virtual string GetOutputPathFromKey(string key)
        {
            var prefix = typeof(JsCrusher).ToString() + "|";
            return key.Substring(prefix.Length);
        }
    }
}