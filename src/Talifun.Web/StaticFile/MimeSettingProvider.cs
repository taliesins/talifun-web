using System;
using System.Collections.Generic;
using System.IO;
using Talifun.Web.StaticFile.Config;

namespace Talifun.Web.StaticFile
{
    public class MimeSettingProvider
    {
        private readonly Dictionary<string, MimeSetting> _fileExtensionMatches;
        private readonly MimeSetting _mimeSettingDefault;

        public MimeSettingProvider()
        {  
            _fileExtensionMatches = GetFileExtensionsForMatches();
            _mimeSettingDefault = GetDefaultFileExtensionForNoMatches();
        }

        public MimeSetting GetSetting(string fileExtension)
        {
            MimeSetting fileMimeSetting = null;
            if (!_fileExtensionMatches.TryGetValue(fileExtension, out fileMimeSetting))
            {
                fileMimeSetting = _mimeSettingDefault;
            }

            return fileMimeSetting;
        }

        public MimeSetting GetSetting(FileInfo fileInfo)
        {
            return GetSetting(fileInfo.Extension.ToLower());
        }

        private static Dictionary<string, MimeSetting> GetFileExtensionsForMatches()
        {
            var fileExtensionMatches = new Dictionary<string, MimeSetting>();

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

                    var fileExtensionElement = new MimeSetting
                    {
                        Compress = fileExtension.Compress,
                        Extension = fileExtension.Extension,
                        MaxMemorySize = fileExtension.MaxMemorySize,
                        ServeFromMemory = fileExtension.ServeFromMemory,
                        EtagMethod = fileExtension.EtagMethod,
                        Expires = fileExtension.Expires,
                        MemorySlidingExpiration = fileExtension.MemorySlidingExpiration,
                        UrlEtagQuerystringName = fileExtension.UrlEtagQuerystringName,
                        UrlEtagHandlingMethod = fileExtension.UrlEtagHandlingMethod,
                    };

                    fileExtensionMatches.Add(key, fileExtensionElement);
                }
            }

            return fileExtensionMatches;
        }

        private static MimeSetting GetDefaultFileExtensionForNoMatches()
        {
            var fileExtensionElementDefault = CurrentStaticFileHandlerConfiguration.Current.FileExtensionDefault;

            return new MimeSetting
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
