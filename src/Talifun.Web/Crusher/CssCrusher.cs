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
    /// Manages the adding and removing of css files to crush. It also does the css crushing.
    /// </summary>
    public class CssCrusher : ICssCrusher
    {
        protected readonly ICacheManager CacheManager;
        protected readonly IPathProvider PathProvider;
        protected readonly IRetryableFileOpener RetryableFileOpener;
        protected readonly IRetryableFileWriter RetryableFileWriter;
        protected readonly ICssPathRewriter CssPathRewriter;
        protected static string CssCrusherType = typeof(CssCrusher).ToString();
		protected readonly Lazy<Yahoo.Yui.Compressor.CssCompressor> YahooYuiCssCompressor;
		protected readonly Lazy<Microsoft.Ajax.Utilities.Minifier> MicrosoftAjaxMinCssCompressor;

        public CssCrusher(ICacheManager cacheManager, IPathProvider pathProvider, IRetryableFileOpener retryableFileOpener, IRetryableFileWriter retryableFileWriter, ICssPathRewriter cssPathRewriter)
        {
            CacheManager = cacheManager;
            PathProvider = pathProvider;
            RetryableFileOpener = retryableFileOpener;
            RetryableFileWriter = retryableFileWriter;
            CssPathRewriter = cssPathRewriter;
			YahooYuiCssCompressor = new Lazy<Yahoo.Yui.Compressor.CssCompressor>();
			MicrosoftAjaxMinCssCompressor = new Lazy<Microsoft.Ajax.Utilities.Minifier>();
        }

        /// <summary>
        /// Add css files to be crushed.
        /// </summary>
        /// <param name="outputUri">The virtual path for the crushed css file.</param>
        /// <param name="cssFiles">The css files to be crushed.</param>
        /// <param name="appendHashToAssets">Should css assets have a hash appended to them.</param>
        public virtual void AddFiles(Uri outputUri, IEnumerable<CssFile> cssFiles, bool appendHashToAssets)
        {
            var outputFileInfo = new FileInfo(PathProvider.MapPath(outputUri));
            var crushedContent = ProcessFiles(outputFileInfo, outputUri, cssFiles, appendHashToAssets);
            
            RetryableFileWriter.SaveContentsToFile(crushedContent.Output, outputFileInfo);
            AddFilesToCache(outputUri, cssFiles, crushedContent.CssAssetFilePaths);
        }


        /// <summary>
        /// Compress the css files and store them in the specified css file.
        /// </summary>
        /// <param name="outputFileInfo">The virtual path for the crushed js file.</param>
        /// <param name="cssRootUri">The path for the crushed css file.</param>
        /// <param name="cssFiles">The css files to be crushed.</param>
        /// <param name="appendHashToAssets"></param>
        public virtual CssCrushedOutput ProcessFiles(FileInfo outputFileInfo, Uri cssRootUri, IEnumerable<CssFile> cssFiles, bool appendHashToAssets)
        {
            var uncompressedContents = new StringBuilder();
            var yahooYuiToBeCompressedContents = new StringBuilder();
			var microsoftAjaxMintoBeCompressedContents = new StringBuilder();
            var localCssAssetFilesThatExist = new List<FileInfo>();
            
            var filesToProcess = cssFiles
                .Select(cssFile => new CssFileProcessor(RetryableFileOpener, PathProvider, CssPathRewriter, cssFile, cssRootUri, appendHashToAssets));

            foreach (var fileToProcess in filesToProcess)
            {
                switch (fileToProcess.CompressionType)
                {
                    case CssCompressionType.None:
                        uncompressedContents.AppendLine(fileToProcess.GetContents());
                        break;
                    case CssCompressionType.YahooYui:
                        yahooYuiToBeCompressedContents.AppendLine(fileToProcess.GetContents());
                        break;
					case CssCompressionType.MicrosoftAjaxMin:
						microsoftAjaxMintoBeCompressedContents.AppendLine(fileToProcess.GetContents());
                		break;
                }

                var cssAssets = fileToProcess.GetLocalCssAssetFilesThatExist();
                localCssAssetFilesThatExist.AddRange(cssAssets);
            }

			if (yahooYuiToBeCompressedContents.Length > 0)
			{
				{
					uncompressedContents.Append(YahooYuiCssCompressor.Value.Compress(yahooYuiToBeCompressedContents.ToString()));
				}
			}

			if (microsoftAjaxMintoBeCompressedContents.Length > 0)
			{
				{
					uncompressedContents.Append(MicrosoftAjaxMinCssCompressor.Value.MinifyStyleSheet(microsoftAjaxMintoBeCompressedContents.ToString()));
				}
			}

        	var crushedOutput = new CssCrushedOutput
            {
                Output = uncompressedContents,
                CssAssetFilePaths = localCssAssetFilesThatExist
            };

            return crushedOutput;
        }

        /// <summary>
        /// Remove all css files from being crushed
        /// </summary>
        /// <param name="outputUri">The path for the crushed css file.</param>
        public virtual void RemoveFiles(Uri outputUri)
        {
            CacheManager.Remove<CssCacheItem>(GetKey(outputUri));
        }

        /// <summary>
        /// Add the css files to the cache so that they are monitored for any changes.
        /// </summary>
        /// <param name="outputUri">The path for the crushed css file.</param>
        /// <param name="cssFiles">The css files to be crushed.</param>
        /// <param name="cssAssetFilePaths">The css asset files referenced by the css files.</param>
        public virtual void AddFilesToCache(Uri outputUri, IEnumerable<CssFile> cssFiles, IEnumerable<FileInfo> cssAssetFilePaths)
        {
            var fileNames = new List<string>
                                {
                                    PathProvider.MapPath(outputUri)
                                };
            fileNames.AddRange(cssFiles.Select(cssFile => PathProvider.MapPath(cssFile.FilePath)));
            fileNames.AddRange(cssAssetFilePaths.Select(cssAssetFilePath => cssAssetFilePath.FullName));

            var cssCacheItem = new CssCacheItem()
                                   {
                                       OutputUri = outputUri,
                                       CssFiles = cssFiles,
                                       CssAssetFilePaths = cssAssetFilePaths
                                   };

            CacheManager.Insert(
                GetKey(outputUri),
                cssCacheItem,
                new CacheDependency(fileNames.ToArray(), System.DateTime.Now),
                Cache.NoAbsoluteExpiration,
                Cache.NoSlidingExpiration,
                CacheItemPriority.High,
                FileRemoved);
        }

        /// <summary>
        /// When a file is removed from cache, keep it in the cache if it is unused or expired as we want to continue to monitor
        /// any changes to file. If it has been removed because the file has changed then regenerate the crushed css file and
        /// start the monitoring again.
        /// </summary>
        /// <param name="key">The key of the cache item.</param>
        /// <param name="value">The value of the cache item.</param>
        /// <param name="reason">The reason the file was removed from cache.</param>
        public virtual void FileRemoved(string key, object value, CacheItemRemovedReason reason)
        {
            var cssCacheItem = (CssCacheItem)value;
            switch (reason)
            {
                case CacheItemRemovedReason.DependencyChanged:
                    AddFiles(cssCacheItem.OutputUri, cssCacheItem.CssFiles, cssCacheItem.AppendHashToAssets);
                    break;
                case CacheItemRemovedReason.Underused:
                case CacheItemRemovedReason.Expired:
                    AddFilesToCache(cssCacheItem.OutputUri, cssCacheItem.CssFiles, cssCacheItem.CssAssetFilePaths);
                    break;
            }
        }

        /// <summary>
        /// Get the cache key to use for caching.
        /// </summary>
        /// <param name="outputUri">The path for the crushed css file.</param>
        /// <returns>The cache key to use for caching.</returns>
        public virtual string GetKey(Uri outputUri)
        {
            var prefix = CssCrusherType + "|";
            return prefix + outputUri;
        }
    }
}