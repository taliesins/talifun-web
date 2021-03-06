﻿using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Caching;
using System.Web.Hosting;
using Talifun.Crusher.Configuration.Css;
using Talifun.Crusher.Configuration.Js;
using Talifun.Crusher.Configuration.Sprites;
using Talifun.Web;
using Talifun.Web.Helper;

namespace Talifun.Crusher.Configuration
{
    public class CrusherHelper
    {
        protected readonly ICacheManager CacheManager;
        protected readonly string QuerystringKeyName;
        protected readonly CssGroupElementCollection CssGroups;
        protected readonly JsGroupElementCollection JsGroups;
        protected readonly CssSpriteGroupElementCollection CssSpriteGroups;

        protected readonly IRetryableFileOpener RetryableFileOpener;
        protected readonly IHasher Hasher;
        protected readonly IAmdModule AmdModule;
        private static string CrusherHelperType = typeof(CrusherHelper).ToString();
        private static string CssType = "css";
        private static string JsType = "js";
        private static string SpriteImageType = "spriteImage";
        private static string SpriteCssType = "spriteCss";

        private CrusherHelper()
        {
            CacheManager = new HttpCacheManager();
            QuerystringKeyName = CurrentCrusherConfiguration.Current.QuerystringKeyName;
            CssGroups = CurrentCrusherConfiguration.Current.CssGroups;
            JsGroups = CurrentCrusherConfiguration.Current.JsGroups;
            CssSpriteGroups = CurrentCrusherConfiguration.Current.CssSpriteGroups;
            RetryableFileOpener = new RetryableFileOpener();
            Hasher = new Md5Hasher(RetryableFileOpener);
            AmdModule = new AmdModule(RetryableFileOpener);
        }

        private static CrusherHelper Instance
        {
            get
            {
                return Nested.Instance;
            }
        }

        private class Nested
        {
            // Explicit static constructor to tell C# compiler
            // not to mark type as beforefieldinit
            static Nested()
            {
            }

            internal static readonly CrusherHelper Instance = new CrusherHelper();
        }

        public static string Css(string groupName)
        {
            return Instance.CssInternal(groupName);
        }

        public static string Js(string groupName)
        {
            return Instance.JsInternal(groupName);
        }

        public static string CssSprite(string groupName)
        {
            return Instance.CssSpriteInternal(groupName);
        }

        public string CssSpriteInternal(string groupName)
        {
            var cssSpriteGroup = CssSpriteGroups[groupName];
            if (cssSpriteGroup == null)
            {
                throw new ConfigurationErrorsException(string.Format("CssSpriteGroup \"{0}\" does not exists!", groupName));
            }

            var outputFilePath = cssSpriteGroup.CssOutputFilePath;
            var scriptLinks = string.Empty;
            var cacheKey = GetKey(SpriteCssType, outputFilePath);

            var cachedValue = CacheManager.Get<string>(cacheKey);
            if (cachedValue != null)
            {
                scriptLinks = cachedValue;
            }
            else
            {
                var fileInfo = new FileInfo(new Uri(MapPath(outputFilePath)).LocalPath);
                var etag = Hasher.Hash(fileInfo);
                var url = cssSpriteGroup.CssUrl;

                if (string.IsNullOrEmpty(url))
                {
                    url = this.ResolveUrl(outputFilePath);
                }
                else
                {
                    if (HttpContext.Current != null && url.StartsWith("//", StringComparison.InvariantCultureIgnoreCase))
                    {
                        url = HttpContext.Current.Request.Url.Scheme + ":" + url;
                    }
                }

                scriptLinks = "<link rel=\"stylesheet\" type=\"text/css\" href=\"" + url + "?" + QuerystringKeyName + "=" + etag + "\" />";
            }

            return scriptLinks;
        }

