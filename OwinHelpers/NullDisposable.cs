using System;

namespace OwinHelpers
{
    internal sealed class NullDisposable : IDisposable
    {
        public void Dispose()
        {

        }
    }
}