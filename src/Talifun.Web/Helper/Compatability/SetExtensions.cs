#if NET35
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;

namespace Talifun.Web
{
    public static class SetExtensions
    {
        public static bool SetEquals<T>(this ISet<T> set, IEnumerable<T> otherSet)
        {
            return set.ContainsAll(new Collection<T>(new List<T>(otherSet)));
        }
    }
}
#endif
