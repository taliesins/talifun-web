using System;

namespace Talifun.Web.Crusher.JsModule
{
    public interface IJsModule
    {
        string Process(Uri jsRootPathUri, System.IO.FileInfo file, string fileContents);
    }
}
