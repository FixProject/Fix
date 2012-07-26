namespace Fix.AspNet
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.ComponentModel.Composition.Hosting;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web;
    using Fix;
    using OwinEnvironment = System.Collections.Generic.IDictionary<string, object>;
    using OwinHeaders = System.Collections.Generic.IDictionary<string, string[]>;
    using ResponseHandler = System.Func<int, System.Collections.Generic.IDictionary<string, string[]>, System.Func<System.IO.Stream, System.Threading.CancellationToken, System.Threading.Tasks.Task>, System.Threading.Tasks.Task>;
    using App = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Collections.Generic.IDictionary<string, string[]>, System.IO.Stream, System.Threading.CancellationToken, System.Func<int, System.Collections.Generic.IDictionary<string, string[]>, System.Func<System.IO.Stream, System.Threading.CancellationToken, System.Threading.Tasks.Task>, System.Threading.Tasks.Task>, System.Delegate, System.Threading.Tasks.Task>;
    using Starter = System.Action<System.Func<System.Collections.Generic.IDictionary<string, object>, System.Collections.Generic.IDictionary<string, string[]>, System.IO.Stream, System.Threading.CancellationToken, System.Func<int, System.Collections.Generic.IDictionary<string, string[]>, System.Func<System.IO.Stream, System.Threading.CancellationToken, System.Threading.Tasks.Task>, System.Threading.Tasks.Task>, System.Delegate, System.Threading.Tasks.Task>>;
    using System.Linq;

    internal class Bridge
    {
        private static readonly object SyncApp = new object();
        private static volatile App _app;

        public static Task RunContext(HttpContext context)
        {
            if (_app == null)
            {
                Initialize(context);
            }
            var env = CreateEnvironmentHash(context);
            var headers = CreateRequestHeaders(context.Request);
            return _app(env, headers, context.Request.InputStream, CancellationToken.None,
                (status, outputHeaders, bodyDelegate) =>
                {
                    context.Response.StatusCode = status > 0 ? status : 404;
                    WriteHeaders(outputHeaders, context);
                    if (bodyDelegate != null)
                    {
                        return bodyDelegate(context.Response.OutputStream, CancellationToken.None);
                    }
                    return TaskHelper.Completed();
                }, null);
        }

        private static void Initialize(HttpContext context)
        {
            lock (SyncApp)
            {
                if (_app == null)
                {
                    var fixer = new Fixer();
                    string path = Path.Combine(context.Request.PhysicalApplicationPath, "bin");
                    using (var catalog = new DirectoryCatalog(path))
                    {
                        var container = new CompositionContainer(catalog);
                        container.ComposeParts(fixer);
                    }
                    _app = fixer.BuildApp();
                }
            }
        }

        private static void WriteHeaders(OwinHeaders outputHeaders, HttpContext context)
        {
            if (outputHeaders != null)
            {
                //context.Response.Headers.Clear();
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
            //if (key.Equals("Content-Length", StringComparison.OrdinalIgnoreCase))
            //{
            //    response.Con = long.Parse(value[0]);
            //    return true;
            //}
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

                    {"owin.RequestMethod", request.HttpMethod},
                    {"owin.RequestPath", request.Url.AbsolutePath},
                    {"owin.RequestPathBase", string.Empty},
                    {"owin.RequestQueryString", request.Url.Query.TrimStart('?')},
                    {"host.ServerName", request.Url.Host},
                    {"host.ServerPort", request.Url.Port},
                    {"owin.RequestProtocol", request.ServerVariables["HTTP_VERSION"]},
                    {"owin.RequestScheme", request.Url.Scheme},
                    {"owin.Version", "1.0"},
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
