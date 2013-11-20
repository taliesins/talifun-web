using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text;
using System.Threading;
using Talifun.Crusher.Configuration;
using Talifun.Crusher.Configuration.Css;
using Talifun.Crusher.Configuration.Js;
using Talifun.Crusher.Configuration.Sprites;
using Talifun.Crusher.Crusher;
using Talifun.Crusher.CssSprite;
using Talifun.Web;
using Talifun.Web.Helper;

namespace Talifun.Crusher.MsBuild
{
    [Serializable]
    public class CrusherMsBuildCommand : MarshalByRefObject, ICloneable //IClonable is marker interface so we can use msbuild with different app domain
    {
        private readonly string _applicationPath;
        private readonly string _binDirectoryPath;
        private readonly string _configPath;
        private readonly Action<string> _logMessage;
        private readonly Action<string> _logError;
        private readonly Action<string[]> _setOutputFilePaths;
        private readonly Action<string[]> _setOutputFileRelativePaths;
        
        private const string CrusherSectionName = "Crusher";
        private const int BufferSize = 32768;
        private static readonly Encoding Encoding = Encoding.UTF8;
        private readonly IRetryableFileOpener _retryableFileOpener;
        private readonly IHasher _hasher;
        private readonly IRetryableFileWriter _retryableFileWriter;
        private readonly IMetaData _fileMetaData;
        private readonly IPathProvider _pathProvider;
        private readonly ICacheManager _cacheManager;
        private readonly CrusherSection _crusherConfiguration;

        public CrusherMsBuildCommand(string applicationPath, string binDirectoryPath, string configPath, Action<string> logMessage, Action<string> logError, Action<string[]> setOutputFilePaths, Action<string[]> setOutputFileRelativePaths)
        {
            if (string.IsNullOrEmpty(applicationPath))
            {
                throw new ArgumentNullException("applicationPath");
            }

            if (string.IsNullOrEmpty(binDirectoryPath))
            {
                throw new ArgumentNullException("binDirectoryPath");
            }

            if (string.IsNullOrEmpty(configPath))
            {
                throw new ArgumentNullException("configPath");
            }

            if (logMessage == null)
            {
                throw new ArgumentNullException("logMessage");
            }

            if (logError == null)
            {
                throw new ArgumentNullException("logError");
            }

            if (setOutputFilePaths == null)
            {
                throw new ArgumentNullException("setOutputFilePaths");
            }

            if (setOutputFileRelativePaths == null)
            {
                throw new ArgumentNullException("setOutputFileRelativePaths");
            }

            _applicationPath = applicationPath;
            _binDirectoryPath = binDirectoryPath;
            _configPath = configPath;
            _logMessage = logMessage;
            _logError = logError;
            _setOutputFilePaths = setOutputFilePaths;
            _setOutputFileRelativePaths = setOutputFileRelativePaths;

            _retryableFileOpener = new RetryableFileOpener();
            _hasher = new Md5Hasher(_retryableFileOpener);
            _retryableFileWriter = new RetryableFileWriter(BufferSize, Encoding, _retryableFileOpener, _hasher);
            _fileMetaData = new MultiFileMetaData(_retryableFileOpener, _retryableFileWriter);

            _crusherConfiguration = GetCrusherSection(_configPath, CrusherSectionName);

            var configUri = new Uri(_configPath, UriKind.RelativeOrAbsolute);
            if (!configUri.IsAbsoluteUri)
            {
                configUri = new Uri(Path.Combine(Environment.CurrentDirectory, configUri.ToString()));
            }

            var physicalApplicationPath = new FileInfo(configUri.LocalPath).DirectoryName;
            _pathProvider = new PathProvider(_applicationPath, physicalApplicationPath);
            _cacheManager = new HttpCacheManager();
        }

        private CrusherSection GetCrusherSection(string configPath, string sectionName)
        {
            var map = new ExeConfigurationFileMap
            {
                ExeConfigFilename = configPath
            };
            var config = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);
            var crusherSection = config.GetSection(sectionName) as CrusherSection;

            return crusherSection;
        }

        private string MakeRelative(string uri)
        {
            if (uri.StartsWith("~"))
            {
                uri = uri.Substring(1);
            }

            if (uri.StartsWith("/"))
            {
                uri = uri.Substring(1);
            }
            return uri;
        }

