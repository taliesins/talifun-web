using System;
using System.IO;
using System.Linq;
using Talifun.Web.Helper.Pooling;
using Talifun.Web.Hogan;

namespace Talifun.Web.Crusher.JsModule
{
    public class HoganModule : IJsModule
    {
        private readonly string[] _hoganExtensions = { ".mustache", ".mustache.js" }; 

        protected readonly IPathProvider PathProvider;
        protected readonly Pool<HoganCompiler> HoganCompilerPool;

        public HoganModule(IPathProvider pathProvider, Pool<HoganCompiler> hoganCompilerPool)
        {
            PathProvider = pathProvider;
            HoganCompilerPool = hoganCompilerPool;
        }

        public string Process(Uri jsRootPathUri, FileInfo file, string fileContents)
        {
            if (!_hoganExtensions.Contains(file.Extension.ToLower()))
            {
                return fileContents;
            }

            var relativePath = PathProvider.MakeRelativeUri(jsRootPathUri, file);

            var compiler = HoganCompilerPool.Acquire();
            try
            {
                var compiledTemplate = compiler.Compile(fileContents);
                var javascript = string.Format("HoganTemplate['{0}']=new Hogan.Template ({1});", relativePath, compiledTemplate);
                return javascript;
            }
            finally
            {
                HoganCompilerPool.Release(compiler);
            }
        }
    }
}