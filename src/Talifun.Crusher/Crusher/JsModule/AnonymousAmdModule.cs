using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Talifun.Crusher.Crusher.JsModule
{
    public class AnonymousAmdModule : IJsModule
    {
        private readonly IPathProvider _pathProvider;

        public AnonymousAmdModule(IPathProvider pathProvider)
        {
            _pathProvider = pathProvider;
        }

        private static readonly Regex AnonymousAmdModuleRegex = new Regex("define\\(\\[", RegexOptions.Compiled);
        public string Process(Uri jsRootPathUri, FileInfo file, string fileContents)
        {
            if (AnonymousAmdModuleRegex.IsMatch(fileContents))
            {
                var moduleName = Path.ChangeExtension(file.Name, "");
                if (moduleName.EndsWith("."))
                {
                    moduleName = moduleName.Substring(0, moduleName.Length - 1);
                }

                if (moduleName.StartsWith("~"))
                {
                    moduleName = moduleName.Substring(1);
                }

                fileContents = ";if(define.amd){define.amd.name='" + moduleName + "';};" + fileContents + ";if(define.amd){define.amd.name=null;};";
            }
            return fileContents;
        }
    }
}
