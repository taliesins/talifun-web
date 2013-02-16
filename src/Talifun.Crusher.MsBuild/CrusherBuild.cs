using System;
using System.Configuration;
using System.IO;
using System.Text;
using System.Threading;
using Microsoft.Build.Framework;
using Talifun.Web;
using Talifun.Web.Crusher;
using Talifun.Web.Crusher.Config;
using Talifun.Web.Helper;

namespace Talifun.Crusher.MsBuild
{
    public class CrusherBuild
    {
        private const string SenderName = "Crusher";
        private const string CrusherSectionName = "Crusher";
        private const int BufferSize = 32768;
        private static readonly Encoding Encoding = Encoding.UTF8;

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

        public void Process(string configPath, string applicationPath, IBuildEngine buildEngine)
        {
            System.Diagnostics.Debugger.Launch();

            var crusherConfiguration = GetCrusherSection(configPath, CrusherSectionName);

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

            var resetEvents = new WaitHandle[2]
                    {
                        new ManualResetEvent(false),
                        new ManualResetEvent(false)
                    };

            ThreadPool.QueueUserWorkItem(data =>
            {
                var manualResetEvent = (ManualResetEvent)data;
                if (crusherConfiguration != null)
                {
                    var jsCrusher = new JsCrusher(cacheManager, pathProvider, retryableFileOpener,
                                                  retryableFileWriter);
                    var jsGroups = crusherConfiguration.JsGroups;
                    var jsGroupsProcessor = new JsGroupsProcessor();

                    jsOutput = jsGroupsProcessor.ProcessGroups(pathProvider, jsCrusher, jsGroups).ToString();
                }
                manualResetEvent.Set();
            }, resetEvents[0]);

            ThreadPool.QueueUserWorkItem(data =>
            {
                var manualResetEvent = (ManualResetEvent)data;
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
                    cssOutput = cssGroupsCrusher.ProcessGroups(pathProvider, cssCrusher, cssGroups).ToString();
                }
                manualResetEvent.Set();
            }, resetEvents[1]);

            WaitHandle.WaitAll(resetEvents);

        }
    }
}
