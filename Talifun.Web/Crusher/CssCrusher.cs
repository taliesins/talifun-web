using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;
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
        protected readonly IRetryableFileOpener RetryableFileOpener;
        protected readonly IRetryableFileWriter RetryableFileWriter;
        protected readonly ICssPathRewriter CssPathRewriter;
        protected readonly IPathProvider PathProvider;
        protected static string CssCrusherType = typeof(CssCrusher).ToString();

        public CssCrusher(IRetryableFileOpener retryableFileOpener, IRetryableFileWriter retryableFileWriter, ICssPathRewriter cssPathRewriter, IPathProvider pathProvider)
        {
            RetryableFileOpener = retryableFileOpener;
            RetryableFileWriter = retryableFileWriter;
            CssPathRewriter = cssPathRewriter;
            PathProvider = pathProvider;
        }

        /// <summary>
        /// Add css files to be crushed.
        /// </summary>
        /// <param name="outputUri">The virtual path for the crushed css file.</param>
        /// <param name="cssFiles">The css files to be crushed.</param>
        /// <param name="appendHashToAssets">Should css assets have a hash appended to them.</param>
        public virtual void AddFiles(Uri outputUri, IEnumerable<CssFile> cssFiles, bool appendHashToAssets)
        {
            IEnumerable<FileInfo> cssAssetFilePaths;
            var crushedContent = ProcessFiles(outputUri, cssFiles, appendHashToAssets, out cssAssetFilePaths);
            var outputFileInfo = new FileInfo(PathProvider.MapPath(outputUri));
            RetryableFileWriter.SaveContentsToFile(crushedContent, outputFileInfo);
            AddFilesToCache(outputUri, cssFiles, cssAssetFilePaths);
        }

        /// <summary>
        /// Compiles dot less css file contents.
        /// </summary>
        /// <param name="fileContents">Uncompiled css file contents.</param>
        /// <returns>Compiled css file contents.</returns>
        public virtual string ProcessDotLess(string fileContents)
        {
            return dotless.Core.Less.Parse(fileContents);
        }

        public virtual Uri GetUriDirectory(Uri uri)
        {            
            var path = uri.OriginalString;

            var queryStringPosition = path.IndexOf('?');

            if (queryStringPosition > -1)
            {
                path = path.Substring(0, queryStringPosition);
            }

            var startIndex = path.Length - Path.GetFileName(path).Length;

            path = path.Remove(startIndex);

            return new Uri(path, UriKind.RelativeOrAbsolute);
        }

        /// <summary>
        /// Compress the css files and store them in the specified css file.
        /// </summary>
        /// <param name="cssRootUri">The path for the crushed css file.</param>
        /// <param name="cssFiles">The css files to be crushed.</param>
        /// <param name="appendHashToAssets"></param>
        /// <param name="cssAssetFilePaths">The asset file paths in the crushed css file.</param>
        public virtual StringBuilder ProcessFiles(Uri cssRootUri, IEnumerable<CssFile> cssFiles, bool appendHashToAssets, out IEnumerable<FileInfo> cssAssetFilePaths)
        {
            var cssRootPathUri = GetUriDirectory(cssRootUri);
            cssRootPathUri = !cssRootUri.IsAbsoluteUri
                                       ? new Uri(PathProvider.MapPath(cssRootUri))
                                       : cssRootPathUri;

            var uncompressedContents = new StringBuilder();
            var toBeStockYuiCompressedContents = new StringBuilder();
            var toBeMichaelAshRegexCompressedContents = new StringBuilder();
            var toBeHybridCompressedContents = new StringBuilder();
            var localCssAssetFilesThatExist = new List<FileInfo>(); 

            foreach (var cssFile in cssFiles)
            {
                var cssFilePath = VirtualPathUtility.ToAbsolute(cssFile.FilePath);
                var relativeRootUri = GetUriDirectory(new Uri(cssFilePath, UriKind.RelativeOrAbsolute));
                relativeRootUri = !relativeRootUri.IsAbsoluteUri
                                      ? new Uri(PathProvider.MapPath(relativeRootUri))
                                      : relativeRootUri;

                var cssFileInfo = new FileInfo(PathProvider.MapPath(cssFilePath));

                var fileContents = RetryableFileOpener.ReadAllText(cssFileInfo);
                var fileName = cssFileInfo.Name.ToLower();

                if (fileName.EndsWith(".less") || fileName.EndsWith(".less.css"))
                {
                    fileContents = ProcessDotLess(fileContents);
                }

                var distinctRelativePaths = CssPathRewriter.FindDistinctRelativePaths(fileContents);
                fileContents = CssPathRewriter.RewriteCssPathsToBeRelativeToPath(distinctRelativePaths, cssRootPathUri, relativeRootUri, fileContents);

                if (appendHashToAssets)
                {
                    var distinctLocalPaths = CssPathRewriter.FindDistinctLocalPaths(fileContents);
                    var distinctLocalPathsThatExist = new List<Uri>();

                    foreach (var distinctLocalPath in distinctLocalPaths)
                    {
                        var cssAssetFileInfo = new FileInfo(PathProvider.MapPath(cssRootPathUri, distinctLocalPath));

                        if (!cssAssetFileInfo.Exists)
                        {
                            continue;
                        }

                        distinctLocalPathsThatExist.Add(distinctLocalPath);
                        localCssAssetFilesThatExist.Add(cssAssetFileInfo);
                    }

                    fileContents = CssPathRewriter.RewriteCssPathsToAppendHash(distinctLocalPathsThatExist, cssRootPathUri, fileContents);
                }

                switch (cssFile.CompressionType)
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

            if (toBeStockYuiCompressedContents.Length > 0)
            {
                uncompressedContents.Append(CssCompressor.Compress(toBeStockYuiCompressedContents.ToString(), 0, Yahoo.Yui.Compressor.CssCompressionType.StockYuiCompressor));
            }

            if (toBeMichaelAshRegexCompressedContents.Length > 0)
            {
                uncompressedContents.Append(CssCompressor.Compress(toBeMichaelAshRegexCompressedContents.ToString(), 0, Yahoo.Yui.Compressor.CssCompressionType.MichaelAshRegexEnhancements));
            }

            if (toBeHybridCompressedContents.Length > 0)
            {
                uncompressedContents.Append(CssCompressor.Compress(toBeHybridCompressedContents.ToString(), 0, Yahoo.Yui.Compressor.CssCompressionType.Hybrid));
            }

            cssAssetFilePaths = localCssAssetFilesThatExist;
            return uncompressedContents;
        }

        /// <summary>
        /// Remove all css files from being crushed
        /// </summary>
        /// <param name="outputUri">The path for the crushed css file.</param>
        public virtual void RemoveFiles(Uri outputUri)
        {
            HttpRuntime.Cache.Remove(GetKey(outputUri));
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

            foreach (var cssFile in cssFiles)
            {
                fileNames.Add(PathProvider.MapPath(cssFile.FilePath));
            }

            foreach (var cssAssetFilePath in cssAssetFilePaths)
            {
                fileNames.Add(cssAssetFilePath.FullName);
            }

            var cssCacheItem = new CssCacheItem()
                                   {
                                       OutputUri = outputUri,
                                       CssFiles = cssFiles,
                                       CssAssetFilePaths = cssAssetFilePaths
                                   };

            HttpRuntime.Cache.Insert(
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