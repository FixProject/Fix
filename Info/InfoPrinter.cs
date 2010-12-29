using System;
using System.Collections.Generic;
using System.Text;
using ResponseHandler = System.Action<int, System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, string>>, System.Func<byte[]>>;

namespace Info
{
    public class InfoPrinter
    {
        public void PrintInfo(IDictionary<string, string> env, Func<byte[]> body,
            ResponseHandler responseHandler)
        {
            if (env["SCRIPT_NAME"].Equals("/info", StringComparison.CurrentCultureIgnoreCase))
            {
                var bytes = Encoding.UTF8.GetBytes("<html><body><h1>This server is running on <a href=\"http://github.com/markrendle/Fix\">Fix</a>.</h1></body></html>");
                var headers = new Dictionary<string, string>
                              {
                                  { "Content-Type", "text/html" },
                                  { "Content-Length", bytes.Length.ToString() }
                              };
                responseHandler(200, headers, () => bytes);
            }
        }
    }
}
