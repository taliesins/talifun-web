using System.Text;
using Talifun.Web.Crusher.Config;

namespace Talifun.Web.Crusher
{
    public interface IJsGroupsProcessor
    {
        StringBuilder ProcessGroups(IPathProvider pathProvider, IJsCrusher jsCrusher, JsGroupElementCollection jsGroups);
    }
}