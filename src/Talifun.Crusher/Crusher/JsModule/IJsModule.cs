using System;

namespace Talifun.Crusher.Crusher.JsModule
{
    public interface IJsModule
    {
        string Process(Uri jsRootPathUri, System.IO.FileInfo file, string fileContents);
    }
}
