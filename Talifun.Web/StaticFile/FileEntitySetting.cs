﻿using System;

namespace Talifun.Web.StaticFile
{
    public class FileEntitySetting
    {
        public bool Compress { get; set; }
        public string Extension { get; set;}
        public bool ServeFromMemory { get; set;}
        public long MaxMemorySize { get; set; }
        public TimeSpan MemorySlidingExpiration { get; set; }
        public TimeSpan Expires { get; set; }
        public EtagMethodType EtagMethod { get; set; }
        public string UrlEtagQuerystringName { get; set; }
        public UrlEtagHandlingMethodType UrlEtagHandlingMethod { get; set; }
    }
}