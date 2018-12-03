using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.Extensions
{
    public class ExDictionary<K, V> : Dictionary<K, V>
    {
        public V GetValueOrDefault(K key)
        {
            return this.ContainsKey(key) ? this[key] : default(V);
        }
    }
}
