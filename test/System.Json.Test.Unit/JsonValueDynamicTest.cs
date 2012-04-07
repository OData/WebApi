// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Dynamic;
using System.Runtime.Serialization.Json;
using Xunit;

namespace System.Json
{
    public class JsonValueDynamicTest
    {
        const string InvalidIndexType = "Invalid '{0}' index type; only 'System.String' and non-negative 'System.Int32' types are supported.";
        const string NonSingleNonNullIndexNotSupported = "Null index or multidimensional indexing is not supported by this indexer; use 'System.Int32' or 'System.String' for array and object indexing respectively.";

        [Fact]
        public void SettingDifferentValueTypes()
        {
            dynamic dyn = new JsonObject();
            dyn.boolean = AnyInstance.AnyBool;
            dyn.int16 = AnyInstance.AnyShort;
            dyn.int32 = AnyInstance.AnyInt;
            dyn.int64 = AnyInstance.AnyLong;
            dyn.uint16 = AnyInstance.AnyUShort;
            dyn.uint32 = AnyInstance.AnyUInt;
            dyn.uint64 = AnyInstance.AnyULong;
            dyn.@char = AnyInstance.AnyChar;
            dyn.dbl = AnyInstance.AnyDouble;
            dyn.flt = AnyInstance.AnyFloat;
            dyn.dec = AnyInstance.AnyDecimal;
            dyn.str = AnyInstance.AnyString;
            dyn.uri = AnyInstance.AnyUri;
            dyn.@byte = AnyInstance.AnyByte;
            dyn.@sbyte = AnyInstance.AnySByte;
            dyn.guid = AnyInstance.AnyGuid;
            dyn.dateTime = AnyInstance.AnyDateTime;
            dyn.dateTimeOffset = AnyInstance.AnyDateTimeOffset;
            dyn.JsonArray = AnyInstance.AnyJsonArray;
            dyn.JsonPrimitive = AnyInstance.AnyJsonPrimitive;
            dyn.JsonObject = AnyInstance.AnyJsonObject;

            JsonObject jo = (JsonObject)dyn;
            Assert.Equal(AnyInstance.AnyBool, (bool)jo["boolean"]);
            Assert.Equal(AnyInstance.AnyShort, (short)jo["int16"]);
            Assert.Equal(AnyInstance.AnyUShort, (ushort)jo["uint16"]);
            Assert.Equal(AnyInstance.AnyInt, (int)jo["int32"]);
            Assert.Equal(AnyInstance.AnyUInt, (uint)jo["uint32"]);
            Assert.Equal(AnyInstance.AnyLong, (long)jo["int64"]);
            Assert.Equal(AnyInstance.AnyULong, (ulong)jo["uint64"]);
            Assert.Equal(AnyInstance.AnySByte, (sbyte)jo["sbyte"]);
            Assert.Equal(AnyInstance.AnyByte, (byte)jo["byte"]);
            Assert.Equal(AnyInstance.AnyChar, (char)jo["char"]);
            Assert.Equal(AnyInstance.AnyDouble, (double)jo["dbl"]);
            Assert.Equal(AnyInstance.AnyFloat, (float)jo["flt"]);
            Assert.Equal(AnyInstance.AnyDecimal, (decimal)jo["dec"]);
            Assert.Equal(AnyInstance.AnyString, (string)jo["str"]);
            Assert.Equal(AnyInstance.AnyUri, (Uri)jo["uri"]);
            Assert.Equal(AnyInstance.AnyGuid, (Guid)jo["guid"]);
            Assert.Equal(AnyInstance.AnyDateTime, (DateTime)jo["dateTime"]);
            Assert.Equal(AnyInstance.AnyDateTimeOffset, (DateTimeOffset)jo["dateTimeOffset"]);
            Assert.Same(AnyInstance.AnyJsonArray, jo["JsonArray"]);
            Assert.Equal(AnyInstance.AnyJsonPrimitive, jo["JsonPrimitive"]);
            Assert.Same(AnyInstance.AnyJsonObject, jo["JsonObject"]);

            Assert.Equal(AnyInstance.AnyBool, (bool)dyn.boolean);
            Assert.Equal(AnyInstance.AnyShort, (short)dyn.int16);
            Assert.Equal(AnyInstance.AnyUShort, (ushort)dyn.uint16);
            Assert.Equal(AnyInstance.AnyInt, (int)dyn.int32);
            Assert.Equal(AnyInstance.AnyUInt, (uint)dyn.uint32);
            Assert.Equal(AnyInstance.AnyLong, (long)dyn.int64);
            Assert.Equal(AnyInstance.AnyULong, (ulong)dyn.uint64);
            Assert.Equal(AnyInstance.AnySByte, (sbyte)dyn.@sbyte);
            Assert.Equal(AnyInstance.AnyByte, (byte)dyn.@byte);
            Assert.Equal(AnyInstance.AnyChar, (char)dyn.@char);
            Assert.Equal(AnyInstance.AnyDouble, (double)dyn.dbl);
            Assert.Equal(AnyInstance.AnyFloat, (float)dyn.flt);
            Assert.Equal(AnyInstance.AnyDecimal, (decimal)dyn.dec);
            Assert.Equal(AnyInstance.AnyString, (string)dyn.str);
            Assert.Equal(AnyInstance.AnyUri, (Uri)dyn.uri);
            Assert.Equal(AnyInstance.AnyGuid, (Guid)dyn.guid);
            Assert.Equal(AnyInstance.AnyDateTime, (DateTime)dyn.dateTime);
            Assert.Equal(AnyInstance.AnyDateTimeOffset, (DateTimeOffset)dyn.dateTimeOffset);
            Assert.Same(AnyInstance.AnyJsonArray, dyn.JsonArray);
            Assert.Equal(AnyInstance.AnyJsonPrimitive, dyn.JsonPrimitive);
            Assert.Same(AnyInstance.AnyJsonObject, dyn.JsonObject);

            ExceptionHelper.Throws<ArgumentException>(delegate { dyn.other = Console.Out; });
            ExceptionHelper.Throws<ArgumentException>(delegate { dyn.other = dyn.NonExistentProp; });
        }

