// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Json
{
    /// <summary>
    /// Functional tests for the JsonObject class.
    /// </summary>
    public class JObjectFunctionalTest
    {
        static int iterationCount = 500;
        static int arrayLength = 10;

        /// <summary>
        /// Validates round-trip of a JsonArray containing both primitives and objects.
        /// </summary>
        [Fact]
        public void MixedJsonTypeFunctionalTest()
        {
            bool oldValue = CreatorSettings.CreateDateTimeWithSubMilliseconds;
            CreatorSettings.CreateDateTimeWithSubMilliseconds = false;
            try
            {
                int seed = 1;

                for (int i = 0; i < iterationCount; i++)
                {
                    seed++;
                    Log.Info("Seed: {0}", seed);
                    Random rndGen = new Random(seed);

                    JsonArray sourceJson = new JsonArray(new List<JsonValue>()
                    {
                        PrimitiveCreator.CreateInstanceOfBoolean(rndGen),
                        PrimitiveCreator.CreateInstanceOfByte(rndGen),
                        PrimitiveCreator.CreateInstanceOfDateTime(rndGen),
                        PrimitiveCreator.CreateInstanceOfDateTimeOffset(rndGen),
                        PrimitiveCreator.CreateInstanceOfDecimal(rndGen),
                        PrimitiveCreator.CreateInstanceOfDouble(rndGen),
                        PrimitiveCreator.CreateInstanceOfInt16(rndGen),
                        PrimitiveCreator.CreateInstanceOfInt32(rndGen),
                        PrimitiveCreator.CreateInstanceOfInt64(rndGen),
                        PrimitiveCreator.CreateInstanceOfSByte(rndGen),
                        PrimitiveCreator.CreateInstanceOfSingle(rndGen),
                        PrimitiveCreator.CreateInstanceOfString(rndGen),
                        PrimitiveCreator.CreateInstanceOfUInt16(rndGen),
                        PrimitiveCreator.CreateInstanceOfUInt32(rndGen),
                        PrimitiveCreator.CreateInstanceOfUInt64(rndGen),
                        new JsonObject(new Dictionary<string, JsonValue>()
                        {
                            { "Boolean", PrimitiveCreator.CreateInstanceOfBoolean(rndGen) },
                            { "Byte", PrimitiveCreator.CreateInstanceOfByte(rndGen) },
                            { "DateTime", PrimitiveCreator.CreateInstanceOfDateTime(rndGen) },
                            { "DateTimeOffset", PrimitiveCreator.CreateInstanceOfDateTimeOffset(rndGen) },
                            { "Decimal", PrimitiveCreator.CreateInstanceOfDecimal(rndGen) },
                            { "Double", PrimitiveCreator.CreateInstanceOfDouble(rndGen) },
                            { "Int16", PrimitiveCreator.CreateInstanceOfInt16(rndGen) },
                            { "Int32", PrimitiveCreator.CreateInstanceOfInt32(rndGen) },
                            { "Int64", PrimitiveCreator.CreateInstanceOfInt64(rndGen) },
                            { "SByte", PrimitiveCreator.CreateInstanceOfSByte(rndGen) },
                            { "Single", PrimitiveCreator.CreateInstanceOfSingle(rndGen) },
                            { "String", PrimitiveCreator.CreateInstanceOfString(rndGen) },
                            { "UInt16", PrimitiveCreator.CreateInstanceOfUInt16(rndGen) },
                            { "UInt32", PrimitiveCreator.CreateInstanceOfUInt32(rndGen) },
                            { "UInt64", PrimitiveCreator.CreateInstanceOfUInt64(rndGen) }
                        })
                    });

                    JsonArray newJson = (JsonArray)JsonValue.Parse(sourceJson.ToString());
                    Assert.True(JsonValueVerifier.Compare(sourceJson, newJson));
                }
            }
            finally
            {
                CreatorSettings.CreateDateTimeWithSubMilliseconds = oldValue;
            }
        }

        /// <summary>
        /// Tests for the <see cref="System.Json.JsonArray.CopyTo"/> method.
        /// </summary>
        [Fact]
        public void JsonArrayCopytoFunctionalTest()
        {
            int seed = 1;

            for (int i = 0; i < iterationCount / 10; i++)
            {
                seed++;
                Log.Info("Seed: {0}", seed);
                Random rndGen = new Random(seed);

                bool retValue = true;

                JsonArray sourceJson = SpecialJsonValueHelper.CreatePrePopulatedJsonArray(seed, arrayLength);
                JsonValue[] destJson = new JsonValue[arrayLength];
                sourceJson.CopyTo(destJson, 0);

                for (int k = 0; k < destJson.Length; k++)
                {
                    if (destJson[k] != sourceJson[k])
                    {
                        retValue = false;
                    }
                }

                Assert.True(retValue, "[JsonArrayCopytoFunctionalTest] JsonArray.CopyTo() failed to function properly. destJson.GetLength(0) = " + destJson.GetLength(0));
            }
        }

        /// <summary>
        /// Tests for add and remove methods in the <see cref="System.Json.JsonArray"/> class.
        /// </summary>
        [Fact]
        public void JsonArrayAddRemoveFunctionalTest()
        {
            int seed = 1;

            for (int i = 0; i < iterationCount / 10; i++)
            {
                seed++;
                Log.Info("Seed: {0}", seed);
                Random rndGen = new Random(seed);
                bool retValue = true;

                JsonArray sourceJson = SpecialJsonValueHelper.CreatePrePopulatedJsonArray(seed, arrayLength);
                JsonValue[] cloneJson = SpecialJsonValueHelper.CreatePrePopulatedJsonValueArray(seed, 3);

                // JsonArray.AddRange(JsonValue[])
                sourceJson.AddRange(cloneJson);
                if (sourceJson.Count != arrayLength + cloneJson.Length)
                {
                    Log.Info("[JsonArrayAddRemoveFunctionalTest] JsonArray.AddRange(JsonValue[]) failed to function properly.");
                    retValue = false;
                }
                else
                {
                    Log.Info("[JsonArrayAddRemoveFunctionalTest] JsonArray.AddRange(JsonValue[]) passed test.");
                }

                // JsonArray.RemoveAt(int)
                int count = sourceJson.Count;
                for (int j = 0; j < count; j++)
                {
                    sourceJson.RemoveAt(0);
                }

                if (sourceJson.Count > 0)
                {
                    Log.Info("[JsonArrayAddRemoveFunctionalTest] JsonArray.RemoveAt(int) failed to function properly.");
                    retValue = false;
                }
                else
                {
                    Log.Info("[JsonArrayAddRemoveFunctionalTest] JsonArray.RemoveAt(int) passed test.");
                }

                // JsonArray.JsonType
                if (sourceJson.JsonType != JsonType.Array)
                {
                    Log.Info("[JsonArrayAddRemoveFunctionalTest] JsonArray.JsonType failed to function properly.");
                    retValue = false;
                }

                // JsonArray.Clear()
                sourceJson = SpecialJsonValueHelper.CreatePrePopulatedJsonArray(seed, arrayLength);
                sourceJson.Clear();
                if (sourceJson.Count > 0)
                {
                    Log.Info("[JsonArrayAddRemoveFunctionalTest] JsonArray.Clear() failed to function properly.");
                    retValue = false;
                }
                else
                {
                    Log.Info("[JsonArrayAddRemoveFunctionalTest] JsonArray.Clear() passed test.");
                }

                // JsonArray.AddRange(JsonValue)
                sourceJson = SpecialJsonValueHelper.CreatePrePopulatedJsonArray(seed, arrayLength);

                // adding one additional value to the array
                sourceJson.AddRange(SpecialJsonValueHelper.GetRandomJsonPrimitives(seed));
                if (sourceJson.Count != arrayLength + 1)
                {
                    Log.Info("[JsonArrayAddRemoveFunctionalTest] JsonArray.AddRange(JsonValue) failed to function properly.");
                    retValue = false;
                }
                else
                {
                    Log.Info("[JsonArrayAddRemoveFunctionalTest] JsonArray.AddRange(JsonValue) passed test.");
                }

                // JsonArray.AddRange(IEnumerable<JsonValue> items)
                sourceJson = SpecialJsonValueHelper.CreatePrePopulatedJsonArray(seed, arrayLength);
                MyJsonValueCollection<JsonValue> myCols = new MyJsonValueCollection<JsonValue>();
                myCols.Add(new JsonPrimitive(PrimitiveCreator.CreateInstanceOfUInt32(rndGen)));
                string str;
                do
                {
                    str = PrimitiveCreator.CreateInstanceOfString(rndGen);
                } while (str == null);

                myCols.Add(new JsonPrimitive(str));
                myCols.Add(new JsonPrimitive(PrimitiveCreator.CreateInstanceOfDateTime(rndGen)));

                // adding 3 additional value to the array
                sourceJson.AddRange(myCols);
                if (sourceJson.Count != arrayLength + 3)
                {
                    Log.Info("[JsonArrayAddRemoveFunctionalTest] JsonArray.AddRange(IEnumerable<JsonValue> items) failed to function properly.");
                    retValue = false;
                }
                else
                {
                    Log.Info("[JsonArrayAddRemoveFunctionalTest] JsonArray.AddRange(IEnumerable<JsonValue> items) passed test.");
                }

                // JsonArray[index].set_Item
                sourceJson = SpecialJsonValueHelper.CreatePrePopulatedJsonArray(seed, arrayLength);
                string temp;
                do
                {
                    temp = PrimitiveCreator.CreateInstanceOfString(rndGen);
                } while (temp == null);

                sourceJson[1] = temp;
                if ((string)sourceJson[1] != temp)
                {
                    Log.Info("[JsonArrayAddRemoveFunctionalTest] JsonArray[index].set_Item failed to function properly.");
                    retValue = false;
                }
                else
                {
                    Log.Info("[JsonArrayAddRemoveFunctionalTest] JsonArray[index].set_Item passed test.");
                }

                // JsonArray.Remove(JsonValue)
                count = sourceJson.Count;
                for (int j = 0; j < count; j++)
                {
                    sourceJson.Remove(sourceJson[0]);
                }

                if (sourceJson.Count > 0)
                {
                    Log.Info("[JsonArrayAddRemoveFunctionalTest] JsonArray.Remove(JsonValue) failed to function properly.");
                    retValue = false;
                }

                Assert.True(retValue);
            }
        }

        /// <summary>
        /// Tests for indexers in the <see cref="System.Json.JsonArray"/> class.
        /// </summary>
        [Fact]
        public void JsonArrayItemsFunctionalTest()
        {
            int seed = 1;

            for (int i = 0; i < iterationCount / 10; i++)
            {
                seed++;
                Log.Info("Seed: {0}", seed);
                Random rndGen = new Random(seed);
                bool retValue = true;

                // JsonArray.Contains(JsonValue)
                // JsonArray.IndexOf(JsonValue)
                JsonArray sourceJson = SpecialJsonValueHelper.CreatePrePopulatedJsonArray(seed, arrayLength);
                for (int j = 0; j < sourceJson.Count; j++)
                {
                    if (!sourceJson.Contains(sourceJson[j]))
                    {
                        Log.Info("[JsonArrayItemsFunctionalTest] JsonArray.Contains(JsonValue) failed to function properly.");
                        retValue = false;
                    }
                    else
                    {
                        Log.Info("[JsonArrayItemsFunctionalTest] JsonArray.Contains(JsonValue) passed test.");
                    }

                    if (sourceJson.IndexOf(sourceJson[j]) != j)
                    {
                        Log.Info("[JsonArrayItemsFunctionalTest] JsonArray.IndexOf(JsonValue) failed to function properly.");
                        retValue = false;
                    }
                    else
                    {
                        Log.Info("[JsonArrayItemsFunctionalTest] JsonArray.IndexOf(JsonValue) passed test.");
                    }
                }

                // JsonArray.Insert(int, JsonValue)
                JsonValue newItem = SpecialJsonValueHelper.GetRandomJsonPrimitives(seed);
                sourceJson.Insert(3, newItem);
                if (sourceJson[3] != newItem || sourceJson.Count != arrayLength + 1)
                {
                    Log.Info("[JsonArrayItemsFunctionalTest] JsonArray.Insert(int, JsonValue) failed to function properly.");
                    retValue = false;
                }
                else
                {
                    Log.Info("[JsonArrayItemsFunctionalTest] JsonArray.Insert(int, JsonValue) passed test.");
                }

                Assert.True(retValue);
            }
        }

        /// <summary>
        /// Tests for the CopyTo methods in the <see cref="System.Json.JsonObject"/> class.
        /// </summary>
        [Fact]
        public void JsonObjectCopytoFunctionalTest()
        {
            int seed = 1;

            for (int i = 0; i < iterationCount / 10; i++)
            {
                seed++;
                Log.Info("Seed: {0}", seed);
                Random rndGen = new Random(seed);

                bool retValue = true;

                JsonObject sourceJson = SpecialJsonValueHelper.CreateIndexPopulatedJsonObject(seed, arrayLength);
                KeyValuePair<string, JsonValue>[] destJson = new KeyValuePair<string, JsonValue>[arrayLength];
                if (sourceJson != null && destJson != null)
                {
                    sourceJson.CopyTo(destJson, 0);
                }
                else
                {
                    Log.Info("[JsonObjectCopytoFunctionalTest] sourceJson.ToString() = " + sourceJson.ToString());
                    Log.Info("[JsonObjectCopytoFunctionalTest] destJson.ToString() = " + destJson.ToString());
                    Assert.False(true, "[JsonObjectCopytoFunctionalTest] failed to create the source JsonObject object.");
                    return;
                }

                if (destJson.Length == arrayLength)
                {
                    for (int k = 0; k < destJson.Length; k++)
                    {
                        JsonValue temp;
                        sourceJson.TryGetValue(k.ToString(), out temp);
                        if (!(temp != null && destJson[k].Value == temp))
                        {
                            retValue = false;
                        }
                    }
                }
                else
                {
                    retValue = false;
                }

                Assert.True(retValue, "[JsonObjectCopytoFunctionalTest] JsonObject.CopyTo() failed to function properly. destJson.GetLength(0) = " + destJson.GetLength(0));
            }
        }

        /// <summary>
        /// Tests for the add and remove methods in the <see cref="System.Json.JsonObject"/> class.
        /// </summary>
        [Fact]
        public void JsonObjectAddRemoveFunctionalTest()
        {
            int seed = 1;

            for (int i = 0; i < iterationCount / 10; i++)
            {
                seed++;
                Log.Info("Seed: {0}", seed);
                Random rndGen = new Random(seed);
                bool retValue = true;

                JsonObject sourceJson = SpecialJsonValueHelper.CreateIndexPopulatedJsonObject(seed, arrayLength);

                // JsonObject.JsonType
                if (sourceJson.JsonType != JsonType.Object)
                {
                    Log.Info("[JsonObjectAddRemoveFunctionalTest] JsonArray.JsonType failed to function properly.");
                    retValue = false;
                }

                // JsonObject.Add(KeyValuePair<string, JsonValue> item)
                // JsonObject.Add(string key, JsonValue value)
                // + various numers below so .AddRange() won't try to add an already existing value
                sourceJson.Add(SpecialJsonValueHelper.GetUniqueNonNullInstanceOfString(seed + 3, sourceJson), SpecialJsonValueHelper.GetUniqueValue(seed, sourceJson));
                KeyValuePair<string, JsonValue> kvp;
                int startingSeed = seed + 1;
                do
                {
                    kvp = SpecialJsonValueHelper.CreatePrePopulatedKeyValuePair(startingSeed);
                    startingSeed++;
                }
                while (sourceJson.ContainsKey(kvp.Key));

                sourceJson.Add(kvp);
                do
                {
                    kvp = SpecialJsonValueHelper.CreatePrePopulatedKeyValuePair(startingSeed);
                    startingSeed++;
                }
                while (sourceJson.ContainsKey(kvp.Key));

                sourceJson.Add(kvp);
                if (sourceJson.Count != arrayLength + 3)
                {
                    Log.Info("[JsonObjectAddRemoveFunctionalTest] JsonObject.Add() failed to function properly.");
                    retValue = false;
                }
                else
                {
                    Log.Info("[JsonObjectAddRemoveFunctionalTest] JsonObject.Add() passed test.");
                }

                // JsonObject.Clear()
                sourceJson.Clear();
                if (sourceJson.Count > 0)
                {
                    Log.Info("[JsonObjectAddRemoveFunctionalTest] JsonObject.Clear() failed to function properly.");
                    retValue = false;
                }
                else
                {
                    Log.Info("[JsonObjectAddRemoveFunctionalTest] JsonObject.Clear() passed test.");
                }

                // JsonObject.AddRange(IEnumerable<KeyValuePair<string, JsonValue>> items)
                sourceJson = SpecialJsonValueHelper.CreateIndexPopulatedJsonObject(seed, arrayLength);

                // + various numers below so .AddRange() won't try to add an already existing value
                sourceJson.AddRange(SpecialJsonValueHelper.CreatePrePopulatedListofKeyValuePair(seed + 13 + (arrayLength * 2), 5));
                if (sourceJson.Count != arrayLength + 5)
                {
                    Log.Info("[JsonObjectAddRemoveFunctionalTest] JsonObject.AddRange(IEnumerable<KeyValuePair<string, JsonValue>> items) failed to function properly.");
                    retValue = false;
                }
                else
                {
                    Log.Info("[JsonObjectAddRemoveFunctionalTest] JsonObject.AddRange(IEnumerable<KeyValuePair<string, JsonValue>> items) passed test.");
                }

                // JsonObject.AddRange(params KeyValuePair<string, JsonValue>[] items)
                sourceJson = SpecialJsonValueHelper.CreateIndexPopulatedJsonObject(seed, arrayLength);

                // + various numers below so .AddRange() won't try to add an already existing value
                KeyValuePair<string, JsonValue> item1 = SpecialJsonValueHelper.CreatePrePopulatedKeyValuePair(seed + arrayLength + 41);
                KeyValuePair<string, JsonValue> item2 = SpecialJsonValueHelper.CreatePrePopulatedKeyValuePair(seed + arrayLength + 47);
                KeyValuePair<string, JsonValue> item3 = SpecialJsonValueHelper.CreatePrePopulatedKeyValuePair(seed + arrayLength + 53);
                sourceJson.AddRange(new KeyValuePair<string, JsonValue>[] { item1, item2, item3 });
                if (sourceJson.Count != arrayLength + 3)
                {
                    Log.Info("[JsonObjectAddRemoveFunctionalTest] JsonObject.AddRange(params KeyValuePair<string, JsonValue>[] items) failed to function properly.");
                    retValue = false;
                }
                else
                {
                    Log.Info("[JsonObjectAddRemoveFunctionalTest] JsonObject.AddRange(params KeyValuePair<string, JsonValue>[] items) passed test.");
                }

                sourceJson.Clear();

                // JsonObject.Remove(Key)
                sourceJson = SpecialJsonValueHelper.CreateIndexPopulatedJsonObject(seed, arrayLength);
                int count = sourceJson.Count;
                List<string> keys = new List<string>(sourceJson.Keys);
                foreach (string key in keys)
                {
                    sourceJson.Remove(key);
                }

                if (sourceJson.Count > 0)
                {
                    Log.Info("[JsonObjectAddRemoveFunctionalTest] JsonObject.Remove(Key) failed to function properly.");
                    retValue = false;
                }
                else
                {
                    Log.Info("[JsonObjectAddRemoveFunctionalTest] JsonObject.Remove(Key) passed test.");
                }

                Assert.True(retValue);
            }
        }

        /// <summary>
        /// Tests for the indexers in the <see cref="System.Json.JsonObject"/> class.
        /// </summary>
        [Fact]
        public void JsonObjectItemsFunctionalTest()
        {
            int seed = 1;

            for (int i = 0; i < iterationCount / 10; i++)
            {
                seed++;
                Log.Info("Seed: {0}", seed);
                Random rndGen = new Random(seed);
                bool retValue = true;

                JsonObject sourceJson = SpecialJsonValueHelper.CreateIndexPopulatedJsonObject(seed, arrayLength);

                // JsonObject[key].set_Item
                sourceJson["1"] = new JsonPrimitive(true);
                if (sourceJson["1"].ToString() != "true")
                {
                    Log.Info("[JsonObjectItemsFunctionalTest] JsonObject[key].set_Item failed to function properly.");
                    retValue = false;
                }
                else
                {
                    Log.Info("[JsonObjectItemsFunctionalTest] JsonObject[key].set_Item passed test.");
                }

                // ICollection<KeyValuePair<string, JsonValue>>.Contains(KeyValuePair<string, JsonValue> item)
                KeyValuePair<string, System.Json.JsonValue> kp = new KeyValuePair<string, JsonValue>("5", sourceJson["5"]);
                if (!((ICollection<KeyValuePair<string, JsonValue>>)sourceJson).Contains(kp))
                {
                    Log.Info("[JsonObjectItemsFunctionalTest] ICollection<KeyValuePair<string, JsonValue>>.Contains(KeyValuePair<string, JsonValue> item) failed to function properly.");
                    retValue = false;
                }
                else
                {
                    Log.Info("[JsonObjectItemsFunctionalTest] ICollection<KeyValuePair<string, JsonValue>>.Contains(KeyValuePair<string, JsonValue> item) passed test.");
                }

                // ICollection<KeyValuePair<string, JsonValue>>.IsReadOnly
                if (((ICollection<KeyValuePair<string, JsonValue>>)sourceJson).IsReadOnly)
                {
                    Log.Info("[JsonObjectItemsFunctionalTest] ICollection<KeyValuePair<string, JsonValue>>.IsReadOnly failed to function properly.");
                    retValue = false;
                }
                else
                {
                    Log.Info("[JsonObjectItemsFunctionalTest] ICollection<KeyValuePair<string, JsonValue>>.IsReadOnly passed test.");
                }

                // ICollection<KeyValuePair<string, JsonValue>>.Add(KeyValuePair<string, JsonValue> item)
                kp = new KeyValuePair<string, JsonValue>("100", new JsonPrimitive(100));
                ((ICollection<KeyValuePair<string, JsonValue>>)sourceJson).Add(kp);
                if (sourceJson.Count != arrayLength + 1)
                {
                    Log.Info("[JsonObjectItemsFunctionalTest] ICollection<KeyValuePair<string, JsonValue>>.Add(KeyValuePair<string, JsonValue> item) failed to function properly.");
                    retValue = false;
                }
                else
                {
                    Log.Info("[JsonObjectItemsFunctionalTest] ICollection<KeyValuePair<string, JsonValue>>.Add(KeyValuePair<string, JsonValue> item) passed test.");
                }

                // ICollection<KeyValuePair<string, JsonValue>>.Remove(KeyValuePair<string, JsonValue> item)
                ((ICollection<KeyValuePair<string, JsonValue>>)sourceJson).Remove(kp);
                if (sourceJson.Count != arrayLength)
                {
                    Log.Info("[JsonObjectItemsFunctionalTest] ICollection<KeyValuePair<string, JsonValue>>.Remove(KeyValuePair<string, JsonValue> item) failed to function properly.");
                    retValue = false;
                }
                else
                {
                    Log.Info("[JsonObjectItemsFunctionalTest] ICollection<KeyValuePair<string, JsonValue>>.Remove(KeyValuePair<string, JsonValue> item) passed test.");
                }

                // ICollection<KeyValuePair<string, JsonValue>>.GetEnumerator()
                JsonObject jo = new JsonObject { { "member 1", 123 }, { "member 2", new JsonArray { 1, 2, 3 } } };
                List<string> expected = new List<string> { "member 1 - 123", "member 2 - [1,2,3]" };
                expected.Sort();
                IEnumerator<KeyValuePair<string, JsonValue>> ko = ((ICollection<KeyValuePair<string, JsonValue>>)jo).GetEnumerator();
                List<string> actual = new List<string>();
                ko.Reset();
                ko.MoveNext();
                do
                {
                    actual.Add(String.Format("{0} - {1}", ko.Current.Key, ko.Current.Value.ToString()));
                    Log.Info("added one item: {0}", String.Format("{0} - {1}", ko.Current.Key, ko.Current.Value));
                    ko.MoveNext();
                }
                while (ko.Current.Value != null);

                actual.Sort();
                if (!JsonValueVerifier.CompareStringLists(expected, actual))
                {
                    Log.Info("[JsonObjectItemsFunctionalTest] ICollection<KeyValuePair<string, JsonValue>>.GetEnumerator() failed to function properly.");
                    retValue = false;
                }
                else
                {
                    Log.Info("[JsonObjectItemsFunctionalTest] ICollection<KeyValuePair<string, JsonValue>>.GetEnumerator() passed test.");
                }

                // JsonObject.Values
                sourceJson = SpecialJsonValueHelper.CreateIndexPopulatedJsonObject(seed, arrayLength);
                JsonValue[] manyValues = SpecialJsonValueHelper.CreatePrePopulatedJsonValueArray(seed, arrayLength);
                JsonObject jov = new JsonObject();
                for (int j = 0; j < manyValues.Length; j++)
                {
                    jov.Add("member" + j, manyValues[j]);
                }

                List<string> expectedList = new List<string>();
                foreach (JsonValue v in manyValues)
                {
                    expectedList.Add(v.ToString());
                }

                expectedList.Sort();
                List<string> actualList = new List<string>();
                foreach (JsonValue v in jov.Values)
                {
                    actualList.Add(v.ToString());
                }

                actualList.Sort();
                if (!JsonValueVerifier.CompareStringLists(expectedList, actualList))
                {
                    Log.Info("[JsonObjectItemsFunctionalTest] JsonObject.Values failed to function properly.");
                    retValue = false;
                }
                else
                {
                    Log.Info("[JsonObjectItemsFunctionalTest] JsonObject.Values passed test.");
                }

                for (int j = 0; j < sourceJson.Count; j++)
                {
                    // JsonObject.Contains(Key)
                    if (!sourceJson.ContainsKey(j.ToString()))
                    {
                        Log.Info("[JsonObjectItemsFunctionalTest] JsonObject.Contains(Key) failed to function properly.");
                        retValue = false;
                    }
                    else
                    {
                        Log.Info("[JsonObjectItemsFunctionalTest] JsonObject.Contains(Key) passed test.");
                    }

                    // JsonObject.TryGetValue(String, out JsonValue)
                    JsonValue retJson;
                    if (!sourceJson.TryGetValue(j.ToString(), out retJson))
                    {
                        Log.Info("[JsonObjectItemsFunctionalTest] JsonObject.TryGetValue(String, out JsonValue) failed to function properly.");
                        retValue = false;
                    }
                    else if (retJson != sourceJson[j.ToString()])
                    {
                        // JsonObjectthis[string key]
                        Log.Info("[JsonObjectItemsFunctionalTest] JsonObject[string key] or JsonObject.TryGetValue(String, out JsonValue) failed to function properly.");
                        retValue = false;
                    }
                    else
                    {
                        Log.Info("[JsonObjectItemsFunctionalTest] JsonObject.TryGetValue(String, out JsonValue) & JsonObject[string key] passed test.");
                    }
                }

                Assert.True(retValue);
            }
        }

        /// <summary>
        /// Tests for casting to integer values.
        /// </summary>
        [Fact]
        public void GettingIntegerValueTest()
        {
            string json = "{\"byte\":160,\"sbyte\":-89,\"short\":12345,\"ushort\":65530," +
                "\"int\":1234567890,\"uint\":3000000000,\"long\":1234567890123456," +
                "\"ulong\":10000000000000000000}";
            Dictionary<string, object> expected = new Dictionary<string, object>();
            expected.Add("byte", (byte)160);
            expected.Add("sbyte", (sbyte)-89);
            expected.Add("short", (short)12345);
            expected.Add("ushort", (ushort)65530);
            expected.Add("int", (int)1234567890);
            expected.Add("uint", (uint)3000000000);
            expected.Add("long", (long)1234567890123456L);
            expected.Add("ulong", (((ulong)5000000000000000000L) * 2));
            JsonObject jo = (JsonObject)JsonValue.Parse(json);
            bool success = true;
            foreach (string key in jo.Keys)
            {
                object expectedObj = expected[key];
                Log.Info("Testing for type = {0}", key);
                try
                {
                    switch (key)
                    {
                        case "byte":
                            Assert.Equal<byte>((byte)expectedObj, (byte)jo[key]);
                            break;
                        case "sbyte":
                            Assert.Equal<sbyte>((sbyte)expectedObj, (sbyte)jo[key]);
                            break;
                        case "short":
                            Assert.Equal<short>((short)expectedObj, (short)jo[key]);
                            break;
                        case "ushort":
                            Assert.Equal<ushort>((ushort)expectedObj, (ushort)jo[key]);
                            break;
                        case "int":
                            Assert.Equal<int>((int)expectedObj, (int)jo[key]);
                            break;
                        case "uint":
                            Assert.Equal<uint>((uint)expectedObj, (uint)jo[key]);
                            break;
                        case "long":
                            Assert.Equal<long>((long)expectedObj, (long)jo[key]);
                            break;
                        case "ulong":
                            Assert.Equal<ulong>((ulong)expectedObj, (ulong)jo[key]);
                            break;
                    }
                }
                catch (InvalidCastException e)
                {
                    Log.Info("Caught InvalidCastException: {0}", e);
                    success = false;
                }
            }

            Assert.True(success);
        }

        /// <summary>
        /// Tests for casting to floating point values.
        /// </summary>
        [Fact]
        public void GettingFloatingPointValueTest()
        {
            string json = "{\"float\":1.23,\"double\":1.23e+290,\"decimal\":1234567890.123456789}";
            Dictionary<string, object> expected = new Dictionary<string, object>();
            expected.Add("float", 1.23f);
            expected.Add("double", 1.23e+290);
            expected.Add("decimal", 1234567890.123456789m);
            JsonObject jo = (JsonObject)JsonValue.Parse(json);
            bool success = true;
            foreach (string key in jo.Keys)
            {
                object expectedObj = expected[key];
                Log.Info("Testing for type = {0}", key);
                try
                {
                    switch (key)
                    {
                        case "float":
                            Assert.Equal<float>((float)expectedObj, (float)jo[key]);
                            break;
                        case "double":
                            Assert.Equal<double>((double)expectedObj, (double)jo[key]);
                            break;
                        case "decimal":
                            Assert.Equal<decimal>((decimal)expectedObj, (decimal)jo[key]);
                            break;
                    }
                }
                catch (InvalidCastException e)
                {
                    Log.Info("Caught InvalidCastException: {0}", e);
                    success = false;
                }
            }

            Assert.True(success);
        }

        /// <summary>
        /// Negative tests for invalid operations.
        /// </summary>
        [Fact]
        public void TestInvalidOperations()
        {
            JsonArray ja = new JsonArray { 1, null, "hello" };
            JsonObject jo = new JsonObject
            {
                { "first", 1 },
                { "second", null },
                { "third", "hello" },
            };
            JsonPrimitive jp = new JsonPrimitive("hello");

            Assert.Throws<InvalidOperationException>(() => "jp[\"hello\"] should fail: " + jp["hello"].ToString());

            Assert.Throws<InvalidOperationException>(() => "ja[\"hello\"] should fail: " + ja["hello"].ToString());


            Assert.Throws<InvalidOperationException>(() => jp["hello"] = "This shouldn't happen");


            Assert.Throws<InvalidOperationException>(() => ja["hello"] = "This shouldn't happen");

            Assert.Throws<InvalidOperationException>(() => ("jp[1] should fail: " + jp[1].ToString()));

            Assert.Throws<InvalidOperationException>(() => "jo[0] should fail: " + jo[1].ToString());

            Assert.Throws<InvalidOperationException>(() => jp[0] = "This shouldn't happen");

            Assert.Throws<InvalidOperationException>(() => jo[0] = "This shouldn't happen");

            Assert.Throws<InvalidCastException>(() => "(DateTimeOffset)jp[\"hello\"] should fail: " + (DateTimeOffset)jp);

            Assert.Throws<InvalidCastException>(() => ("(Char)jp[\"hello\"] should fail: " + (char)jp));

            Assert.Throws<InvalidCastException>(() =>
            {
                short jprim = (short)new JsonPrimitive(false);
            });
        }

        /// <summary>
        /// Test for consuming deeply nested object graphs.
        /// </summary>
        [Fact]
        public void TestDeeplyNestedObjectGraph()
        {
            JsonObject jo = new JsonObject();
            JsonObject current = jo;
            StringBuilder builderExpected = new StringBuilder();
            builderExpected.Append('{');
            int depth = 10000;
            for (int i = 0; i < depth; i++)
            {
                JsonObject next = new JsonObject();
                string key = i.ToString(CultureInfo.InvariantCulture);
                builderExpected.AppendFormat("\"{0}\":{{", key);
                current.Add(key, next);
                current = next;
            }

            for (int i = 0; i < depth + 1; i++)
            {
                builderExpected.Append('}');
            }

            Assert.Equal(builderExpected.ToString(), jo.ToString());
        }

        /// <summary>
        /// Test for consuming deeply nested array graphs.
        /// </summary>
        [Fact]
        public void TestDeeplyNestedArrayGraph()
        {
            JsonArray ja = new JsonArray();
            JsonArray current = ja;
            StringBuilder builderExpected = new StringBuilder();
            builderExpected.Append('[');
            int depth = 10000;
            for (int i = 0; i < depth; i++)
            {
                JsonArray next = new JsonArray();
                builderExpected.Append('[');
                current.Add(next);
                current = next;
            }

            for (int i = 0; i < depth + 1; i++)
            {
                builderExpected.Append(']');
            }

            Assert.Equal(builderExpected.ToString(), ja.ToString());
        }

        /// <summary>
        /// Test for consuming deeply nested object and array graphs.
        /// </summary>
        [Fact]
        public void TestDeeplyNestedObjectAndArrayGraph()
        {
            JsonObject jo = new JsonObject();
            JsonObject current = jo;
            StringBuilder builderExpected = new StringBuilder();
            builderExpected.Append('{');
            int depth = 10000;
            for (int i = 0; i < depth; i++)
            {
                JsonObject next = new JsonObject();
                string key = i.ToString(CultureInfo.InvariantCulture);
                builderExpected.AppendFormat("\"{0}\":[{{", key);
                current.Add(key, new JsonArray(next));
                current = next;
            }

            for (int i = 0; i < depth; i++)
            {
                builderExpected.Append("}]");
            }

            builderExpected.Append('}');

            Assert.Equal(builderExpected.ToString(), jo.ToString());
        }

        /// <summary>
        /// Test for calling <see cref="JsonValue.ToString()"/> on the same instance in different threads.
        /// </summary>
        [Fact]
        public void TestConcurrentToString()
        {
            bool exceptionThrown = false;
            bool incorrectValue = false;
            JsonObject jo = new JsonObject();
            StringBuilder sb = new StringBuilder();
            sb.Append('{');
            for (int i = 0; i < 100000; i++)
            {
                if (i > 0)
                {
                    sb.Append(',');
                }

                string key = i.ToString(CultureInfo.InvariantCulture);
                jo.Add(key, i);
                sb.AppendFormat("\"{0}\":{0}", key);
            }

            sb.Append('}');
            string expected = sb.ToString();

            int numberOfThreads = 5;
            Thread[] threads = new Thread[numberOfThreads];
            for (int i = 0; i < numberOfThreads; i++)
            {
                threads[i] = new Thread(new ThreadStart(delegate
                {
                    for (int j = 0; j < 10; j++)
                    {
                        try
                        {
                            string str = jo.ToString();
                            if (str != expected)
                            {
                                incorrectValue = true;
                                Log.Info("Value is incorrect");
                            }
                        }
                        catch (Exception e)
                        {
                            exceptionThrown = true;
                            Log.Info("Exception thrown: {0}", e);
                        }
                    }
                }));
            }

            for (int i = 0; i < numberOfThreads; i++)
            {
                threads[i].Start();
            }

            for (int i = 0; i < numberOfThreads; i++)
            {
                threads[i].Join();
            }

            Assert.False(incorrectValue);
            Assert.False(exceptionThrown);
        }

        class MyJsonValueCollection<JsonValue> : System.Collections.Generic.IEnumerable<JsonValue>
        {
            List<JsonValue> internalList = new List<JsonValue>();

            public MyJsonValueCollection()
            {
            }

            public void Add(JsonValue obj)
            {
                this.internalList.Add(obj);
            }

            public IEnumerator<JsonValue> GetEnumerator()
            {
                return this.internalList.GetEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }
        }
    }
}