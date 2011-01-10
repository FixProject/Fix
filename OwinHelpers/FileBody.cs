using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace OwinHelpers
{
    public class FileBody : IObservable<ArraySegment<byte>>
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
                using (var stream = _fileInfo.OpenRead())
                {
                    var buffer = new byte[stream.Length];
                    stream.Read(buffer, 0, buffer.Length);
                    observer.OnNext(new ArraySegment<byte>(buffer));
                }
                observer.OnCompleted();
            }
            catch (Exception ex)
            {
                observer.OnError(ex);
            }
        }
    }
}
