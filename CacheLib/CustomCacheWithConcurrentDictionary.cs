using System.Collections.Concurrent;
using System.Configuration;
using static CacheLib.CustomCache;

namespace CacheLib
{
    public sealed class CustomCacheWithConcurrentDictionary
    {
        private static readonly object _instancelock = new object();

        private ConcurrentDictionary<string, Cache> _cache = new ConcurrentDictionary<string, Cache>();

        private static CustomCacheWithConcurrentDictionary instance = null;

        private readonly int _maxCacheSize;

        private CustomCacheWithConcurrentDictionary(int maxCacheSize)
        {
            _maxCacheSize = maxCacheSize;
        }

        public static CustomCacheWithConcurrentDictionary GetInstance
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
                        instance = new CustomCacheWithConcurrentDictionary(maxCacheSize);                       
                    }
                }
            }
        }

        public void AddReplace(string key, object value)
        {
            if (value == null) throw new ArgumentNullException("value cannot be null");

            //We still aquire a lock here even though we'ew using a concurrent dictionary so that we can syncronise a potential TryRemove operation as well as an AddOrUpdate
            lock (_instancelock)
            {
                if (value != null)
                {
                    if (_cache.Count >= _maxCacheSize)
                    {
                        var leastUsedCacheItem = _cache.MinBy(x => x.Value.WhenUsed);
                        _cache.TryRemove(leastUsedCacheItem);

                        CacheNotifyMediator.GetInstance().OnCacheItemRemoved(this, leastUsedCacheItem.Key);
                    }                      
                    
                    var cache = new Cache(value, DateTime.UtcNow);
                    _cache.AddOrUpdate(key, cache, (key, o) => cache); 
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

        public int GetItemsCount()
        {
            return _cache.Count;
        }

        public void RemoveAllFromCache()
        {
            foreach (var cacheItem in _cache)
            {
                _cache.TryRemove(cacheItem);

                CacheNotifyMediator.GetInstance().OnCacheItemRemoved(this, cacheItem.Key);
            }
        }        
    }
}