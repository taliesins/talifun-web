using System;
using System.IO;
using System.Security.Cryptography;

namespace Talifun.Web
{
    public static class HashHelper
    {
        /// <summary>
        /// Calculates an ETAG MD5 hash for a specified byte range of the file specified by <paramref name="stream" />
        /// </summary>
        /// <param name="stream">A <see cref="FileInfo" /> object specifying the file containing the content for which we're calculating the hash.</param>
        /// <returns>A string containing an MD5 hash that can be used in an HTTP ETAG header.</returns>
        public static string CalculateMd5Etag(Stream stream)
        {
            if (stream.CanSeek)
            {
                stream.Seek(0, SeekOrigin.Begin);
            }
            // Now that we have a byte array we can ask the CSP to hash it
            var md5 = new MD5CryptoServiceProvider();
            var hashBytes = md5.ComputeHash(stream);
            return Convert.ToBase64String(hashBytes);
        }

        /// <summary>
        /// Calculates an ETAG MD5 hash for a specified byte range of the file specified by <paramref name="stream" />
        /// </summary>
        /// <param name="stream">A <see cref="FileInfo" /> object specifying the file containing the content for which we're calculating the hash.</param>
        /// <param name="beginRange"></param>
        /// <param name="endRange"></param>
        /// <returns>A string containing an MD5 hash that can be used in an HTTP ETAG header.</returns>
        public static string CalculateMd5Etag(Stream stream, int beginRange, int endRange)
        {
            var data = new byte[endRange - beginRange];
            stream.Read(data, beginRange, endRange);

            // Now that we have a byte array we can ask the CSP to hash it
            var md5 = new MD5CryptoServiceProvider();
            var hashBytes = md5.ComputeHash(data);
            return Convert.ToBase64String(hashBytes);
        }

        /// <summary>
        /// Calculates an ETAG MD5 hash for a specified byte range of the file specified by <paramref name="fileInfo" />
        /// </summary>
        /// <param name="fileInfo">A <see cref="FileInfo" /> object specifying the file containing the content for which we're calculating the hash.</param>
        /// <returns>A string containing an MD5 hash that can be used in an HTTP ETAG header.</returns>
        public static string CalculateMd5Etag(FileInfo fileInfo)
        {
            using (var stream = FileHelper.OpenFileStream(fileInfo, 5, FileMode.Open , FileAccess.Read, FileShare.Read))
            {
                // Now that we have a byte array we can ask the CSP to hash it
                var md5 = new MD5CryptoServiceProvider();
                var hashBytes = md5.ComputeHash(stream);
                return Convert.ToBase64String(hashBytes);
            }
        }
    }
}