        [Fact]
        public void NullTests()
        {
            dynamic dyn = new JsonObject();
            JsonObject jo = (JsonObject)dyn;

            dyn.@null = null;
            Assert.Same(dyn.@null, AnyInstance.DefaultJsonValue);

            jo["@null"] = null;
            Assert.Null(jo["@null"]);
        }

        [Fact]
        public void DynamicNotationTest()
        {
            bool boolValue;
            JsonValue jsonValue;

            Person person = Person.CreateSample();
            dynamic jo = JsonValueExtensions.CreateFrom(person);

            dynamic target = jo;
            Assert.Equal<int>(person.Age, target.Age.ReadAs<int>()); // JsonPrimitive
            Assert.Equal<string>(person.Address.ToString(), ((JsonObject)target.Address).ReadAsType<Address>().ToString()); // JsonObject

            target = jo.Address.City;  // JsonPrimitive
            Assert.NotNull(target);
            Assert.Equal<string>(target.ReadAs<string>(), person.Address.City);

            target = jo.Friends;  // JsonArray
            Assert.NotNull(target);
            jsonValue = target as JsonValue;
            Assert.Equal<int>(person.Friends.Count, jsonValue.ReadAsType<List<Person>>().Count);

            target = jo.Friends[1].Address.City;
            Assert.NotNull(target);
            Assert.Equal<string>(target.ReadAs<string>(), person.Address.City);

            target = jo.Address.NonExistentProp.NonExistentProp2; // JsonObject (default)
            Assert.NotNull(target);
            Assert.True(jo is JsonObject);
            Assert.False(target.TryReadAs<bool>(out boolValue));
            Assert.True(target.TryReadAs<JsonValue>(out jsonValue));
            Assert.Same(target, jsonValue);

            Assert.Same(jo.Address.NonExistent, AnyInstance.DefaultJsonValue);
            Assert.Same(jo.Friends[1000], AnyInstance.DefaultJsonValue);
            Assert.Same(jo.Age.NonExistentProp, AnyInstance.DefaultJsonValue);
            Assert.Same(jo.Friends.NonExistentProp, AnyInstance.DefaultJsonValue);
        }

