// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Text;
using Xunit;

namespace System.Json
{
    /// <summary>
    /// JsonPrimitive unit tests
    /// </summary>
    public class JsonPrimitiveTests
    {
        const string DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.fffK";

        /// <summary>
        /// Validates round-trip of <see cref="JsonPrimitive"/> values created from <see cref="Int16"/> values.
        /// </summary>
        [Fact]
        public void JsonPrimitiveFromInt16()
        {
            short[] values = new short[] { Int16.MinValue, Int16.MaxValue, 1 };
            for (int i = 0; i < values.Length; i++)
            {
                this.ValidateJson(new JsonPrimitive(values[i]), GetExpectedRepresentation(values[i]), JsonType.Number);
                this.TestReadAsRoundtrip<short>(new JsonPrimitive(values[i]), values[i]);
            }
        }

        /// <summary>
        /// Validates round-trip of <see cref="JsonPrimitive"/> values created from <see cref="Int32"/> values.
        /// </summary>
        [Fact]
        public void JsonPrimitiveFromInt32()
        {
            int[] values = new int[] { Int32.MinValue, Int32.MaxValue, 12345678 };
            for (int i = 0; i < values.Length; i++)
            {
                this.ValidateJson(new JsonPrimitive(values[i]), GetExpectedRepresentation(values[i]), JsonType.Number);
                this.TestReadAsRoundtrip<int>(new JsonPrimitive(values[i]), values[i]);
            }
        }

        /// <summary>
        /// Validates round-trip of <see cref="JsonPrimitive"/> values created from <see cref="Int64"/> values.
        /// </summary>
        [Fact]
        public void JsonPrimitiveFromInt64()
        {
            long[] values = new long[] { Int64.MinValue, Int64.MaxValue, 12345678901232L };
            for (int i = 0; i < values.Length; i++)
            {
                this.ValidateJson(new JsonPrimitive(values[i]), GetExpectedRepresentation(values[i]), JsonType.Number);
                this.TestReadAsRoundtrip<long>(new JsonPrimitive(values[i]), values[i]);
            }
        }

        /// <summary>
        /// Validates round-trip of <see cref="JsonPrimitive"/> values created from <see cref="UInt64"/> values.
        /// </summary>
        [Fact]
        public void JsonPrimitiveFromUInt64()
        {
            ulong[] values = new ulong[] { UInt64.MinValue, UInt64.MaxValue, 12345678901232L };
            for (int i = 0; i < values.Length; i++)
            {
                this.ValidateJson(new JsonPrimitive(values[i]), GetExpectedRepresentation(values[i]), JsonType.Number);
                this.TestReadAsRoundtrip<ulong>(new JsonPrimitive(values[i]), values[i]);
            }
        }

        /// <summary>
        /// Validates round-trip of <see cref="JsonPrimitive"/> values created from <see cref="UInt32"/> values.
        /// </summary>
        [Fact]
        public void JsonPrimitiveFromUInt32()
        {
            uint[] values = new uint[] { UInt32.MinValue, UInt32.MaxValue, 3234567890 };
            for (int i = 0; i < values.Length; i++)
            {
                this.ValidateJson(new JsonPrimitive(values[i]), GetExpectedRepresentation(values[i]), JsonType.Number);
                this.TestReadAsRoundtrip<uint>(new JsonPrimitive(values[i]), values[i]);
            }
        }

        /// <summary>
        /// Validates round-trip of <see cref="JsonPrimitive"/> values created from <see cref="UInt16"/> values.
        /// </summary>
        [Fact]
        public void JsonPrimitiveFromUInt16()
        {
            ushort[] values = new ushort[] { UInt16.MinValue, UInt16.MaxValue, 33333 };
            for (int i = 0; i < values.Length; i++)
            {
                this.ValidateJson(new JsonPrimitive(values[i]), GetExpectedRepresentation(values[i]), JsonType.Number);
                this.TestReadAsRoundtrip<ushort>(new JsonPrimitive(values[i]), values[i]);
            }
        }

        /// <summary>
        /// Validates round-trip of <see cref="JsonPrimitive"/> values created from <see cref="Byte"/> values.
        /// </summary>
        [Fact]
        public void JsonPrimitiveFromByte()
        {
            byte[] values = new byte[] { Byte.MinValue, Byte.MaxValue, 0x83 };
            for (int i = 0; i < values.Length; i++)
            {
                this.ValidateJson(new JsonPrimitive(values[i]), GetExpectedRepresentation(values[i]), JsonType.Number);
                this.TestReadAsRoundtrip<byte>(new JsonPrimitive(values[i]), values[i]);
            }
        }

        /// <summary>
        /// Validates round-trip of <see cref="JsonPrimitive"/> values created from <see cref="SByte"/> values.
        /// </summary>
        [Fact]
        public void JsonPrimitiveFromSByte()
        {
            sbyte[] values = new sbyte[] { SByte.MinValue, SByte.MaxValue, -0x33 };
            for (int i = 0; i < values.Length; i++)
            {
                this.ValidateJson(new JsonPrimitive(values[i]), GetExpectedRepresentation(values[i]), JsonType.Number);
                this.TestReadAsRoundtrip<sbyte>(new JsonPrimitive(values[i]), values[i]);
            }
        }

        /// <summary>
        /// Validates round-trip of <see cref="JsonPrimitive"/> values created from <see cref="Single"/> values.
        /// </summary>
        [Fact]
        public void JsonPrimitiveFromFloat()
        {
            float[] values = new float[] { float.MinValue, float.MaxValue, 1.234f, float.PositiveInfinity, float.NegativeInfinity, float.NaN };
            for (int i = 0; i < values.Length; i++)
            {
                this.ValidateJson(new JsonPrimitive(values[i]), GetExpectedRepresentation(values[i]), JsonType.Number);
                this.TestReadAsRoundtrip<float>(new JsonPrimitive(values[i]), values[i]);
            }
        }

        /// <summary>
        /// Validates round-trip of <see cref="JsonPrimitive"/> values created from <see cref="Double"/> values.
        /// </summary>
        [Fact]
        public void JsonPrimitiveFromDouble()
        {
            double[] values = new double[] { double.MinValue, double.MaxValue, 1.234, double.PositiveInfinity, double.NegativeInfinity, double.NaN };
            for (int i = 0; i < values.Length; i++)
            {
                this.ValidateJson(new JsonPrimitive(values[i]), GetExpectedRepresentation(values[i]), JsonType.Number);
                this.TestReadAsRoundtrip<double>(new JsonPrimitive(values[i]), values[i]);
            }
        }

        /// <summary>
        /// Validates round-trip of <see cref="JsonPrimitive"/> values created from <see cref="Decimal"/> values.
        /// </summary>
        [Fact]
        public void JsonPrimitiveFromDecimal()
        {
            decimal[] values = new decimal[] { decimal.MinValue, decimal.MaxValue, 123456789.123456789m };
            for (int i = 0; i < values.Length; i++)
            {
                this.ValidateJson(new JsonPrimitive(values[i]), GetExpectedRepresentation(values[i]), JsonType.Number);
                this.TestReadAsRoundtrip<decimal>(new JsonPrimitive(values[i]), values[i]);
            }
        }

        /// <summary>
        /// Validates round-trip of <see cref="JsonPrimitive"/> values created from <see cref="Boolean"/> values.
        /// </summary>
        [Fact]
        public void JsonPrimitiveFromBoolean()
        {
            bool[] values = new bool[] { true, false };
            for (int i = 0; i < values.Length; i++)
            {
                this.ValidateJson(new JsonPrimitive(values[i]), GetExpectedRepresentation(values[i]), JsonType.Boolean);
                this.TestReadAsRoundtrip<bool>(new JsonPrimitive(values[i]), values[i]);
            }
        }

