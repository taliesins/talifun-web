using System.Text;
using Talifun.Web.Crusher.Config;

namespace Talifun.Web.Crusher
{
    public class JsGroupToProcess
    {
        public JsGroupElement Group { get; set; }
        public IJsCrusher Crusher { get; set; }
        public IPathProvider PathProvider { get; set; }
        public StringBuilder Output { get; set; }
    }
}