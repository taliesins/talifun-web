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

            if (moduleName.EndsWith(".min"))
            {
                moduleName = moduleName.Substring(0, moduleName.Length - 4);
            }

            if (moduleName.EndsWith(".debug"))
            {
                moduleName = moduleName.Substring(0, moduleName.Length - 6);
            }

            return moduleName;
        }

        public string GetModuleHeader(string moduleName)
        {
            return ";if(define.amd){define.amd.name='" + moduleName + "';};";
        }

        public string GetModuleFooter()
        {
            return ";if(define.amd){define.amd.name=null;};";
        }
    }
}