using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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
    }
}
