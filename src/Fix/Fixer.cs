using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fix
{
    using System.Threading;
    using Env = IDictionary<string, object>;
    using ComponentFunc = Func<IDictionary<string, object>, Func<IDictionary<string, object>,Task>, Task>;
    using BuilderFunc = Func<Func<IDictionary<string, object>, Task>, Func<IDictionary<string,object>, Task>>;
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class Fixer
    {
        private readonly Stack<BuilderFunc> _funcs = new Stack<BuilderFunc>();
        private int _useCount = 0;

        public Fixer Use(BuilderFunc component)
        {
            _funcs.Push(component);
            return this;
        }

        public AppFunc Build()
        {
            if (Interlocked.Increment(ref _useCount) > 1)
            {
                throw new InvalidOperationException("Fixer instances may only be used once.");
            }

            var lastFunc = _funcs.Pop();
            AppFunc f = lastFunc(Completed);

            while (_funcs.Count > 0)
            {
                var func = _funcs.Pop();
                f = func(f);
            }

            return f;
        }

        private static Task Completed(IDictionary<string, object> _)
        {
            var tcs = new TaskCompletionSource<int>();
            tcs.SetResult(0);
            return tcs.Task;
        }
    }
}
