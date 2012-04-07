// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using Xunit;
using Xunit.Extensions;

namespace System.Json
{
    public class JsonValueExtensionsTest
    {
        const string DynamicPropertyNotDefined = "'{0}' does not contain a definition for property '{1}'.";
        const string OperationNotSupportedOnJsonTypeMsgFormat = "Operation not supported on JsonValue instance of 'JsonType.{0}' type.";

        [Fact]
        public void CreateFromTypeTest()
        {
            JsonValue[] values =
            {
                AnyInstance.AnyJsonObject,
                AnyInstance.AnyJsonArray,
                AnyInstance.AnyJsonPrimitive,
                AnyInstance.DefaultJsonValue
            };

            foreach (JsonValue value in values)
            {
                Assert.Same(value, JsonValueExtensions.CreateFrom(value));
            }
        }

        public static IEnumerable<object[]> PrimitiveTestData
        {
            get
            {
                yield return new object[] { AnyInstance.AnyBool };
                yield return new object[] { AnyInstance.AnyByte };
                yield return new object[] { AnyInstance.AnyChar };
                yield return new object[] { AnyInstance.AnyDateTime };
                yield return new object[] { AnyInstance.AnyDateTimeOffset };
                yield return new object[] { AnyInstance.AnyDecimal };
                yield return new object[] { AnyInstance.AnyDouble };
                yield return new object[] { AnyInstance.AnyFloat };
                yield return new object[] { AnyInstance.AnyGuid };
                yield return new object[] { AnyInstance.AnyLong };
                yield return new object[] { AnyInstance.AnySByte };
                yield return new object[] { AnyInstance.AnyShort };
                yield return new object[] { AnyInstance.AnyUInt };
                yield return new object[] { AnyInstance.AnyULong };
                yield return new object[] { AnyInstance.AnyUri };
                yield return new object[] { AnyInstance.AnyUShort };
                yield return new object[] { AnyInstance.AnyInt };
                yield return new object[] { AnyInstance.AnyString };
            }
        }

        [Theory]
        [PropertyData("PrimitiveTestData")]
        public void CreateFromPrimitiveTest(object value)
        {
            Type valueType = value.GetType();
            Assert.Equal(value, JsonValueExtensions.CreateFrom(value).ReadAs(valueType));
        }

        [Fact]
        public void CreateFromComplexTest()
        {
            JsonValue target = JsonValueExtensions.CreateFrom(AnyInstance.AnyPerson);

            Assert.Equal(AnyInstance.AnyPerson.Name, (string)target["Name"]);
            Assert.Equal(AnyInstance.AnyPerson.Age, (int)target["Age"]);
            Assert.Equal(AnyInstance.AnyPerson.Address.City, (string)target.ValueOrDefault("Address", "City"));
        }

        [Fact]
        public void CreateFromDynamicSimpleTest()
        {
            JsonValue target;

            target = JsonValueExtensions.CreateFrom(AnyInstance.AnyDynamic);
            Assert.NotNull(target);

            string expected = "{\"Name\":\"Bill Gates\",\"Age\":21,\"Grades\":[\"F\",\"B-\",\"C\"]}";
            dynamic obj = new TestDynamicObject();
            obj.Name = "Bill Gates";
            obj.Age = 21;
            obj.Grades = new[] { "F", "B-", "C" };

            target = JsonValueExtensions.CreateFrom(obj);
            Assert.Equal<string>(expected, target.ToString());

            target = JsonValueExtensions.CreateFrom(new TestDynamicObject());
            Assert.Equal<string>("{}", target.ToString());
        }

        [Fact]
        public void CreateFromDynamicComplextTest()
        {
            JsonValue target;
            Person person = AnyInstance.AnyPerson;
            dynamic dyn = TestDynamicObject.CreatePersonAsDynamic(person);

            dyn.TestProperty = AnyInstance.AnyString;

            target = JsonValueExtensions.CreateFrom(dyn);
            Assert.NotNull(target);
            Assert.Equal<string>(AnyInstance.AnyString, dyn.TestProperty);
            Person jvPerson = target.ReadAsType<Person>();
            Assert.Equal(person.ToString(), jvPerson.ToString());

            Person p1 = Person.CreateSample();
            Person p2 = Person.CreateSample();

            p2.Name += "__2";
            p2.Age += 10;
            p2.Address.City += "__2";

            Person[] friends = new Person[] { p1, p2 };
            target = JsonValueExtensions.CreateFrom(friends);
            Person[] personArr = target.ReadAsType<Person[]>();
            Assert.Equal<int>(friends.Length, personArr.Length);
            Assert.Equal<string>(friends[0].ToString(), personArr[0].ToString());
            Assert.Equal<string>(friends[1].ToString(), personArr[1].ToString());
        }

