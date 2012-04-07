// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization.Json;
using Xunit;

namespace System.Json
{
    public class JsonArrayTest
    {
        [Fact]
        public void JsonArrayConstructorParamsTest()
        {
            JsonArray target;

            target = new JsonArray();
            Assert.Equal(0, target.Count);

            target = new JsonArray(null);
            Assert.Equal(0, target.Count);

            List<JsonValue> items = new List<JsonValue> { AnyInstance.AnyJsonValue1, AnyInstance.AnyJsonValue2 };
            target = new JsonArray(items.ToArray());
            ValidateJsonArrayItems(target, items);

            target = new JsonArray(items[0], items[1]);
            ValidateJsonArrayItems(target, items);

            // Invalide tests
            items.Add(AnyInstance.DefaultJsonValue);
            ExceptionHelper.Throws<ArgumentException>(() => new JsonArray(items.ToArray()));
            ExceptionHelper.Throws<ArgumentException>(() => new JsonArray(items[0], items[1], items[2]));
        }

        [Fact]
        public void JsonArrayConstructorEnumTest()
        {
            List<JsonValue> items = new List<JsonValue> { AnyInstance.AnyJsonValue1, AnyInstance.AnyJsonValue2, AnyInstance.AnyJsonValue3 };
            JsonArray target;

            target = new JsonArray(items);
            ValidateJsonArrayItems(target, items);

            ExceptionHelper.Throws<ArgumentNullException>(() => new JsonArray((IEnumerable<JsonValue>)null));

            items.Add(AnyInstance.DefaultJsonValue);
            ExceptionHelper.Throws<ArgumentException>(() => new JsonArray(items));
        }

        [Fact]
        public void AddTest()
        {
            JsonArray target = new JsonArray();
            JsonValue item = AnyInstance.AnyJsonValue1;
            Assert.False(target.Contains(item));
            target.Add(item);
            Assert.Equal(1, target.Count);
            Assert.Equal(item, target[0]);
            Assert.True(target.Contains(item));

            ExceptionHelper.Throws<ArgumentException>(() => target.Add(AnyInstance.DefaultJsonValue));
        }

        [Fact]
        public void AddRangeEnumTest()
        {
            List<JsonValue> items = new List<JsonValue> { AnyInstance.AnyJsonValue1, AnyInstance.AnyJsonValue2 };

            JsonArray target = new JsonArray();
            target.AddRange(items);
            ValidateJsonArrayItems(target, items);

            ExceptionHelper.Throws<ArgumentNullException>(() => new JsonArray().AddRange((IEnumerable<JsonValue>)null));

            items.Add(AnyInstance.DefaultJsonValue);
            ExceptionHelper.Throws<ArgumentException>(() => new JsonArray().AddRange(items));
        }

        [Fact]
        public void AddRangeParamsTest()
        {
            List<JsonValue> items = new List<JsonValue> { AnyInstance.AnyJsonValue1, AnyInstance.AnyJsonValue2, AnyInstance.AnyJsonValue3 };
            JsonArray target;

            target = new JsonArray();
            target.AddRange(items[0], items[1], items[2]);
            ValidateJsonArrayItems(target, items);

            target = new JsonArray();
            target.AddRange(items.ToArray());
            ValidateJsonArrayItems(target, items);

            target.AddRange();
            ValidateJsonArrayItems(target, items);

            items.Add(AnyInstance.DefaultJsonValue);
            ExceptionHelper.Throws<ArgumentException>(() => new JsonArray().AddRange(items[items.Count - 1]));
            ExceptionHelper.Throws<ArgumentException>(() => new JsonArray().AddRange(items));
        }

        [Fact]
        public void ClearTest()
        {
            JsonArray target = new JsonArray(AnyInstance.AnyJsonValue1, AnyInstance.AnyJsonValue2);
            Assert.Equal(2, target.Count);
            target.Clear();
            Assert.Equal(0, target.Count);
        }

