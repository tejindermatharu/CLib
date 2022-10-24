using System.Collections.Concurrent;
using System.Configuration;
using static CacheLib.CustomCache;

namespace CacheLib
{
    public sealed class CustomCache
    {
        private static readonly object _instancelock = new object();

        private Dictionary<string, Cache> _cache = new Dictionary<string, Cache>();

        private static CustomCache instance = null;

        private readonly int _maxCacheSize;

        private CustomCache(int maxCacheSize)
        {
            _maxCacheSize = maxCacheSize;
        }

        public static CustomCache GetInstance
        {
            get
            {   
                if (instance == null)
                {
                    throw new Exception("Cache not intialised, call Initialise first");
                }

                return instance;
            }
        }

        /// <summary>
        /// This is so that we can initialise the instance by passing in parameter config upon startup of the client app
        /// </summary>
        /// <param name="maxCacheSize"></param>
        /// <exception cref="Exception"></exception>
        public static void Initialise(int maxCacheSize)
        {
            if (instance == null)
            {
                lock (_instancelock)
                {
                    if (instance == null)
                    {
                        instance = new CustomCache(maxCacheSize);                       
                    }
                }
            }
        }

        public void AddReplace(string key, object value)
        {
            if (value == null) throw new ArgumentNullException("value cannot be null");

            lock (_instancelock)
            {
                if (value != null)
                {
                    if (_cache.Count >= _maxCacheSize)
                    {
                        var leastUsedCacheItem = _cache.MinBy(x => x.Value.WhenUsed);
                        _cache.Remove(leastUsedCacheItem.Key, out Cache cacheRemoved);

                        CacheNotifyMediator.GetInstance().OnCacheItemRemoved(this, leastUsedCacheItem.Key);
                    }

                    var item = _cache.SingleOrDefault(x => x.Key == key);

                    //Rather than updating, just remove it
                    if (_cache.ContainsKey(key))
                    {
                        _cache.Remove(key);
                    }
                    
                    var cache = new Cache(value, DateTime.UtcNow);
                    _cache.Add(key, cache); 
                }
            }
        }

        /// <summary>
        /// TryGetValue using generics so that you get the value in the desired expected type
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidCastException"></exception>
        public bool TryGetValue<T>(string key, out T value)
        {
            if (key == null) throw new ArgumentNullException("Key cannot be null");

            lock (_instancelock)
            {
                var cacheItem = _cache.TryGetValue(key, out Cache? foundCache);

                if (cacheItem == true && foundCache != null)
                {
                    foundCache.WhenUsed = DateTime.UtcNow;

                    try
                    {
                        value = (T)foundCache.Value;
                    }
                    catch (InvalidCastException e)
                    {
                        throw new InvalidCastException($"Cannot convert object {foundCache.Value.GetType()} to type { typeof(T)}", e);
                    }

                    return true;
                }
                else
                {
                    value = default(T);
                    return false;
                }
            }
        }

        public int GetItemsCount()
        {
            return _cache.Count;
        }

        public void RemoveAllFromCache()
        {
            lock (_instancelock)
            {
                foreach (var cacheItem in _cache)
                {
                    _cache.Remove(cacheItem.Key);

                    CacheNotifyMediator.GetInstance().OnCacheItemRemoved(this, cacheItem.Key);
                }
            }
        }
    }
}