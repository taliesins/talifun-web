using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Caching;
using System.Web.Hosting;
using Talifun.Web.Helper;

namespace Talifun.Web.CssSprite
{
    /// <summary>
    /// Manages the creation of css sprites images.
    /// </summary>
    public class CssSpriteCreator : ICssSpriteCreator
    {
        private readonly int BufferSize = 32768;
        private readonly int ImagePadding = 2;

        private readonly IRetryableFileOpener _retryableFileOpener;
        private readonly IHasher _hasher;
        private readonly IRetryableFileWriter _retryableFileWriter;

        protected static string CssSpriteCreatorType = typeof(CssSpriteCreator).ToString();

        public CssSpriteCreator()
        {
            _retryableFileOpener = new RetryableFileOpener();
            _hasher = new Hasher(_retryableFileOpener);
            _retryableFileWriter = new RetryableFileWriter(BufferSize, _retryableFileOpener, _hasher);
        }

        /// <summary>
        /// Add images to be generated into sprite image.
        /// </summary>
        /// <param name="imageOutputPath">Sprite image output path.</param>
        /// <param name="spriteImageUrl">Sprite image url.</param>
        /// <param name="cssOutputPath">Sprite css output path.</param>
        /// <param name="files">The component images for the sprite.</param>
        public virtual void AddFiles(string imageOutputPath, string spriteImageUrl, string cssOutputPath, IEnumerable<ImageFile> files)
        {
            var spriteElements = ProcessFiles(files);
            var etag = SaveSpritesImage(spriteElements, imageOutputPath);
            var cssSpriteImageUrl = string.IsNullOrEmpty(spriteImageUrl) ? VirtualPathUtility.ToAbsolute(imageOutputPath) : spriteImageUrl;
            var css = GetCssSpriteCss(spriteElements, etag, cssSpriteImageUrl);
            _retryableFileWriter.SaveContentsToFile(css, cssOutputPath);
            AddFilesToCache(imageOutputPath, spriteImageUrl, cssOutputPath, files);
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
                var filePath = HostingEnvironment.MapPath(file.FilePath);
                var fileInfo = new FileInfo(filePath);
                using (var reader = _retryableFileOpener.OpenFileStream(fileInfo, 5, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var spriteElement = new SpriteElement(file.Name, reader);
                    spriteElements.Add(spriteElement);
                }
            }

            return spriteElements;
        }

        /// <summary>
        /// Save sprites image.
        /// </summary>
        /// <param name="spriteElements">The sprite elements to save</param>
        /// <param name="imageOutputPath">The location to save the image too.</param>
        /// <returns>etag of generated sprite file</returns>
        public virtual string SaveSpritesImage(List<SpriteElement> spriteElements, string imageOutputPath)
        {
            using (var image = GetCssSpriteImage(spriteElements))
            {
                using (var writer = new MemoryStream())
                {
                    image.Save(writer, ImageFormat.Png);
                    writer.Flush();

                    return _retryableFileWriter.SaveContentsToFile(writer, imageOutputPath);
                }
            }
        }

        /// <summary>
        /// Add the images to the cache so that they are monitored for any changes.
        /// </summary>
        /// <param name="imageOutputPath">Sprite image output path.</param>
        /// <param name="spriteImageUrl">Sprite image url.</param>
        /// <param name="cssOutputPath">Sprite css output path.</param>
        /// <param name="files">The component images for the sprite.</param>
        public virtual void AddFilesToCache(string imageOutputPath, string spriteImageUrl, string cssOutputPath, IEnumerable<ImageFile> files)
        {
            var fileNames = new List<string>
                                {
                                    HostingEnvironment.MapPath(imageOutputPath),
                                    HostingEnvironment.MapPath(cssOutputPath)
                                };

            foreach (var file in files)
            {
                fileNames.Add(HostingEnvironment.MapPath(file.FilePath));
            }

            HttpRuntime.Cache.Insert(
                GetKey(imageOutputPath, spriteImageUrl, cssOutputPath),
                files,
                new CacheDependency(fileNames.ToArray(), System.DateTime.Now),
                Cache.NoAbsoluteExpiration,
                Cache.NoSlidingExpiration,
                CacheItemPriority.High,
                FileRemoved);
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
            var imageOutputPath = GetImageOutputPathFromKey(key);
            var cssOutputPath = GetCssOutputPathFromKey(key);
            var spriteImageUrl = GetSpriteImageUrlFromKey(key);
            var cssFiles = (List<ImageFile>)value;
            switch (reason)
            {
                case CacheItemRemovedReason.DependencyChanged:
                    AddFiles(imageOutputPath, spriteImageUrl, cssOutputPath, cssFiles);
                    break;
                case CacheItemRemovedReason.Underused:
                case CacheItemRemovedReason.Expired:
                    AddFilesToCache(imageOutputPath, spriteImageUrl, cssOutputPath, cssFiles);
                    break;
            }
        }

