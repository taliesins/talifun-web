using System.Collections.Generic;

namespace Talifun.Web.Crusher
{
    public interface ICssCrusher
    {
        /// <summary>
        /// Add css files to be crushed.
        /// </summary>
        /// <param name="outputPath">The path for the crushed css file.</param>
        /// <param name="files">The css files to be crushed.</param>
        void AddFiles(string outputPath, IEnumerable<CssFile> files);

        /// <summary>
        /// Remove all css files from being crushed
        /// </summary>
        /// <param name="outputPath">The path for the crushed css file.</param>
        void RemoveFiles(string outputPath);
    }
}
