using System;
using System.Web.Caching;

namespace Talifun.Web
{
    public class MimeTyper : IMimeTyper
    {
        protected readonly ICacheManager CacheManger;
        protected TimeSpan MimeTypeSlidingExpiration = new TimeSpan(24, 0, 0);
        protected static string MimeTyperType = typeof(MimeTyper).ToString();

        public MimeTyper(ICacheManager cacheManger)
        {
            CacheManger = cacheManger;
        }

        /// <summary>
        /// Get the mime type for a file based on its extension
        /// </summary>
        /// <param name="extension">The extension of the file</param>
        /// <returns>Mime type of a file</returns>
        public string GetMimeType(string extension)
        {
            extension = extension.ToLowerInvariant();

            var cacheKey = GetKey(extension);
            var cachedValue = CacheManger.Get<string>(cacheKey);
            if (cachedValue != null)
            {
                return cachedValue;
            }

            var mime = "application/octetstream";

            var rk = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(extension);
            if (rk != null && rk.GetValue("Content Type") != null)
            {
                mime = rk.GetValue("Content Type").ToString();
            }

            CacheManger.Insert(
                cacheKey,
                mime,
                null,
                Cache.NoAbsoluteExpiration,
                MimeTypeSlidingExpiration,
                CacheItemPriority.Normal,
                null);

            return mime;
        }

        private string GetKey(string extension)
        {
            var prefix = MimeTyperType + "|";
            return prefix + extension;
        }
    }
}
