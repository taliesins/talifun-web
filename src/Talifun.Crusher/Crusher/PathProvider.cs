using System;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Hosting;

namespace Talifun.Crusher.Crusher
{
    public class PathProvider : IPathProvider
    {
        protected string ApplicationPath;
        protected string PhysicalApplicationPath;

        public PathProvider()
            : this(GetAppDomainAppVirtualPath(), GetPhysicalApplicationPath())
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

        private static string GetAppDomainAppVirtualPath()
        {
            return HttpRuntime.AppDomainAppVirtualPath??"/";
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

            if (HttpContext.Current == null || string.IsNullOrEmpty(HostingEnvironment.ApplicationPhysicalPath))
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
                    resolvedUrl = new Uri(MapPath(url)).LocalPath;
                }

                return Path.GetFullPath(resolvedUrl);
            }

            return urlUri.LocalPath;
        }

        public virtual string ToAbsolute(string virtualPath)
        {
            var uri = new Uri(virtualPath, UriKind.RelativeOrAbsolute);
            if (uri.IsAbsoluteUri)
            {
                return uri.ToString();
            }

            if (HttpContext.Current == null)
            {
                return ToAbsolute(virtualPath, GetApplicationAbsoluteUri().ToString());
            }
            return VirtualPathUtility.ToAbsolute(virtualPath);
        }

        public virtual string ToAbsolute(string virtualPath, string applicationPath)
        {
            var uri = new Uri(virtualPath, UriKind.RelativeOrAbsolute);
            if (uri.IsAbsoluteUri)
            {
                return uri.ToString();
            }

            if (HttpContext.Current == null || string.IsNullOrEmpty(HostingEnvironment.ApplicationPhysicalPath))
            {
                var path = Path.Combine(applicationPath, virtualPath.TrimStart('~').TrimStart('/', '\\'));
                return new Uri(path, UriKind.Absolute).AbsolutePath;
            }

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

            if (path.EndsWith("/"))
            {
                return new Uri(path, UriKind.RelativeOrAbsolute);
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
        /// <param name="uri">path relative to application path</param>
        /// <returns></returns>
        public virtual Uri ToRelative(string uri)
        {
            var absolutePathUri = new Uri(uri, UriKind.RelativeOrAbsolute);
            if (!absolutePathUri.IsAbsoluteUri)
            {
                return absolutePathUri;
            }

            var applicationPathAbsoluteUri = GetApplicationAbsoluteUri();
            var relativeUri = new Uri("~/" + applicationPathAbsoluteUri.MakeRelativeUri(absolutePathUri), UriKind.Relative);
            return relativeUri;
        }

        /// <summary>
        /// Get the relative uri for a file
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public virtual Uri ToRelative(FileInfo file)
        {
            var physicalApplicationPath = GetPhysicalApplicationPath();
            var physicalFilePath = file.FullName;

            if (!physicalFilePath.StartsWith(physicalApplicationPath))
            {
                throw new Exception("Can't convert to relative as file is not under physical application path");
            }

            var relativeUrl = physicalFilePath.Substring(physicalApplicationPath.Length).Replace('\\', '/');

            return ToRelative(relativeUrl);
        }

        /// <summary>
        /// Get the relative uri for a file
        /// </summary>
        /// <param name="rootPathUri"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        public Uri MakeRelativeUri(Uri rootPathUri, FileInfo file)
        {
            var fileUri = new Uri(file.FullName);

            if (!rootPathUri.IsAbsoluteUri)
            {
                rootPathUri = new Uri(MapPath(rootPathUri));
            }

            return rootPathUri.MakeRelativeUri(fileUri);
        }

        /// <summary>
        /// Get the absolute application path
        /// </summary>
        /// <returns></returns>
        private Uri GetApplicationAbsoluteUri()
        {
            var rootUri = new Uri(ApplicationPath, UriKind.RelativeOrAbsolute);
            if (!rootUri.IsAbsoluteUri)
            {
                var path = Path.Combine(PhysicalApplicationPath, rootUri.ToString().TrimStart('~').TrimStart('/', '\\'));
                if (!(path.EndsWith("/") || path.EndsWith("\\")))
                {
                    path += "\\";
                }
                
                rootUri = new Uri(path, UriKind.Absolute);
            }
            return rootUri;
        }

        public string ResolveUrl(string relativeUrl)
        {
            if (relativeUrl == null) throw new ArgumentNullException("relativeUrl");

            if (relativeUrl.Length == 0 || relativeUrl[0] == '/' || relativeUrl[0] == '\\')
                return relativeUrl;

            var idxOfScheme = relativeUrl.IndexOf(@"://", StringComparison.Ordinal);
            if (idxOfScheme != -1)
            {
                var idxOfQM = relativeUrl.IndexOf('?');
                if (idxOfQM == -1 || idxOfQM > idxOfScheme) return relativeUrl;
            }

            var sbUrl = new StringBuilder();
            sbUrl.Append(ApplicationPath);
            if (sbUrl.Length == 0 || sbUrl[sbUrl.Length - 1] != '/') sbUrl.Append('/');

            // found question mark already? query string, do not touch!
            var foundQM = false;
            bool foundSlash; // the latest char was a slash?
            if (relativeUrl.Length > 1
                && relativeUrl[0] == '~'
                && (relativeUrl[1] == '/' || relativeUrl[1] == '\\'))
            {
                relativeUrl = relativeUrl.Substring(2);
                foundSlash = true;
            }
            else
            {
                foundSlash = false;
            }
            foreach (var c in relativeUrl)
            {
                if (!foundQM)
                {
                    if (c == '?')
                    {
                        foundQM = true;
                    }
                    else
                    {
                        if (c == '/' || c == '\\')
                        {
                            if (foundSlash)
                            {
                                continue;
                            }
                            sbUrl.Append('/');
                            foundSlash = true;
                            continue;
                        }
                        else if (foundSlash) foundSlash = false;
                    }
                }
                sbUrl.Append(c);
            }

            return sbUrl.ToString();
        }
    }
}
