using System;
using System.Collections.Generic;

namespace Fix
{
    static class InvokeAndForgetEx
    {
        public static void InvokeAndForget(this Action<string, string, IEnumerable<KeyValuePair<string, string>>, Func<byte[]>, Action<int, string, IEnumerable<KeyValuePair<string, string>>, Func<byte[]>>> handler, string arg1, string arg2, IEnumerable<KeyValuePair<string, string>> arg3, System.Func<byte[]> arg4, Action<int, string, IEnumerable<KeyValuePair<string, string>>, System.Func<byte[]>> arg5)
        {
            handler.BeginInvoke(arg1, arg2, arg3, arg4, arg5, handler.EndInvoke, null);
        }
    }
}