        /// <summary>
        /// Validates round-trip of <see cref="JsonPrimitive"/> values created from <see cref="Char"/> values.
        /// </summary>
        [Fact]
        public void JsonPrimitiveFromChar()
        {
            char[] values = new char[] { 'H', '\0', '\uffff' };
            for (int i = 0; i < values.Length; i++)
            {
                this.ValidateJson(new JsonPrimitive(values[i]), GetExpectedRepresentation(values[i]), JsonType.String);
                this.TestReadAsRoundtrip<char>(new JsonPrimitive(values[i]), values[i]);
            }
        }

        /// <summary>
        /// Validates round-trip of <see cref="JsonPrimitive"/> values created from <see cref="String"/> values.
        /// </summary>
        [Fact]
        public void JsonPrimitiveFromString()
        {
            string[] values = new string[] { "Hello", "abcdef", "\r\t123\n32" };
            for (int i = 0; i < values.Length; i++)
            {
                this.ValidateJson(new JsonPrimitive(values[i]), GetExpectedRepresentation(values[i]), JsonType.String);
                this.TestReadAsRoundtrip<string>(new JsonPrimitive(values[i]), values[i]);
            }
        }

        /// <summary>
        /// Validates round-trip of <see cref="JsonPrimitive"/> values created from <see cref="DateTime"/> values.
        /// </summary>
        [Fact]
        public void JsonPrimitiveFromDateTime()
        {
            DateTime[] values = new DateTime[]
            {
                new DateTime(2000, 10, 16, 8, 0, 0, DateTimeKind.Utc),
                new DateTime(2000, 10, 16, 8, 0, 0, DateTimeKind.Local),
            };
            for (int i = 0; i < values.Length; i++)
            {
                this.ValidateJson(new JsonPrimitive(values[i]), GetExpectedRepresentation(values[i]), JsonType.String);
                this.TestReadAsRoundtrip<DateTime>(new JsonPrimitive(values[i]), values[i]);
            }
        }

        /// <summary>
        /// Validates round-trip of <see cref="JsonPrimitive"/> values created from <see cref="Uri"/> values.
        /// </summary>
        [Fact]
        public void JsonPrimitiveFromUri()
        {
            Uri[] values = new Uri[] { new Uri("http://tempuri.org"), new Uri("foo/bar", UriKind.Relative) };
            for (int i = 0; i < values.Length; i++)
            {
                this.ValidateJson(new JsonPrimitive(values[i]), GetExpectedRepresentation(values[i]), JsonType.String);
                this.TestReadAsRoundtrip<Uri>(new JsonPrimitive(values[i]), values[i]);
            }
        }

        /// <summary>
        /// Validates round-trip of <see cref="JsonPrimitive"/> values created from <see cref="Guid"/> values.
        /// </summary>
        [Fact]
        public void JsonPrimitiveFromGuid()
        {
            Guid[] values = new Guid[] { Guid.NewGuid(), Guid.Empty, Guid.NewGuid() };
            for (int i = 0; i < values.Length; i++)
            {
                this.ValidateJson(new JsonPrimitive(values[i]), GetExpectedRepresentation(values[i]), JsonType.String);
                this.TestReadAsRoundtrip<Guid>(new JsonPrimitive(values[i]), values[i]);
            }
        }

        /// <summary>
        /// Validates round-trip of <see cref="JsonPrimitive"/> values created from different types of values.
        /// </summary>
        [Fact]
        public void JsonPrimitiveFromObject()
        {
            List<KeyValuePair<object, JsonType>> values = new List<KeyValuePair<object, JsonType>>
            {
                new KeyValuePair<object, JsonType>(true, JsonType.Boolean),
                new KeyValuePair<object, JsonType>((short)1, JsonType.Number),
                new KeyValuePair<object, JsonType>(234, JsonType.Number),
                new KeyValuePair<object, JsonType>(3435434233443L, JsonType.Number),
                new KeyValuePair<object, JsonType>(UInt64.MaxValue, JsonType.Number),
                new KeyValuePair<object, JsonType>(UInt32.MaxValue, JsonType.Number),
                new KeyValuePair<object, JsonType>(UInt16.MaxValue, JsonType.Number),
                new KeyValuePair<object, JsonType>(Byte.MaxValue, JsonType.Number),
                new KeyValuePair<object, JsonType>(SByte.MinValue, JsonType.Number),
                new KeyValuePair<object, JsonType>(double.MaxValue, JsonType.Number),
                new KeyValuePair<object, JsonType>(float.Epsilon, JsonType.Number),
                new KeyValuePair<object, JsonType>(decimal.MinusOne, JsonType.Number),
                new KeyValuePair<object, JsonType>("hello", JsonType.String),
                new KeyValuePair<object, JsonType>(Guid.NewGuid(), JsonType.String),
                new KeyValuePair<object, JsonType>(DateTime.UtcNow, JsonType.String),
                new KeyValuePair<object, JsonType>(new Uri("http://www.microsoft.com"), JsonType.String),
            };

            foreach (var value in values)
            {
                string json = GetExpectedRepresentation(value.Key);
                JsonValue jsonValue = JsonValue.Parse(json);
                Assert.IsType(typeof(JsonPrimitive), jsonValue);
                this.ValidateJson((JsonPrimitive)jsonValue, json, value.Value);
            }
        }

        /// <summary>
        /// Negative tests for <see cref="JsonPrimitive"/> constructors with null values.
        /// </summary>
        [Fact]
        public void NullChecks()
        {
            ExpectException<ArgumentNullException>(() => new JsonPrimitive((string)null));
            ExpectException<ArgumentNullException>(() => new JsonPrimitive((Uri)null));
        }

        /// <summary>
        /// Tests for casting string values into non-string values.
        /// </summary>
        [Fact]
        public void CastingFromStringTests()
        {
            int seed = MethodBase.GetCurrentMethod().Name.GetHashCode();
            Random rndGen = new Random(seed);

            Assert.Equal(false, (bool)(new JsonPrimitive("false")));
            Assert.Equal(false, (bool)(new JsonPrimitive("False")));
            Assert.Equal(true, (bool)(new JsonPrimitive("true")));
            Assert.Equal(true, (bool)(new JsonPrimitive("True")));

            byte b = PrimitiveCreator.CreateInstanceOfByte(rndGen);
            Assert.Equal(b, (byte)(new JsonPrimitive(b.ToString(CultureInfo.InvariantCulture))));

            decimal dec = PrimitiveCreator.CreateInstanceOfDecimal(rndGen);
            Assert.Equal(dec, (decimal)(new JsonPrimitive(dec.ToString(CultureInfo.InvariantCulture))));

            double dbl = rndGen.NextDouble() * rndGen.Next();
            Assert.Equal(dbl, (double)(new JsonPrimitive(dbl.ToString("R", CultureInfo.InvariantCulture))));

            Assert.Equal(Double.PositiveInfinity, (double)(new JsonPrimitive("Infinity")));
            Assert.Equal(Double.NegativeInfinity, (double)(new JsonPrimitive("-Infinity")));
            Assert.Equal(Double.NaN, (double)(new JsonPrimitive("NaN")));

            ExpectException<InvalidCastException>(delegate { var d = (double)(new JsonPrimitive("INF")); });
            ExpectException<InvalidCastException>(delegate { var d = (double)(new JsonPrimitive("-INF")); });
            ExpectException<InvalidCastException>(delegate { var d = (double)(new JsonPrimitive("infinity")); });
            ExpectException<InvalidCastException>(delegate { var d = (double)(new JsonPrimitive("INFINITY")); });
            ExpectException<InvalidCastException>(delegate { var d = (double)(new JsonPrimitive("nan")); });
            ExpectException<InvalidCastException>(delegate { var d = (double)(new JsonPrimitive("Nan")); });

            float flt = (float)(rndGen.NextDouble() * rndGen.Next());
            Assert.Equal(flt, (float)(new JsonPrimitive(flt.ToString("R", CultureInfo.InvariantCulture))));

            Assert.Equal(Single.PositiveInfinity, (float)(new JsonPrimitive("Infinity")));
            Assert.Equal(Single.NegativeInfinity, (float)(new JsonPrimitive("-Infinity")));
            Assert.Equal(Single.NaN, (float)(new JsonPrimitive("NaN")));

            ExpectException<InvalidCastException>(delegate { var f = (float)(new JsonPrimitive("INF")); });
            ExpectException<InvalidCastException>(delegate { var f = (float)(new JsonPrimitive("-INF")); });
            ExpectException<InvalidCastException>(delegate { var f = (float)(new JsonPrimitive("infinity")); });
            ExpectException<InvalidCastException>(delegate { var f = (float)(new JsonPrimitive("INFINITY")); });
            ExpectException<InvalidCastException>(delegate { var f = (float)(new JsonPrimitive("nan")); });
            ExpectException<InvalidCastException>(delegate { var f = (float)(new JsonPrimitive("Nan")); });

            int i = PrimitiveCreator.CreateInstanceOfInt32(rndGen);
            Assert.Equal(i, (int)(new JsonPrimitive(i.ToString(CultureInfo.InvariantCulture))));

            long l = PrimitiveCreator.CreateInstanceOfInt64(rndGen);
            Assert.Equal(l, (long)(new JsonPrimitive(l.ToString(CultureInfo.InvariantCulture))));

            sbyte sb = PrimitiveCreator.CreateInstanceOfSByte(rndGen);
            Assert.Equal(sb, (sbyte)(new JsonPrimitive(sb.ToString(CultureInfo.InvariantCulture))));

            short s = PrimitiveCreator.CreateInstanceOfInt16(rndGen);
            Assert.Equal(s, (short)(new JsonPrimitive(s.ToString(CultureInfo.InvariantCulture))));

            ushort ui16 = PrimitiveCreator.CreateInstanceOfUInt16(rndGen);
            Assert.Equal(ui16, (ushort)(new JsonPrimitive(ui16.ToString(CultureInfo.InvariantCulture))));

            uint ui32 = PrimitiveCreator.CreateInstanceOfUInt32(rndGen);
            Assert.Equal(ui32, (uint)(new JsonPrimitive(ui32.ToString(CultureInfo.InvariantCulture))));

            ulong ui64 = PrimitiveCreator.CreateInstanceOfUInt64(rndGen);
            Assert.Equal(ui64, (ulong)(new JsonPrimitive(ui64.ToString(CultureInfo.InvariantCulture))));
        }

