using System;
using System.Collections.Generic;
using System.IO;

namespace Talifun.Crusher.CssSprite
{
    public interface ICssSpriteCreator
    {
        IEnumerable<ImageFile> AddFiles(FileInfo imageOutputPath, Uri spriteImageUrl, FileInfo cssOutputPath, IEnumerable<ImageFile> files, IEnumerable<ImageDirectory> directories);
        void RemoveFiles(Uri spriteImageUrl);
    }
}
