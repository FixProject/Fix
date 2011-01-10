using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace OwinHelpers
{
    public static class ObservableExtensions
    {
        public static Action<Action<T>, Action<FileInfo>, Action, Action<Exception>> ToAction<T>(this IObservable<T> source)
        {
            return
                (onNext, onFile, onCompleted, onError) => source.Subscribe(new ActionObserver<T>(onNext, onFile, onCompleted, onError));
        }

    }
}
