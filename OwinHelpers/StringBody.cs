using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OwinHelpers
{
    using System.Threading.Tasks;
    using BodyDelegate = System.Func<System.IO.Stream,System.Threading.CancellationToken,System.Threading.Tasks.Task>;
    internal class StringBody : IObservable<ArraySegment<byte>>
    {
        private readonly byte[] _bytes;

        public StringBody(string text) : this(text, Encoding.Default)
        {
        }

        public StringBody(string text, Encoding encoding)
        {
            _bytes = encoding.GetBytes(text);
        }

        public BodyDelegate ToAction()
        {
            return (stream, token) => Task.Factory.FromAsync(stream.BeginWrite, stream.EndWrite, _bytes, 0, _bytes.Length, null);
        }

        public int Length
        {
            get { return _bytes.Length; }
        }

        public IDisposable Subscribe(IObserver<ArraySegment<byte>> observer)
        {
            Action action = () =>
                                {
                                    observer.OnNext(new ArraySegment<byte>(_bytes));
                                    observer.OnCompleted();
                                };
            action.BeginInvoke(action.EndInvoke, null);
            return new NullDisposable();
        }

    }
}
