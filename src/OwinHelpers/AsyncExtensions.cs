namespace OwinHelpers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Result = System.Tuple< //Result
        System.Collections.Generic.IDictionary<string, object>, // Properties
        int, // Status
        System.Collections.Generic.IDictionary<string, string[]>, // Headers
        System.Func< // Body
            System.IO.Stream, // Output
            System.Threading.Tasks.Task>>; // Done

    public static class AsyncExtensions
    {
        public static Task WriteAsync(this Stream stream, byte[] bytes, int offset, int count, CancellationToken cancellationToken = default(CancellationToken))
        {
            var factory = cancellationToken.Equals(default(CancellationToken))
                              ? Task.Factory
                              : new TaskFactory(cancellationToken);
            return factory.FromAsync(stream.BeginWrite, stream.EndWrite, bytes, offset, count, null);
        }

        public static Task WriteAsync(this Stream stream, string text)
        {
            var bytes = Encoding.UTF8.GetBytes(text);
            return stream.WriteAsync(bytes, 0, bytes.Length);
        }
    }

    public static class TaskHelper
    {
        public static Task Completed()
        {
            var tcs = new TaskCompletionSource<object>();
            tcs.SetResult(null);
            return tcs.Task;
        }

        public static Task<Result> Completed(IDictionary<string,object> properties, int status, IDictionary<string,string[]> headers, Func<Stream,Task> body)
        {
            var tcs = new TaskCompletionSource<Result>();
            tcs.SetResult(new Result(properties, status, headers, body));
            return tcs.Task;
        }

        public static Task<Result> NotFound()
        {
            return Completed(null, 404, null, null);
        }

        public static Task<Result> Error(Exception ex)
        {
            return Completed(null, 500, null, stream => stream.WriteAsync(ex.ToString()));
        }
    }
}