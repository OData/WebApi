// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using Xunit;

namespace System.Json
{
    public class JsonPrimitiveTest
    {
        const string DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.fffK";

        [Fact]
        public void JsonPrimitiveConstructorTest()
        {
            Assert.Equal(AnyInstance.AnyString, (string)(new JsonPrimitive(AnyInstance.AnyString)));
            Assert.Equal(AnyInstance.AnyChar, (char)(new JsonPrimitive(AnyInstance.AnyChar)));
            Assert.Equal(AnyInstance.AnyUri, (Uri)(new JsonPrimitive(AnyInstance.AnyUri)));
            Assert.Equal(AnyInstance.AnyGuid, (Guid)(new JsonPrimitive(AnyInstance.AnyGuid)));
            Assert.Equal(AnyInstance.AnyDateTime, (DateTime)(new JsonPrimitive(AnyInstance.AnyDateTime)));
            Assert.Equal(AnyInstance.AnyDateTimeOffset, (DateTimeOffset)(new JsonPrimitive(AnyInstance.AnyDateTimeOffset)));
            Assert.Equal(AnyInstance.AnyBool, (bool)(new JsonPrimitive(AnyInstance.AnyBool)));
            Assert.Equal(AnyInstance.AnyByte, (byte)(new JsonPrimitive(AnyInstance.AnyByte)));
            Assert.Equal(AnyInstance.AnyShort, (short)(new JsonPrimitive(AnyInstance.AnyShort)));
            Assert.Equal(AnyInstance.AnyInt, (int)(new JsonPrimitive(AnyInstance.AnyInt)));
            Assert.Equal(AnyInstance.AnyLong, (long)(new JsonPrimitive(AnyInstance.AnyLong)));
            Assert.Equal(AnyInstance.AnySByte, (sbyte)(new JsonPrimitive(AnyInstance.AnySByte)));
            Assert.Equal(AnyInstance.AnyUShort, (ushort)(new JsonPrimitive(AnyInstance.AnyUShort)));
            Assert.Equal(AnyInstance.AnyUInt, (uint)(new JsonPrimitive(AnyInstance.AnyUInt)));
            Assert.Equal(AnyInstance.AnyULong, (ulong)(new JsonPrimitive(AnyInstance.AnyULong)));
            Assert.Equal(AnyInstance.AnyDecimal, (decimal)(new JsonPrimitive(AnyInstance.AnyDecimal)));
            Assert.Equal(AnyInstance.AnyFloat, (float)(new JsonPrimitive(AnyInstance.AnyFloat)));
            Assert.Equal(AnyInstance.AnyDouble, (double)(new JsonPrimitive(AnyInstance.AnyDouble)));
        }

        [Fact]
        public void ValueTest()
        {
            object[] values = 
            {
                AnyInstance.AnyInt, AnyInstance.AnyString, AnyInstance.AnyGuid, AnyInstance.AnyDecimal, AnyInstance.AnyBool, AnyInstance.AnyDateTime
            };

            foreach (object value in values)
            {
                JsonPrimitive jp;
                bool success = JsonPrimitive.TryCreate(value, out jp);
                Assert.True(success);
                Assert.Equal(value, jp.Value);
            }
        }

        [Fact]
        public void TryCreateTest()
        {
            object[] numberValues =
            {
                AnyInstance.AnyByte, AnyInstance.AnySByte, AnyInstance.AnyShort, AnyInstance.AnyDecimal, 
                AnyInstance.AnyDouble, AnyInstance.AnyShort, AnyInstance.AnyInt, AnyInstance.AnyLong, 
                AnyInstance.AnyUShort, AnyInstance.AnyUInt, AnyInstance.AnyULong, AnyInstance.AnyFloat
            };

            object[] booleanValues =
            {
                true, false
            };


            object[] stringValues =
            {
                AnyInstance.AnyString, AnyInstance.AnyChar, 
                AnyInstance.AnyDateTime, AnyInstance.AnyDateTimeOffset,
                AnyInstance.AnyGuid, AnyInstance.AnyUri
            };

            CheckValues(numberValues, JsonType.Number);
            CheckValues(booleanValues, JsonType.Boolean);
            CheckValues(stringValues, JsonType.String);
        }

