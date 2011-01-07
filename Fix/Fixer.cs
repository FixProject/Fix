using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading;
using Infix = System.Action<System.Collections.Generic.IDictionary<string,string>, System.Func<byte[]>, System.Action<int, System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, string>>, System.Func<byte[]>>, System.Action<System.Exception>, System.Delegate>;
using RequestHandler = System.Action<System.Collections.Generic.IDictionary<string,string>, System.Func<byte[]>, System.Action<int, System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, string>>, System.Func<byte[]>>, System.Action<System.Exception>>;
using ResponseHandler = System.Action<int, System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, string>>, System.Func<byte[]>>;
using Starter = System.Action<System.Action<System.Collections.Generic.IDictionary<string, string>, System.Func<byte[]>, System.Action<int, System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, string>>, System.Func<byte[]>>, System.Action<System.Exception>, System.Delegate>>;

namespace Fix
{
    public class Fixer
    {
        private readonly Starter _starter;
        private readonly Action _stopper;
        private int _startCallCount;
        private int _handlerCount;
        RequestHandler _handler;
        private Infix _infix;

        [ImportMany]
        private IEnumerable<RequestHandler> _handlers;

        [ImportMany]
        private IEnumerable<Infix> _infixes;

        public Fixer(Starter starter, Action stopper)
        {
            if (starter == null) throw new ArgumentNullException("starter");
            if (stopper == null) throw new ArgumentNullException("stopper");

            _starter = starter;
            _stopper = stopper;
            _handler = EmptyHandler;
            _infix = (env, body, responseHandler, exceptionHandler, next) => DefaultInfix(env, body, responseHandler, exceptionHandler, () => _handler);
        }

        public void Start()
        {
            if (Interlocked.Increment(ref _startCallCount) > 1) throw new InvalidOperationException("Fixer has been used.");
            AddImportedInfixes();
            AddImportedHandlers();
            if (_handlerCount == 0) throw new InvalidOperationException("No handlers attached.");
            _starter(_infix);
        }

        public void Stop()
        {
            _stopper();
        }

        private void AddImportedHandlers()
        {
            if (_handlers == null) return;
            foreach (var handler in _handlers)
            {
                AddHandler(handler);
            }
        }

        public void AddHandler(RequestHandler handlerToAdd)
        {
            RequestHandler currentHandler;
            RequestHandler newHandler;
            do
            {
                currentHandler = _handler;
                newHandler = GetNewHandler(currentHandler, handlerToAdd);
            } while (!ReferenceEquals(currentHandler, Interlocked.CompareExchange(ref _handler, newHandler, currentHandler)));
            _handlerCount++;
        }

        private void AddImportedInfixes()
        {
            if (_infixes == null) return;
            foreach (var infix in _infixes)
            {
                AddInfix(infix);
            }
        }

        public void AddInfix(Infix infixToAdd)
        {
            Infix currentInfix;
            Infix newInfix;
            do
            {
                currentInfix = _infix;
                newInfix =
                    (env, body, responseHandler, exceptionHandler, next) => infixToAdd(env, body, responseHandler, exceptionHandler, currentInfix);

            } while (!ReferenceEquals(currentInfix, Interlocked.CompareExchange(ref _infix, newInfix, currentInfix)));
            
        }

        private static RequestHandler GetNewHandler(RequestHandler currentHandler, RequestHandler handlerToAdd)
        {
            return (RequestHandler) Delegate.Combine(currentHandler,
                                                     new RequestHandler(
                                                         (env, body, responseHandler, exceptionHandler) =>
                                                         handlerToAdd.InvokeAndForget(env, body, responseHandler, exceptionHandler)));
        }

        private static void EmptyHandler(IDictionary<string, string> env, Func<byte[]> body, ResponseHandler responsehandler, Action<Exception> exceptionHandler)
        {

        }

        private static void DefaultInfix(IDictionary<string, string> env, Func<byte[]> body, ResponseHandler responseHandler, Action<Exception> exceptionHandler, Func<RequestHandler> requestHandler)
        {
            requestHandler()(env, body, responseHandler, exceptionHandler);
        }
    }
}
