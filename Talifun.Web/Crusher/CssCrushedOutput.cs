using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Talifun.Web.Crusher
{
    public class CssCrushedOutput
    {
        public StringBuilder Output { get; set; }
        public IEnumerable<FileInfo> CssAssetFilePaths { get; set; }
    }
}
