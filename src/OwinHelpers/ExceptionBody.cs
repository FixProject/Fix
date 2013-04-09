namespace OwinHelpers
{
    using System;
    using System.Text;
    using System.Threading.Tasks;
    using BodyDelegate = System.Func<System.IO.Stream,System.Threading.CancellationToken,System.Threading.Tasks.Task>;

    internal class ExceptionBody
    {
        private readonly Exception _error;

        public ExceptionBody(Exception error)
        {
            _error = error;
        }

        public BodyDelegate ToAction()
        {
            var buffer = Encoding.Default.GetBytes(_error.ToString());
            return (stream, token) => Task.Factory.FromAsync(stream.BeginWrite, stream.EndWrite, buffer, 0, buffer.Length, null);
        }
    }
}
