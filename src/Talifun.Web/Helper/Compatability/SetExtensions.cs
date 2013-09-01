#if NET35
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using Iesi.Collections.Generic;

namespace Talifun.Web.Helper
{
    public static class SetExtensions
    {
        public static bool SetEquals<T>(this Iesi.Collections.Generic.ISet<T> set, IEnumerable<T> otherSet)
        {
            return set.ContainsAll(new Collection<T>(new List<T>(otherSet)));
        }
    }
}
#endif
