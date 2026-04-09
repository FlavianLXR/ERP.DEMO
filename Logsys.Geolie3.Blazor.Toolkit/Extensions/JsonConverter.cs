using Newtonsoft.Json;

namespace ERP.DEMO.Toolkit.Extensions
{
    public class JsonConverter
    {
        public static string SerializeObjectToJson<T>(T model)
        {
            if (model == null) return null;
            return JsonConvert.SerializeObject(model);
        }

        public static T DeserializeJsonToObject<T>(string json)
        {
            if (string.IsNullOrEmpty(json)) return default(T);
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
