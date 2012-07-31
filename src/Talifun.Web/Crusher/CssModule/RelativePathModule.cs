using System;

namespace Talifun.Web.Crusher.CssModule
{
    public class RelativePathModule : ICssModule
    {
        private readonly ICssPathRewriter _cssPathRewriter;
        private readonly IPathProvider _pathProvider;

        public RelativePathModule(ICssPathRewriter cssPathRewriter, IPathProvider pathProvider)
        {
            _cssPathRewriter = cssPathRewriter;
            _pathProvider = pathProvider;
        }

        public string Process(Uri cssRootPathUri, System.IO.FileInfo file, string fileContents)
        {
            var relativeRootUri = _pathProvider.GetRelativeRootUri(file.FullName);
            var distinctRelativePaths = _cssPathRewriter.FindDistinctRelativePaths(fileContents);
            return _cssPathRewriter.RewriteCssPathsToBeRelativeToPath(distinctRelativePaths,
                                                                          cssRootPathUri,
                                                                          relativeRootUri, fileContents);
        }
    }
}