        /// <summary>
        /// Tests for casting <see cref="JsonPrimitive"/> created from special floating point values (infinity, NaN).
        /// </summary>
        [Fact]
        public void CastingNumbersTest()
        {
            Assert.Equal(float.PositiveInfinity, (float)(new JsonPrimitive(double.PositiveInfinity)));
            Assert.Equal(float.NegativeInfinity, (float)(new JsonPrimitive(double.NegativeInfinity)));
            Assert.Equal(float.NaN, (float)(new JsonPrimitive(double.NaN)));

            Assert.Equal(double.PositiveInfinity, (double)(new JsonPrimitive(float.PositiveInfinity)));
            Assert.Equal(double.NegativeInfinity, (double)(new JsonPrimitive(float.NegativeInfinity)));
            Assert.Equal(double.NaN, (double)(new JsonPrimitive(float.NaN)));
        }

        /// <summary>
        /// Tests for the many formats which can be cast to a <see cref="DateTime"/>.
        /// </summary>
        [Fact]
        public void CastingDateTimeTest()
        {
            int seed = MethodBase.GetCurrentMethod().Name.GetHashCode();
            Random rndGen = new Random(seed);
            DateTime dt = new DateTime(
                rndGen.Next(1000, 3000), // year
                rndGen.Next(1, 13), // month
                rndGen.Next(1, 28), // day
                rndGen.Next(0, 24), // hour
                rndGen.Next(0, 60), // minute
                rndGen.Next(0, 60), // second
                DateTimeKind.Utc);
            Log.Info("dt = {0}", dt);

            const string JsonDateFormat = "yyyy-MM-ddTHH:mm:ssZ";
            string dateString = dt.ToString(JsonDateFormat, CultureInfo.InvariantCulture);
            JsonValue jv = dateString;
            DateTime dt2 = (DateTime)jv;
            Assert.Equal(dt.ToUniversalTime(), dt2.ToUniversalTime());

            const string DateTimeLocalFormat = "yyyy-MM-ddTHH:mm:ss";
            const string DateLocalFormat = "yyyy-MM-dd";
            const string TimeLocalFormat = "HH:mm:ss";

            for (int i = 0; i < 100; i++)
            {
                DateTime dateLocal = PrimitiveCreator.CreateInstanceOfDateTime(rndGen).ToLocalTime();
                dateLocal = new DateTime(dateLocal.Year, dateLocal.Month, dateLocal.Day, dateLocal.Hour, dateLocal.Minute, dateLocal.Second, DateTimeKind.Local);
                string localDateTime = dateLocal.ToString(DateTimeLocalFormat, CultureInfo.InvariantCulture);
                string localDate = dateLocal.ToString(DateLocalFormat, CultureInfo.InvariantCulture);
                string localTime = dateLocal.ToString(TimeLocalFormat, CultureInfo.InvariantCulture);

                Assert.Equal(dateLocal, new JsonPrimitive(localDateTime).ReadAs<DateTime>());
                Assert.Equal(dateLocal.Date, new JsonPrimitive(localDate).ReadAs<DateTime>());
                DateTime timeOnly = new JsonPrimitive(localTime).ReadAs<DateTime>();
                Assert.Equal(dateLocal.Hour, timeOnly.Hour);
                Assert.Equal(dateLocal.Minute, timeOnly.Minute);
                Assert.Equal(dateLocal.Second, timeOnly.Second);

                DataContractJsonSerializer dcjs = new DataContractJsonSerializer(typeof(DateTime));
                using (MemoryStream ms = new MemoryStream())
                {
                    dcjs.WriteObject(ms, dateLocal);
                    ms.Position = 0;
                    JsonValue jvFromString = JsonValue.Load(ms);
                    Assert.Equal(dateLocal, jvFromString.ReadAs<DateTime>());
                }

                using (MemoryStream ms = new MemoryStream())
                {
                    DateTime dateUtc = dateLocal.ToUniversalTime();
                    dcjs.WriteObject(ms, dateUtc);
                    ms.Position = 0;
                    JsonValue jvFromString = JsonValue.Load(ms);
                    Assert.Equal(dateUtc, jvFromString.ReadAs<DateTime>());
                }
            }
        }

        /// <summary>
        /// Tests for date parsing form the RFC2822 format.
        /// </summary>
        [Fact]
        public void Rfc2822DateTimeFormatTest()
        {
            string[] localFormats = new string[]
            {
                "ddd, d MMM yyyy HH:mm:ss zzz",
                "d MMM yyyy HH:mm:ss zzz",
                "ddd, dd MMM yyyy HH:mm:ss zzz",
                "ddd, dd MMM yyyy HH:mm zzz",
            };

            string[] utcFormats = new string[]
            {
                @"ddd, d MMM yyyy HH:mm:ss \U\T\C",
                "d MMM yyyy HH:mm:ssZ",
                @"ddd, dd MMM yyyy HH:mm:ss \U\T\C",
                "ddd, dd MMM yyyy HH:mmZ",
            };

            DateTime today = DateTime.Today;
            int seed = today.Year * 10000 + today.Month * 100 + today.Day;
            Log.Info("Seed: {0}", seed);
            Random rndGen = new Random(seed);
            const int DatesToTry = 100;
            const string DateTraceFormat = "ddd yyyy/MM/dd HH:mm:ss.fffZ";

            for (int i = 0; i < DatesToTry; i++)
            {
                DateTime dt = PrimitiveCreator.CreateInstanceOfDateTime(rndGen);
                dt = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, dt.Kind);
                Log.Info("Test with date: {0} ({1})", dt.ToString(DateTraceFormat, CultureInfo.InvariantCulture), dt.Kind);
                string[] formatsToTest = dt.Kind == DateTimeKind.Utc ? utcFormats : localFormats;
                foreach (string format in formatsToTest)
                {
                    string strDate = dt.ToString(format, CultureInfo.InvariantCulture);
                    Log.Info("As string: {0} (format = {1})", strDate, format);
                    JsonPrimitive jp = new JsonPrimitive(strDate);
                    DateTime parsedDate = jp.ReadAs<DateTime>();
                    Log.Info("Parsed date: {0} ({1})", parsedDate.ToString(DateTraceFormat, CultureInfo.InvariantCulture), parsedDate.Kind);

                    DateTime dtExpected = dt;
                    DateTime dtActual = parsedDate;

                    if (dt.Kind != parsedDate.Kind)
                    {
                        dtExpected = dtExpected.ToUniversalTime();
                        dtActual = dtActual.ToUniversalTime();
                    }

                    Assert.Equal(dtExpected.Year, dtActual.Year);
                    Assert.Equal(dtExpected.Month, dtActual.Month);
                    Assert.Equal(dtExpected.Day, dtActual.Day);
                    Assert.Equal(dtExpected.Hour, dtActual.Hour);
                    Assert.Equal(dtExpected.Minute, dtActual.Minute);
                    if (format.Contains(":ss"))
                    {
                        Assert.Equal(dtExpected.Second, dtActual.Second);
                    }
                    else
                    {
                        Assert.Equal(0, parsedDate.Second);
                    }
                }

                Log.Info("");
            }
        }