        [Fact]
        public void PropertyAccessTest()
        {
            Person p = AnyInstance.AnyPerson;
            JsonObject jo = JsonValueExtensions.CreateFrom(p) as JsonObject;
            JsonArray ja = JsonValueExtensions.CreateFrom(p.Friends) as JsonArray;
            JsonPrimitive jp = AnyInstance.AnyJsonPrimitive;
            JsonValue jv = AnyInstance.DefaultJsonValue;

            dynamic jod = jo;
            dynamic jad = ja;
            dynamic jpd = jp;
            dynamic jvd = jv;

            Assert.Equal(jo.Count, jod.Count);
            Assert.Equal(jo.JsonType, jod.JsonType);
            Assert.Equal(jo.Keys.Count, jod.Keys.Count);
            Assert.Equal(jo.Values.Count, jod.Values.Count);
            Assert.Equal(p.Age, (int)jod.Age);
            Assert.Equal(p.Age, (int)jod["Age"]);
            Assert.Equal(p.Age, (int)jo["Age"]);
            Assert.Equal(p.Address.City, (string)jo["Address"]["City"]);
            Assert.Equal(p.Address.City, (string)jod["Address"]["City"]);
            Assert.Equal(p.Address.City, (string)jod.Address.City);

            Assert.Equal(p.Friends.Count, ja.Count);
            Assert.Equal(ja.Count, jad.Count);
            Assert.Equal(ja.IsReadOnly, jad.IsReadOnly);
            Assert.Equal(ja.JsonType, jad.JsonType);
            Assert.Equal(p.Friends[0].Age, (int)ja[0]["Age"]);
            Assert.Equal(p.Friends[0].Age, (int)jad[0].Age);

            Assert.Equal(jp.JsonType, jpd.JsonType);
        }

        [Fact]
        public void ConcatDynamicAssignmentTest()
        {
            string value = "MyValue";
            dynamic dynArray = JsonValue.Parse(AnyInstance.AnyJsonArray.ToString());
            dynamic dynObj = JsonValue.Parse(AnyInstance.AnyJsonObject.ToString());

            JsonValue target;

            target = dynArray[0] = dynArray[1] = dynArray[2] = value;
            Assert.Equal((string)target, value);
            Assert.Equal((string)dynArray[0], value);
            Assert.Equal((string)dynArray[1], value);
            Assert.Equal((string)dynArray[2], value);

            target = dynObj["key0"] = dynObj["key1"] = dynObj["key2"] = value;
            Assert.Equal((string)target, value);
            Assert.Equal((string)dynObj["key0"], value);
            Assert.Equal((string)dynObj["key1"], value);
            Assert.Equal((string)dynObj["key2"], value);
            foreach (KeyValuePair<string, JsonValue> pair in AnyInstance.AnyJsonObject)
            {
                Assert.Equal<string>(AnyInstance.AnyJsonObject[pair.Key].ToString(), dynObj[pair.Key].ToString());
            }
        }

        [Fact]
        public void IndexConversionTest()
        {
            dynamic target = AnyInstance.AnyJsonArray;
            dynamic expected = AnyInstance.AnyJsonArray[0];
            dynamic result;

            dynamic[] zero_indexes = 
            {
                (short)0,
                (ushort)0,
                (byte)0,
                (sbyte)0,
                (char)0,
                (int)0
            };


            result = target[(short)0];
            Assert.Same(expected, result);
            result = target[(ushort)0];
            Assert.Same(expected, result);
            result = target[(byte)0];
            Assert.Same(expected, result);
            result = target[(sbyte)0];
            Assert.Same(expected, result);
            result = target[(char)0];
            Assert.Same(expected, result);

            foreach (dynamic zero_index in zero_indexes)
            {
                result = target[zero_index];
                Assert.Same(expected, result);
            }
        }

