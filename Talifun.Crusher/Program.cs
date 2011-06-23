using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using Talifun.Crusher.Options;
using Talifun.Web;
using Talifun.Web.Crusher;
using Talifun.Web.Crusher.Config;
using Talifun.Web.Helper;

namespace Talifun.Crusher
{
    class Program
    {
        private const int BufferSize = 32768;

        private const string HeaderMessage = "Talifun.Crusher:";

        private const string UsageMessage = "Usage: Talifun.Crusher -c=../../../Talifun.Web.Examples/Crusher.Demo/web.config";
        private const string HelpMessage = "Help: Talifun.Crusher -?";


        static void Main(string[] args)
        {
            var showHelp = false;
            var configPath = string.Empty;
            var sectionName = "Crusher";
            var applicationPath = "/";

            var options = new OptionSet()
            {
                {
                    "c=|configPath=",
                    "the configuration path to the web.config or app.config file. E.g. ../../../Talifun.Web.Examples/Crusher.Demo/web.config",
                    c => configPath = c
                },
                {
                    "s=|sectionName=",
                    "the section name of the configuration element for the Talifun.Crusher configuration. Defaults to 'Crusher' if not specified.",
                    s => sectionName = s 
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
                return;
            }

            if (showHelp)
            {
                DisplayHelp(options);
                return;
            }

            if (string.IsNullOrEmpty(configPath))
            {
                Console.WriteLine(HeaderMessage);
                Console.WriteLine(UsageMessage);
                Console.WriteLine(HelpMessage);
                return;
            }

            var crusherConfiguration = GetCrusherSection(configPath, sectionName);;

            if (crusherConfiguration == null)
            {
                Console.WriteLine(HeaderMessage);
                Console.WriteLine(sectionName + " section name not found in " + configPath);
                Console.WriteLine(HelpMessage);
                return;
            }
            
            var physicalApplicationPath = new FileInfo(configPath).DirectoryName;
            var cssGroups = crusherConfiguration.CssGroups;
            var jsGroups = crusherConfiguration.JsGroups;

            var retryableFileOpener = new RetryableFileOpener();
            var hasher = new Hasher(retryableFileOpener);
            var retryableFileWriter = new RetryableFileWriter(BufferSize, retryableFileOpener, hasher);
            var pathProvider = new PathProvider(applicationPath, physicalApplicationPath);
            var hashQueryStringKeyName = crusherConfiguration.QuerystringKeyName;
            var cssAssetsFileHasher = new CssAssetsFileHasher(hashQueryStringKeyName, hasher, pathProvider);
            var cssPathRewriter = new CssPathRewriter(cssAssetsFileHasher, pathProvider);

            var cacheManager = new HttpCacheManager();
            var cssCrusher = new CssCrusher(cacheManager, pathProvider, retryableFileOpener, retryableFileWriter, cssPathRewriter);
            var jsCrusher = new JsCrusher(cacheManager, pathProvider, retryableFileOpener, retryableFileWriter);

            CreateCrushedFiles(pathProvider, cssGroups, jsGroups, cssCrusher, jsCrusher);
            Console.WriteLine();
            Console.WriteLine("Settings used:");
            Console.WriteLine("configPath = " + configPath);
            Console.WriteLine("sectionName = " + sectionName);
            Console.WriteLine("applicationPath = " + applicationPath);
            Console.WriteLine();
            Console.WriteLine(_cssOutput);
            Console.WriteLine();
            Console.WriteLine(_jsOutput);
        }

        private static void DisplayHelp(OptionSet options)
        {
            Console.WriteLine(HeaderMessage);
            Console.WriteLine("Talifun.Crusher will crush js and css files as specified in .config file.");
            Console.WriteLine();
            Console.WriteLine("Use this when you want to crush the js and css files as part of your build process. Useful if you need to deploy to a CDN or you can't run the http module as it requires medium trust.");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine();
            options.WriteOptionDescriptions(Console.Out);
            Console.WriteLine(UsageMessage);
        }

        private static CrusherSection GetCrusherSection(string configPath, string sectionName)
        {
            var map = new ExeConfigurationFileMap
                {
                    ExeConfigFilename = configPath
                };
            var config = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);
            var crusherSection = config.GetSection(sectionName) as CrusherSection;

            return crusherSection;
        }

        private static string _cssOutput;
        private static string _jsOutput;
        private static void CreateCrushedFiles(IPathProvider pathProvider, CssGroupElementCollection cssGroups, JsGroupElementCollection jsGroups, CssCrusher cssCrusher, JsCrusher jsCrusher)
        {
            _cssOutput = "Css Files created:\r\n";
            foreach (CssGroupElement group in cssGroups)
            {
                var files = new List<CssFile>();

                foreach (CssFileElement cssFile in group.Files)
                {
                    var file = new CssFile()
                    {
                        CompressionType = cssFile.CompressionType,
                        FilePath = cssFile.FilePath
                    };
                    files.Add(file);
                }

                var outputUri = new Uri(pathProvider.ToAbsolute(group.OutputFilePath), UriKind.Relative);
                cssCrusher.AddFiles(outputUri, files, group.AppendHashToCssAsset);

                _cssOutput += outputUri + " (" + group.Name + ")\r\n";
                foreach (var cssFile in files)
                {
                    outputUri = new Uri(pathProvider.ToAbsolute(cssFile.FilePath), UriKind.Relative);
                    _cssOutput += "    " + outputUri + "\r\n";
                }
            }

            _jsOutput = "Js Files created:\r\n";
            foreach (JsGroupElement group in jsGroups)
            {
                var files = new List<JsFile>();

                foreach (JsFileElement cssFile in group.Files)
                {
                    var file = new JsFile()
                    {
                        CompressionType = cssFile.CompressionType,
                        FilePath = cssFile.FilePath
                    };
                    files.Add(file);
                }

                var outputUri = new Uri(pathProvider.ToAbsolute(group.OutputFilePath), UriKind.Relative);
                jsCrusher.AddFiles(outputUri, files);

                _jsOutput += outputUri + " (" + group.Name + ")\r\n";
                foreach (var jsFile in files)
                {
                    outputUri = new Uri(pathProvider.ToAbsolute(jsFile.FilePath), UriKind.Relative);
                    _jsOutput += "    " + outputUri + "\r\n";
                }
            }
        }
    }
}
