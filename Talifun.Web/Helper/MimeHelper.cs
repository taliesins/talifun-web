using System;
using System.Web;
using System.Web.Caching;

namespace Talifun.Web
{
    public static class MimeHelper
    {
        private static readonly TimeSpan MimeTypeSlidingExpiration = new TimeSpan(24, 0, 0);

        /// <summary>
        /// Get the mime type for a file based on its extension
        /// </summary>
        /// <param name="extension">The extension of the file</param>
        /// <returns>Mime type of a file</returns>
        public static string GetMimeType(string extension)
        {
            extension = extension.ToLowerInvariant();

            var cacheKey = GetKey(extension);
            var cachedValue = HttpRuntime.Cache.Get(cacheKey);
            if (cachedValue != null)
            {
                return (string)cachedValue;
            }

            var mime = "application/octetstream";

            var rk = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(extension);
            if (rk != null && rk.GetValue("Content Type") != null)
            {
                mime = rk.GetValue("Content Type").ToString();
            }

            HttpRuntime.Cache.Insert(
                cacheKey,
                mime,
                null,
                Cache.NoAbsoluteExpiration,
                MimeTypeSlidingExpiration,
                CacheItemPriority.Normal,
                null);

            return mime;
        }

        private static string GetKey(string extension)
        {
            var prefix = typeof(MimeHelper).ToString() + "|";
            return prefix + extension;
        }
    }
}
