using System.IO;
using System.Text.RegularExpressions;

namespace Talifun.Web.Helper
{
    public class AmdModule : IAmdModule
    {
        private readonly IRetryableFileOpener _retryableFileOpener;
        private static readonly Regex AnonymousAmdModuleRegex = new Regex("define\\(\\[", RegexOptions.Compiled);

        public AmdModule(IRetryableFileOpener retryableFileOpener)
        {
            _retryableFileOpener = retryableFileOpener;
        }

        public bool IsAnonymousAmdModule(FileInfo fileInfo)
        {
            var fileContent = _retryableFileOpener.ReadAllText(fileInfo);
            return AnonymousAmdModuleRegex.IsMatch(fileContent);
        }

        public bool IsAnonymousAmdModule(string content)
        {
            return AnonymousAmdModuleRegex.IsMatch(content);
        }

        public string GetModuleName(string moduleName)
        {
            moduleName = Path.ChangeExtension(moduleName, "");
            if (moduleName.EndsWith("."))
            {
                moduleName = moduleName.Substring(0, moduleName.Length - 1);
            }

            var regex = new Regex(@"(.*)(\-\d+(\.\d+)+|\.min|\.debug)$");
            var match = regex.Match(moduleName);

            while (match.Success)
            {
                moduleName = match.Groups[0].Value;
                match = regex.Match(moduleName);
            }

            return moduleName;
        }

        public string GetModuleHeader(string moduleName)
        {
            return ";if(define&&define.amd){define.amd.name='" + moduleName + "';};";
        }

        public string GetModuleFooter()
        {
            return ";if(define&&define.amd){define.amd.name=null;};";
        }
    }
}