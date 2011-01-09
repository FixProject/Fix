using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Text;
using OwinHelpers;
using App = System.Action<System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string,object>>, System.Func<byte[]>, System.Action<int, System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, string>>, System.IObservable<byte[]>>, System.Delegate>;
using ResponseHandler = System.Action<int, System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, string>>, System.IObservable<byte[]>>;

namespace Print
{
    public class RequestPrinter
    {
        [Export("Owin.Application")]
        public void PrintRequest(IEnumerable<KeyValuePair<string,object>> env, Func<byte[]> body,
            ResponseHandler responseHandler, Delegate next)
        {
            try
            {
                if (!env.GetScriptName().ToLower().Contains("/info"))
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
                responseHandler(0, null, new ExceptionBody(ex));
            }
        }

        private static void HandlePrintRequest(IDictionary<string,object> env, ResponseHandler responseHandler)
        {
            var builder = new StringBuilder("<html><body>");
            builder.AppendFormat("<p>{0}</p>", ConstructUri(env));
            builder.AppendFormat("<p>{0}</p>", env["REQUEST_METHOD"]);
            foreach (var header in env)
            {
                builder.AppendFormat("<p><strong>{0}</strong>: {1}</p>", header.Key, header.Value);
            }
            builder.Append("</body></html>");
            var body = new StringBody(builder.ToString());
            var headers = new Dictionary<string, string>
                              {
                                  { "Content-Type", "text/html" },
                                  { "Content-Length", body.Length.ToString() }
                              };
            responseHandler(200, headers, body);
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