        /// <summary>
        /// Tests for the <see cref="System.Json.JsonValue.ReadAs{T}()"/> function from string values.
        /// </summary>
        [Fact]
        public void ReadAsFromStringTests()
        {
            int seed = MethodBase.GetCurrentMethod().Name.GetHashCode();
            Random rndGen = new Random(seed);

            TestReadAsFromStringRoundtrip<bool>(false, "false");
            TestReadAsFromStringRoundtrip<bool>(false, "False");
            TestReadAsFromStringRoundtrip<bool>(true, "true");
            TestReadAsFromStringRoundtrip<bool>(true, "True");
            TestReadAsFromStringRoundtrip<byte>(PrimitiveCreator.CreateInstanceOfByte(rndGen));
            TestReadAsFromStringRoundtrip<char>(PrimitiveCreator.CreateInstanceOfChar(rndGen));
            TestReadAsFromStringRoundtrip<decimal>(PrimitiveCreator.CreateInstanceOfDecimal(rndGen));
            TestReadAsFromStringRoundtrip<int>(PrimitiveCreator.CreateInstanceOfInt32(rndGen));
            TestReadAsFromStringRoundtrip<long>(PrimitiveCreator.CreateInstanceOfInt64(rndGen));
            TestReadAsFromStringRoundtrip<sbyte>(PrimitiveCreator.CreateInstanceOfSByte(rndGen));
            TestReadAsFromStringRoundtrip<short>(PrimitiveCreator.CreateInstanceOfInt16(rndGen));
            TestReadAsFromStringRoundtrip<ushort>(PrimitiveCreator.CreateInstanceOfUInt16(rndGen));
            TestReadAsFromStringRoundtrip<uint>(PrimitiveCreator.CreateInstanceOfUInt32(rndGen));
            TestReadAsFromStringRoundtrip<ulong>(PrimitiveCreator.CreateInstanceOfUInt64(rndGen));
            double dbl = rndGen.NextDouble() * rndGen.Next();
            TestReadAsFromStringRoundtrip<double>(dbl, dbl.ToString("R", CultureInfo.InvariantCulture));
            TestReadAsFromStringRoundtrip<double>(double.PositiveInfinity, "Infinity");
            TestReadAsFromStringRoundtrip<double>(double.NegativeInfinity, "-Infinity");
            TestReadAsFromStringRoundtrip<double>(double.NaN, "NaN");
            float flt = (float)(rndGen.NextDouble() * rndGen.Next());
            TestReadAsFromStringRoundtrip<float>(flt, flt.ToString("R", CultureInfo.InvariantCulture));
            TestReadAsFromStringRoundtrip<float>(float.PositiveInfinity, "Infinity");
            TestReadAsFromStringRoundtrip<float>(float.NegativeInfinity, "-Infinity");
            TestReadAsFromStringRoundtrip<float>(float.NaN, "NaN");
            Guid guid = PrimitiveCreator.CreateInstanceOfGuid(rndGen);
            TestReadAsFromStringRoundtrip<Guid>(guid, guid.ToString("N", CultureInfo.InvariantCulture));
            TestReadAsFromStringRoundtrip<Guid>(guid, guid.ToString("D", CultureInfo.InvariantCulture));
            TestReadAsFromStringRoundtrip<Guid>(guid, guid.ToString("B", CultureInfo.InvariantCulture));
            TestReadAsFromStringRoundtrip<Guid>(guid, guid.ToString("P", CultureInfo.InvariantCulture));
            TestReadAsFromStringRoundtrip<Guid>(guid, guid.ToString("X", CultureInfo.InvariantCulture));
            TestReadAsFromStringRoundtrip<Guid>(guid, guid.ToString("X", CultureInfo.InvariantCulture).Replace("0x", "0X"));
            TestReadAsFromStringRoundtrip<Guid>(guid, guid.ToString("X", CultureInfo.InvariantCulture).Replace("{", " { "));
            TestReadAsFromStringRoundtrip<Guid>(guid, guid.ToString("X", CultureInfo.InvariantCulture).Replace("}", " } "));
            TestReadAsFromStringRoundtrip<Guid>(guid, guid.ToString("X", CultureInfo.InvariantCulture).Replace(",", "  ,    "));
            TestReadAsFromStringRoundtrip<Guid>(guid, guid.ToString("X", CultureInfo.InvariantCulture).Replace("0x", "0x0000"));
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

            TestReadAsFromStringRoundtrip<Uri>(uri);
            TestReadAsFromStringRoundtrip<string>(PrimitiveCreator.CreateInstanceOfString(rndGen));

            // Roundtrip reference DateTime to remove some of the precision in the ticks. Otherwise, value is too precise.
            DateTimeOffset dateTimeOffset = PrimitiveCreator.CreateInstanceOfDateTimeOffset(rndGen);
            const string ISO8601Format = "yyyy-MM-ddTHH:mm:sszzz";
            dateTimeOffset = DateTimeOffset.ParseExact(dateTimeOffset.ToString(ISO8601Format, CultureInfo.InvariantCulture), ISO8601Format, CultureInfo.InvariantCulture);
            DateTime dateTime = dateTimeOffset.UtcDateTime;
            TestReadAsFromStringRoundtrip<DateTime>(dateTime, dateTimeOffset.ToUniversalTime().ToString(@"ddd, d MMM yyyy HH:mm:ss \U\T\C"));
            TestReadAsFromStringRoundtrip<DateTime>(dateTime, dateTimeOffset.ToUniversalTime().ToString(@"ddd, d MMM yyyy HH:mm:ss \G\M\T"));
            TestReadAsFromStringRoundtrip<DateTime>(dateTime.ToLocalTime(), dateTimeOffset.ToString(@"ddd, d MMM yyyy HH:mm:ss zzz"));
            TestReadAsFromStringRoundtrip<DateTime>(dateTime, dateTime.ToString("yyyy-MM-ddTHH:mm:ssK"));
            TestReadAsFromStringRoundtrip<DateTime>(dateTime.ToLocalTime(), dateTimeOffset.ToString(@"ddd, d MMM yyyy HH:mm:ss zzz"));
            TestReadAsFromStringRoundtrip<DateTime>(dateTime.ToLocalTime(), dateTimeOffset.ToString("yyyy-MM-ddTHH:mm:sszzz"));
            TestReadAsFromStringRoundtrip<DateTimeOffset>(dateTimeOffset.UtcDateTime, dateTimeOffset.ToUniversalTime().ToString(@"ddd, d MMM yyyy HH:mm:ss \U\T\C"));
            TestReadAsFromStringRoundtrip<DateTimeOffset>(dateTimeOffset.UtcDateTime, dateTimeOffset.ToUniversalTime().ToString(@"ddd, d MMM yyyy HH:mm:ss \G\M\T"));
            TestReadAsFromStringRoundtrip<DateTimeOffset>(dateTimeOffset, dateTimeOffset.ToUniversalTime().ToString(@"ddd, d MMM yyyy HH:mm:ss zzz"));
            TestReadAsFromStringRoundtrip<DateTimeOffset>(dateTimeOffset, dateTime.ToString("yyyy-MM-ddTHH:mm:ssK"));
            TestReadAsFromStringRoundtrip<DateTimeOffset>(dateTimeOffset, dateTimeOffset.ToString("yyyy-MM-ddTHH:mm:sszzz"));
            TestReadAsFromStringRoundtrip<DateTimeOffset>(dateTimeOffset, dateTimeOffset.ToString(@"ddd, d MMM yyyy HH:mm:ss zzz"));

            // Create ASPNetFormat DateTime
            long unixEpochMilliseconds = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks / 10000;
            long millisecondsFromUnixEpoch = dateTime.Ticks / 10000 - unixEpochMilliseconds;
            string AspNetFormattedDateTime = String.Format("/Date({0})/", millisecondsFromUnixEpoch);
            string AspNetFormattedDateTimeWithValidTZ = String.Format("/Date({0}+0700)/", millisecondsFromUnixEpoch);
            string AspNetFormattedDateTimeInvalid1 = String.Format("/Date({0}+99999)/", millisecondsFromUnixEpoch);
            string AspNetFormattedDateTimeInvalid2 = String.Format("/Date({0}+07z0)/", millisecondsFromUnixEpoch);
            TestReadAsFromStringRoundtrip<DateTime>(dateTime, AspNetFormattedDateTime);
            TestReadAsFromStringRoundtrip<DateTime>(dateTime.ToLocalTime(), AspNetFormattedDateTimeWithValidTZ);
            TestReadAsFromStringRoundtrip<DateTimeOffset>(dateTimeOffset, AspNetFormattedDateTime);
            TestReadAsFromStringRoundtrip<DateTimeOffset>(dateTimeOffset, AspNetFormattedDateTimeWithValidTZ);

            ExpectException<FormatException>(delegate { new JsonPrimitive(AspNetFormattedDateTimeInvalid1).ReadAs<DateTime>(); });
            ExpectException<FormatException>(delegate { new JsonPrimitive(AspNetFormattedDateTimeInvalid2).ReadAs<DateTime>(); });
            ExpectException<FormatException>(delegate { new JsonPrimitive(AspNetFormattedDateTimeInvalid1).ReadAs<DateTimeOffset>(); });
            ExpectException<FormatException>(delegate { new JsonPrimitive(AspNetFormattedDateTimeInvalid2).ReadAs<DateTimeOffset>(); });

            ExpectException<FormatException>(delegate { new JsonPrimitive("INF").ReadAs<float>(); });
            ExpectException<FormatException>(delegate { new JsonPrimitive("-INF").ReadAs<float>(); });
            ExpectException<FormatException>(delegate { new JsonPrimitive("infinity").ReadAs<float>(); });
            ExpectException<FormatException>(delegate { new JsonPrimitive("INFINITY").ReadAs<float>(); });
            ExpectException<FormatException>(delegate { new JsonPrimitive("nan").ReadAs<float>(); });
            ExpectException<FormatException>(delegate { new JsonPrimitive("Nan").ReadAs<float>(); });

            ExpectException<FormatException>(delegate { new JsonPrimitive("INF").ReadAs<double>(); });
            ExpectException<FormatException>(delegate { new JsonPrimitive("-INF").ReadAs<double>(); });
            ExpectException<FormatException>(delegate { new JsonPrimitive("infinity").ReadAs<double>(); });
            ExpectException<FormatException>(delegate { new JsonPrimitive("INFINITY").ReadAs<double>(); });
            ExpectException<FormatException>(delegate { new JsonPrimitive("nan").ReadAs<double>(); });
            ExpectException<FormatException>(delegate { new JsonPrimitive("Nan").ReadAs<double>(); });
        }

