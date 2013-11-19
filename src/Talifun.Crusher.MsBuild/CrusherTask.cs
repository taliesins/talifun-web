using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Remoting;
using Microsoft.Build.Framework;

namespace Talifun.Crusher.MsBuild
{
    [LoadInSeparateAppDomain]
    [Serializable]
    public class CrusherTask : MarshalByRefObject, ITask
    {
        private const string SenderName = "Crusher";
        private const string CrusherAssemblyName = "Talifun.Crusher";
        private const string CrusherBuildFullName = "Talifun.Crusher.MsBuild.CrusherMsBuildCommand";

        [Required]
        public string ConfigFilePath { get; set; }

        [Required]
        public string BinDirectoryPath { get; set; }

        [Output]
        public string[] OutputFilePaths { get; private set; }

        private void LogMessage(string message)
        {
            BuildEngine.LogMessageEvent(new BuildMessageEventArgs(string.Format("{0}: {1}", SenderName, message), "", SenderName, MessageImportance.High));
        }

        private void LogError(string message)
        {
            BuildEngine.LogErrorEvent(new BuildErrorEventArgs("", "", "", 0, 0, 0, 0, string.Format("{0}: {1}", SenderName, message), "", SenderName));
        }

        private void SetCrushedFilePaths(string[] filePaths)
        {
            OutputFilePaths = filePaths;
        }

        public bool Execute()
        {
            var stopwatch = Stopwatch.StartNew();

            LogMessage(string.Format("(version {0}) Starting", GetType().Assembly.GetName().Version));
            LogMessage(string.Format("ConfigFilePath = {0}", ConfigFilePath));
            LogMessage(string.Format("BinDirectoryPath = {0}", BinDirectoryPath));

            try
            {
                var configPath = GetConfigPath();

                if (string.IsNullOrEmpty(configPath) || !File.Exists(configPath))
                {
                    LogError(string.Format("{0} not found", configPath));
                    return false;
                }

                var binPath = GetBinPath();

                if (string.IsNullOrEmpty(binPath) || !Directory.Exists(binPath))
                {
                    LogError(string.Format("{0} not found", binPath));
                    return false;
                }

                var applicationPath = "/";

                ExecuteInChildDomain(applicationPath, binPath, configPath, BuildEngine);
            }
            catch (Exception exception)
            {
                LogError(exception.ToString());
            }
            finally
            {
                stopwatch.Stop();
                LogMessage(string.Format("Finished ({0}ms)", stopwatch.ElapsedMilliseconds));
            }

            return true;
        }

        public IBuildEngine BuildEngine { get; set; }
        public ITaskHost HostObject { get; set; }

        private string GetConfigPath()
        {
            var configFilePath = ConfigFilePath;
            if (File.Exists(configFilePath))
            {
                return configFilePath;
            }

            var projectFilePath = string.Empty;
            if (!string.IsNullOrEmpty(BuildEngine.ProjectFileOfTaskNode))
            {
                projectFilePath = Path.GetDirectoryName(BuildEngine.ProjectFileOfTaskNode);
                configFilePath = Path.Combine(projectFilePath, "web.config");

                if (File.Exists(configFilePath))
                {
                    return configFilePath;
                }
            }

            projectFilePath = Environment.CurrentDirectory;
            configFilePath = Path.Combine(projectFilePath, "web.config");

            return configFilePath;
        }

        private string GetBinPath()
        {
            var binDirectoryPath = BinDirectoryPath;
            if (Directory.Exists(binDirectoryPath))
            {
                return binDirectoryPath;
            }

            var projectFilePath = Path.GetDirectoryName(BuildEngine.ProjectFileOfTaskNode);
            binDirectoryPath = Path.Combine(projectFilePath, "bin");
            if (Directory.Exists(binDirectoryPath))
            {
                return binDirectoryPath;
            }

            projectFilePath = Environment.CurrentDirectory;
            binDirectoryPath = Path.Combine(projectFilePath, "bin");

            return binDirectoryPath;
        }

        private void ExecuteInChildDomain(string applicationPath, string binDirectoryPath, string configPath, IBuildEngine buildEngine)
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

            if (buildEngine == null)
            {
                throw new ArgumentNullException("buildEngine");
            }

            var crusherAssemblyPath = Path.Combine(binDirectoryPath, CrusherAssemblyName + ".dll");
            Action<string> logMessage = LogMessage;
            Action<string> logError = LogError;
            Action<string[]> setCrushedFilePaths = SetCrushedFilePaths;
            var constructorArguments = new object[] { applicationPath, binDirectoryPath, configPath, logMessage, logError, setCrushedFilePaths};

            var setup = new AppDomainSetup
            {
                ApplicationBase = binDirectoryPath,
                ShadowCopyFiles = "true"
            };

            var childDomain = AppDomain.CreateDomain(SenderName, null, setup);
            childDomain.UnhandledException += childDomain_UnhandledException;

            try
            {
                var crusherMsBuildCommandHandle = CreateInstance(childDomain, crusherAssemblyPath, CrusherBuildFullName, constructorArguments);

                if (crusherMsBuildCommandHandle == null)
                {
                    throw new Exception("Cannot resolve crusherMsBuildCommandHandle");
                }

                var crusherMsBuildCommand = (ICloneable)crusherMsBuildCommandHandle.Unwrap();

                if (crusherMsBuildCommand == null)
                {
                    throw new Exception("Cannot unwrap crusherMsBuildCommand");
                }

                crusherMsBuildCommand.Clone();
            }
            finally
            {
                childDomain.UnhandledException -= childDomain_UnhandledException;
                AppDomain.Unload(childDomain);
            }
        }

        private ObjectHandle CreateInstance(AppDomain childDomain, string assemblyFilePath, string typeName, object[] constructorArguments)
        {
#if NET35
            var crusherMsBuildCommandHandle = Activator.CreateInstanceFrom(
                childDomain,
                assemblyFilePath,
                typeName,
                false,
                0,
                null,
                constructorArguments,
                null,
                null,
                null
            );
#else
            var crusherMsBuildCommandHandle = Activator.CreateInstanceFrom(
                childDomain,
                assemblyFilePath,
                typeName,
                false,
                0,
                null,
                constructorArguments,
                null,
                null
            );
#endif

            return crusherMsBuildCommandHandle;
        }

        private void childDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            LogError(e.ExceptionObject.ToString());
        }
    }
}
