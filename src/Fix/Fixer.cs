using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fix
{
    using System.Threading;
    using Env = IDictionary<string, object>;
    using ComponentFunc = Func<IDictionary<string, object>, Func<Task>, Task>;
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class Fixer
    {
        private readonly Stack<ComponentFunc> _funcs = new Stack<ComponentFunc>();
        private int _useCount = 0;

        public Fixer Use(ComponentFunc component)
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
            AppFunc f = env => lastFunc(env, Completed);

            while (_funcs.Count > 0)
            {
                var func = _funcs.Pop();
                var f1 = f;
                f = env => func(env, () => f1(env));
            }

            return f;
        }

        private static Task Completed()
        {
            var tcs = new TaskCompletionSource<int>();
            tcs.SetResult(0);
            return tcs.Task;
        }
    }
}
