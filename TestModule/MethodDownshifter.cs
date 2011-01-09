using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using OwinHelpers;
using App = System.Action<System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string,object>>, System.Func<byte[]>, System.Action<int, System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, string>>, System.Func<byte[]>>, System.Action<System.Exception>, System.Delegate>;
using ResponseHandler = System.Action<int, System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, string>>, System.Func<byte[]>>;

namespace TestModule
{
    public class MethodDownshifter
    {
        [Export("Owin.Middleware")]
        public void DownshiftMethod(IEnumerable<KeyValuePair<string,object>> env, Func<byte[]> body,
            ResponseHandler responseHandler, Action<Exception> exceptionHandler, Delegate next)
        {
            var nextInfix = next as App;
            if (nextInfix != null)
            {
                env = env.Mutate(kvp => kvp.Key.Equals("REQUEST_METHOD"),
                                 kvp => new KeyValuePair<string, object>("REQUEST_METHOD", kvp.Value.ToString().ToLower()));
                nextInfix(env, body, responseHandler, exceptionHandler, null);
            }
        }
    }
}
