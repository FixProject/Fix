using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OwinHelpers
{
    public static class EnumerableDictionaryExtension
    {
        public static IDictionary<TKey,TValue> ToDictionary<TKey,TValue>(this IEnumerable<KeyValuePair<TKey,TValue>> source)
        {
            var alreadyDictionary = source as IDictionary<TKey, TValue>;
            if (alreadyDictionary != null)
            {
                return new Dictionary<TKey, TValue>(alreadyDictionary);
            }
            return source.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public static IEnumerable<KeyValuePair<TKey,TValue>> Mutate<TKey,TValue>(this IEnumerable<KeyValuePair<TKey,TValue>> source,
            Func<KeyValuePair<TKey,TValue>, bool> predicate, Func<KeyValuePair<TKey,TValue>,KeyValuePair<TKey,TValue>> mutator)
        {
            foreach (var pair in source)
            {
                if (predicate(pair))
                {
                    yield return mutator(pair);
                }
                else
                {
                    yield return pair;
                }
            }
        }
    }
}
