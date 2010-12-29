using System;
using System.Collections.Generic;
using System.Threading;
using Infix = System.Action<System.Collections.Generic.IDictionary<string,string>, System.Func<byte[]>, System.Action<int, System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, string>>, System.Func<byte[]>>, System.Delegate>;
using RequestHandler = System.Action<System.Collections.Generic.IDictionary<string,string>, System.Func<byte[]>, System.Action<int, System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, string>>, System.Func<byte[]>>>;
using ResponseHandler = System.Action<int, System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, string>>, System.Func<byte[]>>;
using Starter = System.Action<System.Action<System.Collections.Generic.IDictionary<string, string>, System.Func<byte[]>, System.Action<int, System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, string>>, System.Func<byte[]>>, System.Delegate>>;

namespace Fix
{
    public class Fixer
    {
        private readonly Starter _starter;
        private readonly Action _stopper;
        RequestHandler _handler;
        private Infix _infix;

        public Fixer(Starter starter, Action stopper)
        {
            if (starter == null) throw new ArgumentNullException("starter");
            if (stopper == null) throw new ArgumentNullException("stopper");

            _starter = starter;
            _stopper = stopper;
            _handler = EmptyHandler;
            _infix = (env, body, responseHandler, next) => DefaultInfix(env, body, responseHandler, _handler);
        }

        public void Start()
        {
            _starter(_infix);
        }

        public void Stop()
        {
            _stopper();
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
        }

        public void AddInfix(Infix infixToAdd)
        {
            Infix currentInfix;
            Infix newInfix;
            do
            {
                currentInfix = _infix;
                newInfix =
                    (env, body, responseHandler, next) => infixToAdd(env, body, responseHandler, currentInfix);

            } while (!ReferenceEquals(currentInfix, Interlocked.CompareExchange(ref _infix, newInfix, currentInfix)));
            
        }

        private static RequestHandler GetNewHandler(RequestHandler currentHandler, RequestHandler handlerToAdd)
        {
            return (RequestHandler) Delegate.Combine(currentHandler,
                                                     new RequestHandler(
                                                         (env, body, responseHandler) =>
                                                         handlerToAdd.InvokeAndForget(env, body, responseHandler)));
        }

        private static void EmptyHandler(IDictionary<string, string> env, Func<byte[]> body, ResponseHandler responsehandler)
        {

        }

        private static void DefaultInfix(IDictionary<string, string> env, Func<byte[]> body, ResponseHandler responseHandler, RequestHandler requestHandler)
        {
            requestHandler(env, body, responseHandler);
        }
    }
}
