//namespace Fix
//{
//    using System;
//    using System.Collections.Generic;
//    using System.IO;
//    using System.Threading;
//    using System.Threading.Tasks;
//    using _BodyDelegate = System.Func<System.IO.Stream,System.Threading.CancellationToken,System.Threading.Tasks.Task>;
//    using _ResponseHandler = System.Func<int, System.Collections.Generic.IDictionary<string, string[]>, System.Func<System.IO.Stream, System.Threading.CancellationToken, System.Threading.Tasks.Task>, System.Threading.Tasks.Task>;
//    using _App = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Collections.Generic.IDictionary<string, string[]>, System.IO.Stream, System.Threading.CancellationToken, System.Func<int, System.Collections.Generic.IDictionary<string, string[]>, System.Func<System.IO.Stream, System.Threading.CancellationToken, System.Threading.Tasks.Task>, System.Threading.Tasks.Task>, System.Threading.Tasks.Task>;

//    public delegate Task Body(Stream stream, CancellationToken cancellationToken);

//    public delegate Task Respond(int status, IDictionary<string, string[]> headers, Body body);

//    public delegate Task App(IDictionary<string, object> env, IDictionary<string, string[]> headers, Stream input, CancellationToken cancellation, Respond respond);

//    public static class Converter
//    {
//        public static App ToApp(Func<IDictionary<string, object>, IDictionary<string, string[]>, Stream, CancellationToken, Func<int, IDictionary<string, string[]>, Func<Stream, CancellationToken, Task>, Task>, Task> appFunc)
//        {
//            return (env, headers, input, cancellation, respond) =>
//                   appFunc(env, headers, input, cancellation, respond.ToRespondFunc());
//        }

//        public static Body ToBody(this Func<Stream,CancellationToken,Task> bodyFunc)
//        {
//            return (stream, token) => bodyFunc(stream, token);
//        }

//        public static Func<int, IDictionary<string,string[]>, Func<Stream,CancellationToken,Task>,Task> ToRespondFunc(this Respond respond)
//        {
//            return (status, headers, body) => respond(status, headers, body.ToBody());
//        }
//    }
//}