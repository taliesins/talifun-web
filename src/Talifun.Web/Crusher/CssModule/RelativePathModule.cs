using System;

namespace Talifun.Web.Crusher.CssModule
{
    public class RelativePathModule : ICssModule
    {
        private readonly Uri _absoluteUriDirectory;
        private readonly ICssPathRewriter _cssPathRewriter;

        public RelativePathModule(Uri absoluteUriDirectory, ICssPathRewriter cssPathRewriter)
        {
            _absoluteUriDirectory = absoluteUriDirectory;
            _cssPathRewriter = cssPathRewriter;
        }

        public string Process(Uri cssRootPathUri, System.IO.FileInfo file, string fileContents)
        {
            var distinctRelativePaths = _cssPathRewriter.FindDistinctRelativePaths(fileContents);
            return _cssPathRewriter.RewriteCssPathsToBeRelativeToPath(distinctRelativePaths,
                                                                          cssRootPathUri,
                                                                          _absoluteUriDirectory, fileContents);
        }
    }
}
