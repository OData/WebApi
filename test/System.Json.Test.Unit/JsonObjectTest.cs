// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization.Json;
using Xunit;

namespace System.Json
{
    public class JsonObjectTest
    {
        [Fact]
        public void JsonObjectConstructorEnumTest()
        {
            string key1 = AnyInstance.AnyString;
            string key2 = AnyInstance.AnyString2;
            JsonValue value1 = AnyInstance.AnyJsonValue1;
            JsonValue value2 = AnyInstance.AnyJsonValue2;

            List<KeyValuePair<string, JsonValue>> items = new List<KeyValuePair<string, JsonValue>>()
            {
                new KeyValuePair<string, JsonValue>(key1, value1),
                new KeyValuePair<string, JsonValue>(key2, value2),
            };

            JsonObject target = new JsonObject(null);
            Assert.Equal(0, target.Count);

            target = new JsonObject(items);
            Assert.Equal(2, target.Count);
            ValidateJsonObjectItems(target, key1, value1, key2, value2);

            // Invalid tests
            items.Add(new KeyValuePair<string, JsonValue>(key1, AnyInstance.DefaultJsonValue));
            ExceptionHelper.Throws<ArgumentException>(delegate { new JsonObject(items); });
        }

        [Fact]
        public void JsonObjectConstructorParmsTest()
        {
            JsonObject target = new JsonObject();
            Assert.Equal(0, target.Count);

            string key1 = AnyInstance.AnyString;
            string key2 = AnyInstance.AnyString2;
            JsonValue value1 = AnyInstance.AnyJsonValue1;
            JsonValue value2 = AnyInstance.AnyJsonValue2;

            List<KeyValuePair<string, JsonValue>> items = new List<KeyValuePair<string, JsonValue>>()
            {
                new KeyValuePair<string, JsonValue>(key1, value1),
                new KeyValuePair<string, JsonValue>(key2, value2),
            };

            target = new JsonObject(items[0], items[1]);
            Assert.Equal(2, target.Count);
            ValidateJsonObjectItems(target, key1, value1, key2, value2);

            target = new JsonObject(items.ToArray());
            Assert.Equal(2, target.Count);
            ValidateJsonObjectItems(target, key1, value1, key2, value2);

            // Invalid tests
            items.Add(new KeyValuePair<string, JsonValue>(key1, AnyInstance.DefaultJsonValue));
            ExceptionHelper.Throws<ArgumentException>(delegate { new JsonObject(items[0], items[1], items[2]); });
            ExceptionHelper.Throws<ArgumentException>(delegate { new JsonObject(items.ToArray()); });
        }

        [Fact]
        public void AddTest()
        {
            string key1 = AnyInstance.AnyString;
            string key2 = AnyInstance.AnyString2;
            JsonValue value1 = AnyInstance.AnyJsonValue1;
            JsonValue value2 = AnyInstance.AnyJsonValue2;

            JsonObject target;

            target = new JsonObject();
            target.Add(new KeyValuePair<string, JsonValue>(key1, value1));
            Assert.Equal(1, target.Count);
            Assert.True(target.ContainsKey(key1));
            Assert.Equal(value1, target[key1]);

            target.Add(key2, value2);
            Assert.Equal(2, target.Count);
            Assert.True(target.ContainsKey(key2));
            Assert.Equal(value2, target[key2]);

            ExceptionHelper.Throws<ArgumentNullException>(delegate { new JsonObject().Add(null, value1); });
            ExceptionHelper.Throws<ArgumentNullException>(delegate { new JsonObject().Add(new KeyValuePair<string, JsonValue>(null, value1)); });

            ExceptionHelper.Throws<ArgumentException>(delegate { new JsonObject().Add(key1, AnyInstance.DefaultJsonValue); });
            ExceptionHelper.Throws<ArgumentException>(delegate { new JsonArray().Add(AnyInstance.DefaultJsonValue); });
        }

