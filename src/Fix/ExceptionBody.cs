namespace Fix
{
    using System;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    internal class ExceptionBody
    {
        private readonly Exception _error;

        public ExceptionBody(Exception error)
        {
            _error = error;
        }

        public Func<Stream, CancellationToken, Task> ToAction()
        {
            var buffer = Encoding.Default.GetBytes(_error.ToString());
            return (stream, token) => Task.Factory.FromAsync(stream.BeginWrite, stream.EndWrite, buffer, 0, buffer.Length, null);
        }
    }
}