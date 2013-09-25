using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using NUnit.Framework;
using Talifun.Crusher.MsBuild;

namespace Talifun.Web.Tests.MsBuild
{
    [Serializable]
    public class BuildEngingStub : IBuildEngine
    {
        public BuildEngingStub()
        {
            BuildErrorEventArgs = new List<BuildErrorEventArgs>();
            BuildWarningEventArgs = new List<BuildWarningEventArgs>();
            BuildMessageEventArgs = new List<BuildMessageEventArgs>();
            CustomBuildEventArgs = new List<CustomBuildEventArgs>();
        }

        public List<BuildErrorEventArgs> BuildErrorEventArgs { get; set; }
        public List<BuildWarningEventArgs> BuildWarningEventArgs { get; set; }
        public List<BuildMessageEventArgs> BuildMessageEventArgs { get; set; }
        public List<CustomBuildEventArgs> CustomBuildEventArgs { get; set; }

        public void LogErrorEvent(BuildErrorEventArgs e)
        {
            BuildErrorEventArgs.Add(e);
        }

        public void LogWarningEvent(BuildWarningEventArgs e)
        {
            BuildWarningEventArgs.Add(e);
        }

        public void LogMessageEvent(BuildMessageEventArgs e)
        {
            BuildMessageEventArgs.Add(e);
        }

        public void LogCustomEvent(CustomBuildEventArgs e)
        {
            CustomBuildEventArgs.Add(e);
        }

        public bool BuildProjectFile(string projectFileName, string[] targetNames, IDictionary globalProperties,
                                     IDictionary targetOutputs)
        {
            throw new System.NotImplementedException();
        }

        public bool ContinueOnError { get; private set; }
        public int LineNumberOfTaskNode { get; private set; }
        public int ColumnNumberOfTaskNode { get; private set; }
        public string ProjectFileOfTaskNode { get; private set; }
    }

    [TestFixture]
    public class CreateInstanceTest
    {
         [Test]
         public void CreateCrusherTask()
         {
             var buildEngine = new BuildEngingStub();

             var crusherTask = new CrusherTask
                 {
                     BinDirectoryPath = Environment.CurrentDirectory,
                     ConfigFilePath = Path.Combine(Environment.CurrentDirectory, "Talifun.Web.Tests.dll.config"),
                     BuildEngine = buildEngine,
                     HostObject = null
                 };

             crusherTask.Execute();

             Assert.False(buildEngine.BuildErrorEventArgs.Any(), buildEngine.BuildErrorEventArgs.Any() ? buildEngine.BuildErrorEventArgs.First().Message : string.Empty);
         }
    }
}
