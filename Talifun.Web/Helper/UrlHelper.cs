using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Talifun.Web
{
    public static class UrlHelper
    {
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
        public static bool TryParseUrl(string input, out List<string> filePath, out string fileName, out string fileExtension, out NameValueCollection queryString, out string bookMark)
        {
            var regexForUrl = new Regex(@"^/([^?#/]+/)*([^/?#]*(?=\.)|[^/?#.]*)?([^/?#]*)?(\?)?([^#]*)?(\#)?(.*)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
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

        /// <summary>
        /// Converts parts of a url into its string representation.
        /// </summary>
        /// <param name="filePath">A list of all directories making up the path of the url</param>
        /// <param name="fileName">The file of the url</param>
        /// <param name="fileExtension">The file extension of the file in the url</param>
        /// <param name="queryString">Querystring of the url</param>
        /// <param name="bookMark">Bookmark of the url</param>
        /// <returns>The string value of the url</returns>
        public static string UrlToString(List<string> filePath, string fileName, string fileExtension, NameValueCollection queryString, string bookMark)
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
    }
}
