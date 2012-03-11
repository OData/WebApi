using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace System.Net.Http.Formatting
{
    internal class JsonValueConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(JsonValue).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JToken token = JToken.ReadFrom(reader);
            return ConvertJTokenToJsonValue(token);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            JToken token = ConvertJsonValueToJToken(value as JsonValue);
            token.WriteTo(writer);
        }

        private static JToken ConvertJsonValueToJToken(JsonValue value)
        {
            if (value == null)
            {
                return new JValue((object)null);
            }

            switch (value.JsonType)
            {
                case JsonType.Boolean:
                case JsonType.Number:
                case JsonType.String:
                    JsonPrimitive primitive = value as JsonPrimitive;
                    return new JValue(primitive.Value);

                case JsonType.Array:
                    JArray jsonArray = new JArray();
                    foreach (JsonValue property in value as JsonArray)
                    {
                        jsonArray.Add(ConvertJsonValueToJToken(property));
                    }
                    return jsonArray;

                case JsonType.Object:
                    JObject jsonObject = new JObject();
                    foreach (KeyValuePair<string, JsonValue> kvp in value as JsonObject)
                    {
                        jsonObject.Add(kvp.Key, ConvertJsonValueToJToken(kvp.Value));
                    }
                    return jsonObject;

                case JsonType.Default:
                default:
                    throw new NotSupportedException();
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily", Justification = "token is never cast to JValue twice in the same code path")]
        private static JsonValue ConvertJTokenToJsonValue(JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.Null:
                    return null;

                case JTokenType.Boolean:
                    return new JsonPrimitive((bool)((JValue)token).Value);

                case JTokenType.Float:
                    return new JsonPrimitive((double)((JValue)token).Value);

                case JTokenType.Integer:
                    return new JsonPrimitive((long)((JValue)token).Value);

                case JTokenType.String:
                    return new JsonPrimitive((string)((JValue)token).Value);

                case JTokenType.Array:
                    JsonArray jsonArray = new JsonArray();
                    foreach (JToken item in (JArray)token)
                    {
                        jsonArray.Add(ConvertJTokenToJsonValue(item));
                    }
                    return jsonArray;

                case JTokenType.Object:
                    JsonObject jsonObject = new JsonObject();
                    foreach (KeyValuePair<string, JToken> kvp in (JObject)token)
                    {
                        jsonObject.Add(kvp.Key, ConvertJTokenToJsonValue(kvp.Value));
                    }
                    return jsonObject;

                case JTokenType.None:
                default:
                    throw new NotSupportedException();
            }
        }
    }
}