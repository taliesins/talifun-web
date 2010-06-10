using System.IO;

namespace Talifun.Web
{
    public interface IRetryableFileOpener
    {
        /// <summary>
        /// File might be locked when attempting to open it. This will attempt to open the file the number of times specified by <paramref name="retry"/>
        /// </summary>
        /// <param name="fileInfo">The file to attempt to get a file stream for</param>
        /// <param name="retry">The number of times a file open should be attempted</param>
        /// <param name="fileMode">The file mode to use</param>
        /// <param name="fileAccess">The file access to use</param>
        /// <param name="fileShare">The file sharing to use</param>
        /// <returns>A file stream of the file</returns>
        /// <remarks>
        /// It attempt to open the file in increasingly longer periods and throw an exception if it cannot open it within the
        /// specified number of retries.
        /// </remarks>
        FileStream OpenFileStream(FileInfo fileInfo, int retry, FileMode fileMode, FileAccess fileAccess, FileShare fileShare);

        /// <summary>
        /// File might be locked when attempting to open it. This will attempt to open the file the number of times specified by <paramref name="retry"/>
        /// </summary>
        /// <param name="fileInfo">The file to attempt to get a text stream for</param>
        /// <param name="retry">The number of times a file open should be attempted</param>
        /// <returns>A text stream of the file</returns>
        /// <remarks>
        /// It attempt to open the file in increasingly longer periods and throw an exception if it cannot open it within the
        /// specified number of retries.
        /// </remarks>
        StreamReader OpenTextStreamReader(FileInfo fileInfo, int retry);

        /// <summary>
        /// Read the contents of a text file and return them as a string.
        /// </summary>
        /// <param name="fileInfo">The file to attempt to read the text contents on.</param>
        /// <returns>Contents of text file</returns>
        string ReadAllText(FileInfo fileInfo);
    }
}