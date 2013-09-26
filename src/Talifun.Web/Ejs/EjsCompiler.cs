using System.Reflection;
using Talifun.Web.Javascript;

namespace Talifun.Web.Ejs
{
    public class EjsCompiler
    {
        private readonly IEmbeddedResourceLoader _embeddedResourceLoader;
        private readonly V8JavascriptRuntime _engine;

        public EjsCompiler(IEmbeddedResourceLoader embeddedResourceLoader)
        {
            _embeddedResourceLoader = embeddedResourceLoader;
            _engine = new V8JavascriptRuntime();
            LoadLibrary();
        }

        private void LoadLibrary()
        {
            var assemblyName = Assembly.GetExecutingAssembly();

            var templateJs = _embeddedResourceLoader.LoadEmbeddedResource(assemblyName, "/Ejs/Resources/ejs.min.js");

            const string compileTemplateMethod = @"
var Templates = {};

var CompileTemplate = function (template) {
    var template = ejs.compile(template,{ open: '{{', close: '}}', debug: false });
    return template;
};

var RenderTemplate = function(templateKey, templateModel) {
    var template = Templates[templateKey];
    return template(templateModel);
};
";

            _engine.LoadLibrary(templateJs);
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
