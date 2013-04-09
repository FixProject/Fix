using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Env = System.Collections.Generic.IDictionary<string, object>;
using Headers = System.Collections.Generic.IDictionary<string, string[]>;
using BodyDelegate = System.Func<System.IO.Stream,System.Threading.CancellationToken,System.Threading.Tasks.Task>;
using ResponseHandler = System.Func<int, System.Collections.Generic.IDictionary<string, string[]>, System.Func<System.IO.Stream, System.Threading.CancellationToken, System.Threading.Tasks.Task>, System.Threading.Tasks.Task>;
using App = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Collections.Generic.IDictionary<string, string[]>, System.IO.Stream, System.Threading.CancellationToken, System.Func<int, System.Collections.Generic.IDictionary<string, string[]>, System.Func<System.IO.Stream, System.Threading.CancellationToken, System.Threading.Tasks.Task>, System.Threading.Tasks.Task>, System.Delegate, System.Threading.Tasks.Task>;

namespace OwinHelpers
{
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    public static class DelegateExtensions
    {
        public static Task InvokeAsNextApp(this Delegate @delegate, Env env, Headers headers, Stream body, CancellationToken token, ResponseHandler responseHandler)
        {
            var nextApp = @delegate as App;
            if (nextApp != null)
            {
                return nextApp(env, headers, body, token, responseHandler, null);
            }

            return TaskHelper.Completed();
        }
    }
}
