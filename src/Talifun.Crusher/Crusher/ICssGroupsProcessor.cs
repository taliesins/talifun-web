using System.Text;
using Talifun.Crusher.Configuration.Css;

namespace Talifun.Crusher.Crusher
{
    public interface ICssGroupsProcessor
    {
        StringBuilder ProcessGroups(IPathProvider pathProvider, ICssCrusher cssCrusher, CssGroupElementCollection cssGroups);
    }
}