        [Fact]
        public void ContainsTest()
        {
            JsonValue item1 = AnyInstance.AnyJsonValue1;
            JsonValue item2 = AnyInstance.AnyJsonValue2;
            JsonArray target = new JsonArray(item1);
            Assert.True(target.Contains(item1));
            Assert.False(target.Contains(item2));

            target.Add(item2);
            Assert.True(target.Contains(item1));
            Assert.True(target.Contains(item2));

            target.Remove(item1);
            Assert.False(target.Contains(item1));
            Assert.True(target.Contains(item2));
        }

        [Fact]
        public void ReadAsComplexTypeTest()
        {
            JsonArray target = new JsonArray(AnyInstance.AnyInt, AnyInstance.AnyInt + 1, AnyInstance.AnyInt + 2);
            int[] intArray1 = (int[])target.ReadAsType(typeof(int[]));
            int[] intArray2 = target.ReadAsType<int[]>();

            Assert.Equal(((JsonArray)target).Count, intArray1.Length);
            Assert.Equal(((JsonArray)target).Count, intArray2.Length);

            for (int i = 0; i < intArray1.Length; i++)
            {
                Assert.Equal(AnyInstance.AnyInt + i, intArray1[i]);
                Assert.Equal(AnyInstance.AnyInt + i, intArray2[i]);
            }
        }

        [Fact]
        public void CopyToTest()
        {
            JsonValue item1 = AnyInstance.AnyJsonValue1;
            JsonValue item2 = AnyInstance.AnyJsonValue2;
            JsonArray target = new JsonArray(item1, item2);
            JsonValue[] array = new JsonValue[target.Count + 1];

            target.CopyTo(array, 0);
            Assert.Equal(item1, array[0]);
            Assert.Equal(item2, array[1]);

            target.CopyTo(array, 1);
            Assert.Equal(item1, array[1]);
            Assert.Equal(item2, array[2]);

            ExceptionHelper.Throws<ArgumentNullException>(() => target.CopyTo(null, 0));
            ExceptionHelper.Throws<ArgumentOutOfRangeException>(() => target.CopyTo(array, -1));
            ExceptionHelper.Throws<ArgumentException>(() => target.CopyTo(array, array.Length - target.Count + 1));
        }

        [Fact]
        public void IndexOfTest()
        {
            JsonValue item1 = AnyInstance.AnyJsonValue1;
            JsonValue item2 = AnyInstance.AnyJsonValue2;
            JsonValue item3 = AnyInstance.AnyJsonValue3;
            JsonArray target = new JsonArray(item1, item2);

            Assert.Equal(0, target.IndexOf(item1));
            Assert.Equal(1, target.IndexOf(item2));
            Assert.Equal(-1, target.IndexOf(item3));
        }

        [Fact]
        public void InsertTest()
        {
            JsonValue item1 = AnyInstance.AnyJsonValue1;
            JsonValue item2 = AnyInstance.AnyJsonValue2;
            JsonValue item3 = AnyInstance.AnyJsonValue3;
            JsonArray target = new JsonArray(item1);

            Assert.Equal(1, target.Count);
            target.Insert(0, item2);
            Assert.Equal(2, target.Count);
            Assert.Equal(item2, target[0]);
            Assert.Equal(item1, target[1]);

            target.Insert(1, item3);
            Assert.Equal(3, target.Count);
            Assert.Equal(item2, target[0]);
            Assert.Equal(item3, target[1]);
            Assert.Equal(item1, target[2]);

            target.Insert(target.Count, item2);
            Assert.Equal(4, target.Count);
            Assert.Equal(item2, target[0]);
            Assert.Equal(item3, target[1]);
            Assert.Equal(item1, target[2]);
            Assert.Equal(item2, target[3]);

            ExceptionHelper.Throws<ArgumentOutOfRangeException>(() => target.Insert(-1, item3));
            ExceptionHelper.Throws<ArgumentOutOfRangeException>(() => target.Insert(target.Count + 1, item1));
            ExceptionHelper.Throws<ArgumentException>(() => target.Insert(0, AnyInstance.DefaultJsonValue));
        }