        [Fact]
        public void InvalidIndexTest()
        {
            object index1 = new object();
            bool index2 = true;
            Person index3 = AnyInstance.AnyPerson;
            JsonObject jo = AnyInstance.AnyJsonObject;

            dynamic target;
            object ret;

            JsonValue[] values = { AnyInstance.AnyJsonObject, AnyInstance.AnyJsonArray };

            foreach (JsonValue value in values)
            {
                target = value;

                ExceptionHelper.Throws<ArgumentException>(delegate { ret = target[index1]; }, String.Format(InvalidIndexType, index1.GetType().FullName));
                ExceptionHelper.Throws<ArgumentException>(delegate { ret = target[index2]; }, String.Format(InvalidIndexType, index2.GetType().FullName));
                ExceptionHelper.Throws<ArgumentException>(delegate { ret = target[index3]; }, String.Format(InvalidIndexType, index3.GetType().FullName));
                ExceptionHelper.Throws<ArgumentException>(delegate { ret = target[null]; }, NonSingleNonNullIndexNotSupported);

                ExceptionHelper.Throws<ArgumentException>(delegate { ret = target[0, 1]; }, NonSingleNonNullIndexNotSupported);
                ExceptionHelper.Throws<ArgumentException>(delegate { ret = target["key1", "key2"]; }, NonSingleNonNullIndexNotSupported);

                ExceptionHelper.Throws<ArgumentException>(delegate { ret = target[true]; }, String.Format(InvalidIndexType, true.GetType().FullName));

                ExceptionHelper.Throws<ArgumentException>(delegate { target[index1] = jo; }, String.Format(InvalidIndexType, index1.GetType().FullName));
                ExceptionHelper.Throws<ArgumentException>(delegate { target[index2] = jo; }, String.Format(InvalidIndexType, index2.GetType().FullName));
                ExceptionHelper.Throws<ArgumentException>(delegate { target[index3] = jo; }, String.Format(InvalidIndexType, index3.GetType().FullName));
                ExceptionHelper.Throws<ArgumentException>(delegate { target[null] = jo; }, NonSingleNonNullIndexNotSupported);

                ExceptionHelper.Throws<ArgumentException>(delegate { target[0, 1] = jo; }, NonSingleNonNullIndexNotSupported);
                ExceptionHelper.Throws<ArgumentException>(delegate { target["key1", "key2"] = jo; }, NonSingleNonNullIndexNotSupported);

                ExceptionHelper.Throws<ArgumentException>(delegate { target[true] = jo; }, String.Format(InvalidIndexType, true.GetType().FullName));
            }
        }

        [Fact]
        public void InvalidCastingTests()
        {
            dynamic dyn;
            string value = "NameValue";

            dyn = AnyInstance.AnyJsonPrimitive;
            ExceptionHelper.Throws<InvalidOperationException>(delegate { dyn.name = value; });

            dyn = AnyInstance.AnyJsonArray;
            ExceptionHelper.Throws<InvalidOperationException>(delegate { dyn.name = value; });

            dyn = new JsonObject(AnyInstance.AnyJsonObject);
            dyn.name = value;
            Assert.Equal((string)dyn.name, value);

            dyn = AnyInstance.DefaultJsonValue;
            ExceptionHelper.Throws<InvalidOperationException>(delegate { dyn.name = value; });
        }

        [Fact]
        public void CastTests()
        {
            dynamic dyn = JsonValueExtensions.CreateFrom(AnyInstance.AnyPerson) as JsonObject;
            string city = dyn.Address.City;

            Assert.Equal<string>(AnyInstance.AnyPerson.Address.City, dyn.Address.City.ReadAs<string>());
            Assert.Equal<string>(AnyInstance.AnyPerson.Address.City, city);

            JsonValue[] values = 
            {
                AnyInstance.AnyInt,
                AnyInstance.AnyString,
                AnyInstance.AnyDateTime,
                AnyInstance.AnyJsonObject,
                AnyInstance.AnyJsonArray,
                AnyInstance.DefaultJsonValue 
            };

            int loopCount = 2;
            bool explicitCast = true;

            while (loopCount > 0)
            {
                loopCount--;

                foreach (JsonValue jv in values)
                {
                    EvaluateNoExceptions<JsonValue>(null, explicitCast);
                    EvaluateNoExceptions<JsonValue>(jv, explicitCast);
                    EvaluateNoExceptions<object>(jv, explicitCast);
                    EvaluateNoExceptions<IDynamicMetaObjectProvider>(jv, explicitCast);
                    EvaluateNoExceptions<IEnumerable<KeyValuePair<string, JsonValue>>>(jv, explicitCast);
                    EvaluateNoExceptions<string>(null, explicitCast);

                    EvaluateExpectExceptions<int>(null, explicitCast);
                    EvaluateExpectExceptions<Person>(jv, explicitCast);
                    EvaluateExpectExceptions<Exception>(jv, explicitCast);

                    EvaluateIgnoreExceptions<JsonObject>(jv, explicitCast);
                    EvaluateIgnoreExceptions<int>(jv, explicitCast);
                    EvaluateIgnoreExceptions<string>(jv, explicitCast);
                    EvaluateIgnoreExceptions<DateTime>(jv, explicitCast);
                    EvaluateIgnoreExceptions<JsonArray>(jv, explicitCast);
                    EvaluateIgnoreExceptions<JsonPrimitive>(jv, explicitCast);
                }

                explicitCast = false;
            }

            EvaluateNoExceptions<IDictionary<string, JsonValue>>(AnyInstance.AnyJsonObject, false);
            EvaluateNoExceptions<IList<JsonValue>>(AnyInstance.AnyJsonArray, false);
        }