        [Fact]
        public void TryCreateInvalidTest()
        {
            bool success;
            JsonPrimitive target;

            object[] values =
            {
                AnyInstance.AnyJsonArray, AnyInstance.AnyJsonObject, AnyInstance.AnyJsonPrimitive, 
                null, AnyInstance.DefaultJsonValue, AnyInstance.AnyDynamic, AnyInstance.AnyAddress,
                AnyInstance.AnyPerson
            };

            foreach (object value in values)
            {
                success = JsonPrimitive.TryCreate(value, out target);
                Assert.False(success);
                Assert.Null(target);
            }
        }

        [Fact]
        public void NumberToNumberConversionTest()
        {
            long longValue;
            Assert.Equal((long)AnyInstance.AnyInt, (long)(new JsonPrimitive(AnyInstance.AnyInt)));
            Assert.Equal((long)AnyInstance.AnyUInt, (long)(new JsonPrimitive(AnyInstance.AnyUInt)));
            Assert.True(new JsonPrimitive(AnyInstance.AnyInt).TryReadAs<long>(out longValue));
            Assert.Equal((long)AnyInstance.AnyInt, longValue);

            int intValue;
            Assert.Equal((int)AnyInstance.AnyShort, (int)(new JsonPrimitive(AnyInstance.AnyShort)));
            Assert.Equal((int)AnyInstance.AnyUShort, (int)(new JsonPrimitive(AnyInstance.AnyUShort)));
            Assert.True(new JsonPrimitive(AnyInstance.AnyUShort).TryReadAs<int>(out intValue));
            Assert.Equal((int)AnyInstance.AnyUShort, intValue);

            short shortValue;
            Assert.Equal((short)AnyInstance.AnyByte, (short)(new JsonPrimitive(AnyInstance.AnyByte)));
            Assert.Equal((short)AnyInstance.AnySByte, (short)(new JsonPrimitive(AnyInstance.AnySByte)));
            Assert.True(new JsonPrimitive(AnyInstance.AnyByte).TryReadAs<short>(out shortValue));
            Assert.Equal((short)AnyInstance.AnyByte, shortValue);

            double dblValue;
            Assert.Equal((double)AnyInstance.AnyFloat, (double)(new JsonPrimitive(AnyInstance.AnyFloat)));
            Assert.Equal((double)AnyInstance.AnyDecimal, (double)(new JsonPrimitive(AnyInstance.AnyDecimal)));
            Assert.True(new JsonPrimitive(AnyInstance.AnyFloat).TryReadAs<double>(out dblValue));
            Assert.Equal((double)AnyInstance.AnyFloat, dblValue);
            ExceptionHelper.Throws<OverflowException>(delegate { int i = (int)(new JsonPrimitive(1L << 32)); });
            Assert.False(new JsonPrimitive(1L << 32).TryReadAs<int>(out intValue));
            Assert.Equal(default(int), intValue);

            byte byteValue;
            ExceptionHelper.Throws<OverflowException>(delegate { byte b = (byte)(new JsonPrimitive(1L << 32)); });
            ExceptionHelper.Throws<OverflowException>(delegate { byte b = (byte)(new JsonPrimitive(SByte.MinValue)); });
            Assert.False(new JsonPrimitive(SByte.MinValue).TryReadAs<byte>(out byteValue));
            Assert.Equal(default(byte), byteValue);
        }

