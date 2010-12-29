using System;
using System.Collections.Generic;
using RequestHandler = System.Action<System.Collections.Generic.IDictionary<string, string>, System.Func<byte[]>, System.Action<int, System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, string>>, System.Func<byte[]>>, System.Action<System.Exception>>;
using ResponseHandler = System.Action<int, System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, string>>, System.Func<byte[]>>;

namespace Fix
{
    static class InvokeAndForgetEx
    {
        public static void InvokeAndForget(this RequestHandler handler, IDictionary<string, string> env, Func<byte[]> body, ResponseHandler responseHandler, Action<Exception> exceptionHandler)
        {
            handler.BeginInvoke(env, body, responseHandler, exceptionHandler, handler.EndInvoke, null);
        }
    }
}
