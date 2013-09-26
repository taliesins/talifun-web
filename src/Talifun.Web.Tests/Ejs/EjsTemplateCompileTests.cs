using NUnit.Framework;
using Talifun.Web.Ejs;

namespace Talifun.Web.Tests.Ejs
{
    [TestFixture]
    public class EjsTemplateCompileTests
    {
        [Test]
        public void CompileEjsTemplate()
        {
            const string template = @"<ul>
<% for(var i=0; i<supplies.length; i++) {%>
   <li><%= supplies[i] %></li>
<% } %>
</ul>";
            var embeddedResourceLoader = new EmbeddedResourceLoader();
            var ejsCompiler = new EjsCompiler(embeddedResourceLoader);

            const string expectedCompiledTemplate = @"{code: function (c,p,i) { var t=this;t.b(i=i||"""");t.b(""Hello "");t.b(t.v(t.f(""name"",c,p,0)));return t.fl(); },partials: {}, subs: {  }}";

            var compiledTemplate = ejsCompiler.Compile(template);

            Assert.AreEqual(expectedCompiledTemplate, compiledTemplate);
        }

        [Test]
        public void RenderEjsTemplate()
        {
            const string templateName = "helloWorld";
            const string template = @"<ul>
<% for(var i=0; i<supplies.length; i++) {%>
   <li><%= supplies[i] %></li>
<% } %>
</ul>";
            var embeddedResourceLoader = new EmbeddedResourceLoader();
            var ejsCompiler = new EjsCompiler(embeddedResourceLoader);
            var compiledTemplate = ejsCompiler.Compile(template);
            ejsCompiler.AddTemplate(templateName, compiledTemplate);
            var model = new
            {
                title = "Cleaning Supplies",
                supplies = new[]{"mop", "broom", "duster"}
            };

            const string expectedTemplateOuput = "Hello World";

            var templateOutput = ejsCompiler.Render(templateName, model);

            Assert.AreEqual(expectedTemplateOuput, templateOutput);
        }
    }
}
