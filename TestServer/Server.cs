using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using App = System.Action<System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string,object>>, System.Func<byte[]>, System.Action<int, System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, string>>, System.Action<System.Action<System.ArraySegment<byte>>, System.Action, System.Action<System.Exception>>>, System.Delegate>;
using ResponseHandler = System.Action<int, System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, string>>, System.Action<System.Action<System.ArraySegment<byte>>, System.Action, System.Action<System.Exception>>>;
using Body = System.Action<System.Action<System.ArraySegment<byte>>, System.Action, System.Action<System.Exception>>;
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
                app.BeginInvoke(env,
                    () => context.Request.InputStream.ToBytes(),
                    (statusCode, headers, body) => Respond(context, env, statusCode, headers, body),
                    null, app.EndInvoke, null);

                _listener.BeginGetContext(GotContext, app);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private static IEnumerable<KeyValuePair<string,object>> CreateEnvironmentHash(HttpListenerRequest request)
        {
            yield return new KeyValuePair<string, object>("REQUEST_METHOD", request.HttpMethod);
            yield return new KeyValuePair<string, object>("SCRIPT_NAME", request.Url.AbsolutePath);
            yield return new KeyValuePair<string, object>("PATH_INFO", string.Empty);
            yield return new KeyValuePair<string, object>("QUERY_STRING", request.Url.Query);
            yield return new KeyValuePair<string, object>("SERVER_NAME", request.Url.Host);
            yield return new KeyValuePair<string, object>("SERVER_PORT", request.Url.Port.ToString());
            yield return
                new KeyValuePair<string, object>("SERVER_PROTOCOL", "HTTP/" + request.ProtocolVersion.ToString(2));
            yield return new KeyValuePair<string, object>("url_scheme", request.Url.Scheme);
        }

        static void Respond(HttpListenerContext context, IEnumerable<KeyValuePair<string,object>> env, int status, IEnumerable<KeyValuePair<string, string>> headers, Body body)
        {
            try
            {
                context.Response.StatusCode = status;
                context.Response.StatusDescription = GetStatusText(status);
                if (headers != null)
                {
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
                }
                var bodyObserver = new BodyObserver(context);
                body(bodyObserver.OnNext, bodyObserver.OnCompleted, bodyObserver.OnError);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
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

        private class BodyObserver : IObserver<ArraySegment<byte>>
        {
            private readonly HttpListenerContext _context;

            public BodyObserver(HttpListenerContext context)
            {
                _context = context;
            }

            public void OnNext(ArraySegment<byte> value)
            {
                _context.Response.OutputStream.Write(value.Array, value.Offset, value.Count);
            }

            public void OnError(Exception error)
            {
                _context.Response.Close();
            }

            public void OnCompleted()
            {
                _context.Response.Close();
            }
        }
    }
}
