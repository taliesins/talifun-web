using System;
using System.Collections.Generic;

namespace Talifun.Web.Crusher
{
    public interface ICssCrusher
    {
        /// <summary>
        /// Add css files to be crushed.
        /// </summary>
        /// <param name="outputUri">The path for the crushed css file.</param>
        /// <param name="cssFiles">The css files to be crushed.</param>
        /// <param name="appendHashToAssets">Should css assets have a hash appended to them.</param>
        void AddFiles(Uri outputUri, IEnumerable<CssFile> cssFiles, bool appendHashToAssets);

        /// <summary>
        /// Remove all css files from being crushed
        /// </summary>
        /// <param name="outputUri">The path for the crushed css file.</param>
        void RemoveFiles(Uri outputUri);
    }
}