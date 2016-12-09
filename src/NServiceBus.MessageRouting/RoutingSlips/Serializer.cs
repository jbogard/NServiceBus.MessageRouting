using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace NServiceBus.MessageRouting.RoutingSlips
{
    static class Serializer
    {
        static Newtonsoft.Json.JsonSerializer jsonSerializer = Newtonsoft.Json.JsonSerializer.CreateDefault();

        public static T Deserialize<T>(string json)
        {
            using (var stringReader = new StringReader(json))
            using (var jsonReader = new JsonTextReader(stringReader))
            {
                return jsonSerializer.Deserialize<T>(jsonReader);
            }
        }

        public static string Serialize(object target)
        {
            var stringBuilder = new StringBuilder();
            using (var stringWriter = new StringWriter(stringBuilder))
            using (var jsonWriter = new JsonTextWriter(stringWriter))
            {
                jsonSerializer.Serialize(jsonWriter, target);
            }
            return stringBuilder.ToString();
        }
    }
}