using NUnit.Framework;
using Talifun.Web.Hogan;

namespace Talifun.Web.Tests.Hogan
{
    [TestFixture]
    public class HoganTemplateCompileTests
    {
        [Test]
        public void CompileHoganTemplate()
        {
            const string template = @"<div class=""entry"">
  <h1>{{title}}</h1>

  {{#author}}
  <h2>By {{firstName}} {{lastName}}</h2>
  {{/author}}
</div>";
            var embeddedResourceLoader = new EmbeddedResourceLoader();
            var hoganCompiler = new HoganCompiler(embeddedResourceLoader);

            const string expectedCompiledTemplate = @"{code: function (c,p,i) { var t=this;t.b(i=i||"""");t.b(""<div class=\""entry\"">\r"");t.b(""\n"" + i);t.b(""  <h1>"");t.b(t.v(t.f(""title"",c,p,0)));t.b(""</h1>\r"");t.b(""\n"" + i);t.b(""\r"");t.b(""\n"" + i);if(t.s(t.f(""author"",c,p,1),c,p,0,58,104,""{{ }}"")){t.rs(c,p,function(c,p,t){t.b(""  <h2>By "");t.b(t.v(t.f(""firstName"",c,p,0)));t.b("" "");t.b(t.v(t.f(""lastName"",c,p,0)));t.b(""</h2>\r"");t.b(""\n"" + i);});c.pop();}t.b(""</div>"");return t.fl(); },partials: {}, subs: {  }}";

            var compiledTemplate = hoganCompiler.Compile(template);

            Assert.AreEqual(expectedCompiledTemplate, compiledTemplate);
        }

        [Test]
        public void RenderHoganTemplate()
        {
            const string template = @"<div class=""entry"">
  <h1>{{title}}</h1>

  {{#author}}
  <h2>By {{firstName}} {{lastName}}</h2>
  {{/author}}
</div>";
            const string templateName = "Authors.mustache";
            var embeddedResourceLoader = new EmbeddedResourceLoader();
            var hoganCompiler = new HoganCompiler(embeddedResourceLoader);
            var compiledTemplate = hoganCompiler.Compile(template);
            hoganCompiler.AddTemplate(templateName, compiledTemplate);
            var model = new
            {
                title = "The Title",
                author = new []
                {
                    new
                    {
                        firstName = "FirstName",
                        lastName = "LastName"
                    },
                                        new
                    {
                        firstName = "2ndFirstName",
                        lastName = "2ndLastName"
                    },
                }
            };

            const string expectedTemplateOuput = @"<div class=""entry"">
  <h1>The Title</h1>

  <h2>By FirstName LastName</h2>
  <h2>By 2ndFirstName 2ndLastName</h2>
</div>";

            var templateOutput = hoganCompiler.Render(templateName, model);

            Assert.AreEqual(expectedTemplateOuput, templateOutput);
        }
    }
}
