using Newtonsoft.Json;

namespace JsonUtils
{
    public static class JsonHelper
    {
        public static T Deserialize<T>(string jsonString)
        {
            return JsonConvert.DeserializeObject<T>(jsonString);
        }

        public static string Serialize<T>(T customObject, Formatting formatting = Formatting.None)
        {
            return JsonConvert.SerializeObject(customObject, formatting);
        }
    }
}
