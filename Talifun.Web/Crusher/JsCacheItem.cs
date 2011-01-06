using System;
using System.Collections.Generic;

namespace Talifun.Web.Crusher
{
    public class JsCacheItem
    {
        public Uri OutputUri { get; set; }
        public IEnumerable<JsFile> JsFiles { get; set; }
    }
}
