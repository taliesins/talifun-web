using System;

namespace Talifun.Web.Crusher
{
    public class JsException : Exception
    {
        public JsException(Exception innerException)
            : base("Js compiling exception", innerException)
        {
        }

        public JsException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public override string ToString()
        {
            return InnerException.ToString();
        }
    }
}
