using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CacheLib
{
    public sealed class CacheNotifyMediator
    {
        private static readonly object _instancelock = new object();

        private static CacheNotifyMediator _instance = null;

        private CacheNotifyMediator() { }

        public static CacheNotifyMediator GetInstance()
        {
            if (_instance == null)
            {
                lock (_instancelock)
                {
                    if (_instance == null)
                    {
                        _instance = new CacheNotifyMediator();
                    }
                }
            }
            return _instance;
        }

        public event EventHandler<CacheItemRemovedEventArgs> CacheItemRemovedChanged;

        public void OnCacheItemRemoved(object sender, string key)
        { 
            var cacheItemRemovedChanged = CacheItemRemovedChanged as EventHandler<CacheItemRemovedEventArgs>;

            if (cacheItemRemovedChanged != null)
            {
                cacheItemRemovedChanged(sender, new CacheItemRemovedEventArgs(key));
            }
        }

    }
}