        [Fact]
        public void NumberToStringConverstionTest()
        {
            Dictionary<string, JsonPrimitive> allNumbers = new Dictionary<string, JsonPrimitive>
            {
                { AnyInstance.AnyByte.ToString(CultureInfo.InvariantCulture), new JsonPrimitive(AnyInstance.AnyByte) },
                { AnyInstance.AnySByte.ToString(CultureInfo.InvariantCulture), new JsonPrimitive(AnyInstance.AnySByte) },
                { AnyInstance.AnyShort.ToString(CultureInfo.InvariantCulture), new JsonPrimitive(AnyInstance.AnyShort) },
                { AnyInstance.AnyUShort.ToString(CultureInfo.InvariantCulture), new JsonPrimitive(AnyInstance.AnyUShort) },
                { AnyInstance.AnyInt.ToString(CultureInfo.InvariantCulture), new JsonPrimitive(AnyInstance.AnyInt) },
                { AnyInstance.AnyUInt.ToString(CultureInfo.InvariantCulture), new JsonPrimitive(AnyInstance.AnyUInt) },
                { AnyInstance.AnyLong.ToString(CultureInfo.InvariantCulture), new JsonPrimitive(AnyInstance.AnyLong) },
                { AnyInstance.AnyULong.ToString(CultureInfo.InvariantCulture), new JsonPrimitive(AnyInstance.AnyULong) },
                { AnyInstance.AnyDecimal.ToString(CultureInfo.InvariantCulture), new JsonPrimitive(AnyInstance.AnyDecimal) },
                { AnyInstance.AnyDouble.ToString("R", CultureInfo.InvariantCulture), new JsonPrimitive(AnyInstance.AnyDouble) },
                { AnyInstance.AnyFloat.ToString("R", CultureInfo.InvariantCulture), new JsonPrimitive(AnyInstance.AnyFloat) },
            };

            foreach (string stringRepresentation in allNumbers.Keys)
            {
                JsonPrimitive jp = allNumbers[stringRepresentation];
                Assert.Equal(stringRepresentation, (string)jp);
                Assert.Equal(stringRepresentation, jp.ReadAs<string>());
            }
        }

        [Fact]
        public void NonNumberToStringConversionTest()
        {
            Dictionary<string, JsonPrimitive> allValues = new Dictionary<string, JsonPrimitive>
            {
                { new string(AnyInstance.AnyChar, 1), new JsonPrimitive(AnyInstance.AnyChar) },
                { AnyInstance.AnyBool.ToString(CultureInfo.InvariantCulture).ToLowerInvariant(), new JsonPrimitive(AnyInstance.AnyBool) },
                { AnyInstance.AnyGuid.ToString("D", CultureInfo.InvariantCulture), new JsonPrimitive(AnyInstance.AnyGuid) },
                { AnyInstance.AnyDateTime.ToString(DateTimeFormat, CultureInfo.InvariantCulture), new JsonPrimitive(AnyInstance.AnyDateTime) },
                { AnyInstance.AnyDateTimeOffset.ToString(DateTimeFormat, CultureInfo.InvariantCulture), new JsonPrimitive(AnyInstance.AnyDateTimeOffset) },
            };

            foreach (char escapedChar in "\r\n\t\u0000\uffff\u001f\\\"")
            {
                allValues.Add(new string(escapedChar, 1), new JsonPrimitive(escapedChar));
            }

            foreach (string stringRepresentation in allValues.Keys)
            {
                JsonPrimitive jp = allValues[stringRepresentation];
                Assert.Equal(stringRepresentation, (string)jp);
                Assert.Equal(stringRepresentation, jp.ReadAs<string>());
            }
        }

