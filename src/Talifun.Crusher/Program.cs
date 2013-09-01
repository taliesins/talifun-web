using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Talifun.Crusher.Options;
using Talifun.Web;
using Talifun.Web.Crusher;
using Talifun.Web.Crusher.Config;
using Talifun.Web.CssSprite;
using Talifun.Web.CssSprite.Config;
using Talifun.Web.Helper;
using AggregateException = Talifun.Web.Crusher.AggregateException;

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

        private const string UsageMessage = "Usage: Talifun.Crusher -c=../../../../Talifun.Web.Examples/Crusher.Demo/web.config";
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
				var hasher = new Md5Hasher(retryableFileOpener);
				var retryableFileWriter = new RetryableFileWriter(BufferSize, Encoding, retryableFileOpener, hasher);
				var pathProvider = new PathProvider(applicationPath, physicalApplicationPath);
				var cacheManager = new HttpCacheManager();

                var cssSpriteSpriteMetaDataFileInfo = new FileInfo("cssSprite.metadata");
                var cssSpriteMetaData = new SingleFileMetaData(cssSpriteSpriteMetaDataFileInfo, retryableFileOpener, retryableFileWriter);

                var jsSpriteMetaDataFileInfo = new FileInfo("js.metadata");
                var jsMetaData = new SingleFileMetaData(jsSpriteMetaDataFileInfo, retryableFileOpener, retryableFileWriter);

                var cssSpriteMetaDataFileInfo = new FileInfo("css.metadata");
                var cssMetaData = new SingleFileMetaData(cssSpriteMetaDataFileInfo, retryableFileOpener, retryableFileWriter);

        	    var jsOutput = string.Empty;
        	    var cssOutput = string.Empty;
                var cssSpriteOutput = string.Empty;

                
                //We want to be able to use output from css sprites in crushed content
                var countdownEvents = new CountdownEvent(1);

                var cssSpriteExceptions = new List<CssSpriteException>();
                ThreadPool.QueueUserWorkItem(data =>
                {
                    var manualResetEvent = (CountdownEvent)data;
                    try
                    {
                        if (cssSpriteConfiguration != null)
                        {
                            var cssSpriteGroups = cssSpriteConfiguration.CssSpriteGroups;
                            var cssSpriteCreator = new CssSpriteCreator(cacheManager, retryableFileOpener, pathProvider, retryableFileWriter, cssSpriteMetaData);
                            var cssSpriteGroupsProcessor = new CssSpriteGroupsProcessor();

                            cssSpriteOutput = cssSpriteGroupsProcessor.ProcessGroups(pathProvider, cssSpriteCreator, cssSpriteGroups).ToString();
                        }
                    }
                    catch (Exception exception)
                    {
                        cssSpriteExceptions.Add(new CssSpriteException(exception));
                    }
                    manualResetEvent.Signal();
                }, countdownEvents);

        	    countdownEvents.Wait();

                countdownEvents = new CountdownEvent(2);
                var jsExceptions = new List<JsException>();
                ThreadPool.QueueUserWorkItem(data =>
                {
                    var manualResetEvent = (CountdownEvent)data;
                    try
                    {
                        if (crusherConfiguration != null)
                        {
                            var jsCrusher = new JsCrusher(cacheManager, pathProvider, retryableFileOpener, retryableFileWriter, jsMetaData);
                            var jsGroups = crusherConfiguration.JsGroups;
                            var jsGroupsProcessor = new JsGroupsProcessor();

                            jsOutput = jsGroupsProcessor.ProcessGroups(pathProvider, jsCrusher, jsGroups).ToString();
                        }
                    }
                    catch (Exception exception)
                    {
                        jsExceptions.Add(new JsException(exception));
                    }
                    manualResetEvent.Signal();
                }, countdownEvents);

                var cssExceptions = new List<CssException>();
                ThreadPool.QueueUserWorkItem(data =>
                {
                    var manualResetEvent = (CountdownEvent)data;
                    try
                    {
                        if (crusherConfiguration != null)
                        {
                            var hashQueryStringKeyName = crusherConfiguration.QuerystringKeyName;
                            var cssAssetsFileHasher = new CssAssetsFileHasher(hashQueryStringKeyName, hasher, pathProvider);
                            var cssPathRewriter = new CssPathRewriter(cssAssetsFileHasher, pathProvider);
                            var cssCrusher = new CssCrusher(cacheManager, pathProvider, retryableFileOpener, retryableFileWriter, cssPathRewriter, cssMetaData, crusherConfiguration.WatchAssets);
                            var cssGroups = crusherConfiguration.CssGroups;
                            var cssGroupsCrusher = new CssGroupsProcessor();
                            cssOutput = cssGroupsCrusher.ProcessGroups(pathProvider, cssCrusher, cssGroups).ToString();
                        }
                    }
                    catch (Exception exception)
                    {
                        cssExceptions.Add(new CssException(exception));
                    }
                    manualResetEvent.Signal();
                }, countdownEvents);

        	    countdownEvents.Wait();

                if (string.IsNullOrEmpty(cssSpriteOutput) && !cssSpriteExceptions.Any())
                {
                    Console.WriteLine();
                    Console.WriteLine("Skipping css sprite creation. \"{0}\" section name not found in \"{1}\"", cssSpriteSectionName, configPath);
                }
                else
                {
                    Console.WriteLine();
                    Console.WriteLine(cssSpriteOutput);

                    if (cssSpriteExceptions.Any())
                    {
                        Console.WriteLine("Css sprite errors:");
                        Console.WriteLine(new AggregateException(cssSpriteExceptions.Cast<Exception>()));
                    }
                }

                if (string.IsNullOrEmpty(jsOutput) && string.IsNullOrEmpty(cssOutput) && !jsExceptions.Any() && !cssExceptions.Any())
                {
                    Console.WriteLine();
                    Console.WriteLine("Skipping css/js crushed content creation. \"{0}\" section name not found in \"{1}\"", crusherSectionName, configPath);
                }
                else
                {
                    Console.WriteLine();
                    Console.WriteLine(cssOutput);
                    if (cssExceptions.Any())
                    {
                        Console.WriteLine("Css errors:");
                        Console.WriteLine(new AggregateException(cssExceptions.Cast<Exception>()));
                    }

                    Console.WriteLine();
                    Console.WriteLine(jsOutput);
                    if (jsExceptions.Any())
                    {
                        Console.WriteLine("Js errors:");
                        Console.WriteLine(new AggregateException(jsExceptions.Cast<Exception>()));
                    }
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
