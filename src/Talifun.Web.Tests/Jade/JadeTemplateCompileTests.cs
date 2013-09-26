using NUnit.Framework;
using Talifun.Web.Jade;

namespace Talifun.Web.Tests.Jade
{
    [TestFixture]
    public class JadeTemplateCompileTests
    {
        [Test]
        public void CompileJadeTemplate()
        {
            const string template = @"doctype 5
html(lang=""en"")
  head
    title= pageTitle
    script(type='text/javascript').
      if (foo) {
         bar(1 + 5)
      }
  body
    h1 Jade - node template engine
    #container.col
      if youAreUsingJade
        p You are amazing
      else
        p Get on it!
      p.
        Jade is a terse and simple
        templating language with a
        strong focus on performance
        and powerful features.";
            var embeddedResourceLoader = new EmbeddedResourceLoader();
            var jadeCompiler = new JadeCompiler(embeddedResourceLoader);

            const string expectedCompiledTemplate = @"";

            var compiledTemplate = jadeCompiler.Compile(template);

            Assert.AreEqual(expectedCompiledTemplate, compiledTemplate);
        }

        [Test]
        public void RenderJadeTemplate()
        {
            const string templateName = "helloWorld";
            const string template = @"doctype 5
html(lang=""en"")
  head
    title= pageTitle
    script(type='text/javascript').
      if (foo) {
         bar(1 + 5)
      }
  body
    h1 Jade - node template engine
    #container.col
      if youAreUsingJade
        p You are amazing
      else
        p Get on it!
      p.
        Jade is a terse and simple
        templating language with a
        strong focus on performance
        and powerful features.";
            var embeddedResourceLoader = new EmbeddedResourceLoader();
            var jadeCompiler = new JadeCompiler(embeddedResourceLoader);
            var compiledTemplate = jadeCompiler.Compile(template);
            jadeCompiler.AddTemplate(templateName, compiledTemplate);
            var model = new
            {
                name = "World"
            };

            const string expectedTemplateOuput = "Hello World";

            var templateOutput = jadeCompiler.Render(templateName, model);

            Assert.AreEqual(expectedTemplateOuput, templateOutput);
        }
    }
}