        [Fact]
        public void NonNumberToNumberConversionTest()
        {
            Assert.Equal(1, new JsonPrimitive('1').ReadAs<int>());
            Assert.Equal<byte>(AnyInstance.AnyByte, new JsonPrimitive(AnyInstance.AnyByte.ToString(CultureInfo.InvariantCulture)).ReadAs<byte>());
            Assert.Equal<sbyte>(AnyInstance.AnySByte, (sbyte)(new JsonPrimitive(AnyInstance.AnySByte.ToString(CultureInfo.InvariantCulture))));
            Assert.Equal<short>(AnyInstance.AnyShort, (short)(new JsonPrimitive(AnyInstance.AnyShort.ToString(CultureInfo.InvariantCulture))));
            Assert.Equal<ushort>(AnyInstance.AnyUShort, new JsonPrimitive(AnyInstance.AnyUShort.ToString(CultureInfo.InvariantCulture)).ReadAs<ushort>());
            Assert.Equal<int>(AnyInstance.AnyInt, new JsonPrimitive(AnyInstance.AnyInt.ToString(CultureInfo.InvariantCulture)).ReadAs<int>());
            Assert.Equal<uint>(AnyInstance.AnyUInt, (uint)(new JsonPrimitive(AnyInstance.AnyUInt.ToString(CultureInfo.InvariantCulture))));
            Assert.Equal<long>(AnyInstance.AnyLong, (long)(new JsonPrimitive(AnyInstance.AnyLong.ToString(CultureInfo.InvariantCulture))));
            Assert.Equal<ulong>(AnyInstance.AnyULong, new JsonPrimitive(AnyInstance.AnyULong.ToString(CultureInfo.InvariantCulture)).ReadAs<ulong>());

            Assert.Equal<decimal>(AnyInstance.AnyDecimal, (decimal)(new JsonPrimitive(AnyInstance.AnyDecimal.ToString(CultureInfo.InvariantCulture))));
            Assert.Equal<float>(AnyInstance.AnyFloat, new JsonPrimitive(AnyInstance.AnyFloat.ToString(CultureInfo.InvariantCulture)).ReadAs<float>());
            Assert.Equal<double>(AnyInstance.AnyDouble, (double)(new JsonPrimitive(AnyInstance.AnyDouble.ToString(CultureInfo.InvariantCulture))));

            Assert.Equal<byte>(Convert.ToByte(1.23, CultureInfo.InvariantCulture), new JsonPrimitive("1.23").ReadAs<byte>());
            Assert.Equal<int>(Convert.ToInt32(12345.6789, CultureInfo.InvariantCulture), new JsonPrimitive("12345.6789").ReadAs<int>());
            Assert.Equal<short>(Convert.ToInt16(1.23e2), (short)new JsonPrimitive("1.23e2"));
            Assert.Equal<float>(Convert.ToSingle(1.23e40), (float)new JsonPrimitive("1.23e40"));
            Assert.Equal<float>(Convert.ToSingle(1.23e-38), (float)new JsonPrimitive("1.23e-38"));

            ExceptionHelper.Throws<InvalidCastException>(delegate { var n = new JsonPrimitive(AnyInstance.AnyBool).ReadAs<sbyte>(); });
            ExceptionHelper.Throws<InvalidCastException>(delegate { var n = new JsonPrimitive(AnyInstance.AnyBool).ReadAs<short>(); });
            ExceptionHelper.Throws<InvalidCastException>(delegate { var n = new JsonPrimitive(AnyInstance.AnyBool).ReadAs<uint>(); });
            ExceptionHelper.Throws<InvalidCastException>(delegate { var n = new JsonPrimitive(AnyInstance.AnyBool).ReadAs<long>(); });
            ExceptionHelper.Throws<InvalidCastException>(delegate { var n = new JsonPrimitive(AnyInstance.AnyBool).ReadAs<double>(); });

            ExceptionHelper.Throws<FormatException>(delegate { var n = new JsonPrimitive(AnyInstance.AnyUri).ReadAs<int>(); });
            ExceptionHelper.Throws<FormatException>(delegate { var n = new JsonPrimitive(AnyInstance.AnyDateTime).ReadAs<float>(); });
            ExceptionHelper.Throws<InvalidCastException>(delegate { var n = (decimal)(new JsonPrimitive('c')); });
            ExceptionHelper.Throws<InvalidCastException>(delegate { var n = (byte)(new JsonPrimitive("0xFF")); });
            ExceptionHelper.Throws<InvalidCastException>(delegate { var n = (sbyte)(new JsonPrimitive(AnyInstance.AnyDateTimeOffset)); });
            ExceptionHelper.Throws<FormatException>(delegate { var n = new JsonPrimitive(AnyInstance.AnyUri).ReadAs<uint>(); });
            ExceptionHelper.Throws<FormatException>(delegate { var n = new JsonPrimitive(AnyInstance.AnyDateTime).ReadAs<double>(); });
            ExceptionHelper.Throws<InvalidCastException>(delegate { var n = (long)(new JsonPrimitive('c')); });
            ExceptionHelper.Throws<InvalidCastException>(delegate { var n = (ulong)(new JsonPrimitive("0xFF")); });
            ExceptionHelper.Throws<InvalidCastException>(delegate { var n = (short)(new JsonPrimitive(AnyInstance.AnyDateTimeOffset)); });
            ExceptionHelper.Throws<InvalidCastException>(delegate { var n = (ushort)(new JsonPrimitive('c')); });

            ExceptionHelper.Throws<OverflowException>(delegate { int i = (int)new JsonPrimitive((1L << 32).ToString(CultureInfo.InvariantCulture)); });
            ExceptionHelper.Throws<OverflowException>(delegate { byte b = (byte)new JsonPrimitive("-1"); });
        }

