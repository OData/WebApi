﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using System.Data.Linq;
using System.Xml.Linq;
using Microsoft.TestCommon;

namespace System.Web.OData.Formatter
{
    public class EdmPrimitiveHelpersTest
    {
        public static TheoryDataSet<object, object, Type> ConvertPrimitiveValue_NonStandardPrimitives_Data
        {
            get
            {
                return new TheoryDataSet<object, object, Type>
                {
                     { "1", (char)'1', typeof(char) },
                     { "1", (char?)'1', typeof(char?) },
                     { "123", (char[]) new char[] {'1', '2', '3' }, typeof(char[]) },
                     { (int)1 , (ushort)1, typeof(ushort)},
                     { (int?)1, (ushort?)1,  typeof(ushort?) },
                     { (long)1, (uint)1,  typeof(uint) },
                     { (long?)1, (uint?)1, typeof(uint?) },
                     { (long)1 , (ulong)1, typeof(ulong)},
                     { (long?)1 ,(ulong?)1, typeof(ulong?)},
                    //(Stream) new MemoryStream(new byte[] { 1 }), // TODO: Enable once we have support for streams
                     { "<element xmlns=\"namespace\" />" ,(XElement) new XElement(XName.Get("element","namespace")), typeof(XElement)},
                     { new byte[] {1}, new Binary(new byte[] {1}), typeof(Binary)}
                };
            }
        }

        public static TheoryDataSet<DateTimeOffset> ConvertDateTime_NonStandardPrimitives_Data
        {
            get
            {
                return new TheoryDataSet<DateTimeOffset>
                {
                    DateTimeOffset.Parse("2014-12-12T01:02:03Z"),
                    DateTimeOffset.Parse("2014-12-12T01:02:03-8:00"),
                    DateTimeOffset.Parse("2014-12-12T01:02:03+8:00"),
                };
            }
        }

        [Theory]
        [PropertyData("ConvertPrimitiveValue_NonStandardPrimitives_Data")]
        public void ConvertPrimitiveValue_NonStandardPrimitives(object valueToConvert, object result, Type conversionType)
        {
            Assert.Equal(result.GetType(), EdmPrimitiveHelpers.ConvertPrimitiveValue(valueToConvert, conversionType).GetType());
            Assert.Equal(result.ToString(), EdmPrimitiveHelpers.ConvertPrimitiveValue(valueToConvert, conversionType).ToString());
        }

        [Theory]
        [PropertyData("ConvertDateTime_NonStandardPrimitives_Data")]
        public void ConvertDateTimeValue_NonStandardPrimitives_DefaultTimeZoneInfo(DateTimeOffset valueToConvert)
        {
            // Arrange & Act
            TimeZoneInfoHelper.TimeZone = null;
            object actual = EdmPrimitiveHelpers.ConvertPrimitiveValue(valueToConvert, typeof(DateTime));

            // Assert
            DateTime dt = Assert.IsType<DateTime>(actual);
            Assert.Equal(valueToConvert.LocalDateTime, dt);
        }

        [Theory]
        [PropertyData("ConvertDateTime_NonStandardPrimitives_Data")]
        public void ConvertDateTimeValue_NonStandardPrimitives_CustomTimeZoneInfo(DateTimeOffset valueToConvert)
        {
            // Arrange & Act
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
            TimeZoneInfoHelper.TimeZone = timeZone;
            object actual = EdmPrimitiveHelpers.ConvertPrimitiveValue(valueToConvert, typeof(DateTime));

            // Assert
            DateTime dt = Assert.IsType<DateTime>(actual);
            Assert.Equal(TimeZoneInfo.ConvertTime(valueToConvert, timeZone).DateTime, dt);
        }

        [Theory]
        [InlineData("123")]
        [InlineData("")]
        public void ConvertPrimitiveValueToChar_Throws(string input)
        {
            Assert.Throws<ValidationException>(
                () => EdmPrimitiveHelpers.ConvertPrimitiveValue(input, typeof(char)),
                "The value must be a string with a length of 1.");
        }

        [Fact]
        public void ConvertPrimitiveValueToNullableChar_Throws()
        {
            Assert.Throws<ValidationException>(
                () => EdmPrimitiveHelpers.ConvertPrimitiveValue("123", typeof(char?)),
                "The value must be a string with a maximum length of 1.");
        }

        [Fact]
        public void ConvertPrimitiveValueToXElement_Throws_IfInputIsNotString()
        {
            Assert.Throws<ValidationException>(
                () => EdmPrimitiveHelpers.ConvertPrimitiveValue(123, typeof(XElement)),
                "The value must be a string.");
        }
    }
}
