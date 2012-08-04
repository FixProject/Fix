namespace Fix.AspNet
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.ComponentModel.Composition.Hosting;
    using System.IO;
    using System.Threading.Tasks;
    using System.Web;
    using Fix;
    using OwinEnvironment = System.Collections.Generic.IDictionary<string, object>;
    using OwinHeaders = System.Collections.Generic.IDictionary<string, string[]>;
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
    using System.Linq;

    internal class Bridge
    {
        private static readonly object SyncApp = new object();
        private static volatile AppFunc _app;

        public static Task RunContext(HttpContext context)
        {
            if (_app == null)
            {
                Initialize(context);
                if (_app == null)
                {
                    throw new InvalidOperationException("No application found.");
                }
            }
            var env = CreateEnvironmentHash(context);
            var tcs = new TaskCompletionSource<object>();
            env.Add(OwinKeys.CallCompleted, tcs.Task);
            var headers = CreateRequestHeaders(context.Request);
            var task = _app(env, headers, context.Request.InputStream);
            return task
                .ContinueWith(t =>
                {
                    context.Response.StatusCode = t.Result.Item2 > 0 ? t.Result.Item2 : 404;
                    WriteHeaders(t.Result.Item3, context);
                    if (t.Result.Item4 != null)
                    {
                        return t.Result.Item4(context.Response.OutputStream);
                    }
                    return TaskHelper.Completed();
                }, TaskContinuationOptions.None)
                .Unwrap()
                .ContinueWith(t => SetOwinCallCompleted(t, tcs));
        }

        private static void SetOwinCallCompleted(Task t, TaskCompletionSource<object> tcs)
        {
            if (t.IsFaulted)
            {
                tcs.TrySetException(t.Exception ?? new Exception("An unknown error occurred."));
            }
            else if (t.IsCanceled)
            {
                tcs.TrySetCanceled();
            }
            else
            {
                tcs.SetResult(null);
            }
        }

        private static void Initialize(HttpContext context)
        {
            lock (SyncApp)
            {
                if (_app == null)
                {
                    var fixer = new Fixer();
                    string path = Path.Combine(context.Request.PhysicalApplicationPath ?? Environment.CurrentDirectory, "bin");
                    using (var catalog = new DirectoryCatalog(path))
                    {
                        var container = new CompositionContainer(catalog);
                        container.ComposeParts(fixer);
                    }
                    _app = fixer.BuildApp();
                }
            }
        }

        private static void WriteHeaders(IEnumerable<KeyValuePair<string, string[]>> outputHeaders, HttpContext context)
        {
            if (outputHeaders != null)
            {
                foreach (var outputHeader in outputHeaders)
                {
                    if (SpecialCase(outputHeader.Key, outputHeader.Value, context.Response)) continue;
                    foreach (var value in outputHeader.Value)
                    {
                        context.Response.Headers.Add(outputHeader.Key, value);
                    }
                }
            }
        }

        private static bool SpecialCase(string key, string[] value, HttpResponse response)
        {
            if (key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
            {
                response.ContentType = value[0];
                return true;
            }
            return false;
        }

        private static OwinEnvironment CreateEnvironmentHash(HttpContext context)
        {
            var request = context.Request;
            return new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
                {
                    {OwinKeys.RequestMethod, request.HttpMethod},
                    {OwinKeys.RequestPath, request.Url.AbsolutePath},
                    {OwinKeys.RequestPathBase, string.Empty},
                    {OwinKeys.RequestQueryString, request.Url.Query.TrimStart('?')},
                    {ServerKeys.LocalIpAddress, request.ServerVariables["LOCAL_ADDR"]},
                    {ServerKeys.RemoteIpAddress, request.ServerVariables["REMOTE_ADDR"]},
                    {ServerKeys.RemotePort, request.ServerVariables["REMOTE_PORT"]},
                    {ServerKeys.LocalPort, request.ServerVariables["SERVER_PORT"]},
                    {OwinKeys.RequestProtocol, request.ServerVariables["HTTP_VERSION"]},
                    {OwinKeys.RequestScheme, request.Url.Scheme},
                    {OwinKeys.Version, "1.0"},
                    {"aspnet.Context", context},
                };
        }

        private static IDictionary<string, string[]> CreateRequestHeaders(HttpRequest request)
        {
            return request.Headers.AllKeys.ToDictionary(k => k, k => request.Headers.GetValues(k),
                                                        StringComparer.OrdinalIgnoreCase);
        }
    }
}
