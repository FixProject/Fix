using System;
using System.Collections.Generic;
using RequestHandler = System.Action<System.Collections.Generic.IDictionary<string, string>, System.Func<byte[]>, System.Action<int, System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, string>>, System.Func<byte[]>>>;
using ResponseHandler = System.Action<int, System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, string>>, System.Func<byte[]>>;

namespace Fix
{
    static class InvokeAndForgetEx
    {
        public static void InvokeAndForget(this RequestHandler handler, IDictionary<string, string> env, Func<byte[]> body, ResponseHandler responseHandler)
        {
            handler.BeginInvoke(env, body, responseHandler, handler.EndInvoke, null);
        }
    }
}
