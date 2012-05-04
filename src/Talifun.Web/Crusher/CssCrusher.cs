using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Caching;
using Talifun.Web.Helper;
using Yahoo.Yui.Compressor;

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

        public CssCrusher(ICacheManager cacheManager, IPathProvider pathProvider, IRetryableFileOpener retryableFileOpener, IRetryableFileWriter retryableFileWriter, ICssPathRewriter cssPathRewriter)
        {
            CacheManager = cacheManager;
            PathProvider = pathProvider;
            RetryableFileOpener = retryableFileOpener;
            RetryableFileWriter = retryableFileWriter;
            CssPathRewriter = cssPathRewriter;
        }

    	/// <summary>
    	/// Add css files to be crushed.
    	/// </summary>
    	/// <param name="outputUri">The virtual path for the crushed css file.</param>
    	/// <param name="files">The css files to be crushed.</param>
    	/// <param name="directories"> </param>
    	/// <param name="appendHashToAssets">Should css assets have a hash appended to them.</param>
    	public virtual void CreateGroup(Uri outputUri, IEnumerable<CssFile> files, IEnumerable<CssDirectory> directories, bool appendHashToAssets)
        {
            var outputFileInfo = new FileInfo(PathProvider.MapPath(outputUri));
            var crushedContent = ProcessGroup(outputFileInfo, outputUri, files, directories, appendHashToAssets);
            
            RetryableFileWriter.SaveContentsToFile(crushedContent.Output, outputFileInfo);
			AddGroupToCache(outputUri, crushedContent.FilesToWatch, crushedContent.CssAssetFilePaths, files, directories);
        }

		private IEnumerable<CssFileToWatch> GetFilesToWatch(IEnumerable<CssFile> files, IEnumerable<CssDirectory> directories)
        {
			return files.Select(x => new CssFileToWatch()
            {
                CompressionType = x.CompressionType,
                FilePath = x.FilePath
            });
        }

    	/// <summary>
    	/// Compress the css files and store them in the specified css file.
    	/// </summary>
    	/// <param name="outputFileInfo">The virtual path for the crushed js file.</param>
    	/// <param name="cssRootUri">The path for the crushed css file.</param>
    	/// <param name="files">The css files to be crushed.</param>
    	/// <param name="directories">The css directories to be crushed.</param>
    	/// <param name="appendHashToAssets"></param>
    	public virtual CssCrushedOutput ProcessGroup(FileInfo outputFileInfo, Uri cssRootUri, IEnumerable<CssFile> files, IEnumerable<CssDirectory> directories, bool appendHashToAssets)
        {
            var uncompressedContents = new StringBuilder();
            var toBeStockYuiCompressedContents = new StringBuilder();
            var toBeMichaelAshRegexCompressedContents = new StringBuilder();
            var toBeHybridCompressedContents = new StringBuilder();
            var localCssAssetFilesThatExist = new List<FileInfo>();

    		var filesToWatch = GetFilesToWatch(files, directories);

            var filesToProcess = filesToWatch
                .Select(cssFile => new CssFileProcessor(RetryableFileOpener, PathProvider, CssPathRewriter, cssFile.FilePath, cssFile.CompressionType, cssRootUri, appendHashToAssets));

            foreach (var fileToProcess in filesToProcess)
            {
                switch (fileToProcess.CompressionType)
                {
                    case CssCompressionType.None:
                        uncompressedContents.AppendLine(fileToProcess.GetContents());
                        break;
                    case CssCompressionType.StockYuiCompressor:
                        toBeStockYuiCompressedContents.AppendLine(fileToProcess.GetContents());
                        break;
                    case CssCompressionType.MichaelAshRegexEnhancements:
                        toBeMichaelAshRegexCompressedContents.AppendLine(fileToProcess.GetContents());
                        break;
                    case CssCompressionType.Hybrid:
                        toBeHybridCompressedContents.AppendLine(fileToProcess.GetContents());
                        break;
                }

                var cssAssets = fileToProcess.GetLocalCssAssetFilesThatExist();
                localCssAssetFilesThatExist.AddRange(cssAssets);
            }

            if (toBeStockYuiCompressedContents.Length > 0)
            {
				uncompressedContents.Append(CssCompressor.Compress(toBeStockYuiCompressedContents.ToString(), 0, Yahoo.Yui.Compressor.CssCompressionType.StockYuiCompressor, true));
            }

            if (toBeMichaelAshRegexCompressedContents.Length > 0)
            {
				uncompressedContents.Append(CssCompressor.Compress(toBeMichaelAshRegexCompressedContents.ToString(), 0, Yahoo.Yui.Compressor.CssCompressionType.MichaelAshRegexEnhancements, true));
            }

            if (toBeHybridCompressedContents.Length > 0)
            {
				uncompressedContents.Append(CssCompressor.Compress(toBeHybridCompressedContents.ToString(), 0, Yahoo.Yui.Compressor.CssCompressionType.Hybrid, true));
            }

            var crushedOutput = new CssCrushedOutput
                                    {
                                        Output = uncompressedContents,
										FilesToWatch = filesToWatch,
                                        CssAssetFilePaths = localCssAssetFilesThatExist
                                    };

            return crushedOutput;
        }

        /// <summary>
        /// Remove all css files from being crushed
        /// </summary>
        /// <param name="outputUri">The path for the crushed css file.</param>
        public virtual void RemoveGroup(Uri outputUri)
        {
            CacheManager.Remove<CssCacheItem>(GetKey(outputUri));
        }

    	/// <summary>
    	/// Add the css files to the cache so that they are monitored for any changes.
    	/// </summary>
    	/// <param name="outputUri">The path for the crushed css file.</param>
		/// <param name="filesToWatch">Files that are crushed.</param>
		/// <param name="assetFilesToWatch">The css asset files referenced by the css files.</param>
		/// <param name="files">The css files to be crushed.</param>
    	/// <param name="directories">The css directories to be crushed. </param>
    	public virtual void AddGroupToCache(Uri outputUri, IEnumerable<CssFileToWatch> filesToWatch, IEnumerable<FileInfo> assetFilesToWatch, IEnumerable<CssFile> files, IEnumerable<CssDirectory> directories)
        {
            var fileNames = new List<string>
            {
                PathProvider.MapPath(outputUri)
            };
			fileNames.AddRange(filesToWatch.Select(cssFile => PathProvider.MapPath(cssFile.FilePath)));
            fileNames.AddRange(assetFilesToWatch.Select(cssAssetFilePath => cssAssetFilePath.FullName));

            var cacheItem = new CssCacheItem()
            {
                OutputUri = outputUri,
				AssetFilesToWatch = assetFilesToWatch,
				FilesToWatch = filesToWatch,
                Files = files,
				Directories = directories,
            };

            CacheManager.Insert(
                GetKey(outputUri),
                cacheItem,
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
            var cacheItem = (CssCacheItem)value;
            switch (reason)
            {
                case CacheItemRemovedReason.DependencyChanged:
                    CreateGroup(cacheItem.OutputUri, cacheItem.Files, cacheItem.Directories, cacheItem.AppendHashToAssets);
                    break;
                case CacheItemRemovedReason.Underused:
                case CacheItemRemovedReason.Expired:
					AddGroupToCache(cacheItem.OutputUri, cacheItem.FilesToWatch, cacheItem.AssetFilesToWatch, cacheItem.Files, cacheItem.Directories);
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