        /// <summary>
        /// Tests for the <see cref="System.Json.JsonValue.ReadAs{T}()">JsonValue.ReadAs&lt;string&gt;</see> method from number values.
        /// </summary>
        [Fact]
        public void TestReadAsStringFromNumbers()
        {
            int seed = MethodBase.GetCurrentMethod().Name.GetHashCode();
            Random rndGen = new Random(seed);

            int intValue = PrimitiveCreator.CreateInstanceOfInt32(rndGen);
            JsonValue jv = intValue;
            Assert.Equal(intValue.ToString(CultureInfo.InvariantCulture), jv.ToString());
            Assert.Equal(intValue.ToString(CultureInfo.InvariantCulture), jv.ReadAs<string>());

            uint uintValue = PrimitiveCreator.CreateInstanceOfUInt32(rndGen);
            jv = uintValue;
            Assert.Equal(uintValue.ToString(CultureInfo.InvariantCulture), jv.ToString());
            Assert.Equal(uintValue.ToString(CultureInfo.InvariantCulture), jv.ReadAs<string>());

            long longValue = PrimitiveCreator.CreateInstanceOfInt64(rndGen);
            jv = longValue;
            Assert.Equal(longValue.ToString(CultureInfo.InvariantCulture), jv.ToString());
            Assert.Equal(longValue.ToString(CultureInfo.InvariantCulture), jv.ReadAs<string>());

            ulong ulongValue = PrimitiveCreator.CreateInstanceOfUInt64(rndGen);
            jv = ulongValue;
            Assert.Equal(ulongValue.ToString(CultureInfo.InvariantCulture), jv.ToString());
            Assert.Equal(ulongValue.ToString(CultureInfo.InvariantCulture), jv.ReadAs<string>());

            short shortValue = PrimitiveCreator.CreateInstanceOfInt16(rndGen);
            jv = shortValue;
            Assert.Equal(shortValue.ToString(CultureInfo.InvariantCulture), jv.ToString());
            Assert.Equal(shortValue.ToString(CultureInfo.InvariantCulture), jv.ReadAs<string>());

            ushort ushortValue = PrimitiveCreator.CreateInstanceOfUInt16(rndGen);
            jv = ushortValue;
            Assert.Equal(ushortValue.ToString(CultureInfo.InvariantCulture), jv.ToString());
            Assert.Equal(ushortValue.ToString(CultureInfo.InvariantCulture), jv.ReadAs<string>());

            byte byteValue = PrimitiveCreator.CreateInstanceOfByte(rndGen);
            jv = byteValue;
            Assert.Equal(byteValue.ToString(CultureInfo.InvariantCulture), jv.ToString());
            Assert.Equal(byteValue.ToString(CultureInfo.InvariantCulture), jv.ReadAs<string>());

            sbyte sbyteValue = PrimitiveCreator.CreateInstanceOfSByte(rndGen);
            jv = sbyteValue;
            Assert.Equal(sbyteValue.ToString(CultureInfo.InvariantCulture), jv.ToString());
            Assert.Equal(sbyteValue.ToString(CultureInfo.InvariantCulture), jv.ReadAs<string>());

            decimal decValue = PrimitiveCreator.CreateInstanceOfDecimal(rndGen);
            jv = decValue;
            Assert.Equal(decValue.ToString(CultureInfo.InvariantCulture), jv.ToString());
            Assert.Equal(decValue.ToString(CultureInfo.InvariantCulture), jv.ReadAs<string>());

            float fltValue = PrimitiveCreator.CreateInstanceOfSingle(rndGen);
            jv = fltValue;
            Assert.Equal(fltValue.ToString("R", CultureInfo.InvariantCulture), jv.ToString());
            Assert.Equal(fltValue.ToString("R", CultureInfo.InvariantCulture), jv.ReadAs<string>());

            double dblValue = PrimitiveCreator.CreateInstanceOfDouble(rndGen);
            jv = dblValue;
            Assert.Equal(dblValue.ToString("R", CultureInfo.InvariantCulture), jv.ToString());
            Assert.Equal(dblValue.ToString("R", CultureInfo.InvariantCulture), jv.ReadAs<string>());
        }

