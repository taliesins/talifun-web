using System;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Caching;
using System.Web.UI;
using System.Web.UI.WebControls;
using Talifun.Web.Crusher.Config;

namespace Talifun.Web.Crusher
{
    /// <summary>
    /// Generates the include references to js for a web page according to the provided configuration.
    /// </summary>
    public class JsControl : WebControl
    {
        protected readonly string QuerystringKeyName;
        protected readonly JsGroupElementCollection JsGroups;
        protected readonly IRetryableFileOpener RetryableFileOpener;
        protected readonly IHasher Hasher;

        public JsControl()
        {
            QuerystringKeyName = CurrentCrusherConfiguration.Current.QuerystringKeyName;
            JsGroups = CurrentCrusherConfiguration.Current.JsGroups;
            RetryableFileOpener = new RetryableFileOpener();
            Hasher = new Hasher(RetryableFileOpener);
        }

        /// <summary>
        /// The name of js group to generate the include headers for.
        /// </summary>
        public virtual string GroupName
        {
            get
            {
                var o = ViewState["GroupName"];
                return ((o == null) ? String.Empty : (string)o);
            }
            set
            {
                ViewState["GroupName"] = value;
            }
        }

        /// <summary>
        /// Generate the url for the crushed js file.
        /// </summary>
        /// <param name="writer"></param>
        /// <remarks>
        /// The generated url will also have a querystring with the hash of the file appended to it.
        /// </remarks>
        protected override void Render(HtmlTextWriter writer)
        {
            var jsGroup = JsGroups[GroupName];
            var outputFilePath = jsGroup.OutputFilePath;
            var scriptLinks = string.Empty;

            var cacheKey = GetKey(outputFilePath);

            var cachedValue = HttpRuntime.Cache.Get(cacheKey);
            if (cachedValue != null)
            {
                scriptLinks = (string)cachedValue;
            }
            else
            {
                if (!jsGroup.Debug)
                {
                    var fileInfo = new FileInfo(HttpContext.Current.Server.MapPath(outputFilePath));
                    var etag = Hasher.CalculateMd5Etag(fileInfo);
                    var url = string.IsNullOrEmpty(jsGroup.Url) ? this.ResolveUrl(outputFilePath) : jsGroup.Url;

                    scriptLinks = "<script language=\"javascript\" type=\"text/javascript\" src=\"" + url + "?" + QuerystringKeyName + "=" + etag + "\"></script>";
                }
                else
                {
                    var scriptLinksBuilder = new StringBuilder();
                    foreach (JsFileElement file in jsGroup.Files)
                    {
                        var fileInfo = new FileInfo(HttpContext.Current.Server.MapPath(file.FilePath));
                        var etag = Hasher.CalculateMd5Etag(fileInfo);
                        var url = this.ResolveUrl(file.FilePath);

                        scriptLinksBuilder.Append("<script language=\"javascript\" type=\"text/javascript\" src=\"" + url + "?" + QuerystringKeyName + "=" + etag + "\"></script>");
                    }
                    scriptLinks = scriptLinksBuilder.ToString();
                }
                AddToCache(outputFilePath, scriptLinks);
            }

            writer.Write(scriptLinks);
            return;

        }

        /// <summary>
        /// Cache the url generated for the crushed js file.
        /// </summary>
        /// <param name="outputFilePath">The crushed js file to use as the cache key.</param>
        /// <param name="scriptLinks">The link generated for the crushed js file.</param>
        protected void AddToCache(string outputFilePath, string scriptLinks)
        {
            var cacheKey = GetKey(outputFilePath);
            var filePath = this.MapPathSecure(outputFilePath);

            HttpRuntime.Cache.Insert(
                cacheKey,
                scriptLinks,
                new CacheDependency(filePath, DateTime.Now),
                Cache.NoAbsoluteExpiration,
                Cache.NoSlidingExpiration);
        }

        /// <summary>
        /// Get the cache key to use for caching.
        /// </summary>
        /// <param name="outputPath">The path for the crushed js file.</param>
        /// <returns>The cache key to use for caching.</returns>
        private static string GetKey(string outputPath)
        {
            var prefix = typeof(JsControl).ToString() + "|";
            return prefix + outputPath;
        }
    }
}