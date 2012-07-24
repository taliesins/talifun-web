using System;
using System.Collections.Generic;
using System.IO;

namespace Talifun.Web.CssSprite
{
    public interface ICssSpriteCreator
    {
        IEnumerable<FileInfo> AddFiles(FileInfo imageOutputPath, Uri spriteImageUrl, FileInfo cssOutputPath, IEnumerable<ImageFile> files, IEnumerable<ImageDirectory> directories);
        void RemoveFiles(Uri spriteImageUrl);
    }
}
