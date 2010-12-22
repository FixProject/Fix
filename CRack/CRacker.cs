using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace CRack
{
    class CRacker : IDisposable
    {
        readonly HttpListener _listener = new HttpListener();
        readonly Action
                <string, string, IEnumerable<KeyValuePair<string, string>>, byte[],
                    Action<int, string, IEnumerable<KeyValuePair<string, string>>, byte[]>> _processor;

        public CRacker(Action<string, string, IEnumerable<KeyValuePair<string, string>>, byte[], Action<int, string, IEnumerable<KeyValuePair<string, string>>, byte[]>> processor)
        {
            _processor = processor;
        }

        public void Start()
        {
            _listener.Prefixes.Add("http://*:8080/");
            _listener.Start();
            _listener.BeginGetContext(GotContext, null);
        }

        public void Stop()
        {
            _listener.Stop();
        }

        private void GotContext(IAsyncResult result)
        {
            try
            {
                var context = _listener.EndGetContext(result);
                _processor.BeginInvoke(context.Request.Url.ToString(), context.Request.HttpMethod,
                    context.Request.Headers.ToKeyValuePairs(),
                    context.Request.InputStream.ToBytes(),
                    (statusCode, statusDescription, headers, body) => Respond(context, statusCode, statusDescription, headers, body),
                    _processor.EndInvoke, null);
                _listener.BeginGetContext(GotContext, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        static void Respond(HttpListenerContext context, int statusCode, string status, IEnumerable<KeyValuePair<string, string>> headers, byte[] body)
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

        public void Dispose()
        {
            ((IDisposable)_listener).Dispose();
        }
    }
}
