using System.Collections.Generic;
using System.IO;

namespace Talifun.Web.Crusher
{
    public interface IMetaData
    {
        /// <summary>
        /// Creates meta data about files to be crushed.
        /// </summary>
        /// <param name="outputPath"></param>
        /// <param name="filesToWatch"></param>
        void CreateMetaData(FileInfo outputPath, IEnumerable<FileInfo> filesToWatch);

        /// <summary>
        /// Checkes if meta data is still valid against files to be crushed.
        /// </summary>
        /// <param name="outputPath"></param>
        /// <param name="filesToWatch"></param>
        /// <returns></returns>
        bool IsMetaDataFresh(FileInfo outputPath, IEnumerable<FileInfo> filesToWatch);
    }
}