        [Fact]
        public void AddRangeParamsTest()
        {
            string key1 = AnyInstance.AnyString;
            string key2 = AnyInstance.AnyString2;
            JsonValue value1 = AnyInstance.AnyJsonValue1;
            JsonValue value2 = AnyInstance.AnyJsonValue2;

            List<KeyValuePair<string, JsonValue>> items = new List<KeyValuePair<string, JsonValue>>()
            {
                new KeyValuePair<string, JsonValue>(key1, value1),
                new KeyValuePair<string, JsonValue>(key2, value2),
            };

            JsonObject target;

            target = new JsonObject();
            target.AddRange(items[0], items[1]);
            Assert.Equal(2, target.Count);
            ValidateJsonObjectItems(target, key1, value1, key2, value2);

            target = new JsonObject();
            target.AddRange(items.ToArray());
            Assert.Equal(2, target.Count);
            ValidateJsonObjectItems(target, key1, value1, key2, value2);

            ExceptionHelper.Throws<ArgumentNullException>(delegate { new JsonObject().AddRange((KeyValuePair<string, JsonValue>[])null); });
            ExceptionHelper.Throws<ArgumentNullException>(delegate { new JsonObject().AddRange((IEnumerable<KeyValuePair<string, JsonValue>>)null); });

            items[1] = new KeyValuePair<string, JsonValue>(key2, AnyInstance.DefaultJsonValue);
            ExceptionHelper.Throws<ArgumentException>(delegate { new JsonObject().AddRange(items.ToArray()); });
            ExceptionHelper.Throws<ArgumentException>(delegate { new JsonObject().AddRange(items[0], items[1]); });
        }

        [Fact]
        public void AddRangeEnumTest()
        {
            string key1 = AnyInstance.AnyString;
            string key2 = AnyInstance.AnyString2;
            JsonValue value1 = AnyInstance.AnyJsonValue1;
            JsonValue value2 = AnyInstance.AnyJsonValue2;

            List<KeyValuePair<string, JsonValue>> items = new List<KeyValuePair<string, JsonValue>>()
            {
                new KeyValuePair<string, JsonValue>(key1, value1),
                new KeyValuePair<string, JsonValue>(key2, value2),
            };

            JsonObject target;

            target = new JsonObject();
            target.AddRange(items);
            Assert.Equal(2, target.Count);
            ValidateJsonObjectItems(target, key1, value1, key2, value2);

            ExceptionHelper.Throws<ArgumentNullException>(delegate { new JsonObject().AddRange(null); });

            items[1] = new KeyValuePair<string, JsonValue>(key2, AnyInstance.DefaultJsonValue);
            ExceptionHelper.Throws<ArgumentException>(delegate { new JsonObject().AddRange(items); });
        }

        [Fact]
        public void ClearTest()
        {
            string key1 = AnyInstance.AnyString;
            string key2 = AnyInstance.AnyString2;
            JsonValue value1 = AnyInstance.AnyJsonValue1;
            JsonValue value2 = AnyInstance.AnyJsonValue2;

            JsonObject target = new JsonObject();
            target.Add(key1, value1);
            target.Clear();
            Assert.Equal(0, target.Count);
            Assert.False(target.ContainsKey(key1));

            target.Add(key2, value2);
            Assert.Equal(1, target.Count);
            Assert.False(target.ContainsKey(key1));
            Assert.True(target.ContainsKey(key2));
        }

        [Fact]
        public void ContainsKeyTest()
        {
            string key1 = AnyInstance.AnyString;
            JsonValue value1 = AnyInstance.AnyJsonValue1;

            JsonObject target = new JsonObject();
            Assert.False(target.ContainsKey(key1));
            target.Add(key1, value1);
            Assert.True(target.ContainsKey(key1));
            target.Clear();
            Assert.False(target.ContainsKey(key1));

            ExceptionHelper.Throws<ArgumentNullException>(delegate { new JsonObject().ContainsKey(null); });
        }

        [Fact]
        public void CopyToTest()
        {
            string key1 = AnyInstance.AnyString;
            string key2 = AnyInstance.AnyString2;
            JsonValue value1 = AnyInstance.AnyJsonValue1;
            JsonValue value2 = AnyInstance.AnyJsonValue2;

            JsonObject target = new JsonObject { { key1, value1 }, { key2, value2 } };

            KeyValuePair<string, JsonValue>[] array = new KeyValuePair<string, JsonValue>[target.Count + 1];

            target.CopyTo(array, 1);
            int index1 = key1 == array[1].Key ? 1 : 2;
            int index2 = index1 == 1 ? 2 : 1;

            Assert.Equal(key1, array[index1].Key);
            Assert.Equal(value1, array[index1].Value);
            Assert.Equal(key2, array[index2].Key);
            Assert.Equal(value2, array[index2].Value);

            ExceptionHelper.Throws<ArgumentNullException>(() => target.CopyTo(null, 0));
            ExceptionHelper.Throws<ArgumentOutOfRangeException>(() => target.CopyTo(array, -1));
            ExceptionHelper.Throws<ArgumentException>(() => target.CopyTo(array, array.Length - target.Count + 1));
        }

