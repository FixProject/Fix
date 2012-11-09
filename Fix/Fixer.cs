using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using OwinEnvironment = System.Collections.Generic.IDictionary<string, object>;
using OwinHeaders = System.Collections.Generic.IDictionary<string, string[]>;
using Starter = System.Action<System.Func< // Call
        System.Collections.Generic.IDictionary<string, object>, // Environment
        System.Threading.Tasks.Task>>;
using AppFunc = System.Func< // Call
        System.Collections.Generic.IDictionary<string, object>, // Environment
        System.Threading.Tasks.Task>;

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
        private AppFunc _app;

        [ImportMany("Owin.Application")]
        private IEnumerable<AppFunc> _handlers;

        [ImportMany("Owin.Middleware")]
        private IEnumerable<Func<AppFunc, AppFunc>> _infixes;

        public Fixer()
        {
            _app = EmptyHandler;
        }

        public Fixer(Starter starter, Action stopper) : this()
        {
            if (starter == null) throw new ArgumentNullException("starter");
            if (stopper == null) throw new ArgumentNullException("stopper");

            _starter = starter;
            _stopper = stopper;
        }

        public AppFunc BuildApp()
        {
            var handlers = _handlers.ToArray();
            if (handlers.Length == 0) throw new InvalidOperationException("No application found.");
            if (handlers.Length > 1)
            {
                _app = new MultiApp(handlers).Handle;
            }
            else
            {
                _app = handlers[0];
            }
            AddInfixes();
            return _app;
        }

        public void Start()
        {
            if (Interlocked.Increment(ref _startCallCount) > 1) throw new InvalidOperationException("Fixer has been used.");
            BuildApp();
            _starter(_app);
        }

        public void Stop()
        {
            _stopper();
        }

        private void AddInfixes()
        {
            if (_infixes == null) return;
            foreach (var infix in _infixes)
            {
                _app = infix(_app);
            }
        }

        private static Task EmptyHandler(OwinEnvironment env)
        {
            env[OwinKeys.ResponseStatusCode] = 404;
            return TaskHelper.Completed();
        }
    }
}
