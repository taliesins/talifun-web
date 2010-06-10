using System;
using System.Collections;
using System.Collections.Generic;

namespace Talifun.Web.Module
{
    static class DictionaryExtensions
    {
        public static V Find<K, V>(this IDictionary<K, V> dict, K key)
        {
            return Find(dict, key, default(V));
        }

        public static V Find<K, V>(this IDictionary<K, V> dict, K key, V @default)
        {
            if (dict == null) throw new ArgumentNullException("dict");
            V value;
            return dict.TryGetValue(key, out value) ? value : @default;
        }

        public static T Find<T>(this IDictionary dict, object key, T @default)
        {
            if (dict == null) throw new ArgumentNullException("dict");
            return (T)(dict[key] ?? @default);
        }
    }
}