        /// <summary>
        /// Tests for the <see cref="System.Json.JsonValue.ReadAs{T}()">JsonValue.ReadAs&lt;string&gt;</see> method from date values.
        /// </summary>
        [Fact]
        public void TestReadAsStringFromDates()
        {
            int seed = MethodBase.GetCurrentMethod().Name.GetHashCode();
            Random rndGen = new Random(seed);

            DateTime dateTimeValue = PrimitiveCreator.CreateInstanceOfDateTime(rndGen);
            JsonValue jv = dateTimeValue;
            Assert.Equal("\"" + dateTimeValue.ToString(DateTimeFormat, CultureInfo.InvariantCulture) + "\"", jv.ToString());
            Assert.Equal(dateTimeValue.ToString(DateTimeFormat, CultureInfo.InvariantCulture), jv.ReadAs<string>());
        }

        /// <summary>
        /// Tests for the <see cref="System.Json.JsonValue.ReadAs{T}()">JsonValue.ReadAs&lt;string&gt;</see> method from char values.
        /// </summary>
        [Fact]
        public void TestReadAsStringFromChar()
        {
            char[] chars = "abc\u0000\b\f\r\n\t\ufedc".ToCharArray();

            foreach (char c in chars)
            {
                string expected = new string(c, 1);
                JsonValue jv = c;
                string actual1 = jv.ReadAs<string>();
                string actual2 = (string)jv;

                Assert.Equal(expected, actual1);
                Assert.Equal(expected, actual2);
            }
        }

        /// <summary>
        /// Tests for the <see cref="System.Json.JsonValue.ReadAs{T}()"/> method where T is a number type and the value is created from a string.
        /// </summary>
        [Fact]
        public void TestReadAsNumberFromStrings()
        {
            Dictionary<object, List<Type>> valuesToNonOverflowingTypesMapping = new Dictionary<object, List<Type>>
            {
                { double.NaN.ToString("R", CultureInfo.InvariantCulture), new List<Type> { typeof(float), typeof(double) } },
                { double.NegativeInfinity.ToString("R", CultureInfo.InvariantCulture), new List<Type> { typeof(float), typeof(double) } },
                { double.PositiveInfinity.ToString("R", CultureInfo.InvariantCulture), new List<Type> { typeof(float), typeof(double) } },
                { double.MaxValue.ToString("R", CultureInfo.InvariantCulture), new List<Type> { typeof(double), typeof(float) } },
                { double.MinValue.ToString("R", CultureInfo.InvariantCulture), new List<Type> { typeof(double), typeof(float) } },
                { float.MaxValue.ToString("R", CultureInfo.InvariantCulture), new List<Type> { typeof(double), typeof(float) } },
                { float.MinValue.ToString("R", CultureInfo.InvariantCulture), new List<Type> { typeof(double), typeof(float) } },
                { Int64.MaxValue.ToString(), new List<Type> { typeof(double), typeof(float), typeof(decimal), typeof(long), typeof(ulong) } },
                { Int64.MinValue.ToString(), new List<Type> { typeof(double), typeof(float), typeof(decimal), typeof(long) } },
                { Int32.MaxValue.ToString(), new List<Type> { typeof(double), typeof(float), typeof(decimal), typeof(long), typeof(int), typeof(ulong), typeof(uint) } },
                { Int32.MinValue.ToString(), new List<Type> { typeof(double), typeof(float), typeof(decimal), typeof(long), typeof(int) } },
                { Int16.MaxValue.ToString(), new List<Type> { typeof(double), typeof(float), typeof(decimal), typeof(long), typeof(int), typeof(short), typeof(ulong), typeof(uint), typeof(ushort) } },
                { Int16.MinValue.ToString(), new List<Type> { typeof(double), typeof(float), typeof(decimal), typeof(long), typeof(int), typeof(short) } },
                { SByte.MaxValue.ToString(), new List<Type> { typeof(double), typeof(float), typeof(decimal), typeof(long), typeof(int), typeof(short), typeof(sbyte), typeof(ulong), typeof(uint), typeof(ushort), typeof(byte) } },
                { SByte.MinValue.ToString(), new List<Type> { typeof(double), typeof(float), typeof(decimal), typeof(long), typeof(int), typeof(short), typeof(sbyte) } },
                { UInt64.MaxValue.ToString(), new List<Type> { typeof(double), typeof(float), typeof(decimal), typeof(ulong) } },
                { UInt32.MaxValue.ToString(), new List<Type> { typeof(double), typeof(float), typeof(decimal), typeof(long), typeof(ulong), typeof(uint) } },
                { UInt16.MaxValue.ToString(), new List<Type> { typeof(double), typeof(float), typeof(decimal), typeof(long), typeof(int), typeof(ulong), typeof(uint), typeof(ushort) } },
                { Byte.MaxValue.ToString(), new List<Type> { typeof(double), typeof(float), typeof(decimal), typeof(long), typeof(int), typeof(short), typeof(ulong), typeof(uint), typeof(ushort), typeof(byte) } },
                { Byte.MinValue.ToString(), new List<Type> { typeof(double), typeof(float), typeof(decimal), typeof(long), typeof(int), typeof(short), typeof(sbyte), typeof(ulong), typeof(uint), typeof(ushort), typeof(byte) } },
                { "1", new List<Type> { typeof(double), typeof(float), typeof(decimal), typeof(long), typeof(int), typeof(short), typeof(sbyte), typeof(ulong), typeof(uint), typeof(ushort), typeof(byte) } },
                { "+01", new List<Type> { typeof(double), typeof(float), typeof(decimal), typeof(long), typeof(int), typeof(short), typeof(sbyte), typeof(ulong), typeof(uint), typeof(ushort), typeof(byte) } },
                { "01.1e+01", new List<Type> { typeof(double), typeof(float), typeof(decimal), typeof(long), typeof(int), typeof(short), typeof(sbyte), typeof(ulong), typeof(uint), typeof(ushort), typeof(byte) } },
                { "1e1", new List<Type> { typeof(double), typeof(float), typeof(decimal), typeof(long), typeof(int), typeof(short), typeof(sbyte), typeof(ulong), typeof(uint), typeof(ushort), typeof(byte) } },
                { "1.0", new List<Type> { typeof(double), typeof(float), typeof(decimal), typeof(long), typeof(int), typeof(short), typeof(sbyte), typeof(ulong), typeof(uint), typeof(ushort), typeof(byte) } },
                { "01.0", new List<Type> { typeof(double), typeof(float), typeof(decimal), typeof(long), typeof(int), typeof(short), typeof(sbyte), typeof(ulong), typeof(uint), typeof(ushort), typeof(byte) } },
                { "-1", new List<Type> { typeof(double), typeof(float), typeof(decimal), typeof(long), typeof(int), typeof(short), typeof(sbyte) } },
                { "-1.0", new List<Type> { typeof(double), typeof(float), typeof(decimal), typeof(long), typeof(int), typeof(short), typeof(sbyte) } },
                { "-01.0", new List<Type> { typeof(double), typeof(float), typeof(decimal), typeof(long), typeof(int), typeof(short), typeof(sbyte) } },
                { "-01.0e+01", new List<Type> { typeof(double), typeof(float), typeof(decimal), typeof(long), typeof(int), typeof(short), typeof(sbyte) } },
                { "-01.0e-01", new List<Type> { typeof(double), typeof(float), typeof(decimal), typeof(long), typeof(int), typeof(short), typeof(sbyte), typeof(ulong), typeof(uint), typeof(ushort), typeof(byte) } },
                { "-.1", new List<Type> { typeof(double), typeof(float), typeof(decimal), typeof(long), typeof(int), typeof(short), typeof(sbyte), typeof(ulong), typeof(uint), typeof(ushort), typeof(byte) } },
                { "-0100.0e-1", new List<Type> { typeof(double), typeof(float), typeof(decimal), typeof(long), typeof(int), typeof(short), typeof(sbyte) } },
            };

            foreach (KeyValuePair<object, List<Type>> mapping in valuesToNonOverflowingTypesMapping)
            {
                ConvertValueToNumber<double>(mapping);
                ConvertValueToNumber<float>(mapping);
                ConvertValueToNumber<decimal>(mapping);
                ConvertValueToNumber<long>(mapping);
                ConvertValueToNumber<int>(mapping);
                ConvertValueToNumber<short>(mapping);
                ConvertValueToNumber<sbyte>(mapping);
                ConvertValueToNumber<ulong>(mapping);
                ConvertValueToNumber<uint>(mapping);
                ConvertValueToNumber<ushort>(mapping);
                ConvertValueToNumber<byte>(mapping);
            }

            Dictionary<object, List<Type>> valuesThatAreInvalidNumber = new Dictionary<object, List<Type>>
            {
                { "1L", new List<Type> { } },
                { "0x1", new List<Type> { } },
                { "1e309", new List<Type> { } },
                { "", new List<Type> { } },
                { "-", new List<Type> { } },
                { "e10", new List<Type> { } },
            };

            foreach (KeyValuePair<object, List<Type>> mapping in valuesThatAreInvalidNumber)
            {
                ConvertValueToNumber<double, FormatException>(mapping);
                ConvertValueToNumber<float, FormatException>(mapping);
                ConvertValueToNumber<decimal, FormatException>(mapping);
                ConvertValueToNumber<long, FormatException>(mapping);
                ConvertValueToNumber<int, FormatException>(mapping);
                ConvertValueToNumber<short, FormatException>(mapping);
                ConvertValueToNumber<sbyte, FormatException>(mapping);
                ConvertValueToNumber<ulong, FormatException>(mapping);
                ConvertValueToNumber<uint, FormatException>(mapping);
                ConvertValueToNumber<ushort, FormatException>(mapping);
                ConvertValueToNumber<byte, FormatException>(mapping);
            }
        }

