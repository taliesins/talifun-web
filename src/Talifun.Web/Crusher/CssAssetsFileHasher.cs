using System;
using System.IO;
using System.Web;

namespace Talifun.Web.Crusher
{
    public class CssAssetsFileHasher : ICssAssetsFileHasher
    {
        protected readonly string HashQueryStringKeyName;
        protected readonly IHasher Hasher;
        protected readonly IPathProvider PathProvider;

        public CssAssetsFileHasher(string hashQueryStringKeyName, IHasher hasher, IPathProvider pathProvider)
        {
            HashQueryStringKeyName = hashQueryStringKeyName;
            Hasher = hasher;
            PathProvider = pathProvider;
        }

        public virtual Uri AppendFileHash(Uri cssRootPath, Uri url)
        {
            if (url.IsAbsoluteUri) return url;

            var fileInfo = new FileInfo(new Uri(PathProvider.MapPath(cssRootPath, url)).LocalPath);

            if (!fileInfo.Exists)
            {
                return url;
            }

            var hash = Hasher.CalculateMd5Etag(fileInfo);
            url = AppendQueryStringPairValue(url, HashQueryStringKeyName, hash);
            return url;
        }

        /// <summary>
        /// Append a query string pair value to a url.
        /// </summary>
        /// <param name="url">The url to add query string pair value value to.</param>
        /// <param name="key">The key to use.</param>
        /// <param name="value">The value to use.</param>
        /// <returns>The uri with the querstring appended.</returns>
        /// <remarks>This will work on relative uris.</remarks>
        public virtual Uri AppendQueryStringPairValue(Uri url, string key, string value)
        {
            var path = url.OriginalString;
            var queryString = string.Empty;

            var queryStringPosition = url.OriginalString.IndexOf('?');

            if (queryStringPosition > -1)
            {
                path = url.OriginalString.Substring(0, queryStringPosition);
                queryString = url.OriginalString.Substring(queryStringPosition);
            }

            var querystring = HttpUtility.ParseQueryString(queryString);

            querystring.Add(key, value);

            var querystringwithAppendedValue = querystring.ToString();
            if (!string.IsNullOrEmpty(querystringwithAppendedValue))
            {
                querystringwithAppendedValue = "?" + querystringwithAppendedValue;
            }

            return new Uri(path + querystringwithAppendedValue, UriKind.RelativeOrAbsolute);
        }
    }
}