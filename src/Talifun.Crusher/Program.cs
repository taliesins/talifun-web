using System;
using System.Configuration;
using System.IO;
using System.Text;
using System.Threading;
using Talifun.Crusher.Options;
using Talifun.Web;
using Talifun.Web.Crusher;
using Talifun.Web.Crusher.Config;
using Talifun.Web.CssSprite;
using Talifun.Web.CssSprite.Config;
using Talifun.Web.Helper;

namespace Talifun.Crusher
{
    public class Program
    {
		private const int SuccessExitCode = 0;
    	private const int ErrorExitCode = 1;
    	private const int DisplayHelpScreenExitCode = 2;
        private const int BufferSize = 32768;
		private static readonly Encoding Encoding = Encoding.UTF8;

        private const string HeaderMessage = "Talifun.Crusher:";

        private const string UsageMessage = "Usage: Talifun.Crusher -c=../../../Talifun.Web.Examples/Crusher.Demo/web.config";
        private const string HelpMessage = "Help: Talifun.Crusher -?";

        static int Main(string[] args)
        {
            var showHelp = false;
            var configPath = string.Empty;
            var crusherSectionName = "Crusher";
			var cssSpriteSectionName = "CssSprite";
            var applicationPath = "/";

            var options = new OptionSet()
            {
                {
                    "c=|configPath=",
                    "the configuration path to the web.config or app.config file. E.g. ../../../Talifun.Web.Examples/Crusher.Demo/web.config",
                    c => configPath = c
                },
                {
                    "cs=|crusherSectionName=",
                    "the section name of the configuration element for the Talifun.Crusher configuration. Defaults to 'Crusher' if not specified.",
                    cs => crusherSectionName = cs 
                },
                {
                    "css=|cssSpriteSectionName=",
                    "the section name of the configuration element for the Talifun.CssSprite configuration. Defaults to 'CssSprite' if not specified.",
                    css => cssSpriteSectionName = css 
                },
                {
                    "a=|applicationPath=",
                    "the application path to be relative from. Defaults to  '/' if not specified.",
                    a => applicationPath = a
                },
                {
                    "?|h|help", 
                    "display help screen",
                    h => showHelp = h != null
                }
            };
                
            try
            {
                options.Parse(args);
            }
            catch(OptionException e)
            {
                Console.WriteLine(HeaderMessage);
                Console.WriteLine(e.Message);
                Console.WriteLine(UsageMessage);
                Console.WriteLine(HelpMessage);
				return DisplayHelpScreenExitCode;
            }

            if (showHelp)
            {
                DisplayHelp(options);
				return DisplayHelpScreenExitCode;
            }

            if (string.IsNullOrEmpty(configPath))
            {
                Console.WriteLine(HeaderMessage);
                Console.WriteLine(UsageMessage);
                Console.WriteLine(HelpMessage);
				return DisplayHelpScreenExitCode;
            }

            var crusherConfiguration = GetCrusherSection(configPath, crusherSectionName);
			var cssSpriteConfiguration = GetCssSpriteSection(configPath, cssSpriteSectionName);

			if (crusherConfiguration == null && cssSpriteConfiguration == null)
            {
                Console.WriteLine(HeaderMessage);
                Console.WriteLine("\"{0}\" section name not found in {1} ", cssSpriteSectionName, configPath);
				Console.WriteLine("\"{0}\" section name not found in {1} ", crusherSectionName, configPath);
                Console.WriteLine(HelpMessage);
				return DisplayHelpScreenExitCode;
            }

        	try
        	{
				Console.WriteLine();
				Console.WriteLine("Settings used:");
				Console.WriteLine("configPath = " + configPath);
                Console.WriteLine("cssSpriteSectionName = " + cssSpriteSectionName);
				Console.WriteLine("crusherSectionName = " + crusherSectionName);
				Console.WriteLine("applicationPath = " + applicationPath);

        	    var configUri = new Uri(configPath, UriKind.RelativeOrAbsolute);
                if (!configUri.IsAbsoluteUri)
                {
                    configUri = new Uri(Path.Combine(Environment.CurrentDirectory, configUri.ToString()));
                }

                var physicalApplicationPath = new FileInfo(configUri.LocalPath).DirectoryName;

				var retryableFileOpener = new RetryableFileOpener();
				var hasher = new Hasher(retryableFileOpener);
				var retryableFileWriter = new RetryableFileWriter(BufferSize, Encoding, retryableFileOpener, hasher);
				var pathProvider = new PathProvider(applicationPath, physicalApplicationPath);
				var cacheManager = new HttpCacheManager();

        	    var jsOutput = string.Empty;
        	    var cssOutput = string.Empty;
                var cssSpriteOutput = string.Empty;

                //We want to be able to use output from css sprites in crushed content
                var countdownEvents = new CountdownEvent(1);

                ThreadPool.QueueUserWorkItem(data =>
                {
                    var manualResetEvent = (CountdownEvent)data;
                    if (cssSpriteConfiguration != null)
                    {
                        var cssSpriteGroups = cssSpriteConfiguration.CssSpriteGroups;
                        var cssSpriteCreator = new CssSpriteCreator(cacheManager, retryableFileOpener, pathProvider, retryableFileWriter);
                        var cssSpriteGroupsProcessor = new CssSpriteGroupsProcessor();

                        cssSpriteOutput = cssSpriteGroupsProcessor.ProcessGroups(pathProvider, cssSpriteCreator, cssSpriteGroups).ToString();
                    }
                    manualResetEvent.Signal();
                }, countdownEvents);

        	    countdownEvents.Wait();

                countdownEvents = new CountdownEvent(2);

                ThreadPool.QueueUserWorkItem(data =>
                {
                    var manualResetEvent = (CountdownEvent)data;
                    if (crusherConfiguration != null)
                    {
                        var jsCrusher = new JsCrusher(cacheManager, pathProvider, retryableFileOpener, retryableFileWriter);
                        var jsGroups = crusherConfiguration.JsGroups;
                        var jsGroupsProcessor = new JsGroupsProcessor();

                        jsOutput = jsGroupsProcessor.ProcessGroups(pathProvider, jsCrusher, jsGroups).ToString();
                    }
                    manualResetEvent.Signal();
                }, countdownEvents);

                ThreadPool.QueueUserWorkItem(data =>
                {
                    var manualResetEvent = (CountdownEvent)data;
                    if (crusherConfiguration != null)
                    {
                        var hashQueryStringKeyName = crusherConfiguration.QuerystringKeyName;
                        var cssAssetsFileHasher = new CssAssetsFileHasher(hashQueryStringKeyName, hasher, pathProvider);
                        var cssPathRewriter = new CssPathRewriter(cssAssetsFileHasher, pathProvider);
                        var cssCrusher = new CssCrusher(cacheManager, pathProvider, retryableFileOpener, retryableFileWriter, cssPathRewriter);
                        var cssGroups = crusherConfiguration.CssGroups;
                        var cssGroupsCrusher = new CssGroupsProcessor();
                        cssOutput = cssGroupsCrusher.ProcessGroups(pathProvider, cssCrusher, cssGroups).ToString();
                    }
                    manualResetEvent.Signal();
                }, countdownEvents);

        	    countdownEvents.Wait();

                if (string.IsNullOrEmpty(cssSpriteOutput))
                {
                    Console.WriteLine();
                    Console.WriteLine("Skipping css sprite creation. \"{0}\" section name not found in \"{1}\"", cssSpriteSectionName, configPath);
                }
                else
                {
                    Console.WriteLine();
                    Console.WriteLine(cssSpriteOutput);
                }

        	    if (string.IsNullOrEmpty(jsOutput) && string.IsNullOrEmpty(cssOutput))
                {
                    Console.WriteLine();
                    Console.WriteLine("Skipping css/js crushed content creation. \"{0}\" section name not found in \"{1}\"", crusherSectionName, configPath);
                }
                else
                {
                    Console.WriteLine();
                    Console.WriteLine(cssOutput);
                    Console.WriteLine();
                    Console.WriteLine(jsOutput);
                }
			}
			catch (Exception exception)
			{
				Console.Write(exception);
				return ErrorExitCode;
			}

			return SuccessExitCode;
        }

        private static void DisplayHelp(OptionSet options)
        {
            Console.WriteLine(HeaderMessage);
            Console.WriteLine("Talifun.Crusher will crush js/css files and create css sprite images as specified in .config file.");
            Console.WriteLine();
			Console.WriteLine("Use this when you want to crush the js/css files and create css sprite images as part of your build process. Useful if you need to deploy to a CDN or you can't run the http module as it requires medium trust.");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine();
            options.WriteOptionDescriptions(Console.Out);
            Console.WriteLine(UsageMessage);
        }

        private static Talifun.Web.Crusher.Config.CrusherSection GetCrusherSection(string configPath, string sectionName)
        {
            var map = new ExeConfigurationFileMap
            {
                ExeConfigFilename = configPath
            };
            var config = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);
            var crusherSection = config.GetSection(sectionName) as CrusherSection;

            return crusherSection;
        }

		private static CssSpriteSection GetCssSpriteSection(string configPath, string sectionName)
		{
			var map = new ExeConfigurationFileMap
			{
				ExeConfigFilename = configPath
			};
			var config = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);
			var cssSpriteSection = config.GetSection(sectionName) as CssSpriteSection;

			return cssSpriteSection;
		}

    }
}