        public object Clone()
        {
            var filePaths = new List<string>();
            var fileRelativePaths = new List<string>();

            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            try
            {
                var cssSpriteOutput = string.Empty;
                var jsOutput = string.Empty;
                var cssOutput = string.Empty;
                
                var countdownEvents = new CountdownEvent(3);

                ThreadPool.QueueUserWorkItem(data =>
                    {
                        var countdownEvent = (CountdownEvent) data;
                        try
                        {
                            if (_crusherConfiguration != null)
                            {
                                var cssSpriteGroups = _crusherConfiguration.CssSpriteGroups;
                                var cssSpriteCreator = new CssSpriteCreator(_cacheManager, _retryableFileOpener, _pathProvider, _retryableFileWriter, _fileMetaData);
                                var cssSpriteGroupsProcessor = new CssSpriteGroupsProcessor();

                                cssSpriteOutput = cssSpriteGroupsProcessor.ProcessGroups(_pathProvider, cssSpriteCreator, cssSpriteGroups).ToString();

                                var cssFilePaths = new List<string>();
                                var cssFileRelativePaths = new List<string>();
                                foreach (CssSpriteGroupElement cssSpriteGroup in cssSpriteGroups)
                                {
                                    cssFilePaths.Add(new Uri(_pathProvider.MapPath(cssSpriteGroup.CssOutputFilePath)).LocalPath);
                                    cssFilePaths.Add(new Uri(_pathProvider.MapPath(cssSpriteGroup.ImageOutputFilePath)).LocalPath);

                                    cssFileRelativePaths.Add(MakeRelative(cssSpriteGroup.CssOutputFilePath));
                                    cssFileRelativePaths.Add(MakeRelative(cssSpriteGroup.ImageOutputFilePath));
                                }

                                filePaths.AddRange(cssFilePaths);
                                fileRelativePaths.AddRange(cssFileRelativePaths);

                                _logMessage(cssSpriteOutput);
                            }
                        }
                        catch (Exception exception)
                        {
                            _logError(exception.ToString());
                        }
                        countdownEvent.Signal();
                    }, countdownEvents);

                ThreadPool.QueueUserWorkItem(data =>
                    {
                        var countdownEvent = (CountdownEvent) data;

                        try
                        {
                            if (_crusherConfiguration != null)
                            {
                                var jsCrusher = new JsCrusher(_cacheManager, _pathProvider, _retryableFileOpener, _retryableFileWriter, _fileMetaData);
                                var jsGroups = _crusherConfiguration.JsGroups;
                                var jsGroupsProcessor = new JsGroupsProcessor();
                                jsOutput = jsGroupsProcessor.ProcessGroups(_pathProvider, jsCrusher, jsGroups).ToString();

                                var jsFilePaths = new List<string>();
                                var jsFileRelativePaths = new List<string>();
                                foreach (JsGroupElement jsGroup in jsGroups)
                                {
                                    jsFilePaths.Add(new Uri(_pathProvider.MapPath(jsGroup.OutputFilePath)).LocalPath);
                                    jsFileRelativePaths.Add(MakeRelative(jsGroup.OutputFilePath));
                                }

                                filePaths.AddRange(jsFilePaths);
                                fileRelativePaths.AddRange(jsFileRelativePaths);

                                _logMessage(jsOutput);
                            }
                        }
                        catch (Exception exception)
                        {
                            _logError(exception.ToString());
                        }
                        countdownEvent.Signal();
                    }, countdownEvents);

                ThreadPool.QueueUserWorkItem(data =>
                    {
                        var countdownEvent = (CountdownEvent) data;

                        try
                        {
                            if (_crusherConfiguration != null)
                            {
                                var hashQueryStringKeyName = _crusherConfiguration.QuerystringKeyName;
                                var cssAssetsFileHasher = new CssAssetsFileHasher(hashQueryStringKeyName, _hasher, _pathProvider);
                                var cssPathRewriter = new CssPathRewriter(cssAssetsFileHasher, _pathProvider);
                                var cssCrusher = new CssCrusher(_cacheManager, _pathProvider, _retryableFileOpener, _retryableFileWriter, cssPathRewriter, _fileMetaData, _crusherConfiguration.WatchAssets);
                                var cssGroups = _crusherConfiguration.CssGroups;
                                var cssGroupsCrusher = new CssGroupsProcessor();
                                cssOutput = cssGroupsCrusher.ProcessGroups(_pathProvider, cssCrusher, cssGroups).ToString();

                                var cssFilePaths = new List<string>();
                                var cssFileRelativePaths = new List<string>();

                                foreach (CssGroupElement cssGroup in cssGroups)
                                {
                                    cssFilePaths.Add(new Uri(_pathProvider.MapPath(cssGroup.OutputFilePath)).LocalPath);
                                    cssFileRelativePaths.Add(MakeRelative(cssGroup.OutputFilePath));
                                }

                                filePaths.AddRange(cssFilePaths);
                                fileRelativePaths.AddRange(cssFileRelativePaths);

                                _logMessage(cssOutput);
                            }
                        }
                        catch (Exception exception)
                        {
                            _logError(exception.ToString());
                        }
                        countdownEvent.Signal();
                    }, countdownEvents);

                countdownEvents.Wait();
            }
            catch (Exception exception)
            {
                _logError(exception.ToString());
            }
            finally
            {
                AppDomain.CurrentDomain.UnhandledException -= OnUnhandledException;
            }

            _setOutputFilePaths(filePaths.ToArray());
            _setOutputFileRelativePaths(fileRelativePaths.ToArray());

            return null;
        }

        void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            _logError(e.ToString());
        }
    }
}
