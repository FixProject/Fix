using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace CRack
{
    class CRacker : IDisposable
    {
        readonly HttpListener _listener;
        RequestHandler _handler;
        private Middleware _middleware;

        public CRacker(string prefix)
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add(prefix);
            _handler = EmptyHandler;
            _middleware = (uri, method, headers, body, responseHandler, next) => DefaultMiddleware(uri, method, headers, body, responseHandler, _handler);
        }

        public void AddHandler(RequestHandler handlerToAdd)
        {
            RequestHandler currentHandler;
            RequestHandler newHandler;
            do
            {
                currentHandler = _handler;
                newHandler = GetNewHandler(currentHandler, handlerToAdd);
            } while (!ReferenceEquals(currentHandler, Interlocked.CompareExchange(ref _handler, newHandler, currentHandler)));
        }

        public void AddMiddleware(Middleware middlewareToAdd)
        {
            Middleware currentMiddleware;
            Middleware newMiddleware;
            do
            {
                currentMiddleware = _middleware;
                newMiddleware =
                    (uri, method, headers, body, responseHandler, next) => middlewareToAdd(uri, method, headers, body, responseHandler, currentMiddleware);

            } while (!ReferenceEquals(currentMiddleware, Interlocked.CompareExchange(ref _middleware, newMiddleware, currentMiddleware)));
            
        }

        private static RequestHandler GetNewHandler(RequestHandler currentHandler, RequestHandler handlerToAdd)
        {
            return (RequestHandler) Delegate.Combine(currentHandler,
                                                     new RequestHandler(
                                                         (url, method, headers, body, responseHandler) =>
                                                         handlerToAdd.InvokeAndForget(url, method, headers, body, responseHandler)));
        }

        public void Start()
        {
            _listener.Start();
            _listener.BeginGetContext(GotContext, null);
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

                _middleware(context.Request.Url.ToString(),
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

        public void Dispose()
        {
            ((IDisposable)_listener).Dispose();
        }

        private static void EmptyHandler(string uri, string method, IEnumerable<KeyValuePair<string, string>> headers, byte[] body, ResponseHandler responsehandler)
        {

        }

        private static void DefaultMiddleware(string uri, string method, IEnumerable<KeyValuePair<string, string>> headers, byte[] body, ResponseHandler responseHandler, RequestHandler requestHandler)
        {
            requestHandler(uri, method, headers, body, responseHandler);
        }
    }
}
