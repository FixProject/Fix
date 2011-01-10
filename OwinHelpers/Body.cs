using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ResponseHandler = System.Action<int, System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, string>>, System.Action<System.Action<System.ArraySegment<byte>>, System.Action<System.IO.FileInfo>, System.Action, System.Action<System.Exception>>>;

namespace OwinHelpers
{
    public static class Body
    {
        public static Action<Action<ArraySegment<byte>>, Action<FileInfo>, Action, Action<Exception>> FromException(Exception ex)
        {
            return new ExceptionBody(ex).ToAction();
        }

        public static Action<Action<ArraySegment<byte>>, Action<FileInfo>, Action, Action<Exception>> FromString(string text)
        {
            return new StringBody(text).ToAction();
        }

        public static Action<Action<ArraySegment<byte>>, Action<FileInfo>, Action, Action<Exception>> FromFile(FileInfo fileInfo)
        {
            return new FileBody(fileInfo).ToAction();
        }

        public static void WriteString(this ResponseHandler responseHandler, Func<string> textGenerator, string contentType)
        {
            try
            {
                var text = textGenerator();
                var body = FromString(text);

                var headers = BuildBasicHeaders(text.Length, contentType);
                responseHandler(200, headers, body);
            }
            catch (Exception ex)
            {
                responseHandler(500, null, FromException(ex));
            }
        }

        private static IEnumerable<KeyValuePair<string, string>> BuildBasicHeaders(long contentLength, string contentType)
        {
            yield return new KeyValuePair<string, string>("Content-Type", contentType);
            yield return new KeyValuePair<string, string>("Content-Length", contentLength.ToString());
        }

        public static void WriteHtml(this ResponseHandler responseHandler, Func<string> htmlGenerator)
        {
            WriteString(responseHandler, htmlGenerator, "text/html");
        }

        public static void WriteFile(this ResponseHandler responseHandler, FileInfo fileInfo, string contentType)
        {
            if (fileInfo == null) throw new ArgumentNullException("fileInfo");
            var headers = BuildBasicHeaders(fileInfo.Length, contentType);
            responseHandler(200, headers, FromFile(fileInfo));
        }
    }
}
