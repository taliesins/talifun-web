using System;
using System.Collections.Generic;

namespace Talifun.Crusher.Crusher
{
    public class JsCacheItem
    {
        public Uri OutputUri { get; set; }
		public IEnumerable<JsFileToWatch> FilesToWatch { get; set; }
        public IEnumerable<JsFile> Files { get; set; }
        public IEnumerable<Talifun.FileWatcher.IEnhancedFileSystemWatcher> FoldersToWatch { get; set; } 
		public IEnumerable<JsDirectory> Directories { get; set; }
    }
}
