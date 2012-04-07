// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Xunit;

namespace System.Json
{
    /// <summary>
    /// Tests for round-tripping <see cref="JsonValue"/> instances via JSON strings.
    /// </summary>
    public class JsonStringRoundTripTests
    {
        /// <summary>
        /// Tests for <see cref="JsonObject"/> round-trip.
        /// </summary>
        [Fact]
        public void ValidJsonObjectRoundTrip()
        {
            bool oldValue = CreatorSettings.CreateDateTimeWithSubMilliseconds;
            CreatorSettings.CreateDateTimeWithSubMilliseconds = false;
            try
            {
                int seed = 1;
                Log.Info("Seed: {0}", seed);
                Random rndGen = new Random(seed);

                JsonObject sourceJson = new JsonObject(new Dictionary<string, JsonValue>()
                {
                    { "Name", PrimitiveCreator.CreateInstanceOfString(rndGen) },
                    { "Age", PrimitiveCreator.CreateInstanceOfInt32(rndGen) },
                    { "DateTimeOffset", PrimitiveCreator.CreateInstanceOfDateTimeOffset(rndGen) },
                    { "Birthday", PrimitiveCreator.CreateInstanceOfDateTime(rndGen) }
                });
                sourceJson.Add("NewItem1", PrimitiveCreator.CreateInstanceOfString(rndGen));
                sourceJson.Add(new KeyValuePair<string, JsonValue>("NewItem2", PrimitiveCreator.CreateInstanceOfString(rndGen)));

                JsonObject newJson = (JsonObject)JsonValue.Parse(sourceJson.ToString());

                newJson.Remove("NewItem1");
                sourceJson.Remove("NewItem1");

                Assert.False(newJson.ContainsKey("NewItem1"));

                Assert.False(!JsonValueVerifier.Compare(sourceJson, newJson));
            }
            finally
            {
                CreatorSettings.CreateDateTimeWithSubMilliseconds = oldValue;
            }
        }

        /// <summary>
        /// Test for <see cref="JsonPrimitive"/> round-trip created via <see cref="DateTime"/>.
        /// </summary>
        [Fact]
        public void SimpleDateTimeTest()
        {
            JsonValue jv = DateTime.Now;
            JsonValue jv2 = JsonValue.Parse(jv.ToString());
            Assert.Equal(jv.ToString(), jv2.ToString());
        }

        /// <summary>
        /// Test for <see cref="JsonPrimitive"/> round-trip created via <see cref="DateTimeOffset"/>.
        /// </summary>
        [Fact]
        public void ValidJsonObjectDateTimeOffsetRoundTrip()
        {
            int seed = 1;
            Log.Info("Seed: {0}", seed);
            Random rndGen = new Random(seed);

            JsonPrimitive sourceJson = new JsonPrimitive(PrimitiveCreator.CreateInstanceOfDateTimeOffset(rndGen));
            JsonPrimitive newJson = (JsonPrimitive)JsonValue.Parse(sourceJson.ToString());

            Assert.True(JsonValueVerifier.Compare(sourceJson, newJson));
        }

        /// <summary>
        /// Tests for <see cref="JsonArray"/> round-trip.
        /// </summary>
        [Fact]
        public void ValidJsonArrayRoundTrip()
        {
            bool oldValue = CreatorSettings.CreateDateTimeWithSubMilliseconds;
            CreatorSettings.CreateDateTimeWithSubMilliseconds = false;
            try
            {
                int seed = 1;
                Log.Info("Seed: {0}", seed);
                Random rndGen = new Random(seed);

                JsonArray sourceJson = new JsonArray(new JsonValue[]
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
                    PrimitiveCreator.CreateInstanceOfUInt64(rndGen)
                });

                JsonArray newJson = (JsonArray)JsonValue.Parse(sourceJson.ToString());

                Log.Info("Original JsonArray object is: {0}", sourceJson);
                Log.Info("Round-tripped JsonArray object is: {0}", newJson);

                Assert.True(JsonValueVerifier.Compare(sourceJson, newJson));
            }
            finally
            {
                CreatorSettings.CreateDateTimeWithSubMilliseconds = oldValue;
            }
        }

        /// <summary>
        /// Test for <see cref="JsonPrimitive"/> round-trip created via <see cref="String"/>.
        /// </summary>
        [Fact]
        public void ValidPrimitiveStringRoundTrip()
        {
            Assert.True(this.TestPrimitiveType("String"));
        }

