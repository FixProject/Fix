using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using OwinEnvironment = System.Collections.Generic.IDictionary<string, object>;
using OwinHeaders = System.Collections.Generic.IDictionary<string, string[]>;
using ResponseHandler = System.Func<int, System.Collections.Generic.IDictionary<string, string[]>, System.Func<System.IO.Stream, System.Threading.CancellationToken, System.Threading.Tasks.Task>, System.Threading.Tasks.Task>;
using App = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Collections.Generic.IDictionary<string, string[]>, System.IO.Stream, System.Threading.CancellationToken, System.Func<int, System.Collections.Generic.IDictionary<string, string[]>, System.Func<System.IO.Stream, System.Threading.CancellationToken, System.Threading.Tasks.Task>, System.Threading.Tasks.Task>, System.Delegate, System.Threading.Tasks.Task>;
using Starter = System.Action<System.Func<System.Collections.Generic.IDictionary<string, object>, System.Collections.Generic.IDictionary<string, string[]>, System.IO.Stream, System.Threading.CancellationToken, System.Func<int, System.Collections.Generic.IDictionary<string, string[]>, System.Func<System.IO.Stream, System.Threading.CancellationToken, System.Threading.Tasks.Task>, System.Threading.Tasks.Task>, System.Delegate, System.Threading.Tasks.Task>>;

namespace Fix
{
    using System.IO;
    using System.Threading.Tasks;

    public class Fixer
    {
        private readonly Starter _starter;
        private readonly Action _stopper;
        private int _startCallCount;
        private int _buildCallCount;
        private int _handlerCount;
        private App _app;

        [ImportMany("Owin.Application")]
        private IEnumerable<App> _handlers;

        [ImportMany("Owin.Middleware")]
        private IEnumerable<App> _infixes;

        public Fixer()
        {
            _app = (env, headers, body, cancel, responseHandler, next) => DefaultInfix(env, headers, body, cancel, responseHandler, () => EmptyHandler);
        }

        public Fixer(Starter starter, Action stopper) : this()
        {
            if (starter == null) throw new ArgumentNullException("starter");
            if (stopper == null) throw new ArgumentNullException("stopper");

            _starter = starter;
            _stopper = stopper;
        }

        public App BuildApp()
        {
            AddHandlers();
            AddInfixes();
            if (_handlerCount == 0) throw new InvalidOperationException("No handlers attached.");
            return _app;
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
                var closureApp = currentApp = _app;
                newApp =
                    (env, headers, body, cancel, responseHandler, next) => appToAdd(env, headers, body, cancel, responseHandler, closureApp);

            } while (!ReferenceEquals(currentApp, Interlocked.CompareExchange(ref _app, newApp, currentApp)));
            
        }

        private static Task EmptyHandler(OwinEnvironment env, IDictionary<string,string[]> headers, Stream inputStream, CancellationToken cancellationToken, ResponseHandler responseHandler, Delegate next)
        {
            return responseHandler(500, null, null);
        }

        private static Task DefaultInfix(OwinEnvironment env, IDictionary<string,string[]> headers, Stream inputStream, CancellationToken cancellationToken, ResponseHandler responseHandler, Func<App> requestHandler)
        {
            try
            {
                return requestHandler()(env, headers, inputStream, cancellationToken, responseHandler, null);
            }
            catch (Exception ex)
            {
                return responseHandler(0, null, Body.FromException(ex));
            }
        }
    }
}
