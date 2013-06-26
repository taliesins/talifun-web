using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Caching;
using Talifun.FileWatcher;
using Talifun.Web.Crusher;
using Talifun.Web.Helper;

namespace Talifun.Web.CssSprite
{
    /// <summary>
    /// Manages the creation of css sprites images.
    /// </summary>
    public class CssSpriteCreator : ICssSpriteCreator
    {
        protected readonly ICacheManager CacheManager;
        protected readonly IRetryableFileOpener RetryableFileOpener;
		protected readonly IPathProvider PathProvider;   
        protected readonly IRetryableFileWriter RetryableFileWriter;
        protected readonly IMetaData FileMetaData;
        protected static string CssSpriteCreatorType = typeof(CssSpriteCreator).ToString();
        protected readonly IComparer<SpriteElement> SquarenessComparer = new SquarenessComparer();

        public CssSpriteCreator(ICacheManager cacheManager, IRetryableFileOpener retryableFileOpener, IPathProvider pathProvider, IRetryableFileWriter retryableFileWriter, IMetaData fileMetaData)
        {
            CacheManager = cacheManager;
            RetryableFileOpener = retryableFileOpener;
			PathProvider = pathProvider;
            RetryableFileWriter = retryableFileWriter;
            FileMetaData = fileMetaData;
        }

        /// <summary>
        /// Add images to be generated into sprite image.
        /// </summary>
        /// <param name="imageOutputPath">Sprite image output path.</param>
        /// <param name="spriteImageUrl">Sprite image url.</param>
        /// <param name="cssOutputPath">Sprite css output path.</param>
        /// <param name="files">The component images for the sprite.</param>
        /// <param name="directories">The component images via convention for the sprite </param>
        public virtual IEnumerable<ImageFile> AddFiles(FileInfo imageOutputPath, Uri spriteImageUrl, FileInfo cssOutputPath, IEnumerable<ImageFile> files, IEnumerable<ImageDirectory> directories)
        {
            var filesToWatch = GetFilesToWatch(files, directories);
            var metaDataFiles = filesToWatch.Select(x => new FileInfo(x.FilePath)).Distinct().OrderBy(x => x.FullName);

            var isMetaDataFresh = FileMetaData.IsMetaDataFresh(cssOutputPath, metaDataFiles);
            
            if (!isMetaDataFresh)
            {
                var spriteElements = ProcessFiles(filesToWatch);
                spriteElements = CalculatePositions(spriteElements);
                var etag = SaveSpritesImage(spriteElements, imageOutputPath);
                var css = GetCssSpriteCss(spriteElements, etag, spriteImageUrl);
                RetryableFileWriter.SaveContentsToFile(css, cssOutputPath);

               FileMetaData. CreateMetaData(cssOutputPath, metaDataFiles);
            }

            var foldersToWatch = directories
                .Select(x => Talifun.FileWatcher.EnhancedFileSystemWatcherFactory.Instance
                .CreateEnhancedFileSystemWatcher(new Uri(PathProvider.MapPath(x.DirectoryPath)).LocalPath, x.IncludeFilter, x.ExcludeFilter, x.PollTime, x.IncludeSubDirectories));

            AddFilesToCache(imageOutputPath, spriteImageUrl, cssOutputPath, filesToWatch, files, foldersToWatch, directories);

            return filesToWatch;
        }

        private IEnumerable<ImageFile> GetFilesToWatch(IEnumerable<ImageFile> files, IEnumerable<ImageDirectory> directories)
        {
            var filesInDirectoriesToWatch = directories
                .SelectMany(x => new DirectoryInfo(new Uri(PathProvider.MapPath(x.DirectoryPath)).LocalPath)
                    .GetFiles("*", SearchOption.AllDirectories)
                    .Where(y => (string.IsNullOrEmpty(x.IncludeFilter) || Regex.IsMatch(y.Name, x.IncludeFilter, RegexOptions.Compiled | RegexOptions.IgnoreCase))
                    && (string.IsNullOrEmpty(x.ExcludeFilter) || !Regex.IsMatch(y.Name, x.ExcludeFilter, RegexOptions.Compiled | RegexOptions.IgnoreCase)))
                    )
                    .Select(x=>new ImageFile()
                        {
                            Name = x.Name,
                            FilePath = PathProvider.ToRelative(x.FullName).ToString(),
                        });

            return filesInDirectoriesToWatch.Concat(files).Distinct();
        }

