namespace Fix.AspNet
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.ComponentModel.Composition.Hosting;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web;
    using Fix;
    using OwinEnvironment = System.Collections.Generic.IDictionary<string, object>;
    using OwinHeaders = System.Collections.Generic.IDictionary<string, string[]>;
    using AppFunc = System.Func< // Call
        System.Collections.Generic.IDictionary<string, object>, // Environment
        System.Threading.Tasks.Task>;
    using System.Linq;

    internal class Bridge
    {
        private static readonly object SyncApp = new object();
        private static AppFunc _app;
        private static IDictionary<string, string> _serverVariables;

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
            var task = _app(env);
            return task
                .ContinueWith(t =>
                                  {
                                      if (!env.ContainsKey(OwinKeys.ResponseStatusCode))
                                      {
                                          context.Response.StatusCode = 404;
                                      }
                                      else
                                      {
                                          context.Response.StatusCode = (int) env[OwinKeys.ResponseStatusCode];
                                      }
                                      if (env.ContainsKey(OwinKeys.ResponseHeaders))
                                      {
                                          WriteHeaders((IDictionary<string,string[]>)env[OwinKeys.ResponseHeaders], context);
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

                    var aggregateCatalog = new AggregateCatalog();
                    foreach (var file in Directory.EnumerateFiles(path, "*.dll"))
                    {
                        var justFileName = Path.GetFileName(file);
                        if (justFileName == null) continue;
                        // Skip Microsoft DLLs, because they break MEF
                        if (justFileName.StartsWith("Microsoft.") || justFileName.StartsWith("System.")) continue;
                        var catalog = new AssemblyCatalog(file);
                        aggregateCatalog.Catalogs.Add(catalog);
                    }

                    var container = new CompositionContainer(aggregateCatalog);
                    try
                    {
                        container.ComposeParts(fixer);
                    }
                    catch (ReflectionTypeLoadException ex)
                    {
                        throw;
                    }

                    _app = fixer.BuildApp();
                    _serverVariables = context.Request.ServerVariables.AllKeys
                                              .ToDictionary(v => v, v => context.Request.ServerVariables.Get(v));
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
            string localAddr;
            string remoteAddr;
            string remotePort;
            string serverPort;
            string httpVersion;

            _serverVariables.TryGetValue("LOCAL_ADDR", out localAddr);
            _serverVariables.TryGetValue("REMOTE_ADDR", out remoteAddr);
            _serverVariables.TryGetValue("REMOTE_PORT", out remotePort);
            _serverVariables.TryGetValue("SERVER_PORT", out serverPort);
            _serverVariables.TryGetValue("HTTP_VERSION", out httpVersion);

            return new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
                {
                    {OwinKeys.RequestMethod, request.HttpMethod},
                    {OwinKeys.RequestPath, request.Url.AbsolutePath},
                    {OwinKeys.RequestPathBase, string.Empty},
                    {OwinKeys.RequestQueryString, request.Url.Query.TrimStart('?')},
                    {ServerKeys.LocalIpAddress, localAddr},
                    {ServerKeys.RemoteIpAddress, remoteAddr},
                    {ServerKeys.RemotePort, remotePort},
                    {ServerKeys.LocalPort, serverPort},
                    {OwinKeys.RequestProtocol, httpVersion},
                    {OwinKeys.RequestScheme, request.Url.Scheme},
                    {OwinKeys.RequestBody, request.InputStream},
                    {OwinKeys.RequestHeaders, CreateRequestHeaders(request)},
                    {OwinKeys.Version, "1.0"},
                    {OwinKeys.ResponseBody, context.Response.OutputStream},
                    {OwinKeys.ResponseHeaders, new Dictionary<string,string[]>()},
                    {OwinKeys.CallCancelled, new CancellationToken()},
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
