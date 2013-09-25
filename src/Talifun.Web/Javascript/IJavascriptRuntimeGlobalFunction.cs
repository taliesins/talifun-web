using System;

namespace Talifun.Web.Javascript
{
    public interface IJavascriptRuntimeGlobalFunction
    {
        void SetFunction(string functionName, Delegate function);
    }
}
