using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Talifun.Web.Crusher
{
    public class CssPathRewriter : ICssPathRewriter
    {
        protected readonly ICssAssetsFileHasher CssAssetsFileHasher;
        protected readonly IPathProvider PathProvider;
        public CssPathRewriter(ICssAssetsFileHasher cssAssetsFileHasher, IPathProvider pathProvider)
        {
            CssAssetsFileHasher = cssAssetsFileHasher;
            PathProvider = pathProvider;
        }

        public virtual string RewriteCssPathsToBeRelativeToPath(IEnumerable<Uri> relativePaths, Uri cssRootUri, Uri relativeRootUri, string css)
        {
            if (!cssRootUri.IsAbsoluteUri)
            {
                cssRootUri = new Uri(PathProvider.MapPath(cssRootUri));
            }

            if (!relativeRootUri.IsAbsoluteUri)
            {
                relativeRootUri = new Uri(PathProvider.MapPath(relativeRootUri));
            }

            foreach (var relativePath in relativePaths)
            {
                var absoluteUri = relativePath.IsAbsoluteUri
                                      ? relativePath
                                      : new Uri(PathProvider.MapPath(relativeRootUri, relativePath));

                var resolvedOutput = cssRootUri.MakeRelativeUri(absoluteUri);

                if (relativePath.OriginalString == resolvedOutput.OriginalString) continue;

                css = css.Replace(relativePath.OriginalString, resolvedOutput.OriginalString);
            }

            return css;
        }

        public virtual IEnumerable<Uri> FindDistinctRelativePaths(string css)
        {
            var matches = Regex.Matches(css, @"url\([""']{0,1}(.+?)[""']{0,1}\)", RegexOptions.IgnoreCase);
            var matchesHash = new HashSet<Uri>();
            
            foreach (Match match in matches)
            {
                var path = match.Groups[1].Captures[0].Value;
                if (path.StartsWith("http", StringComparison.InvariantCultureIgnoreCase) || path.StartsWith("/"))
                    continue;

                var uri = new Uri(path, UriKind.RelativeOrAbsolute);
                matchesHash.Add(uri);
            }

            return matchesHash;
        }

        public virtual string RewriteCssPathsToAppendHash(IEnumerable<Uri> localPaths, Uri cssRootUri, string css)
        {
            if (!cssRootUri.IsAbsoluteUri)
            {
                cssRootUri = new Uri(PathProvider.MapPath(cssRootUri));
            }

            foreach (var localPath in localPaths)
            {
                var localRelativePathThatExistWithFileHash = CssAssetsFileHasher.AppendFileHash(cssRootUri, localPath);

                if (localPath != localRelativePathThatExistWithFileHash)
                {
                    css = css.Replace(localPath.OriginalString, localRelativePathThatExistWithFileHash.OriginalString);
                }
            }

            return css;
        }

        public virtual IEnumerable<Uri> FindDistinctLocalPaths(string css)
        {
            var matches = Regex.Matches(css, @"url\([""']{0,1}(.+?)[""']{0,1}\)", RegexOptions.IgnoreCase);
            var matchesHash = new HashSet<Uri>();
            foreach (Match match in matches)
            {
                var path = match.Groups[1].Captures[0].Value;
                if (path.StartsWith("http", StringComparison.InvariantCultureIgnoreCase)) continue;

                var uri = new Uri(path, UriKind.RelativeOrAbsolute);
                matchesHash.Add(uri);
            }

            return matchesHash;
        }
    }
}
