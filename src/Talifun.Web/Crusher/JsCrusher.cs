using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Caching;
using Talifun.Web.Helper;

namespace Talifun.Web.Crusher
{
    /// <summary>
    /// Manages the adding and removing of js files to crush. It also does the js crushing.
    /// </summary>
    public class JsCrusher : IJsCrusher
    {
        protected readonly ICacheManager CacheManager;
        protected readonly IPathProvider PathProvider;
        protected readonly IRetryableFileOpener RetryableFileOpener;
        protected readonly IRetryableFileWriter RetryableFileWriter;
    	protected readonly Lazy<Yahoo.Yui.Compressor.JavaScriptCompressor> YahooYuiJavaScriptCompressor;
    	protected readonly Lazy<Microsoft.Ajax.Utilities.Minifier> MicrosoftAjaxMinJavaScriptCompressor;
        
        protected static string JsCrusherType = typeof(JsCrusher).ToString();

        public JsCrusher(ICacheManager cacheManager, IPathProvider pathProvider, IRetryableFileOpener retryableFileOpener, IRetryableFileWriter retryableFileWriter)
        {
            CacheManager = cacheManager;
            PathProvider = pathProvider;
            RetryableFileOpener = retryableFileOpener;
            RetryableFileWriter = retryableFileWriter;
			YahooYuiJavaScriptCompressor = new Lazy<Yahoo.Yui.Compressor.JavaScriptCompressor>();
        	MicrosoftAjaxMinJavaScriptCompressor = new Lazy<Microsoft.Ajax.Utilities.Minifier>();
        }

        /// <summary>
        /// Add js files to be crushed.
        /// </summary>
        /// <param name="outputUri">The virtual path for the crushed js file.</param>
        /// <param name="files">The js files to be crushed.</param>
        /// 
        public virtual void AddFiles(Uri outputUri, IEnumerable<JsFile> files)
        {
            var outputFileInfo = new FileInfo(PathProvider.MapPath(outputUri));
            var crushedContent = ProcessFiles(outputFileInfo, files);
            
            RetryableFileWriter.SaveContentsToFile(crushedContent.Output, outputFileInfo);
            AddFilesToCache(outputUri, files);
        }

        /// <summary>
        /// Compress the js files and store them in the specified js file.
        /// </summary>
        /// <param name="outputFileInfo">The output path for the crushed js file.</param>
        /// <param name="files">The js files to be crushed.</param>
        public virtual JsCrushedOutput ProcessFiles(FileInfo outputFileInfo, IEnumerable<JsFile> files)
        {
            var uncompressedContents = new StringBuilder();
            var yahooYuiToBeCompressedContents = new StringBuilder();
			var microsoftAjaxMinToBeCompressedContents = new StringBuilder();
            var filesToProcess = files.Select(jsFile => new JsFileProcessor(RetryableFileOpener, PathProvider, jsFile));
            foreach (var fileToProcess in filesToProcess)
            {
                switch (fileToProcess.CompressionType)
                {
                    case JsCompressionType.None:
                        uncompressedContents.AppendLine(fileToProcess.GetContents());
                        break;
                    case JsCompressionType.YahooYui:
                        yahooYuiToBeCompressedContents.AppendLine(fileToProcess.GetContents());
                        break;
					case JsCompressionType.MicrosoftAjaxMin:
                		microsoftAjaxMinToBeCompressedContents.AppendLine(fileToProcess.GetContents());
                		break;
                }
            }

            if (yahooYuiToBeCompressedContents.Length > 0)
            {
                uncompressedContents.Append(YahooYuiJavaScriptCompressor.Value.Compress(yahooYuiToBeCompressedContents.ToString()));
            }

			if (microsoftAjaxMinToBeCompressedContents.Length > 0)
			{
				uncompressedContents.Append(MicrosoftAjaxMinJavaScriptCompressor.Value.MinifyJavaScript(microsoftAjaxMinToBeCompressedContents.ToString()));
			}

            var crushedOutput = new JsCrushedOutput {Output = uncompressedContents};

            return crushedOutput;
        }

        /// <summary>
        /// Remove all js files from being crushed
        /// </summary>
        /// <param name="outputUri">The path for the crushed js file</param>
        public virtual void RemoveFiles(Uri outputUri)
        {
            CacheManager.Remove<JsCacheItem>(GetKey(outputUri));
        }

        /// <summary>
        /// Add the js files to the cache so that they are monitored for any changes.
        /// </summary>
        /// <param name="outputUri">The path for the crushed js file.</param>
        /// <param name="jsFiles">The js files to be crushed.</param>
        public virtual void AddFilesToCache(Uri outputUri, IEnumerable<JsFile> jsFiles)
        {
            var fileNames = new List<string>
            {
                    PathProvider.MapPath(outputUri)
            };

            fileNames.AddRange(jsFiles.Select(file => PathProvider.MapPath(file.FilePath)));

            var jsCacheItem = new JsCacheItem()
            {
                OutputUri = outputUri,
                JsFiles = jsFiles
            };

            CacheManager.Insert(
                GetKey(outputUri),
                jsCacheItem,
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
            var jsCacheItem = (JsCacheItem)value;
            switch (reason)
            {
                case CacheItemRemovedReason.DependencyChanged:
                    AddFiles(jsCacheItem.OutputUri, jsCacheItem.JsFiles);
                    break;
                case CacheItemRemovedReason.Underused:
                case CacheItemRemovedReason.Expired:
                    AddFilesToCache(jsCacheItem.OutputUri, jsCacheItem.JsFiles);
                    break;
            }
        }

        /// <summary>
        /// Get the cache key to use for caching.
        /// </summary>
        /// <param name="outputUri">The path for the crushed js file.</param>
        /// <returns>The cache key to use for caching.</returns>
        public virtual string GetKey(Uri outputUri)
        {
            var prefix = JsCrusherType + "|";
            return prefix + outputUri;
        }
    }
}