using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CRack
{
    public class InfoPrinter
    {
        public void PrintInfo(string uri, string method, IEnumerable<KeyValuePair<string, string>> requestHeaders, byte[] body,
            ResponseHandler responseHandler)
        {
            if (uri.ToLower().Contains("/info"))
            {
                var bytes = Encoding.UTF8.GetBytes("<html><body><h1>This server is running on <a href=\"http://github.com/markrendle/CRack\">CRack</a>.</h1></body></html>");
                var headers = new Dictionary<string, string>
                              {
                                  { "Content-Type", "text/html" },
                                  { "Content-Length", bytes.Length.ToString() }
                              };
                responseHandler(200, "OK", headers, bytes);
            }
        }
    }
}
