using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Talifun.Crusher.Configuration.Css;
using Talifun.Web.Helper;

namespace Talifun.Crusher.Crusher
{
    public class CssGroupsProcessor : ICssGroupsProcessor
    {
        public StringBuilder ProcessGroups(IPathProvider pathProvider, ICssCrusher cssCrusher, CssGroupElementCollection cssGroups)
        {
            var output = new StringBuilder("Css Files created:\r\n");
         
            Action<CssGroupToProcess> processGroupConfiguration = ProcessGroup;
            var cssGroupsToProcess = cssGroups.Cast<CssGroupElement>()
                .Select(group => new CssGroupToProcess
                {
                    Crusher = cssCrusher,
                    PathProvider = pathProvider,
                    Group = group,
                    Output = output
                }).ToList();

            if (cssGroupsToProcess.Any())
            {
                ParallelExecute.EachParallel(cssGroupsToProcess, processGroupConfiguration);
            }
            else
            {
                output.AppendFormat("No files to process");
            }
            return output;
        }

        private void ProcessGroup(CssGroupToProcess cssGroupToProcess)
        {
            var stopwatch = Stopwatch.StartNew();
            var files = cssGroupToProcess.Group.Files.Cast<CssFileElement>()
                .Select(cssFile => new CssFile
                {
                    CompressionType = cssFile.CompressionType,
                    FilePath = cssFile.FilePath
                })
                .ToList();

            var directories = cssGroupToProcess.Group.Directories.Cast<CssDirectoryElement>()
                .Select(cssDirectory => new CssDirectory
                    {
                        CompressionType = cssDirectory.CompressionType,
                        DirectoryPath = cssDirectory.DirectoryPath,
                        IncludeSubDirectories = cssDirectory.IncludeSubDirectories,
                        PollTime = cssDirectory.PollTime,
                        IncludeFilter = cssDirectory.IncludeFilter,
                        ExcludeFilter = cssDirectory.ExcludeFilter
                    })
                .ToList();

            var outputUri = new Uri(cssGroupToProcess.PathProvider.ToAbsolute(cssGroupToProcess.Group.OutputFilePath), UriKind.Relative);
            var output = cssGroupToProcess.Crusher.AddGroup(outputUri, files, directories, cssGroupToProcess.Group.AppendHashToCssAsset);

            stopwatch.Stop();

            cssGroupToProcess.Output.Append(CreateLogEntries(cssGroupToProcess, outputUri, output, stopwatch));
        }

        private StringBuilder CreateLogEntries(CssGroupToProcess cssGroupToProcess, Uri outputUri, CssCrushedOutput crushedOutput, Stopwatch stopwatch)
        {
            outputUri = new Uri(cssGroupToProcess.PathProvider.ToAbsolute(outputUri.ToString()), UriKind.Absolute);
            var rootPath = cssGroupToProcess.PathProvider.GetAbsoluteUriDirectory("~/");

            var output = new StringBuilder();
            output.AppendFormat("{0} ({1} - {2} ms)\r\n", rootPath.MakeRelativeUri(outputUri), cssGroupToProcess.Group.Name, stopwatch.ElapsedMilliseconds);
            output.AppendFormat("{0}   (Css)\r\n", rootPath.MakeRelativeUri(outputUri));
            foreach (var cssFile in crushedOutput.FilesToWatch)
            {
                outputUri = new Uri(cssGroupToProcess.PathProvider.ToAbsolute(cssFile.FilePath), UriKind.Absolute);
                output.AppendFormat("      {0}\r\n", rootPath.MakeRelativeUri(outputUri));
            }
            output.AppendFormat("{0}   (Css Assets)\r\n", rootPath.MakeRelativeUri(outputUri));
            foreach (var cssAssetFile in crushedOutput.CssAssetFilePaths)
            {
                outputUri = new Uri(cssGroupToProcess.PathProvider.ToAbsolute(cssAssetFile.FullName), UriKind.Absolute);
                output.AppendFormat("      {0}\r\n", rootPath.MakeRelativeUri(outputUri));
            }
            return output;
        }
    }
}
