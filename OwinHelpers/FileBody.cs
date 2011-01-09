using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace OwinHelpers
{
    public class FileBody : IObservable<byte[]>
    {
        private readonly FileInfo _fileInfo;

        public FileBody(FileInfo fileInfo)
        {
            _fileInfo = fileInfo;
        }

        public IDisposable Subscribe(IObserver<byte[]> observer)
        {
            Action action = () => WriteBody(observer);
            action.BeginInvoke(action.EndInvoke, null);

            return new NullDisposable();
        }

        private void WriteBody(IObserver<byte[]> observer)
        {
            try
            {
                using (var stream = _fileInfo.OpenRead())
                {
                    var buffer = new byte[stream.Length];
                    stream.Read(buffer, 0, buffer.Length);
                    observer.OnNext(buffer);
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
