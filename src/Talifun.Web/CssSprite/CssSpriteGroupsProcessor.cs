using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Talifun.Web.Crusher;
using Talifun.Web.CssSprite.Config;
using Talifun.Web.Helper;

namespace Talifun.Web.CssSprite
{
    public class CssSpriteGroupsProcessor
    {
        public StringBuilder ProcessGroups(IPathProvider pathProvider, ICssSpriteCreator cssSpriteCreator, CssSpriteGroupElementCollection cssSpriteGroups)
        {
            var output = new StringBuilder("Css Sprite Files created:\r\n");

            Action<CssSpriteGroupToProcess> processJsGroup = ProcessJsGroup;

            var jsGroupsToProcess = cssSpriteGroups.Cast<CssSpriteGroupElement>()
                .Select(group => new CssSpriteGroupToProcess
                {
                    CssSpriteCreator = cssSpriteCreator,
                    PathProvider = pathProvider,
                    Group = group,
                    Output = output
                });

            ParallelExecute.EachParallel(jsGroupsToProcess, processJsGroup);

            return output;
        }

        private void ProcessJsGroup(CssSpriteGroupToProcess cssSpriteGroupToProcess)
        {
            var stopwatch = Stopwatch.StartNew();
            
            var files = cssSpriteGroupToProcess.Group.Files.Cast<ImageFileElement>()
                .Select(imageFile => new ImageFile
                {
                    FilePath = imageFile.FilePath,
                    Name = imageFile.Name
                })
                .ToList();

            var directories = cssSpriteGroupToProcess.Group.Directories.Cast<ImageDirectoryElement>()
                .Select(imageDirectory => new ImageDirectory
                    {
                        DirectoryPath = imageDirectory.DirectoryPath,
                        ExcludeFilter = imageDirectory.ExcludeFilter,
                        IncludeFilter = imageDirectory.IncludeFilter,
                        IncludeSubDirectories =
                            imageDirectory.IncludeSubDirectories,
                        PollTime = imageDirectory.PollTime
                    })
                 .ToList();

            var cssOutPutUri = string.IsNullOrEmpty(cssSpriteGroupToProcess.Group.CssUrl) ? new Uri(cssSpriteGroupToProcess.PathProvider.ToAbsolute(cssSpriteGroupToProcess.Group.CssOutputFilePath), UriKind.Absolute) : new Uri(cssSpriteGroupToProcess.Group.CssUrl, UriKind.RelativeOrAbsolute);
            var cssOutputPath = new FileInfo(new Uri(cssSpriteGroupToProcess.PathProvider.MapPath(cssSpriteGroupToProcess.Group.CssOutputFilePath)).LocalPath);

            var imageOutputUri = string.IsNullOrEmpty(cssSpriteGroupToProcess.Group.ImageUrl) ? new Uri(cssSpriteGroupToProcess.PathProvider.ToAbsolute(cssSpriteGroupToProcess.Group.ImageOutputFilePath), UriKind.Absolute) : new Uri(cssSpriteGroupToProcess.Group.ImageUrl, UriKind.RelativeOrAbsolute);
            var imageOutputPath = new FileInfo(new Uri(cssSpriteGroupToProcess.PathProvider.MapPath(cssSpriteGroupToProcess.Group.ImageOutputFilePath)).LocalPath);

            var output = cssSpriteGroupToProcess.CssSpriteCreator.AddFiles(imageOutputPath, imageOutputUri, cssOutputPath, files, directories);

            stopwatch.Stop();
            cssSpriteGroupToProcess.Output.Append(CreateLogEntries(cssSpriteGroupToProcess, cssOutPutUri, imageOutputUri, output, stopwatch));
        }

        private StringBuilder CreateLogEntries(CssSpriteGroupToProcess cssSpriteGroupToProcess, Uri cssOutPutUri, Uri imageOutputUri, IEnumerable<ImageFile> filesToWatch, Stopwatch stopwatch)
        {
            cssOutPutUri = new Uri(cssSpriteGroupToProcess.PathProvider.ToAbsolute(cssOutPutUri.ToString()), UriKind.Absolute);
            imageOutputUri = new Uri(cssSpriteGroupToProcess.PathProvider.ToAbsolute(imageOutputUri.ToString()), UriKind.Absolute);
            var rootPath = cssSpriteGroupToProcess.PathProvider.GetAbsoluteUriDirectory("~/");

            var output = new StringBuilder();

            output.AppendFormat("{0}({1} - {2} ms)\r\n", rootPath.MakeRelativeUri(cssOutPutUri), cssSpriteGroupToProcess.Group.Name, stopwatch.ElapsedMilliseconds);
            output.AppendFormat("{0}({1} - {2} ms)\r\n", rootPath.MakeRelativeUri(imageOutputUri), cssSpriteGroupToProcess.Group.Name, stopwatch.ElapsedMilliseconds);
            foreach (var fileToWatch in filesToWatch)
            {
                var imageProcessedOutputUri = new Uri(cssSpriteGroupToProcess.PathProvider.ToAbsolute(fileToWatch.FilePath), UriKind.Absolute);
                output.AppendFormat("    {0}\r\n", rootPath.MakeRelativeUri(imageProcessedOutputUri));
            }

            return output;
        }
    }
}
