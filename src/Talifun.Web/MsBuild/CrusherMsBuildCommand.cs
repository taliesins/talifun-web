using System;
using System.Configuration;
using System.IO;
using System.Text;
using System.Threading;
using Talifun.Web.Crusher;
using Talifun.Web.Crusher.Config;
using Talifun.Web.CssSprite;
using Talifun.Web.CssSprite.Config;
using Talifun.Web.Helper;

namespace Talifun.Web.MsBuild
{
    [Serializable]
    public class CrusherMsBuildCommand : MarshalByRefObject, ICloneable //IClonable is marker interface so we can use msbuild with different app domain
    {
        private readonly string _applicationPath;
        private readonly string _binDirectoryPath;
        private readonly string _configPath;
        private readonly Action<string> _logMessage;
        private readonly Action<string> _logError;
        private const string CrusherSectionName = "Crusher";
        private const string CssSpriteSectionName = "CssSprite";
        private const int BufferSize = 32768;
        private static readonly Encoding Encoding = Encoding.UTF8;

        public CrusherMsBuildCommand(string applicationPath, string binDirectoryPath, string configPath, Action<string> logMessage, Action<string> logError)
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

            _applicationPath = applicationPath;
            _binDirectoryPath = binDirectoryPath;
            _configPath = configPath;
            _logMessage = logMessage;
            _logError = logError;
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

        public object Clone()
        {
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            try
            {
                var cssSpriteConfiguration = GetCssSpriteSection(_configPath, CssSpriteSectionName);
                var crusherConfiguration = GetCrusherSection(_configPath, CrusherSectionName);

                var configUri = new Uri(_configPath, UriKind.RelativeOrAbsolute);
                if (!configUri.IsAbsoluteUri)
                {
                    configUri = new Uri(Path.Combine(Environment.CurrentDirectory, configUri.ToString()));
                }

                var physicalApplicationPath = new FileInfo(configUri.LocalPath).DirectoryName;

                var retryableFileOpener = new RetryableFileOpener();
                var hasher = new Hasher(retryableFileOpener);
                var retryableFileWriter = new RetryableFileWriter(BufferSize, Encoding, retryableFileOpener, hasher);
                var pathProvider = new PathProvider(_applicationPath, physicalApplicationPath);
                var cacheManager = new HttpCacheManager();

                var cssSpriteOutput = string.Empty;
                var jsOutput = string.Empty;
                var cssOutput = string.Empty;

                var countdownEvents = new CountdownEvent(1);

                ThreadPool.QueueUserWorkItem(data =>
                    {
                        var countdownEvent = (CountdownEvent) data;

                        try
                        {
                            if (cssSpriteConfiguration != null)
                            {
                                var cssSpriteGroups = cssSpriteConfiguration.CssSpriteGroups;
                                var cssSpriteCreator = new CssSpriteCreator(cacheManager, retryableFileOpener,
                                                                            pathProvider, retryableFileWriter);
                                var cssSpriteGroupsProcessor = new CssSpriteGroupsProcessor();

                                cssSpriteOutput =
                                    cssSpriteGroupsProcessor.ProcessGroups(pathProvider, cssSpriteCreator,
                                                                           cssSpriteGroups).ToString();

                                _logMessage(cssSpriteOutput);
                            }
                        }
                        catch (Exception exception)
                        {
                            _logError(exception.ToString());
                        }
                        countdownEvent.Signal();
                    }, countdownEvents);

                countdownEvents.Wait();

                countdownEvents = new CountdownEvent(2);

                ThreadPool.QueueUserWorkItem(data =>
                    {
                        var countdownEvent = (CountdownEvent) data;

                        try
                        {
                            if (crusherConfiguration != null)
                            {
                                var jsCrusher = new JsCrusher(cacheManager, pathProvider, retryableFileOpener,
                                                              retryableFileWriter);
                                var jsGroups = crusherConfiguration.JsGroups;
                                var jsGroupsProcessor = new JsGroupsProcessor();

                                jsOutput = jsGroupsProcessor.ProcessGroups(pathProvider, jsCrusher, jsGroups).ToString();

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
                            if (crusherConfiguration != null)
                            {
                                var hashQueryStringKeyName = crusherConfiguration.QuerystringKeyName;
                                var cssAssetsFileHasher = new CssAssetsFileHasher(hashQueryStringKeyName, hasher,
                                                                                  pathProvider);
                                var cssPathRewriter = new CssPathRewriter(cssAssetsFileHasher, pathProvider);
                                var cssCrusher = new CssCrusher(cacheManager, pathProvider, retryableFileOpener,
                                                                retryableFileWriter, cssPathRewriter);
                                var cssGroups = crusherConfiguration.CssGroups;
                                var cssGroupsCrusher = new CssGroupsProcessor();
                                cssOutput =
                                    cssGroupsCrusher.ProcessGroups(pathProvider, cssCrusher, cssGroups).ToString();

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

            return null;
        }

        void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            _logError(e.ToString());
        }
    }
}