        [Fact]
        public void CreateFromDynamicBinderFallbackTest()
        {
            JsonValue target;
            Person person = AnyInstance.AnyPerson;
            dynamic dyn = new TestDynamicObject();
            dyn.Name = AnyInstance.AnyString;

            dyn.UseFallbackMethod = true;
            string expectedMessage = String.Format(DynamicPropertyNotDefined, dyn.GetType().FullName, "Name");
            ExceptionHelper.Throws<InvalidOperationException>(() => target = JsonValueExtensions.CreateFrom(dyn), expectedMessage);

            dyn.UseErrorSuggestion = true;
            ExceptionHelper.Throws<TestDynamicObject.TestDynamicObjectException>(() => target = JsonValueExtensions.CreateFrom(dyn));
        }

        [Fact]
        public void CreateFromNestedDynamicTest()
        {
            JsonValue target;
            string expected = "{\"Name\":\"Root\",\"Level1\":{\"Name\":\"Level1\",\"Level2\":{\"Name\":\"Level2\"}}}";
            dynamic dyn = new TestDynamicObject();
            dyn.Name = "Root";
            dyn.Level1 = new TestDynamicObject();
            dyn.Level1.Name = "Level1";
            dyn.Level1.Level2 = new TestDynamicObject();
            dyn.Level1.Level2.Name = "Level2";

            target = JsonValueExtensions.CreateFrom(dyn);
            Assert.NotNull(target);
            Assert.Equal<string>(expected, target.ToString());
        }

        [Fact]
        public void CreateFromDynamicWithJsonValueChildrenTest()
        {
            JsonValue target;
            string level3 = "{\"Name\":\"Level3\",\"Null\":null}";
            string level2 = "{\"Name\":\"Level2\",\"JsonObject\":" + AnyInstance.AnyJsonObject.ToString() + ",\"JsonArray\":" + AnyInstance.AnyJsonArray.ToString() + ",\"Level3\":" + level3 + "}";
            string level1 = "{\"Name\":\"Level1\",\"JsonPrimitive\":" + AnyInstance.AnyJsonPrimitive.ToString() + ",\"Level2\":" + level2 + "}";
            string expected = "{\"Name\":\"Root\",\"Level1\":" + level1 + "}";

            dynamic dyn = new TestDynamicObject();
            dyn.Name = "Root";
            dyn.Level1 = new TestDynamicObject();
            dyn.Level1.Name = "Level1";
            dyn.Level1.JsonPrimitive = AnyInstance.AnyJsonPrimitive;
            dyn.Level1.Level2 = new TestDynamicObject();
            dyn.Level1.Level2.Name = "Level2";
            dyn.Level1.Level2.JsonObject = AnyInstance.AnyJsonObject;
            dyn.Level1.Level2.JsonArray = AnyInstance.AnyJsonArray;
            dyn.Level1.Level2.Level3 = new TestDynamicObject();
            dyn.Level1.Level2.Level3.Name = "Level3";
            dyn.Level1.Level2.Level3.Null = null;

            target = JsonValueExtensions.CreateFrom(dyn);
            Assert.Equal<string>(expected, target.ToString());
        }

        [Fact]
        public void CreateFromDynamicJVTest()
        {
            JsonValue target;

            dynamic[] values = new dynamic[]
            {
                AnyInstance.AnyJsonArray,
                AnyInstance.AnyJsonObject,
                AnyInstance.AnyJsonPrimitive,
                AnyInstance.DefaultJsonValue
            };

            foreach (dynamic dyn in values)
            {
                target = JsonValueExtensions.CreateFrom(dyn);
                Assert.Same(dyn, target);
            }
        }

        [Fact]
        public void ReadAsTypeFallbackTest()
        {
            JsonValue jv = AnyInstance.AnyInt;
            Person personFallback = Person.CreateSample();

            Person personResult = jv.ReadAsType<Person>(personFallback);
            Assert.Same(personFallback, personResult);

            int intFallback = 45;
            int intValue = jv.ReadAsType<int>(intFallback);
            Assert.Equal<int>(AnyInstance.AnyInt, intValue);
        }

