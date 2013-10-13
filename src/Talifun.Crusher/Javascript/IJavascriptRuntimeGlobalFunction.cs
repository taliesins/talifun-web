using System;

namespace Talifun.Crusher.Javascript
{
    public interface IJavascriptRuntimeGlobalFunction
    {
        void SetFunction(string functionName, Delegate function);
    }
}
