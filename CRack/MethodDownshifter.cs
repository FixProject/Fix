using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CRack
{
    class MethodDownshifter
    {
        public void DownshiftMethod(string url, string method, IEnumerable<KeyValuePair<string, string>> headers, byte[] body, ResponseHandler responseHandler, Middleware next)
        {
            next(url, method.ToLower(), headers, body, responseHandler, null);
        }
    }
}
