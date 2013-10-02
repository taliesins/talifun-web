using System;
using System.IO;
using System.Linq;
using Talifun.Web.Helper.Pooling;
using Talifun.Web.IcedCoffee;

namespace Talifun.Web.Crusher.JsModule
{
    public class IcedCoffeeModule : IJsModule
    {
        private readonly string[] _icedCoffeeExtensions = { ".iced", ".iced.js" };

        protected readonly Pool<IcedCoffeeCompiler> IcedCoffeeCompilerPool;

        public IcedCoffeeModule(Pool<IcedCoffeeCompiler> icedCoffeeCompilerPool)
        {
            IcedCoffeeCompilerPool = icedCoffeeCompilerPool;
        }

        public string Process(Uri jsRootPathUri, FileInfo file, string fileContents)
        {
            if (!_icedCoffeeExtensions.Contains(file.Extension.ToLower()))
            {
                return fileContents;
            }

            var compiler = IcedCoffeeCompilerPool.Acquire();
            try
            {
                var compiledJavascript = compiler.Compile(fileContents);
                return compiledJavascript;
            }
            finally
            {
                IcedCoffeeCompilerPool.Release(compiler);
            }
        }
    }
}