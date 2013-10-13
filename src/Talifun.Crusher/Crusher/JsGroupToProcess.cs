using System.Text;
using Talifun.Crusher.Configuration.Js;

namespace Talifun.Crusher.Crusher
{
    public class JsGroupToProcess
    {
        public JsGroupElement Group { get; set; }
        public IJsCrusher Crusher { get; set; }
        public IPathProvider PathProvider { get; set; }
        public StringBuilder Output { get; set; }
    }
}