        public string CssInternal(string groupName)
        {
            var cssGroup = CssGroups[groupName];
            if (cssGroup == null)
            {
                throw new ConfigurationErrorsException(string.Format("CssGroup \"{0}\" does not exists!", groupName));
            }

            var outputFilePath = cssGroup.OutputFilePath;
            var scriptLinks = string.Empty;
            var cacheKey = GetKey(CssType, outputFilePath);

            var cachedValue = CacheManager.Get<string>(cacheKey);
            if (cachedValue != null)
            {
                scriptLinks = cachedValue;
            }
            else
            {
                if (!cssGroup.Debug)
                {
                    var fileInfo = new FileInfo(new Uri(MapPath(outputFilePath)).LocalPath);
                    var etag = Hasher.Hash(fileInfo);
					var url = cssGroup.Url;

					if (string.IsNullOrEmpty(url))
					{
						url = this.ResolveUrl(outputFilePath);
					}
					else
					{
						if (HttpContext.Current != null && url.StartsWith("//", StringComparison.InvariantCultureIgnoreCase))
						{
							url = HttpContext.Current.Request.Url.Scheme + ":" + url;
						}
					}

                    scriptLinks = "<link rel=\"stylesheet\" type=\"text/css\" href=\"" + url + "?" + QuerystringKeyName + "=" + etag + "\" media=\"" + cssGroup.Media + "\" />";
                }
                else
                {
                    var scriptLinksBuilder = new StringBuilder();

                    var files = cssGroup.Files.Cast<CssFileElement>()
                        .Select(file => new FileInfo(new Uri(MapPath(file.FilePath)).LocalPath));

                    var filesInDirectory = cssGroup.Directories.Cast<CssDirectoryElement>()
                        .SelectMany(x => new DirectoryInfo(new Uri(MapPath((x).DirectoryPath)).LocalPath)
                            .GetFiles("*", SearchOption.AllDirectories)
                            .Where(y => (string.IsNullOrEmpty(x.IncludeFilter) || Regex.IsMatch(y.Name, x.IncludeFilter, RegexOptions.Compiled | RegexOptions.IgnoreCase))
                            && (string.IsNullOrEmpty(x.ExcludeFilter) || !Regex.IsMatch(y.Name, x.ExcludeFilter, RegexOptions.Compiled | RegexOptions.IgnoreCase))));

                    files = files.Concat(filesInDirectory);

                    foreach (var fileInfo in files)
                    {
                        var etag = Hasher.Hash(fileInfo);
                        var url = this.ResolveUrl(ToRelative(fileInfo.FullName).ToString());

                        var fileName = fileInfo.Name.ToLower();

                        if (fileName.EndsWith(".less") || fileName.EndsWith(".less.css"))
                        {
                            etag = "'" + etag + "'";
                        }

                        scriptLinksBuilder.Append("<link rel=\"stylesheet\" type=\"text/css\" href=\"" + url + "?" + QuerystringKeyName + "=" + etag + "\" media=\"" + cssGroup.Media + "\" />");
                    }
                    scriptLinks = scriptLinksBuilder.ToString();
                }
                AddToCache(CssType, outputFilePath, scriptLinks);
            }

            return scriptLinks;
        }

