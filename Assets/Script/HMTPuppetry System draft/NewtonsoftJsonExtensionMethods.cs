using Newtonsoft.Json.Linq;

namespace HMT.Puppetry {
    public static class NewtonsoftJsonExtensionMethods {
        public static T TryGetDefault<T>(this JObject job, string key, T defaultValue) {
            return job.TryGetValue(key, out JToken token) ? token.Value<T>() : defaultValue;
        }
    }
}