using System;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Web.Caching;

namespace Talifun.Web.StaticFile
{
    public class EmbeddedResourceEntityResponder : IEntityResponder
    {
        protected static string EmbeddedResourceEntityResponderType = typeof(EmbeddedResourceEntityResponder).ToString();

        protected readonly ICacheManager CacheManager;
        protected readonly IMimeTyper MimeTyper;
        protected readonly IHasher Hasher;
        protected readonly IEmbeddedResourceLoader EmbeddedResourceLoader;
        private readonly Assembly _assembly;
        private readonly string _resourcePath;
        protected readonly MimeSetting MimeSetting;
        protected readonly int BufferSize;
        protected readonly int ResourceSize = 0;
        protected readonly long MaxFileSizeToServe;
        protected readonly string ResourcePath;
        protected readonly string ResourceExtension;
        protected readonly DateTime ResourceLastModified;

        public EmbeddedResourceEntityResponder(ICacheManager cacheManager, IMimeTyper mimeTyper, IHasher hasher, IEmbeddedResourceLoader embeddedResourceLoader, long maxFileSizeToServe, int bufferSize, MimeSettingProvider mimeSettingProvider, Assembly assembly, string resourcePath)
        {
            _assembly = assembly;
            _resourcePath = resourcePath;

            ResourcePath = string.Format("{0}.{1}", assembly.GetName().Name, resourcePath.Replace("/", "."));
            ResourceExtension = Path.GetExtension(ResourcePath);

            var version = _assembly.GetName().Version;
            ResourceLastModified = new DateTime(2000, 1, 1)
                .AddDays(version.Build)
                .AddSeconds(version.Revision * 2)
                .ToUniversalTime();

            using (var bodyStream = assembly.GetManifestResourceStream(ResourcePath))
            {
                if (bodyStream != null)
                {
                    ResourceSize = (int)bodyStream.Length;
                }
            }

            MimeSetting = mimeSettingProvider.GetSetting(ResourceExtension.ToLower()); 
            BufferSize = bufferSize;
            MaxFileSizeToServe = maxFileSizeToServe;
            Hasher = hasher;
            EmbeddedResourceLoader = embeddedResourceLoader;
            MimeTyper = mimeTyper;
            CacheManager = cacheManager;
        }

        public bool IsAllowedToServeRequestedEntity
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// The transmit entity strategy to use.
        /// </summary>
        /// <param name="entityCacheItem">The cache item.</param>
        /// <returns></returns>
        public ITransmitEntityStrategy GetTransmitEntityStrategy(EntityCacheItem entityCacheItem)
        {
            if (entityCacheItem.EntityData == null)
            {
                //We will serve the embedded resource
                return new TransmitEntityStrategyForEmbeddedResource(EmbeddedResourceLoader, entityCacheItem, _assembly, _resourcePath, BufferSize);
            }
            else
            {
                //We will serve from the in memory copy
                return new TransmitEntityStrategyForByteArray(entityCacheItem, entityCacheItem.EntityData);
            }
        }

        /// <summary>
        /// Get a fileHanderCacheItem for the requested file.
        /// </summary>
        /// <param name="entityStoredWithCompressionType">The compression type to use for the file.</param>
        /// <param name="entityCacheItem">The fileHandlerCacheItem </param>
        /// <returns>Returns true if a fileHandlerCacheItem can be created; otherwise false.</returns>
        public bool TryGetFileHandlerCacheItem(ResponseCompressionType entityStoredWithCompressionType, out EntityCacheItem entityCacheItem)
        {
            entityCacheItem = null;

            // If the response bytes are already cached, then deliver the bytes directly from cache
            var cacheKey = EmbeddedResourceEntityResponderType + ":" + entityStoredWithCompressionType + ":" + ResourcePath;

            var cachedValue = CacheManager.Get<EntityCacheItem>(cacheKey);
            if (cachedValue != null)
            {
                entityCacheItem = cachedValue;
            }
            else
            {
                //File does not exist
                if (!DoesEntityExists)
                {
                    return false;
                }

                //File too large to send
                if (ResourceSize > MaxFileSizeToServe)
                {
                    return false;
                }

                var etag = string.Empty;
                var lastModifiedFileTime = ResourceLastModified;
                //When a browser sets the If-Modified-Since field to 13-1-2010 10:30:58, another DateTime instance is created, but this one has a Ticks value of 633989754580000000
                //But the time from the file system is accurate to a tick. So it might be 633989754586086250.
                var lastModified = new DateTime(lastModifiedFileTime.Year, lastModifiedFileTime.Month,
                                                lastModifiedFileTime.Day, lastModifiedFileTime.Hour,
                                                lastModifiedFileTime.Minute, lastModifiedFileTime.Second);
                var contentType = MimeTyper.GetMimeType(ResourceExtension);
                var contentLength = ResourceSize;

                //ETAG is always calculated from uncompressed entity data
                switch (MimeSetting.EtagMethod)
                {
                    case EtagMethodType.MD5:
                        using (var resourceStream = _assembly.GetManifestResourceStream(ResourcePath))
                        {
                            etag = Hasher.Hash(resourceStream);
                        }
                        break;
                    case EtagMethodType.LastModified:
                        etag = lastModified.ToString("r");
                        break;
                    default:
                        throw new Exception("Unknown etag method generation");
                }

                entityCacheItem = new EntityCacheItem
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
                                Convert.ToInt32(ResourceSize)
                            :
                                Convert.ToInt32((double)ResourceSize / 3)))
                    {
                        GetEntityData(entityStoredWithCompressionType, memoryStream);
                        var entityData = memoryStream.ToArray();
                        var entityDataLength = entityData.LongLength;

                        entityCacheItem.EntityData = entityData;
                        entityCacheItem.ContentLength = entityDataLength;
                        entityCacheItem.CompressionType = entityStoredWithCompressionType;
                    }
                }

                //Put fileHandlerCacheItem into cache with 30 min sliding expiration, also if file changes then remove fileHandlerCacheItem from cache
                CacheManager.Insert(
                    cacheKey,
                    entityCacheItem,
                    null,
                    Cache.NoAbsoluteExpiration,
                    MimeSetting.MemorySlidingExpiration,
                    CacheItemPriority.BelowNormal,
                    null);
            }

            return true;
        }

        public bool DoesEntityExists
        {
            get
            {
                return ResourceSize > 0;
            }
        }

        public bool IsEntityLargerThanMaxFileSize
        {
            get { return ResourceSize > MaxFileSizeToServe; }
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
                using (var resourceStream = _assembly.GetManifestResourceStream(ResourcePath))
                {
                    var bufferSize = Convert.ToInt32(Math.Min(ResourceSize, BufferSize));
                    var buffer = new byte[bufferSize];

                    int bytesRead;
                    while ((bytesRead = resourceStream.Read(buffer, 0, bufferSize)) > 0)
                    {
                        outputStream.Write(buffer, 0, bytesRead);
                    }
                }

                outputStream.Flush();
            }
        }
    }
}
