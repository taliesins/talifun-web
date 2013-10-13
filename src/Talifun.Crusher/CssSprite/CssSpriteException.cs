using System;

namespace Talifun.Crusher.CssSprite
{
    public class CssSpriteException : Exception
    {
        public CssSpriteException(Exception innerException)
            : base("CssSprite compiling exception", innerException)
        {
        }

        public CssSpriteException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public override string ToString()
        {
            return InnerException.ToString();
        }
    }
}