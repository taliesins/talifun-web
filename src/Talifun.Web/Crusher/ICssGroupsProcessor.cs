using System.Text;
using Talifun.Web.Crusher.Config;

namespace Talifun.Web.Crusher
{
    public interface ICssGroupsProcessor
    {
        StringBuilder ProcessGroups(IPathProvider pathProvider, ICssCrusher cssCrusher, CssGroupElementCollection cssGroups);
    }
}