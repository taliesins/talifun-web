﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using Talifun.Crusher.Options;
using Talifun.Web;
using Talifun.Web.Crusher;
using Talifun.Web.Crusher.Config;
using Talifun.Web.CssSprite;
using Talifun.Web.CssSprite.Config;
using Talifun.Web.Helper;

namespace Talifun.Crusher
{
    class Program
    {
		private const int SuccessExitCode = 0;
    	private const int ErrorExitCode = 1;
    	private const int DisplayHelpScreenExitCode = 2;
        private const int BufferSize = 32768;

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
				Console.WriteLine("\"{0}\" section name not found in {1} ", crusherSectionName, configPath);
				Console.WriteLine("\"{0}\" section name not found in {1} ", cssSpriteSectionName, configPath);
                Console.WriteLine(HelpMessage);
				return DisplayHelpScreenExitCode;
            }

        	try
        	{
				Console.WriteLine();
				Console.WriteLine("Settings used:");
				Console.WriteLine("configPath = " + configPath);
				Console.WriteLine("crusherSectionName = " + crusherSectionName);
				Console.WriteLine("cssSpriteSectionName = " + cssSpriteSectionName);
				Console.WriteLine("applicationPath = " + applicationPath);

				var physicalApplicationPath = new FileInfo(configPath).DirectoryName;
				var retryableFileOpener = new RetryableFileOpener();
				var hasher = new Hasher(retryableFileOpener);
				var retryableFileWriter = new RetryableFileWriter(BufferSize, retryableFileOpener, hasher);
				var pathProvider = new PathProvider(applicationPath, physicalApplicationPath);
				var cacheManager = new HttpCacheManager();

				if (crusherConfiguration == null)
				{
					Console.WriteLine();
					Console.WriteLine("Skipping css/js crushed content creation. \"{0}\" section name not found in \"{1}\"", crusherSectionName, configPath);
				}
				else
				{
					var hashQueryStringKeyName = crusherConfiguration.QuerystringKeyName;
					var cssAssetsFileHasher = new CssAssetsFileHasher(hashQueryStringKeyName, hasher, pathProvider);
					var cssPathRewriter = new CssPathRewriter(cssAssetsFileHasher, pathProvider);
					var cssCrusher = new CssCrusher(cacheManager, pathProvider, retryableFileOpener, retryableFileWriter, cssPathRewriter);
					var jsCrusher = new JsCrusher(cacheManager, pathProvider, retryableFileOpener, retryableFileWriter);

					var cssGroups = crusherConfiguration.CssGroups;
					var jsGroups = crusherConfiguration.JsGroups;
					CreateCrushedFiles(pathProvider, cssGroups, jsGroups, cssCrusher, jsCrusher);

					Console.WriteLine();
					Console.WriteLine(_cssOutput);
					Console.WriteLine();
					Console.WriteLine(_jsOutput);
				}

				if (cssSpriteConfiguration == null)
				{
					Console.WriteLine();
					Console.WriteLine("Skipping css sprite creation. \"{0}\" section name not found in \"{1}\"", cssSpriteSectionName, configPath);
				}
				else
				{
					var cssSpriteGroups = cssSpriteConfiguration.CssSpriteGroups;
					var cssSpriteCreator = new CssSpriteCreator(cacheManager, retryableFileOpener, pathProvider, retryableFileWriter);
					CreateCssSpriteFiles(pathProvider, cssSpriteGroups, cssSpriteCreator);

					Console.WriteLine();
					Console.WriteLine(_cssSpriteOutput);
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

                var directories = new List<CssDirectory>();

                foreach (CssDirectoryElement cssDirectory in group.Directories)
                {
                    var directory = new CssDirectory()
                    {
                        CompressionType = cssDirectory.CompressionType,
                        DirectoryPath = cssDirectory.DirectoryPath,
                        IncludeSubDirectories = cssDirectory.IncludeSubDirectories,
                        PollTime = cssDirectory.PollTime,
                        IncludeFilter = cssDirectory.IncludeFilter,
                        ExcludeFilter = cssDirectory.ExcludeFilter
                    };
                    directories.Add(directory);
                }

                var outputUri = new Uri(pathProvider.ToAbsolute(group.OutputFilePath), UriKind.Relative);
                var output = cssCrusher.CreateGroup(outputUri, files, directories, group.AppendHashToCssAsset);
                
                _cssOutput += outputUri + " (" + group.Name + ")\r\n";
                _cssOutput += outputUri + "   (Css)\r\n";
                foreach (var cssFile in output.FilesToWatch)
                {
                    outputUri = new Uri(pathProvider.ToAbsolute(cssFile.FilePath), UriKind.Relative);
                    _cssOutput += "      " + outputUri + "\r\n";
                }
                _cssOutput += outputUri + "   (Css Assets)\r\n";
                foreach (var cssAssetFile in output.CssAssetFilePaths)
                {
                    outputUri = new Uri(pathProvider.ToAbsolute(cssAssetFile.FullName), UriKind.Relative);
                    _cssOutput += "      " + outputUri + "\r\n";
                }
            }

            _jsOutput = "Js Files created:\r\n";
            foreach (JsGroupElement group in jsGroups)
            {
                var files = new List<JsFile>();

                foreach (JsFileElement jsFile in group.Files)
                {
                    var file = new JsFile()
                    {
                        CompressionType = jsFile.CompressionType,
                        FilePath = jsFile.FilePath
                    };
                    files.Add(file);
                }

                var directories = new List<JsDirectory>();

                foreach (JsDirectoryElement jsDirectory in group.Directories)
                {
                    var directory = new JsDirectory()
                    {
                        CompressionType = jsDirectory.CompressionType,
                        DirectoryPath = jsDirectory.DirectoryPath,
                        IncludeSubDirectories = jsDirectory.IncludeSubDirectories,
                        PollTime = jsDirectory.PollTime,
                        IncludeFilter = jsDirectory.IncludeFilter,
                        ExcludeFilter = jsDirectory.ExcludeFilter
                    };
                    directories.Add(directory);
                }

                var outputUri = new Uri(pathProvider.ToAbsolute(group.OutputFilePath), UriKind.Relative);
                var output = jsCrusher.AddGroup(outputUri, files, directories);

                _jsOutput += outputUri + " (" + group.Name + ")\r\n";
                foreach (var jsFile in output.FilesToWatch)
                {
                    outputUri = new Uri(pathProvider.ToAbsolute(jsFile.FilePath), UriKind.Relative);
                    _jsOutput += "    " + outputUri + "\r\n";
                }
            }
        }

