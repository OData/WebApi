// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Json;
using Newtonsoft.Json.Linq;

namespace System.Net.Http.Formatting
{
    public class JTokenCreatorSurrogate : InstanceCreatorSurrogate
    {
        private const int MaxDepth = 4;

        public override bool CanCreateInstanceOf(Type type)
        {
            return (type == typeof(JToken) || type == typeof(JArray) || type == typeof(JObject) || type == typeof(JValue));
        }

        public override object CreateInstanceOf(Type type, Random rndGen)
        {
            if (!this.CanCreateInstanceOf(type))
            {
                return null;
            }

            if (type == typeof(JToken))
            {
                return CreateJToken(rndGen, 0);
            }
            else if (type == typeof(JArray))
            {
                return CreateJArray(rndGen, 0);
            }
            else if (type == typeof(JObject))
            {
                return CreateJObject(rndGen, 0);
            }
            else
            {
                return CreateJsonPrimitive(rndGen);
            }
        }

        private static JToken CreateJToken(Random rndGen, int depth)
        {
            if (rndGen.Next() < CreatorSettings.NullValueProbability)
            {
                return null;
            }

            if (depth < MaxDepth)
            {
                switch (rndGen.Next(10))
                {
                    case 0:
                    case 1:
                    case 2:
                        // 30% chance to create an array
                        return CreateJArray(rndGen, depth);
                    case 3:
                    case 4:
                    case 5:
                        // 30% chance to create an object
                        return CreateJObject(rndGen, depth);
                    default:
                        // 40% chance to create a primitive
                        break;
                }
            }

            return CreateJsonPrimitive(rndGen);
        }

        static JToken CreateJsonPrimitive(Random rndGen)
        {
            switch (rndGen.Next(17))
            {
                case 0:
                    return PrimitiveCreator.CreateInstanceOfChar(rndGen);
                case 1:
                    return new JValue(PrimitiveCreator.CreateInstanceOfByte(rndGen));
                case 2:
                    return PrimitiveCreator.CreateInstanceOfSByte(rndGen);
                case 3:
                    return PrimitiveCreator.CreateInstanceOfInt16(rndGen);
                case 4:
                    return PrimitiveCreator.CreateInstanceOfUInt16(rndGen);
                case 5:
                    return PrimitiveCreator.CreateInstanceOfInt32(rndGen);
                case 6:
                    return PrimitiveCreator.CreateInstanceOfUInt32(rndGen);
                case 7:
                    return PrimitiveCreator.CreateInstanceOfInt64(rndGen);
                case 8:
                    return PrimitiveCreator.CreateInstanceOfUInt64(rndGen);
                case 9:
                    return PrimitiveCreator.CreateInstanceOfDecimal(rndGen);
                case 10:
                    return PrimitiveCreator.CreateInstanceOfDouble(rndGen);
                case 11:
                    return PrimitiveCreator.CreateInstanceOfSingle(rndGen);
                case 12:
                    return PrimitiveCreator.CreateInstanceOfDateTime(rndGen);
                case 13:
                    return PrimitiveCreator.CreateInstanceOfDateTimeOffset(rndGen);
                case 14:
                case 15:
                    // TODO: 199532 fix uri comparer
                    return PrimitiveCreator.CreateInstanceOfString(rndGen);
                default:
                    return PrimitiveCreator.CreateInstanceOfBoolean(rndGen);
            }
        }

        static JArray CreateJArray(Random rndGen, int depth)
        {
            int size = rndGen.Next(CreatorSettings.MaxArrayLength);
            if (CreatorSettings.NullValueProbability == 0 && size == 0)
            {
                size++;
            }

            JArray result = new JArray();
            for (int i = 0; i < size; i++)
            {
                result.Add(CreateJToken(rndGen, depth + 1));
            }

            return result;
        }

        static JObject CreateJObject(Random rndGen, int depth)
        {
            const string keyChars = "abcdefghijklmnopqrstuvwxyz0123456789";
            int size = rndGen.Next(CreatorSettings.MaxArrayLength);
            if (CreatorSettings.NullValueProbability == 0 && size == 0)
            {
                size++;
            }

            JObject result = new JObject();
            for (int i = 0; i < size; i++)
            {
                string key;
                do
                {
                    key = PrimitiveCreator.CreateInstanceOfString(rndGen, 10, keyChars);
                } while (result.Count > 0 && ((IDictionary<string, JToken>)result).ContainsKey(key));

                result.Add(key, CreateJToken(rndGen, depth + 1));
            }

            return result;
        }
    }
}