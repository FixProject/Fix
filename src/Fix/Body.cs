namespace Fix
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    public static class Body
    {
        public static Func<Stream, CancellationToken, Task> FromException(Exception ex)
        {
            return new ExceptionBody(ex).ToAction();
        }
    }
}