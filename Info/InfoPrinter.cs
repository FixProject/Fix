using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Text;
using OwinHelpers;
using ResponseHandler = System.Action<int, System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, string>>, System.Action<System.Action<System.ArraySegment<byte>>, System.Action<System.IO.FileInfo>, System.Action, System.Action<System.Exception>>>;
using App = System.Action<System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, object>>, System.Func<byte[]>, System.Action<int, System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, string>>, System.Action<System.Action<System.ArraySegment<byte>>, System.Action<System.IO.FileInfo>, System.Action, System.Action<System.Exception>>>, System.Delegate>;

namespace Info
{
    public class InfoPrinter
    {
        [Export("Owin.Application")]
        public void PrintInfo(IEnumerable<KeyValuePair<string,object>> env, Func<byte[]> body,
            ResponseHandler responseHandler, Delegate next)
        {
            try
            {
                if (env.GetScriptName().ToLower().Equals("/info", StringComparison.CurrentCultureIgnoreCase))
                {
                    HandleRequest(responseHandler);
                }
                else
                {
                    next.InvokeAsNextApp(env, body, responseHandler);
                }
            }
            catch (Exception ex)
            {
                responseHandler(0, null, Body.FromException(ex));
            }
        }

        private static void HandleRequest(ResponseHandler responseHandler)
        {
            responseHandler.WriteHtml(() => "<html><body><h1>This server is running on <a href=\"http://github.com/markrendle/Fix\">Fix</a>.</h1></body></html>");
        }
    }
}
