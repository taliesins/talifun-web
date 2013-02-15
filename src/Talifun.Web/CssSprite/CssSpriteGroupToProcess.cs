using System.Text;
using Talifun.Web.Crusher;
using Talifun.Web.CssSprite.Config;

namespace Talifun.Web.CssSprite
{
    public class CssSpriteGroupToProcess
    {
        public CssSpriteGroupElement Group { get; set; }
        public ICssSpriteCreator CssSpriteCreator { get; set; }
        public IPathProvider PathProvider { get; set; }
        public StringBuilder Output { get; set; }
    }
}
