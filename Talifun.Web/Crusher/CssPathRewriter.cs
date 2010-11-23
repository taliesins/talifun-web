using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Talifun.Web.Crusher
{
    public class CssPathRewriter : ICssPathRewriter
    {
        protected readonly ICssAssetsFileHasher CssAssetsFileHasher;
        public CssPathRewriter(ICssAssetsFileHasher cssAssetsFileHasher)
        {
            CssAssetsFileHasher = cssAssetsFileHasher;
        }

        public virtual string RewriteCssPaths(string outputPath, string sourcePath, string css)
        {
            var sourceUri = new Uri(Path.GetDirectoryName(sourcePath) + "/", UriKind.Absolute);
            var outputUri = new Uri(Path.GetDirectoryName(outputPath) + "/", UriKind.Absolute);

            var relativePaths = FindDistinctRelativePathsIn(css);

            foreach (var relativePath in relativePaths)
            {
                var resolvedSourcePath = new Uri(sourceUri + relativePath);
                var resolvedOutput = outputUri.MakeRelativeUri(resolvedSourcePath);

                css = css.Replace(relativePath, resolvedOutput.OriginalString);
            }

            if (CssAssetsFileHasher != null)
            {
                var localRelativePathsThatExist = FindDistinctLocalRelativePathsThatExist(css);

                foreach (string localRelativePathThatExist in localRelativePathsThatExist)
                {
                    var localRelativePathThatExistWithFileHash = CssAssetsFileHasher.AppendFileHash(outputPath, localRelativePathThatExist);

                    if (localRelativePathThatExist != localRelativePathThatExistWithFileHash)
                    {
                        css = css.Replace(localRelativePathThatExist, localRelativePathThatExistWithFileHash);
                    }
                }
            }

            return css;
        }

        public virtual IEnumerable<string> FindDistinctRelativePathsIn(string css)
        {
            var matches = Regex.Matches(css, @"url\([""']{0,1}(.+?)[""']{0,1}\)", RegexOptions.IgnoreCase);
            var matchesHash = new HashSet<string>();
            foreach (Match match in matches)
            {
                var path = match.Groups[1].Captures[0].Value;
                if (!path.StartsWith("/"))
                {
                    if (matchesHash.Add(path))
                    {
                        yield return path;
                    }
                }
            }
        }

        public virtual IEnumerable<string> FindDistinctLocalRelativePathsThatExist(string css)
        {
            var matches = Regex.Matches(css, @"url\([""']{0,1}(.+?)[""']{0,1}\)", RegexOptions.IgnoreCase);
            var matchesHash = new HashSet<string>();
            foreach (Match match in matches)
            {
                var path = match.Groups[1].Captures[0].Value;
                if (!path.StartsWith("http", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (matchesHash.Add(path))
                    {
                        yield return path;
                    }
                }
            }
        }
    }
}
