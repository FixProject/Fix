using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using OwinHelpers;
using App = System.Action<System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string,object>>, System.Func<byte[]>, System.Action<int, System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, string>>, System.Action<System.Action<System.ArraySegment<byte>>, System.Action, System.Action<System.Exception>>>, System.Delegate>;
using ResponseHandler = System.Action<int, System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, string>>, System.Action<System.Action<System.ArraySegment<byte>>, System.Action, System.Action<System.Exception>>>;
using Starter = System.Action<System.Action<System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string,object>>, System.Func<byte[]>, System.Action<int, System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, string>>, System.Action<System.Action<System.ArraySegment<byte>>, System.Action, System.Action<System.Exception>>>, System.Delegate>>;

namespace Fix
{
    public class Fixer
    {
        private readonly Starter _starter;
        private readonly Action _stopper;
        private int _startCallCount;
        private int _handlerCount;
        private App _app;

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
            _app = (env, body, responseHandler, next) => DefaultInfix(env, body, responseHandler, () => EmptyHandler);
        }

        public void Start()
        {
            if (Interlocked.Increment(ref _startCallCount) > 1) throw new InvalidOperationException("Fixer has been used.");
            AddHandlers();
            AddInfixes();
            if (_handlerCount == 0) throw new InvalidOperationException("No handlers attached.");
            _starter(_app);
        }

        public void Stop()
        {
            _stopper();
        }

        private void AddHandlers()
        {
            if (_handlers == null) return;
            foreach (var handler in _handlers)
            {
                AddApp(handler);
                _handlerCount++;
            }
        }

        public void AddHandler(App handlerToAdd)
        {
            _handlers = (_handlers ?? Enumerable.Empty<App>()).Append(handlerToAdd);
        }

        public void AddInfix(App infixToAdd)
        {
            _infixes = (_infixes ?? Enumerable.Empty<App>()).Append(infixToAdd);
        }

        private void AddInfixes()
        {
            if (_infixes == null) return;
            foreach (var infix in _infixes)
            {
                AddApp(infix);
            }
        }

        private void AddApp(App appToAdd)
        {
            App currentApp;
            App newApp;
            do
            {
                currentApp = _app;
                newApp =
                    (env, body, responseHandler, next) => appToAdd(env, body, responseHandler, currentApp);

            } while (!ReferenceEquals(currentApp, Interlocked.CompareExchange(ref _app, newApp, currentApp)));
            
        }

        private static void EmptyHandler(IEnumerable<KeyValuePair<string,object>> env, Func<byte[]> body, ResponseHandler responseHandler, Delegate next)
        {
            responseHandler(500, null, null);
        }

        private static void DefaultInfix(IEnumerable<KeyValuePair<string,object>> env, Func<byte[]> body, ResponseHandler responseHandler, Func<App> requestHandler)
        {
            try
            {
                requestHandler()(env, body, responseHandler, null);
            }
            catch (Exception ex)
            {
                responseHandler(0, null, new ExceptionBody(ex).ToAction());
            }
        }
    }
}