        [Fact]
        public void RemoveTest()
        {
            JsonValue item1 = AnyInstance.AnyJsonValue1;
            JsonValue item2 = AnyInstance.AnyJsonValue2;
            JsonValue item3 = AnyInstance.AnyJsonValue3;
            JsonArray target = new JsonArray(item1, item2, item3);

            Assert.True(target.Remove(item2));
            Assert.Equal(2, target.Count);
            Assert.Equal(item1, target[0]);
            Assert.Equal(item3, target[1]);

            Assert.False(target.Remove(item2));
            Assert.Equal(2, target.Count);
        }

        [Fact]
        public void RemoveAtTest()
        {
            JsonValue item1 = AnyInstance.AnyJsonValue1;
            JsonValue item2 = AnyInstance.AnyJsonValue2;
            JsonValue item3 = AnyInstance.AnyJsonValue3;
            JsonArray target = new JsonArray(item1, item2, item3);

            target.RemoveAt(1);
            Assert.Equal(2, target.Count);
            Assert.Equal(item1, target[0]);
            Assert.Equal(item3, target[1]);

            ExceptionHelper.Throws<ArgumentOutOfRangeException>(() => target.RemoveAt(-1));
            ExceptionHelper.Throws<ArgumentOutOfRangeException>(() => target.RemoveAt(target.Count));
        }

        [Fact]
        public void ToStringTest()
        {
            JsonArray target;
            JsonValue item1 = AnyInstance.AnyJsonValue1;
            JsonValue item2 = null;
            JsonValue item3 = AnyInstance.AnyJsonValue2;

            target = new JsonArray(item1, item2, item3);

            string expected = String.Format(CultureInfo.InvariantCulture, "[{0},null,{1}]", item1.ToString(), item3.ToString());
            Assert.Equal(expected, target.ToString());

            string json = "[\r\n  \"hello\",\r\n  null,\r\n  [\r\n    1,\r\n    2,\r\n    3\r\n  ]\r\n]";
            target = JsonValue.Parse(json) as JsonArray;

            Assert.Equal<string>(json.Replace("\r\n", "").Replace(" ", ""), target.ToString());
        }

        [Fact]
        public void GetEnumeratorTest()
        {
            JsonValue item1 = AnyInstance.AnyJsonValue1;
            JsonValue item2 = AnyInstance.AnyJsonValue2;

            IEnumerable<JsonValue> target = new JsonArray(item1, item2);
            IEnumerator<JsonValue> enumerator = target.GetEnumerator();
            Assert.True(enumerator.MoveNext());
            Assert.Equal(item1, enumerator.Current);
            Assert.True(enumerator.MoveNext());
            Assert.Equal(item2, enumerator.Current);
            Assert.False(enumerator.MoveNext());
        }

        [Fact]
        public void GetEnumeratorTest1()
        {
            JsonValue item1 = AnyInstance.AnyJsonValue1;
            JsonValue item2 = AnyInstance.AnyJsonValue2;

            IEnumerable target = new JsonArray(item1, item2);
            IEnumerator enumerator = target.GetEnumerator();
            Assert.True(enumerator.MoveNext());
            Assert.Equal(item1, enumerator.Current);
            Assert.True(enumerator.MoveNext());
            Assert.Equal(item2, enumerator.Current);
            Assert.False(enumerator.MoveNext());
        }

        [Fact]
        public void CountTest()
        {
            JsonValue item1 = AnyInstance.AnyJsonValue1;
            JsonValue item2 = AnyInstance.AnyJsonValue2;

            JsonArray target = new JsonArray();
            Assert.Equal(0, target.Count);
            target.Add(item1);
            Assert.Equal(1, target.Count);
            target.Add(item2);
            Assert.Equal(2, target.Count);
            target.Remove(item1);
            Assert.Equal(1, target.Count);
        }