        public string JsInternal(string groupName)
        {
            var jsGroup = JsGroups[groupName];
            if (jsGroup == null)
            {
                throw new ConfigurationErrorsException(string.Format("JsGroup \"{0}\" does not exists!", groupName));
            }
            var outputFilePath = jsGroup.OutputFilePath;
            var scriptLinks = string.Empty;

            var cacheKey = GetKey(JsType, outputFilePath);

            var cachedValue = CacheManager.Get<string>(cacheKey);
            if (cachedValue != null)
            {
                scriptLinks = cachedValue;
            }
            else
            {
                if (!jsGroup.Debug)
                {
                    var fileInfo = new FileInfo(new Uri(MapPath(outputFilePath)).LocalPath);
                    var etag = Hasher.Hash(fileInfo);
                    var url = string.IsNullOrEmpty(jsGroup.Url) ? this.ResolveUrl(outputFilePath) : jsGroup.Url;

					if (string.IsNullOrEmpty(url))
					{
						url = this.ResolveUrl(outputFilePath);
					}
					else
					{
						if (HttpContext.Current != null && url.StartsWith("//", StringComparison.InvariantCultureIgnoreCase))
						{
							url = HttpContext.Current.Request.Url.Scheme + ":" + url;
						}
					}

                    scriptLinks = "<script language=\"javascript\" type=\"text/javascript\" src=\"" + url + "?" + QuerystringKeyName + "=" + etag + "\"></script>";

                    if (!string.IsNullOrEmpty(url) && !string.IsNullOrEmpty(jsGroup.FallbackCondition))
                    {
                        var fallbackUrl = this.ResolveUrl(outputFilePath);
                        if (fallbackUrl != url)
                        {
                            scriptLinks += "<script language=\"javascript\" type=\"text/javascript\">if (" + jsGroup.FallbackCondition + "){document.write(unescape('%3Cscript language=\"javascript\" type=\"text/javascript\" src=\"" + fallbackUrl + "?" + QuerystringKeyName + "=" + etag + "\"%3E%3C/script%3E'));}</script>";
                        }
                    }
                }
                else
                {
                    var scriptLinksBuilder = new StringBuilder();

                    var files = jsGroup.Files.Cast<JsFileElement>()
                        .Select(file => new FileInfo(new Uri(MapPath(file.FilePath)).LocalPath));

                    var filesInDirectory = jsGroup.Directories.Cast<JsDirectoryElement>()
                        .SelectMany(x => new DirectoryInfo(new Uri(MapPath((x).DirectoryPath)).LocalPath)
                            .GetFiles("*", SearchOption.AllDirectories)
                            .Where(y => (string.IsNullOrEmpty(x.IncludeFilter) || Regex.IsMatch(y.Name, x.IncludeFilter, RegexOptions.Compiled | RegexOptions.IgnoreCase))
                            && (string.IsNullOrEmpty(x.ExcludeFilter) || !Regex.IsMatch(y.Name, x.ExcludeFilter, RegexOptions.Compiled | RegexOptions.IgnoreCase))));

                    files = files.Concat(filesInDirectory);

                    foreach (var fileInfo in files)
                    {
                        var etag = Hasher.Hash(fileInfo);
                        var relativePath = ToRelative(fileInfo.FullName).ToString();
                        var url = this.ResolveUrl(relativePath);

                        var isAnonymousModule = AmdModule.IsAnonymousAmdModule(fileInfo);

                        if (isAnonymousModule)
                        {
                            var moduleName = AmdModule.GetModuleName(fileInfo.Name);

                            scriptLinksBuilder.Append("<script language=\"javascript\" type=\"text/javascript\">" + AmdModule.GetModuleHeader(moduleName) + "</script>");
                        }

                        scriptLinksBuilder.Append("<script language=\"javascript\" type=\"text/javascript\" src=\"" + url + "?" + QuerystringKeyName + "=" + etag + "\"></script>");

                        if (isAnonymousModule)
                        {
                            scriptLinksBuilder.Append("<script language=\"javascript\" type=\"text/javascript\">" + AmdModule.GetModuleFooter() + "</script>");
                        }
                    }
                    scriptLinks = scriptLinksBuilder.ToString();
                }
                AddToCache(JsType, outputFilePath, scriptLinks);
            }

            return scriptLinks;
        }

        /// <summary>
        /// Get the cache key to use for caching.
        /// </summary>
        /// <param name="type"> </param>
        /// <param name="outputPath">The path for the crushed css file.</param>
        /// <returns>The cache key to use for caching.</returns>
        protected string GetKey(string type, string outputPath)
        {
            var prefix = CrusherHelperType + "|" + type + "|";
            return prefix + outputPath;
        }

        /// <summary>
        /// Cache the url generated for the crushed css file.
        /// </summary>
        /// <param name="type"> </param>
        /// <param name="outputFilePath">The crushed css file to use as the cache key.</param>
        /// <param name="scriptLinks">The link generated for the crushed file.</param>
        protected void AddToCache(string type, string outputFilePath, string scriptLinks)
        {
            var cacheKey = GetKey(type, outputFilePath);
            var filePath = this.MapPath(outputFilePath);

            CacheManager.Insert(
                cacheKey,
                scriptLinks,
                new CacheDependency(filePath, DateTime.Now),
                Cache.NoAbsoluteExpiration,
                Cache.NoSlidingExpiration,
                CacheItemPriority.Normal,
                null);
        }

        protected string ResolveUrl(string relativeUrl)
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
            sbUrl.Append(HttpRuntime.AppDomainAppVirtualPath);
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
            else foundSlash = false;
            foreach (char c in relativeUrl)
            {
                if (!foundQM)
                {
                    if (c == '?') foundQM = true;
                    else
                    {
                        if (c == '/' || c == '\\')
                        {
                            if (foundSlash) continue;
                            else
                            {
                                sbUrl.Append('/');
                                foundSlash = true;
                                continue;
                            }
                        }
                        else if (foundSlash) foundSlash = false;
                    }
                }
                sbUrl.Append(c);
            }

            return sbUrl.ToString();
        }
       
        protected string MapPath(string virtualPath)
        {
            return HostingEnvironment.MapPath(virtualPath);
        }

        public virtual Uri ToRelative(string filePath)
        {
            var absolutePathUri = new Uri(filePath);
            var rootUri = new Uri(HostingEnvironment.ApplicationPhysicalPath);

            var relativeUri = new Uri("~/" + rootUri.MakeRelativeUri(absolutePathUri), UriKind.Relative);

            return relativeUri;
        }
    }
}