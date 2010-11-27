using System;

namespace Talifun.Web.StaticFile
{
    public class FileEntityCacheItem : IEntity
    {
        public byte[] EntityData { get; set; }
        public long ContentLength { get; set;}
        public string ContentType { get; set; }
        public string Etag { get; set; }
        public DateTime LastModified { get; set; }
        public ResponseCompressionType CompressionType { get; set; }
    }
}