        [Fact(Skip = "See bug #228569 in CSDMain")]
        public void ReadAsTypeCollectionTest()
        {
            JsonValue jsonValue;
            jsonValue = JsonValue.Parse("[1,2,3]");

            List<object> list = jsonValue.ReadAsType<List<object>>();
            Array array = jsonValue.ReadAsType<Array>();
            object[] objArr = jsonValue.ReadAsType<object[]>();

            IList[] collections = 
            {
                list, array, objArr
            };

            foreach (IList collection in collections)
            {
                Assert.Equal<int>(jsonValue.Count, collection.Count);

                for (int i = 0; i < jsonValue.Count; i++)
                {
                    Assert.Equal<int>((int)jsonValue[i], (int)collection[i]);
                }
            }

            jsonValue = JsonValue.Parse("{\"A\":1,\"B\":2,\"C\":3}");
            Dictionary<string, object> dictionary = jsonValue.ReadAsType<Dictionary<string, object>>();

            Assert.Equal<int>(jsonValue.Count, dictionary.Count);
            foreach (KeyValuePair<string, JsonValue> pair in jsonValue)
            {
                Assert.Equal((int)jsonValue[pair.Key], (int)dictionary[pair.Key]);
            }
        }

        [Fact]
        public void TryReadAsInvalidCollectionTest()
        {
            JsonValue jo = AnyInstance.AnyJsonObject;
            JsonValue ja = AnyInstance.AnyJsonArray;
            JsonValue jp = AnyInstance.AnyJsonPrimitive;
            JsonValue jd = AnyInstance.DefaultJsonValue;

            JsonValue[] invalidArrays = 
            {
                jo, jp, jd
            };

            JsonValue[] invalidDictionaries =
            {
                ja, jp, jd
            };

            bool success;
            object[] array;
            Dictionary<string, object> dictionary;

            foreach (JsonValue value in invalidArrays)
            {
                success = value.TryReadAsType<object[]>(out array);
                Console.WriteLine("Try reading {0} as object[]; success = {1}", value.ToString(), success);
                Assert.False(success);
                Assert.Null(array);
            }

            foreach (JsonValue value in invalidDictionaries)
            {
                success = value.TryReadAsType<Dictionary<string, object>>(out dictionary);
                Console.WriteLine("Try reading {0} as Dictionary<string, object>; success = {1}", value.ToString(), success);
                Assert.False(success);
                Assert.Null(dictionary);
            }
        }

        [Fact]
        public void ReadAsExtensionsOnDynamicTest()
        {
            dynamic jv = JsonValueExtensions.CreateFrom(AnyInstance.AnyPerson);
            bool success;
            object obj;

            success = jv.TryReadAsType(typeof(Person), out obj);
            Assert.True(success);
            Assert.NotNull(obj);
            Assert.Equal<string>(AnyInstance.AnyPerson.ToString(), obj.ToString());

            obj = jv.ReadAsType(typeof(Person));
            Assert.NotNull(obj);
            Assert.Equal<string>(AnyInstance.AnyPerson.ToString(), obj.ToString());
        }

#if CODEPLEX
        [Fact]
        public void ToCollectionTest()
        {
            JsonValue target;
            object[] array;

            target = AnyInstance.AnyJsonArray;
            array = target.ToObjectArray();

            Assert.Equal(target.Count, array.Length);

            for (int i = 0; i < target.Count; i++)
            {
                Assert.Equal(array[i], target[i].ReadAs(array[i].GetType()));
            }

            target = AnyInstance.AnyJsonObject;
            IDictionary<string, object> dictionary = target.ToDictionary();

            Assert.Equal(target.Count, dictionary.Count);

            foreach (KeyValuePair<string, JsonValue> pair in target)
            {
                Assert.True(dictionary.ContainsKey(pair.Key));
                Assert.Equal<string>(target[pair.Key].ToString(), dictionary[pair.Key].ToString());
            }
        }

        [Fact]
        public void ToCollectionsNestedTest()
        {
            JsonArray ja = JsonValue.Parse("[1, {\"A\":[1,2,3]}, 5]") as JsonArray;
            JsonObject jo = JsonValue.Parse("{\"A\":1,\"B\":[1,2,3]}") as JsonObject;

            object[] objArray = ja.ToObjectArray();
            Assert.NotNull(objArray);
            Assert.Equal<int>(ja.Count, objArray.Length);
            Assert.Equal((int)ja[0], (int)objArray[0]);
            Assert.Equal((int)ja[2], (int)objArray[2]);

            IDictionary<string, object> dict = objArray[1] as IDictionary<string, object>;
            Assert.NotNull(dict);

            objArray = dict["A"] as object[];
            Assert.NotNull(objArray);
            for (int i = 0; i < 3; i++)
            {
                Assert.Equal(i + 1, (int)objArray[i]);
            }

            dict = jo.ToDictionary();
            Assert.NotNull(dict);
            Assert.Equal<int>(jo.Count, dict.Count);
            Assert.Equal<int>(1, (int)dict["A"]);

            objArray = dict["B"] as object[];
            Assert.NotNull(objArray);
            for (int i = 1; i < 3; i++)
            {
                Assert.Equal(i + 1, (int)objArray[i]);
            }
        }