        /// <summary>
        /// Tests for the <see cref="System.Json.JsonValue.ReadAs{T}()"/> method where T is a number type and the value is created from a number.
        /// This is essentially a number conversion test.
        /// </summary>
        [Fact]
        public void TestReadAsNumberFromNumber()
        {
            Dictionary<object, List<Type>> valuesToNonOverflowingTypesMapping = new Dictionary<object, List<Type>>
            { 
                { double.NaN, new List<Type> { typeof(float), typeof(double) } },
                { double.NegativeInfinity, new List<Type> { typeof(float), typeof(double) } },
                { double.PositiveInfinity, new List<Type> { typeof(float), typeof(double) } },
                { float.NaN, new List<Type> { typeof(float), typeof(double) } },
                { float.NegativeInfinity, new List<Type> { typeof(float), typeof(double) } },
                { float.PositiveInfinity, new List<Type> { typeof(float), typeof(double) } },
                { double.MaxValue, new List<Type> { typeof(double), typeof(float) } },
                { double.MinValue, new List<Type> { typeof(double), typeof(float) } },
                { float.MaxValue, new List<Type> { typeof(double), typeof(float) } },
                { float.MinValue, new List<Type> { typeof(double), typeof(float) } },
                { Int64.MaxValue, new List<Type> { typeof(double), typeof(float), typeof(decimal), typeof(long), typeof(ulong) } },
                { Int64.MinValue, new List<Type> { typeof(double), typeof(float), typeof(decimal), typeof(long) } },
                { Int32.MaxValue, new List<Type> { typeof(double), typeof(float), typeof(decimal), typeof(long), typeof(int), typeof(ulong), typeof(uint) } },
                { Int32.MinValue, new List<Type> { typeof(double), typeof(float), typeof(decimal), typeof(long), typeof(int) } },
                { Int16.MaxValue, new List<Type> { typeof(double), typeof(float), typeof(decimal), typeof(long), typeof(int), typeof(short), typeof(ulong), typeof(uint), typeof(ushort) } },
                { Int16.MinValue, new List<Type> { typeof(double), typeof(float), typeof(decimal), typeof(long), typeof(int), typeof(short) } },
                { SByte.MaxValue, new List<Type> { typeof(double), typeof(float), typeof(decimal), typeof(long), typeof(int), typeof(short), typeof(sbyte), typeof(ulong), typeof(uint), typeof(ushort), typeof(byte) } },
                { SByte.MinValue, new List<Type> { typeof(double), typeof(float), typeof(decimal), typeof(long), typeof(int), typeof(short), typeof(sbyte) } },
                { UInt64.MaxValue, new List<Type> { typeof(double), typeof(float), typeof(decimal), typeof(ulong) } },
                { UInt64.MinValue, new List<Type> { typeof(double), typeof(float), typeof(decimal), typeof(long), typeof(int), typeof(short), typeof(sbyte), typeof(ulong), typeof(uint), typeof(ushort), typeof(byte) } },
                { UInt32.MaxValue, new List<Type> { typeof(double), typeof(float), typeof(decimal), typeof(long), typeof(ulong), typeof(uint) } },
                { UInt32.MinValue, new List<Type> { typeof(double), typeof(float), typeof(decimal), typeof(long), typeof(int), typeof(short), typeof(sbyte), typeof(ulong), typeof(uint), typeof(ushort), typeof(byte) } },
                { UInt16.MaxValue, new List<Type> { typeof(double), typeof(float), typeof(decimal), typeof(long), typeof(int), typeof(ulong), typeof(uint), typeof(ushort) } },
                { UInt16.MinValue, new List<Type> { typeof(double), typeof(float), typeof(decimal), typeof(long), typeof(int), typeof(short), typeof(sbyte), typeof(ulong), typeof(uint), typeof(ushort), typeof(byte) } },
                { Byte.MaxValue, new List<Type> { typeof(double), typeof(float), typeof(decimal), typeof(long), typeof(int), typeof(short), typeof(ulong), typeof(uint), typeof(ushort), typeof(byte) } },
                { Byte.MinValue, new List<Type> { typeof(double), typeof(float), typeof(decimal), typeof(long), typeof(int), typeof(short), typeof(sbyte), typeof(ulong), typeof(uint), typeof(ushort), typeof(byte) } },
                { (double)1, new List<Type> { typeof(double), typeof(float), typeof(decimal), typeof(long), typeof(int), typeof(short), typeof(sbyte), typeof(ulong), typeof(uint), typeof(ushort), typeof(byte) } },
                { (float)1, new List<Type> { typeof(double), typeof(float), typeof(decimal), typeof(long), typeof(int), typeof(short), typeof(sbyte), typeof(ulong), typeof(uint), typeof(ushort), typeof(byte) } },
                { (decimal)1, new List<Type> { typeof(double), typeof(float), typeof(decimal), typeof(long), typeof(int), typeof(short), typeof(sbyte), typeof(ulong), typeof(uint), typeof(ushort), typeof(byte) } },
                { (long)1, new List<Type> { typeof(double), typeof(float), typeof(decimal), typeof(long), typeof(int), typeof(short), typeof(sbyte), typeof(ulong), typeof(uint), typeof(ushort), typeof(byte) } },
                { (int)1, new List<Type> { typeof(double), typeof(float), typeof(decimal), typeof(long), typeof(int), typeof(short), typeof(sbyte), typeof(ulong), typeof(uint), typeof(ushort), typeof(byte) } },
                { (short)1, new List<Type> { typeof(double), typeof(float), typeof(decimal), typeof(long), typeof(int), typeof(short), typeof(sbyte), typeof(ulong), typeof(uint), typeof(ushort), typeof(byte) } },
                { (sbyte)1, new List<Type> { typeof(double), typeof(float), typeof(decimal), typeof(long), typeof(int), typeof(short), typeof(sbyte), typeof(ulong), typeof(uint), typeof(ushort), typeof(byte) } },
                { (ulong)1, new List<Type> { typeof(double), typeof(float), typeof(decimal), typeof(long), typeof(int), typeof(short), typeof(sbyte), typeof(ulong), typeof(uint), typeof(ushort), typeof(byte) } },
                { (uint)1, new List<Type> { typeof(double), typeof(float), typeof(decimal), typeof(long), typeof(int), typeof(short), typeof(sbyte), typeof(ulong), typeof(uint), typeof(ushort), typeof(byte) } },
                { (ushort)1, new List<Type> { typeof(double), typeof(float), typeof(decimal), typeof(long), typeof(int), typeof(short), typeof(sbyte), typeof(ulong), typeof(uint), typeof(ushort), typeof(byte) } },
                { (byte)1, new List<Type> { typeof(double), typeof(float), typeof(decimal), typeof(long), typeof(int), typeof(short), typeof(sbyte), typeof(ulong), typeof(uint), typeof(ushort), typeof(byte) } },
            };

            foreach (KeyValuePair<object, List<Type>> mapping in valuesToNonOverflowingTypesMapping)
            {
                ConvertValueToNumber<double>(mapping);
                ConvertValueToNumber<float>(mapping);
                ConvertValueToNumber<decimal>(mapping);
                ConvertValueToNumber<long>(mapping);
                ConvertValueToNumber<int>(mapping);
                ConvertValueToNumber<short>(mapping);
                ConvertValueToNumber<sbyte>(mapping);
                ConvertValueToNumber<ulong>(mapping);
                ConvertValueToNumber<uint>(mapping);
                ConvertValueToNumber<ushort>(mapping);
                ConvertValueToNumber<byte>(mapping);
            }
        }

