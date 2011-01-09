using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OwinHelpers
{
    public static class BodyExtensions
    {
        public static IObservable<T> ToObservable<T>(this Action<Action<T>, Action, Action<Exception>> source)
        {
            return new ActionObservable<T>(source);
        }

        private class ActionObservable<T> : IObservable<T>
        {
            private readonly Action<Action<T>, Action, Action<Exception>> _action;

            public ActionObservable(Action<Action<T>, Action, Action<Exception>> action)
            {
                _action = action;
            }

            public IDisposable Subscribe(IObserver<T> observer)
            {
                _action.BeginInvoke(observer.OnNext, observer.OnCompleted, observer.OnError, _action.EndInvoke, null);
                return new NullDisposable();
            }
        }
    }
}
