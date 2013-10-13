using System;

namespace Talifun.Crusher.Crusher.CssModule
{
    public interface ICssModule
    {
        string Process(Uri cssRootPathUri, System.IO.FileInfo file, string fileContents);
    }
}
