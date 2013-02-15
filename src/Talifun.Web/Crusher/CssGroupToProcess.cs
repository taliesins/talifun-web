using System.Text;
using Talifun.Web.Crusher.Config;

namespace Talifun.Web.Crusher
{
    public class CssGroupToProcess
    {
        public CssGroupElement Group { get; set; }
        public ICssCrusher Crusher { get; set; }
        public IPathProvider PathProvider { get; set; }
        public StringBuilder Output { get; set; }
    }
}