        [Fact]
        public void ToCollectionsInvalidTest()
        {
            JsonValue jo = AnyInstance.AnyJsonObject;
            JsonValue ja = AnyInstance.AnyJsonArray;
            JsonValue jp = AnyInstance.AnyJsonPrimitive;
            JsonValue jd = AnyInstance.DefaultJsonValue;

            ExceptionTestHelper.ExpectException<NotSupportedException>(delegate { var ret = jd.ToObjectArray(); }, String.Format(OperationNotSupportedOnJsonTypeMsgFormat, jd.JsonType));
            ExceptionTestHelper.ExpectException<NotSupportedException>(delegate { var ret = jd.ToDictionary(); }, String.Format(OperationNotSupportedOnJsonTypeMsgFormat, jd.JsonType));

            ExceptionTestHelper.ExpectException<NotSupportedException>(delegate { var ret = jp.ToObjectArray(); }, String.Format(OperationNotSupportedOnJsonTypeMsgFormat, jp.JsonType));
            ExceptionTestHelper.ExpectException<NotSupportedException>(delegate { var ret = jp.ToDictionary(); }, String.Format(OperationNotSupportedOnJsonTypeMsgFormat, jp.JsonType));

            ExceptionTestHelper.ExpectException<NotSupportedException>(delegate { var ret = jo.ToObjectArray(); }, String.Format(OperationNotSupportedOnJsonTypeMsgFormat, jo.JsonType));
            ExceptionTestHelper.ExpectException<NotSupportedException>(delegate { var ret = ja.ToDictionary(); }, String.Format(OperationNotSupportedOnJsonTypeMsgFormat, ja.JsonType));
        }

        // 195843 JsonValue to support generic extension methods defined in JsonValueExtensions.
        // 195867 Consider creating extension point for allowing new extension methods to be callable via dynamic interface
        //[Fact] This requires knowledge of the C# binder to be able to get the generic call parameters.
        public void ReadAsGenericExtensionsOnDynamicTest()
        {
            dynamic jv = JsonValueExtensions.CreateFrom(AnyInstance.AnyPerson);
            Person person;
            bool success;

            person = jv.ReadAsType<Person>();
            Assert.NotNull(person);
            Assert.Equal<string>(AnyInstance.AnyPerson.ToString(), person.ToString());

            success = jv.TryReadAsType<Person>(out person);
            Assert.True(success);
            Assert.NotNull(person);
            Assert.Equal<string>(AnyInstance.AnyPerson.ToString(), person.ToString());
        }
#else
        [Fact(Skip = "See bug #228569 in CSDMain")]
        public void TestDataContractJsonSerializerSettings()
        {
            TestTypeForSerializerSettings instance = new TestTypeForSerializerSettings
            {
                BaseRef = new DerivedType(),
                Date = AnyInstance.AnyDateTime,
                Dict = new Dictionary<string, object>
                {
                    { "one", 1 },
                    { "two", 2 },
                    { "two point five", 2.5 },
                }
            };

            JsonObject dict = new JsonObject
            {
                { "one", 1 },
                { "two", 2 },
                { "two point five", 2.5 },
            };

            JsonObject equivalentJsonObject = new JsonObject
            {
                { "BaseRef", new JsonObject { { "__type", "DerivedType:NS" } } },
                { "Date", AnyInstance.AnyDateTime },
                { "Dict", dict },
            };

            JsonObject createdFromType = JsonValueExtensions.CreateFrom(instance) as JsonObject;
            Assert.Equal(equivalentJsonObject.ToString(), createdFromType.ToString());

            TestTypeForSerializerSettings newInstance = equivalentJsonObject.ReadAsType<TestTypeForSerializerSettings>();
            // DISABLED, 198487 - Assert.Equal(instance.Date, newInstance.Date);
            Assert.Equal(instance.BaseRef.GetType().FullName, newInstance.BaseRef.GetType().FullName);
            Assert.Equal(3, newInstance.Dict.Count);
            Assert.Equal(1, newInstance.Dict["one"]);
            Assert.Equal(2, newInstance.Dict["two"]);
            Assert.Equal(2.5, Convert.ToDouble(newInstance.Dict["two point five"], CultureInfo.InvariantCulture));
        }

        [DataContract]
        public class TestTypeForSerializerSettings
        {
            [DataMember]
            public BaseType BaseRef { get; set; }
            [DataMember]
            public DateTime Date { get; set; }
            [DataMember]
            public Dictionary<string, object> Dict { get; set; }
        }

        [DataContract(Name = "BaseType", Namespace = "NS")]
        [KnownType(typeof(DerivedType))]
        public class BaseType
        {
        }

        [DataContract(Name = "DerivedType", Namespace = "NS")]
        public class DerivedType : BaseType
        {
        }
#endif
    }
}