        [Fact]
        public void CreateFromComplexTypeTest()
        {
            Assert.Null(JsonValueExtensions.CreateFrom(null));

            Person anyObject = AnyInstance.AnyPerson;

            JsonObject jv = JsonValueExtensions.CreateFrom(anyObject) as JsonObject;
            Assert.NotNull(jv);
            Assert.Equal(4, jv.Count);
            foreach (string key in "Name Age Address".Split())
            {
                Assert.True(jv.ContainsKey(key));
            }

            Assert.Equal(AnyInstance.AnyString, (string)jv["Name"]);
            Assert.Equal(AnyInstance.AnyInt, (int)jv["Age"]);

            JsonObject nestedObject = jv["Address"] as JsonObject;
            Assert.NotNull(nestedObject);
            Assert.Equal(3, nestedObject.Count);
            foreach (string key in "Street City State".Split())
            {
                Assert.True(nestedObject.ContainsKey(key));
            }

            Assert.Equal(Address.AnyStreet, (string)nestedObject["Street"]);
            Assert.Equal(Address.AnyCity, (string)nestedObject["City"]);
            Assert.Equal(Address.AnyState, (string)nestedObject["State"]);
        }

        [Fact]
        public void ReadAsComplexTypeTest()
        {
            JsonObject target = new JsonObject
            {
                { "Name", AnyInstance.AnyString },
                { "Age", AnyInstance.AnyInt },
                { "Address", new JsonObject { { "Street", Address.AnyStreet }, { "City", Address.AnyCity }, { "State", Address.AnyState } } },
            };

            Person person = target.ReadAsType<Person>();
            Assert.Equal(AnyInstance.AnyString, person.Name);
            Assert.Equal(AnyInstance.AnyInt, person.Age);
            Assert.NotNull(person.Address);
            Assert.Equal(Address.AnyStreet, person.Address.Street);
            Assert.Equal(Address.AnyCity, person.Address.City);
            Assert.Equal(Address.AnyState, person.Address.State);
        }

        [Fact]
        public void GetEnumeratorTest()
        {
            string key1 = AnyInstance.AnyString;
            string key2 = AnyInstance.AnyString2;
            JsonValue value1 = AnyInstance.AnyJsonValue1;
            JsonValue value2 = AnyInstance.AnyJsonValue2;

            JsonObject target = new JsonObject { { key1, value1 }, { key2, value2 } };

            IEnumerator<KeyValuePair<string, JsonValue>> enumerator = target.GetEnumerator();
            Assert.True(enumerator.MoveNext());
            bool key1IsFirst = key1 == enumerator.Current.Key;
            if (key1IsFirst)
            {
                Assert.Equal(key1, enumerator.Current.Key);
                Assert.Equal(value1, enumerator.Current.Value);
            }
            else
            {
                Assert.Equal(key2, enumerator.Current.Key);
                Assert.Equal(value2, enumerator.Current.Value);
            }

            Assert.True(enumerator.MoveNext());
            if (key1IsFirst)
            {
                Assert.Equal(key2, enumerator.Current.Key);
                Assert.Equal(value2, enumerator.Current.Value);
            }
            else
            {
                Assert.Equal(key1, enumerator.Current.Key);
                Assert.Equal(value1, enumerator.Current.Value);
            }

            Assert.False(enumerator.MoveNext());
        }

