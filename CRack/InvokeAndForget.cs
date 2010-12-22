using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RequestHandler = System.Action<string, string, System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, string>>, byte[], System.Action<int, string, System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, string>>, byte[]>>;

namespace CRack
{
    static class InvokeAndForgetEx
    {
        public static void InvokeAndForget(this RequestHandler handler, string arg1, string arg2, IEnumerable<KeyValuePair<string, string>> arg3, byte[] arg4, Action<int, string, IEnumerable<KeyValuePair<string, string>>, byte[]> arg5)
        {
            handler.BeginInvoke(arg1, arg2, arg3, arg4, arg5, handler.EndInvoke, null);
        }
    }
}