        [Fact]
        public void StringToNonNumberConversionTest()
        {
            const string DateTimeWithOffsetFormat = "yyyy-MM-ddTHH:mm:sszzz";
            const string DateTimeWithOffsetFormat2 = "yyy-MM-ddTHH:mm:ss.fffK";
            const string DateTimeWithoutOffsetWithoutTimeFormat = "yyy-MM-dd";
            const string DateTimeWithoutOffsetFormat = "yyy-MM-ddTHH:mm:ss";
            const string DateTimeWithoutOffsetFormat2 = "yyy-MM-ddTHH:mm:ss.fff";
            const string TimeWithoutOffsetFormat = "HH:mm:ss";
            const string TimeWithoutOffsetFormat2 = "HH:mm";

            Assert.Equal(false, new JsonPrimitive("false").ReadAs<bool>());
            Assert.Equal(false, (bool)(new JsonPrimitive("False")));
            Assert.Equal(true, (bool)(new JsonPrimitive("true")));
            Assert.Equal(true, new JsonPrimitive("True").ReadAs<bool>());

            Assert.Equal<Uri>(AnyInstance.AnyUri, new JsonPrimitive(AnyInstance.AnyUri.ToString()).ReadAs<Uri>());
            Assert.Equal<char>(AnyInstance.AnyChar, (char)(new JsonPrimitive(new string(AnyInstance.AnyChar, 1))));
            Assert.Equal<Guid>(AnyInstance.AnyGuid, (Guid)(new JsonPrimitive(AnyInstance.AnyGuid.ToString("D", CultureInfo.InvariantCulture))));

            DateTime anyLocalDateTime = AnyInstance.AnyDateTime.ToLocalTime();
            DateTime anyUtcDateTime = AnyInstance.AnyDateTime.ToUniversalTime();

            Assert.Equal<DateTime>(anyUtcDateTime, (DateTime)(new JsonPrimitive(anyUtcDateTime.ToString(DateTimeFormat, CultureInfo.InvariantCulture))));
            Assert.Equal<DateTime>(anyLocalDateTime, new JsonPrimitive(anyLocalDateTime.ToString(DateTimeWithOffsetFormat2, CultureInfo.InvariantCulture)).ReadAs<DateTime>());
            Assert.Equal<DateTime>(anyUtcDateTime, new JsonPrimitive(anyUtcDateTime.ToString(DateTimeWithOffsetFormat2, CultureInfo.InvariantCulture)).ReadAs<DateTime>());
            Assert.Equal<DateTime>(anyLocalDateTime.Date, (DateTime)(new JsonPrimitive(anyLocalDateTime.ToString(DateTimeWithoutOffsetWithoutTimeFormat, CultureInfo.InvariantCulture))));
            Assert.Equal<DateTime>(anyLocalDateTime, new JsonPrimitive(anyLocalDateTime.ToString(DateTimeWithoutOffsetFormat, CultureInfo.InvariantCulture)).ReadAs<DateTime>());
            Assert.Equal<DateTime>(anyLocalDateTime, new JsonPrimitive(anyLocalDateTime.ToString(DateTimeWithoutOffsetFormat2, CultureInfo.InvariantCulture)).ReadAs<DateTime>());

            DateTime dt = new JsonPrimitive(anyLocalDateTime.ToString(TimeWithoutOffsetFormat, CultureInfo.InvariantCulture)).ReadAs<DateTime>();
            Assert.Equal(anyLocalDateTime.Hour, dt.Hour);
            Assert.Equal(anyLocalDateTime.Minute, dt.Minute);
            Assert.Equal(anyLocalDateTime.Second, dt.Second);

            dt = new JsonPrimitive(anyLocalDateTime.ToString(TimeWithoutOffsetFormat2, CultureInfo.InvariantCulture)).ReadAs<DateTime>();
            Assert.Equal(anyLocalDateTime.Hour, dt.Hour);
            Assert.Equal(anyLocalDateTime.Minute, dt.Minute);
            Assert.Equal(0, dt.Second);

            Assert.Equal<DateTimeOffset>(AnyInstance.AnyDateTimeOffset, new JsonPrimitive(AnyInstance.AnyDateTimeOffset.ToString(DateTimeFormat, CultureInfo.InvariantCulture)).ReadAs<DateTimeOffset>());
            Assert.Equal<DateTimeOffset>(AnyInstance.AnyDateTimeOffset, new JsonPrimitive(AnyInstance.AnyDateTimeOffset.ToString(DateTimeWithOffsetFormat, CultureInfo.InvariantCulture)).ReadAs<DateTimeOffset>());
            Assert.Equal<DateTimeOffset>(AnyInstance.AnyDateTimeOffset, new JsonPrimitive(AnyInstance.AnyDateTimeOffset.ToString(DateTimeWithOffsetFormat2, CultureInfo.InvariantCulture)).ReadAs<DateTimeOffset>());
            Assert.Equal<DateTimeOffset>(AnyInstance.AnyDateTimeOffset.ToLocalTime(), (DateTimeOffset)(new JsonPrimitive(AnyInstance.AnyDateTimeOffset.ToLocalTime().ToString(DateTimeWithoutOffsetFormat, CultureInfo.InvariantCulture))));
            Assert.Equal<DateTimeOffset>(AnyInstance.AnyDateTimeOffset.ToLocalTime(), (DateTimeOffset)(new JsonPrimitive(AnyInstance.AnyDateTimeOffset.ToLocalTime().ToString(DateTimeWithoutOffsetFormat2, CultureInfo.InvariantCulture))));

            DataContractJsonSerializer dcjs = new DataContractJsonSerializer(typeof(DateTime));
            MemoryStream ms = new MemoryStream();
            dcjs.WriteObject(ms, AnyInstance.AnyDateTime);
            string dcjsSerializedDateTime = Encoding.UTF8.GetString(ms.ToArray());
            Assert.Equal(AnyInstance.AnyDateTime, JsonValue.Parse(dcjsSerializedDateTime).ReadAs<DateTime>());

            ExceptionHelper.Throws<InvalidCastException>(delegate { var b = (bool)(new JsonPrimitive("notBool")); });
            ExceptionHelper.Throws<UriFormatException>(delegate { var u = new JsonPrimitive("not an uri - " + new string('r', 100000)).ReadAs<Uri>(); });
            ExceptionHelper.Throws<FormatException>(delegate { var date = new JsonPrimitive("not a date time").ReadAs<DateTime>(); });
            ExceptionHelper.Throws<InvalidCastException>(delegate { var dto = (DateTimeOffset)(new JsonPrimitive("not a date time offset")); });
            ExceptionHelper.Throws<InvalidCastException>(delegate { var c = (char)new JsonPrimitive(""); });
            ExceptionHelper.Throws<InvalidCastException>(delegate { var c = (char)new JsonPrimitive("cc"); });
            ExceptionHelper.Throws<FormatException>(delegate { var g = new JsonPrimitive("not a guid").ReadAs<Guid>(); });
        }

