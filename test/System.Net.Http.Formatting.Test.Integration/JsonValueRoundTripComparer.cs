using System.Collections.Generic;
using System.Json;
using System.Net.Http.Internal;

namespace System.Net.Http.Formatting
{
    internal class JsonValueRoundTripComparer
    {
        public static bool Compare(JsonValue initValue, JsonValue newValue)
        {
            if (initValue == null && newValue == null)
            {
                return true;
            }

            if (initValue == null || newValue == null)
            {
                return false;
            }

            if (initValue is JsonPrimitive)
            {
                string initStr;
                if (initValue.JsonType == JsonType.String)
                {
                    initStr = initValue.ToString();
                }
                else
                {
                    initStr = String.Format("\"{0}\"", ((JsonPrimitive)initValue).Value.ToString());
                }

                string newStr;
                if (newValue is JsonPrimitive)
                {
                    newStr = newValue.ToString();
                    initStr = UriQueryUtility.UrlDecode(UriQueryUtility.UrlEncode(initStr));
                    return initStr.Equals(newStr);
                }
                else if (newValue is JsonObject && newValue.Count == 1)
                {
                    initStr = String.Format("{0}", initValue.ToString());
                    return ((JsonObject)newValue).Keys.Contains(initStr);
                }

                return false;
            }

            if (initValue.Count != newValue.Count)
            {
                return false;
            }

            if (initValue is JsonObject && newValue is JsonObject)
            {
                foreach (KeyValuePair<string, JsonValue> item in initValue)
                {
                    if (!Compare(item.Value, newValue[item.Key]))
                    {
                        return false;
                    }
                }

                return true;
            }

            if (initValue is JsonArray && newValue is JsonArray)
            {
                for (int i = 0; i < initValue.Count; i++)
                {
                    if (!Compare(initValue[i], newValue[i]))
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }
    }
}
