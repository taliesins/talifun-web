using System.Reflection;
using Talifun.Crusher.Javascript;
using Talifun.Web;

namespace Talifun.Crusher.Hogan
{
    public class HoganCompiler
    {
        private readonly IEmbeddedResourceLoader _embeddedResourceLoader;
        private readonly V8JavascriptRuntime _engine;

        public HoganCompiler(IEmbeddedResourceLoader embeddedResourceLoader)
        {
            _embeddedResourceLoader = embeddedResourceLoader;
            _engine = new V8JavascriptRuntime();
            LoadLibrary();
        }

        private void LoadLibrary()
        {
            var assemblyName = Assembly.GetExecutingAssembly();

            var templateJs = _embeddedResourceLoader.LoadEmbeddedResource(assemblyName, "/Hogan/Resources/Template.js");
            var compilerJs = _embeddedResourceLoader.LoadEmbeddedResource(assemblyName, "/Hogan/Resources/Compiler.js");

            const string compileTemplateMethod = @"
var Templates = {};

var CompileTemplate = function (template) {
    return Hogan.compile(template, { asString: true });
};

var RenderTemplate = function(templateKey, templateModel) {
    var template = Templates[templateKey];
    return template.render(templateModel);
};
";

            _engine.LoadLibrary(templateJs);
            _engine.LoadLibrary(compilerJs);
            _engine.LoadLibrary(compileTemplateMethod);
        }

        public string Compile(string template)
        {
            var compiledTemplate = _engine.ExecuteFunction<string>("CompileTemplate", new[] { template });
            return compiledTemplate;
        }

        public void AddTemplate(string key, string compiledTemplate)
        {
            var addTemplate = string.Format("Templates['{0}']=new Hogan.Template ({1});", key, compiledTemplate);
            _engine.LoadLibrary(addTemplate);
        }

        public string Render<T>(string key, T model)
        {
            var view = _engine.ExecuteFunction<string>("RenderTemplate", new object[] { key, model });
            return view;
        }
    }
}
