using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace OwinHelpers
{
    internal sealed class FileBody : IObservable<ArraySegment<byte>>
    {
        private readonly FileInfo _fileInfo;

        public FileBody(FileInfo fileInfo)
        {
            _fileInfo = fileInfo;
        }

        public IDisposable Subscribe(IObserver<ArraySegment<byte>> observer)
        {
            Action action = () => WriteBody(observer);
            action.BeginInvoke(action.EndInvoke, null);

            return new NullDisposable();
        }

        private void WriteBody(IObserver<ArraySegment<byte>> observer)
        {
            try
            {
                observer.OnNext(_fileInfo.ToOwinBody());
                observer.OnCompleted();
            }
            catch (Exception ex)
            {
                observer.OnError(ex);
            }
        }
    }
}
