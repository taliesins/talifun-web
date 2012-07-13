using System.Collections.Generic;

namespace Talifun.Web.Crusher
{
    public class CssFileToWatchEqualityComparer : IEqualityComparer<CssFileToWatch>
    {
        public bool Equals(CssFileToWatch x, CssFileToWatch y)
        {
            return string.Equals(x.FilePath, y.FilePath);
        }

        public int GetHashCode(CssFileToWatch obj)
        {
            return obj.FilePath.GetHashCode();
        }
    }
}