        [Fact]
        public void RemoveTest()
        {
            string key1 = AnyInstance.AnyString;
            string key2 = AnyInstance.AnyString2;
            JsonValue value1 = AnyInstance.AnyJsonValue1;
            JsonValue value2 = AnyInstance.AnyJsonValue2;

            JsonObject target = new JsonObject { { key1, value1 }, { key2, value2 } };

            Assert.True(target.ContainsKey(key1));
            Assert.True(target.ContainsKey(key2));
            Assert.Equal(2, target.Count);

            Assert.True(target.Remove(key2));
            Assert.True(target.ContainsKey(key1));
            Assert.False(target.ContainsKey(key2));
            Assert.Equal(1, target.Count);

            Assert.False(target.Remove(key2));
            Assert.True(target.ContainsKey(key1));
            Assert.False(target.ContainsKey(key2));
            Assert.Equal(1, target.Count);
        }

        [Fact]
        public void ToStringTest()
        {
            JsonObject target = new JsonObject();

            JsonValue item1 = AnyInstance.AnyJsonValue1 ?? "not null";
            JsonValue item2 = null;
            JsonValue item3 = AnyInstance.AnyJsonValue2 ?? "not null";
            JsonValue item4 = AnyInstance.AnyJsonValue3 ?? "not null";
            target.Add("item1", item1);
            target.Add("item2", item2);
            target.Add("item3", item3);
            target.Add("", item4);

            string expected = String.Format(CultureInfo.InvariantCulture, "{{\"item1\":{0},\"item2\":null,\"item3\":{1},\"\":{2}}}", item1.ToString(), item3.ToString(), item4.ToString());
            Assert.Equal<string>(expected, target.ToString());

            string json = "{\r\n  \"item1\": \"hello\",\r\n  \"item2\": null,\r\n  \"item3\": [\r\n    1,\r\n    2,\r\n    3\r\n  ],\r\n  \"\": \"notnull\"\r\n}";
            target = JsonValue.Parse(json) as JsonObject;

            Assert.Equal<string>(json.Replace("\r\n", "").Replace(" ", ""), target.ToString());
        }

        [Fact]
        public void ContainsKVPTest()
        {
            JsonObject target = new JsonObject();
            KeyValuePair<string, JsonValue> item = new KeyValuePair<string, JsonValue>(AnyInstance.AnyString, AnyInstance.AnyJsonValue1);
            KeyValuePair<string, JsonValue> item2 = new KeyValuePair<string, JsonValue>(AnyInstance.AnyString2, AnyInstance.AnyJsonValue2);
            target.Add(item);
            Assert.True(((ICollection<KeyValuePair<string, JsonValue>>)target).Contains(item));
            Assert.False(((ICollection<KeyValuePair<string, JsonValue>>)target).Contains(item2));
        }

        [Fact]
        public void RemoveKVPTest()
        {
            JsonObject target = new JsonObject();
            KeyValuePair<string, JsonValue> item1 = new KeyValuePair<string, JsonValue>(AnyInstance.AnyString, AnyInstance.AnyJsonValue1);
            KeyValuePair<string, JsonValue> item2 = new KeyValuePair<string, JsonValue>(AnyInstance.AnyString2, AnyInstance.AnyJsonValue2);
            target.AddRange(item1, item2);

            Assert.Equal(2, target.Count);
            Assert.True(((ICollection<KeyValuePair<string, JsonValue>>)target).Contains(item1));
            Assert.True(((ICollection<KeyValuePair<string, JsonValue>>)target).Contains(item2));

            Assert.True(((ICollection<KeyValuePair<string, JsonValue>>)target).Remove(item1));
            Assert.Equal(1, target.Count);
            Assert.False(((ICollection<KeyValuePair<string, JsonValue>>)target).Contains(item1));
            Assert.True(((ICollection<KeyValuePair<string, JsonValue>>)target).Contains(item2));

            Assert.False(((ICollection<KeyValuePair<string, JsonValue>>)target).Remove(item1));
            Assert.Equal(1, target.Count);
            Assert.False(((ICollection<KeyValuePair<string, JsonValue>>)target).Contains(item1));
            Assert.True(((ICollection<KeyValuePair<string, JsonValue>>)target).Contains(item2));
        }

