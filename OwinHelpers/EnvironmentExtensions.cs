using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Environment = System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string,object>>;

namespace OwinHelpers
{
    public static class EnvironmentExtensions
    {
        public static string GetScriptName(this Environment env)
        {
            return GetValue(env, "SCRIPT_NAME");
        }

        public static string GetRequestMethod(this Environment env)
        {
            return GetValue(env, "REQUEST_METHOD");
        }

        private static string GetValue(Environment env, string key)
        {
            var dictionary = env as IDictionary<string, object>;
            if (dictionary != null)
            {
                return dictionary.GetValueOrDefault(key, string.Empty).ToString();
            }
            var pair = env.FirstOrDefault(kvp => kvp.Key.Equals(key));
            return (pair.Value != null) ? pair.Value.ToString() : string.Empty;
        }
    }
}
