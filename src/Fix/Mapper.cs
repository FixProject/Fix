using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fix
{
    using AppFunc = Func<IDictionary<string,object>, Task>;
    public static class Mapper
    {
        public static Func<AppFunc, AppFunc> Map(string mapPath, Func<AppFunc, AppFunc> mappedFunc)
        {
            if (mapPath == null) throw new ArgumentNullException("mapPath");
            mapPath = '/' + mapPath.Trim('/');

            return next =>
            {
                var func = mappedFunc(NotFound);
                return async env =>
                {
                    var path = (string)env["owin.RequestPath"];
                    if (path.Equals(mapPath, StringComparison.OrdinalIgnoreCase))
                    {
                        var pathBase = env.GetValueOrDefault("owin.RequestPathBase", string.Empty);
                        env["owin.RequestPathBase"] = pathBase + mapPath;
                        await func(env);
                    }
                    else if (path.StartsWith(mapPath + '/'))
                    {
                        var pathBase = env.GetValueOrDefault("owin.RequestPathBase", string.Empty);
                        env["owin.RequestPathBase"] = pathBase + mapPath;
                        env["owin.RequestPath"] = path.Substring(mapPath.Length);
                        await func(env);
                    }
                    else
                    {
                        await next(env);
                    }
                };
            };
        }

        private static Task NotFound(IDictionary<string, object> env)
        {
            env["owin.ResponseStatusCode"] = 404;
            return Task.FromResult(0);
        }
        public static T GetValueOrDefault<T>(this IDictionary<string, object> dict, string key,
            T defaultValue = default(T))
        {
            object value;
            if (!dict.TryGetValue(key, out value)) return defaultValue;
            return value != null ? (T)value : defaultValue;
        }

    }
}
