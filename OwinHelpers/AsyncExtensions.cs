namespace OwinHelpers
{
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    public static class AsyncExtensions
    {
        public static Task WriteAsync(this Stream stream, byte[] bytes, int offset, int count, CancellationToken cancellationToken = default(CancellationToken))
        {
            var factory = cancellationToken.Equals(default(CancellationToken))
                              ? Task.Factory
                              : new TaskFactory(cancellationToken);
            return factory.FromAsync(stream.BeginWrite, stream.EndWrite, bytes, offset, count, null);
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
    }
}