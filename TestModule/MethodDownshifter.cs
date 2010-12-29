using System;
using System.Collections.Generic;
using Infix = System.Action<System.Collections.Generic.IDictionary<string, string>, System.Func<byte[]>, System.Action<int, string, System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, string>>, System.Func<byte[]>>, System.Delegate>;
using ResponseHandler = System.Action<int, string, System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, string>>, System.Func<byte[]>>;

namespace TestModule
{
    public class MethodDownshifter
    {
        public void DownshiftMethod(IDictionary<string, string> env, Func<byte[]> body,
            ResponseHandler responseHandler, Delegate next)
        {
            var nextInfix = next as Infix;
            if (nextInfix != null)
            {
                env["REQUEST_METHOD"] = env["REQUEST_METHOD"].ToLower();
                nextInfix(env, body, responseHandler, null);
            }
        }
    }
}
