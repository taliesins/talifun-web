namespace Talifun.Web.Crusher.JsModule
{
    public interface IJsModule
    {
        string Process(System.IO.FileInfo file, string fileContents);
    }
}
