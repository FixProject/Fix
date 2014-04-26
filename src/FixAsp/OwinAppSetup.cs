using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;

namespace FixAsp
{
    using System.Text;
    using Fix;
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public static class OwinAppSetup
    {
        public static void Setup(Action<Func<AppFunc,AppFunc>>  use)
        {
            use(next => async env =>
            {
                var stream = (Stream) env["owin.ResponseBody"];
                await stream.WriteAsync("<h1>OWIN!</h1>");
                await stream.WriteAsync("<h2>" + env[OwinKeys.RequestPath] + "</h2>");
                env["owin.ResponseStatusCode"] = 200;
            });
        }

        public static Task WriteAsync(this Stream stream, string text)
        {
            var bytes = Encoding.Default.GetBytes(text);
            return stream.WriteAsync(bytes, 0, bytes.Length);
        }
    }
}