        [Fact]
        public void IsReadOnlyTest()
        {
            JsonArray target = AnyInstance.AnyJsonArray;
            Assert.False(target.IsReadOnly);
        }

        [Fact]
        public void ItemTest()
        {
            JsonValue item1 = AnyInstance.AnyJsonValue1;
            JsonValue item2 = AnyInstance.AnyJsonValue2;

            JsonArray target = new JsonArray(item1);
            Assert.Equal(item1, target[0]);
            target[0] = item2;
            Assert.Equal(item2, target[0]);
            Assert.Equal(item2, target[(short)0]);
            Assert.Equal(item2, target[(ushort)0]);
            Assert.Equal(item2, target[(byte)0]);
            Assert.Equal(item2, target[(sbyte)0]);
            Assert.Equal(item2, target[(char)0]);

            ExceptionHelper.Throws<ArgumentOutOfRangeException>(delegate { var i = target[-1]; });
            ExceptionHelper.Throws<ArgumentOutOfRangeException>(delegate { var i = target[target.Count]; });
            ExceptionHelper.Throws<ArgumentOutOfRangeException>(delegate { target[-1] = AnyInstance.AnyJsonValue1; });
            ExceptionHelper.Throws<ArgumentOutOfRangeException>(delegate { target[target.Count] = AnyInstance.AnyJsonValue2; });
            ExceptionHelper.Throws<ArgumentException>(delegate { target[0] = AnyInstance.DefaultJsonValue; });
        }

        [Fact]
        public void ChangingEventsTest()
        {
            JsonArray ja = new JsonArray(AnyInstance.AnyInt, AnyInstance.AnyBool, null);
            TestEvents(
                ja,
                arr => arr.Add(1),
                new List<Tuple<bool, JsonValue, JsonValueChangeEventArgs>>
                {
                    new Tuple<bool, JsonValue, JsonValueChangeEventArgs>(true, ja, new JsonValueChangeEventArgs(1, JsonValueChange.Add, 3)),
                    new Tuple<bool, JsonValue, JsonValueChangeEventArgs>(false, ja, new JsonValueChangeEventArgs(1, JsonValueChange.Add, 3)),
                });

            TestEvents(
                ja,
                arr => arr.AddRange(AnyInstance.AnyString, AnyInstance.AnyDouble),
                new List<Tuple<bool, JsonValue, JsonValueChangeEventArgs>>
                {
                    new Tuple<bool, JsonValue, JsonValueChangeEventArgs>(true, ja, new JsonValueChangeEventArgs(AnyInstance.AnyString, JsonValueChange.Add, 4)),
                    new Tuple<bool, JsonValue, JsonValueChangeEventArgs>(true, ja, new JsonValueChangeEventArgs(AnyInstance.AnyDouble, JsonValueChange.Add, 5)),
                    new Tuple<bool, JsonValue, JsonValueChangeEventArgs>(false, ja, new JsonValueChangeEventArgs(AnyInstance.AnyString, JsonValueChange.Add, 4)),
                    new Tuple<bool, JsonValue, JsonValueChangeEventArgs>(false, ja, new JsonValueChangeEventArgs(AnyInstance.AnyDouble, JsonValueChange.Add, 5)),
                });

            TestEvents(
                ja,
                arr => arr[1] = 2,
                new List<Tuple<bool, JsonValue, JsonValueChangeEventArgs>>
                {
                    new Tuple<bool, JsonValue, JsonValueChangeEventArgs>(true, ja, new JsonValueChangeEventArgs(2, JsonValueChange.Replace, 1)),
                    new Tuple<bool, JsonValue, JsonValueChangeEventArgs>(false, ja, new JsonValueChangeEventArgs(AnyInstance.AnyBool, JsonValueChange.Replace, 1)),
                });

            ja = new JsonArray { 1, 2, 3 };
            TestEvents(
                ja,
                arr => arr.Insert(1, "new value"),
                new List<Tuple<bool, JsonValue, JsonValueChangeEventArgs>>
                {
                    new Tuple<bool, JsonValue, JsonValueChangeEventArgs>(true, ja, new JsonValueChangeEventArgs("new value", JsonValueChange.Add, 1)),
                    new Tuple<bool, JsonValue, JsonValueChangeEventArgs>(false, ja, new JsonValueChangeEventArgs("new value", JsonValueChange.Add, 1)),
                });

            TestEvents(
                ja,
                arr => arr.RemoveAt(1),
                new List<Tuple<bool, JsonValue, JsonValueChangeEventArgs>>
                {
                    new Tuple<bool, JsonValue, JsonValueChangeEventArgs>(true, ja, new JsonValueChangeEventArgs("new value", JsonValueChange.Remove, 1)),
                    new Tuple<bool, JsonValue, JsonValueChangeEventArgs>(false, ja, new JsonValueChangeEventArgs("new value", JsonValueChange.Remove, 1)),
                });

            TestEvents(
                ja,
                arr => arr.Clear(),
                new List<Tuple<bool, JsonValue, JsonValueChangeEventArgs>>
                {
                    new Tuple<bool, JsonValue, JsonValueChangeEventArgs>(true, ja, new JsonValueChangeEventArgs(null, JsonValueChange.Clear, 0)),
                    new Tuple<bool, JsonValue, JsonValueChangeEventArgs>(false, ja, new JsonValueChangeEventArgs(null, JsonValueChange.Clear, 0)),
                });

            ja = new JsonArray(1, 2, 3);
            TestEvents(
                ja,
                arr => arr.Remove(new JsonPrimitive("Not there")),
                new List<Tuple<bool, JsonValue, JsonValueChangeEventArgs>>());

            JsonValue elementInArray = ja[1];
            TestEvents(
                ja,
                arr => arr.Remove(elementInArray),
                new List<Tuple<bool, JsonValue, JsonValueChangeEventArgs>>
                {
                    new Tuple<bool, JsonValue, JsonValueChangeEventArgs>(true, ja, new JsonValueChangeEventArgs(elementInArray, JsonValueChange.Remove, 1)),
                    new Tuple<bool, JsonValue, JsonValueChangeEventArgs>(false, ja, new JsonValueChangeEventArgs(elementInArray, JsonValueChange.Remove, 1)),
                });
        }

