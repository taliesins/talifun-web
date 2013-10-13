using System.Reflection;
using Talifun.Crusher.Javascript;
using Talifun.Web;

namespace Talifun.Crusher.LiveScript
{
    public class LiveScriptCompiler
    {
        private readonly IEmbeddedResourceLoader _embeddedResourceLoader;
        private readonly V8JavascriptRuntime _engine;

        public LiveScriptCompiler(IEmbeddedResourceLoader embeddedResourceLoader)
        {
            _embeddedResourceLoader = embeddedResourceLoader;
            _engine = new V8JavascriptRuntime();
            LoadLibrary();
        }

        private void LoadLibrary()
        {
            var assemblyName = Assembly.GetExecutingAssembly();

            var liveScriptJs = _embeddedResourceLoader.LoadEmbeddedResource(assemblyName, "/LiveScript/Resources/livescript.js");

            const string compileScriptMethod = @"
var CompileScript = function(code) {
    return LiveScript.compile(code)
};
";
            
            _engine.LoadLibrary(liveScriptJs);
            _engine.LoadLibrary(compileScriptMethod);
        }

        public string Compile(string script)
        {
            var compiledTemplate = _engine.ExecuteFunction<string>("CompileScript", new[] { script });
            return compiledTemplate;
        }
    }
}