        static void ConvertValueToNumber<T>(KeyValuePair<object, List<Type>> mapping)
        {
            ConvertValueToNumber<T, OverflowException>(mapping);
        }

        static void ConvertValueToNumber<T, TException>(KeyValuePair<object, List<Type>> mapping)
            where TException : Exception
        {
            JsonValue jsonValue = CastToJsonValue(mapping.Key);

            Log.Info("Converting value {0} of type {1} to type {2}.", mapping.Key, mapping.Key.GetType().Name, typeof(T).Name);

            if (mapping.Value.Contains(typeof(T)))
            {
                Console.Write("Conversion should work... ");
                T valueOfT;
                Assert.True(jsonValue.TryReadAs<T>(out valueOfT));
                if (mapping.Key.GetType() != typeof(string))
                {
                    Console.Write("and original value casted to {0} should be the same as the retrieved value... ", typeof(T).Name);
                    T castValue = (T)Convert.ChangeType(mapping.Key, typeof(T), CultureInfo.InvariantCulture);
                    Assert.Equal<T>(castValue, valueOfT);
                }
            }
            else
            {
                Console.Write("Conversion should fail... ");
                T valueOfT;
                Assert.False(jsonValue.TryReadAs<T>(out valueOfT), String.Format("It was possible to read the value as {0}", valueOfT));
                ExpectException<TException>(delegate
                {
                    jsonValue.ReadAs<T>();
                });
            }

            Log.Info("Success!");
        }

        static JsonValue CastToJsonValue(object o)
        {
            switch (Type.GetTypeCode(o.GetType()))
            {
                case TypeCode.Boolean:
                    return (JsonValue)(bool)o;
                case TypeCode.Byte:
                    return (JsonValue)(byte)o;
                case TypeCode.Char:
                    return (JsonValue)(char)o;
                case TypeCode.DateTime:
                    return (JsonValue)(DateTime)o;
                case TypeCode.Decimal:
                    return (JsonValue)(decimal)o;
                case TypeCode.Double:
                    return (JsonValue)(double)o;
                case TypeCode.Int16:
                    return (JsonValue)(short)o;
                case TypeCode.Int32:
                    return (JsonValue)(int)o;
                case TypeCode.Int64:
                    return (JsonValue)(long)o;
                case TypeCode.SByte:
                    return (JsonValue)(sbyte)o;
                case TypeCode.Single:
                    return (JsonValue)(float)o;
                case TypeCode.String:
                    return (JsonValue)(string)o;
                case TypeCode.UInt16:
                    return (JsonValue)(ushort)o;
                case TypeCode.UInt32:
                    return (JsonValue)(uint)o;
                case TypeCode.UInt64:
                    return (JsonValue)(ulong)o;
                default:
                    if (o.GetType() == typeof(DateTimeOffset))
                    {
                        return (JsonValue)(DateTimeOffset)o;
                    }

                    if (o.GetType() == typeof(Guid))
                    {
                        return (JsonValue)(Guid)o;
                    }

                    if (o.GetType() == typeof(Uri))
                    {
                        return (JsonValue)(Uri)o;
                    }

                    break;
            }

            return (JsonObject)o;
        }

        static void ExpectException<T>(Action action) where T : Exception
        {
            JsonValueTests.ExpectException<T>(action);
        }

        static string GetExpectedRepresentation(object obj)
        {
            if (obj is double)
            {
                double dbl = (double)obj;
                if (Double.IsPositiveInfinity(dbl))
                {
                    return "Infinity";
                }
                else if (Double.IsNegativeInfinity(dbl))
                {
                    return "-Infinity";
                }
            }
            else if (obj is float)
            {
                float flt = (float)obj;
                if (Single.IsPositiveInfinity(flt))
                {
                    return "Infinity";
                }
                else if (Single.IsNegativeInfinity(flt))
                {
                    return "-Infinity";
                }
            }
            else if (obj is DateTime)
            {
                DateTime dt = (DateTime)obj;
                return "\"" + dt.ToString(DateTimeFormat, CultureInfo.InvariantCulture) + "\"";
            }

            using (MemoryStream ms = new MemoryStream())
            {
                DataContractJsonSerializer dcjs = new DataContractJsonSerializer(obj.GetType());
                dcjs.WriteObject(ms, obj);
                return Encoding.UTF8.GetString(ms.GetBuffer(), 0, (int)ms.Position);
            }
        }

        void ValidateJson(JsonPrimitive jsonPrim, string expectedJson, JsonType expectedJsonType)
        {
            Assert.Equal(expectedJson, jsonPrim.ToString());
            Assert.Equal(expectedJsonType, jsonPrim.JsonType);
        }

        void TestReadAsRoundtrip<T>(JsonPrimitive jsonPrim, T myOriginalObjectOfT)
        {
            T myReadObjectOfT = jsonPrim.ReadAs<T>();
            T myTryReadObjectOfT;
            Assert.True(jsonPrim.TryReadAs<T>(out myTryReadObjectOfT));
            Assert.Equal(myOriginalObjectOfT, myReadObjectOfT);
            Assert.Equal(myOriginalObjectOfT, myTryReadObjectOfT);

            string stringValue;
            Assert.True(jsonPrim.TryReadAs<string>(out stringValue));
            if (typeof(T) == typeof(bool))
            {
                // bool returns a lowercase version. make sure we get something usable by doing another roundtrip of the value in .NET
                Assert.Equal(String.Format(CultureInfo.InvariantCulture, "{0}", myOriginalObjectOfT), bool.Parse(stringValue).ToString(CultureInfo.InvariantCulture));
            }
            else if (typeof(T) == typeof(float) || typeof(T) == typeof(double))
            {
                Assert.Equal(String.Format(CultureInfo.InvariantCulture, "{0:R}", myOriginalObjectOfT), stringValue);
            }
            else if (typeof(T) == typeof(DateTime))
            {
                Assert.Equal(String.Format(CultureInfo.InvariantCulture, "{0:" + DateTimeFormat + "}", myOriginalObjectOfT), stringValue);
            }
            else
            {
                Assert.Equal(String.Format(CultureInfo.InvariantCulture, "{0}", myOriginalObjectOfT), stringValue);
            }
        }

        void TestReadAsFromStringRoundtrip<T>(T value)
        {
            TestReadAsFromStringRoundtrip<T>(value, String.Format(CultureInfo.InvariantCulture, "{0}", value));
        }

        void TestReadAsFromStringRoundtrip<T>(T value, string valueString)
        {
            T tempOfT;
            JsonPrimitive jsonPrim = new JsonPrimitive(valueString);
            Assert.True(jsonPrim.TryReadAs<T>(out tempOfT));
            Assert.Equal<T>(value, tempOfT);
        }
    }
}
