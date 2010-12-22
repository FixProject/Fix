using System;
using System.Collections.Generic;
using Pipe = System.Action<string, string, System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, string>>, System.Func<byte[]>, System.Action<int, string, System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, string>>, System.Func<byte[]>>, System.Delegate>;

namespace TestModule
{
    public class MethodDownshifter
    {
        public void DownshiftMethod(string url, string method, IEnumerable<KeyValuePair<string, string>> headers, Func<byte[]> body, Action<int, string, IEnumerable<KeyValuePair<string, string>>, Func<byte[]>> responseHandler, Delegate next)
        {
            var nextPipe = next as Pipe;
            if (nextPipe != null)
                nextPipe(url, method.ToLower(), headers, body, responseHandler, null);
        }
    }
}
