using System.Text;
using Talifun.Crusher.Configuration.Js;

namespace Talifun.Crusher.Crusher
{
    public interface IJsGroupsProcessor
    {
        StringBuilder ProcessGroups(IPathProvider pathProvider, IJsCrusher jsCrusher, JsGroupElementCollection jsGroups);
    }
}