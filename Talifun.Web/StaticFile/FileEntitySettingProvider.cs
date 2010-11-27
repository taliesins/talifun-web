using System;
using System.Collections.Generic;
using System.IO;
using Talifun.Web.StaticFile.Config;

namespace Talifun.Web.StaticFile
{
    public class FileEntitySettingProvider
    {
        private readonly Dictionary<string, FileEntitySetting> _fileExtensionMatches;
        private readonly FileEntitySetting _fileEntitySettingDefault;

        public FileEntitySettingProvider()
        {  
            _fileExtensionMatches = GetFileExtensionsForMatches();
            _fileEntitySettingDefault = GetDefaultFileExtensionForNoMatches();
        }

        public FileEntitySetting GetSetting(FileInfo fileInfo)
        {
            FileEntitySetting fileEntitySetting = null;
            if (!_fileExtensionMatches.TryGetValue(fileInfo.Extension.ToLower(), out fileEntitySetting))
            {
                fileEntitySetting = _fileEntitySettingDefault;
            }

            return fileEntitySetting;
        }

        private static Dictionary<string, FileEntitySetting> GetFileExtensionsForMatches()
        {
            var fileExtensionMatches = new Dictionary<string, FileEntitySetting>();

            var fileExtensionElements = CurrentStaticFileHandlerConfiguration.Current.FileExtensions;
            foreach (FileExtensionElement fileExtension in fileExtensionElements)
            {
                var extensions = fileExtension.Extension.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var extension in extensions)
                {
                    var key = extension.Trim().ToLower();
                    if (!key.StartsWith("."))
                    {
                        key = "." + key;
                    }

                    var fileExtensionElement = new FileEntitySetting
                    {
                        Compress = fileExtension.Compress,
                        Extension = fileExtension.Extension,
                        MaxMemorySize = fileExtension.MaxMemorySize,
                        ServeFromMemory = fileExtension.ServeFromMemory,
                        EtagMethod = fileExtension.EtagMethod,
                        Expires = fileExtension.Expires,
                        MemorySlidingExpiration = fileExtension.MemorySlidingExpiration
                    };

                    fileExtensionMatches.Add(key, fileExtensionElement);
                }
            }

            return fileExtensionMatches;
        }

        private static FileEntitySetting GetDefaultFileExtensionForNoMatches()
        {
            var fileExtensionElementDefault = CurrentStaticFileHandlerConfiguration.Current.FileExtensionDefault;

            return new FileEntitySetting
            {
                Compress = fileExtensionElementDefault.Compress,
                Extension = string.Empty,
                MaxMemorySize = fileExtensionElementDefault.MaxMemorySize,
                ServeFromMemory = fileExtensionElementDefault.ServeFromMemory,
                EtagMethod = fileExtensionElementDefault.EtagMethod,
                Expires = fileExtensionElementDefault.Expires,
                MemorySlidingExpiration = fileExtensionElementDefault.MemorySlidingExpiration
            };
        }
    }
}
