using System;
using System.Linq;
using System.Text;
using Talifun.Web.Crusher.Config;
using Talifun.Web.Helper;

namespace Talifun.Web.Crusher
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
                });

            ParallelExecute.EachParallel(cssGroupsToProcess, processGroupConfiguration);

            return output;
        }

        private void ProcessGroup(CssGroupToProcess cssGroupToProcess)
        {
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
            var output = cssGroupToProcess.Crusher.CreateGroup(outputUri, files, directories, cssGroupToProcess.Group.AppendHashToCssAsset);

            cssGroupToProcess.Output.Append(CreateLogEntries(cssGroupToProcess, outputUri, output));
        }

        private StringBuilder CreateLogEntries(CssGroupToProcess cssGroupToProcess, Uri outputUri, CssCrushedOutput crushedOutput)
        {
            var output = new StringBuilder();
            output.AppendFormat("{0} ({1})\r\n", outputUri, cssGroupToProcess.Group.Name);
            output.AppendFormat("{0}   (Css)\r\n", outputUri);
            foreach (var cssFile in crushedOutput.FilesToWatch)
            {
                outputUri = new Uri(cssGroupToProcess.PathProvider.ToAbsolute(cssFile.FilePath), UriKind.Absolute);
                output.AppendFormat("      {0}\r\n", outputUri);
            }
            output.AppendFormat("{0}   (Css Assets)\r\n", outputUri);
            foreach (var cssAssetFile in crushedOutput.CssAssetFilePaths)
            {
                outputUri = new Uri(cssGroupToProcess.PathProvider.ToAbsolute(cssAssetFile.FullName), UriKind.Absolute);
                output.AppendFormat("      {0}\r\n", outputUri);
            }
            return output;
        }
    }
}
