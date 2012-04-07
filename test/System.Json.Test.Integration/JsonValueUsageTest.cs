// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace System.Json
{
    /// <summary>
    /// Test class for some scenario usages for <see cref="JsonValue"/> types.
    /// </summary>
    public class JsonValueUsageTest
    {
        /// <summary>
        /// Test for consuming <see cref="JsonValue"/> objects in a Linq query.
        /// </summary>
        [Fact]
        public void JLinqSimpleCreationQueryTest()
        {
            int seed = 1;
            Random rndGen = new Random(seed);

            JsonArray sourceJson = new JsonArray
            {
                new JsonObject { { "Name", "Alex" }, { "Age", 18 }, { "Birthday", PrimitiveCreator.CreateInstanceOfDateTime(rndGen) } },
                new JsonObject { { "Name", "Joe" }, { "Age", 19 }, { "Birthday", DateTime.MinValue } },
                new JsonObject { { "Name", "Chris" }, { "Age", 20 }, { "Birthday", DateTime.Now } },
                new JsonObject { { "Name", "Jeff" }, { "Age", 21 }, { "Birthday", DateTime.MaxValue } },
                new JsonObject { { "Name", "Carlos" }, { "Age", 22 }, { "Birthday", PrimitiveCreator.CreateInstanceOfDateTime(rndGen) } },
                new JsonObject { { "Name", "Mohammad" }, { "Age", 23 }, { "Birthday", PrimitiveCreator.CreateInstanceOfDateTime(rndGen) } },
                new JsonObject { { "Name", "Sara" }, { "Age", 24 }, { "Birthday", new DateTime(1998, 3, 20) } },
                new JsonObject { { "Name", "Tomasz" }, { "Age", 25 }, { "Birthday", PrimitiveCreator.CreateInstanceOfDateTime(rndGen) } },
                new JsonObject { { "Name", "Suwat" }, { "Age", 26 }, { "Birthday", new DateTime(1500, 12, 20) } },
                new JsonObject { { "Name", "Eugene" }, { "Age", 27 }, { "Birthday", PrimitiveCreator.CreateInstanceOfDateTime(rndGen) } }
            };

            var adults = from JsonValue adult in sourceJson
                         where (int)adult["Age"] > 21
                         select adult;
            Log.Info("Team contains: ");
            int count = 0;
            foreach (JsonValue adult in adults)
            {
                count++;
                Log.Info((string)adult["Name"]);
            }

            Assert.Equal(count, 6);
        }

        /// <summary>
        /// Test for consuming <see cref="JsonValue"/> arrays in a Linq query.
        /// </summary>
        [Fact]
        public void JLinqSimpleQueryTest()
        {
            JsonArray sourceJson = this.CreateArrayOfPeople();

            var adults = from JsonValue adult in sourceJson
                         where (int)adult["Age"] > 21
                         select adult;
            Log.Info("Team contains: ");
            int count = 0;
            foreach (JsonValue adult in adults)
            {
                count++;
                Log.Info((string)adult["Name"]);
            }

            Assert.Equal(count, 6);
        }

        /// <summary>
        /// Test for consuming deep <see cref="JsonValue"/> objects in a Linq query.
        /// </summary>
        [Fact]
        public void JLinqDeepQueryTest()
        {
            int seed = 1;

            JsonArray mixedOrderJsonObj;
            JsonArray myJsonObj = SpecialJsonValueHelper.CreateDeepLevelJsonValuePair(seed, out mixedOrderJsonObj);

            if (myJsonObj != null && mixedOrderJsonObj != null)
            {
                bool retValue = true;

                var dict = new Dictionary<string, int>
                {
                    { "myArray", 1 }, 
                    { "myArrayLevel2", 2 }, 
                    { "myArrayLevel3", 3 }, 
                    { "myArrayLevel4", 4 }, 
                    { "myArrayLevel5", 5 }, 
                    { "myArrayLevel6", 6 }, 
                    { "myArrayLevel7", 7 },
                    { "myBool", 8 }, 
                    { "myByte", 9 }, 
                    { "myDatetime", 10 },
                    { "myDateTimeOffset", 11 },
                    { "myDecimal", 12 },
                    { "myDouble", 13 }, 
                    { "myInt16", 14 }, 
                    { "myInt32", 15 }, 
                    { "myInt64", 16 }, 
                    { "mySByte", 17 }, 
                    { "mySingle", 18 },
                    { "myString", 19 },
                    { "myUInt16", 20 },
                    { "myUInt32", 21 },
                    { "myUInt64", 22 }
                };

                foreach (string name in dict.Keys)
                {
                    if (!this.InternalVerificationViaLinqQuery(myJsonObj, name, dict[name]))
                    {
                        retValue = false;
                    }

                    if (!this.InternalVerificationViaLinqQuery(mixedOrderJsonObj, name, dict[name]))
                    {
                        retValue = false;
                    }

                    if (!this.CrossJsonValueVerificationOnNameViaLinqQuery(myJsonObj, mixedOrderJsonObj, name))
                    {
                        retValue = false;
                    }

                    if (!this.CrossJsonValueVerificationOnIndexViaLinqQuery(myJsonObj, mixedOrderJsonObj, dict[name]))
                    {
                        retValue = false;
                    }
                }

                Assert.True(retValue, "The JsonValue did not verify as expected!");
            }
            else
            {
                Assert.True(false, "Failed to create the pair of JsonValues!");
            }
        }

        /// <summary>
        /// Test for consuming <see cref="JsonValue"/> objects in a Linq query using the dynamic notation.
        /// </summary>
        [Fact]
        public void LinqToDynamicJsonArrayTest()
        {
            JsonValue people = this.CreateArrayOfPeople();

            var match = from person in people select person;
            Assert.True(match.Count() == people.Count, "IEnumerable returned different number of elements that JsonArray contains");

            int sum = 0;
            foreach (KeyValuePair<string, JsonValue> kv in match)
            {
                sum += Int32.Parse(kv.Key);
            }

            Assert.True(sum == (people.Count * (people.Count - 1) / 2), "Not all elements of the array were enumerated exactly once");

            match = from person in people
                    where person.Value.AsDynamic().Name.ReadAs<string>().StartsWith("S")
                        && person.Value.AsDynamic().Age.ReadAs<int>() > 20
                    select person;
            Assert.True(match.Count() == 2, "Number of matches was expected to be 2 but was " + match.Count());
        }

        /// <summary>
        /// Test for consuming <see cref="JsonValue"/> objects in a Linq query.
        /// </summary>
        [Fact]
        public void LinqToJsonObjectTest()
        {
            JsonValue person = this.CreateArrayOfPeople()[0];
            var match = from nameValue in person select nameValue;
            Assert.True(match.Count() == 3, "IEnumerable of JsonObject returned a different number of elements than there are name value pairs in the JsonObject" + match.Count());

            List<string> missingNames = new List<string>(new string[] { "Name", "Age", "Birthday" });
            foreach (KeyValuePair<string, JsonValue> kv in match)
            {
                Assert.Equal(person[kv.Key], kv.Value);
                missingNames.Remove(kv.Key);
            }

            Assert.True(missingNames.Count == 0, "Not all JsonObject properties were present in the enumeration");
        }

        /// <summary>
        /// Test for consuming <see cref="JsonValue"/> objects in a Linq query.
        /// </summary>
        [Fact]
        public void LinqToJsonObjectAsAssociativeArrayTest()
        {
            JsonValue gameScores = new JsonObject(new Dictionary<string, JsonValue>()
                {
                    { "tomek", 12 },
                    { "suwat", 27 },
                    { "carlos", 127 },
                    { "miguel", 57 },
                    { "henrik", 2 },
                    { "joe", 15 }
                });

            var match = from score in gameScores
                        where score.Key.Contains("o") && score.Value.ReadAs<int>() > 100
                        select score;
            Assert.True(match.Count() == 1, "Incorrect number of matching game scores");
        }

        /// <summary>
        /// Test for consuming <see cref="JsonPrimitive"/> objects in a Linq query.
        /// </summary>
        [Fact]
        public void LinqToJsonPrimitiveTest()
        {
            JsonValue primitive = 12;

            var match = from m in primitive select m;
            KeyValuePair<string, JsonValue>[] kv = match.ToArray();
            Assert.True(kv.Length == 0);
        }

        /// <summary>
        /// Test for consuming <see cref="JsonValue"/> objects with <see cref="JsonType">JsonType.Default</see> in a Linq query.
        /// </summary>
        [Fact]
        public void LinqToJsonUndefinedTest()
        {
            JsonValue primitive = 12;

            var match = from m in primitive.ValueOrDefault("idontexist")
                        select m;
            Assert.True(match.Count() == 0);
        }

        /// <summary>
        /// Test for consuming calling <see cref="JsonValue.ReadAs{T}(T)"/> in a Linq query.
        /// </summary>
        [Fact]
        public void LinqToDynamicJsonUndefinedWithFallbackTest()
        {
            JsonValue people = this.CreateArrayOfPeople();

            var match = from person in people
                        where person.Value.AsDynamic().IDontExist.IAlsoDontExist.ReadAs<int>(5) > 2
                        select person;
            Assert.True(match.Count() == people.Count, "Number of matches was expected to be " + people.Count + " but was " + match.Count());

            match = from person in people
                    where person.Value.AsDynamic().Age.ReadAs<int>(1) < 21
                    select person;
            Assert.True(match.Count() == 3);
        }

        private JsonArray CreateArrayOfPeople()
        {
            int seed = 1;
            Random rndGen = new Random(seed);
            return new JsonArray(new List<JsonValue>()
            { 
                new JsonObject(new Dictionary<string, JsonValue>()
                {
                    { "Name", "Alex" },
                    { "Age", 18 },
                    { "Birthday", PrimitiveCreator.CreateInstanceOfDateTime(rndGen) }
                }),
                new JsonObject(new Dictionary<string, JsonValue>()
                {
                    { "Name", "Joe" },
                    { "Age", 19 },
                    { "Birthday", DateTime.MinValue }
                }),
                new JsonObject(new Dictionary<string, JsonValue>()
                {
                    { "Name", "Chris" },
                    { "Age", 20 },
                    { "Birthday", DateTime.Now }
                }),
                new JsonObject(new Dictionary<string, JsonValue>()
                {
                    { "Name", "Jeff" },
                    { "Age", 21 },
                    { "Birthday", DateTime.MaxValue }
                }),
                new JsonObject(new Dictionary<string, JsonValue>()
                {
                    { "Name", "Carlos" },
                    { "Age", 22 },
                    { "Birthday", PrimitiveCreator.CreateInstanceOfDateTime(rndGen) }
                }),
                new JsonObject(new Dictionary<string, JsonValue>()
                {
                    { "Name", "Mohammad" },
                    { "Age", 23 },
                    { "Birthday", PrimitiveCreator.CreateInstanceOfDateTime(rndGen) }
                }),
                new JsonObject(new Dictionary<string, JsonValue>()
                {
                    { "Name", "Sara" },
                    { "Age", 24 },
                    { "Birthday", new DateTime(1998, 3, 20) }
                }),
                new JsonObject(new Dictionary<string, JsonValue>()
                {
                    { "Name", "Tomasz" },
                    { "Age", 25 },
                    { "Birthday", PrimitiveCreator.CreateInstanceOfDateTime(rndGen) }
                }),
                new JsonObject(new Dictionary<string, JsonValue>()
                {
                    { "Name", "Suwat" },
                    { "Age", 26 },
                    { "Birthday", new DateTime(1500, 12, 20) }
                }),
                new JsonObject(new Dictionary<string, JsonValue>()
                {
                    { "Name", "Eugene" },
                    { "Age", 27 },
                    { "Birthday", PrimitiveCreator.CreateInstanceOfDateTime(rndGen) }
                })
            });
        }

        private bool InternalVerificationViaLinqQuery(JsonArray sourceJson, string name, int index)
        {
            var itemsByName = from JsonValue itemByName in sourceJson
                              where (itemByName != null && (string)itemByName["Name"] == name)
                              select itemByName;
            int countByName = 0;
            foreach (JsonValue a in itemsByName)
            {
                countByName++;
            }

            Log.Info("Collection contains: " + countByName + " item By Name " + name);

            var itemsByIndex = from JsonValue itemByIndex in sourceJson
                               where (itemByIndex != null && (int)itemByIndex["Index"] == index)
                               select itemByIndex;
            int countByIndex = 0;
            foreach (JsonValue a in itemsByIndex)
            {
                countByIndex++;
            }

            Log.Info("Collection contains: " + countByIndex + " item By Index " + index);

            if (countByIndex != countByName)
            {
                Log.Info("Count by Name = " + countByName + "; Count by Index = " + countByIndex);
                Log.Info("The number of items matching the provided Name does NOT equal to that matching the provided Index, The two JsonValues are not equal!");
                return false;
            }
            else
            {
                return true;
            }
        }

        private bool CrossJsonValueVerificationOnNameViaLinqQuery(JsonArray sourceJson, JsonArray newJson, string name)
        {
            var itemsByName = from JsonValue itemByName in sourceJson
                              where (itemByName != null && (string)itemByName["Name"] == name)
                              select itemByName;
            int countByName = 0;
            foreach (JsonValue a in itemsByName)
            {
                countByName++;
            }

            Log.Info("Original Collection contains: " + countByName + " item By Name " + name);

            var newItemsByName = from JsonValue newItemByName in newJson
                                 where (newItemByName != null && (string)newItemByName["Name"] == name)
                                 select newItemByName;
            int newcountByName = 0;
            foreach (JsonValue a in newItemsByName)
            {
                newcountByName++;
            }

            Log.Info("New Collection contains: " + newcountByName + " item By Name " + name);

            if (countByName != newcountByName)
            {
                Log.Info("Count by Original JsonValue = " + countByName + "; Count by New JsonValue = " + newcountByName);
                Log.Info("The number of items matching the provided Name does NOT equal between these two JsonValues!");
                return false;
            }
            else
            {
                return true;
            }
        }

        private bool CrossJsonValueVerificationOnIndexViaLinqQuery(JsonArray sourceJson, JsonArray newJson, int index)
        {
            var itemsByIndex = from JsonValue itemByIndex in sourceJson
                               where (itemByIndex != null && (int)itemByIndex["Index"] == index)
                               select itemByIndex;
            int countByIndex = 0;
            foreach (JsonValue a in itemsByIndex)
            {
                countByIndex++;
            }

            Log.Info("Original Collection contains: " + countByIndex + " item By Index " + index);

            var newItemsByIndex = from JsonValue newItemByIndex in newJson
                                  where (newItemByIndex != null && (int)newItemByIndex["Index"] == index)
                                  select newItemByIndex;
            int newcountByIndex = 0;
            foreach (JsonValue a in newItemsByIndex)
            {
                newcountByIndex++;
            }

            Log.Info("New Collection contains: " + newcountByIndex + " item By Index " + index);

            if (countByIndex != newcountByIndex)
            {
                Log.Info("Count by Original JsonValue = " + countByIndex + "; Count by New JsonValue = " + newcountByIndex);
                Log.Info("The number of items matching the provided Index does NOT equal between these two JsonValues!");
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