        /// <summary>
        /// Remove sprite image from cache.
        /// </summary>
        /// <param name="imageOutputPath">Sprite image output path.</param>
        /// <param name="spriteImageUrl">Sprite image url.</param>
        /// <param name="cssOutputPath">Sprite css output path.</param>
        public virtual void RemoveFiles(string imageOutputPath, string spriteImageUrl, string cssOutputPath)
        {
            HttpRuntime.Cache.Remove(GetKey(imageOutputPath, spriteImageUrl, cssOutputPath));
        }

        /// <summary>
        /// Get the cache key to use.
        /// </summary>
        /// <param name="imageOutputPath">Sprite image output path.</param>
        /// <param name="spriteImageUrl">Sprite image url.</param>
        /// <param name="cssOutputPath">Sprite css output path.</param>
        /// <returns></returns>
        public virtual string GetKey(string imageOutputPath, string spriteImageUrl, string cssOutputPath)
        {
            var prefix = CssSpriteCreatorType + "|";
            return prefix + imageOutputPath + "|" + spriteImageUrl + "|" + cssOutputPath;
        }

        /// <summary>
        /// Get the sprite image path from cache key.
        /// </summary>
        /// <param name="key">Cache key.</param>
        /// <returns>Sprite image output path</returns>
        public virtual string GetImageOutputPathFromKey(string key)
        {
            var prefix = CssSpriteCreatorType + "|";
            var tokens = key.Substring(prefix.Length).Split(new char[] { '|' }, StringSplitOptions.None);
            return tokens[0];
        }

        /// <summary>
        /// Get the sprite image url from the cache key.
        /// </summary>
        /// <param name="key">Cache key.</param>
        /// <returns>Sprite image url.</returns>
        public virtual string GetSpriteImageUrlFromKey(string key)
        {
            var prefix = typeof(CssSpriteCreator).ToString() + "|";
            var tokens = key.Substring(prefix.Length).Split(new char[] { '|' }, StringSplitOptions.None);
            return tokens[1];
        }

        /// <summary>
        /// Get the sprite css output path from the cache key.
        /// </summary>
        /// <param name="key">Cache key.</param>
        /// <returns>Sprite css output path.</returns>
        public virtual string GetCssOutputPathFromKey(string key)
        {
            var prefix = typeof(CssSpriteCreator).ToString() + "|";
            var tokens = key.Substring(prefix.Length).Split(new char[] {'|'}, StringSplitOptions.None);
            return tokens[2];
        }

        /// <summary>
        /// Generate the css that denotes the composite sprite part locations and dimensions.
        /// </summary>
        /// <param name="spriteElements">The sprites that make the image up.</param>
        /// <param name="etag">The unique hash for the sprite image.</param>
        /// <param name="cssSpriteImageUrl">The url of the sprite image.</param>
        /// <returns></returns>
        public virtual string GetCssSpriteCss(IEnumerable<SpriteElement> spriteElements, string etag, string cssSpriteImageUrl)
        {
            var cssBuilder = new StringBuilder();
            var currentY = 0;
            foreach (var element in spriteElements)
            {
                cssBuilder.AppendFormat(".{0} {{background-image: url('{1}');background-position: 0px -{2}px;width: {3}px;height: {4}px;}}", element.Name, cssSpriteImageUrl + "?" + etag, currentY, element.Width, element.Height);
                currentY += element.Height + ImagePadding;
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
            var maxWidth = MaxWidth(spriteElements);
            var maxHeight = TotalHeight(spriteElements);

            var sprite = new Bitmap(maxWidth, maxHeight);
            var graphic = Graphics.FromImage(sprite);

            var currentY = 0;
            foreach (var element in spriteElements)
            {
                graphic.DrawImage(element.Image, 0, currentY, element.Width, element.Height);
                currentY += element.Height + ImagePadding;
            }

            return sprite;
        }

        /// <summary>
        /// Get the height of all the sprites added together.
        /// </summary>
        /// <param name="spriteElements">The sprites that make the image up.</param>
        /// <returns>The height if all the sprites added together.</returns>
        public virtual int TotalHeight(IEnumerable<SpriteElement> spriteElements)
        {
            return spriteElements.Sum(x => x.Height + ImagePadding);
        }

        /// <summary>
        /// Get the width of the widest sprite.
        /// </summary>
        /// <param name="spriteElements">The sprites that make the image up.</param>
        /// <returns>The width of the widest sprite.</returns>
        public virtual int MaxWidth(IEnumerable<SpriteElement> spriteElements)
        {
            return spriteElements.Max(x => x.Width);
        }
    }
}
