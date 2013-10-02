using System;
using System.IO;
using System.Linq;
using Talifun.Web.Helper.Pooling;
using Talifun.Web.LiveScript;

namespace Talifun.Web.Crusher.JsModule
{
    public class LiveScriptModule : IJsModule
    {
        private readonly string[] _liveScriptExtensions = { ".ls", ".ls.js" };

        protected readonly Pool<LiveScriptCompiler> LiveScriptCompilerPool;

        public LiveScriptModule(Pool<LiveScriptCompiler> liveScriptCompilerPool)
        {
            LiveScriptCompilerPool = liveScriptCompilerPool;
        }

        public string Process(Uri jsRootPathUri, FileInfo file, string fileContents)
        {
            if (!_liveScriptExtensions.Contains(file.Extension.ToLower()))
            {
                return fileContents;
            }

            var compiler = LiveScriptCompilerPool.Acquire();
            try
            {
                var compiledJavascript = compiler.Compile(fileContents);
                return compiledJavascript;
            }
            finally
            {
                LiveScriptCompilerPool.Release(compiler);
            }
        }
    }
}