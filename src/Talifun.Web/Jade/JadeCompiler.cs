using System.Reflection;
using Talifun.Web.Javascript;

namespace Talifun.Web.Jade
{
    public class JadeCompiler
    {
        private readonly IEmbeddedResourceLoader _embeddedResourceLoader;
        private readonly V8JavascriptRuntime _engine;

        public JadeCompiler(IEmbeddedResourceLoader embeddedResourceLoader)
        {
            _embeddedResourceLoader = embeddedResourceLoader;
            _engine = new V8JavascriptRuntime();
            LoadLibrary();
        }

        private void LoadLibrary()
        {
            var assemblyName = Assembly.GetExecutingAssembly();

            var compilerJs = _embeddedResourceLoader.LoadEmbeddedResource(assemblyName, "/Jade/Resources/jade.min.js");

            const string compileTemplateMethod = @"
var Templates = {};

var CompileTemplate = function (template) {
    return Jade.compile(template);
};

var RenderTemplate = function(templateKey, templateModel) {
    var template = Templates[templateKey];
    return template(templateModel);
};
";
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
            var addTemplate = string.Format("Templates['{0}']={1};", key, compiledTemplate);
            _engine.LoadLibrary(addTemplate);
        }

        public string Render<T>(string key, T model)
        {
            var view = _engine.ExecuteFunction<string>("RenderTemplate", new object[] { key, model });
            return view;
        }
    }
}
