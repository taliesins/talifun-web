using System.Text;
using Talifun.Crusher.Configuration.Sprites;
using Talifun.Crusher.Crusher;

namespace Talifun.Crusher.CssSprite
{
    public class CssSpriteGroupToProcess
    {
        public CssSpriteGroupElement Group { get; set; }
        public ICssSpriteCreator CssSpriteCreator { get; set; }
        public IPathProvider PathProvider { get; set; }
        public StringBuilder Output { get; set; }
    }
}
