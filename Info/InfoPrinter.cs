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

namespace Info
{
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    public class InfoPrinter
    {
        [Export("Owin.Application")]
        public Task<Result> PrintInfo(IDictionary<string,object> env, IDictionary<string,string[]> headers, Stream body)
        {
            try
            {
                if (env.GetPath().ToLower().Equals("/info", StringComparison.CurrentCultureIgnoreCase))
                {
                    return HandleRequest();
                }

                return TaskHelper.NotFound();
            }
            catch (Exception ex)
            {
                return TaskHelper.Error(ex);
            }
        }

        private static Task<Result> HandleRequest()
        {
            return TaskHelper.Completed(null, 200,
                                        new Dictionary<string, string[]> {{"Content-Type", new[] {"text/html"}}},
                                        WriteHtml);
        }

        private static Task WriteHtml(Stream s)
        {
            var bytes =
                Encoding.UTF8.GetBytes(
                    "<html><body><h1>This server is running on <a href=\"http://github.com/markrendle/Fix\">Fix</a>.</h1></body></html>");
            return s.WriteAsync(bytes, 0, bytes.Length);
        }
    }
}
