using System;

namespace MonteCristo.CacheManager
{
    public interface ICacheManager
    {
        void Create(object key, object value);
        T Get<T>(object key);

        T GetOrCreate<T>(object key, Func<T> action);

        void Remove(object key);
    }
}