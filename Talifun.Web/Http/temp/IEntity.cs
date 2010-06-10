using System;

namespace Talifun.Web
{
    public interface IEntity
    {
        long ContentLength { get; }
        string ContentType { get; }
        string Etag { get; }
        DateTime LastModified { get; }
        ResponseCompressionType CompressionType { get; }
    }
}
