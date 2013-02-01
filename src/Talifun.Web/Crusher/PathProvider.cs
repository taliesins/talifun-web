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

        public PathProvider(string applicationPath, string physicalApplicationPath)
        {
            ApplicationPath = applicationPath;
            PhysicalApplicationPath = physicalApplicationPath;
        }

        private static string GetPhysicalApplicationPath()
        {
            return !string.IsNullOrEmpty(HostingEnvironment.ApplicationPhysicalPath) ? HostingEnvironment.ApplicationPhysicalPath : Environment.CurrentDirectory;
        }

        public virtual string MapPath(Uri uri)
        {
            return MapPath(uri.OriginalString);
        }

        public virtual string MapPath(string uri)
        {
            var queryStringPosition = uri.IndexOf('?');

            if (queryStringPosition > -1)
            {
                uri = uri.Substring(0, queryStringPosition);
            }

            if (HttpContext.Current == null)
            {
                uri = uri.TrimStart('~').TrimStart('/', '\\');
                var applicationUri = GetApplicationAbsoluteUri().ToString();
                return Path.Combine(applicationUri, uri).Replace("/", "\\");
            }

            return HostingEnvironment.MapPath(uri);
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
            if (HttpContext.Current == null)
            {
                return ToAbsolute(virtualPath, GetApplicationAbsoluteUri().ToString());
            }
            return VirtualPathUtility.ToAbsolute(virtualPath);
        }

        public virtual string ToAbsolute(string virtualPath, string applicationPath)
        {
            return VirtualPathUtility.ToAbsolute(virtualPath, applicationPath);
        }

        /// <summary>
        /// Get directory uri for uri
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Get the absolute directory for the uri
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public virtual Uri GetAbsoluteUriDirectory(Uri uri)
        {
            var uriDirectory = GetUriDirectory(uri);
            uriDirectory = !uriDirectory.IsAbsoluteUri
                                       ? new Uri(MapPath(uriDirectory), UriKind.Absolute)
                                       : uriDirectory;

            return uriDirectory;
        }

        /// <summary>
        /// Get the absolute directory for the string uri
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public virtual Uri GetAbsoluteUriDirectory(string uri)
        {
            var absoluteFileUri = new Uri(uri, UriKind.RelativeOrAbsolute);

            return GetAbsoluteUriDirectory(absoluteFileUri);
        }

        /// <summary>
        /// Get the relative path to application path
        /// </summary>
        /// <param name="filePath">path relative to application path</param>
        /// <returns></returns>
        public virtual Uri ToRelative(string filePath)
        {
            var absolutePathUri = new Uri(filePath, UriKind.RelativeOrAbsolute);
            var applicationPathAbsoluteUri = GetApplicationAbsoluteUri();
            var relativeUri = new Uri("~/" + applicationPathAbsoluteUri.MakeRelativeUri(absolutePathUri), UriKind.Relative);
            return relativeUri;
        }

        /// <summary>
        /// Get the absolute application path
        /// </summary>
        /// <returns></returns>
        public Uri GetApplicationAbsoluteUri()
        {
            var rootUri = new Uri(ApplicationPath, UriKind.RelativeOrAbsolute);
            if (!rootUri.IsAbsoluteUri)
            {
                var path = Path.Combine(PhysicalApplicationPath, rootUri.ToString().TrimStart('~').TrimStart('/', '\\'));
                rootUri = new Uri(path, UriKind.Absolute);
            }
            return rootUri;
        }
    }
}
