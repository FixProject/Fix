using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading;
using App = System.Action<System.Collections.Generic.IDictionary<string,object>, System.Func<byte[]>, System.Action<int, System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, string>>, System.Func<byte[]>>, System.Action<System.Exception>, System.Delegate>;
using ResponseHandler = System.Action<int, System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, string>>, System.Func<byte[]>>;
using Starter = System.Action<System.Action<System.Collections.Generic.IDictionary<string, object>, System.Func<byte[]>, System.Action<int, System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, string>>, System.Func<byte[]>>, System.Action<System.Exception>, System.Delegate>>;

namespace Fix
{
    public class Fixer
    {
        private readonly Starter _starter;
        private readonly Action _stopper;
        private int _startCallCount;
        private int _handlerCount;
        App _handler;
        private App _infix;

        [ImportMany("Owin.Application")]
        private IEnumerable<App> _handlers;

        [ImportMany("Owin.Middleware")]
        private IEnumerable<App> _infixes;

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

        public void AddHandler(App handlerToAdd)
        {
            App currentHandler;
            App newHandler;
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

        public void AddInfix(App infixToAdd)
        {
            App currentInfix;
            App newInfix;
            do
            {
                currentInfix = _infix;
                newInfix =
                    (env, body, responseHandler, exceptionHandler, next) => infixToAdd(env, body, responseHandler, exceptionHandler, currentInfix);

            } while (!ReferenceEquals(currentInfix, Interlocked.CompareExchange(ref _infix, newInfix, currentInfix)));
            
        }

        private static App GetNewHandler(App currentHandler, App handlerToAdd)
        {
            return (App)Delegate.Combine(currentHandler,
                                                     new App(
                                                         (env, body, responseHandler, exceptionHandler, next) =>
                                                         handlerToAdd.InvokeAndForget(env, body, responseHandler, exceptionHandler, next)));
        }

        private static void EmptyHandler(IDictionary<string, object> env, Func<byte[]> body, ResponseHandler responseHandler, Action<Exception> exceptionHandler, Delegate next)
        {
            responseHandler(500, null, null);
        }

        private static void DefaultInfix(IDictionary<string, object> env, Func<byte[]> body, ResponseHandler responseHandler, Action<Exception> exceptionHandler, Func<App> requestHandler)
        {
            requestHandler()(env, body, responseHandler, exceptionHandler, null);
        }
    }
}