        [Fact]
        public void GetEnumeratorTest1()
        {
            string key1 = AnyInstance.AnyString;
            string key2 = AnyInstance.AnyString2;
            JsonValue value1 = AnyInstance.AnyJsonValue1;
            JsonValue value2 = AnyInstance.AnyJsonValue2;

            JsonObject target = new JsonObject { { key1, value1 }, { key2, value2 } };

            IEnumerator enumerator = ((IEnumerable)target).GetEnumerator();
            Assert.True(enumerator.MoveNext());
            Assert.IsType<KeyValuePair<string, JsonValue>>(enumerator.Current);
            KeyValuePair<string, JsonValue> current = (KeyValuePair<string, JsonValue>)enumerator.Current;

            bool key1IsFirst = key1 == current.Key;
            if (key1IsFirst)
            {
                Assert.Equal(key1, current.Key);
                Assert.Equal(value1, current.Value);
            }
            else
            {
                Assert.Equal(key2, current.Key);
                Assert.Equal(value2, current.Value);
            }

            Assert.True(enumerator.MoveNext());
            Assert.IsType<KeyValuePair<string, JsonValue>>(enumerator.Current);
            current = (KeyValuePair<string, JsonValue>)enumerator.Current;
            if (key1IsFirst)
            {
                Assert.Equal(key2, current.Key);
                Assert.Equal(value2, current.Value);
            }
            else
            {
                Assert.Equal(key1, current.Key);
                Assert.Equal(value1, current.Value);
            }

            Assert.False(enumerator.MoveNext());
        }

        [Fact]
        public void TryGetValueTest()
        {
            string key1 = AnyInstance.AnyString;
            string key2 = AnyInstance.AnyString2;
            JsonValue value1 = AnyInstance.AnyJsonValue1;
            JsonValue value2 = AnyInstance.AnyJsonValue2;

            JsonObject target = new JsonObject { { key1, value1 }, { key2, value2 } };

            JsonValue value;
            Assert.True(target.TryGetValue(key2, out value));
            Assert.Equal(value2, value);

            Assert.False(target.TryGetValue("not a key", out value));
            Assert.Null(value);
        }

        [Fact]
        public void GetValueOrDefaultTest()
        {
            bool boolValue;
            JsonValue target;
            JsonValue jsonValue;

            Person person = AnyInstance.AnyPerson;
            JsonObject jo = JsonValueExtensions.CreateFrom(person) as JsonObject;
            Assert.Equal<int>(person.Age, jo.ValueOrDefault("Age").ReadAs<int>()); // JsonPrimitive

            Assert.Equal<string>(person.Address.ToString(), jo.ValueOrDefault("Address").ReadAsType<Address>().ToString()); // JsonObject
            Assert.Equal<int>(person.Friends.Count, jo.ValueOrDefault("Friends").Count); // JsonArray

            target = jo.ValueOrDefault("Address").ValueOrDefault("City"); // JsonPrimitive
            Assert.NotNull(target);
            Assert.Equal<string>(person.Address.City, target.ReadAs<string>());

            target = jo.ValueOrDefault("Address", "City"); // JsonPrimitive
            Assert.NotNull(target);
            Assert.Equal<string>(person.Address.City, target.ReadAs<string>());

            target = jo.ValueOrDefault("Address").ValueOrDefault("NonExistentProp").ValueOrDefault("NonExistentProp2"); // JsonObject
            Assert.Equal(JsonType.Default, target.JsonType);
            Assert.NotNull(target);
            Assert.False(target.TryReadAs<bool>(out boolValue));
            Assert.True(target.TryReadAs<JsonValue>(out jsonValue));

            target = jo.ValueOrDefault("Address", "NonExistentProp", "NonExistentProp2"); // JsonObject
            Assert.Equal(JsonType.Default, target.JsonType);
            Assert.NotNull(target);
            Assert.False(target.TryReadAs<bool>(out boolValue));
            Assert.True(target.TryReadAs<JsonValue>(out jsonValue));
            Assert.Same(target, jsonValue);
        }

        [Fact]
        public void CountTest()
        {
            string key1 = AnyInstance.AnyString;
            string key2 = AnyInstance.AnyString2;
            JsonValue value1 = AnyInstance.AnyJsonValue1;
            JsonValue value2 = AnyInstance.AnyJsonValue2;

            JsonObject target = new JsonObject();
            Assert.Equal(0, target.Count);
            target.Add(key1, value1);
            Assert.Equal(1, target.Count);
            target.Add(key2, value2);
            Assert.Equal(2, target.Count);
            target.Remove(key2);
            Assert.Equal(1, target.Count);
        }

