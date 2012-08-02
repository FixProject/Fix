using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Text;
using OwinHelpers;
using AppFunc = System.Func< // Call
        System.Collections.Generic.IDictionary<string, object>, // Environment
        System.Collections.Generic.IDictionary<string, string[]>, // Headers
        System.IO.Stream, // Body
        System.Threading.Tasks.Task<System.Tuple< //Result
            System.Collections.Generic.IDictionary<string, object>, // Properties
            int, // Status
            System.Collections.Generic.IDictionary<string, string[]>, // Headers
            System.Func< // Body
                System.IO.Stream, // Output
                System.Threading.Tasks.Task>>>>; // Done
using Result = System.Tuple< //Result
        System.Collections.Generic.IDictionary<string, object>, // Properties
        int, // Status
        System.Collections.Generic.IDictionary<string, string[]>, // Headers
        System.Func< // Body
            System.IO.Stream, // Output
            System.Threading.Tasks.Task>>; // Done

namespace Print
{
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    public class RequestPrinter
    {
        [Export("Owin.Application")]
        public Task<Result> PrintRequest(IDictionary<string, object> env, IDictionary<string, string[]> headers, Stream body)
        {
            try
            {
                var scriptName = env.GetPath().ToLower();
                if (!(scriptName.Contains("/info") || scriptName.Contains(".")))
                {
                    return HandlePrintRequest(env);
                }
            }
            catch (Exception ex)
            {
                return TaskHelper.Error(ex);
            }
            return TaskHelper.NotFound();
        }

        private static Task<Result> HandlePrintRequest(IDictionary<string, object> env)
        {
            return TaskHelper.Completed(null, 200,
                                        new Dictionary<string, string[]> {{"Content-Type", new[] {"text/html"}}},
                                        stream => stream.WriteAsync(BuildHtml(env)));
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
