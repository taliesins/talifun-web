using System;
using System.Collections.Generic;
using System.IO;

namespace Talifun.Web.CssSprite
{
    public interface ICssSpriteCreator
    {
        void AddFiles(FileInfo imageOutputPath, Uri spriteImageUrl, FileInfo cssOutputPath, IEnumerable<ImageFile> files);
        void RemoveFiles(FileInfo imageOutputPath, Uri spriteImageUrl, FileInfo cssOutputPath);
    }
}
