using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fix.AppBuilder.Test
{
    using System.Reflection;
    using Owin;
    using Xunit;

    public class AppBuilderAdapterTest
    {
        [Fact]
        public void FixWorksWithIAppBuilderUsingAdapter()
        {
            var assemblies = new[] {Assembly.GetExecutingAssembly(), typeof (AppBuilderAdapter).Assembly};
            var appFuncBuilder = AppFuncBuilder.Create(assemblies);
            var func = appFuncBuilder.Build();
            var dict = new Dictionary<string, object>();
            func(dict);
            Assert.Equal("Passed", dict["Test"]);
        }
    }

    public class OwinAppSetup
    {
        public static void Setup(IAppBuilder app)
        {
            app.Use((Func<IDictionary<string,object>, Func<IDictionary<string, object>, Task>, Task>)Run);
        }

        private static Task Run(IDictionary<string, object> env, Func<IDictionary<string, object>, Task> next)
        {
            env["Test"] = "Passed";
            var tcs = new TaskCompletionSource<int>();
            tcs.SetResult(0);
            return tcs.Task;
        }
    }
}