        [Fact]
        public void NestedChangingEventTest()
        {
            JsonArray target = new JsonArray { new JsonArray { 1, 2 }, new JsonArray { 3, 4 } };
            JsonArray child = target[1] as JsonArray;
            TestEvents(
                target,
                arr => ((JsonArray)arr[1]).Add(5),
                new List<Tuple<bool, JsonValue, JsonValueChangeEventArgs>>());

            target = new JsonArray();
            child = new JsonArray(1, 2);
            TestEvents(
                target,
                arr =>
                {
                    arr.Add(child);
                    ((JsonArray)arr[0]).Add(5);
                },
                new List<Tuple<bool, JsonValue, JsonValueChangeEventArgs>>
                {
                    new Tuple<bool, JsonValue, JsonValueChangeEventArgs>(true, target, new JsonValueChangeEventArgs(child, JsonValueChange.Add, 0)),
                    new Tuple<bool, JsonValue, JsonValueChangeEventArgs>(false, target, new JsonValueChangeEventArgs(child, JsonValueChange.Add, 0)),
                });
        }

        [Fact]
        public void MultipleListenersTest()
        {
            for (int changingListeners = 0; changingListeners <= 2; changingListeners++)
            {
                for (int changedListeners = 0; changedListeners <= 2; changedListeners++)
                {
                    MultipleListenersTestHelper<JsonArray>(
                        () => new JsonArray(1, 2),
                        delegate(JsonArray arr)
                        {
                            arr[1] = "hello";
                            arr.RemoveAt(0);
                            arr.Add("world");
                            arr.Clear();
                        },
                        new List<JsonValueChangeEventArgs>
                        {
                            new JsonValueChangeEventArgs("hello", JsonValueChange.Replace, 1),
                            new JsonValueChangeEventArgs(1, JsonValueChange.Remove, 0),
                            new JsonValueChangeEventArgs("world", JsonValueChange.Add, 1),
                            new JsonValueChangeEventArgs(null, JsonValueChange.Clear, 0),
                        },
                        new List<JsonValueChangeEventArgs>
                        {
                            new JsonValueChangeEventArgs(2, JsonValueChange.Replace, 1),
                            new JsonValueChangeEventArgs(1, JsonValueChange.Remove, 0),
                            new JsonValueChangeEventArgs("world", JsonValueChange.Add, 1),
                            new JsonValueChangeEventArgs(null, JsonValueChange.Clear, 0),
                        },
                        changingListeners,
                        changedListeners);
                }
            }
        }

