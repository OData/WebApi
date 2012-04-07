// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;

namespace System.Json
{
    internal static class JsonValueVerifier
    {
        public static bool Compare(JsonValue objA, JsonValue objB)
        {
            if (objA == null && objB == null)
            {
                return true;
            }

            if ((objA == null && objB != null) || (objA != null && objB == null))
            {
                Log.Info("JsonValueVerifier Error: At least one of the JsonValue compared is null");
                return false;
            }

            if (objA.JsonType != objB.JsonType)
            {
                Log.Info("JsonValueVerifier Error: These two JsonValues are not of the same Type!");
                Log.Info("objA is of type {0} while objB is of type {1}", objA.JsonType.ToString(), objB.JsonType.ToString());
                return false;
            }

            return CompareJsonValues(objA, objB);
        }

        public static bool CompareStringLists(List<string> strListA, List<string> strListB)
        {
            bool retValue = true;
            if (strListA.Count != strListB.Count)
            {
                retValue = false;
            }
            else
            {
                for (int i = 0; i < strListA.Count; i++)
                {
                    if (strListA[i] != strListB[i])
                    {
                        retValue = false;
                        break;
                    }
                }
            }

            return retValue;
        }

        // Because we are currently taking a "flat design" model on JsonValues, the intellense doesn't work, and we have to be smart about what to verify
        // and what not to so to avoid any potentially invalid access
        private static bool CompareJsonValues(JsonValue objA, JsonValue objB)
        {
            bool retValue = false;
            switch (objA.JsonType)
            {
                case JsonType.Array:
                    retValue = CompareJsonArrayTypes((JsonArray)objA, (JsonArray)objB);
                    break;
                case JsonType.Object:
                    retValue = CompareJsonObjectTypes((JsonObject)objA, (JsonObject)objB);
                    break;
                case JsonType.Boolean:
                case JsonType.Number:
                case JsonType.String:
                    retValue = CompareJsonPrimitiveTypes((JsonPrimitive)objA, (JsonPrimitive)objB);
                    break;
                default:
                    Log.Info("JsonValueVerifier Error: the JsonValue isn’t an array, a complex type or a primitive type!");
                    break;
            }

            return retValue;
        }

