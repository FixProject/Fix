using System;
using System.Collections.Generic;
using RequestHandler = System.Action<System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string,object>>, System.Func<byte[]>, System.Action<int, System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, string>>, System.Func<byte[]>>, System.Action<System.Exception>, System.Delegate>;
using ResponseHandler = System.Action<int, System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, string>>, System.Func<byte[]>>;

namespace Fix
{
    static class InvokeAndForgetEx
    {
        public static void InvokeAndForget(this RequestHandler handler, IEnumerable<KeyValuePair<string,object>> env, Func<byte[]> body, ResponseHandler responseHandler, Action<Exception> exceptionHandler, Delegate next)
        {
            handler.BeginInvoke(env, body, responseHandler, exceptionHandler, next, handler.EndInvoke, null);
        }
    }
}
