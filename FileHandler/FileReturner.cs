using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using OwinHelpers;
//using ResponseHandler = System.Action<int, System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, string>>, System.Action<System.Action<System.ArraySegment<byte>>, System.Action<System.IO.FileInfo>, System.Action, System.Action<System.Exception>>>;
//using App = System.Action<System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, object>>, System.Func<byte[]>, System.Action<int, System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, string>>, System.Action<System.Action<System.ArraySegment<byte>>, System.Action<System.IO.FileInfo>, System.Action, System.Action<System.Exception>>>, System.Delegate>;

using BodyDelegate = System.Func<System.IO.Stream,System.Threading.CancellationToken,System.Threading.Tasks.Task>;
using ResponseHandler = System.Func<int, System.Collections.Generic.IDictionary<string, string[]>, System.Func<System.IO.Stream, System.Threading.CancellationToken, System.Threading.Tasks.Task>, System.Threading.Tasks.Task>;
using App = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Collections.Generic.IDictionary<string, string[]>, System.IO.Stream, System.Threading.CancellationToken, System.Func<int, System.Collections.Generic.IDictionary<string, string[]>, System.Func<System.IO.Stream, System.Threading.CancellationToken, System.Threading.Tasks.Task>, System.Threading.Tasks.Task>, System.Delegate, System.Threading.Tasks.Task>;

namespace FileHandler
{
    using System.Threading;
    using System.Threading.Tasks;

    public class FileReturner
    {
        [Export("Owin.Application")]
        public Task ReturnFile(IDictionary<string, object> env, IDictionary<string, string[]> headers, Stream body, CancellationToken cancellationToken, ResponseHandler responseHandler, Delegate next)
        {
            try
            {
                if (env.GetPath().ToLower().Equals("/index.html", StringComparison.CurrentCultureIgnoreCase))
                {
                    return HandleRequest(responseHandler);
                }
                else
                {
                    return next.InvokeAsNextApp(env, headers, body, cancellationToken, responseHandler);
                }
            }
            catch (Exception ex)
            {
                return responseHandler(0, null, Body.FromException(ex));
            }
        }

        private static Task HandleRequest(ResponseHandler responseHandler)
        {
            var fileInfo = new FileInfo(Path.Combine(Environment.CurrentDirectory, "index.html"));
            return responseHandler.WriteFile(fileInfo, "text/html");
        }
    }
}
