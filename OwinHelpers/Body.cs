using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ResponseHandler = System.Func<int, System.Collections.Generic.IDictionary<string, string[]>, System.Func<System.IO.Stream, System.Threading.CancellationToken, System.Threading.Tasks.Task>, System.Threading.Tasks.Task>;
using BodyDelegate = System.Func<System.IO.Stream,System.Threading.CancellationToken,System.Threading.Tasks.Task>;

namespace OwinHelpers
{
    using System.Threading.Tasks;

    public static class Body
    {
        public static BodyDelegate FromException(Exception ex)
        {
            return new ExceptionBody(ex).ToAction();
        }

        public static BodyDelegate FromString(string text)
        {
            return new StringBody(text).ToAction();
        }

        public static BodyDelegate FromFile(FileInfo fileInfo)
        {
            return new FileBody(fileInfo).ToAction();
        }

        public static Task WriteString(this ResponseHandler responseHandler, Func<string> textGenerator, string contentType)
        {
            try
            {
                var text = textGenerator();
                var body = FromString(text);

                var headers = BuildBasicHeaders(text.Length, contentType);
                return responseHandler(200, headers, body);
            }
            catch (Exception ex)
            {
                return responseHandler(500, null, FromException(ex));
            }
        }

        private static IDictionary<string,string[]> BuildBasicHeaders(long contentLength, string contentType)
        {
            return new Dictionary<string, string[]>
                {
                    {"Content-Type", new[] {contentType}},
                    {"Content-Length", new[] {contentLength.ToString()}},
                };
        }

        public static Task WriteHtml(this ResponseHandler responseHandler, Func<string> htmlGenerator)
        {
            return WriteString(responseHandler, htmlGenerator, "text/html");
        }

        public static Task WriteFile(this ResponseHandler responseHandler, FileInfo fileInfo, string contentType)
        {
            if (fileInfo == null) throw new ArgumentNullException("fileInfo");
            var headers = BuildBasicHeaders(fileInfo.Length, contentType);
            return responseHandler(200, headers, FromFile(fileInfo));
        }
    }
}
