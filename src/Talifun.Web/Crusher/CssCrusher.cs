using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Caching;
using Talifun.FileWatcher;
using Talifun.Web.Helper;
#if NET35
using Talifun.Web;
#endif

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

		protected readonly object YahooYuiCssCompressorLock = new object();
		protected readonly Lazy<Yahoo.Yui.Compressor.CssCompressor> YahooYuiCssCompressor;

		protected readonly object MicrosoftAjaxMinCssCompressorLock = new object();
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
    	/// <param name="files">The css files to be crushed.</param>
    	/// <param name="directories"> </param>
    	/// <param name="appendHashToAssets">Should css assets have a hash appended to them.</param>
    	public virtual CssCrushedOutput CreateGroup(Uri outputUri, IEnumerable<CssFile> files, IEnumerable<CssDirectory> directories, bool appendHashToAssets)
        {
            var outputFileInfo = new FileInfo(PathProvider.MapPath(outputUri));
            var crushedContent = ProcessGroup(outputFileInfo, outputUri, files, directories, appendHashToAssets);
            
            RetryableFileWriter.SaveContentsToFile(crushedContent.Output, outputFileInfo);
			AddGroupToCache(outputUri, crushedContent.FilesToWatch, crushedContent.CssAssetFilePaths, files, crushedContent.FoldersToWatch, directories);

    	    return crushedContent;
        }

        /// <summary>
        /// Get all files and files in directories that are going to be crushed.
        /// </summary>
        /// <param name="files"></param>
        /// <param name="directories"></param>
        /// <returns></returns>
        private IEnumerable<CssFileToWatch> GetFilesToWatch(IEnumerable<CssFile> files, IEnumerable<CssDirectory> directories)
        {
            var filesToWatch = files.Select(x => new CssFileToWatch()
            {
                CompressionType = x.CompressionType,
                FilePath = x.FilePath
            });

            var filesInDirectoriesToWatch = directories
                .SelectMany(x => new DirectoryInfo(PathProvider.MapPath(x.DirectoryPath))
                    .GetFiles( "*", SearchOption.AllDirectories)
                    .Where(y => (string.IsNullOrEmpty(x.IncludeFilter) || Regex.IsMatch(y.Name, x.IncludeFilter, RegexOptions.Compiled | RegexOptions.IgnoreCase))
                    && (string.IsNullOrEmpty(x.ExcludeFilter) || !Regex.IsMatch(y.Name, x.ExcludeFilter, RegexOptions.Compiled | RegexOptions.IgnoreCase)))
                    .Select(y => new CssFileToWatch()
                    {
                        CompressionType = x.CompressionType,
                        FilePath = PathProvider.ToRelative(y.FullName).ToString()
                    }));

            filesToWatch = filesInDirectoriesToWatch.Concat(filesToWatch).Distinct(new CssFileToWatchEqualityComparer());

            return filesToWatch;
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
            var yahooYuiToBeCompressedContents = new StringBuilder();
			var microsoftAjaxMintoBeCompressedContents = new StringBuilder();
            var localCssAssetFilesThatExist = new List<FileInfo>();

    		var filesToWatch = GetFilesToWatch(files, directories);

            var filesToProcess = filesToWatch
                .Select(cssFile => new CssFileProcessor(RetryableFileOpener, PathProvider, CssPathRewriter, cssFile.FilePath, cssFile.CompressionType, cssRootUri, appendHashToAssets));

            var foldersToWatch = directories
                .Select(x =>
                    Talifun.FileWatcher.EnhancedFileSystemWatcherFactory.Instance
                    .CreateEnhancedFileSystemWatcher(PathProvider.MapPath(x.DirectoryPath), x.IncludeFilter, x.ExcludeFilter, x.PollTime, x.IncludeSubDirectories));

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

                var cssAssets = fileToProcess.GetLocalCssAssetFilesThatExist().Select(x=>x.File);
                localCssAssetFilesThatExist.AddRange(cssAssets);
            }

			if (yahooYuiToBeCompressedContents.Length > 0)
			{
				lock (YahooYuiCssCompressorLock)
				{
					uncompressedContents.Append(YahooYuiCssCompressor.Value.Compress(yahooYuiToBeCompressedContents.ToString()));
				}
			}

			if (microsoftAjaxMintoBeCompressedContents.Length > 0)
			{
				lock (MicrosoftAjaxMinCssCompressorLock)
				{
					uncompressedContents.Append(MicrosoftAjaxMinCssCompressor.Value.MinifyStyleSheet(microsoftAjaxMintoBeCompressedContents.ToString()));
				}
			}

            var crushedOutput = new CssCrushedOutput
            {
                Output = uncompressedContents,
				FilesToWatch = filesToWatch,
                FoldersToWatch = foldersToWatch,
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
        /// <param name="foldersToWatch"> </param>
        /// <param name="directories">The css directories to be crushed. </param>
        public virtual void AddGroupToCache(Uri outputUri, IEnumerable<CssFileToWatch> filesToWatch, IEnumerable<FileInfo> assetFilesToWatch, IEnumerable<CssFile> files, IEnumerable<Talifun.FileWatcher.IEnhancedFileSystemWatcher> foldersToWatch, IEnumerable<CssDirectory> directories)
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
                FoldersToWatch = foldersToWatch,
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

            foreach (var enhancedFileSystemWatcher in foldersToWatch)
            {
                enhancedFileSystemWatcher.FileActivityFinishedEvent += OnFileActivityFinishedEvent;
                enhancedFileSystemWatcher.UserState = outputUri;
                enhancedFileSystemWatcher.Start();
            }
        }

        /// <summary>
        /// When file events have finished it means we should should remove them from the cache
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void OnFileActivityFinishedEvent(object sender, FileWatcher.FileActivityFinishedEventArgs e)
        {
            var outputUri = (Uri)e.UserState;

            if (e.FileEventItems.Any(x => x.FileEventType != FileEventType.InDirectory))
            {
                RemoveGroup(outputUri);
            }
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
            foreach (var enhancedFileSystemWatcher in cacheItem.FoldersToWatch)
            {
                enhancedFileSystemWatcher.Stop();
                enhancedFileSystemWatcher.FileActivityFinishedEvent -= OnFileActivityFinishedEvent;
            }
            switch (reason)
            {
                case CacheItemRemovedReason.DependencyChanged:
                    CreateGroup(cacheItem.OutputUri, cacheItem.Files, cacheItem.Directories, cacheItem.AppendHashToAssets);
                    break;
                case CacheItemRemovedReason.Underused:
                case CacheItemRemovedReason.Expired:
					AddGroupToCache(cacheItem.OutputUri, cacheItem.FilesToWatch, cacheItem.AssetFilesToWatch, cacheItem.Files, cacheItem.FoldersToWatch, cacheItem.Directories);
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