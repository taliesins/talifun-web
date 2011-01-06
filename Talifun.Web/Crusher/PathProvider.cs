using System;
using System.IO;
using System.Web;
using System.Web.Hosting;

namespace Talifun.Web.Crusher
{
    public class PathProvider : IPathProvider
    {
        public string MapPath(Uri url)
        {
            return MapPath(url.OriginalString);
        }

        public string MapPath(string url)
        {
            var queryStringPosition = url.IndexOf('?');

            if (queryStringPosition > -1)
            {
                url = url.Substring(0, queryStringPosition);
            }

            if (HttpContext.Current == null)
            {
                url = url.Replace("/", "\\").TrimStart('~').TrimStart('\\');
                return @"C:\" + url.Replace("/", "\\");
            }
            return HostingEnvironment.MapPath(url);
        }

        public virtual string MapPath(Uri rootPath, Uri url)
        {
            return MapPath(rootPath, url.OriginalString);
        }

        public virtual string MapPath(Uri rootPath, string url)
        {
            var queryStringPosition = url.IndexOf('?');

            if (queryStringPosition > -1)
            {
                url = url.Substring(0, queryStringPosition);
            }

            var urlUri = new Uri(url, UriKind.RelativeOrAbsolute);

            if (!urlUri.IsAbsoluteUri)
            {
                var resolvedUrl = string.Empty;
                if (!url.StartsWith("/"))
                {
                    var resolvedSourcePath = new Uri(rootPath, urlUri);
                    resolvedUrl = resolvedSourcePath.LocalPath;
                }
                else
                {
                    resolvedUrl = MapPath(url);
                }

                return Path.GetFullPath(resolvedUrl);
            }

            return urlUri.LocalPath;
        }
    }
}
