using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Talifun.Web.Crusher.Config;
using Talifun.Web.Helper;

namespace Talifun.Web.Crusher
{
    public class JsGroupsProcessor : IJsGroupsProcessor
    {
        public StringBuilder ProcessGroups(IPathProvider pathProvider, IJsCrusher jsCrusher, JsGroupElementCollection jsGroups)
        {
            var output = new StringBuilder("Js Files created:\r\n");
         
            Action<JsGroupToProcess> processJsGroup = ProcessJsGroup;

            var jsGroupsToProcess = jsGroups.Cast<JsGroupElement>()
                .Select(group => new JsGroupToProcess
                {
                    Crusher = jsCrusher,
                    PathProvider = pathProvider,
                    Group = group,
                    Output = output
                });

            ParallelExecute.EachParallel(jsGroupsToProcess, processJsGroup);

            return output;
        }

        private void ProcessJsGroup(JsGroupToProcess jsGroupToProcess)
        {
            var stopwatch = Stopwatch.StartNew();
            var files = jsGroupToProcess.Group.Files.Cast<JsFileElement>()
                .Select(jsFile => new JsFile
                {
                    CompressionType = jsFile.CompressionType,
                    FilePath = jsFile.FilePath
                })
                .ToList();

            var directories = jsGroupToProcess.Group.Directories.Cast<JsDirectoryElement>()
                .Select(jsDirectory => new JsDirectory
                    {
                        CompressionType = jsDirectory.CompressionType,
                        DirectoryPath = jsDirectory.DirectoryPath,
                        IncludeSubDirectories = jsDirectory.IncludeSubDirectories,
                        PollTime = jsDirectory.PollTime,
                        IncludeFilter = jsDirectory.IncludeFilter,
                        ExcludeFilter = jsDirectory.ExcludeFilter
                    })
                 .ToList();

            var outputUri = new Uri(jsGroupToProcess.PathProvider.ToAbsolute(jsGroupToProcess.Group.OutputFilePath), UriKind.Relative);
            var output = jsGroupToProcess.Crusher.AddGroup(outputUri, files, directories);

            stopwatch.Stop();
            jsGroupToProcess.Output.Append(CreateLogEntries(jsGroupToProcess, outputUri, output, stopwatch));
        }

        private StringBuilder CreateLogEntries(JsGroupToProcess jsGroupToProcess, Uri outputUri, JsCrushedOutput crushedOutput, Stopwatch stopwatch)
        {
            outputUri = new Uri(jsGroupToProcess.PathProvider.ToAbsolute(outputUri.ToString()), UriKind.Absolute);
            var rootPath = jsGroupToProcess.PathProvider.GetAbsoluteUriDirectory("~/");

            var output = new StringBuilder();
            output.AppendFormat("{0} ({1} - {2} ms)\r\n", rootPath.MakeRelativeUri(outputUri), jsGroupToProcess.Group.Name, stopwatch.ElapsedMilliseconds);
            foreach (var jsFile in crushedOutput.FilesToWatch)
            {
                outputUri = new Uri(jsGroupToProcess.PathProvider.ToAbsolute(jsFile.FilePath), UriKind.Absolute);
                output.AppendFormat("    {0}\r\n", rootPath.MakeRelativeUri(outputUri));
            }

            return output;
        }
    }
}
