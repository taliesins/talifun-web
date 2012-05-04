using System;
using System.Collections.Generic;

namespace Talifun.Web.Crusher
{
    public class JsCacheItem
    {
        public Uri OutputUri { get; set; }
		public IEnumerable<JsFileToWatch> FilesToWatch { get; set; }
        public IEnumerable<JsFile> Files { get; set; }
		public IEnumerable<JsDirectory> Directories { get; set; }
    }
}
