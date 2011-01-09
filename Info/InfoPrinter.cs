using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Text;
using ResponseHandler = System.Action<int, System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, string>>, System.Func<byte[]>>;

namespace Info
{
    public class InfoPrinter
    {
        [Export("Owin.Application")]
        public void PrintInfo(IDictionary<string, object> env, Func<byte[]> body,
            ResponseHandler responseHandler, Action<Exception> exceptionHandler, Delegate next)
        {
            try
            {
                if (env["SCRIPT_NAME"].ToString().Equals("/info", StringComparison.CurrentCultureIgnoreCase))
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
            catch (Exception ex)
            {
                exceptionHandler(ex);
            }
        }
    }
}
