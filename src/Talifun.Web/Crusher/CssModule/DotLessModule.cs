using System;

namespace Talifun.Web.Crusher.CssModule
{
    public class DotLessModule : ICssModule
    {
        public string Process(Uri cssRootPathUri, System.IO.FileInfo file, string fileContents)
        {
            var fileName = file.Name.ToLower();
            if (fileName.EndsWith(".less") || fileName.EndsWith(".less.css"))
            {
                return dotless.Core.Less.Parse(fileContents);
            }

            return fileContents;
        }
    }
}