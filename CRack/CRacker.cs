using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using Pipe = System.Action<string, string, System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, string>>, byte[], System.Action<int, string, System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, string>>, byte[]>, System.Delegate>;
using RequestHandler = System.Action<string, string, System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, string>>, byte[], System.Action<int, string, System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, string>>, byte[]>>;
using ResponseHandler = System.Action<int, string, System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, string>>, byte[]>;

namespace CRack
{
    public class CRacker
    {
        private readonly Action<Pipe> _starter;
        private readonly Action _stopper;
        RequestHandler _handler;
        private Pipe _pipe;

        public CRacker(Action<Pipe> starter, Action stopper)
        {
            if (starter == null) throw new ArgumentNullException("starter");
            if (stopper == null) throw new ArgumentNullException("stopper");

            _starter = starter;
            _stopper = stopper;
            _handler = EmptyHandler;
            _pipe = (uri, method, headers, body, responseHandler, next) => DefaultPipe(uri, method, headers, body, responseHandler, _handler);
        }

        public void Start()
        {
            _starter(_pipe);
        }

        public void Stop()
        {
            _stopper();
        }

        public void AddHandler(RequestHandler handlerToAdd)
        {
            RequestHandler currentHandler;
            RequestHandler newHandler;
            do
            {
                currentHandler = _handler;
                newHandler = GetNewHandler(currentHandler, handlerToAdd);
            } while (!ReferenceEquals(currentHandler, Interlocked.CompareExchange(ref _handler, newHandler, currentHandler)));
        }

        public void AddPipe(Pipe pipeToAdd)
        {
            Pipe currentPipe;
            Pipe newPipe;
            do
            {
                currentPipe = _pipe;
                newPipe =
                    (uri, method, headers, body, responseHandler, next) => pipeToAdd(uri, method, headers, body, responseHandler, currentPipe);

            } while (!ReferenceEquals(currentPipe, Interlocked.CompareExchange(ref _pipe, newPipe, currentPipe)));
            
        }

        private static RequestHandler GetNewHandler(RequestHandler currentHandler, RequestHandler handlerToAdd)
        {
            return (RequestHandler) Delegate.Combine(currentHandler,
                                                     new RequestHandler(
                                                         (url, method, headers, body, responseHandler) =>
                                                         handlerToAdd.InvokeAndForget(url, method, headers, body, responseHandler)));
        }

        private static void EmptyHandler(string uri, string method, IEnumerable<KeyValuePair<string, string>> headers, byte[] body, ResponseHandler responsehandler)
        {

        }

        private static void DefaultPipe(string uri, string method, IEnumerable<KeyValuePair<string, string>> headers, byte[] body, ResponseHandler responseHandler, RequestHandler requestHandler)
        {
            requestHandler(uri, method, headers, body, responseHandler);
        }
    }
}
