using System.Text;
using Talifun.Crusher.Configuration.Css;

namespace Talifun.Crusher.Crusher
{
    public class CssGroupToProcess
    {
        public CssGroupElement Group { get; set; }
        public ICssCrusher Crusher { get; set; }
        public IPathProvider PathProvider { get; set; }
        public StringBuilder Output { get; set; }
    }
}
