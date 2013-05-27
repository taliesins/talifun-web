using System;
using System.IO;
using System.IO.Compression;
using System.Web.Caching;

namespace Talifun.Web.StaticFile
{
    public class FileEntityResponder : IEntityResponder
    {
        protected static string FileEntityResponderType = typeof(FileEntityResponder).ToString();

        protected readonly ICacheManager CacheManager;
        protected readonly IRetryableFileOpener RetryableFileOpener;
        protected readonly IMimeTyper MimeTyper;
        protected readonly IHasher Hasher;
        protected readonly long MaxFileSizeToServe;
        protected readonly int BufferSize;
        protected readonly FileInfo FileInfo;
        protected readonly MimeSetting MimeSetting;

        public FileEntityResponder(ICacheManager cacheManager, IRetryableFileOpener retryableFileOpener, IMimeTyper mimeTyper, IHasher hasher, long maxFileSizeToServe, int bufferSize, MimeSettingProvider mimeSettingProvider, FileInfo fileInfo)
        {
            CacheManager = cacheManager;
            RetryableFileOpener = retryableFileOpener;
            MimeTyper = mimeTyper;
            Hasher = hasher;
            BufferSize = bufferSize;
            MaxFileSizeToServe = maxFileSizeToServe;
            FileInfo = fileInfo;
            MimeSetting = mimeSettingProvider.GetSetting(fileInfo); 
        }

        /// <summary>
        /// Determine of requested entity can be served.
        /// </summary>
        /// <returns>True if file can be served; false if it can not be.</returns>
        public bool IsAllowedToServeRequestedEntity
        {
            get
            {
                return !(FileInfo.FullName.EndsWith(".asp", StringComparison.InvariantCultureIgnoreCase) ||
                         FileInfo.FullName.EndsWith(".aspx", StringComparison.InvariantCultureIgnoreCase));
            }
        }

        /// <summary>
        /// The transmit entity strategy to use.
        /// </summary>
        /// <param name="fileEntityCacheItem">The cache item.</param>
        /// <returns></returns>
        public ITransmitEntityStrategy GetTransmitEntityStrategy(EntityCacheItem fileEntityCacheItem)
        {
            if (fileEntityCacheItem.EntityData == null)
            {
                //Let IIS send file content with TransmitFile
                return new TransmitEntityStrategyForIIS(fileEntityCacheItem, FileInfo.FullName);
            }
            else
            {
                //We will serve the in memory file
                return new TransmitEntityStrategyForByteArray(fileEntityCacheItem, fileEntityCacheItem.EntityData);
            }
        }

