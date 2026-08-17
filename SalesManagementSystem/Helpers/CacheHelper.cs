using System;
using System.Web;
using System.Web.Caching;

namespace SalesManagementSystem.Helpers
{
    public static class CacheHelper
    {
        public static T GetOrSet<T>(string cacheKey, Func<T> getItemCallback, int cacheMinutes = 10) where T : class
        {
            if (HttpRuntime.Cache == null)
            {
                return getItemCallback();
            }

            T item = HttpRuntime.Cache.Get(cacheKey) as T;
            if (item == null)
            {
                item = getItemCallback();
                if (item != null)
                {
                    HttpRuntime.Cache.Insert(
                        cacheKey,
                        item,
                        null,
                        DateTime.Now.AddMinutes(cacheMinutes),
                        Cache.NoSlidingExpiration);
                }
            }
            return item;
        }

        public static void Remove(string cacheKey)
        {
            if (HttpRuntime.Cache != null && HttpRuntime.Cache.Get(cacheKey) != null)
            {
                HttpRuntime.Cache.Remove(cacheKey);
            }
        }

        public static void ClearAllDropdowns()
        {
            if (HttpRuntime.Cache == null) return;
            System.Collections.IDictionaryEnumerator enumerator = HttpRuntime.Cache.GetEnumerator();
            while (enumerator.MoveNext())
            {
                string key = enumerator.Key.ToString();
                if (key.StartsWith("ddl_"))
                {
                    HttpRuntime.Cache.Remove(key);
                }
            }
        }
    }
}
