using System;
using System.Collections.Generic;
using Pipe = System.Action<string, string, System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, string>>, byte[], System.Action<int, string, System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, string>>, byte[]>, System.Delegate>;

namespace TestModule
{
    public class MethodDownshifter
    {
        public void DownshiftMethod(string url, string method, IEnumerable<KeyValuePair<string, string>> headers, byte[] body, Action<int, string, IEnumerable<KeyValuePair<string, string>>, byte[]> responseHandler, Delegate next)
        {
            var nextPipe = next as Pipe;
            if (nextPipe != null)
                nextPipe(url, method.ToLower(), headers, body, responseHandler, null);
        }
    }
}
