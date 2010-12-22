using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace CRack
{
    static class NameValueCollectionEx
    {
        public static IEnumerable<KeyValuePair<string, string>> ToKeyValuePairs(this NameValueCollection nameValueCollection)
        {
            return from key in nameValueCollection.AllKeys
                   from value in nameValueCollection.GetValues(key)
                   select new KeyValuePair<string, string>(key, value);
        }
    }
}