        static void EvaluateNoExceptions<T>(JsonValue value, bool cast)
        {
            Evaluate<T>(value, cast, false, true);
        }

        static void EvaluateExpectExceptions<T>(JsonValue value, bool cast)
        {
            Evaluate<T>(value, cast, true, true);
        }

        static void EvaluateIgnoreExceptions<T>(JsonValue value, bool cast)
        {
            Evaluate<T>(value, cast, true, false);
        }

        static void Evaluate<T>(JsonValue value, bool cast, bool throwExpected, bool assertExceptions)
        {
            T ret2;
            object obj = null;
            bool exceptionThrown = false;
            string retstr2, retstr1;

            Console.WriteLine("Test info: expected:[{0}], explicitCast type:[{1}]", value, typeof(T));

            try
            {
                if (typeof(int) == typeof(T))
                {
                    obj = ((int)value);
                }
                else if (typeof(string) == typeof(T))
                {
                    obj = ((string)value);
                }
                else if (typeof(DateTime) == typeof(T))
                {
                    obj = ((DateTime)value);
                }
                else if (typeof(IList<JsonValue>) == typeof(T))
                {
                    obj = (IList<JsonValue>)value;
                }
                else if (typeof(IDictionary<string, JsonValue>) == typeof(T))
                {
                    obj = (IDictionary<string, JsonValue>)value;
                }
                else if (typeof(JsonValue) == typeof(T))
                {
                    obj = (JsonValue)value;
                }
                else if (typeof(JsonObject) == typeof(T))
                {
                    obj = (JsonObject)value;
                }
                else if (typeof(JsonArray) == typeof(T))
                {
                    obj = (JsonArray)value;
                }
                else if (typeof(JsonPrimitive) == typeof(T))
                {
                    obj = (JsonPrimitive)value;
                }
                else
                {
                    obj = (T)(object)value;
                }

                retstr1 = obj == null ? "null" : obj.ToString();
            }
            catch (Exception ex)
            {
                exceptionThrown = true;
                retstr1 = ex.Message;
            }

            if (assertExceptions)
            {
                Assert.Equal<bool>(throwExpected, exceptionThrown);
            }

            exceptionThrown = false;

            try
            {
                dynamic dyn = value as dynamic;
                if (cast)
                {
                    ret2 = (T)dyn;
                }
                else
                {
                    ret2 = dyn;
                }
                retstr2 = ret2 != null ? ret2.ToString() : "null";
            }
            catch (Exception ex)
            {
                exceptionThrown = true;
                retstr2 = ex.Message;
            }

            if (assertExceptions)
            {
                Assert.Equal<bool>(throwExpected, exceptionThrown);
            }

            // fixup string
            retstr1 = retstr1.Replace("\'Person\'", String.Format("\'{0}\'", typeof(Person).FullName));
            if (retstr1.EndsWith(".")) retstr1 = retstr1.Substring(0, retstr1.Length - 1);

            // fixup string
            retstr2 = retstr2.Replace("\'string\'", String.Format("\'{0}\'", typeof(string).FullName));
            retstr2 = retstr2.Replace("\'int\'", String.Format("\'{0}\'", typeof(int).FullName));
            if (retstr2.EndsWith(".")) retstr2 = retstr2.Substring(0, retstr2.Length - 1);

            Assert.Equal<string>(retstr1, retstr2);
        }
    }
}
