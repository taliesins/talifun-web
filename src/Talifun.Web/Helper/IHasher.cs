using System.IO;

namespace Talifun.Web
{
    public interface IHasher
    {
        /// <summary>
        /// Calculates a hash for a specified byte range of the file specified by <paramref name="stream" />
        /// </summary>
        /// <param name="stream">A <see cref="FileInfo" /> object specifying the file containing the content for which we're calculating the hash.</param>
        /// <returns>A string containing a hash that can be used in an HTTP ETAG header.</returns>
        string Hash(Stream stream);

        /// <summary>
        /// Calculates a hash for a specified byte range of the file specified by <paramref name="stream" />
        /// </summary>
        /// <param name="stream">A <see cref="FileInfo" /> object specifying the file containing the content for which we're calculating the hash.</param>
        /// <param name="beginRange"></param>
        /// <param name="endRange"></param>
        /// <returns>A string containing a hash that can be used in an HTTP ETAG header.</returns>
        string Hash(Stream stream, int beginRange, int endRange);

        /// <summary>
        /// Calculates a hash for a specified byte range of the file specified by <paramref name="fileInfo" />
        /// </summary>
        /// <param name="fileInfo">A <see cref="FileInfo" /> object specifying the file containing the content for which we're calculating the hash.</param>
        /// <returns>A string containing a hash that can be used in an HTTP ETAG header.</returns>
        string Hash(FileInfo fileInfo);
    }
}