        /// <summary>
        /// Test for <see cref="JsonPrimitive"/> round-trip created via <see cref="DateTime"/>.
        /// </summary>
        [Fact]
        public void ValidPrimitiveDateTimeRoundTrip()
        {
            bool oldValue = CreatorSettings.CreateDateTimeWithSubMilliseconds;
            CreatorSettings.CreateDateTimeWithSubMilliseconds = false;
            try
            {
                Assert.True(this.TestPrimitiveType("DateTime"));
            }
            finally
            {
                CreatorSettings.CreateDateTimeWithSubMilliseconds = oldValue;
            }
        }

        /// <summary>
        /// Test for <see cref="JsonPrimitive"/> round-trip created via <see cref="Boolean"/>.
        /// </summary>
        [Fact]
        public void ValidPrimitiveBooleanRoundTrip()
        {
            Assert.True(this.TestPrimitiveType("Boolean"));
        }

        /// <summary>
        /// Test for <see cref="JsonPrimitive"/> round-trip created via <see cref="Byte"/>.
        /// </summary>
        [Fact]
        public void ValidPrimitiveByteRoundTrip()
        {
            Assert.True(this.TestPrimitiveType("Byte"));
        }

        /// <summary>
        /// Test for <see cref="JsonPrimitive"/> round-trip created via <see cref="Decimal"/>.
        /// </summary>
        [Fact]
        public void ValidPrimitiveDecimalRoundTrip()
        {
            Assert.True(this.TestPrimitiveType("Decimal"));
        }

        /// <summary>
        /// Test for <see cref="JsonPrimitive"/> round-trip created via <see cref="Double"/>.
        /// </summary>
        [Fact]
        public void ValidPrimitiveDoubleRoundTrip()
        {
            Assert.True(this.TestPrimitiveType("Double"));
        }

        /// <summary>
        /// Test for <see cref="JsonPrimitive"/> round-trip created via <see cref="Int16"/>.
        /// </summary>
        [Fact]
        public void ValidPrimitiveInt16RoundTrip()
        {
            Assert.True(this.TestPrimitiveType("Int16"));
        }

        /// <summary>
        /// Test for <see cref="JsonPrimitive"/> round-trip created via <see cref="Int32"/>.
        /// </summary>
        [Fact]
        public void ValidPrimitiveInt32RoundTrip()
        {
            Assert.True(this.TestPrimitiveType("Int32"));
        }

        /// <summary>
        /// Test for <see cref="JsonPrimitive"/> round-trip created via <see cref="Int64"/>.
        /// </summary>
        [Fact]
        public void ValidPrimitiveInt64RoundTrip()
        {
            Assert.True(this.TestPrimitiveType("Int64"));
        }

        /// <summary>
        /// Test for <see cref="JsonPrimitive"/> round-trip created via <see cref="SByte"/>.
        /// </summary>
        [Fact]
        public void ValidPrimitiveSByteRoundTrip()
        {
            Assert.True(this.TestPrimitiveType("SByte"));
        }

        /// <summary>
        /// Test for <see cref="JsonPrimitive"/> round-trip created via <see cref="UInt16"/>.
        /// </summary>
        [Fact]
        public void ValidPrimitiveUInt16RoundTrip()
        {
            Assert.True(this.TestPrimitiveType("Uint16"));
        }

        /// <summary>
        /// Test for <see cref="JsonPrimitive"/> round-trip created via <see cref="UInt32"/>.
        /// </summary>
        [Fact]
        public void ValidPrimitiveUInt32RoundTrip()
        {
            Assert.True(this.TestPrimitiveType("UInt32"));
        }

        /// <summary>
        /// Test for <see cref="JsonPrimitive"/> round-trip created via <see cref="UInt64"/>.
        /// </summary>
        [Fact]
        public void ValidPrimitiveUInt64RoundTrip()
        {
            Assert.True(this.TestPrimitiveType("UInt64"));
        }

        /// <summary>
        /// Test for <see cref="JsonPrimitive"/> round-trip created via <see cref="Char"/>.
        /// </summary>
        [Fact]
        public void ValidPrimitiveCharRoundTrip()
        {
            Assert.True(this.TestPrimitiveType("Char"));
        }

        /// <summary>
        /// Test for <see cref="JsonPrimitive"/> round-trip created via <see cref="Guid"/>.
        /// </summary>
        [Fact]
        public void ValidPrimitiveGuidRoundTrip()
        {
            Assert.True(this.TestPrimitiveType("Guid"));
        }

