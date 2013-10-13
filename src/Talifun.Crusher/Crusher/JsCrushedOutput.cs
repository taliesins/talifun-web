using System.Collections.Generic;
using Talifun.FileWatcher;

namespace Talifun.Crusher.Crusher
{
    public class JsCrushedOutput
    {
    	public IEnumerable<JsFileToWatch> FilesToWatch { get; set; }
        public IEnumerable<IEnhancedFileSystemWatcher> FoldersToWatch { get; set; }
    }
}
