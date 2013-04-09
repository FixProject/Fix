using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OwinHelpers
{
    public static class DictionaryExtensions
    {
        public static TValue GetValueOrDefault<TKey,TValue>(this IDictionary<TKey,TValue> dictionary, TKey key, TValue defaultValue)
        {
            TValue value;
            if (!dictionary.TryGetValue(key, out value))
            {
                value = defaultValue;
            }
            return value;
        }

        public static TValue GetValueOrDefault<TKey,TValue>(this IDictionary<TKey,TValue> dictionary, TKey key)
        {
            return GetValueOrDefault(dictionary, key, default(TValue));
        }   
    }
}
