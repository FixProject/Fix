using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRack
{
    class RequestProcessor
    {
        public void ProcessRequest(string uri, string method, IEnumerable<KeyValuePair<string, string>> requestHeaders, byte[] body,
            Action<int, string, IEnumerable<KeyValuePair<string, string>>, byte[]> respond)
        {
            var builder = new StringBuilder("<html><body>");
            builder.AppendFormat("<p>{0}</p>", uri);
            builder.AppendFormat("<p>{0}</p>", method);
            foreach (var header in requestHeaders)
            {
                builder.AppendFormat("<p><strong>{0}</strong>: {1}</p>", header.Key, header.Value);
            }
            builder.Append("</body></html>");
            var bytes = Encoding.UTF8.GetBytes(builder.ToString());
            var headers = new Dictionary<string, string>
                              {
                                  { "Content-Type", "text/html" },
                                  { "Content-Length", bytes.Length.ToString() }
                              };
            respond(200, "OK", headers, bytes);
        }
    }
}
