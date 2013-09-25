using NUnit.Framework;
using Talifun.Web.Hogan;

namespace Talifun.Web.Tests.Hogan
{
    [TestFixture]
    public class TemplateCompileTests
    {
        [Test]
        public void CompileHoganTemplate()
        {
            const string template = "Hello {{name}}";
            var embeddedResourceLoader = new EmbeddedResourceLoader();
            var hoganCompiler = new HoganCompiler(embeddedResourceLoader);

            const string expectedCompiledTemplate = @"{code: function (c,p,i) { var t=this;t.b(i=i||"""");t.b(""Hello "");t.b(t.v(t.f(""name"",c,p,0)));return t.fl(); },partials: {}, subs: {  }}";

            var compiledTemplate = hoganCompiler.Compile(template);

            Assert.AreEqual(expectedCompiledTemplate, compiledTemplate);
        }

        [Test]
        public void RenderHoganTemplate()
        {
            const string templateName = "helloWorld";
            const string template = "Hello {{name}}";
            var embeddedResourceLoader = new EmbeddedResourceLoader();
            var hoganCompiler = new HoganCompiler(embeddedResourceLoader);
            var compiledTemplate = hoganCompiler.Compile(template);
            hoganCompiler.AddTemplate(templateName, compiledTemplate);
            var model = new
            {
                name = "World"
            };

            const string expectedTemplateOuput = "Hello World";

            var templateOutput = hoganCompiler.Render(templateName, model);

            Assert.AreEqual(expectedTemplateOuput, templateOutput);
        }
    }
}
