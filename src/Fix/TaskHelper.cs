namespace Fix
{
    using System.Threading.Tasks;

    public static class TaskHelper
    {
        public static Task Completed()
        {
            var tcs = new TaskCompletionSource<int>();
            tcs.SetResult(0);
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