        private static bool CompareJsonArrayTypes(JsonArray objA, JsonArray objB)
        {
            bool retValue = true;

            if (objA == null || objB == null || objA.Count != objB.Count || objA.IsReadOnly != objB.IsReadOnly)
            {
                return false;
            }

            try
            {
                for (int i = 0; i < objA.Count; i++)
                {
                    if (!Compare(objA[i], objB[i]))
                    {
                        Log.Info("JsonValueVerifier (JsonArrayType) Error: objA[{0}] = {1}", i, objA[i].ToString());
                        Log.Info("JsonValueVerifier (JsonArrayType) Error: objB[{0}] = {1}", i, objB[i].ToString());
                        return false;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Info("JsonValueVerifier (JsonArrayType) Error: An Exception was thrown: " + e);
                return false;
            }

            return retValue;
        }

        private static bool CompareJsonObjectTypes(JsonObject objA, JsonObject objB)
        {
            bool retValue = true;

            try
            {
                if (objA.Keys.Count != objB.Keys.Count)
                {
                    Log.Info("JsonValueVerifier (JsonObjectTypes) Error: objA.Keys.Count does not match objB.Keys.Count!");
                    Log.Info("JsonValueVerifier (JsonObjectTypes) Error: objA.Keys.Count = {0}, objB.Keys.Count = {1}", objA.Keys.Count, objB.Keys.Count);
                    return false;
                }

                if (objA.Keys.IsReadOnly != objB.Keys.IsReadOnly)
                {
                    Log.Info("JsonValueVerifier (JsonObjectTypes) Error: objA.Keys.IsReadOnly does not match objB.Keys.IsReadOnly!");
                    Log.Info("JsonValueVerifier (JsonObjectTypes) Error: objA.Keys.IsReadOnly = {0}, objB.Keys.IsReadOnly = {1}", objA.Keys.IsReadOnly, objB.Keys.IsReadOnly);
                    return false;
                }
                else
                {
                    foreach (string keyA in objA.Keys)
                    {
                        if (!objB.ContainsKey(keyA))
                        {
                            Log.Info("JsonValueVerifier (JsonObjectTypes) Error: objB does not contain Key " + keyA + "!");
                            return false;
                        }

                        if (!Compare(objA[keyA], objB[keyA]))
                        {
                            Log.Info("JsonValueVerifier (JsonObjectTypes) Error: objA[" + keyA + "] = " + objA[keyA]);
                            Log.Info("JsonValueVerifier (JsonObjectTypes) Error: objB[" + keyA + "] = " + objB[keyA]);
                            return false;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Info("JsonValueVerifier (JsonObjectTypes) Error: An Exception was thrown: " + e);
                return false;
            }

            return retValue;
        }

        private static bool CompareJsonPrimitiveTypes(JsonPrimitive objA, JsonPrimitive objB)
        {
            try
            {
                if (objA.ToString() != objB.ToString())
                {
                    // Special case due to daylight saving hours change: every March on the morning of the third Sunday, we adjust the time 
                    // from 2am to 3am straight, so for that one hour 2:13am = 3:15am.  We must result to the UTC ticks to verify the actual 
                    // time is always the same, regardless of the loc/glob setup on the machine
                    if (objA.ToString().StartsWith("\"\\/Date(") && objA.ToString().EndsWith(")\\/\""))
                    {
                        return GetUTCTicks(objA) == GetUTCTicks(objB);
                    }
                    else
                    {
                        Log.Info("JsonValueVerifier (JsonPrimitiveTypes) Error: objA = " + objA.ToString());
                        Log.Info("JsonValueVerifier (JsonPrimitiveTypes) Error: objB = " + objB.ToString());
                        return false;
                    }
                }
                else
                {
                    return true;
                }
            }
            catch (Exception e)
            {
                Log.Info("JsonValueVerifier (JsonPrimitiveTypes) Error: An Exception was thrown: " + e);
                return false;
            }
        }

        // the input JsonPrimitive DateTime format is "\/Date(24735422733034-0700)\/" or "\/Date(24735422733034)\/"
        // the only thing useful for us is the UTC ticks "24735422733034"
        // everything after - if present - is just an optional offset between the local time and UTC
        private static string GetUTCTicks(JsonPrimitive jprim)
        {
            string retValue = String.Empty;

            string origStr = jprim.ToString();
            int startIndex = origStr.IndexOf("Date(") + 5;
            int endIndex = origStr.IndexOf('-', startIndex + 1); // the UTC ticks can start with a '-' sign (dates prior to 1970/01/01)

            // if the optional offset is present in the data format, we want to take only the UTC ticks
            if (startIndex < endIndex)
            {
                retValue = origStr.Substring(startIndex, endIndex - startIndex);
            }
            else
            {
                // otherwise we assume the time format is without the oiptional offset, or unexpected, and use the whole string for comparison.
                retValue = origStr;
            }

            return retValue;
        }
    }

    internal static class SpecialJsonValueHelper
    {
        public static JsonArray CreateDeepLevelJsonValuePair(int seed, out JsonArray newOrderJson)
        {
            Log.Info("Seed: {0}", seed);
            Random rndGen = new Random(seed);

            bool myBool = PrimitiveCreator.CreateInstanceOfBoolean(rndGen);
            byte myByte = PrimitiveCreator.CreateInstanceOfByte(rndGen);
            DateTime myDatetime = PrimitiveCreator.CreateInstanceOfDateTime(rndGen);
            DateTimeOffset myDateTimeOffset = PrimitiveCreator.CreateInstanceOfDateTimeOffset(rndGen);
            decimal myDecimal = PrimitiveCreator.CreateInstanceOfDecimal(rndGen);
            double myDouble = PrimitiveCreator.CreateInstanceOfDouble(rndGen);
            short myInt16 = PrimitiveCreator.CreateInstanceOfInt16(rndGen);
            int myInt32 = PrimitiveCreator.CreateInstanceOfInt32(rndGen);
            long myInt64 = PrimitiveCreator.CreateInstanceOfInt64(rndGen);
            sbyte mySByte = PrimitiveCreator.CreateInstanceOfSByte(rndGen);
            float mySingle = PrimitiveCreator.CreateInstanceOfSingle(rndGen);
            string myString = PrimitiveCreator.CreateInstanceOfString(rndGen, 20, null);
            ushort myUInt16 = PrimitiveCreator.CreateInstanceOfUInt16(rndGen);
            uint myUInt32 = PrimitiveCreator.CreateInstanceOfUInt32(rndGen);
            ulong myUInt64 = PrimitiveCreator.CreateInstanceOfUInt64(rndGen);
            JsonArray myArray = new JsonArray { myBool, myByte, myDatetime, myDateTimeOffset, myDecimal, myDouble, myInt16, myInt32, myInt64, mySByte, mySingle, myString, myUInt16, myUInt32, myUInt64 };
            JsonArray myArrayLevel2 = new JsonArray { myArray, myArray, myArray };
            JsonArray myArrayLevel3 = new JsonArray { myArrayLevel2, myArrayLevel2, myArrayLevel2 };
            JsonArray myArrayLevel4 = new JsonArray { myArrayLevel3, myArrayLevel3, myArrayLevel3 };
            JsonArray myArrayLevel5 = new JsonArray { myArrayLevel4, myArrayLevel4, myArrayLevel4 };
            JsonArray myArrayLevel6 = new JsonArray { myArrayLevel5, myArrayLevel5, myArrayLevel5 };
            JsonArray myArrayLevel7 = new JsonArray { myArrayLevel6, myArrayLevel6, myArrayLevel6 };

            JsonArray sourceJson = BuildJsonArrayinSequence1(myBool, myByte, myDatetime, myDateTimeOffset, myDecimal, myDouble, myInt16, myInt32, myInt64, mySByte, mySingle, myString, myUInt16, myUInt32, myUInt64, myArray, myArrayLevel2, myArrayLevel3, myArrayLevel4, myArrayLevel5, myArrayLevel6, myArrayLevel7);

            newOrderJson = BuildJsonArrayinSequence2(myBool, myByte, myDatetime, myDateTimeOffset, myDecimal, myDouble, myInt16, myInt32, myInt64, mySByte, mySingle, myString, myUInt16, myUInt32, myUInt64, myArray, myArrayLevel2, myArrayLevel3, myArrayLevel4, myArrayLevel5, myArrayLevel6, myArrayLevel7);

            return sourceJson;
        }

        public static JsonValue CreateDeepLevelJsonValue()
        {
            int seed = Environment.TickCount;
            Log.Info("Seed: {0}", seed);
            Random rndGen = new Random(seed);

            bool myBool = PrimitiveCreator.CreateInstanceOfBoolean(rndGen);
            byte myByte = PrimitiveCreator.CreateInstanceOfByte(rndGen);
            DateTime myDatetime = PrimitiveCreator.CreateInstanceOfDateTime(rndGen);
            DateTimeOffset myDateTimeOffset = PrimitiveCreator.CreateInstanceOfDateTimeOffset(rndGen);
            decimal myDecimal = PrimitiveCreator.CreateInstanceOfDecimal(rndGen);
            double myDouble = PrimitiveCreator.CreateInstanceOfDouble(rndGen);
            short myInt16 = PrimitiveCreator.CreateInstanceOfInt16(rndGen);
            int myInt32 = PrimitiveCreator.CreateInstanceOfInt32(rndGen);
            long myInt64 = PrimitiveCreator.CreateInstanceOfInt64(rndGen);
            sbyte mySByte = PrimitiveCreator.CreateInstanceOfSByte(rndGen);
            float mySingle = PrimitiveCreator.CreateInstanceOfSingle(rndGen);
            string myString = PrimitiveCreator.CreateInstanceOfString(rndGen, 20, null);
            ushort myUInt16 = PrimitiveCreator.CreateInstanceOfUInt16(rndGen);
            uint myUInt32 = PrimitiveCreator.CreateInstanceOfUInt32(rndGen);
            ulong myUInt64 = PrimitiveCreator.CreateInstanceOfUInt64(rndGen);
            JsonArray myArray = new JsonArray { myBool, myByte, myDatetime, myDateTimeOffset, myDecimal, myDouble, myInt16, myInt32, myInt64, mySByte, mySingle, myString, myUInt16, myUInt32, myUInt64 };
            JsonArray myArrayLevel2 = new JsonArray { myArray, myArray, myArray };
            JsonArray myArrayLevel3 = new JsonArray { myArrayLevel2, myArrayLevel2, myArrayLevel2 };
            JsonArray myArrayLevel4 = new JsonArray { myArrayLevel3, myArrayLevel3, myArrayLevel3 };
            JsonArray myArrayLevel5 = new JsonArray { myArrayLevel4, myArrayLevel4, myArrayLevel4 };
            JsonArray myArrayLevel6 = new JsonArray { myArrayLevel5, myArrayLevel5, myArrayLevel5 };
            JsonArray myArrayLevel7 = new JsonArray { myArrayLevel6, myArrayLevel6, myArrayLevel6 };

            JsonArray sourceJson = BuildJsonArrayinSequence1(myBool, myByte, myDatetime, myDateTimeOffset, myDecimal, myDouble, myInt16, myInt32, myInt64, mySByte, mySingle, myString, myUInt16, myUInt32, myUInt64, myArray, myArrayLevel2, myArrayLevel3, myArrayLevel4, myArrayLevel5, myArrayLevel6, myArrayLevel7);

            return sourceJson;
        }

        public static JsonObject CreateRandomPopulatedJsonObject(int seed, int length)
        {
            JsonObject myObject;

            myObject = new JsonObject(
                new Dictionary<string, JsonValue>()
                {
                    { "Name", "myArray" },
                    { "Index", 1 }
                });

            for (int i = myObject.Count; i < length / 2; i++)
            {
                myObject.Add(PrimitiveCreator.CreateInstanceOfString(new Random(seed + i)), GetRandomJsonPrimitives(seed + (i * 2)));
            }

            for (int i = myObject.Count; i < length; i++)
            {
                myObject.Add(new KeyValuePair<string, JsonValue>(PrimitiveCreator.CreateInstanceOfString(new Random(seed + (i * 10))), GetRandomJsonPrimitives(seed + (i * 20))));
            }

            return myObject;
        }

        public static JsonArray CreatePrePopulatedJsonArray(int seed, int length)
        {
            JsonArray myObject;

            myObject = new JsonArray(new List<JsonValue>());

            for (int i = myObject.Count; i < length; i++)
            {
                myObject.Add(GetRandomJsonPrimitives(seed + i));
            }

            return myObject;
        }

        public static JsonObject CreateIndexPopulatedJsonObject(int seed, int length)
        {
            JsonObject myObject;
            myObject = new JsonObject(new Dictionary<string, JsonValue>() { });

            for (int i = myObject.Count; i < length; i++)
            {
                myObject.Add(i.ToString(CultureInfo.InvariantCulture), GetRandomJsonPrimitives(seed + i));
            }

            return myObject;
        }

        public static JsonValue[] CreatePrePopulatedJsonValueArray(int seed, int length)
        {
            JsonValue[] myObject = new JsonValue[length];

            for (int i = 0; i < length; i++)
            {
                myObject[i] = GetRandomJsonPrimitives(seed + i);
            }

            return myObject;
        }

        public static KeyValuePair<string, JsonValue> CreatePrePopulatedKeyValuePair(int seed)
        {
            KeyValuePair<string, JsonValue> myObject = new KeyValuePair<string, JsonValue>(seed.ToString(), GetRandomJsonPrimitives(seed));
            return myObject;
        }

        public static List<KeyValuePair<string, JsonValue>> CreatePrePopulatedListofKeyValuePair(int seed, int length)
        {
            List<KeyValuePair<string, JsonValue>> myObject = new List<KeyValuePair<string, JsonValue>>();

            for (int i = 0; i < length; i++)
            {
                myObject.Add(CreatePrePopulatedKeyValuePair(seed + i));
            }

            return myObject;
        }

        public static JsonPrimitive GetRandomJsonPrimitives(int seed)
        {
            JsonPrimitive myObject;
            Random rndGen = new Random(seed);

            int mod = seed % 13;
            switch (mod)
            {
                case 1:
                    myObject = new JsonPrimitive(PrimitiveCreator.CreateInstanceOfBoolean(rndGen));
                    break;
                case 2:
                    myObject = new JsonPrimitive(PrimitiveCreator.CreateInstanceOfByte(rndGen));
                    break;
                case 3:
                    myObject = new JsonPrimitive(PrimitiveCreator.CreateInstanceOfDateTime(rndGen));
                    break;
                case 4:
                    myObject = new JsonPrimitive(PrimitiveCreator.CreateInstanceOfDecimal(rndGen));
                    break;
                case 5:
                    myObject = new JsonPrimitive(PrimitiveCreator.CreateInstanceOfInt16(rndGen));
                    break;
                case 6:
                    myObject = new JsonPrimitive(PrimitiveCreator.CreateInstanceOfInt32(rndGen));
                    break;
                case 7:
                    myObject = new JsonPrimitive(PrimitiveCreator.CreateInstanceOfInt64(rndGen));
                    break;
                case 8:
                    myObject = new JsonPrimitive(PrimitiveCreator.CreateInstanceOfSByte(rndGen));
                    break;
                case 9:
                    myObject = new JsonPrimitive(PrimitiveCreator.CreateInstanceOfSingle(rndGen));
                    break;
                case 10:
                    string temp;
                    do
                    {
                        temp = PrimitiveCreator.CreateInstanceOfString(rndGen);
                    }
                    while (temp == null);

                    myObject = new JsonPrimitive(temp);
                    break;
                case 11:
                    myObject = new JsonPrimitive(PrimitiveCreator.CreateInstanceOfUInt16(rndGen));
                    break;
                case 12:
                    myObject = new JsonPrimitive(PrimitiveCreator.CreateInstanceOfUInt32(rndGen));
                    break;
                default:
                    myObject = new JsonPrimitive(PrimitiveCreator.CreateInstanceOfUInt64(rndGen));
                    break;
            }

            return myObject;
        }

        public static string GetUniqueNonNullInstanceOfString(int seed, JsonObject sourceJson)
        {
            string retValue = String.Empty;
            Random rndGen = new Random(seed);
            do
            {
                retValue = PrimitiveCreator.CreateInstanceOfString(rndGen);
            }
            while (retValue == null || sourceJson.Keys.Contains(retValue));

            return retValue;
        }

        public static JsonPrimitive GetUniqueValue(int seed, JsonObject sourceJson)
        {
            JsonPrimitive newValue;
            int i = 0;
            do
            {
                newValue = SpecialJsonValueHelper.GetRandomJsonPrimitives(seed + i);
                i++;
            }
            while (sourceJson.ToString().IndexOf(newValue.ToString()) > 0);

            return newValue;
        }

        private static JsonArray BuildJsonArrayinSequence2(bool myBool, byte myByte, DateTime myDatetime, DateTimeOffset myDateTimeOffset, decimal myDecimal, double myDouble, short myInt16, int myInt32, long myInt64, sbyte mySByte, float mySingle, string myString, ushort myUInt16, uint myUInt32, ulong myUInt64, JsonArray myArray, JsonArray myArrayLevel2, JsonArray myArrayLevel3, JsonArray myArrayLevel4, JsonArray myArrayLevel5, JsonArray myArrayLevel6, JsonArray myArrayLevel7)
        {
            JsonArray newOrderJson;
            newOrderJson = new JsonArray
            {
                new JsonObject { { "Name", "myArray" }, { "Index", 1 }, { "Obj", myArray } },
                new JsonObject { { "Name", "myArrayLevel2" }, { "Index", 2 }, { "Obj", myArrayLevel2 } },
                new JsonObject { { "Name", "myArrayLevel2" }, { "Index", 2 }, { "Obj", myArrayLevel2 } },
                new JsonObject { { "Name", "myUInt32" }, { "Index", 21 }, { "Obj", myUInt32 } },
                new JsonObject { { "Name", "myUInt32" }, { "Index", 21 }, { "Obj", myUInt32 } },
                new JsonObject { { "Name", "myArrayLevel3" }, { "Index", 3 }, { "Obj", myArrayLevel3 } },
                new JsonObject { { "Name", "myArrayLevel4" }, { "Index", 4 }, { "Obj", myArrayLevel4 } },
                new JsonObject { { "Name", "myArrayLevel4" }, { "Index", 4 }, { "Obj", myArrayLevel4 } },
                new JsonObject { { "Name", "myArrayLevel5" }, { "Index", 5 }, { "Obj", myArrayLevel5 } },
                new JsonObject { { "Name", "myArrayLevel3" }, { "Index", 3 }, { "Obj", myArrayLevel3 } },
                new JsonObject { { "Name", "myInt64" }, { "Index", 16 }, { "Obj", myInt64 } },
                new JsonObject { { "Name", "myArrayLevel5" }, { "Index", 5 }, { "Obj", myArrayLevel5 } },
                new JsonObject { { "Name", "myArrayLevel5" }, { "Index", 5 }, { "Obj", myArrayLevel5 } },
                new JsonObject { { "Name", "myArrayLevel4" }, { "Index", 4 }, { "Obj", myArrayLevel4 } },
                new JsonObject { { "Name", "myArrayLevel6" }, { "Index", 6 }, { "Obj", myArrayLevel6 } },
                new JsonObject { { "Name", "myInt64" }, { "Index", 16 }, { "Obj", myInt64 } },
                new JsonObject { { "Name", "myArrayLevel5" }, { "Index", 5 }, { "Obj", myArrayLevel5 } },
                new JsonObject { { "Name", "myArrayLevel5" }, { "Index", 5 }, { "Obj", myArrayLevel5 } },
                new JsonObject { { "Name", "myArrayLevel7" }, { "Index", 7 }, { "Obj", myArrayLevel7 } },
                new JsonObject { { "Name", "myDecimal" }, { "Index", 12 }, { "Obj", myDecimal } },
                new JsonObject { { "Name", "myArrayLevel7" }, { "Index", 7 }, { "Obj", myArrayLevel7 } },
                new JsonObject { { "Name", "myArrayLevel6" }, { "Index", 6 }, { "Obj", myArrayLevel6 } },
                new JsonObject { { "Name", "myArrayLevel4" }, { "Index", 4 }, { "Obj", myArrayLevel4 } },
                new JsonObject { { "Name", "myArrayLevel6" }, { "Index", 6 }, { "Obj", myArrayLevel6 } },
                new JsonObject { { "Name", "myArrayLevel7" }, { "Index", 7 }, { "Obj", myArrayLevel7 } },
                new JsonObject { { "Name", "myBool" }, { "Index", 8 }, { "Obj", myBool } },
                new JsonObject { { "Name", "myByte" }, { "Index", 9 }, { "Obj", myByte } },
                null,
                new JsonObject { { "Name", "myDecimal" }, { "Index", 12 }, { "Obj", myDecimal } },
                new JsonObject { { "Name", "myArrayLevel7" }, { "Index", 7 }, { "Obj", myArrayLevel7 } },
                new JsonObject { { "Name", "myByte" }, { "Index", 9 }, { "Obj", myByte } },
                new JsonObject { { "Name", "myArrayLevel7" }, { "Index", 7 }, { "Obj", myArrayLevel7 } },
                new JsonObject { { "Name", "myArrayLevel6" }, { "Index", 6 }, { "Obj", myArrayLevel6 } },
                new JsonObject { { "Name", "myDatetime" }, { "Index", 10 }, { "Obj", myDatetime } },
                new JsonObject { { "Name", "myArrayLevel6" }, { "Index", 6 }, { "Obj", myArrayLevel6 } },
                new JsonObject { { "Name", "myDatetime" }, { "Index", 10 }, { "Obj", myDatetime } },
                new JsonObject { { "Name", "myDateTimeOffset" }, { "Index", 11 }, { "Obj", myDateTimeOffset } },
                new JsonObject { { "Name", "myArrayLevel7" }, { "Index", 7 }, { "Obj", myArrayLevel7 } },
                new JsonObject { { "Name", "myUInt16" }, { "Index", 20 }, { "Obj", myUInt16 } },
                new JsonObject { { "Name", "myDateTimeOffset" }, { "Index", 11 }, { "Obj", myDateTimeOffset } },
                new JsonObject { { "Name", "myDecimal" }, { "Index", 12 }, { "Obj", myDecimal } },
                new JsonObject { { "Name", "myInt32" }, { "Index", 15 }, { "Obj", myInt32 } },
                new JsonObject { { "Name", "mySByte" }, { "Index", 17 }, { "Obj", mySByte } },
                new JsonObject { { "Name", "myDecimal" }, { "Index", 12 }, { "Obj", myDecimal } },
                new JsonObject { { "Name", "myDouble" }, { "Index", 13 }, { "Obj", myDouble } },
                new JsonObject { { "Name", "myInt16" }, { "Index", 14 }, { "Obj", myInt16 } },
                new JsonObject { { "Name", "myString" }, { "Index", 19 }, { "Obj", myString } },
                new JsonObject { { "Name", "myUInt16" }, { "Index", 20 }, { "Obj", myUInt16 } },
                new JsonObject { { "Name", "myUInt16" }, { "Index", 20 }, { "Obj", myUInt16 } },
                new JsonObject { { "Name", "myArrayLevel7" }, { "Index", 7 }, { "Obj", myArrayLevel7 } },
                new JsonObject { { "Name", "myUInt32" }, { "Index", 21 }, { "Obj", myUInt32 } },
                new JsonObject { { "Name", "myInt16" }, { "Index", 14 }, { "Obj", myInt16 } },
                new JsonObject { { "Name", "myInt32" }, { "Index", 15 }, { "Obj", myInt32 } },
                new JsonObject { { "Name", "mySByte" }, { "Index", 17 }, { "Obj", mySByte } },
                new JsonObject { { "Name", "myDateTimeOffset" }, { "Index", 11 }, { "Obj", myDateTimeOffset } },
                new JsonObject { { "Name", "myArrayLevel6" }, { "Index", 6 }, { "Obj", myArrayLevel6 } },
                new JsonObject { { "Name", "myDatetime" }, { "Index", 10 }, { "Obj", myDatetime } },
                new JsonObject { { "Name", "mySByte" }, { "Index", 17 }, { "Obj", mySByte } },
                new JsonObject { { "Name", "myInt32" }, { "Index", 15 }, { "Obj", myInt32 } },
                new JsonObject { { "Name", "myInt64" }, { "Index", 16 }, { "Obj", myInt64 } },
                new JsonObject { { "Name", "myUInt32" }, { "Index", 21 }, { "Obj", myUInt32 } },
                new JsonObject { { "Name", "myArrayLevel3" }, { "Index", 3 }, { "Obj", myArrayLevel3 } },
                new JsonObject { { "Name", "myInt64" }, { "Index", 16 }, { "Obj", myInt64 } },
                new JsonObject { { "Name", "mySByte" }, { "Index", 17 }, { "Obj", mySByte } },
                new JsonObject { { "Name", "mySByte" }, { "Index", 17 }, { "Obj", mySByte } },
                new JsonObject { { "Name", "mySingle" }, { "Index", 18 }, { "Obj", mySingle } },
                new JsonObject { { "Name", "myDateTimeOffset" }, { "Index", 11 }, { "Obj", myDateTimeOffset } },
                new JsonObject { { "Name", "myDecimal" }, { "Index", 12 }, { "Obj", myDecimal } },
                new JsonObject { { "Name", "myString" }, { "Index", 19 }, { "Obj", myString } },
                new JsonObject { { "Name", "myUInt64" }, { "Index", 22 }, { "Obj", myUInt64 } }
            };
            return newOrderJson;
        }

        private static JsonArray BuildJsonArrayinSequence1(bool myBool, byte myByte, DateTime myDatetime, DateTimeOffset myDateTimeOffset, decimal myDecimal, double myDouble, short myInt16, int myInt32, long myInt64, sbyte mySByte, float mySingle, string myString, ushort myUInt16, uint myUInt32, ulong myUInt64, JsonArray myArray, JsonArray myArrayLevel2, JsonArray myArrayLevel3, JsonArray myArrayLevel4, JsonArray myArrayLevel5, JsonArray myArrayLevel6, JsonArray myArrayLevel7)
        {
            JsonArray sourceJson = new JsonArray
            { 
                new JsonObject { { "Name", "myArray" }, { "Index", 1 }, { "Obj", myArray } },
                new JsonObject { { "Name", "myArrayLevel2" }, { "Index", 2 }, { "Obj", myArrayLevel2 } },
                new JsonObject { { "Name", "myArrayLevel2" }, { "Index", 2 }, { "Obj", myArrayLevel2 } },
                new JsonObject { { "Name", "myArrayLevel3" }, { "Index", 3 }, { "Obj", myArrayLevel3 } },
                new JsonObject { { "Name", "myArrayLevel3" }, { "Index", 3 }, { "Obj", myArrayLevel3 } },
                new JsonObject { { "Name", "myArrayLevel3" }, { "Index", 3 }, { "Obj", myArrayLevel3 } },
                new JsonObject { { "Name", "myArrayLevel4" }, { "Index", 4 }, { "Obj", myArrayLevel4 } },
                new JsonObject { { "Name", "myArrayLevel4" }, { "Index", 4 }, { "Obj", myArrayLevel4 } },
                new JsonObject { { "Name", "myArrayLevel4" }, { "Index", 4 }, { "Obj", myArrayLevel4 } },
                new JsonObject { { "Name", "myArrayLevel4" }, { "Index", 4 }, { "Obj", myArrayLevel4 } },
                new JsonObject { { "Name", "myArrayLevel5" }, { "Index", 5 }, { "Obj", myArrayLevel5 } },
                new JsonObject { { "Name", "myArrayLevel5" }, { "Index", 5 }, { "Obj", myArrayLevel5 } },
                new JsonObject { { "Name", "myArrayLevel5" }, { "Index", 5 }, { "Obj", myArrayLevel5 } },
                new JsonObject { { "Name", "myArrayLevel5" }, { "Index", 5 }, { "Obj", myArrayLevel5 } },
                new JsonObject { { "Name", "myArrayLevel5" }, { "Index", 5 }, { "Obj", myArrayLevel5 } },
                new JsonObject { { "Name", "myArrayLevel6" }, { "Index", 6 }, { "Obj", myArrayLevel6 } },
                new JsonObject { { "Name", "myArrayLevel6" }, { "Index", 6 }, { "Obj", myArrayLevel6 } },
                new JsonObject { { "Name", "myArrayLevel6" }, { "Index", 6 }, { "Obj", myArrayLevel6 } },
                new JsonObject { { "Name", "myArrayLevel6" }, { "Index", 6 }, { "Obj", myArrayLevel6 } },
                new JsonObject { { "Name", "myArrayLevel6" }, { "Index", 6 }, { "Obj", myArrayLevel6 } },
                new JsonObject { { "Name", "myArrayLevel6" }, { "Index", 6 }, { "Obj", myArrayLevel6 } },
                new JsonObject { { "Name", "myArrayLevel7" }, { "Index", 7 }, { "Obj", myArrayLevel7 } },
                new JsonObject { { "Name", "myArrayLevel7" }, { "Index", 7 }, { "Obj", myArrayLevel7 } },
                new JsonObject { { "Name", "myArrayLevel7" }, { "Index", 7 }, { "Obj", myArrayLevel7 } },
                new JsonObject { { "Name", "myArrayLevel7" }, { "Index", 7 }, { "Obj", myArrayLevel7 } },
                new JsonObject { { "Name", "myArrayLevel7" }, { "Index", 7 }, { "Obj", myArrayLevel7 } },
                new JsonObject { { "Name", "myArrayLevel7" }, { "Index", 7 }, { "Obj", myArrayLevel7 } },
                new JsonObject { { "Name", "myArrayLevel7" }, { "Index", 7 }, { "Obj", myArrayLevel7 } },
                new JsonObject { { "Name", "myBool" }, { "Index", 8 }, { "Obj", myBool } },
                new JsonObject { { "Name", "myByte" }, { "Index", 9 }, { "Obj", myByte } },
                null,
                new JsonObject { { "Name", "myByte" }, { "Index", 9 }, { "Obj", myByte } },
                new JsonObject { { "Name", "myDatetime" }, { "Index", 10 }, { "Obj", myDatetime } },
                new JsonObject { { "Name", "myDatetime" }, { "Index", 10 }, { "Obj", myDatetime } },
                new JsonObject { { "Name", "myDatetime" }, { "Index", 10 }, { "Obj", myDatetime } },
                new JsonObject { { "Name", "myDateTimeOffset" }, { "Index", 11 }, { "Obj", myDateTimeOffset } },
                new JsonObject { { "Name", "myDateTimeOffset" }, { "Index", 11 }, { "Obj", myDateTimeOffset } },
                new JsonObject { { "Name", "myDateTimeOffset" }, { "Index", 11 }, { "Obj", myDateTimeOffset } },
                new JsonObject { { "Name", "myDateTimeOffset" }, { "Index", 11 }, { "Obj", myDateTimeOffset } },
                new JsonObject { { "Name", "myDecimal" }, { "Index", 12 }, { "Obj", myDecimal } },
                new JsonObject { { "Name", "myDecimal" }, { "Index", 12 }, { "Obj", myDecimal } },
                new JsonObject { { "Name", "myDecimal" }, { "Index", 12 }, { "Obj", myDecimal } },
                new JsonObject { { "Name", "myDecimal" }, { "Index", 12 }, { "Obj", myDecimal } },
                new JsonObject { { "Name", "myDecimal" }, { "Index", 12 }, { "Obj", myDecimal } },
                new JsonObject { { "Name", "myDouble" }, { "Index", 13 }, { "Obj", myDouble } },
                new JsonObject { { "Name", "myInt16" }, { "Index", 14 }, { "Obj", myInt16 } },
                new JsonObject { { "Name", "myInt16" }, { "Index", 14 }, { "Obj", myInt16 } },
                new JsonObject { { "Name", "myInt32" }, { "Index", 15 }, { "Obj", myInt32 } },
                new JsonObject { { "Name", "myInt32" }, { "Index", 15 }, { "Obj", myInt32 } },
                new JsonObject { { "Name", "myInt32" }, { "Index", 15 }, { "Obj", myInt32 } },
                new JsonObject { { "Name", "myInt64" }, { "Index", 16 }, { "Obj", myInt64 } },
                new JsonObject { { "Name", "myInt64" }, { "Index", 16 }, { "Obj", myInt64 } },
                new JsonObject { { "Name", "myInt64" }, { "Index", 16 }, { "Obj", myInt64 } },
                new JsonObject { { "Name", "myInt64" }, { "Index", 16 }, { "Obj", myInt64 } },
                new JsonObject { { "Name", "mySByte" }, { "Index", 17 }, { "Obj", mySByte } },
                new JsonObject { { "Name", "mySByte" }, { "Index", 17 }, { "Obj", mySByte } },
                new JsonObject { { "Name", "mySByte" }, { "Index", 17 }, { "Obj", mySByte } },
                new JsonObject { { "Name", "mySByte" }, { "Index", 17 }, { "Obj", mySByte } },
                new JsonObject { { "Name", "mySByte" }, { "Index", 17 }, { "Obj", mySByte } },
                new JsonObject { { "Name", "mySingle" }, { "Index", 18 }, { "Obj", mySingle } },
                new JsonObject { { "Name", "myString" }, { "Index", 19 }, { "Obj", myString } },
                new JsonObject { { "Name", "myString" }, { "Index", 19 }, { "Obj", myString } },
                new JsonObject { { "Name", "myUInt16" }, { "Index", 20 }, { "Obj", myUInt16 } },
                new JsonObject { { "Name", "myUInt16" }, { "Index", 20 }, { "Obj", myUInt16 } },
                new JsonObject { { "Name", "myUInt16" }, { "Index", 20 }, { "Obj", myUInt16 } },
                new JsonObject { { "Name", "myUInt32" }, { "Index", 21 }, { "Obj", myUInt32 } },
                new JsonObject { { "Name", "myUInt32" }, { "Index", 21 }, { "Obj", myUInt32 } },
                new JsonObject { { "Name", "myUInt32" }, { "Index", 21 }, { "Obj", myUInt32 } },
                new JsonObject { { "Name", "myUInt32" }, { "Index", 21 }, { "Obj", myUInt32 } },
                new JsonObject { { "Name", "myUInt64" }, { "Index", 22 }, { "Obj", myUInt64 } }
            };
            return sourceJson;
        }
    }
}
