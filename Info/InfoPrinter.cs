using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Text;
using OwinHelpers;
using BodyDelegate = System.Func<System.IO.Stream,System.Threading.CancellationToken,System.Threading.Tasks.Task>;
using ResponseHandler = System.Func<int, System.Collections.Generic.IDictionary<string, string[]>, System.Func<System.IO.Stream, System.Threading.CancellationToken, System.Threading.Tasks.Task>, System.Threading.Tasks.Task>;
using App = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Collections.Generic.IDictionary<string, string[]>, System.IO.Stream, System.Threading.CancellationToken, System.Func<int, System.Collections.Generic.IDictionary<string, string[]>, System.Func<System.IO.Stream, System.Threading.CancellationToken, System.Threading.Tasks.Task>, System.Threading.Tasks.Task>, System.Delegate, System.Threading.Tasks.Task>;

namespace Info
{
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    public class InfoPrinter
    {
        [Export("Owin.Application")]
        public Task PrintInfo(IDictionary<string,object> env, IDictionary<string,string[]> headers, Stream body, CancellationToken cancellationToken, ResponseHandler responseHandler, Delegate next)
        {
            try
            {
                if (env.GetPath().ToLower().Equals("/info", StringComparison.CurrentCultureIgnoreCase))
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
            return responseHandler.WriteHtml(() => "<html><body><h1>This server is running on <a href=\"http://github.com/markrendle/Fix\">Fix</a>.</h1></body></html>");
        }
    }
}
