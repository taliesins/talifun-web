using System.IO;
using System.Text;
using System.Threading;

namespace Talifun.Web
{
    public class RetryableFileOpener : IRetryableFileOpener
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
        public FileStream OpenFileStream(FileInfo fileInfo, int retry, FileMode fileMode, FileAccess fileAccess, FileShare fileShare)
        {
            var delay = 0;

            for (var i = 0; i < retry; i++)
            {
                try
                {
                    var stream = new FileStream(fileInfo.FullName, fileMode, fileAccess, fileShare);
                    return stream;
                }
                catch (IOException)
                {
                    delay += 100;
                    if (i == retry) throw;
                }

                Thread.Sleep(delay);
            }

            //We will never get here
            throw new IOException("Unable to open file - " + fileInfo.FullName);
        }

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
        public StreamReader OpenTextStreamReader(FileInfo fileInfo, int retry)
        {
            var delay = 0;

            for (var i = 0; i < retry; i++)
            {
                try
                {
                    var stream = new StreamReader(fileInfo.FullName, Encoding.UTF8);
                    return stream;
                }
                catch (IOException)
                {
                    delay += 100;
                    if (i == retry) throw;
                }

                Thread.Sleep(delay);
            }

            //We will never get here
            throw new IOException("Unable to open file - " + fileInfo.FullName);
        }

        /// <summary>
        /// Read the contents of a text file and return them as a string.
        /// </summary>
        /// <param name="fileInfo">The file to attempt to read the text contents on.</param>
        /// <returns>Contents of text file</returns>
        public string ReadAllText(FileInfo fileInfo)
        {
            using (var stream = OpenTextStreamReader(fileInfo, 5))
            {
                return stream.ReadToEnd();
            }
        }
    }
}
