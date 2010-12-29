using System;
using System.Collections.Generic;
using System.Threading;

namespace Fix
{
    public class Fixer
    {
        private readonly Action<Action<string, string, IEnumerable<KeyValuePair<string, string>>, Func<byte[]>, Action<int, string, IEnumerable<KeyValuePair<string, string>>, Func<byte[]>>, Delegate>> _starter;
        private readonly Action _stopper;
        Action<string, string, IEnumerable<KeyValuePair<string, string>>, Func<byte[]>, Action<int, string, IEnumerable<KeyValuePair<string, string>>, Func<byte[]>>> _handler;
        private Action<string, string, IEnumerable<KeyValuePair<string, string>>, Func<byte[]>, Action<int, string, IEnumerable<KeyValuePair<string, string>>, Func<byte[]>>, Delegate> _pipe;

        public Fixer(Action<Action<string, string, IEnumerable<KeyValuePair<string, string>>, Func<byte[]>, Action<int, string, IEnumerable<KeyValuePair<string, string>>, Func<byte[]>>, Delegate>> starter, Action stopper)
        {
            if (starter == null) throw new ArgumentNullException("starter");
            if (stopper == null) throw new ArgumentNullException("stopper");

            _starter = starter;
            _stopper = stopper;
            _handler = EmptyHandler;
            _pipe = (uri, method, headers, body, responseHandler, next) => DefaultInfix(uri, method, headers, body, responseHandler, _handler);
        }

        public void Start()
        {
            _starter(_pipe);
        }

        public void Stop()
        {
            _stopper();
        }

        public void AddHandler(Action<string, string, IEnumerable<KeyValuePair<string, string>>, Func<byte[]>, Action<int, string, IEnumerable<KeyValuePair<string, string>>, Func<byte[]>>> handlerToAdd)
        {
            Action<string, string, IEnumerable<KeyValuePair<string, string>>, Func<byte[]>, Action<int, string, IEnumerable<KeyValuePair<string, string>>, Func<byte[]>>> currentHandler;
            Action<string, string, IEnumerable<KeyValuePair<string, string>>, Func<byte[]>, Action<int, string, IEnumerable<KeyValuePair<string, string>>, Func<byte[]>>> newHandler;
            do
            {
                currentHandler = _handler;
                newHandler = GetNewHandler(currentHandler, handlerToAdd);
            } while (!ReferenceEquals(currentHandler, Interlocked.CompareExchange(ref _handler, newHandler, currentHandler)));
        }

        public void AddInfix(Action<string, string, IEnumerable<KeyValuePair<string, string>>, Func<byte[]>, Action<int, string, IEnumerable<KeyValuePair<string, string>>, Func<byte[]>>, Delegate> pipeToAdd)
        {
            Action<string, string, IEnumerable<KeyValuePair<string, string>>, Func<byte[]>, Action<int, string, IEnumerable<KeyValuePair<string, string>>, Func<byte[]>>, Delegate> currentInfix;
            Action<string, string, IEnumerable<KeyValuePair<string, string>>, Func<byte[]>, Action<int, string, IEnumerable<KeyValuePair<string, string>>, Func<byte[]>>, Delegate> newInfix;
            do
            {
                currentInfix = _pipe;
                newInfix =
                    (uri, method, headers, body, responseHandler, next) => pipeToAdd(uri, method, headers, body, responseHandler, currentInfix);

            } while (!ReferenceEquals(currentInfix, Interlocked.CompareExchange(ref _pipe, newInfix, currentInfix)));
            
        }

        private static Action<string, string, IEnumerable<KeyValuePair<string, string>>, Func<byte[]>, Action<int, string, IEnumerable<KeyValuePair<string, string>>, Func<byte[]>>> GetNewHandler(Action<string, string, IEnumerable<KeyValuePair<string, string>>, Func<byte[]>, Action<int, string, IEnumerable<KeyValuePair<string, string>>, Func<byte[]>>> currentHandler, Action<string, string, IEnumerable<KeyValuePair<string, string>>, Func<byte[]>, Action<int, string, IEnumerable<KeyValuePair<string, string>>, Func<byte[]>>> handlerToAdd)
        {
            return (Action<string, string, IEnumerable<KeyValuePair<string, string>>, Func<byte[]>, Action<int, string, IEnumerable<KeyValuePair<string, string>>, Func<byte[]>>>) Delegate.Combine(currentHandler,
                                                     new Action<string, string, IEnumerable<KeyValuePair<string, string>>, Func<byte[]>, Action<int, string, IEnumerable<KeyValuePair<string, string>>, Func<byte[]>>>(
                                                         (url, method, headers, body, responseHandler) =>
                                                         handlerToAdd.InvokeAndForget(url, method, headers, body, responseHandler)));
        }

        private static void EmptyHandler(string uri, string method, IEnumerable<KeyValuePair<string, string>> headers, Func<byte[]> body, Action<int, string, IEnumerable<KeyValuePair<string, string>>, Func<byte[]>> responsehandler)
        {

        }

        private static void DefaultInfix(string uri, string method, IEnumerable<KeyValuePair<string, string>> headers, Func<byte[]> body, Action<int, string, IEnumerable<KeyValuePair<string, string>>, Func<byte[]>> responseHandler, Action<string, string, IEnumerable<KeyValuePair<string, string>>, Func<byte[]>, Action<int, string, IEnumerable<KeyValuePair<string, string>>, Func<byte[]>>> requestHandler)
        {
            requestHandler(uri, method, headers, body, responseHandler);
        }
    }
}