        [Fact]
        public void AspNetDateTimeFormatConversionTest()
        {
            DateTime unixEpochUtc = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime unixEpochLocal = unixEpochUtc.ToLocalTime();
            Assert.Equal(unixEpochUtc, new JsonPrimitive("/Date(0)/").ReadAs<DateTime>());
            Assert.Equal(unixEpochLocal, new JsonPrimitive("/Date(0-0900)/").ReadAs<DateTime>());
            Assert.Equal(unixEpochLocal, new JsonPrimitive("/Date(0+1000)/").ReadAs<DateTime>());
        }

        [Fact]
        public void ToStringTest()
        {
            char anyUnescapedChar = 'c';
            string anyUnescapedString = "hello";

            Dictionary<string, JsonPrimitive> toStringResults = new Dictionary<string, JsonPrimitive>
            {
                // Boolean types
                { AnyInstance.AnyBool.ToString(CultureInfo.InvariantCulture).ToLowerInvariant(), new JsonPrimitive(AnyInstance.AnyBool) },

                // Numeric types
                { AnyInstance.AnyByte.ToString(CultureInfo.InvariantCulture), new JsonPrimitive(AnyInstance.AnyByte) },
                { AnyInstance.AnySByte.ToString(CultureInfo.InvariantCulture), new JsonPrimitive(AnyInstance.AnySByte) },
                { AnyInstance.AnyShort.ToString(CultureInfo.InvariantCulture), new JsonPrimitive(AnyInstance.AnyShort) },
                { AnyInstance.AnyUShort.ToString(CultureInfo.InvariantCulture), new JsonPrimitive(AnyInstance.AnyUShort) },
                { AnyInstance.AnyInt.ToString(CultureInfo.InvariantCulture), new JsonPrimitive(AnyInstance.AnyInt) },
                { AnyInstance.AnyUInt.ToString(CultureInfo.InvariantCulture), new JsonPrimitive(AnyInstance.AnyUInt) },
                { AnyInstance.AnyLong.ToString(CultureInfo.InvariantCulture), new JsonPrimitive(AnyInstance.AnyLong) },
                { AnyInstance.AnyULong.ToString(CultureInfo.InvariantCulture), new JsonPrimitive(AnyInstance.AnyULong) },
                { AnyInstance.AnyFloat.ToString("R", CultureInfo.InvariantCulture), new JsonPrimitive(AnyInstance.AnyFloat) },
                { AnyInstance.AnyDouble.ToString("R", CultureInfo.InvariantCulture), new JsonPrimitive(AnyInstance.AnyDouble) },
                { AnyInstance.AnyDecimal.ToString(CultureInfo.InvariantCulture), new JsonPrimitive(AnyInstance.AnyDecimal) },

                // String types
                { "\"" + new string(anyUnescapedChar, 1) + "\"", new JsonPrimitive(anyUnescapedChar) },
                { "\"" + anyUnescapedString + "\"", new JsonPrimitive(anyUnescapedString) },
                { "\"" + AnyInstance.AnyDateTime.ToString(DateTimeFormat, CultureInfo.InvariantCulture) + "\"", new JsonPrimitive(AnyInstance.AnyDateTime) },
                { "\"" + AnyInstance.AnyDateTimeOffset.ToString(DateTimeFormat, CultureInfo.InvariantCulture) + "\"", new JsonPrimitive(AnyInstance.AnyDateTimeOffset) },
                { "\"" + AnyInstance.AnyUri.GetComponents(UriComponents.SerializationInfoString, UriFormat.UriEscaped).Replace("/", "\\/") + "\"", new JsonPrimitive(AnyInstance.AnyUri) },
                { "\"" + AnyInstance.AnyGuid.ToString("D", CultureInfo.InvariantCulture) + "\"", new JsonPrimitive(AnyInstance.AnyGuid) },
            };

            foreach (string stringRepresentation in toStringResults.Keys)
            {
                string actualResult = toStringResults[stringRepresentation].ToString();
                Assert.Equal(stringRepresentation, actualResult);
            }

            Dictionary<string, JsonPrimitive> escapedValues = new Dictionary<string, JsonPrimitive>
            {
                { "\"\\u000d\"", new JsonPrimitive('\r') },
                { "\"\\u000a\"", new JsonPrimitive('\n') },
                { "\"\\\\\"", new JsonPrimitive('\\') },
                { "\"\\/\"", new JsonPrimitive('/') },
                { "\"\\u000b\"", new JsonPrimitive('\u000b') },
                { "\"\\\"\"", new JsonPrimitive('\"') },
                { "\"slash-r-\\u000d-fffe-\\ufffe-ffff-\\uffff-tab-\\u0009\"", new JsonPrimitive("slash-r-\r-fffe-\ufffe-ffff-\uffff-tab-\t") },
            };

            foreach (string stringRepresentation in escapedValues.Keys)
            {
                string actualResult = escapedValues[stringRepresentation].ToString();
                Assert.Equal(stringRepresentation, actualResult);
            }
        }