        [Fact]
        public void ItemTest()
        {
            string key1 = AnyInstance.AnyString;
            string key2 = AnyInstance.AnyString2;
            JsonValue value1 = AnyInstance.AnyJsonValue1;
            JsonValue value2 = AnyInstance.AnyJsonValue2;
            JsonValue value3 = AnyInstance.AnyJsonValue3;

            JsonObject target;

            target = new JsonObject { { key1, value1 }, { key2, value2 } };
            Assert.Equal(value1, target[key1]);
            Assert.Equal(value2, target[key2]);
            target[key1] = value3;
            Assert.Equal(value3, target[key1]);
            Assert.Equal(value2, target[key2]);

            ExceptionHelper.Throws<KeyNotFoundException>(delegate { var o = target["not a key"]; });
            ExceptionHelper.Throws<ArgumentNullException>(delegate { var o = target[null]; });
            ExceptionHelper.Throws<ArgumentNullException>(delegate { target[null] = 123; });
            ExceptionHelper.Throws<ArgumentException>(delegate { target[key1] = AnyInstance.DefaultJsonValue; });
        }

        [Fact]
        public void ChangingEventsTest()
        {
            const string key1 = "first";
            const string key2 = "second";
            const string key3 = "third";
            const string key4 = "fourth";
            const string key5 = "fifth";
            JsonObject jo = new JsonObject
            {
                { key1, AnyInstance.AnyString },
                { key2, AnyInstance.AnyBool },
                { key3, null },
            };

            TestEvents(
                jo,
                obj => obj.Add(key4, 1),
                new List<Tuple<bool, JsonValue, JsonValueChangeEventArgs>>
                {
                    new Tuple<bool, JsonValue, JsonValueChangeEventArgs>(true, jo, new JsonValueChangeEventArgs(1, JsonValueChange.Add, key4)),
                    new Tuple<bool, JsonValue, JsonValueChangeEventArgs>(false, jo, new JsonValueChangeEventArgs(1, JsonValueChange.Add, key4)),
                });

            TestEvents(
                jo,
                obj => obj[key2] = 2,
                new List<Tuple<bool, JsonValue, JsonValueChangeEventArgs>>
                {
                    new Tuple<bool, JsonValue, JsonValueChangeEventArgs>(true, jo, new JsonValueChangeEventArgs(2, JsonValueChange.Replace, key2)),
                    new Tuple<bool, JsonValue, JsonValueChangeEventArgs>(false, jo, new JsonValueChangeEventArgs(AnyInstance.AnyBool, JsonValueChange.Replace, key2)),
                });

            TestEvents(
                jo,
                obj => obj[key5] = 3,
                new List<Tuple<bool, JsonValue, JsonValueChangeEventArgs>>
                {
                    new Tuple<bool, JsonValue, JsonValueChangeEventArgs>(true, jo, new JsonValueChangeEventArgs(3, JsonValueChange.Add, key5)),
                    new Tuple<bool, JsonValue, JsonValueChangeEventArgs>(false, jo, new JsonValueChangeEventArgs(3, JsonValueChange.Add, key5)),
                });

            jo.Remove(key4);
            jo.Remove(key5);

            TestEvents(
                jo,
                obj => obj.AddRange(new JsonObject { { key4, AnyInstance.AnyString }, { key5, AnyInstance.AnyDouble } }),
                new List<Tuple<bool, JsonValue, JsonValueChangeEventArgs>>
                {
                    new Tuple<bool, JsonValue, JsonValueChangeEventArgs>(true, jo, new JsonValueChangeEventArgs(AnyInstance.AnyString, JsonValueChange.Add, key4)),
                    new Tuple<bool, JsonValue, JsonValueChangeEventArgs>(true, jo, new JsonValueChangeEventArgs(AnyInstance.AnyDouble, JsonValueChange.Add, key5)),
                    new Tuple<bool, JsonValue, JsonValueChangeEventArgs>(false, jo, new JsonValueChangeEventArgs(AnyInstance.AnyString, JsonValueChange.Add, key4)),
                    new Tuple<bool, JsonValue, JsonValueChangeEventArgs>(false, jo, new JsonValueChangeEventArgs(AnyInstance.AnyDouble, JsonValueChange.Add, key5)),
                });

            TestEvents(
                jo,
                obj => obj.Remove(key5),
                new List<Tuple<bool, JsonValue, JsonValueChangeEventArgs>>
                {
                    new Tuple<bool, JsonValue, JsonValueChangeEventArgs>(true, jo, new JsonValueChangeEventArgs(AnyInstance.AnyDouble, JsonValueChange.Remove, key5)),
                    new Tuple<bool, JsonValue, JsonValueChangeEventArgs>(false, jo, new JsonValueChangeEventArgs(AnyInstance.AnyDouble, JsonValueChange.Remove, key5)),
                });

            TestEvents(
                jo,
                obj => obj.Remove("not there"),
                new List<Tuple<bool, JsonValue, JsonValueChangeEventArgs>>());

            jo = new JsonObject { { key1, 1 }, { key2, 2 }, { key3, 3 } };

            TestEvents(
                jo,
                obj => obj.Clear(),
                new List<Tuple<bool, JsonValue, JsonValueChangeEventArgs>>
                {
                    new Tuple<bool, JsonValue, JsonValueChangeEventArgs>(true, jo, new JsonValueChangeEventArgs(null, JsonValueChange.Clear, null)),
                    new Tuple<bool, JsonValue, JsonValueChangeEventArgs>(false, jo, new JsonValueChangeEventArgs(null, JsonValueChange.Clear, null)),
                });

            jo = new JsonObject { { key1, 1 }, { key2, 2 }, { key3, 3 } };
            TestEvents(
                jo,
                obj => ((IDictionary<string, JsonValue>)obj).Remove(new KeyValuePair<string, JsonValue>(key2, jo[key2])),
                new List<Tuple<bool, JsonValue, JsonValueChangeEventArgs>>
                {
                    new Tuple<bool, JsonValue, JsonValueChangeEventArgs>(true, jo, new JsonValueChangeEventArgs(2, JsonValueChange.Remove, key2)),
                    new Tuple<bool, JsonValue, JsonValueChangeEventArgs>(false, jo, new JsonValueChangeEventArgs(2, JsonValueChange.Remove, key2)),
                });

            TestEvents(
                jo,
                obj => ((IDictionary<string, JsonValue>)obj).Remove(new KeyValuePair<string, JsonValue>("key not in object", jo[key1])),
                new List<Tuple<bool, JsonValue, JsonValueChangeEventArgs>>
                {
                });

            TestEvents(
                jo,
                obj => ((IDictionary<string, JsonValue>)obj).Remove(new KeyValuePair<string, JsonValue>(key1, "different object")),
                new List<Tuple<bool, JsonValue, JsonValueChangeEventArgs>>
                {
                });

            ExceptionHelper.Throws<ArgumentNullException>(() => new JsonValueChangeEventArgs(1, JsonValueChange.Add, null));
        }

