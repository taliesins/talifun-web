using System.IO;
using System.Text;

namespace Talifun.Web.Helper
{
    public interface IRetryableFileWriter
    {
        /// <summary>
        /// Save string content to file.
        /// </summary>
        /// <param name="output">The string to save.</param>
        /// <param name="outputPath">The path for the file to save.</param>
        string SaveContentsToFile(string output, FileInfo outputPath);

        /// <summary>
        /// Save StringBuilder content to file.
        /// </summary>
        /// <param name="output">The StringBuilder to save.</param>
        /// <param name="outputPath">The path for the file to save.</param>
        string SaveContentsToFile(StringBuilder output, FileInfo outputPath);

        /// <summary>
        /// Save stream to file.
        /// </summary>
        /// <param name="outputStream">The stream to save.</param>
        /// <param name="outputPath">The path for the file to save.</param>
        string SaveContentsToFile(Stream outputStream, FileInfo outputPath);
    }
}
