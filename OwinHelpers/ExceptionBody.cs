using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OwinHelpers
{
    public class ExceptionBody : IObservable<byte[]>
    {
        private readonly Exception _error;

        public ExceptionBody(Exception error)
        {
            _error = error;
        }

        public IDisposable Subscribe(IObserver<byte[]> observer)
        {
            Action action = () => observer.OnError(_error);
            action.BeginInvoke(action.EndInvoke, null);
            return new NullDisposable();
        }
    }
}
