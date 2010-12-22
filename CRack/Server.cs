using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace CRack
{
    public class Server
    {
        readonly HttpListener _listener;

        public Server(string prefix)
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add(prefix);
        }

        public void Start(Middleware middleware)
        {
            _listener.Start();
            _listener.BeginGetContext(GotContext, middleware);
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
                var middleware = (Middleware) result.AsyncState;

                middleware(context.Request.Url.ToString(),
                    context.Request.HttpMethod,
                    context.Request.Headers.ToKeyValuePairs(),
                    context.Request.InputStream.ToBytes(),
                    (statusCode, statusDescription, headers, body) => Respond(context, statusCode, statusDescription, headers, body), null);

                _listener.BeginGetContext(GotContext, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        static void Respond(HttpListenerContext context, int statusCode, string status, IEnumerable<KeyValuePair<string, string>> headers, byte[] body)
        {
            try
            {
                context.Response.StatusCode = statusCode;
                context.Response.StatusDescription = status;
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
                context.Response.Close(body, false);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

    }
}