        /// <summary>
        /// Get a fileHanderCacheItem for the requested file.
        /// </summary>
        /// <param name="entityStoredWithCompressionType">The compression type to use for the file.</param>
        /// <param name="fileEntityCacheItem">The fileHandlerCacheItem </param>
        /// <returns>Returns true if a fileHandlerCacheItem can be created; otherwise false.</returns>
        public bool TryGetFileHandlerCacheItem(ResponseCompressionType entityStoredWithCompressionType, out EntityCacheItem fileEntityCacheItem)
        {
            fileEntityCacheItem = null;

            // If the response bytes are already cached, then deliver the bytes directly from cache
            var cacheKey = FileEntityResponderType + ":" + entityStoredWithCompressionType + ":" + FileInfo.FullName;

            var cachedValue = CacheManager.Get<EntityCacheItem>(cacheKey);
            if (cachedValue != null)
            {
                fileEntityCacheItem = cachedValue;
            }
            else
            {
                //File does not exist
                if (!FileInfo.Exists)
                {
                    return false;
                }

                //File too large to send
                if (FileInfo.Length > MaxFileSizeToServe)
                {
                    return false;
                }

                var etag = string.Empty;
                var lastModifiedFileTime = FileInfo.LastWriteTime.ToUniversalTime();
                //When a browser sets the If-Modified-Since field to 13-1-2010 10:30:58, another DateTime instance is created, but this one has a Ticks value of 633989754580000000
                //But the time from the file system is accurate to a tick. So it might be 633989754586086250.
                var lastModified = new DateTime(lastModifiedFileTime.Year, lastModifiedFileTime.Month,
                                                lastModifiedFileTime.Day, lastModifiedFileTime.Hour,
                                                lastModifiedFileTime.Minute, lastModifiedFileTime.Second);
                var contentType = MimeTyper.GetMimeType(FileInfo.Extension);
                var contentLength = FileInfo.Length;

                //ETAG is always calculated from uncompressed entity data
                switch (MimeSetting.EtagMethod)
                {
                    case EtagMethodType.MD5:
                        etag = Hasher.CalculateMd5Etag(FileInfo);
                        break;
                    case EtagMethodType.LastModified:
                        etag = lastModified.ToString();
                        break;
                    default:
                        throw new Exception("Unknown etag method generation");
                }

                fileEntityCacheItem = new EntityCacheItem
                {
                    Etag = etag,
                    LastModified = lastModified,
                    ContentLength = contentLength,
                    ContentType = contentType,
                    CompressionType = ResponseCompressionType.None
                };

                if (MimeSetting.ServeFromMemory
                    && (contentLength <= MimeSetting.MaxMemorySize))
                {
                    // When not compressed, buffer is the size of the file but when compressed, 
                    // initial buffer size is one third of the file size. Assuming, compression 
                    // will give us less than 1/3rd of the size
                    using (var memoryStream = new MemoryStream(
                        entityStoredWithCompressionType == ResponseCompressionType.None
                            ?
                                Convert.ToInt32(FileInfo.Length)
                            :
                                Convert.ToInt32((double)FileInfo.Length / 3)))
                    {
                        GetEntityData(entityStoredWithCompressionType, memoryStream);
                        var entityData = memoryStream.ToArray();
                        var entityDataLength = entityData.LongLength;

                        fileEntityCacheItem.EntityData = entityData;
                        fileEntityCacheItem.ContentLength = entityDataLength;
                        fileEntityCacheItem.CompressionType = entityStoredWithCompressionType;
                    }
                }

                //Put fileHandlerCacheItem into cache with 30 min sliding expiration, also if file changes then remove fileHandlerCacheItem from cache
                CacheManager.Insert(
                    cacheKey,
                    fileEntityCacheItem,
                    new CacheDependency(FileInfo.FullName),
                    Cache.NoAbsoluteExpiration,
                    MimeSetting.MemorySlidingExpiration,
                    CacheItemPriority.BelowNormal,
                    null);
            }

            return true;
        }

        /// <summary>
        /// Read a files contents into a stream
        /// </summary>
        /// <param name="compressionType">The compression type to use.</param>
        /// <param name="stream">The stream to write to.</param>
        private void GetEntityData(ResponseCompressionType compressionType, Stream stream)
        {
            using (var outputStream = (compressionType == ResponseCompressionType.None ? stream : (compressionType == ResponseCompressionType.GZip ? (Stream)new GZipStream(stream, CompressionMode.Compress, true) : (Stream)new DeflateStream(stream, CompressionMode.Compress))))
            {
                // We can compress and cache this file
                using (var fileStream = RetryableFileOpener.OpenFileStream(FileInfo, 5, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var bufferSize = Convert.ToInt32(Math.Min(FileInfo.Length, BufferSize));
                    var buffer = new byte[bufferSize];

                    int bytesRead;
                    while ((bytesRead = fileStream.Read(buffer, 0, bufferSize)) > 0)
                    {
                        outputStream.Write(buffer, 0, bytesRead);
                    }
                }

                outputStream.Flush();
            }
        }

        public bool DoesEntityExists
        {
            get
            {
                return FileInfo.Exists;
            }
        }

        public bool IsEntityLargerThanMaxFileSize
        {
            get
            {
                return FileInfo.Length > MaxFileSizeToServe;
            }
        }

        public bool IsCompressable
        {
            get
            {
                return MimeSetting.Compress;
            }
        }

        public TimeSpan Expires
        {
            get
            {
                return MimeSetting.Expires;
            }
        }

        public string UrlEtagQuerystringName
        {
            get
            {
                return MimeSetting.UrlEtagQuerystringName;
            }
        }

        public UrlEtagHandlingMethodType UrlEtagHandlingMethod
        {
            get
            {
                return MimeSetting.UrlEtagHandlingMethod;
            }
        }
    }
}
