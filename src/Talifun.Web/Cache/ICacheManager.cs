using System;
using System.Web.Caching;

namespace Talifun.Web
{
    public interface ICacheManager
    {
        T Get<T>(string key) where T : class;

        void Insert<T>(
            string key,
            T value,
            CacheDependency dependencies,
            DateTime absoluteExpiration,
            TimeSpan slidingExpiration,
            CacheItemPriority priority,
            CacheItemRemovedCallback onRemoveCallback) where T : class;

        T Remove<T>(string key) where T : class;
    }
}