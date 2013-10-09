using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;

namespace FixAsp
{
    using System.Text;
    using UseAction = Action<
        Func<
            IDictionary<string, object>, // OWIN Environment
            Func<IDictionary<string, object>, Task>, // Next component in pipeline
            Task // Return
        >
    >;

    public static class OwinAppSetup
    {
        public static void Setup(UseAction use)
        {
            use(async (env, next) =>
            {
                var stream = (Stream) env["owin.ResponseBody"];
                await stream.WriteAsync("<h1>OWIN!</h1>");
                env["owin.ResponseStatusCode"] = 200;
            });
        }

        public static Task WriteAsync(this Stream stream, string text)
        {
            var bytes = Encoding.Default.GetBytes("<h1>OWIN!</h1>");
            return stream.WriteAsync(bytes, 0, bytes.Length);
        }
    }
}