// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using System.Web.OData.Builder;
using System.Web.OData.Builder.TestModels;
using System.Web.OData.Formatter;
using System.Web.OData.Formatter.Deserialization;
using Microsoft.OData.Core;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.OData
{
    public class EnumDeserializerTest
    {
        [Fact]
        public void GetEdmTypeDeserializer_ReturnODataEnumDeserializer_ForEnumType()
        {
            // Arrange
            IEdmTypeReference edmType = new EdmEnumTypeReference(new EdmEnumType("TestModel", "Color"), isNullable: false);

            // Act
            ODataEdmTypeDeserializer deserializer = new DefaultODataDeserializerProvider().GetEdmTypeDeserializer(edmType);

            // Assert
            Assert.NotNull(deserializer);
            Assert.IsType<ODataEnumDeserializer>(deserializer);
        }

        [Fact]
        public void ConvertEnumValue_ReturnEnumValue_ForEnumType()
        {
            // Arrange
            object value = new ODataEnumValue("Red");
            Type type = typeof(Color);

            // Act & Assert
            Assert.Equal(Enum.ToObject(typeof(Color), Color.Red), EnumDeserializationHelpers.ConvertEnumValue(value, type));
        }

        [Fact]
        public void ConvertEnumValue_Throws_ForNullValueParameter()
        {
            // Arrange
            object value = null;
            Type type = typeof(Color);

            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => EnumDeserializationHelpers.ConvertEnumValue(value, type),
                "value");
        }

        [Fact]
        public void ConvertEnumValue_Throws_ForNullTypeParameter()
        {
            // Arrange
            object value = new ODataEnumValue("Red");
            Type type = null;

            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => EnumDeserializationHelpers.ConvertEnumValue(value, type),
                "type");
        }

        [Fact]
        public void ConvertEnumValue_Throws_ForNonEnumValue()
        {
            // Arrange
            object value = new ODataPrimitiveValue(0);
            Type type = typeof(Color);

            // Act & Assert
            Assert.Throws<ValidationException>(
                () => EnumDeserializationHelpers.ConvertEnumValue(value, type),
                "The value with type 'ODataPrimitiveValue' must have type 'ODataEnumValue'.");
        }

        [Fact]
        public void ConvertEnumValue_Throws_ForNonEnumType()
        {
            // Arrange
            object value = new ODataEnumValue("Red");
            Type type = typeof(int);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => EnumDeserializationHelpers.ConvertEnumValue(value, type),
                "The type 'Int32' must be an enum or Nullable<T> where T is an enum type.");
        }

        [Fact]
        public void ODataEnumDeserializerRead_Throws_ForNullMessageReaderParameter()
        {
            // Arrange
            ODataMessageReader messageReader = null;
            Type type = typeof(Color);
            ODataDeserializerContext readContext = new ODataDeserializerContext();

            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => new ODataEnumDeserializer().Read(messageReader, type, readContext),
                "messageReader");
        }

        [Fact]
        public void ODataEnumDeserializerRead_Throws_ForNullTypeParameter()
        {
            // Arrange
            ODataMessageReader messageReader = new ODataMessageReader(new Mock<IODataRequestMessage>().Object);
            Type type = null;
            ODataDeserializerContext readContext = new ODataDeserializerContext();

            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => new ODataEnumDeserializer().Read(messageReader, type, readContext),
                "type");
        }

        [Fact]
        public void ODataEnumDeserializerRead_Throws_ForNullReadContextParameter()
        {
            // Arrange
            ODataMessageReader messageReader = new ODataMessageReader(new Mock<IODataRequestMessage>().Object);
            Type type = typeof(Color);
            ODataDeserializerContext readContext = null;

            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => new ODataEnumDeserializer().Read(messageReader, type, readContext),
                "readContext");
        }

        [Fact]
        public void NullEnumValueDeserializerTest()
        {
            // Arrange
            var deserializerProvider = new Mock<ODataDeserializerProvider>().Object;
            var deserializer = new ODataComplexTypeDeserializer(deserializerProvider);
            ODataComplexValue complexValue = new ODataComplexValue
            {
                Properties = new[]
                { 
                    new ODataProperty { Name = "NullableColor", Value = null}
                },
                TypeName = "System.Web.OData.EnumComplexWithNullableEnum"
            };

            IEdmModel model = GetEdmModel();
            ODataDeserializerContext readContext = new ODataDeserializerContext() { Model = model };
            IEdmComplexTypeReference enumComplexTypeReference = model.GetEdmTypeReference(typeof(EnumComplexWithNullableEnum)).AsComplex();

            // Act
            var enumComplexWithNullableEnum = deserializer.ReadComplexValue(complexValue, enumComplexTypeReference, readContext) as EnumComplexWithNullableEnum;

            // Assert
            Assert.NotNull(enumComplexWithNullableEnum);
            Assert.Null(enumComplexWithNullableEnum.NullableColor);
        }

        [Fact]
        public void UndefinedEnumValueDeserializerTest()
        {
            // Arrange
            var deserializerProvider = new Mock<ODataDeserializerProvider>().Object;
            var deserializer = new ODataComplexTypeDeserializer(deserializerProvider);
            ODataComplexValue complexValue = new ODataComplexValue
            {
                Properties = new[]
                { 
                    new ODataProperty { Name = "RequiredColor", Value = (Color)123}
                },
                TypeName = "System.Web.OData.EnumComplexWithRequiredEnum"
            };

            IEdmModel model = GetEdmModel();
            ODataDeserializerContext readContext = new ODataDeserializerContext() { Model = model };
            IEdmComplexTypeReference enumComplexTypeReference = model.GetEdmTypeReference(typeof(EnumComplexWithRequiredEnum)).AsComplex();

            // Act
            var enumComplexWithRequiredEnum = deserializer.ReadComplexValue(complexValue, enumComplexTypeReference, readContext) as EnumComplexWithRequiredEnum;

            // Assert
            Assert.NotNull(enumComplexWithRequiredEnum);
            Assert.Equal((Color)123, enumComplexWithRequiredEnum.RequiredColor);
        }

        [Theory]
        [InlineData(Color.Red)]
        [InlineData(Color.Green | Color.Blue)]
        [InlineData((Color)1)]
        [InlineData((Color)123)]
        public void EnumValueDeserializerTest(Color color)
        {
            // Arrange
            var deserializerProvider = new Mock<ODataDeserializerProvider>().Object;
            var deserializer = new ODataComplexTypeDeserializer(deserializerProvider);
            ODataComplexValue complexValue = new ODataComplexValue
            {
                Properties = new[]
                { 
                    new ODataProperty { Name = "RequiredColor", Value = color}
                },
                TypeName = "System.Web.OData.EnumComplexWithRequiredEnum"
            };

            IEdmModel model = GetEdmModel();
            ODataDeserializerContext readContext = new ODataDeserializerContext() { Model = model };
            IEdmComplexTypeReference enumComplexTypeReference = model.GetEdmTypeReference(typeof(EnumComplexWithRequiredEnum)).AsComplex();

            // Act
            var enumComplexWithRequiredEnum = deserializer.ReadComplexValue(complexValue, enumComplexTypeReference, readContext) as EnumComplexWithRequiredEnum;

            // Assert
            Assert.NotNull(enumComplexWithRequiredEnum);
            Assert.Equal(color, enumComplexWithRequiredEnum.RequiredColor);
        }

        private IEdmModel GetEdmModel()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.ComplexType<EnumComplexWithRequiredEnum>();
            builder.ComplexType<EnumComplexWithNullableEnum>();
            return builder.GetEdmModel();
        }

        private class EnumComplexWithRequiredEnum
        {
            public Color RequiredColor { get; set; }
        }

        private class EnumComplexWithNullableEnum
        {
            public Color? NullableColor { get; set; }
        }
    }
}