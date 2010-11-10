using System;
using System.IO;
using System.Text;

namespace Talifun.Web.Helper
{
    public class RetryableFileWriter : IRetryableFileWriter
    {
        protected readonly int BufferSize;
        protected readonly IRetryableFileOpener RetryableFileOpener;
        protected readonly IHasher Hasher;
        public RetryableFileWriter(int bufferSize, IRetryableFileOpener retryableFileOpener, IHasher hasher)
        {
            BufferSize = bufferSize;
            RetryableFileOpener = retryableFileOpener;
            Hasher = hasher;
        }

        /// <summary>
        /// Save string content to file.
        /// </summary>
        /// <param name="output">The string to save.</param>
        /// <param name="outputPath">The path for the file to save.</param>
        public virtual void SaveContentsToFile(string output, string outputPath)
        {
            using (var outputStream = new MemoryStream())
            {
                var uniEncoding = Encoding.Default;
                outputStream.Write(uniEncoding.GetBytes(output), 0, uniEncoding.GetByteCount(output));

                SaveContentsToFile(outputStream, outputPath);
            }
        }

        /// <summary>
        /// Save StringBuilder content to file.
        /// </summary>
        /// <param name="output">The StringBuilder to save.</param>
        /// <param name="outputPath">The path for the file to save.</param>
        public virtual void SaveContentsToFile(StringBuilder output, string outputPath)
        {
            using (var outputStream = new MemoryStream())
            {
                var uniEncoding = Encoding.Default;
                var uncompressedContent = output.ToString();

                outputStream.Write(uniEncoding.GetBytes(uncompressedContent), 0, uniEncoding.GetByteCount(uncompressedContent));

                SaveContentsToFile(outputStream, outputPath);
            }
        }

        /// <summary>
        /// Save stream to file.
        /// </summary>
        /// <param name="outputStream">The stream to save.</param>
        /// <param name="outputPath">The path for the file to save.</param>
        public virtual void SaveContentsToFile(Stream outputStream, string outputPath)
        {
            //We might be competing with the web server for the output file, so try to overwrite it at regular intervals
            using (var outputFile = RetryableFileOpener.OpenFileStream(new FileInfo(outputPath), 5, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
            {
                var overwrite = true;
                if (outputFile.Length > 0)
                {
                    var newOutputFileHash = Hasher.CalculateMd5Etag(outputStream);
                    var outputFileHash = Hasher.CalculateMd5Etag(outputFile);

                    overwrite = (newOutputFileHash != outputFileHash);
                }

                if (overwrite)
                {
                    outputStream.Seek(0, SeekOrigin.Begin);
                    outputFile.SetLength(outputStream.Length); //Truncate current file
                    outputFile.Seek(0, SeekOrigin.Begin);

                    var bufferSize = Convert.ToInt32(Math.Min(outputStream.Length, BufferSize));
                    var buffer = new byte[bufferSize];

                    int bytesRead;
                    while ((bytesRead = outputStream.Read(buffer, 0, bufferSize)) > 0)
                    {
                        outputFile.Write(buffer, 0, bytesRead);
                    }
                    outputFile.Flush();
                }
            }
        }

    }
}
