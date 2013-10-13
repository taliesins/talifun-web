using System;

namespace Talifun.Crusher.Javascript
{
    public interface IJavascriptRuntime : IDisposable
    {
        void LoadLibrary(string code);
        T ExecuteFunction<T>(string functionName, params object[] args);
    }
}
