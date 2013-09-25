using System;
using System.Linq;

namespace Talifun.Web.Crusher.CssModule
{
    public class DotLessModule : ICssModule
    {
        private readonly string[] _lessExtensions = { ".less", ".less.js" }; 

        public string Process(Uri cssRootPathUri, System.IO.FileInfo file, string fileContents)
        {
            if (!_lessExtensions.Contains(file.Extension.ToLower()))
            {
                return fileContents;
            }
                
            return dotless.Core.Less.Parse(fileContents);
        }
    }
}