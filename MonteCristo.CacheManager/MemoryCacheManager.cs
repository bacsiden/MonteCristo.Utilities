using Microsoft.Extensions.Caching.Memory;
using System;

namespace MonteCristo.CacheManager
{
    public class MemoryCacheManager : ICacheManager
    {
        private readonly IMemoryCache _cache;

        public MemoryCacheManager(IMemoryCache cache)
        {
            _cache = cache;
        }

        public void Create(object key, object value) => _cache.Set(key, value);

        public T Get<T>(object key)
        {
            if (_cache.TryGetValue<T>(key, out T value))
                return value;
            else
                return default;
        }

        public T GetOrCreate<T>(object key, Func<T> action) => _cache.GetOrCreate(key, m => action.Invoke());

        public void Remove(object key) => _cache.Remove(key);
    }
}
