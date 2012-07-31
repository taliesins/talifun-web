using System;

namespace Talifun.Web.Crusher.CssModule
{
    public class RelativePathModule : ICssModule
    {
        private readonly Uri _relativeRootUri;
        private readonly ICssPathRewriter _cssPathRewriter;

        public RelativePathModule(Uri relativeRootUri, ICssPathRewriter cssPathRewriter)
        {
            _relativeRootUri = relativeRootUri;
            _cssPathRewriter = cssPathRewriter;
        }

        public string Process(Uri cssRootPathUri, System.IO.FileInfo file, string fileContents)
        {
            var distinctRelativePaths = _cssPathRewriter.FindDistinctRelativePaths(fileContents);
            return _cssPathRewriter.RewriteCssPathsToBeRelativeToPath(distinctRelativePaths,
                                                                          cssRootPathUri,
                                                                          _relativeRootUri, fileContents);
        }
    }
}
