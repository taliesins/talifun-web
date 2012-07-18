using System;
using System.Collections.Generic;

namespace Talifun.Web.Crusher
{
    public interface IJsCrusher
    {
    	/// <summary>
    	/// Add group to be crushed.
    	/// </summary>
    	/// <param name="outputUri">The path for the crushed js file.</param>
    	/// <param name="files">The js files to be crushed.</param>
		/// <param name="directories">The js directories to be crushed.</param>
        JsCrushedOutput AddGroup(Uri outputUri, IEnumerable<JsFile> files, IEnumerable<JsDirectory> directories);

        /// <summary>
		/// Remove a group from being crushed.
        /// </summary>
        /// <param name="outputPath">The path for the crushed js file.</param>
        void RemoveGroup(Uri outputPath);
    }
}
