using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CRack
{
    public delegate void Middleware(
        string uri, string method, IEnumerable<KeyValuePair<string, string>> headers, byte[] body, ResponseHandler responseHandler, Middleware next);
}
