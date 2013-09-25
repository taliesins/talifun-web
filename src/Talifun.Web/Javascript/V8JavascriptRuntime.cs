using System;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;
using Noesis.Javascript;

namespace Talifun.Web.Javascript
{
    public class V8JavascriptRuntime : IJavascriptRuntime, IJavascriptRuntimeGlobalVariable, IJavascriptRuntimeGlobalFunction
    {
        protected readonly JavascriptContext ScriptEngine;

        public V8JavascriptRuntime()
        {
            ScriptEngine = new JavascriptContext();
        }

        public void LoadLibrary(string code)
        {
            ScriptEngine.Run(code);
        }

        public T ExecuteFunction<T>(string functionName, params object[] args)
        {
            if (functionName == null)
                throw new ArgumentNullException("functionName");

            var arguments = GetArgs(args);
            var code = string.Format("{0}({1});", functionName, arguments);
            var result = ScriptEngine.Run(code);
            return (T)result;
        }

        private string GetArgs(params object[] args)
        {
            if (args == null || args.Length < 1)
            {
                return string.Empty;
            }

            var argString = string.Join(", ", args.Select(JsonConvert.SerializeObject));

            return argString;
        }

        public T GetVariable<T>(string variableName)
        {
            return (T)ScriptEngine.GetParameter(variableName);
        }

        public void SetVariable(string variableName, object value)
        {
            ScriptEngine.SetParameter(variableName, value);
        }

        public void SetFunction(string functionName, Delegate function)
        {
            ScriptEngine.SetParameter(functionName, function);
        }

        #region IDisposable Members
        private int alreadyDisposed = 0;

        ~V8JavascriptRuntime()
        {
            // call Dispose with false.  Since we're in the
            // destructor call, the managed resources will be
            // disposed of anyways.
            Dispose(false);
        }

        void IDisposable.Dispose()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (alreadyDisposed != 0) return;

            // dispose of the managed and unmanaged resources
            Dispose(true);

            // tell the GC that the Finalize process no longer needs
            // to be run for this object. 

            // it is called after Dispose(true) to ensure that GC.SuppressFinalize() 
            // only gets called if the Dispose operation completes successfully. 
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposeManagedResources)
        {
            var disposedAlready = Interlocked.Exchange(ref alreadyDisposed, 1);
            if (disposedAlready != 0) return;

            if (!disposeManagedResources) return;

            // Dispose managed resources.
            ScriptEngine.Dispose();
        }

        #endregion
    }
}
