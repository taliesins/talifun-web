using System;
using System.Collections.Generic;

namespace Talifun.Web.Crusher
{
    public interface ICssPathRewriter
    {
        string RewriteCssPathsToBeRelativeToPath(IEnumerable<Uri> relativePaths, Uri cssRootUri, Uri relativeRootUri, string css);
        IEnumerable<Uri> FindDistinctRelativePaths(string css);
        string RewriteCssPathsToAppendHash(IEnumerable<Uri> localPaths, Uri cssRootUri, string css);
        IEnumerable<Uri> FindDistinctLocalPaths(string css);
    }
}