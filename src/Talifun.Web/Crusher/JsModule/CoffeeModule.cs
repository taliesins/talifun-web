using System;
using System.IO;
using System.Linq;
using Talifun.Web.Coffee;
using Talifun.Web.Helper.Pooling;

namespace Talifun.Web.Crusher.JsModule
{
    public class CoffeeModule : IJsModule
    {
        private readonly string[] _hoganExtensions = { ".coffee", ".coffee.js" };

        protected readonly Pool<CoffeeCompiler> CoffeeCompilerPool;

        public CoffeeModule(Pool<CoffeeCompiler> coffeeCompilerPool)
        {
            CoffeeCompilerPool = coffeeCompilerPool;
        }

        public string Process(Uri jsRootPathUri, FileInfo file, string fileContents)
        {
            if (!_hoganExtensions.Contains(file.Extension.ToLower()))
            {
                return fileContents;
            }

            var compiler = CoffeeCompilerPool.Acquire();
            try
            {
                var compiledJavascript = compiler.Compile(fileContents);
                return compiledJavascript;
            }
            finally
            {
                CoffeeCompilerPool.Release(compiler);
            }
        }
    }
}