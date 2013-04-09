namespace Fix
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;

    class MultiApp
    {
        private readonly Func<IDictionary<string, object>, Task>[] _appFuncs;

        public MultiApp(Func<IDictionary<string, object>, Task>[] appFuncs)
        {
            _appFuncs = appFuncs;
        }

        public Task Handle(IDictionary<string,object> env)
        {
            var task = _appFuncs[0](env);

            for (int i = 1; i < _appFuncs.Length; i++)
            {
                int index = i;
                task = task.ContinueWith(t =>
                                             {
                                                 if (t.IsFaulted) return t;
                                                 if (t.IsCanceled || (int)env[OwinKeys.ResponseStatusCode] == 404)
                                                 {
                                                     return _appFuncs[index](env);
                                                 }
                                                 return t;
                                             }, TaskContinuationOptions.None).Unwrap();
            }

            return task;
        }
    }
}