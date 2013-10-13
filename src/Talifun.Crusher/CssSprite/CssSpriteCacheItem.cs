using System;
using System.Collections.Generic;
using System.IO;

namespace Talifun.Crusher.CssSprite
{
    public class CssSpriteCacheItem
    {
        public FileInfo ImageOutputPath { get; set; }
        public FileInfo CssOutputPath { get; set; }
        public Uri SpriteImageUrl { get; set; }
        public IEnumerable<ImageFile> FilesToWatch { get; set; } 
        public IEnumerable<ImageFile> Files { get; set; }
        public IEnumerable<Talifun.FileWatcher.IEnhancedFileSystemWatcher> FoldersToWatch { get; set; } 
        public IEnumerable<ImageDirectory> Directories { get; set; }
    }
}
