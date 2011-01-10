using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using OwinHelpers;
using ResponseHandler = System.Action<int, System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, string>>, System.Action<System.Action<System.ArraySegment<byte>>, System.Action<System.IO.FileInfo>, System.Action, System.Action<System.Exception>>>;
using App = System.Action<System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, object>>, System.Func<byte[]>, System.Action<int, System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, string>>, System.Action<System.Action<System.ArraySegment<byte>>, System.Action<System.IO.FileInfo>, System.Action, System.Action<System.Exception>>>, System.Delegate>;

namespace FileHandler
{
    public class FileReturner
    {
        [Export("Owin.Application")]
        public void PrintInfo(IEnumerable<KeyValuePair<string, object>> env, Func<byte[]> body,
            ResponseHandler responseHandler, Delegate next)
        {
            try
            {
                if (env.GetScriptName().ToLower().Equals("/index.html", StringComparison.CurrentCultureIgnoreCase))
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
            var fileInfo = new FileInfo(Path.Combine(Environment.CurrentDirectory, "index.html"));
            responseHandler.WriteFile(fileInfo, "text/html");
        }
    }
}