        /// <summary>
        /// Test for <see cref="JsonPrimitive"/> round-trip created via <see cref="Uri"/>.
        /// </summary>
        [Fact]
        public void ValidPrimitiveUriRoundTrip()
        {
            Assert.True(this.TestPrimitiveType("Uri"));
        }

        /// <summary>
        /// Tests for <see cref="JsonValue"/> round-trip created via <code>null</code> values.
        /// </summary>
        [Fact]
        public void ValidPrimitiveNullRoundTrip()
        {
            Assert.True(this.TestPrimitiveType("Null"));
        }

        /// <summary>
        /// Tests for round-tripping <see cref="JsonPrimitive"/> objects via casting to CLR instances.
        /// </summary>
        [Fact]
        public void JsonValueRoundTripCastTests()
        {
            int seed = 1;
            Log.Info("Seed: {0}", seed);
            Random rndGen = new Random(seed);

            this.DoRoundTripCasting(String.Empty, typeof(string));
            this.DoRoundTripCasting("null", typeof(string));
            string str;
            do
            {
                str = PrimitiveCreator.CreateInstanceOfString(rndGen);
            } while (str == null);

            this.DoRoundTripCasting(str, typeof(string));
            this.DoRoundTripCasting(PrimitiveCreator.CreateInstanceOfInt16(rndGen), typeof(int));
            this.DoRoundTripCasting(PrimitiveCreator.CreateInstanceOfInt32(rndGen), typeof(int));
            this.DoRoundTripCasting(PrimitiveCreator.CreateInstanceOfInt64(rndGen), typeof(int));
            this.DoRoundTripCasting(PrimitiveCreator.CreateInstanceOfUInt16(rndGen), typeof(int));
            this.DoRoundTripCasting(PrimitiveCreator.CreateInstanceOfUInt32(rndGen), typeof(int));
            this.DoRoundTripCasting(PrimitiveCreator.CreateInstanceOfUInt64(rndGen), typeof(int));
            this.DoRoundTripCasting(PrimitiveCreator.CreateInstanceOfGuid(rndGen), typeof(Guid));
            this.DoRoundTripCasting(new Uri("http://bug/test?param=hello%0a"), typeof(Uri));
            this.DoRoundTripCasting(PrimitiveCreator.CreateInstanceOfChar(rndGen), typeof(char));
            this.DoRoundTripCasting(PrimitiveCreator.CreateInstanceOfBoolean(rndGen), typeof(bool));
            this.DoRoundTripCasting(PrimitiveCreator.CreateInstanceOfDateTime(rndGen), typeof(DateTime));
            this.DoRoundTripCasting(PrimitiveCreator.CreateInstanceOfDateTimeOffset(rndGen), typeof(DateTimeOffset));
            this.DoRoundTripCasting(PrimitiveCreator.CreateInstanceOfDouble(rndGen), typeof(double));
            this.DoRoundTripCasting(PrimitiveCreator.CreateInstanceOfDouble(rndGen), typeof(float));
            this.DoRoundTripCasting(0.12345f, typeof(double));
            this.DoRoundTripCasting(0.12345f, typeof(float));
        }