        /// <summary>
        /// Get sprites for image files.
        /// </summary>
        /// <param name="files">The component images for the sprite.</param>
        /// <returns>A list of css sprites</returns>
        public virtual List<SpriteElement> ProcessFiles(IEnumerable<ImageFile> files)
        {
            var spriteElements = new List<SpriteElement>();
            foreach (var file in files)
            {
				var filePath = new Uri(PathProvider.MapPath(file.FilePath)).LocalPath;
                var fileInfo = new FileInfo(filePath);
                using (var reader = RetryableFileOpener.OpenFileStream(fileInfo, 5, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var spriteElement = new SpriteElement(file.Name, reader);
                    spriteElements.Add(spriteElement);
                }
            }

            return spriteElements;
        }



        public List<SpriteElement> CalculatePositions(List<SpriteElement> spriteElements)
        {
            spriteElements.Sort(SquarenessComparer);
            var maxWidth = spriteElements.Select(x => x.Rectangle.Width).Max(x => x);
            return PositionByFillOneColumn(spriteElements, maxWidth);
        }

        /// <summary>
        /// Fill in by rows in a single column. 
        /// </summary>
        /// <param name="binWidth"></param>
        /// <param name="spriteElements"></param>
        public List<SpriteElement> PositionByFillOneColumn(List<SpriteElement> spriteElements, int binWidth)
        {
            // Make lists of positioned and not positioned rectangles.
            var notPositioned = new List<SpriteElement>(spriteElements);
            var positioned = new List<SpriteElement>();

            // Arrange the rectangles.
            var x = 0;
            var y = 0;
            var rowHeight = 0;
            while (notPositioned.Count > 0)
            {
                // Find the next rectangle that will fit on this row.
                var nextSpritePosition = -1;
                for (var i = 0; i <= notPositioned.Count - 1; i++)
                {
                    if (x + notPositioned[i].Rectangle.Width > binWidth) continue;
                    nextSpritePosition = i;
                    break;
                }

                // If we didn't find a rectangle that fits, start a new row.
                if (nextSpritePosition < 0)
                {
                    y += rowHeight;
                    x = 0;
                    rowHeight = 0;
                    nextSpritePosition = 0;
                }

                // Position the selected rectangle.
                var sprite = notPositioned[nextSpritePosition];

                var rectangle = sprite.Rectangle;
                rectangle.X = x;
                rectangle.Y = y;

                x += rectangle.Width;
                if (rowHeight < rectangle.Height)
                {
                    rowHeight = rectangle.Height;
                }

                sprite.Rectangle = rectangle;

                // Move the rectangle into the positioned list.
                positioned.Add(sprite);
                notPositioned.RemoveAt(nextSpritePosition);
            }

            return positioned;
        }


        /// <summary>
        /// Save sprites image.
        /// </summary>
        /// <param name="spriteElements">The sprite elements to save</param>
        /// <param name="imageOutputPath">The location to save the image too.</param>
        /// <returns>etag of generated sprite file</returns>
        public virtual string SaveSpritesImage(List<SpriteElement> spriteElements, FileInfo imageOutputPath)
        {
            using (var image = GetCssSpriteImage(spriteElements))
            {
                using (var writer = new MemoryStream())
                {
                    image.Save(writer, ImageFormat.Png);
                    writer.Flush();

                    return RetryableFileWriter.SaveContentsToFile(writer, imageOutputPath);
                }
            }
        }

        /// <summary>
        /// Remove all css files from being crushed
        /// </summary>
        /// <param name="outputUri">The path for the crushed css file.</param>
        public virtual void RemoveGroup(Uri outputUri)
        {
            CacheManager.Remove<CssSpriteCacheItem>(GetKey(outputUri));
        }

        /// <summary>
        /// Add the images to the cache so that they are monitored for any changes.
        /// </summary>
        /// <param name="imageOutputPath">Sprite image output path.</param>
        /// <param name="outputUri">Sprite image url.</param>
        /// <param name="cssOutputPath">Sprite css output path.</param>
        /// <param name="files">The component images for the sprite.</param>
        public virtual void AddFilesToCache(FileInfo imageOutputPath, Uri outputUri, FileInfo cssOutputPath, IEnumerable<ImageFile> filesToWatch, IEnumerable<ImageFile> files, IEnumerable<Talifun.FileWatcher.IEnhancedFileSystemWatcher> foldersToWatch, IEnumerable<ImageDirectory> directories)
        {
            var fileNames = new List<string>
            {
                imageOutputPath.FullName,
                cssOutputPath.FullName
            };
            fileNames.AddRange(filesToWatch.Select(fileToWatch => new Uri(PathProvider.MapPath(fileToWatch.FilePath)).LocalPath));

            var cssSpriteCacheItem = new CssSpriteCacheItem()
            {
                Files = files,
                Directories = directories,
                CssOutputPath = cssOutputPath,
                ImageOutputPath = imageOutputPath,
                FoldersToWatch = foldersToWatch,
                FilesToWatch = filesToWatch,
                SpriteImageUrl = outputUri
            };

            CacheManager.Insert(
                GetKey(outputUri),
                cssSpriteCacheItem,
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
        /// any changes to file. If it has been removed because the file has changed then regenerate the sprite image and
        /// start the monitoring again.
        /// </summary>
        /// <param name="key">The key of the cache item.</param>
        /// <param name="value">The value of the cache item.</param>
        /// <param name="reason">The reason the file was removed from cache.</param>
        public virtual void FileRemoved(string key, object value, CacheItemRemovedReason reason)
        {
            var cssSpriteCacheItem = (CssSpriteCacheItem)value;
            foreach (var enhancedFileSystemWatcher in cssSpriteCacheItem.FoldersToWatch)
            {
                enhancedFileSystemWatcher.Stop();
                enhancedFileSystemWatcher.FileActivityFinishedEvent -= OnFileActivityFinishedEvent;
            }
            switch (reason)
            {
                case CacheItemRemovedReason.DependencyChanged:
                    AddFiles(cssSpriteCacheItem.ImageOutputPath, cssSpriteCacheItem.SpriteImageUrl, cssSpriteCacheItem.CssOutputPath, cssSpriteCacheItem.Files, cssSpriteCacheItem.Directories);
                    break;
                case CacheItemRemovedReason.Underused:
                case CacheItemRemovedReason.Expired:
                    AddFilesToCache(cssSpriteCacheItem.ImageOutputPath, cssSpriteCacheItem.SpriteImageUrl, cssSpriteCacheItem.CssOutputPath, cssSpriteCacheItem.FilesToWatch, cssSpriteCacheItem.Files, cssSpriteCacheItem.FoldersToWatch, cssSpriteCacheItem.Directories);
                    break;
            }
        }

        /// <summary>
        /// Remove sprite image from cache.
        /// </summary>
        /// <param name="spriteImageUrl">Sprite image url.</param>
        public virtual void RemoveFiles(Uri spriteImageUrl)
        {
            CacheManager.Remove<CssSpriteCacheItem>(GetKey(spriteImageUrl));
        }

        /// <summary>
        /// Get the cache key to use.
        /// </summary>
        /// <param name="spriteImageUrl">Sprite image url.</param>
        /// <returns></returns>
        public virtual string GetKey(Uri spriteImageUrl)
        {
            return CssSpriteCreatorType + "|" + spriteImageUrl;
        }

        /// <summary>
        /// Generate the css that denotes the composite sprite part locations and dimensions.
        /// </summary>
        /// <param name="spriteElements">The sprites that make the image up.</param>
        /// <param name="etag">The unique hash for the sprite image.</param>
        /// <param name="cssSpriteImageUrl">The url of the sprite image.</param>
        /// <returns></returns>
        public virtual string GetCssSpriteCss(IEnumerable<SpriteElement> spriteElements, string etag, Uri cssSpriteImageUrl)
        {
            var cssBuilder = new StringBuilder();
            
            foreach (var element in spriteElements)
            {
                var className = element.Name.Replace(".", "_").Replace(" ", "_");
                if (className.Length < 1)
                {
                    continue;
                }
                if (Char.IsDigit(className[0]))
                {
                    className = "_" + className;
                }
                cssBuilder.AppendFormat(".{0} {{background-image: url('{1}');background-position: -{2}px -{3}px;width: {4}px;height: {5}px;}}", className, cssSpriteImageUrl + "?" + etag, element.Rectangle.X + element.BorderWidth, element.Rectangle.Y + element.BorderWidth, element.Image.Width, element.Image.Height);    
            }

            return cssBuilder.ToString();
        }

        /// <summary>
        /// Generate image from composite sprite parts.
        /// </summary>
        /// <param name="spriteElements">The sprites that make the image up.</param>
        /// <returns>Image the represents all the sprites.</returns>
        public virtual Bitmap GetCssSpriteImage(IEnumerable<SpriteElement> spriteElements)
        {
            var maxWidth = Width(spriteElements);
            var maxHeight = Height(spriteElements);

            var sprite = new Bitmap(maxWidth, maxHeight);
            var graphic = Graphics.FromImage(sprite);

            foreach (var element in spriteElements)
            {
                graphic.DrawImage(element.Image, element.Rectangle.X + element.BorderWidth, element.Rectangle.Y + element.BorderWidth, element.Image.Width, element.Image.Height);
            }

            return sprite;
        }

        /// <summary>
        /// Get the height of all the sprites added together.
        /// </summary>
        /// <param name="spriteElements">The sprites that make the image up.</param>
        /// <returns>The height if all the sprites added together.</returns>
        public virtual int Height(IEnumerable<SpriteElement> spriteElements)
        {
            return spriteElements.Max(y => y.Rectangle.Y + y.Rectangle.Height);
        }

        /// <summary>
        /// Get the width of the widest sprite.
        /// </summary>
        /// <param name="spriteElements">The sprites that make the image up.</param>
        /// <returns>The width of the widest sprite.</returns>
        public virtual int Width(IEnumerable<SpriteElement> spriteElements)
        {
            return spriteElements.Max(x =>x.Rectangle.X + x.Rectangle.Width);
        }
    }
}
