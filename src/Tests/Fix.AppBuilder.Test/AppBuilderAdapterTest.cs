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
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class AppBuilderAdapterTest
    {
        [Fact]
        public void FixWorksWithIAppBuilderUsingAdapter()
        {
            var assemblies = new[] {Assembly.GetExecutingAssembly(), typeof (AppBuilderAdapter).Assembly};
            var startupEnv = new Dictionary<string, object>();
            var appFuncBuilder = AppFuncBuilder.Create(assemblies, startupEnv);
            var func = appFuncBuilder.Build();
            var dict = new Dictionary<string, object>();
            func(dict);
            Assert.Equal("Yes", startupEnv["Constructed"]);
            Assert.Equal("Passed", dict["Test"]);
        }
    }

    public class OwinAppSetup
    {
        public OwinAppSetup(IDictionary<string,object> startupEnv)
        {
            startupEnv["Constructed"] = "Yes";
        }

        public void Setup(Action<Func<AppFunc,AppFunc>> use)
        {
            use(Run);
        }

        private static AppFunc Run(AppFunc _)
        {
            return env =>
            {
                env["Test"] = "Passed";
                var tcs = new TaskCompletionSource<int>();
                tcs.SetResult(0);
                return tcs.Task;
            };
        }
    }
}
