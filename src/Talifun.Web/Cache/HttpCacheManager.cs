using System.Web;

namespace Talifun.Web
{
    public class HttpCacheManager : ICacheManager
    {
        protected readonly System.Web.Caching.Cache Cache = HttpRuntime.Cache;

        public T Get<T>(string key) where T: class
        {
            return Cache.Get(key) as T;
        }

        public void Insert<T>(string key, T value, System.Web.Caching.CacheDependency dependencies, System.DateTime absoluteExpiration, System.TimeSpan slidingExpiration, System.Web.Caching.CacheItemPriority priority, System.Web.Caching.CacheItemRemovedCallback onRemoveCallback) where T : class
        {
            Cache.Insert(key, value, dependencies, absoluteExpiration, slidingExpiration, priority, onRemoveCallback);
        }

        public T Remove<T>(string key) where T : class
        {
            return Cache.Remove(key) as T;
        }
    }
}
