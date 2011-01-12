using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Text;
using OwinHelpers;
using App = System.Action<System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, object>>, System.Func<byte[]>, System.Action<int, System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, string>>, System.IObservable<System.ArraySegment<byte>>>, System.Delegate>;
using ResponseHandler = System.Action<int, System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, string>>, System.IObservable<System.ArraySegment<byte>>>;

namespace Print
{
    public sealed class RequestPrinter
    {
        [Export("Owin.Application")]
        public void PrintRequest(IEnumerable<KeyValuePair<string,object>> env, Func<byte[]> body,
            ResponseHandler responseHandler, Delegate next)
        {
            try
            {
                var scriptName = env.GetScriptName().ToLower();
                if (!(scriptName.Contains("/info") || scriptName.Contains(".")))
                {
                    HandlePrintRequest(env.ToDictionary(), responseHandler);
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

        private static void HandlePrintRequest(IDictionary<string,object> env, ResponseHandler responseHandler)
        {
            responseHandler.WriteHtml(() => BuildHtml(env));
        }

        private static string BuildHtml(IDictionary<string, object> env)
        {
            var builder = new StringBuilder("<html><body>");
            builder.AppendFormat("<p>{0}</p>", ConstructUri(env));
            builder.AppendFormat("<p>{0}</p>", env["REQUEST_METHOD"]);
            foreach (var header in env)
            {
                builder.AppendFormat("<p><strong>{0}</strong>: {1}</p>", header.Key, header.Value);
            }
            builder.Append("</body></html>");
            return builder.ToString();
        }

        private static string ConstructUri(IDictionary<string,object> env)
        {
            var builder = new StringBuilder(env["url_scheme"] + "://" + env["SERVER_NAME"]);
            if (env["SERVER_PORT"].ToString() != "80")
            {
                builder.AppendFormat(":{0}", env["SERVER_PORT"]);
            }
            if (!string.IsNullOrEmpty(env["SCRIPT_NAME"].ToString()))
            {
                builder.AppendFormat("{0}", env["SCRIPT_NAME"]);
            }
            return builder.ToString();
        }
    }
}