        [Fact]
        public void NestedChangingEventTest()
        {
            const string key1 = "first";

            JsonObject target = new JsonObject { { key1, new JsonArray { 1, 2 } } };
            JsonArray child = target[key1] as JsonArray;
            TestEvents(
                target,
                obj => ((JsonArray)obj[key1]).Add(5),
                new List<Tuple<bool, JsonValue, JsonValueChangeEventArgs>>());

            target = new JsonObject();
            child = new JsonArray(1, 2);
            TestEvents(
                target,
                obj =>
                {
                    obj.Add(key1, child);
                    ((JsonArray)obj[key1]).Add(5);
                },
                new List<Tuple<bool, JsonValue, JsonValueChangeEventArgs>>
                {
                    new Tuple<bool, JsonValue, JsonValueChangeEventArgs>(true, target, new JsonValueChangeEventArgs(child, JsonValueChange.Add, key1)),
                    new Tuple<bool, JsonValue, JsonValueChangeEventArgs>(false, target, new JsonValueChangeEventArgs(child, JsonValueChange.Add, key1)),
                });
        }

        [Fact]
        public void MultipleListenersTest()
        {
            const string key1 = "first";
            const string key2 = "second";
            const string key3 = "third";

            for (int changingListeners = 0; changingListeners <= 2; changingListeners++)
            {
                for (int changedListeners = 0; changedListeners <= 2; changedListeners++)
                {
                    JsonArrayTest.MultipleListenersTestHelper<JsonObject>(
                        () => new JsonObject { { key1, 1 }, { key2, 2 } },
                        delegate(JsonObject obj)
                        {
                            obj[key2] = "hello";
                            obj.Remove(key1);
                            obj.Add(key3, "world");
                            obj.Clear();
                        },
                        new List<JsonValueChangeEventArgs>
                        {
                            new JsonValueChangeEventArgs("hello", JsonValueChange.Replace, key2),
                            new JsonValueChangeEventArgs(1, JsonValueChange.Remove, key1),
                            new JsonValueChangeEventArgs("world", JsonValueChange.Add, key3),
                            new JsonValueChangeEventArgs(null, JsonValueChange.Clear, null),
                        },
                        new List<JsonValueChangeEventArgs>
                        {
                            new JsonValueChangeEventArgs(2, JsonValueChange.Replace, key2),
                            new JsonValueChangeEventArgs(1, JsonValueChange.Remove, key1),
                            new JsonValueChangeEventArgs("world", JsonValueChange.Add, key3),
                            new JsonValueChangeEventArgs(null, JsonValueChange.Clear, null),
                        },
                        changingListeners,
                        changedListeners);
                }
            }
        }

