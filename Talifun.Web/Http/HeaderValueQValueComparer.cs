using System.Collections.Generic;

namespace Talifun.Web
{
    /// <summary>
    /// Compare header values by qvalue.
    /// </summary>
    /// <remarks>
    /// A null value qvalue has the highest weight.
    /// </remarks>
    public class HeaderValueQValueComparer : IComparer<HttpHeaderValue>
    {
        public int Compare(HttpHeaderValue x, HttpHeaderValue y)
        {
            if (!x.QValue.HasValue)
            {
                return !y.QValue.HasValue ? 0 : 1;
            }

            if (!y.QValue.HasValue)
            {
                return -1;
            }

            return x.QValue.Value.CompareTo(y.QValue.Value);
        }
    }
}
