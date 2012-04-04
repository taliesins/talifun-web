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
    }
}
