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
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web;

    public class FileReturner
    {
        [Export("Owin.Application")]
        public Task ReturnFile(IDictionary<string, object> env)
        {
            var tcs = new TaskCompletionSource<int>();
            if (env.GetPath().ToLower().Equals("/index.html", StringComparison.CurrentCultureIgnoreCase))
            {
                try
                {
                    env["owin.ResponseHeaders"] = new Dictionary<string, string[]>
                                                      {
                                                          {
                                                              "Content-Type",
                                                              new[] {"text/html"}
                                                          }
                                                      };
                    HandleRequest(env);
                    env["owin.ResponseStatusCode"] = 200;
                    tcs.SetResult(0);
                }
                catch (Exception ex)
                {
                    Trace.TraceError(ex.Message);
                    tcs.SetCanceled();
                }
            }
            else
            {
                tcs.SetCanceled();
            }
            return tcs.Task;
        }

        private static void HandleRequest(IDictionary<string, object> env)
        {
            var responseStream = (Stream) env["owin.ResponseBody"];
            FileInfo fileInfo = null;
            if (env.ContainsKey("aspnet.Context"))
            {
                var context = (HttpContext) env["aspnet.Context"];
                fileInfo = new FileInfo(Path.Combine(context.Server.MapPath("bin"), "index.html"));
            }
            else
            {
                fileInfo = new FileInfo(Path.Combine(Environment.CurrentDirectory, "index.html"));
            }
            using (var source = fileInfo.OpenRead())
            {
                source.CopyTo(responseStream);
            }
        }
    }
}
