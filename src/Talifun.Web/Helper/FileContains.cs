using System.IO;
using System.Text.RegularExpressions;

namespace Talifun.Web.Helper
{
    public class FileContains : IFileContains
    {
        private readonly IRetryableFileOpener _retryableFileOpener;

        public FileContains(IRetryableFileOpener retryableFileOpener)
        {
            _retryableFileOpener = retryableFileOpener;
        }

        public bool Contains(FileInfo fileInfo, string substring)
        {
            var fileContent = _retryableFileOpener.ReadAllText(fileInfo);
            return fileContent.Contains(substring);
        }

        public bool Contains(FileInfo fileInfo, Regex match)
        {
            var fileContent = _retryableFileOpener.ReadAllText(fileInfo);
            return match.IsMatch(fileContent);
        }
    }
}