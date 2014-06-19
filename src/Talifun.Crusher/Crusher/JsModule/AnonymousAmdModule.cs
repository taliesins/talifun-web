using System;
using System.IO;
using Talifun.Web.Helper;

namespace Talifun.Crusher.Crusher.JsModule
{
    public class AnonymousAmdModule : IJsModule
    {
        private readonly IAmdModule _amdModule;

        public AnonymousAmdModule(IAmdModule amdModule)
        {
            _amdModule = amdModule;
        }
     
        public string Process(Uri jsRootPathUri, FileInfo file, string fileContents)
        {
            if (_amdModule.IsAnonymousAmdModule(fileContents))
            {
                var moduleName = _amdModule.GetModuleName(file.Name);

                fileContents = _amdModule.GetModuleHeader(moduleName) + fileContents + _amdModule.GetModuleFooter();
            }
            return fileContents;
        }
    }
}
