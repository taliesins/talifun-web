using System.Collections.Generic;

namespace Talifun.Crusher.Crusher
{
    public class JsFileToWatchEqualityComparer : IEqualityComparer<JsFileToWatch>
    {
        public bool Equals(JsFileToWatch x, JsFileToWatch y)
        {
            return string.Equals(x.FilePath, y.FilePath);
        }

        public int GetHashCode(JsFileToWatch obj)
        {
            return obj.FilePath.GetHashCode();
        }
    }
}
