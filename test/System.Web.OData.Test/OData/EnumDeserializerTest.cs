// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using System.Web.Http.OData.Builder.TestModels;
using System.Web.Http.OData.Formatter;
using System.Web.Http.OData.Formatter.Deserialization;
using Microsoft.OData.Core;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;
using Microsoft.OData.Edm.Library.Values;
using Microsoft.TestCommon;
using Microsoft.TestCommon.Types;
using Moq;

namespace System.Web.Http.OData
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
        public void NullEnumValueDeserializerTest()
        {
            // Arrange
            var deserializerProvider = new Mock<ODataDeserializerProvider>().Object;
            var deserializer = new ODataComplexTypeDeserializer(deserializerProvider);
            ODataComplexValue complexValue = new ODataComplexValue
            {
                Properties = new[]
                { 
                    new ODataProperty { Name = "RequiredColor", Value = "Red"},
                    new ODataProperty { Name = "NullableColor", Value = null}
                },
                TypeName = "TestModel.EnumComplex"
            };

            IEdmModel model = GetEdmModel();
            ODataDeserializerContext readContext = new ODataDeserializerContext() { Model = model };
            IEdmComplexTypeReference enumComplexrTypeReference = model.GetEdmTypeReference(typeof(EnumComplex)).AsComplex();

            // Act
            var enumComplex = deserializer.ReadComplexValue(complexValue, enumComplexrTypeReference, readContext) as EnumComplex;

            // Assert
            Assert.NotNull(enumComplex);
            Assert.Equal(Color.Red, enumComplex.RequiredColor);
            Assert.Null(enumComplex.NullableColor);
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
                    new ODataProperty { Name = "RequiredColor", Value = (Color)123},
                    new ODataProperty { Name = "NullableColor", Value = Color.Green | Color.Blue}
                },
                TypeName = "TestModel.EnumComplex"
            };

            IEdmModel model = GetEdmModel();
            ODataDeserializerContext readContext = new ODataDeserializerContext() { Model = model };
            IEdmComplexTypeReference enumComplexrTypeReference = model.GetEdmTypeReference(typeof(EnumComplex)).AsComplex();

            // Act
            var enumComplex = deserializer.ReadComplexValue(complexValue, enumComplexrTypeReference, readContext) as EnumComplex;

            // Assert
            Assert.NotNull(enumComplex);
            Assert.Equal((Color)123, enumComplex.RequiredColor);
            Assert.Equal(Color.Green | Color.Blue, enumComplex.NullableColor);
        }

        private IEdmModel GetEdmModel()
        {
            EdmModel model = new EdmModel();

            EdmEnumType color = new EdmEnumType("TestModel", "Color");
            color.AddMember(new EdmEnumMember(color, "Red", new EdmIntegerConstant(1)));
            color.AddMember(new EdmEnumMember(color, "Green", new EdmIntegerConstant(2)));
            color.AddMember(new EdmEnumMember(color, "Blue", new EdmIntegerConstant(4)));
            model.AddElement(color);

            EdmComplexType enumComplex = new EdmComplexType("TestModel", "EnumComplex");
            enumComplex.AddStructuralProperty("RequiredColor", color.ToEdmTypeReference(isNullable: false));
            enumComplex.AddStructuralProperty("NullableColor", color.ToEdmTypeReference(isNullable: true));
            model.AddElement(enumComplex);

            model.SetAnnotationValue<ClrTypeAnnotation>(
                model.FindDeclaredType("TestModel.EnumComplex"), new ClrTypeAnnotation(typeof(EnumComplex)));

            return model;
        }

        private class EnumComplex
        {
            public Color RequiredColor { get; set; }
            public Color? NullableColor { get; set; }
        }
    }
}