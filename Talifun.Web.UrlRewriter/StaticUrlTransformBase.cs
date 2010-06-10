using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Caching;
using Intelligencia.UrlRewriter;

namespace Talifun.Web.UrlRewriter
{
    /// <summary>
    /// Base class that does all the necessary duties when it comes to rewriting urls. It caches results and splits the incoming url
    /// into its component parts for easier manipulation.
    /// </summary>
    public abstract class StaticUrlTransformBase : IRewriteTransform
    {
        #region IRewriteTransform Members

        protected const string DefaultDocument = "default.aspx";
        private const string RefreshCacheKey = "RefreshCacheKey";
        private static readonly string[] CacheDependancies = new string[] { RefreshCacheKey };
        private static readonly string[] FileDependancies = null;

        private static readonly TimeSpan SlidingExpiration = new TimeSpan(0, 30, 0);

        static StaticUrlTransformBase()
        {
            RefreshCache();
        }

        public string ApplyTransform(string input)
        {
            List<string> filePath = null;
            string fileName = null;
            string fileExtension = null;
            NameValueCollection queryString = null;
            string bookMark = null;

            if (!TryParseUrl(input, out filePath, out fileName, out fileExtension, out queryString, out bookMark))
            {
                return input;
            }

            var cacheKey = String.Join("/", filePath.ToArray());
            if (!string.IsNullOrEmpty(cacheKey))
            {
                cacheKey = "/" + cacheKey;
            }

            cacheKey += fileName + fileExtension;
            var cachedValue = HttpRuntime.Cache.Get(cacheKey);

            if (cachedValue != null)
            {
                return (string)cachedValue;
            }

            var transformedUrl = RewriteUrl(filePath, fileName, fileExtension, queryString, bookMark);

            HttpRuntime.Cache.Insert(
                cacheKey,
                transformedUrl,
                new CacheDependency(FileDependancies, CacheDependancies),
                System.Web.Caching.Cache.NoAbsoluteExpiration,
                SlidingExpiration,
                System.Web.Caching.CacheItemPriority.Normal,
                null);

            return transformedUrl;
        }

        /// <summary>
        /// Rewrite url
        /// </summary>
        /// <param name="filePath">A list of all directories making up the path of the url</param>
        /// <param name="fileName">The file of the url</param>
        /// <param name="fileExtension">The file extension of the file in the url</param>
        /// <param name="queryString">Querystring of the url</param>
        /// <param name="bookMark">Bookmark of the url</param>
        /// <returns>The string value of the url</returns>
        protected abstract string RewriteUrl(List<string> filePath, string fileName, string fileExtension, NameValueCollection queryString, string bookMark);

        /// <summary>
        /// Converts parts of a url into its string representation.
        /// </summary>
        /// <param name="filePath">A list of all directories making up the path of the url</param>
        /// <param name="fileName">The file of the url</param>
        /// <param name="fileExtension">The file extension of the file in the url</param>
        /// <param name="queryString">Querystring of the url</param>
        /// <param name="bookMark">Bookmark of the url</param>
        /// <returns>The string value of the url</returns>
        protected static string UrlToString(List<string> filePath, string fileName, string fileExtension, NameValueCollection queryString, string bookMark)
        {
            var url = new StringBuilder();

            if (filePath != null)
            {
                if (filePath.Count > 0)
                {
                    url.Append("/");
                }

                url.Append(String.Join("/", filePath.ToArray()));
            }

            url.Append("/");
            if (!string.IsNullOrEmpty(fileName))
            {
                url.Append(fileName);
            }

            if (!string.IsNullOrEmpty(fileExtension))
            {
                url.Append(fileExtension);
            }

            if (queryString != null && queryString.Count > 0)
            {
                url.Append("?");

                var isFirst = true;

                for (var i = 0; i < queryString.Count; i++)
                {
                    var key = queryString.GetKey(i);
                    var values = queryString.GetValues(i);

                    if (values == null) continue;

                    foreach (var value in values)
                    {
                        if (!isFirst)
                        {
                            url.Append("&");
                        }
                        else
                        {
                            isFirst = false;
                        }

                        if (!string.IsNullOrEmpty(key))
                        {
                            url.Append(key);
                            url.Append("=");
                        }

                        url.Append(value);
                    }
                }
            }

            if (!string.IsNullOrEmpty(bookMark))
            {
                url.Append("#" + bookMark);
            }

            return url.ToString();
        }

        /// <summary>
        /// Try parse a url string into its parts.
        /// </summary>
        /// <param name="input">The raw url</param>
        /// <param name="filePath">A list of all directories making up the path of the url</param>
        /// <param name="fileName">The file name of the file in the url</param>
        /// <param name="fileExtension">The file extension of the file in the url</param>
        /// <param name="queryString">Querystring of the url</param>
        /// <param name="bookMark">Bookmark of the url</param>
        /// <returns>True if successful parse; else false</returns>
        protected static bool TryParseUrl(string input, out List<string> filePath, out string fileName, out string fileExtension, out NameValueCollection queryString, out string bookMark)
        {
            var regexForUrl = new Regex(@"^/([^?#/]+/)*([^/?#]*(?=\.)|[^/?#.]*)?([^/?#]*)?(\?)?([^#]*)?(\#)?(.*)$", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Compiled);
            var matchForUrl = regexForUrl.Match(input);

            filePath = null;
            fileName = null;
            fileExtension = null;
            queryString = null;
            bookMark = null;

            if (!matchForUrl.Success) return false;

            filePath = new List<string>();
            foreach (Capture capture in matchForUrl.Groups[1].Captures)
            {
                filePath.Add(capture.Value);
            }

            fileName = matchForUrl.Groups[2].Value;
            fileExtension = matchForUrl.Groups[3].Value;
            queryString = HttpUtility.ParseQueryString(matchForUrl.Groups[5].Value);
            bookMark = matchForUrl.Groups[7].Value;

            return true;
        }

        public abstract string Name { get; }

        public static void RefreshCache()
        {
            HttpRuntime.Cache.Insert(
                RefreshCacheKey,
                System.DateTime.Now,
                null,
                System.Web.Caching.Cache.NoAbsoluteExpiration,
                SlidingExpiration,
                System.Web.Caching.CacheItemPriority.High,
                null);
        }

        #endregion
    }
}

