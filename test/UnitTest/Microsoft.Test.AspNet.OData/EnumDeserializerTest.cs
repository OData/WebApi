// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Formatter.Deserialization;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.Test.AspNet.OData.Builder.TestModels;
using Microsoft.Test.AspNet.OData.Common;
using Microsoft.Test.AspNet.OData.Factories;
using Moq;
using Xunit;

namespace Microsoft.Test.AspNet.OData
{
    public class EnumDeserializerTest
    {
        private readonly ODataDeserializerProvider _deserializerProvider =
            ODataDeserializerProviderFactory.Create();

        [Fact]
        public void GetEdmTypeDeserializer_ReturnODataEnumDeserializer_ForEnumType()
        {
            // Arrange
            IEdmTypeReference edmType = new EdmEnumTypeReference(new EdmEnumType("TestModel", "Color"), isNullable: false);

            // Act
            ODataEdmTypeDeserializer deserializer = _deserializerProvider.GetEdmTypeDeserializer(edmType);

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
            ExceptionAssert.ThrowsArgumentNull(
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
            ExceptionAssert.ThrowsArgumentNull(
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
            ExceptionAssert.Throws<ValidationException>(
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
            ExceptionAssert.Throws<InvalidOperationException>(
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
            ExceptionAssert.ThrowsArgumentNull(
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
            ExceptionAssert.ThrowsArgumentNull(
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
            ExceptionAssert.ThrowsArgumentNull(
                () => new ODataEnumDeserializer().Read(messageReader, type, readContext),
                "readContext");
        }

        [Fact]
        public void NullEnumValueDeserializerTest()
        {
            // Arrange
            var deserializerProvider = new Mock<ODataDeserializerProvider>().Object;
            var deserializer = new ODataResourceDeserializer(deserializerProvider);
            ODataResource resourceValue = new ODataResource
            {
                Properties = new[]
                {
                    new ODataProperty { Name = "NullableColor", Value = null}
                },
                TypeName = "Microsoft.Test.AspNet.OData.EnumComplexWithNullableEnum"
            };

            IEdmModel model = GetEdmModel();
            ODataDeserializerContext readContext = new ODataDeserializerContext() { Model = model };
            IEdmComplexTypeReference enumComplexTypeReference = model.GetEdmTypeReference(typeof(EnumComplexWithNullableEnum)).AsComplex();

            // Act
            var enumComplexWithNullableEnum =
                deserializer.ReadResource(new ODataResourceWrapper(resourceValue), enumComplexTypeReference, readContext)
                    as EnumComplexWithNullableEnum;

            // Assert
            Assert.NotNull(enumComplexWithNullableEnum);
            Assert.Null(enumComplexWithNullableEnum.NullableColor);
        }

        [Fact]
        public void UndefinedEnumValueDeserializerTest()
        {
            // Arrange
            var deserializer = new ODataResourceDeserializer(_deserializerProvider);
            ODataResource resourceValue = new ODataResource
            {
                Properties = new[]
                {
                    new ODataProperty { Name = "RequiredColor", Value = new ODataEnumValue("123") }
                },
                TypeName = "Microsoft.Test.AspNet.OData.EnumComplexWithRequiredEnum"
            };

            IEdmModel model = GetEdmModel();
            ODataDeserializerContext readContext = new ODataDeserializerContext() { Model = model };
            IEdmComplexTypeReference enumComplexTypeReference = model.GetEdmTypeReference(typeof(EnumComplexWithRequiredEnum)).AsComplex();

            // Act
            var enumComplexWithRequiredEnum = deserializer.ReadResource(new ODataResourceWrapper(resourceValue), enumComplexTypeReference, readContext) as EnumComplexWithRequiredEnum;

            // Assert
            Assert.NotNull(enumComplexWithRequiredEnum);
            Assert.Equal((Color)123, enumComplexWithRequiredEnum.RequiredColor);
        }

        [Theory]
        [InlineData(Color.Red)]
        [InlineData(Color.Green | Color.Blue)]
        [InlineData((Color)3)]
        [InlineData((Color)123)]
        public void EnumValueDeserializerTest(Color color)
        {
            // Arrange
            IEdmModel model = GetEdmModel();

            var deserializer = new ODataResourceDeserializer(_deserializerProvider);
            ODataResource resourceValue = new ODataResource
            {
                Properties = new[]
                {
                    new ODataProperty
                    {
                        Name = "RequiredColor",
                        Value = new ODataEnumValue(color.ToString())
                    }
                },
                TypeName = "Microsoft.Test.AspNet.OData.EnumComplexWithRequiredEnum"
            };

            ODataDeserializerContext readContext = new ODataDeserializerContext() { Model = model };
            IEdmComplexTypeReference enumComplexTypeReference = model.GetEdmTypeReference(typeof(EnumComplexWithRequiredEnum)).AsComplex();

            // Act
            var enumComplexWithRequiredEnum = deserializer.ReadResource(new ODataResourceWrapper(resourceValue), enumComplexTypeReference, readContext) as EnumComplexWithRequiredEnum;

            // Assert
            Assert.NotNull(enumComplexWithRequiredEnum);
            Assert.Equal(color, enumComplexWithRequiredEnum.RequiredColor);
        }

        private IEdmModel GetEdmModel()
        {
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
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