        private bool TestPrimitiveType(string typeName)
        {
            bool retValue = true;
            bool specialCase = false;

            int seed = 1;
            Log.Info("Seed: {0}", seed);
            Random rndGen = new Random(seed);

            JsonPrimitive sourceJson = null;
            JsonPrimitive sourceJson2;
            object tempValue = null;
            switch (typeName.ToLower())
            {
                case "boolean":
                    tempValue = PrimitiveCreator.CreateInstanceOfBoolean(rndGen);
                    sourceJson = (JsonPrimitive)JsonValue.Parse(tempValue.ToString().ToLower());
                    sourceJson2 = new JsonPrimitive((bool)tempValue);
                    break;
                case "byte":
                    tempValue = PrimitiveCreator.CreateInstanceOfByte(rndGen);
                    sourceJson = (JsonPrimitive)JsonValue.Parse(tempValue.ToString());
                    sourceJson2 = new JsonPrimitive((byte)tempValue);
                    break;
                case "char":
                    sourceJson2 = new JsonPrimitive((char)PrimitiveCreator.CreateInstanceOfChar(rndGen));
                    specialCase = true;
                    break;
                case "datetime":
                    tempValue = PrimitiveCreator.CreateInstanceOfDateTime(rndGen);
                    sourceJson2 = new JsonPrimitive((DateTime)tempValue);
                    sourceJson = (JsonPrimitive)JsonValue.Parse(sourceJson2.ToString());
                    break;
                case "decimal":
                    tempValue = PrimitiveCreator.CreateInstanceOfDecimal(rndGen);
                    sourceJson = (JsonPrimitive)JsonValue.Parse(((decimal)tempValue).ToString(NumberFormatInfo.InvariantInfo));
                    sourceJson2 = new JsonPrimitive((decimal)tempValue);
                    break;
                case "double":
                    double tempDouble = PrimitiveCreator.CreateInstanceOfDouble(rndGen);
                    sourceJson = (JsonPrimitive)JsonValue.Parse(tempDouble.ToString("R", NumberFormatInfo.InvariantInfo));
                    sourceJson2 = new JsonPrimitive(tempDouble);
                    break;
                case "guid":
                    sourceJson2 = new JsonPrimitive(PrimitiveCreator.CreateInstanceOfGuid(rndGen));
                    specialCase = true;
                    break;
                case "int16":
                    tempValue = PrimitiveCreator.CreateInstanceOfInt16(rndGen);
                    sourceJson = (JsonPrimitive)JsonValue.Parse(tempValue.ToString());
                    sourceJson2 = new JsonPrimitive((short)tempValue);
                    break;
                case "int32":
                    tempValue = PrimitiveCreator.CreateInstanceOfInt32(rndGen);
                    sourceJson = (JsonPrimitive)JsonValue.Parse(tempValue.ToString());
                    sourceJson2 = new JsonPrimitive((int)tempValue);
                    break;
                case "int64":
                    tempValue = PrimitiveCreator.CreateInstanceOfInt64(rndGen);
                    sourceJson = (JsonPrimitive)JsonValue.Parse(tempValue.ToString());
                    sourceJson2 = new JsonPrimitive((long)tempValue);
                    break;
                case "sbyte":
                    tempValue = PrimitiveCreator.CreateInstanceOfSByte(rndGen);
                    sourceJson = (JsonPrimitive)JsonValue.Parse(tempValue.ToString());
                    sourceJson2 = new JsonPrimitive((sbyte)tempValue);
                    break;
                case "single":
                    float fltValue = PrimitiveCreator.CreateInstanceOfSingle(rndGen);
                    sourceJson = (JsonPrimitive)JsonValue.Parse(fltValue.ToString("R", NumberFormatInfo.InvariantInfo));
                    sourceJson2 = new JsonPrimitive(fltValue);
                    break;
                case "string":
                    do
                    {
                        tempValue = PrimitiveCreator.CreateInstanceOfString(rndGen);
                    } while (tempValue == null);

                    sourceJson2 = new JsonPrimitive((string)tempValue);
                    sourceJson = (JsonPrimitive)JsonValue.Parse(sourceJson2.ToString());
                    break;
                case "uint16":
                    tempValue = PrimitiveCreator.CreateInstanceOfUInt16(rndGen);
                    sourceJson = (JsonPrimitive)JsonValue.Parse(tempValue.ToString());
                    sourceJson2 = new JsonPrimitive((ushort)tempValue);
                    break;
                case "uint32":
                    tempValue = PrimitiveCreator.CreateInstanceOfUInt32(rndGen);
                    sourceJson = (JsonPrimitive)JsonValue.Parse(tempValue.ToString());
                    sourceJson2 = new JsonPrimitive((uint)tempValue);
                    break;
                case "uint64":
                    tempValue = PrimitiveCreator.CreateInstanceOfUInt64(rndGen);
                    sourceJson = (JsonPrimitive)JsonValue.Parse(tempValue.ToString());
                    sourceJson2 = new JsonPrimitive((ulong)tempValue);
                    break;
                case "uri":
                    Uri uri = null;
                    do
                    {
                        try
                        {
                            uri = PrimitiveCreator.CreateInstanceOfUri(rndGen);
                        }
                        catch (UriFormatException)
                        {
                        }
                    } while (uri == null);

                    sourceJson2 = new JsonPrimitive(uri);
                    specialCase = true;
                    break;
                case "null":
                    sourceJson = (JsonPrimitive)JsonValue.Parse("null");
                    sourceJson2 = null;
                    break;
                default:
                    sourceJson = null;
                    sourceJson2 = null;
                    break;
            }

            if (!specialCase)
            {
                // comparison between two constructors
                if (!JsonValueVerifier.Compare(sourceJson, sourceJson2))
                {
                    Log.Info("(JsonPrimitive)JsonValue.Parse(string) failed to match the results from default JsonPrimitive(obj)constructor for type {0}", typeName);
                    retValue = false;
                }

                if (sourceJson != null)
                {
                    // test JsonValue.Load(TextReader)
                    JsonPrimitive newJson = null;
                    using (StringReader sr = new StringReader(sourceJson.ToString()))
                    {
                        newJson = (JsonPrimitive)JsonValue.Load(sr);
                    }

                    if (!JsonValueVerifier.Compare(sourceJson, newJson))
                    {
                        Log.Info("JsonValue.Load(TextReader) failed to function properly for type {0}", typeName);
                        retValue = false;
                    }

                    // test JsonValue.Load(Stream) is located in the JObjectFromGenoTypeLib test case

                    // test JsonValue.Parse(string)
                    newJson = null;
                    newJson = (JsonPrimitive)JsonValue.Parse(sourceJson.ToString());
                    if (!JsonValueVerifier.Compare(sourceJson, newJson))
                    {
                        Log.Info("JsonValue.Parse(string) failed to function properly for type {0}", typeName);
                        retValue = false;
                    }
                }
            }
            else
            {
                // test JsonValue.Load(TextReader)
                JsonPrimitive newJson2 = null;
                using (StringReader sr = new StringReader(sourceJson2.ToString()))
                {
                    newJson2 = (JsonPrimitive)JsonValue.Load(sr);
                }

                if (!JsonValueVerifier.Compare(sourceJson2, newJson2))
                {
                    Log.Info("JsonValue.Load(TextReader) failed to function properly for type {0}", typeName);
                    retValue = false;
                }

                // test JsonValue.Load(Stream) is located in the JObjectFromGenoTypeLib test case

                // test JsonValue.Parse(string)
                newJson2 = null;
                newJson2 = (JsonPrimitive)JsonValue.Parse(sourceJson2.ToString());
                if (!JsonValueVerifier.Compare(sourceJson2, newJson2))
                {
                    Log.Info("JsonValue.Parse(string) failed to function properly for type {0}", typeName);
                    retValue = false;
                }
            }

            return retValue;
        }

