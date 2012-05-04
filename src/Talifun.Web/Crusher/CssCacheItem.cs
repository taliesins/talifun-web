using System;
using System.Collections.Generic;
using System.IO;

namespace Talifun.Web.Crusher
{
    public class CssCacheItem
    {
        public bool AppendHashToAssets { get; set; }
        public Uri OutputUri { get; set; }
    	public IEnumerable<CssFileToWatch> FilesToWatch { get; set; }
		public IEnumerable<FileInfo> AssetFilesToWatch { get; set; }
        public IEnumerable<CssFile> Files { get; set; }
    	public IEnumerable<CssDirectory> Directories { get; set; } 
    }
}
