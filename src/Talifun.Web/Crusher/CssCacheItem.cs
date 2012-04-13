using System;
using System.Collections.Generic;
using System.IO;

namespace Talifun.Web.Crusher
{
    public class CssCacheItem
    {
        public bool AppendHashToAssets { get; set; }
        public Uri OutputUri { get; set; }
        public IEnumerable<CssFile> CssFiles { get; set; }
        public IEnumerable<FileInfo> CssAssetFilePaths { get; set; }
    }
}
