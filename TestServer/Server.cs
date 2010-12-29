using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Infix = System.Action<System.Collections.Generic.IDictionary<string, string>, System.Func<byte[]>, System.Action<int, System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, string>>, System.Func<byte[]>>, System.Action<System.Exception>, System.Delegate>;
using RequestHandler = System.Action<System.Collections.Generic.IDictionary<string, string>, System.Func<byte[]>, System.Action<int, System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, string>>, System.Func<byte[]>>, System.Action<System.Exception>>;
using ResponseHandler = System.Action<int, System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, string>>, System.Func<byte[]>>;

namespace TestServer
{
    public class Server : IDisposable
    {
        readonly HttpListener _listener;

        public Server(string prefix)
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add(prefix);
        }

        public void Start(Infix pipe)
        {
            _listener.Start();
            _listener.BeginGetContext(GotContext, pipe);
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
                var pipe = (Infix) result.AsyncState;
                var env = CreateEnvironmentHash(context.Request);
                pipe(env,
                    () => context.Request.InputStream.ToBytes(),
                    (statusCode, headers, body) => Respond(context, env, statusCode, headers, body),
                    exception => HandleException(context, env, exception),
                    null);

                _listener.BeginGetContext(GotContext, pipe);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private static IDictionary<string,string> CreateEnvironmentHash(HttpListenerRequest request)
        {
            return new Dictionary<string, string>
                       {
                           {"REQUEST_METHOD", request.HttpMethod},
                           {"SCRIPT_NAME", request.Url.AbsolutePath},
                           {"PATH_INFO", string.Empty},
                           {"QUERY_STRING", request.Url.Query},
                           {"SERVER_NAME", request.Url.Host},
                           {"SERVER_PORT", request.Url.Port.ToString()},
                           {"SERVER_PROTOCOL", "HTTP/" + request.ProtocolVersion.ToString(2)},
                           {"url_scheme",request.Url.Scheme},
                       };
        }

        static void Respond(HttpListenerContext context, IDictionary<string,string> env, int status, IEnumerable<KeyValuePair<string, string>> headers, Func<byte[]> body)
        {
            try
            {
                context.Response.StatusCode = status;
                context.Response.StatusDescription = GetStatusText(status);
                foreach (var header in headers)
                {
                    if (header.Key.Equals("content-length", StringComparison.CurrentCultureIgnoreCase))
                    {
                        context.Response.ContentLength64 = long.Parse(header.Value);
                        continue;
                    }
                    if (header.Key.Equals("content-type", StringComparison.CurrentCultureIgnoreCase))
                    {
                        context.Response.ContentType = header.Value;
                        continue;
                    }
                    context.Response.Headers[header.Key] = header.Value;
                }
                context.Response.Close(body(), false);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        static void HandleException(HttpListenerContext context, IDictionary<string,string> env, Exception exception)
        {
            
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
