using System;

namespace Talifun.Crusher.Crusher
{
    public class CssException : Exception
    {
        public CssException(Exception innerException)
            : base("Css compiling exception", innerException)
        {
        }

        public CssException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public override string ToString()
        {
            return InnerException.ToString();
        }
    }
}
