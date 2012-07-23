using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace TestServer
{
    using System.Threading;
    using OwinEnvironment = IDictionary<string, object>;
    using OwinHeaders = IDictionary<string, string[]>;
    using ResponseHandler = Func<int, IDictionary<string, string[]>, Func<Stream, System.Threading.CancellationToken, Task>, Task>;
    using App = Func<IDictionary<string, object>, IDictionary<string, string[]>, Stream, System.Threading.CancellationToken, Func<int, IDictionary<string, string[]>, Func<Stream, System.Threading.CancellationToken, Task>, Task>, Delegate, Task>;
    using Starter = Action<Func<IDictionary<string, object>, IDictionary<string, string[]>, Stream, System.Threading.CancellationToken, Func<int, IDictionary<string, string[]>, Func<Stream, System.Threading.CancellationToken, Task>, Task>, Delegate, Task>>;
    using System.Linq;

    public class Server : IDisposable
    {
        readonly HttpListener _listener;

        public Server(string prefix)
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add(prefix);
        }

        public void Start(App app)
        {
            _listener.Start();
            _listener.BeginGetContext(GotContext, app);
        }

        public void Stop()
        {
            try
            {
                _listener.Stop();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void GotContext(IAsyncResult result)
        {
            try
            {
                var context = _listener.EndGetContext(result);
                var app = (App) result.AsyncState;
                var env = CreateEnvironmentHash(context.Request);
                var headers = CreateRequestHeaders(context.Request);
                app(env, headers, context.Request.InputStream, CancellationToken.None,
                    (status, outputHeaders, bodyDelegate) =>
                        {
                            context.Response.StatusCode = status > 0 ? status : 404;
                            WriteHeaders(outputHeaders, context);
                            return bodyDelegate(context.Response.OutputStream, CancellationToken.None);
                        }, null)
                        .ContinueWith(t => context.Response.Close());

                _listener.BeginGetContext(GotContext, app);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private static void WriteHeaders(OwinHeaders outputHeaders, HttpListenerContext context)
        {
            if (outputHeaders != null)
            {
                context.Response.Headers.Clear();
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

        private static bool SpecialCase(string key, string[] value, HttpListenerResponse response)
        {
            if (key.Equals("Content-Length", StringComparison.OrdinalIgnoreCase))
            {
                response.ContentLength64 = long.Parse(value[0]);
                return true;
            }
            if (key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
            {
                response.ContentType = value[0];
                return true;
            }
            return false;
        }

        private static IDictionary<string, object> CreateEnvironmentHash(HttpListenerRequest request)
        {
            return new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
                {

                    {"owin.RequestMethod", request.HttpMethod},
                    {"owin.RequestPath", request.Url.AbsolutePath},
                    {"owin.RequestPathBase", string.Empty},
                    {"owin.RequestQueryString", request.Url.Query},
                    {"host.ServerName", request.Url.Host},
                    {"host.ServerPort", request.Url.Port.ToString()},
                    {"owin.RequestProtocol", "HTTP/" + request.ProtocolVersion.ToString(2)},
                    {"owin.RequestScheme", request.Url.Scheme},
                    {"owin.Version", "1.0"},
                };
        }

        private static IDictionary<string,string[]> CreateRequestHeaders(HttpListenerRequest request)
        {
            return request.Headers.AllKeys.ToDictionary(k => k, k => request.Headers.GetValues(k),
                                                        StringComparer.OrdinalIgnoreCase);
        }

        public void Dispose()
        {
            ((IDisposable)_listener).Dispose();
        }

        private static string GetStatusText(int status)
        {
            return StatusTexts.ContainsKey(status) ? StatusTexts[status] : string.Empty;
        }

        private static readonly Dictionary<int, string> StatusTexts = new Dictionary<int, string>
                                                                          {
                                                                              {200, "OK"},
                                                                          };
    }
}
