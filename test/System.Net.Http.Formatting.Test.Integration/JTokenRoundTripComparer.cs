// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http.Internal;
using Newtonsoft.Json.Linq;

namespace System.Net.Http.Formatting
{
    internal class JTokenRoundTripComparer
    {
        public static bool Compare(JToken initValue, JToken newValue)
        {
            if (initValue == null && newValue == null)
            {
                return true;
            }

            if (initValue == null || newValue == null)
            {
                return false;
            }

            if (initValue is JValue)
            {
                string initStr;
                if (initValue.Type == JTokenType.String)
                {
                    initStr = initValue.ToString();
                }
                else
                {
                    initStr = ((JValue)initValue).Value.ToString();
                }

                string newStr;
                if (newValue is JValue)
                {
                    newStr = newValue.ToString();
                    initStr = UriQueryUtility.UrlDecode(UriQueryUtility.UrlEncode(initStr));
                    return initStr.Equals(newStr);
                }
                else if (newValue is JObject && ((JObject)newValue).Count == 1)
                {
                    initStr = String.Format("{0}", initValue.ToString());
                    return ((IDictionary<string, JToken>)newValue).ContainsKey(initStr);
                }

                return false;
            }

            if (((JContainer)initValue).Count != ((JContainer)newValue).Count)
            {
                return false;
            }

            if (initValue is JObject && newValue is JObject)
            {
                foreach (KeyValuePair<string, JToken> item in (JObject)initValue)
                {
                    if (!Compare(item.Value, newValue[item.Key]))
                    {
                        return false;
                    }
                }

                return true;
            }

            if (initValue is JArray && newValue is JArray)
            {
                for (int i = 0; i < ((JArray)initValue).Count; i++)
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
