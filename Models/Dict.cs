using System.Collections.Generic;

namespace WebStuff.Models
{
    public class Dict<TKey, TValue> : Dictionary<TKey, TValue>
    {
        public TValue Get(TKey key) {
            if (this.ContainsKey(key))
            {
                return this[key];
            }
            else
            {
                return default(TValue);
            }
        }
    }
}
