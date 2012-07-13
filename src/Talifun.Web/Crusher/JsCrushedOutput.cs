using System.Collections.Generic;
using System.Text;
using Talifun.FileWatcher;

namespace Talifun.Web.Crusher
{
    public class JsCrushedOutput
    {
        public StringBuilder Output { get; set; }
    	public IEnumerable<JsFileToWatch> FilesToWatch { get; set; }
        public IEnumerable<IEnhancedFileSystemWatcher> FoldersToWatch { get; set; }
    }
}