		private static string _cssSpriteOutput;
		private static void CreateCssSpriteFiles(IPathProvider pathProvider, CssSpriteGroupElementCollection cssSpriteGroups, CssSpriteCreator cssSpriteCreator)
		{
			_cssSpriteOutput = "Css Sprite Files created:\r\n";
			foreach (CssSpriteGroupElement group in cssSpriteGroups)
			{
				var files = new List<ImageFile>();

				foreach (ImageFileElement imageFile in group.Files)
				{
					var file = new ImageFile()
					{
						FilePath = imageFile.FilePath,
						Name = imageFile.Name
					};
					files.Add(file);
				}

                var directories = new List<ImageDirectory>();

                foreach (ImageDirectoryElement imageDirectory in group.Directories)
                {
                    var directory = new ImageDirectory()
                    {
                        DirectoryPath = imageDirectory.DirectoryPath,
                        ExcludeFilter = imageDirectory.ExcludeFilter,
                        IncludeFilter = imageDirectory.IncludeFilter,
                        IncludeSubDirectories = imageDirectory.IncludeSubDirectories,
                        PollTime = imageDirectory.PollTime
                    };

                    directories.Add(directory);
                }

				var cssOutPutUri = string.IsNullOrEmpty(group.CssUrl) ? new Uri(pathProvider.ToAbsolute(group.CssOutputFilePath), UriKind.Relative) : new Uri(group.CssUrl, UriKind.RelativeOrAbsolute);
				var cssOutputPath = new FileInfo(pathProvider.MapPath(group.CssOutputFilePath));

				var imageOutputUri = string.IsNullOrEmpty(group.ImageUrl) ? new Uri(pathProvider.ToAbsolute(group.ImageOutputFilePath), UriKind.Relative) : new Uri(group.ImageUrl, UriKind.RelativeOrAbsolute);
				var imageOutputPath = new FileInfo(pathProvider.MapPath(group.ImageOutputFilePath));

                var output = cssSpriteCreator.AddFiles(imageOutputPath, imageOutputUri, cssOutputPath, files, directories);

				_cssSpriteOutput += cssOutPutUri + "(" + group.Name + ")\r\n";
				_cssSpriteOutput += imageOutputUri + "(" + group.Name + ")\r\n";
                foreach (var filesToWatch in output)
				{
                    imageOutputUri = new Uri(pathProvider.ToAbsolute(filesToWatch.FullName), UriKind.Relative);
					_cssSpriteOutput += "    " + imageOutputUri + "\r\n";
				}
			}
		}
    }
}
