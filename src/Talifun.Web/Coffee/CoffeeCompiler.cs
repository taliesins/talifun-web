using System.Reflection;
using Talifun.Web.Javascript;

namespace Talifun.Web.Coffee
{
    public class CoffeeCompiler
    {
        private readonly IEmbeddedResourceLoader _embeddedResourceLoader;
        private readonly V8JavascriptRuntime _engine;

        public CoffeeCompiler(IEmbeddedResourceLoader embeddedResourceLoader)
        {
            _embeddedResourceLoader = embeddedResourceLoader;
            _engine = new V8JavascriptRuntime();
            LoadLibrary();
        }

        private void LoadLibrary()
        {
            var assemblyName = Assembly.GetExecutingAssembly();

            var coffeeScriptJs = _embeddedResourceLoader.LoadEmbeddedResource(assemblyName, "/Coffee/Resources/coffee-script.js");

            const string compileScriptMethod = @"
var CompileScript = function (code) {
    return CoffeeScript.compile(code)
};
";
            
            _engine.LoadLibrary(coffeeScriptJs);
            _engine.LoadLibrary(compileScriptMethod);
        }

        public string Compile(string script)
        {
            var compiledTemplate = _engine.ExecuteFunction<string>("CompileScript", new[] { script });
            return compiledTemplate;
        }
    }
}