        [Fact]
        public void JsonTypeTest()
        {
            JsonArray target = AnyInstance.AnyJsonArray;
            Assert.Equal(JsonType.Array, target.JsonType);
        }

        internal static void TestEvents<JsonValueType>(JsonValueType target, Action<JsonValueType> actionToTriggerEvent, List<Tuple<bool, JsonValue, JsonValueChangeEventArgs>> expectedEvents) where JsonValueType : JsonValue
        {
            List<Tuple<bool, JsonValue, JsonValueChangeEventArgs>> actualEvents = new List<Tuple<bool, JsonValue, JsonValueChangeEventArgs>>();
            EventHandler<JsonValueChangeEventArgs> changingHandler = delegate(object sender, JsonValueChangeEventArgs e)
            {
                actualEvents.Add(new Tuple<bool, JsonValue, JsonValueChangeEventArgs>(true, sender as JsonValue, e));
            };

            EventHandler<JsonValueChangeEventArgs> changedHandler = delegate(object sender, JsonValueChangeEventArgs e)
            {
                actualEvents.Add(new Tuple<bool, JsonValue, JsonValueChangeEventArgs>(false, sender as JsonValue, e));
            };

            target.Changing += new EventHandler<JsonValueChangeEventArgs>(changingHandler);
            target.Changed += new EventHandler<JsonValueChangeEventArgs>(changedHandler);

            actionToTriggerEvent(target);

            target.Changing -= new EventHandler<JsonValueChangeEventArgs>(changingHandler);
            target.Changed -= new EventHandler<JsonValueChangeEventArgs>(changedHandler);

            ValidateExpectedEvents(expectedEvents, actualEvents);
        }

        private static void TestEvents(JsonArray array, Action<JsonArray> actionToTriggerEvent, List<Tuple<bool, JsonValue, JsonValueChangeEventArgs>> expectedEvents)
        {
            TestEvents<JsonArray>(array, actionToTriggerEvent, expectedEvents);
        }

        private static void ValidateExpectedEvents(List<Tuple<bool, JsonValue, JsonValueChangeEventArgs>> expectedEvents, List<Tuple<bool, JsonValue, JsonValueChangeEventArgs>> actualEvents)
        {
            Assert.Equal(expectedEvents.Count, actualEvents.Count);
            for (int i = 0; i < expectedEvents.Count; i++)
            {
                bool expectedIsChanging = expectedEvents[i].Item1;
                bool actualIsChanging = expectedEvents[i].Item1;
                Assert.Equal(expectedIsChanging, actualIsChanging);

                JsonValue expectedSender = expectedEvents[i].Item2;
                JsonValue actualSender = actualEvents[i].Item2;
                Assert.Same(expectedSender, actualSender);

                JsonValueChangeEventArgs expectedEventArgs = expectedEvents[i].Item3;
                JsonValueChangeEventArgs actualEventArgs = actualEvents[i].Item3;
                Assert.Equal(expectedEventArgs.Change, actualEventArgs.Change);
                Assert.Equal(expectedEventArgs.Index, actualEventArgs.Index);
                Assert.Equal(expectedEventArgs.Key, actualEventArgs.Key);

                string expectedChild = expectedEventArgs.Child == null ? "null" : expectedEventArgs.Child.ToString();
                string actualChild = actualEventArgs.Child == null ? "null" : actualEventArgs.Child.ToString();
                Assert.Equal(expectedChild, actualChild);
            }
        }