        private void DoRoundTripCasting(JsonValue jo, Type type)
        {
            bool result = false;

            // Casting
            if (jo.JsonType == JsonType.String)
            {
                JsonValue jstr = (string)jo;
                if (type == typeof(DateTime))
                {
                    Log.Info("{0} Value:{1}", type.Name, ((DateTime)jstr).ToString(DateTimeFormatInfo.InvariantInfo));
                }
                else if (type == typeof(DateTimeOffset))
                {
                    Log.Info("{0} Value:{1}", type.Name, ((DateTimeOffset)jstr).ToString(DateTimeFormatInfo.InvariantInfo));
                }
                else if (type == typeof(Guid))
                {
                    Log.Info("{0} Value:{1}", type.Name, (Guid)jstr);
                }
                else if (type == typeof(char))
                {
                    Log.Info("{0} Value:{1}", type.Name, (char)jstr);
                }
                else if (type == typeof(Uri))
                {
                    Log.Info("{0} Value:{1}", type.Name, ((Uri)jstr).AbsoluteUri);
                }
                else
                {
                    Log.Info("{0} Value:{1}", type.Name, (string)jstr);
                }

                if (jo.ToString() == jstr.ToString())
                {
                    result = true;
                }
            }
            else if (jo.JsonType == JsonType.Object)
            {
                JsonObject jobj = new JsonObject((JsonObject)jo);

                if (jo.ToString() == jobj.ToString())
                {
                    result = true;
                }
            }
            else if (jo.JsonType == JsonType.Number)
            {
                JsonPrimitive jprim = (JsonPrimitive)jo;
                Log.Info("{0} Value:{1}", type.Name, jprim);

                if (jo.ToString() == jprim.ToString())
                {
                    result = true;
                }
            }
            else if (jo.JsonType == JsonType.Boolean)
            {
                JsonPrimitive jprim = (JsonPrimitive)jo;
                Log.Info("{0} Value:{1}", type.Name, (bool)jprim);

                if (jo.ToString() == jprim.ToString())
                {
                    result = true;
                }
            }

            Assert.True(result);
        }
    }
}