        [Fact]
        public void JsonTypeTest()
        {
            Assert.Equal(JsonType.Boolean, new JsonPrimitive(AnyInstance.AnyBool).JsonType);
            Assert.Equal(JsonType.Number, new JsonPrimitive(AnyInstance.AnyByte).JsonType);
            Assert.Equal(JsonType.Number, new JsonPrimitive(AnyInstance.AnySByte).JsonType);
            Assert.Equal(JsonType.Number, new JsonPrimitive(AnyInstance.AnyShort).JsonType);
            Assert.Equal(JsonType.Number, new JsonPrimitive(AnyInstance.AnyUShort).JsonType);
            Assert.Equal(JsonType.Number, new JsonPrimitive(AnyInstance.AnyInt).JsonType);
            Assert.Equal(JsonType.Number, new JsonPrimitive(AnyInstance.AnyUInt).JsonType);
            Assert.Equal(JsonType.Number, new JsonPrimitive(AnyInstance.AnyLong).JsonType);
            Assert.Equal(JsonType.Number, new JsonPrimitive(AnyInstance.AnyULong).JsonType);
            Assert.Equal(JsonType.Number, new JsonPrimitive(AnyInstance.AnyDecimal).JsonType);
            Assert.Equal(JsonType.Number, new JsonPrimitive(AnyInstance.AnyDouble).JsonType);
            Assert.Equal(JsonType.Number, new JsonPrimitive(AnyInstance.AnyFloat).JsonType);
            Assert.Equal(JsonType.String, new JsonPrimitive(AnyInstance.AnyChar).JsonType);
            Assert.Equal(JsonType.String, new JsonPrimitive(AnyInstance.AnyString).JsonType);
            Assert.Equal(JsonType.String, new JsonPrimitive(AnyInstance.AnyUri).JsonType);
            Assert.Equal(JsonType.String, new JsonPrimitive(AnyInstance.AnyGuid).JsonType);
            Assert.Equal(JsonType.String, new JsonPrimitive(AnyInstance.AnyDateTime).JsonType);
            Assert.Equal(JsonType.String, new JsonPrimitive(AnyInstance.AnyDateTimeOffset).JsonType);
        }

        [Fact]
        public void InvalidPropertyTest()
        {
            JsonValue target = AnyInstance.AnyJsonPrimitive;
            Assert.True(target.Count == 0);
            Assert.False(target.ContainsKey(String.Empty));
            Assert.False(target.ContainsKey(AnyInstance.AnyString));
        }

        private void CheckValues(object[] values, JsonType expectedType)
        {
            JsonPrimitive target;
            bool success;

            foreach (object value in values)
            {
                success = JsonPrimitive.TryCreate(value, out target);
                Assert.True(success);
                Assert.NotNull(target);
                Assert.Equal(expectedType, target.JsonType);
            }
        }
    }
}