        internal static void MultipleListenersTestHelper<JsonValueType>(
            Func<JsonValueType> createTarget,
            Action<JsonValueType> actionToTriggerEvents,
            List<JsonValueChangeEventArgs> expectedChangingEventArgs,
            List<JsonValueChangeEventArgs> expectedChangedEventArgs,
            int changingListeners,
            int changedListeners) where JsonValueType : JsonValue
        {
            Console.WriteLine("Testing events on a {0} for {1} changING listeners and {2} changED listeners", typeof(JsonValueType).Name, changingListeners, changedListeners);
            JsonValueType target = createTarget();
            List<Tuple<bool, JsonValue, JsonValueChangeEventArgs>>[] actualChangingEvents = new List<Tuple<bool, JsonValue, JsonValueChangeEventArgs>>[changingListeners];
            List<Tuple<bool, JsonValue, JsonValueChangeEventArgs>>[] actualChangedEvents = new List<Tuple<bool, JsonValue, JsonValueChangeEventArgs>>[changedListeners];
            List<Tuple<bool, JsonValue, JsonValueChangeEventArgs>> expectedChangingEvents = new List<Tuple<bool, JsonValue, JsonValueChangeEventArgs>>(
                expectedChangingEventArgs.Select((args) => new Tuple<bool, JsonValue, JsonValueChangeEventArgs>(true, target, args)));
            List<Tuple<bool, JsonValue, JsonValueChangeEventArgs>> expectedChangedEvents = new List<Tuple<bool, JsonValue, JsonValueChangeEventArgs>>(
                expectedChangedEventArgs.Select((args) => new Tuple<bool, JsonValue, JsonValueChangeEventArgs>(false, target, args)));

            for (int i = 0; i < changingListeners; i++)
            {
                actualChangingEvents[i] = new List<Tuple<bool, JsonValue, JsonValueChangeEventArgs>>();
                int index = i;
                target.Changing += delegate(object sender, JsonValueChangeEventArgs e)
                {
                    actualChangingEvents[index].Add(new Tuple<bool, JsonValue, JsonValueChangeEventArgs>(true, sender as JsonValue, e));
                };
            }

            for (int i = 0; i < changedListeners; i++)
            {
                actualChangedEvents[i] = new List<Tuple<bool, JsonValue, JsonValueChangeEventArgs>>();
                int index = i;
                target.Changed += delegate(object sender, JsonValueChangeEventArgs e)
                {
                    actualChangedEvents[index].Add(new Tuple<bool, JsonValue, JsonValueChangeEventArgs>(false, sender as JsonValue, e));
                };
            }

            actionToTriggerEvents(target);

            for (int i = 0; i < changingListeners; i++)
            {
                Console.WriteLine("Validating Changing events for listener {0}", i);
                ValidateExpectedEvents(expectedChangingEvents, actualChangingEvents[i]);
            }

            for (int i = 0; i < changedListeners; i++)
            {
                Console.WriteLine("Validating Changed events for listener {0}", i);
                ValidateExpectedEvents(expectedChangedEvents, actualChangedEvents[i]);
            }
        }

        private static void ValidateJsonArrayItems(JsonArray jsonArray, IEnumerable<JsonValue> expectedItems)
        {
            List<JsonValue> expected = new List<JsonValue>(expectedItems);
            Assert.Equal(expected.Count, jsonArray.Count);
            for (int i = 0; i < expected.Count; i++)
            {
                Assert.Equal(expected[i], jsonArray[i]);
            }
        }
    }
}
