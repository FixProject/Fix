using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace OwinHelpers
{
    internal class FileBody
    {
        private readonly FileInfo _fileInfo;

        public FileBody(FileInfo fileInfo)
        {
            _fileInfo = fileInfo;
        }

        internal IDisposable Subscribe(ActionObserver<ArraySegment<byte>> observer)
        {
            Action action = () => WriteBody(observer);
            action.BeginInvoke(action.EndInvoke, null);

            return new NullDisposable();
        }

        public Action<Action<ArraySegment<byte>>, Action<FileInfo>, Action, Action<Exception>> ToAction()
        {
            return
                (onNext, onFile, onCompleted, onError) =>
                    this.Subscribe(new ActionObserver<ArraySegment<byte>>(onNext, onFile, onCompleted, onError));

        }

        private void WriteBody(ActionObserver<ArraySegment<byte>> observer)
        {
            try
            {
                observer.OnFile(_fileInfo);
                observer.OnCompleted();
            }
            catch (Exception ex)
            {
                observer.OnError(ex);
            }
        }
    }
}
