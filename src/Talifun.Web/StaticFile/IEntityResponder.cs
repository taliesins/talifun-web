using System;

namespace Talifun.Web.StaticFile
{
    public interface IEntityResponder
    {
        bool IsAllowedToServeRequestedEntity { get; }
        ITransmitEntityStrategy GetTransmitEntityStrategy(EntityCacheItem fileEntityCacheItem);
        bool TryGetFileHandlerCacheItem(ResponseCompressionType entityStoredWithCompressionType,
                                        out EntityCacheItem fileEntityCacheItem);
        bool DoesEntityExists { get; }
        bool IsEntityLargerThanMaxFileSize { get; }
        bool IsCompressable { get; }
        TimeSpan Expires { get; }
        UrlEtagHandlingMethodType UrlEtagHandlingMethod { get; }
        string UrlEtagQuerystringName { get; }
    }
}
