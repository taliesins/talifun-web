using System.Collections.Generic;

namespace Talifun.Web.CssSprite
{
    public interface ICssSpriteCreator
    {
        void AddFiles(string imageOutputPath, string spriteImageUrl, string cssOutputPath, IEnumerable<ImageFile> files);
        void RemoveFiles(string imageOutputPath, string spriteImageUrl, string cssOutputPath);
    }
}
