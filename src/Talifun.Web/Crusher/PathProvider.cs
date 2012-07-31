using System;
using System.IO;
using System.Web;
using System.Web.Hosting;

namespace Talifun.Web.Crusher
{
    public class PathProvider : IPathProvider
    {
        protected string ApplicationPath;
        protected string PhysicalApplicationPath;

        public PathProvider()
            : this("/", GetPhysicalApplicationPath())
        {
        }

        private static string GetPhysicalApplicationPath()
        {
            return !string.IsNullOrEmpty(HostingEnvironment.ApplicationPhysicalPath) ? HostingEnvironment.ApplicationPhysicalPath : Environment.CurrentDirectory;
        }

        public PathProvider(string applicationPath, string physicalApplicationPath)
        {
            ApplicationPath = applicationPath;
            PhysicalApplicationPath = physicalApplicationPath;
        }

        public virtual string MapPath(Uri url)
        {
            return MapPath(url.OriginalString);
        }

        public virtual string MapPath(string url)
        {
            var queryStringPosition = url.IndexOf('?');

            if (queryStringPosition > -1)
            {
                url = url.Substring(0, queryStringPosition);
            }

            if (HttpContext.Current == null)
            {
                url = url.Replace("/", "\\").TrimStart('~').TrimStart('\\');
                return Path.Combine(PhysicalApplicationPath, url.Replace("/", "\\"));
            }

            return HostingEnvironment.MapPath(url);
        }

        public virtual string MapPath(Uri rootPath, Uri url)
        {
            return MapPath(rootPath, url.OriginalString);
        }

        public virtual string MapPath(Uri rootUri, string url)
        {
            rootUri = !rootUri.IsAbsoluteUri ? new Uri(MapPath(rootUri)) : rootUri;
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
                    var resolvedSourcePath = new Uri(rootUri, urlUri);
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

        public virtual string ToAbsolute(string virtualPath)
        {
            try
            {
                return VirtualPathUtility.ToAbsolute(virtualPath);
            }
            catch (Exception)
            {
                if (HttpContext.Current == null)
                {
                    return ToAbsolute(virtualPath, ApplicationPath);
                }
                throw;
            }
        }

        public virtual string ToAbsolute(string virtualPath, string applicationPath)
        {
            return VirtualPathUtility.ToAbsolute(virtualPath, applicationPath);
        }

        public virtual Uri GetUriDirectory(Uri uri)
        {
            var path = uri.OriginalString;

            var queryStringPosition = path.IndexOf('?');

            if (queryStringPosition > -1)
            {
                path = path.Substring(0, queryStringPosition);
            }

            var startIndex = path.Length - Path.GetFileName(path).Length;

            path = path.Remove(startIndex);

            return new Uri(path, UriKind.RelativeOrAbsolute);
        }

        public virtual Uri GetRootPathUri(Uri rootUri)
        {
            var cssRootPathUri = GetUriDirectory(rootUri);
            cssRootPathUri = !rootUri.IsAbsoluteUri
                                       ? new Uri(MapPath(rootUri))
                                       : cssRootPathUri;

            return cssRootPathUri;
        }

        public virtual Uri GetRelativeRootUri(string filePath)
        {
            var cssFilePath = ToAbsolute(filePath);

            var relativeRootUri = GetUriDirectory(new Uri(cssFilePath, UriKind.RelativeOrAbsolute));
            relativeRootUri = !relativeRootUri.IsAbsoluteUri
                                  ? new Uri(MapPath(relativeRootUri))
                                  : relativeRootUri;

            return relativeRootUri;
        }


        public virtual Uri ToRelative(string filePath)
        {
            var absolutePathUri = new Uri(filePath);
            var rootUri = new Uri(PhysicalApplicationPath);

            var relativeUri = new Uri("~/" + rootUri.MakeRelativeUri(absolutePathUri), UriKind.Relative);

            return relativeUri;
        }
    }
}
