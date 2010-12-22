using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CRack
{
    // Action<int, string, IEnumerable<KeyValuePair<string, string>>, byte[]>
    public delegate void ResponseHandler(
        int statusCode, string statusText, IEnumerable<KeyValuePair<string, string>> headers, byte[] body);
}
