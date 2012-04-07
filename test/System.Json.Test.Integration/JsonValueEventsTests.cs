// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace System.Json
{
    /// <summary>
    /// Tests for events on <see cref="JsonValue"/> instances.
    /// </summary>
    public class JsonValueEventsTests
    {
        /// <summary>
        /// Events tests for JsonArray, test all method the causes change and all change type and validate changing/changed child and sub/unsub
        /// </summary>
        [Fact]
        public void JsonArrayEventsTest()
        {
            int seed = 1;
            const int maxArrayLength = 1024;
            Random rand = new Random(seed);
            JsonArray ja = SpecialJsonValueHelper.CreatePrePopulatedJsonArray(seed, rand.Next(maxArrayLength));
            int addPosition = ja.Count;
            JsonValue insertValue = SpecialJsonValueHelper.GetRandomJsonPrimitives(seed);

            TestEvents(
                ja,
                arr => arr.Add(insertValue),
                new List<Tuple<bool, JsonValue, JsonValueChangeEventArgs>>
                {
                    new Tuple<bool, JsonValue, JsonValueChangeEventArgs>(true, ja, new JsonValueChangeEventArgs(insertValue, JsonValueChange.Add, addPosition)),
                    new Tuple<bool, JsonValue, JsonValueChangeEventArgs>(false, ja, new JsonValueChangeEventArgs(insertValue, JsonValueChange.Add, addPosition)),
                });

            addPosition = ja.Count;
            JsonValue jv1 = SpecialJsonValueHelper.GetRandomJsonPrimitives(seed);
            JsonValue jv2 = SpecialJsonValueHelper.GetRandomJsonPrimitives(seed);
            TestEvents(
                ja,
                arr => arr.AddRange(jv1, jv2),
                new List<Tuple<bool, JsonValue, JsonValueChangeEventArgs>>
                    {
                        new Tuple<bool, JsonValue, JsonValueChangeEventArgs>(true, ja,
                                                                             new JsonValueChangeEventArgs(
                                                                                 jv1,
                                                                                 JsonValueChange.Add, addPosition)),
                        new Tuple<bool, JsonValue, JsonValueChangeEventArgs>(true, ja,
                                                                             new JsonValueChangeEventArgs(
                                                                                 jv2,
                                                                                 JsonValueChange.Add, addPosition + 1)),
                        new Tuple<bool, JsonValue, JsonValueChangeEventArgs>(false, ja,
                                                                             new JsonValueChangeEventArgs(
                                                                                 jv1,
                                                                                 JsonValueChange.Add, addPosition)),
                        new Tuple<bool, JsonValue, JsonValueChangeEventArgs>(false, ja,
                                                                             new JsonValueChangeEventArgs(
                                                                                 jv2,
                                                                                 JsonValueChange.Add, addPosition + 1)),
                    });

            int replacePosition = rand.Next(ja.Count - 1);
            JsonValue oldValue = ja[replacePosition];
            JsonValue newValue = SpecialJsonValueHelper.GetRandomJsonPrimitives(seed);
            TestEvents(
                ja,
                arr => arr[replacePosition] = newValue,
                new List<Tuple<bool, JsonValue, JsonValueChangeEventArgs>>
                {
                    new Tuple<bool, JsonValue, JsonValueChangeEventArgs>(true, ja, new JsonValueChangeEventArgs(newValue, JsonValueChange.Replace, replacePosition)),
                    new Tuple<bool, JsonValue, JsonValueChangeEventArgs>(false, ja, new JsonValueChangeEventArgs(oldValue, JsonValueChange.Replace, replacePosition)),
                });

            int insertPosition = rand.Next(ja.Count - 1);
            insertValue = SpecialJsonValueHelper.GetRandomJsonPrimitives(seed);

            TestEvents(
                ja,
                arr => arr.Insert(insertPosition, insertValue),
                new List<Tuple<bool, JsonValue, JsonValueChangeEventArgs>>
                {
                    new Tuple<bool, JsonValue, JsonValueChangeEventArgs>(true, ja, new JsonValueChangeEventArgs(insertValue, JsonValueChange.Add, insertPosition)),
                    new Tuple<bool, JsonValue, JsonValueChangeEventArgs>(false, ja, new JsonValueChangeEventArgs(insertValue, JsonValueChange.Add, insertPosition)),
                });

            TestEvents(
                ja,
                arr => arr.RemoveAt(insertPosition),
                new List<Tuple<bool, JsonValue, JsonValueChangeEventArgs>>
                {
                    new Tuple<bool, JsonValue, JsonValueChangeEventArgs>(true, ja, new JsonValueChangeEventArgs(insertValue, JsonValueChange.Remove, insertPosition)),
                    new Tuple<bool, JsonValue, JsonValueChangeEventArgs>(false, ja, new JsonValueChangeEventArgs(insertValue, JsonValueChange.Remove, insertPosition)),
                });

            ja.Insert(0, insertValue);
            TestEvents(
                ja,
                arr => arr.Remove(insertValue),
                new List<Tuple<bool, JsonValue, JsonValueChangeEventArgs>>
                {
                    new Tuple<bool, JsonValue, JsonValueChangeEventArgs>(true, ja, new JsonValueChangeEventArgs(insertValue, JsonValueChange.Remove, 0)),
                    new Tuple<bool, JsonValue, JsonValueChangeEventArgs>(false, ja, new JsonValueChangeEventArgs(insertValue, JsonValueChange.Remove, 0)),
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

        /// <summary>
        /// Tests for events for <see cref="JsonValue"/> instances when using the dynamic programming.
        /// </summary>
        [Fact]
        public void DynamicEventsTest()
        {
            int seed = 1;
            int maxObj = 10;
            JsonArray ja = new JsonArray();
            dynamic d = ja.AsDynamic();
            TestEventsDynamic(
                d,
                (Action<dynamic>)(arr => arr.Add(1)),
                new List<Tuple<bool, JsonValue, JsonValueChangeEventArgs>>
                {
                    new Tuple<bool, JsonValue, JsonValueChangeEventArgs>(true, ja, new JsonValueChangeEventArgs(1, JsonValueChange.Add, 0)),
                    new Tuple<bool, JsonValue, JsonValueChangeEventArgs>(false, ja, new JsonValueChangeEventArgs(1, JsonValueChange.Add, 0)),
                });

            const string key1 = "first";
            const string key2 = "second";
            JsonObject jo = new JsonObject
            {
                { key1, SpecialJsonValueHelper.GetRandomJsonPrimitives(seed) },
            };

            JsonObject objectToAdd = SpecialJsonValueHelper.CreateRandomPopulatedJsonObject(seed, maxObj);
            dynamic d2 = jo.AsDynamic();
            TestEventsDynamic(
                d2,
                (Action<dynamic>)(obj => obj[key2] = objectToAdd),
                new List<Tuple<bool, JsonValue, JsonValueChangeEventArgs>>
                {
                    new Tuple<bool, JsonValue, JsonValueChangeEventArgs>(true, jo, new JsonValueChangeEventArgs(objectToAdd, JsonValueChange.Add, key2)),
                    new Tuple<bool, JsonValue, JsonValueChangeEventArgs>(false, jo, new JsonValueChangeEventArgs(objectToAdd, JsonValueChange.Add, key2)),
                });

            TestEventsDynamic(
                d2,
                (Action<dynamic>)(obj => obj[key2] = objectToAdd),
                new List<Tuple<bool, JsonValue, JsonValueChangeEventArgs>>
                {
                    new Tuple<bool, JsonValue, JsonValueChangeEventArgs>(true, jo, new JsonValueChangeEventArgs(objectToAdd, JsonValueChange.Replace, key2)),
                    new Tuple<bool, JsonValue, JsonValueChangeEventArgs>(false, jo, new JsonValueChangeEventArgs(objectToAdd, JsonValueChange.Replace, key2)),
                });
        }

        /// <summary>
        /// Tests for events in <see cref="JsonObject"/> instances.
        /// </summary>
        [Fact]
        public void JsonObjectEventsTest()
        {
            int seed = 1;
            const int maxObj = 10;

            const string key1 = "first";
            const string key2 = "second";
            const string key3 = "third";
            const string key4 = "fourth";
            const string key5 = "fifth";
            JsonObject jo = new JsonObject
            {
                { key1, SpecialJsonValueHelper.GetRandomJsonPrimitives(seed) },
                { key2, SpecialJsonValueHelper.GetRandomJsonPrimitives(seed) },
                { key3, null },
            };

            JsonObject objToAdd = SpecialJsonValueHelper.CreateRandomPopulatedJsonObject(seed, maxObj);
            TestEvents(
                jo,
                obj => obj.Add(key4, objToAdd),
                new List<Tuple<bool, JsonValue, JsonValueChangeEventArgs>>
                {
                    new Tuple<bool, JsonValue, JsonValueChangeEventArgs>(true, jo, new JsonValueChangeEventArgs(objToAdd, JsonValueChange.Add, key4)),
                    new Tuple<bool, JsonValue, JsonValueChangeEventArgs>(false, jo, new JsonValueChangeEventArgs(objToAdd, JsonValueChange.Add, key4)),
                },
                obj => obj.Add("key44", objToAdd));

            JsonArray jaToAdd = SpecialJsonValueHelper.CreatePrePopulatedJsonArray(seed, maxObj);
            JsonValue replaced = jo[key2];
            TestEvents(
                jo,
                obj => obj[key2] = jaToAdd,
                new List<Tuple<bool, JsonValue, JsonValueChangeEventArgs>>
                {
                    new Tuple<bool, JsonValue, JsonValueChangeEventArgs>(true, jo, new JsonValueChangeEventArgs(jaToAdd, JsonValueChange.Replace, key2)),
                    new Tuple<bool, JsonValue, JsonValueChangeEventArgs>(false, jo, new JsonValueChangeEventArgs(replaced, JsonValueChange.Replace, key2)),
                });

            JsonValue jpToAdd = SpecialJsonValueHelper.GetRandomJsonPrimitives(seed);
            TestEvents(
                jo,
                obj => obj[key5] = jpToAdd,
                new List<Tuple<bool, JsonValue, JsonValueChangeEventArgs>>
                {
                    new Tuple<bool, JsonValue, JsonValueChangeEventArgs>(true, jo, new JsonValueChangeEventArgs(jpToAdd, JsonValueChange.Add, key5)),
                    new Tuple<bool, JsonValue, JsonValueChangeEventArgs>(false, jo, new JsonValueChangeEventArgs(jpToAdd, JsonValueChange.Add, key5)),
                });

            jo.Remove(key4);
            jo.Remove(key5);

            JsonValue jp1 = SpecialJsonValueHelper.GetRandomJsonPrimitives(seed);
            JsonValue jp2 = SpecialJsonValueHelper.GetRandomJsonPrimitives(seed);
            TestEvents(
                jo,
                obj => obj.AddRange(new JsonObject { { key4, jp1 }, { key5, jp1 } }),
                new List<Tuple<bool, JsonValue, JsonValueChangeEventArgs>>
                {
                    new Tuple<bool, JsonValue, JsonValueChangeEventArgs>(true, jo, new JsonValueChangeEventArgs(jp1, JsonValueChange.Add, key4)),
                    new Tuple<bool, JsonValue, JsonValueChangeEventArgs>(true, jo, new JsonValueChangeEventArgs(jp2, JsonValueChange.Add, key5)),
                    new Tuple<bool, JsonValue, JsonValueChangeEventArgs>(false, jo, new JsonValueChangeEventArgs(jp1, JsonValueChange.Add, key4)),
                    new Tuple<bool, JsonValue, JsonValueChangeEventArgs>(false, jo, new JsonValueChangeEventArgs(jp2, JsonValueChange.Add, key5)),
                },
                obj => obj.AddRange(new JsonObject { { "new key", jp1 }, { "newnewKey", jp2 } }));

            TestEvents(
                jo,
                obj => obj.Remove(key5),
                new List<Tuple<bool, JsonValue, JsonValueChangeEventArgs>>
                {
                    new Tuple<bool, JsonValue, JsonValueChangeEventArgs>(true, jo, new JsonValueChangeEventArgs(jp2, JsonValueChange.Remove, key5)),
                    new Tuple<bool, JsonValue, JsonValueChangeEventArgs>(false, jo, new JsonValueChangeEventArgs(jp2, JsonValueChange.Remove, key5)),
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
                },
                obj => ((IDictionary<string, JsonValue>)obj).Remove(new KeyValuePair<string, JsonValue>(key1, jo[key1])));
        }

        /// <summary>
        /// Tests for events in <see cref="JsonValue"/> instances when multiple listeners are registered.
        /// </summary>
        [Fact]
        public void MultipleListenersTest()
        {
            const string key1 = "first";
            const string key2 = "second";
            const string key3 = "third";

            for (int changingListeners = 0; changingListeners <= 3; changingListeners++)
            {
                for (int changedListeners = 0; changedListeners <= 3; changedListeners++)
                {
                    MultipleListenersTestInternal<JsonObject>(
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

                    MultipleListenersTestInternal<JsonArray>(
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

        internal static void TestEvents<JsonValueType>(JsonValueType target, Action<JsonValueType> actionToTriggerEvent, List<Tuple<bool, JsonValue, JsonValueChangeEventArgs>> expectedEvents, Action<JsonValueType> actionToTriggerEvent2 = null) where JsonValueType : JsonValue
        {
            var actualEvents = new List<Tuple<bool, JsonValue, JsonValueChangeEventArgs>>();
            EventHandler<JsonValueChangeEventArgs> changingHandler = (sender, e) => actualEvents.Add(new Tuple<bool, JsonValue, JsonValueChangeEventArgs>(true, sender as JsonValue, e));
            EventHandler<JsonValueChangeEventArgs> changedHandler = (sender, e) => actualEvents.Add(new Tuple<bool, JsonValue, JsonValueChangeEventArgs>(false, sender as JsonValue, e));

            target.Changing += changingHandler;
            target.Changed += changedHandler;
            actionToTriggerEvent(target);

            target.Changing -= changingHandler;
            target.Changed -= changedHandler;
            ValidateExpectedEvents(expectedEvents, actualEvents);
            if (actionToTriggerEvent2 == null)
            {
                actionToTriggerEvent(target);
            }
            else
            {
                actionToTriggerEvent2(target);
            }

            ValidateExpectedEvents(expectedEvents, actualEvents);
        }

        internal static void TestEventsDynamic(dynamic target, Action<dynamic> actionToTriggerEvent, List<Tuple<bool, JsonValue, JsonValueChangeEventArgs>> expectedEvents)
        {
            var actualEvents = new List<Tuple<bool, JsonValue, JsonValueChangeEventArgs>>();
            EventHandler<JsonValueChangeEventArgs> changingHandler = (sender, e) => actualEvents.Add(new Tuple<bool, JsonValue, JsonValueChangeEventArgs>(true, sender as JsonValue, e));
            EventHandler<JsonValueChangeEventArgs> changedHandler = (sender, e) => actualEvents.Add(new Tuple<bool, JsonValue, JsonValueChangeEventArgs>(false, sender as JsonValue, e));

            target.Changing += changingHandler;
            target.Changed += changedHandler;
            actionToTriggerEvent(target);

            target.Changing -= changingHandler;
            target.Changed -= changedHandler;
            ValidateExpectedEvents(expectedEvents, actualEvents);

            actionToTriggerEvent(target);
            ValidateExpectedEvents(expectedEvents, actualEvents);
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
                Assert.Equal(expectedSender, actualSender);

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

        internal static void MultipleListenersTestInternal<JsonValueType>(
            Func<JsonValueType> createTarget,
            Action<JsonValueType> actionToTriggerEvents,
            List<JsonValueChangeEventArgs> expectedChangingEventArgs,
            List<JsonValueChangeEventArgs> expectedChangedEventArgs,
            int changingListeners,
            int changedListeners) where JsonValueType : JsonValue
        {
            Log.Info("Testing events on a {0} for {1} changING listeners and {2} changED listeners", typeof(JsonValueType).Name, changingListeners, changedListeners);
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
                var index = i;
                target.Changing += delegate(object sender, JsonValueChangeEventArgs e)
                {
                    actualChangingEvents[index].Add(new Tuple<bool, JsonValue, JsonValueChangeEventArgs>(true, sender as JsonValue, e));
                };
            }

            for (int i = 0; i < changedListeners; i++)
            {
                actualChangedEvents[i] = new List<Tuple<bool, JsonValue, JsonValueChangeEventArgs>>();
                var index = i;
                target.Changed += delegate(object sender, JsonValueChangeEventArgs e)
                {
                    actualChangedEvents[index].Add(new Tuple<bool, JsonValue, JsonValueChangeEventArgs>(false, sender as JsonValue, e));
                };
            }

            actionToTriggerEvents(target);
            for (int i = 0; i < changingListeners; i++)
            {
                Log.Info("Validating Changing events for listener {0}", i);
                ValidateExpectedEvents(expectedChangingEvents, actualChangingEvents[i]);
            }

            for (int i = 0; i < changedListeners; i++)
            {
                Log.Info("Validating Changed events for listener {0}", i);
                ValidateExpectedEvents(expectedChangedEvents, actualChangedEvents[i]);
            }

            for (int i = 0; i < changingListeners; i++)
            {
                actualChangingEvents[i] = new List<Tuple<bool, JsonValue, JsonValueChangeEventArgs>>();
                var index = i;
                target.Changing -= delegate(object sender, JsonValueChangeEventArgs e)
                {
                    actualChangingEvents[i].Add(new Tuple<bool, JsonValue, JsonValueChangeEventArgs>(true, sender as JsonValue, e));
                };
            }

            for (int i = 0; i < changedListeners; i++)
            {
                actualChangedEvents[i] = new List<Tuple<bool, JsonValue, JsonValueChangeEventArgs>>();
                var index = i;
                target.Changed -= delegate(object sender, JsonValueChangeEventArgs e)
                {
                    actualChangedEvents[i].Add(new Tuple<bool, JsonValue, JsonValueChangeEventArgs>(false, sender as JsonValue, e));
                };
            }

            target = createTarget();
            expectedChangingEvents.Clear();
            expectedChangedEvents.Clear();
            actionToTriggerEvents(target);

            for (int i = 0; i < changingListeners; i++)
            {
                Log.Info("Validating Changing events for listener {0}", i);
                ValidateExpectedEvents(expectedChangingEvents, actualChangingEvents[i]);
            }

            for (int i = 0; i < changedListeners; i++)
            {
                Log.Info("Validating Changed events for listener {0}", i);
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
