using System.Collections.Generic;
using System.IO;
using Talifun.FileWatcher;

namespace Talifun.Web.Crusher
{
    public class CssCrushedOutput
    {
		public IEnumerable<CssFileToWatch> FilesToWatch { get; set; }
        public IEnumerable<IEnhancedFileSystemWatcher> FoldersToWatch { get; set; }
        public IEnumerable<FileInfo> CssAssetFilePaths { get; set; }
    }
}
