using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Caching;
using Talifun.Crusher.Configuration.Css;
using Talifun.FileWatcher;
using Talifun.Web;
using Talifun.Web.Helper;
using Talifun.Web.Helper.Pooling;

#if NET35
using Talifun.Web;
#endif

namespace Talifun.Crusher.Crusher
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
        protected readonly IMetaData FileMetaData;
        protected static string CssCrusherType = typeof(CssCrusher).ToString();
		protected readonly Pool<Yahoo.Yui.Compressor.CssCompressor> YahooYuiCssCompressorPool;
		protected readonly Pool<Microsoft.Ajax.Utilities.Minifier> MicrosoftAjaxMinCssCompressorPool;
        protected readonly bool WatchAssets = false;

        public CssCrusher(ICacheManager cacheManager, IPathProvider pathProvider, IRetryableFileOpener retryableFileOpener, IRetryableFileWriter retryableFileWriter, ICssPathRewriter cssPathRewriter, IMetaData fileMetaData, bool watchAssets)
        {
            CacheManager = cacheManager;
            PathProvider = pathProvider;
            RetryableFileOpener = retryableFileOpener;
            RetryableFileWriter = retryableFileWriter;
            CssPathRewriter = cssPathRewriter;
            FileMetaData = fileMetaData;
            WatchAssets = watchAssets;
            YahooYuiCssCompressorPool = new Pool<Yahoo.Yui.Compressor.CssCompressor>(64, pool => new Yahoo.Yui.Compressor.CssCompressor(), LoadingMode.LazyExpanding, AccessMode.Circular);
            MicrosoftAjaxMinCssCompressorPool = new Pool<Microsoft.Ajax.Utilities.Minifier>(64, pool => new Microsoft.Ajax.Utilities.Minifier(), LoadingMode.LazyExpanding, AccessMode.Circular);
        }

        /// <summary>
        /// Add css files to be crushed.
        /// </summary>
        /// <param name="outputUri">The virtual path for the crushed css file.</param>
        /// <param name="files">The css files to be crushed.</param>
        /// <param name="directories"> </param>
        /// <param name="appendHashToAssets">Should css assets have a hash appended to them.</param>
        public virtual CssCrushedOutput AddGroup(Uri outputUri, IEnumerable<CssFile> files, IEnumerable<CssDirectory> directories, bool appendHashToAssets)
        {
            var outputFileInfo = new FileInfo(new Uri(PathProvider.MapPath(outputUri)).LocalPath);
            var crushedContent = ProcessGroup(outputFileInfo, outputUri, files, directories, appendHashToAssets);
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
                .SelectMany(x => new DirectoryInfo(new Uri(PathProvider.MapPath(x.DirectoryPath)).LocalPath)
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
    		var filesToWatch = GetFilesToWatch(files, directories);

            var filesToProcess = filesToWatch
                .Select(cssFile => new CssFileProcessor(RetryableFileOpener, PathProvider, CssPathRewriter, cssFile.FilePath, cssFile.CompressionType, cssRootUri, appendHashToAssets));

            var localCssAssetFilesThatExist = new List<FileInfo>();
            if (WatchAssets)
            {
                localCssAssetFilesThatExist = filesToProcess
                    .SelectMany(x => x.GetLocalCssAssetFilesThatExist().Select(y => y.File))
                    .ToList();
            }

            var metaDataFiles = filesToWatch
                .Select(x => new FileInfo(x.FilePath)).Concat(localCssAssetFilesThatExist)
                .Distinct()
                .OrderBy(x => x.FullName);

            var isMetaDataFresh = FileMetaData.IsMetaDataFresh(outputFileInfo, metaDataFiles);

            if (!isMetaDataFresh)
            {
                var content = GetGroupContent(filesToProcess);

                RetryableFileWriter.SaveContentsToFile(content, outputFileInfo);

                FileMetaData.CreateMetaData(outputFileInfo, metaDataFiles);
            }

            var foldersToWatch = directories
                .Select(x => Talifun.FileWatcher.EnhancedFileSystemWatcherFactory.Instance
                .CreateEnhancedFileSystemWatcher(new Uri(PathProvider.MapPath(x.DirectoryPath)).LocalPath, x.IncludeFilter, x.ExcludeFilter, x.PollTime, x.IncludeSubDirectories));

            var crushedOutput = new CssCrushedOutput
            {
				FilesToWatch = filesToWatch,
                FoldersToWatch = foldersToWatch,
                CssAssetFilePaths = localCssAssetFilesThatExist
            };

            return crushedOutput;
        }

        private StringBuilder GetGroupContent(IEnumerable<CssFileProcessor> filesToProcess)
        {
            var uncompressedContents = new StringBuilder();
            var yahooYuiToBeCompressedContents = new StringBuilder();
            var microsoftAjaxMintoBeCompressedContents = new StringBuilder();

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
            }

            if (yahooYuiToBeCompressedContents.Length > 0)
            {
                var yahooYuiCssCompressor = YahooYuiCssCompressorPool.Acquire();
                try
                {
                    uncompressedContents.Append(
                        yahooYuiCssCompressor.Compress(yahooYuiToBeCompressedContents.ToString()));
                }
                finally
                {
                    YahooYuiCssCompressorPool.Release(yahooYuiCssCompressor);
                }
            }

            if (microsoftAjaxMintoBeCompressedContents.Length > 0)
            {
                var microsoftAjaxMinCssCompressor = MicrosoftAjaxMinCssCompressorPool.Acquire();
                try
                {
                    uncompressedContents.Append(
                        microsoftAjaxMinCssCompressor.MinifyStyleSheet(microsoftAjaxMintoBeCompressedContents.ToString()));
                }
                finally
                {
                    MicrosoftAjaxMinCssCompressorPool.Release(microsoftAjaxMinCssCompressor);
                }
            }
            return uncompressedContents;
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
			fileNames.AddRange(filesToWatch.Select(cssFile => new Uri(PathProvider.MapPath(cssFile.FilePath)).LocalPath));
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
                    AddGroup(cacheItem.OutputUri, cacheItem.Files, cacheItem.Directories, cacheItem.AppendHashToAssets);
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