// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using Xunit;

namespace System.Json
{
    public class JsonValueTest
    {
        const string IndexerNotSupportedOnJsonType = "'{0}' type indexer is not supported on JsonValue of 'JsonType.{1}' type.";
        const string InvalidIndexType = "Invalid '{0}' index type; only 'System.String' and non-negative 'System.Int32' types are supported.\r\nParameter name: indexes";

        [Fact]
        public void ContainsKeyTest()
        {
            JsonObject target = new JsonObject { { AnyInstance.AnyString, AnyInstance.AnyString } };
            Assert.True(target.ContainsKey(AnyInstance.AnyString));
        }

        [Fact]
        public void LoadTest()
        {
            string json = "{\"a\":123,\"b\":[false,null,12.34]}";
            foreach (bool useLoadTextReader in new bool[] { false, true })
            {
                JsonValue jv;
                if (useLoadTextReader)
                {
                    using (StringReader sr = new StringReader(json))
                    {
                        jv = JsonValue.Load(sr);
                    }
                }
                else
                {
                    using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(json)))
                    {
                        jv = JsonValue.Load(ms);
                    }
                }

                Assert.Equal(json, jv.ToString());
            }

            ExceptionHelper.Throws<ArgumentNullException>(() => JsonValue.Load((Stream)null));
            ExceptionHelper.Throws<ArgumentNullException>(() => JsonValue.Load((TextReader)null));
        }

        [Fact]
        public void ParseTest()
        {
            JsonValue target;
            string indentedJson = "{\r\n  \"a\": 123,\r\n  \"b\": [\r\n    false,\r\n    null,\r\n    12.34\r\n  ],\r\n  \"with space\": \"hello\",\r\n  \"\": \"empty key\",\r\n  \"withTypeHint\": {\r\n    \"__type\": \"typeHint\"\r\n  }\r\n}";
            string plainJson = indentedJson.Replace("\r\n", "").Replace(" ", "").Replace("emptykey", "empty key").Replace("withspace", "with space");

            target = JsonValue.Parse(indentedJson);
            Assert.Equal(plainJson, target.ToString());

            target = JsonValue.Parse(plainJson);
            Assert.Equal(plainJson, target.ToString());

            ExceptionHelper.Throws<ArgumentNullException>(() => JsonValue.Parse(null));
            ExceptionHelper.Throws<ArgumentException>(() => JsonValue.Parse(""));
        }

        [Fact]
        public void ParseNumbersTest()
        {
            string json = "{\"long\":12345678901234,\"zero\":0.0,\"double\":1.23e+200}";
            string expectedJson = "{\"long\":12345678901234,\"zero\":0,\"double\":1.23E+200}";
            JsonValue jv = JsonValue.Parse(json);

            Assert.Equal(expectedJson, jv.ToString());
            Assert.Equal(12345678901234L, (long)jv["long"]);
            Assert.Equal<double>(0, jv["zero"].ReadAs<double>());
            Assert.Equal<double>(1.23e200, jv["double"].ReadAs<double>());

            ExceptionHelper.Throws<ArgumentException>(() => JsonValue.Parse("[1.2e+400]"));
        }

        [Fact]
        public void ReadAsTest()
        {
            JsonValue target = new JsonPrimitive(AnyInstance.AnyInt);
            Assert.Equal(AnyInstance.AnyInt.ToString(CultureInfo.InvariantCulture), target.ReadAs(typeof(string)));
            Assert.Equal(AnyInstance.AnyInt.ToString(CultureInfo.InvariantCulture), target.ReadAs<string>());
            object value;
            double dblValue;
            Assert.True(target.TryReadAs(typeof(double), out value));
            Assert.True(target.TryReadAs<double>(out dblValue));
            Assert.Equal(Convert.ToDouble(AnyInstance.AnyInt, CultureInfo.InvariantCulture), (double)value);
            Assert.Equal(Convert.ToDouble(AnyInstance.AnyInt, CultureInfo.InvariantCulture), dblValue);
            Assert.False(target.TryReadAs(typeof(Guid), out value), "TryReadAs should have failed to read a double as a Guid");
            Assert.Null(value);
        }

        [Fact(Skip = "See bug #228569 in CSDMain")]
        public void SaveTest()
        {
            JsonObject jo = new JsonObject
            {
                { "first", 1 },
                { "second", 2 },
            };
            JsonValue jv = new JsonArray(123, null, jo);
            string indentedJson = "[\r\n  123,\r\n  null,\r\n  {\r\n    \"first\": 1,\r\n    \"second\": 2\r\n  }\r\n]";
            string plainJson = indentedJson.Replace("\r\n", "").Replace(" ", "");

            SaveJsonValue(jv, plainJson, false);
            SaveJsonValue(jv, plainJson, true);

            JsonValue target = AnyInstance.DefaultJsonValue;
            using (MemoryStream ms = new MemoryStream())
            {
                ExceptionHelper.Throws<InvalidOperationException>(() => target.Save(ms));
            }
        }

        private static void SaveJsonValue(JsonValue jv, string expectedJson, bool useStream)
        {
            string json;
            if (useStream)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    jv.Save(ms);
                    json = Encoding.UTF8.GetString(ms.ToArray());
                }
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                using (TextWriter writer = new StringWriter(sb))
                {
                    jv.Save(writer);
                    json = sb.ToString();
                }
            }

            Assert.Equal(expectedJson, json);
        }

        [Fact]
        public void GetEnumeratorTest()
        {
            IEnumerable target = new JsonArray(AnyInstance.AnyGuid);
            IEnumerator enumerator = target.GetEnumerator();
            Assert.True(enumerator.MoveNext());
            Assert.Equal(AnyInstance.AnyGuid, (Guid)(JsonValue)enumerator.Current);
            Assert.False(enumerator.MoveNext());

            target = new JsonObject();
            enumerator = target.GetEnumerator();
            Assert.False(enumerator.MoveNext());
        }

        [Fact]
        public void IEnumerableTest()
        {
            JsonValue target = AnyInstance.AnyJsonArray;

            // Test IEnumerable<JsonValue> on JsonArray
            int count = 0;

            foreach (JsonValue value in ((JsonArray)target))
            {
                Assert.Same(target[count], value);
                count++;
            }

            Assert.Equal<int>(target.Count, count);

            // Test IEnumerable<KeyValuePair<string, JsonValue>> on JsonValue
            count = 0;
            foreach (KeyValuePair<string, JsonValue> pair in target)
            {
                int index = Int32.Parse(pair.Key);
                Assert.Equal(count, index);
                Assert.Same(target[index], pair.Value);
                count++;
            }
            Assert.Equal<int>(target.Count, count);

            target = AnyInstance.AnyJsonObject;
            count = 0;
            foreach (KeyValuePair<string, JsonValue> pair in target)
            {
                count++;
                Assert.Same(AnyInstance.AnyJsonObject[pair.Key], pair.Value);
            }
            Assert.Equal<int>(AnyInstance.AnyJsonObject.Count, count);
        }

        [Fact]
        public void GetJsonPrimitiveEnumeratorTest()
        {
            JsonValue target = AnyInstance.AnyJsonPrimitive;
            IEnumerator<KeyValuePair<string, JsonValue>> enumerator = target.GetEnumerator();
            Assert.False(enumerator.MoveNext());
        }

        [Fact]
        public void GetJsonUndefinedEnumeratorTest()
        {
            JsonValue target = AnyInstance.AnyJsonPrimitive.AsDynamic().IDontExist;
            IEnumerator<KeyValuePair<string, JsonValue>> enumerator = target.GetEnumerator();
            Assert.False(enumerator.MoveNext());
        }

        [Fact]
        public void ToStringTest()
        {
            JsonObject jo = new JsonObject
            {
                { "first", 1 },
                { "second", 2 },
                { "third", new JsonObject { { "inner_one", 4 }, { "", null }, { "inner_3", "" } } },
                { "fourth", new JsonArray { "Item1", 2, false } },
                { "fifth", null }
            };
            JsonValue jv = new JsonArray(123, null, jo);
            string expectedJson = "[\r\n  123,\r\n  null,\r\n  {\r\n    \"first\": 1,\r\n    \"second\": 2,\r\n    \"third\": {\r\n      \"inner_one\": 4,\r\n      \"\": null,\r\n      \"inner_3\": \"\"\r\n    },\r\n    \"fourth\": [\r\n      \"Item1\",\r\n      2,\r\n      false\r\n    ],\r\n    \"fifth\": null\r\n  }\r\n]";
            Assert.Equal<string>(expectedJson.Replace("\r\n", "").Replace(" ", ""), jv.ToString());
        }

        [Fact]
        public void CastTests()
        {
            int value = 10;
            JsonValue target = new JsonPrimitive(value);

            int v1 = JsonValue.CastValue<int>(target);
            Assert.Equal<int>(value, v1);
            v1 = (int)target;
            Assert.Equal<int>(value, v1);

            long v2 = JsonValue.CastValue<long>(target);
            Assert.Equal<long>(value, v2);
            v2 = (long)target;
            Assert.Equal<long>(value, v2);

            string s = JsonValue.CastValue<string>(target);
            Assert.Equal<string>(value.ToString(), s);
            s = (string)target;
            Assert.Equal<string>(value.ToString(), s);

            object obj = JsonValue.CastValue<object>(target);
            Assert.Equal(target, obj);
            obj = (object)target;
            Assert.Equal(target, obj);

            object nill = JsonValue.CastValue<object>(null);
            Assert.Null(nill);

            dynamic dyn = target;
            JsonValue defaultJv = dyn.IamDefault;
            nill = JsonValue.CastValue<string>(defaultJv);
            Assert.Null(nill);
            nill = (string)defaultJv;
            Assert.Null(nill);

            obj = JsonValue.CastValue<object>(defaultJv);
            Assert.Same(defaultJv, obj);
            obj = (object)defaultJv;
            Assert.Same(defaultJv, obj);

            JsonValue jv = JsonValue.CastValue<JsonValue>(target);
            Assert.Equal<JsonValue>(target, jv);

            jv = JsonValue.CastValue<JsonValue>(defaultJv);
            Assert.Equal<JsonValue>(defaultJv, jv);

            jv = JsonValue.CastValue<JsonPrimitive>(target);
            Assert.Equal<JsonValue>(target, jv);

            ExceptionHelper.Throws<InvalidCastException>(delegate { int i = JsonValue.CastValue<int>(null); });
            ExceptionHelper.Throws<InvalidCastException>(delegate { int i = JsonValue.CastValue<int>(defaultJv); });
            ExceptionHelper.Throws<InvalidCastException>(delegate { int i = JsonValue.CastValue<char>(target); });
        }

        [Fact]
        public void CastingTests()
        {
            JsonValue target = new JsonPrimitive(AnyInstance.AnyInt);

            Assert.Equal(AnyInstance.AnyInt.ToString(CultureInfo.InvariantCulture), (string)target);
            Assert.Equal(Convert.ToDouble(AnyInstance.AnyInt, CultureInfo.InvariantCulture), (double)target);

            Assert.Equal(AnyInstance.AnyString, (string)(JsonValue)AnyInstance.AnyString);
            Assert.Equal(AnyInstance.AnyChar, (char)(JsonValue)AnyInstance.AnyChar);
            Assert.Equal(AnyInstance.AnyUri, (Uri)(JsonValue)AnyInstance.AnyUri);
            Assert.Equal(AnyInstance.AnyGuid, (Guid)(JsonValue)AnyInstance.AnyGuid);
            Assert.Equal(AnyInstance.AnyDateTime, (DateTime)(JsonValue)AnyInstance.AnyDateTime);
            Assert.Equal(AnyInstance.AnyDateTimeOffset, (DateTimeOffset)(JsonValue)AnyInstance.AnyDateTimeOffset);
            Assert.Equal(AnyInstance.AnyBool, (bool)(JsonValue)AnyInstance.AnyBool);
            Assert.Equal(AnyInstance.AnyByte, (byte)(JsonValue)AnyInstance.AnyByte);
            Assert.Equal(AnyInstance.AnyShort, (short)(JsonValue)AnyInstance.AnyShort);
            Assert.Equal(AnyInstance.AnyInt, (int)(JsonValue)AnyInstance.AnyInt);
            Assert.Equal(AnyInstance.AnyLong, (long)(JsonValue)AnyInstance.AnyLong);
            Assert.Equal(AnyInstance.AnySByte, (sbyte)(JsonValue)AnyInstance.AnySByte);
            Assert.Equal(AnyInstance.AnyUShort, (ushort)(JsonValue)AnyInstance.AnyUShort);
            Assert.Equal(AnyInstance.AnyUInt, (uint)(JsonValue)AnyInstance.AnyUInt);
            Assert.Equal(AnyInstance.AnyULong, (ulong)(JsonValue)AnyInstance.AnyULong);
            Assert.Equal(AnyInstance.AnyDecimal, (decimal)(JsonValue)AnyInstance.AnyDecimal);
            Assert.Equal(AnyInstance.AnyFloat, (float)(JsonValue)AnyInstance.AnyFloat);
            Assert.Equal(AnyInstance.AnyDouble, (double)(JsonValue)AnyInstance.AnyDouble);

            Uri uri = null;
            string str = null;

            JsonValue jv = uri;
            Assert.Null(jv);
            uri = (Uri)jv;
            Assert.Null(uri);

            jv = str;
            Assert.Null(jv);
            str = (string)jv;
            Assert.Null(str);

            ExceptionHelper.Throws<InvalidCastException>(delegate { var s = (string)AnyInstance.AnyJsonArray; });
            ExceptionHelper.Throws<InvalidCastException>(delegate { var s = (string)AnyInstance.AnyJsonObject; });
        }

        [Fact]
        public void InvalidCastTest()
        {
            JsonValue nullValue = (JsonValue)null;
            JsonValue strValue = new JsonPrimitive(AnyInstance.AnyString);
            JsonValue boolValue = new JsonPrimitive(AnyInstance.AnyBool);
            JsonValue intValue = new JsonPrimitive(AnyInstance.AnyInt);

            ExceptionHelper.Throws<InvalidCastException>(delegate { var v = (double)nullValue; });
            ExceptionHelper.Throws<InvalidCastException>(delegate { var v = (double)strValue; });
            ExceptionHelper.Throws<InvalidCastException>(delegate { var v = (double)boolValue; });
            Assert.Equal<double>(AnyInstance.AnyInt, (double)intValue);

            ExceptionHelper.Throws<InvalidCastException>(delegate { var v = (float)nullValue; });
            ExceptionHelper.Throws<InvalidCastException>(delegate { var v = (float)strValue; });
            ExceptionHelper.Throws<InvalidCastException>(delegate { var v = (float)boolValue; });
            Assert.Equal<float>(AnyInstance.AnyInt, (float)intValue);

            ExceptionHelper.Throws<InvalidCastException>(delegate { var v = (decimal)nullValue; });
            ExceptionHelper.Throws<InvalidCastException>(delegate { var v = (decimal)strValue; });
            ExceptionHelper.Throws<InvalidCastException>(delegate { var v = (decimal)boolValue; });
            Assert.Equal<decimal>(AnyInstance.AnyInt, (decimal)intValue);

            ExceptionHelper.Throws<InvalidCastException>(delegate { var v = (long)nullValue; });
            ExceptionHelper.Throws<InvalidCastException>(delegate { var v = (long)strValue; });
            ExceptionHelper.Throws<InvalidCastException>(delegate { var v = (long)boolValue; });
            Assert.Equal<long>(AnyInstance.AnyInt, (long)intValue);

            ExceptionHelper.Throws<InvalidCastException>(delegate { var v = (ulong)nullValue; });
            ExceptionHelper.Throws<InvalidCastException>(delegate { var v = (ulong)strValue; });
            ExceptionHelper.Throws<InvalidCastException>(delegate { var v = (ulong)boolValue; });
            Assert.Equal<ulong>(AnyInstance.AnyInt, (ulong)intValue);

            ExceptionHelper.Throws<InvalidCastException>(delegate { var v = (int)nullValue; });
            ExceptionHelper.Throws<InvalidCastException>(delegate { var v = (int)strValue; });
            ExceptionHelper.Throws<InvalidCastException>(delegate { var v = (int)boolValue; });
            Assert.Equal<int>(AnyInstance.AnyInt, (int)intValue);

            ExceptionHelper.Throws<InvalidCastException>(delegate { var v = (uint)nullValue; });
            ExceptionHelper.Throws<InvalidCastException>(delegate { var v = (uint)strValue; });
            ExceptionHelper.Throws<InvalidCastException>(delegate { var v = (uint)boolValue; });
            Assert.Equal<uint>(AnyInstance.AnyInt, (uint)intValue);

            ExceptionHelper.Throws<InvalidCastException>(delegate { var v = (short)nullValue; });
            ExceptionHelper.Throws<InvalidCastException>(delegate { var v = (short)strValue; });
            ExceptionHelper.Throws<InvalidCastException>(delegate { var v = (short)boolValue; });

            ExceptionHelper.Throws<InvalidCastException>(delegate { var v = (ushort)nullValue; });
            ExceptionHelper.Throws<InvalidCastException>(delegate { var v = (ushort)strValue; });
            ExceptionHelper.Throws<InvalidCastException>(delegate { var v = (ushort)boolValue; });

            ExceptionHelper.Throws<InvalidCastException>(delegate { var v = (sbyte)nullValue; });
            ExceptionHelper.Throws<InvalidCastException>(delegate { var v = (sbyte)strValue; });
            ExceptionHelper.Throws<InvalidCastException>(delegate { var v = (sbyte)boolValue; });

            ExceptionHelper.Throws<InvalidCastException>(delegate { var v = (byte)nullValue; });
            ExceptionHelper.Throws<InvalidCastException>(delegate { var v = (byte)strValue; });
            ExceptionHelper.Throws<InvalidCastException>(delegate { var v = (byte)boolValue; });

            ExceptionHelper.Throws<InvalidCastException>(delegate { var v = (Guid)nullValue; });
            ExceptionHelper.Throws<InvalidCastException>(delegate { var v = (Guid)strValue; });
            ExceptionHelper.Throws<InvalidCastException>(delegate { var v = (Guid)boolValue; });
            ExceptionHelper.Throws<InvalidCastException>(delegate { var v = (Guid)intValue; });

            ExceptionHelper.Throws<InvalidCastException>(delegate { var v = (DateTime)nullValue; });
            ExceptionHelper.Throws<InvalidCastException>(delegate { var v = (DateTime)strValue; });
            ExceptionHelper.Throws<InvalidCastException>(delegate { var v = (DateTime)boolValue; });
            ExceptionHelper.Throws<InvalidCastException>(delegate { var v = (DateTime)intValue; });

            ExceptionHelper.Throws<InvalidCastException>(delegate { var v = (char)nullValue; });
            ExceptionHelper.Throws<InvalidCastException>(delegate { var v = (char)strValue; });
            ExceptionHelper.Throws<InvalidCastException>(delegate { var v = (char)boolValue; });
            ExceptionHelper.Throws<InvalidCastException>(delegate { var v = (char)intValue; });

            ExceptionHelper.Throws<InvalidCastException>(delegate { var v = (DateTimeOffset)nullValue; });
            ExceptionHelper.Throws<InvalidCastException>(delegate { var v = (DateTimeOffset)strValue; });
            ExceptionHelper.Throws<InvalidCastException>(delegate { var v = (DateTimeOffset)boolValue; });
            ExceptionHelper.Throws<InvalidCastException>(delegate { var v = (DateTimeOffset)intValue; });

            Assert.Null((Uri)nullValue);
            Assert.Equal(((Uri)strValue).ToString(), (string)strValue);
            ExceptionHelper.Throws<InvalidCastException>(delegate { var v = (Uri)boolValue; });
            ExceptionHelper.Throws<InvalidCastException>(delegate { var v = (Uri)intValue; });

            ExceptionHelper.Throws<InvalidCastException>(delegate { var v = (bool)nullValue; });
            ExceptionHelper.Throws<InvalidCastException>(delegate { var v = (bool)strValue; });
            Assert.Equal(AnyInstance.AnyBool, (bool)boolValue);
            ExceptionHelper.Throws<InvalidCastException>(delegate { var v = (bool)intValue; });

            Assert.Equal(null, (string)nullValue);
            Assert.Equal(AnyInstance.AnyString, (string)strValue);
            Assert.Equal(AnyInstance.AnyBool.ToString().ToLowerInvariant(), ((string)boolValue).ToLowerInvariant());
            Assert.Equal(AnyInstance.AnyInt.ToString(CultureInfo.InvariantCulture), (string)intValue);

            ExceptionHelper.Throws<InvalidCastException>(delegate { var v = (int)nullValue; });
            ExceptionHelper.Throws<InvalidCastException>(delegate { var v = (int)strValue; });
            ExceptionHelper.Throws<InvalidCastException>(delegate { var v = (int)boolValue; });
            Assert.Equal(AnyInstance.AnyInt, (int)intValue);
        }

        [Fact]
        public void CountTest()
        {
            JsonArray ja = new JsonArray(1, 2);
            Assert.Equal(2, ja.Count);

            JsonObject jo = new JsonObject
            {
                { "key1", 123 },
                { "key2", null },
                { "key3", "hello" },
            };
            Assert.Equal(3, jo.Count);
        }

        [Fact]
        public void ItemTest()
        {
            //// Positive tests for Item on JsonArray and JsonObject are on JsonArrayTest and JsonObjectTest, respectively.

            JsonValue target;
            target = AnyInstance.AnyJsonPrimitive;
            ExceptionHelper.Throws<InvalidOperationException>(delegate { var c = target[1]; }, String.Format(IndexerNotSupportedOnJsonType, typeof(int), target.JsonType));
            ExceptionHelper.Throws<InvalidOperationException>(delegate { target[0] = 123; }, String.Format(IndexerNotSupportedOnJsonType, typeof(int), target.JsonType));
            ExceptionHelper.Throws<InvalidOperationException>(delegate { var c = target["key"]; }, String.Format(IndexerNotSupportedOnJsonType, typeof(string), target.JsonType));
            ExceptionHelper.Throws<InvalidOperationException>(delegate { target["here"] = 123; }, String.Format(IndexerNotSupportedOnJsonType, typeof(string), target.JsonType));

            target = AnyInstance.AnyJsonObject;
            ExceptionHelper.Throws<InvalidOperationException>(delegate { var c = target[0]; }, String.Format(IndexerNotSupportedOnJsonType, typeof(int), target.JsonType));
            ExceptionHelper.Throws<InvalidOperationException>(delegate { target[0] = 123; }, String.Format(IndexerNotSupportedOnJsonType, typeof(int), target.JsonType));

            target = AnyInstance.AnyJsonArray;
            ExceptionHelper.Throws<InvalidOperationException>(delegate { var c = target["key"]; }, String.Format(IndexerNotSupportedOnJsonType, typeof(string), target.JsonType));
            ExceptionHelper.Throws<InvalidOperationException>(delegate { target["here"] = 123; }, String.Format(IndexerNotSupportedOnJsonType, typeof(string), target.JsonType));
        }

        [Fact(Skip = "Re-enable when DCS have been removed -- see CSDMain 234538")]
        public void NonSerializableTest()
        {
            DataContractJsonSerializer dcjs = new DataContractJsonSerializer(typeof(JsonValue));
            ExceptionHelper.Throws<NotSupportedException>(() => dcjs.WriteObject(Stream.Null, AnyInstance.DefaultJsonValue));
        }

        [Fact]
        public void DefaultConcatTest()
        {
            JsonValue jv = JsonValueExtensions.CreateFrom(AnyInstance.AnyPerson);
            dynamic target = JsonValueExtensions.CreateFrom(AnyInstance.AnyPerson);
            Person person = AnyInstance.AnyPerson;

            Assert.Equal(person.Address.City, target.Address.City.ReadAs<string>());
            Assert.Equal(person.Friends[0].Age, target.Friends[0].Age.ReadAs<int>());

            Assert.Equal(target.ValueOrDefault("Address").ValueOrDefault("City"), target.Address.City);
            Assert.Equal(target.ValueOrDefault("Address", "City"), target.Address.City);

            Assert.Equal(target.ValueOrDefault("Friends").ValueOrDefault(0).ValueOrDefault("Age"), target.Friends[0].Age);
            Assert.Equal(target.ValueOrDefault("Friends", 0, "Age"), target.Friends[0].Age);

            Assert.Equal(JsonType.Default, AnyInstance.AnyJsonValue1.ValueOrDefault((object[])null).JsonType);
            Assert.Equal(JsonType.Default, jv.ValueOrDefault("Friends", null).JsonType);
            Assert.Equal(JsonType.Default, AnyInstance.AnyJsonValue1.ValueOrDefault((string)null).JsonType);
            Assert.Equal(JsonType.Default, AnyInstance.AnyJsonPrimitive.ValueOrDefault(AnyInstance.AnyString, AnyInstance.AnyShort).JsonType);
            Assert.Equal(JsonType.Default, AnyInstance.AnyJsonArray.ValueOrDefault((string)null).JsonType);
            Assert.Equal(JsonType.Default, AnyInstance.AnyJsonObject.ValueOrDefault(AnyInstance.AnyString, null).JsonType);
            Assert.Equal(JsonType.Default, AnyInstance.AnyJsonArray.ValueOrDefault(-1).JsonType);

            Assert.Same(AnyInstance.AnyJsonValue1, AnyInstance.AnyJsonValue1.ValueOrDefault());

            Assert.Same(AnyInstance.AnyJsonArray.ValueOrDefault(0), AnyInstance.AnyJsonArray.ValueOrDefault((short)0));
            Assert.Same(AnyInstance.AnyJsonArray.ValueOrDefault(0), AnyInstance.AnyJsonArray.ValueOrDefault((ushort)0));
            Assert.Same(AnyInstance.AnyJsonArray.ValueOrDefault(0), AnyInstance.AnyJsonArray.ValueOrDefault((byte)0));
            Assert.Same(AnyInstance.AnyJsonArray.ValueOrDefault(0), AnyInstance.AnyJsonArray.ValueOrDefault((sbyte)0));
            Assert.Same(AnyInstance.AnyJsonArray.ValueOrDefault(0), AnyInstance.AnyJsonArray.ValueOrDefault((char)0));

            jv = new JsonObject();
            jv[AnyInstance.AnyString] = AnyInstance.AnyJsonArray;

            Assert.Same(jv.ValueOrDefault(AnyInstance.AnyString, 0), jv.ValueOrDefault(AnyInstance.AnyString, (short)0));
            Assert.Same(jv.ValueOrDefault(AnyInstance.AnyString, 0), jv.ValueOrDefault(AnyInstance.AnyString, (ushort)0));
            Assert.Same(jv.ValueOrDefault(AnyInstance.AnyString, 0), jv.ValueOrDefault(AnyInstance.AnyString, (byte)0));
            Assert.Same(jv.ValueOrDefault(AnyInstance.AnyString, 0), jv.ValueOrDefault(AnyInstance.AnyString, (sbyte)0));
            Assert.Same(jv.ValueOrDefault(AnyInstance.AnyString, 0), jv.ValueOrDefault(AnyInstance.AnyString, (char)0));

            jv = AnyInstance.AnyJsonObject;

            ExceptionHelper.Throws<ArgumentException>(delegate { var c = jv.ValueOrDefault(AnyInstance.AnyString, AnyInstance.AnyLong); }, String.Format(InvalidIndexType, typeof(long)));
            ExceptionHelper.Throws<ArgumentException>(delegate { var c = jv.ValueOrDefault(AnyInstance.AnyString, AnyInstance.AnyUInt); }, String.Format(InvalidIndexType, typeof(uint)));
            ExceptionHelper.Throws<ArgumentException>(delegate { var c = jv.ValueOrDefault(AnyInstance.AnyString, AnyInstance.AnyBool); }, String.Format(InvalidIndexType, typeof(bool)));
        }


        [Fact]
        public void DataContractSerializerTest()
        {
            ValidateSerialization(new JsonPrimitive(DateTime.Now));
            ValidateSerialization(new JsonObject { { "a", 1 }, { "b", 2 }, { "c", 3 } });
            ValidateSerialization(new JsonArray { "a", "b", "c", 1, 2, 3 });

            JsonObject beforeObject = new JsonObject { { "a", 1 }, { "b", 2 }, { "c", 3 } };
            JsonObject afterObject1 = (JsonObject)ValidateSerialization(beforeObject);
            beforeObject.Add("d", 4);
            afterObject1.Add("d", 4);
            Assert.Equal(beforeObject.ToString(), afterObject1.ToString());

            JsonObject afterObject2 = (JsonObject)ValidateSerialization(beforeObject);
            beforeObject.Add("e", 5);
            afterObject2.Add("e", 5);
            Assert.Equal(beforeObject.ToString(), afterObject2.ToString());

            JsonArray beforeArray = new JsonArray { "a", "b", "c" };
            JsonArray afterArray1 = (JsonArray)ValidateSerialization(beforeArray);
            beforeArray.Add("d");
            afterArray1.Add("d");
            Assert.Equal(beforeArray.ToString(), afterArray1.ToString());

            JsonArray afterArray2 = (JsonArray)ValidateSerialization(beforeArray);
            beforeArray.Add("e");
            afterArray2.Add("e");
            Assert.Equal(beforeArray.ToString(), afterArray2.ToString());
        }

        private static JsonValue ValidateSerialization(JsonValue beforeSerialization)
        {
            Assert.NotNull(beforeSerialization);
            NetDataContractSerializer serializer = new NetDataContractSerializer();
            using (MemoryStream memStream = new MemoryStream())
            {
                serializer.Serialize(memStream, beforeSerialization);
                memStream.Position = 0;
                JsonValue afterDeserialization = (JsonValue)serializer.Deserialize(memStream);
                Assert.Equal(beforeSerialization.ToString(), afterDeserialization.ToString());
                return afterDeserialization;
            }
        }
    }
}
