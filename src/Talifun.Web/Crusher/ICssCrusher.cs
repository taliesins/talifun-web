using System;
using System.Collections.Generic;

namespace Talifun.Web.Crusher
{
    public interface ICssCrusher
    {
    	/// <summary>
    	/// Add group to be crushed.
    	/// </summary>
    	/// <param name="outputUri">The path for the crushed css file.</param>
    	/// <param name="cssFiles">The css files to be crushed.</param>
    	/// <param name="cssDirectories">The css directories to be crushed.</param>
    	/// <param name="appendHashToAssets">Should css assets have a hash appended to them.</param>
    	void CreateGroup(Uri outputUri, IEnumerable<CssFile> cssFiles, IEnumerable<CssDirectory> cssDirectories , bool appendHashToAssets);

        /// <summary>
        /// Remove a group from being crushed.
        /// </summary>
        /// <param name="outputUri">The path for the crushed css file.</param>
        void RemoveGroup(Uri outputUri);
    }
}