using System;
using System.IO;

namespace OwinHelpers
{
    internal class ActionObserver<T> : IObserver<T>
    {
        private readonly Action<FileInfo> _onFile;
        private readonly Action<T> _onNext;
        private readonly Action _onCompleted;
        private readonly Action<Exception> _onError;

        public ActionObserver(Action<T> onNext, Action<FileInfo> onFile, Action onCompleted, Action<Exception> onError)
        {
            _onFile = onFile;
            _onNext = onNext ?? new Action<T>(x => { });
            _onError = onError ?? new Action<Exception>(x => { });
            _onCompleted = onCompleted ?? new Action(() => { });
        }

        public void OnNext(T value)
        {
            _onNext(value);
        }

        public void OnFile(FileInfo fileInfo)
        {
            _onFile(fileInfo);
        }

        public void OnError(Exception error)
        {
            _onError(error);
        }

        public void OnCompleted()
        {
            _onCompleted();
        }
    }
}