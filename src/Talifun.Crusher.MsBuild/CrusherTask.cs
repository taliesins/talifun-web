using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Build.Framework;

namespace Talifun.Crusher.MsBuild
{
    public class CrusherTask : ITask
    {
        private const string SenderName = "Crusher";

        public IBuildEngine BuildEngine { get; set; }
        public ITaskHost HostObject { get; set; }

        public bool Execute()
        {
            System.Diagnostics.Debugger.Launch();
            var stopwatch = Stopwatch.StartNew();
            BuildEngine.LogMessageEvent(new BuildMessageEventArgs(string.Format("{0}: (version {1}) Starting", SenderName, GetType().Assembly.GetName().Version), "", SenderName, MessageImportance.High));

            try
            {
                var configPath = GetConfigPath();

                if (string.IsNullOrEmpty(configPath) || !File.Exists(configPath))
                {
                    BuildEngine.LogErrorEvent(new BuildErrorEventArgs("", "", "", 0, 0, 0, 0, string.Format("{0}: {1} not found", SenderName, configPath), "", SenderName));
                    return false;
                }

                var binPath = GetBinPath();

                if (string.IsNullOrEmpty(binPath) || !Directory.Exists(binPath))
                {
                    BuildEngine.LogErrorEvent(new BuildErrorEventArgs("", "", "", 0, 0, 0, 0, string.Format("{0}: {1} not found", SenderName, binPath), "", SenderName));
                    return false;
                }

                var applicationPath = "/";
                
                var parameters = new Dictionary<string, object>()
                    {
                        {"configPath", configPath},
                        {"applicationPath", applicationPath},
                        {"buildEngine", BuildEngine},
                    };

                ExecuteInChildDomain(binPath, parameters, Crush);

                return true;
            }
            finally
            {
                stopwatch.Stop();
                BuildEngine.LogMessageEvent(new BuildMessageEventArgs(string.Format("{0}: Finished ({1}ms)", SenderName, stopwatch.ElapsedMilliseconds), "", SenderName, MessageImportance.High));
            }
        }

        private string GetConfigPath()
        {
            var projectFilePath = Path.GetDirectoryName(BuildEngine.ProjectFileOfTaskNode);
            var configFilePath = Path.Combine(projectFilePath, "web.config");
            return configFilePath;
        }

        private string GetBinPath()
        {
            return Path.Combine(Path.GetDirectoryName(BuildEngine.ProjectFileOfTaskNode), "bin");
        }

        private static void ExecuteInChildDomain(string applicationPath, Dictionary<string, object> parameters, CrossAppDomainDelegate action)
        {
            var setup = new AppDomainSetup
            {
                ApplicationBase = applicationPath,
                ShadowCopyFiles = "true"
            };

            var childDomain = AppDomain.CreateDomain(SenderName, null, setup);

            foreach (var parameter in parameters)
            {
                childDomain.SetData(parameter.Key, parameter.Value);
            }

            try
            {
                childDomain.DoCallBack(action);
            }
            finally
            {
                AppDomain.Unload(childDomain);
            }
        }

        private static void Crush()
        {
            var configPath = (string)AppDomain.CurrentDomain.GetData("configPath");
            var applicationPath = (string)AppDomain.CurrentDomain.GetData("applicationPath");
            var buildEngine = (IBuildEngine)AppDomain.CurrentDomain.GetData("buildEngine");

            try
            {
                var crusherBuild = new CrusherBuild();
                crusherBuild.Process(configPath, applicationPath, buildEngine);
            }
            catch (Exception exception)
            {
                buildEngine.LogErrorEvent(new BuildErrorEventArgs("", "", "", 0, 0, 0, 0,
                                                                  string.Format("{0}: {1}", SenderName, exception), "",
                                                                  SenderName));
            }
        }
    }
}
