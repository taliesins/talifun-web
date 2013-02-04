using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Talifun.Web.Crusher.CssModule
{
    public class CssAssetsHashModule : ICssModule
    {
        private readonly bool _appendAssetsToHash;
        private readonly ICssPathRewriter _cssPathRewriter;
        private readonly IPathProvider _pathProvider;

        public CssAssetsHashModule(bool appendAssetsToHash, ICssPathRewriter cssPathRewriter, IPathProvider pathProvider)
        {
            _appendAssetsToHash = appendAssetsToHash;
            _cssPathRewriter = cssPathRewriter;
            _pathProvider = pathProvider;
        }

        protected IEnumerable<CssAsset> GetCssAssets(Uri cssRootPathUri, string fileContents)
        {
            var distinctLocalPaths = _cssPathRewriter.FindDistinctLocalPaths(fileContents);

            return distinctLocalPaths
                .Select(distinctLocalPath => new CssAsset
                {
                    File = new FileInfo(new Uri(_pathProvider.MapPath(cssRootPathUri, distinctLocalPath)).LocalPath),
                    Url = distinctLocalPath
                })
                .Where(cssAssetFileInfo => cssAssetFileInfo.File.Exists);
        }

        public string Process(Uri cssRootPathUri, System.IO.FileInfo file, string fileContents)
        {
            if (!_appendAssetsToHash)
            {
                return fileContents;
            }

            var distinctLocalPathsThatExist = GetCssAssets(cssRootPathUri, fileContents).Select(x => x.Url);
            return _cssPathRewriter.RewriteCssPathsToAppendHash(distinctLocalPathsThatExist, cssRootPathUri, fileContents);
        }
    }
}