        [Fact]
        public void JsonTypeTest()
        {
            JsonObject target = AnyInstance.AnyJsonObject;
            Assert.Equal(JsonType.Object, target.JsonType);
        }

        [Fact]
        public void KeysTest()
        {
            string key1 = AnyInstance.AnyString;
            string key2 = AnyInstance.AnyString2;
            JsonValue value1 = AnyInstance.AnyJsonValue1;
            JsonValue value2 = AnyInstance.AnyJsonValue2;

            JsonObject target = new JsonObject { { key1, value1 }, { key2, value2 } };

            List<string> expected = new List<string> { key1, key2 };
            List<string> actual = new List<string>(target.Keys);

            Assert.Equal(expected.Count, actual.Count);

            expected.Sort();
            actual.Sort();
            for (int i = 0; i < expected.Count; i++)
            {
                Assert.Equal(expected[i], actual[i]);
            }
        }

        [Fact]
        public void IsReadOnlyTest()
        {
            JsonObject target = AnyInstance.AnyJsonObject;
            Assert.False(((ICollection<KeyValuePair<string, JsonValue>>)target).IsReadOnly);
        }

        [Fact]
        public void ValuesTest()
        {
            string key1 = AnyInstance.AnyString;
            string key2 = AnyInstance.AnyString2;
            JsonValue value1 = AnyInstance.AnyJsonValue1;
            JsonValue value2 = AnyInstance.AnyJsonValue2;

            JsonObject target = new JsonObject { { key1, value1 }, { key2, value2 } };

            List<JsonValue> values = new List<JsonValue>(target.Values);
            Assert.Equal(2, values.Count);
            bool value1IsFirst = value1 == values[0];
            Assert.True(value1IsFirst || value1 == values[1]);
            Assert.Equal(value2, values[value1IsFirst ? 1 : 0]);
        }

        private static void ValidateJsonObjectItems(JsonObject jsonObject, params object[] keyValuePairs)
        {
            Dictionary<string, JsonValue> expected = new Dictionary<string, JsonValue>();
            Assert.True((keyValuePairs.Length % 2) == 0, "Test error");
            for (int i = 0; i < keyValuePairs.Length; i += 2)
            {
                Assert.IsType<String>(keyValuePairs[i]);
                Assert.IsAssignableFrom<JsonValue>(keyValuePairs[i + 1]);
                expected.Add((string)keyValuePairs[i], (JsonValue)keyValuePairs[i + 1]);
            }
        }

        private static void ValidateJsonObjectItems(JsonObject jsonObject, Dictionary<string, JsonValue> expected)
        {
            Assert.Equal(expected.Count, jsonObject.Count);
            foreach (string key in expected.Keys)
            {
                Assert.True(jsonObject.ContainsKey(key));
                Assert.Equal(expected[key], jsonObject[key]);
            }
        }

        private static void TestEvents(JsonObject obj, Action<JsonObject> actionToTriggerEvent, List<Tuple<bool, JsonValue, JsonValueChangeEventArgs>> expectedEvents)
        {
            JsonArrayTest.TestEvents<JsonObject>(obj, actionToTriggerEvent, expectedEvents);
        }
    }
}
