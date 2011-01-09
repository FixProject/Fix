using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OwinHelpers
{
    public static class ObservableExtensions
    {
        public static Action<Action<T>, Action, Action<Exception>> ToAction<T>(this IObservable<T> source)
        {
            return
                (onNext, onCompleted, onError) => source.Subscribe(new ActionObserver<T>(onNext, onCompleted, onError));
        }

        private class ActionObserver<T> : IObserver<T>
        {
            private readonly Action<T> _onNext;
            private readonly Action _onCompleted;
            private readonly Action<Exception> _onError;

            public ActionObserver(Action<T> onNext, Action onCompleted, Action<Exception> onError)
            {
                _onNext = onNext ?? new Action<T>(x => { });
                _onError = onError ?? new Action<Exception>(x => { });
                _onCompleted = onCompleted ?? new Action(() => { });
            }

            public void OnNext(T value)
            {
                _onNext(value);
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
}
