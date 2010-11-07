using System.Collections.Generic;

namespace Talifun.Web.Crusher
{
    public interface IJsCrusher
    {
        /// <summary>
        /// Add js files to be crushed
        /// </summary>
        /// <param name="outputPath">The path for the crushed js file</param>
        /// <param name="files">The js files to be crushed</param>
        void AddFiles(string outputPath, IEnumerable<JsFile> files);

        /// <summary>
        /// Remove all js files from being crushed
        /// </summary>
        /// <param name="outputPath">The path for the crushed js file</param>
        void RemoveFiles(string outputPath);
    }
}
