using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CRack
{
    public delegate void RequestHandler(
        string uri, string method, IEnumerable<KeyValuePair<string, string>> headers, byte[] body,
        ResponseHandler responseHandler);

    public static class RequestHandlerEx
    {
        public static void InvokeAndForget(this RequestHandler handler, string url, string method, IEnumerable<KeyValuePair<string, string>> requestHeaders, byte[] requestBody, ResponseHandler responseHandler)
        {
            handler.BeginInvoke(url, method, requestHeaders, requestBody, responseHandler, handler.EndInvoke, null);
        }
    }
}
