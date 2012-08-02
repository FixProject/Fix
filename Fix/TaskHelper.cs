using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fix
{
    using System.Threading.Tasks;

    public static class TaskHelper
    {
        public static Task Completed()
        {
            var tcs = new TaskCompletionSource<object>();
            tcs.SetResult(null);
            return tcs.Task;
        }
        
        public static Task<T> Completed<T>(T result)
        {
            var tcs = new TaskCompletionSource<T>();
            tcs.SetResult(result);
            return tcs.Task;
        }
    }
}
