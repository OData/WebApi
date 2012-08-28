// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using System.Data.Linq;
using System.Xml.Linq;
using Microsoft.TestCommon;

namespace System.Web.Http.OData.Formatter.Deserialization
{
    public class ODataEntryDeserializerTests
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
                     { (long)1, (uint)1,  typeof(uint)},
                     { (long?)1, (uint?)1, typeof(uint?) },
                     { (long)1 , (ulong)1, typeof(ulong)},
                     { (long?)1 ,(ulong?)1, typeof(ulong?)},
                    //(Stream) new MemoryStream(new byte[] { 1 }), // TODO: Enable once we have support for streams
                     { "<element xmlns=\"namespace\" />" ,(XElement) new XElement(XName.Get("element","namespace")), typeof(XElement)},
                     { new byte[] {1} ,(Binary) new Binary(new byte[] {1}), typeof(Binary)}
                };
            }
        }

        [Theory]
        [PropertyData("ConvertPrimitiveValue_NonStandardPrimitives_Data")]
        public void ConvertPrimitiveValue_NonStandardPrimitives(object valueToConvert, object result, Type conversionType)
        {
            Assert.Equal(result.GetType(), ODataEntryDeserializer.ConvertPrimitiveValue(valueToConvert, conversionType, "", "").GetType());
            Assert.Equal(result.ToString(), ODataEntryDeserializer.ConvertPrimitiveValue(valueToConvert, conversionType, "", "").ToString());
        }

        [Theory]
        [InlineData("123")]
        [InlineData("")]
        public void ConvertPrimitiveValueToChar_Throws(string input)
        {
            Assert.Throws<ValidationException>(
                () => ODataEntryDeserializer.ConvertPrimitiveValue(input, typeof(char), "property", "type"),
                "The property 'property' on type 'type' must be a string with a length of 1.");
        }

        [Fact]
        public void ConvertPrimitiveValueToNullableChar_Throws()
        {
            Assert.Throws<ValidationException>(
                () => ODataEntryDeserializer.ConvertPrimitiveValue("123", typeof(char?), "property", "type"),
                "The property 'property' on type 'type' must be a string with a maximum length of 1.");
        }

        [Fact]
        public void ConvertPrimitiveValueToXElement_Throws_IfInputIsNotString()
        {
            Assert.Throws<ValidationException>(
                () => ODataEntryDeserializer.ConvertPrimitiveValue(123, typeof(XElement), "property", "type"),
                "The property 'property' on type 'type' must be a string.");
        }

        [Theory]
        [InlineData("Property", true, typeof(int))]
        [InlineData("Property", false, typeof(int))]
        [InlineData("PropertyNotPresent", true, null)]
        [InlineData("PropertyNotPresent", false, null)]
        public void GetPropertyType_NonDelta(string propertyName, bool isDelta, Type expectedPropertyType)
        {
            object resource = isDelta ? (object)new Delta<GetPropertyType_TestClass>() : new GetPropertyType_TestClass();
            Assert.Equal(
                expectedPropertyType,
                ODataEntryDeserializer.GetPropertyType(resource, propertyName, isDelta));
        }

        private class GetPropertyType_TestClass
        {
            public int Property { get; set; }
        }
    }
}
