using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CacheLib
{
    public class CacheItemRemovedEventArgs : EventArgs
    {
        public string Key { get; set; }


        public CacheItemRemovedEventArgs(string key)
        {
            Key = key;
        }
    }
}
