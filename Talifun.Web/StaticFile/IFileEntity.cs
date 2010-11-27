using System;

namespace Talifun.Web.StaticFile
{
    public interface IFileEntity
    {
        bool IsAllowedToServeRequestedEntity { get; }
        ITransmitEntityStrategy GetTransmitEntityStrategy(FileEntityCacheItem fileEntityCacheItem);
        bool TryGetFileHandlerCacheItem(ResponseCompressionType entityStoredWithCompressionType,
                                        out FileEntityCacheItem fileEntityCacheItem);
        bool DoesEntityExists { get; }
        bool IsEntityLargerThanMaxFileSize { get; }
        bool IsCompressable { get; }
        TimeSpan Expires { get; }
    }
}
