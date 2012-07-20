using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Text;
using OwinHelpers;
using BodyDelegate = System.Func<System.IO.Stream,System.Threading.CancellationToken,System.Threading.Tasks.Task>;
using ResponseHandler = System.Func<int, System.Collections.Generic.IDictionary<string, string[]>, System.Func<System.IO.Stream, System.Threading.CancellationToken, System.Threading.Tasks.Task>, System.Threading.Tasks.Task>;
using App = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Collections.Generic.IDictionary<string, string[]>, System.IO.Stream, System.Threading.CancellationToken, System.Func<int, System.Collections.Generic.IDictionary<string, string[]>, System.Func<System.IO.Stream, System.Threading.CancellationToken, System.Threading.Tasks.Task>, System.Threading.Tasks.Task>, System.Delegate, System.Threading.Tasks.Task>;

namespace Print
{
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    public class RequestPrinter
    {
        [Export("Owin.Application")]
        public Task PrintRequest(IDictionary<string, object> env, IDictionary<string, string[]> headers, Stream body, CancellationToken cancellationToken, ResponseHandler responseHandler, Delegate next)
        {
            try
            {
                var scriptName = env.GetPath().ToLower();
                if (!(scriptName.Contains("/info") || scriptName.Contains(".")))
                {
                    return HandlePrintRequest(env.ToDictionary(), responseHandler);
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

        private static Task HandlePrintRequest(IDictionary<string,object> env, ResponseHandler responseHandler)
        {
            return responseHandler.WriteHtml(() => BuildHtml(env));
        }

        private static string BuildHtml(IDictionary<string, object> env)
        {
            var builder = new StringBuilder("<html><body>");
            builder.AppendFormat("<p>{0}</p>", ConstructUri(env));
            builder.AppendFormat("<p>{0}</p>", env["owin.RequestMethod"]);
            foreach (var header in env)
            {
                builder.AppendFormat("<p><strong>{0}</strong>: {1}</p>", header.Key, header.Value);
            }
            builder.Append("</body></html>");
            return builder.ToString();
        }

        private static string ConstructUri(IDictionary<string,object> env)
        {
            var builder = new StringBuilder(env["owin.RequestScheme"] + "://" + env["host.ServerName"]);
            if (env["host.ServerPort"].ToString() != "80")
            {
                builder.AppendFormat(":{0}", env["host.ServerPort"]);
            }
            if (!string.IsNullOrEmpty(env["owin.RequestPath"].ToString()))
            {
                builder.AppendFormat("{0}", env["owin.RequestPath"]);
            }
            return builder.ToString();
        }
    }
}
