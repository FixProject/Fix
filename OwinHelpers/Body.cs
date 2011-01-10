using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OwinHelpers
{
    public static class Body
    {
        public static Action<Action<ArraySegment<byte>>, Action, Action<Exception>> FromException(Exception ex)
        {
            return (onNext, onCompleted, onException) => onException(ex);
        }
    }
}
