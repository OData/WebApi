//-----------------------------------------------------------------------------
// <copyright file="EnumDeserializerTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.DataAnnotations;
using System.Xml;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Formatter.Deserialization;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Builder.TestModels;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using Moq;
using Xunit;

namespace Microsoft.AspNet.OData.Test
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
                TypeName = "Microsoft.AspNet.OData.Test.EnumComplexWithNullableEnum"
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
                TypeName = "Microsoft.AspNet.OData.Test.EnumComplexWithRequiredEnum"
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
                TypeName = "Microsoft.AspNet.OData.Test.EnumComplexWithRequiredEnum"
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

        [Fact]
        public void AlternativeModelEnumValueDeserializerTest()
        {
            // Arrange
            var deserializer = new ODataResourceDeserializer(_deserializerProvider);
            ODataResource resourceValue = new ODataResource
            {
                Properties = new[]
                {
                    new ODataProperty { Name = "AccountType", Value = new ODataEnumValue("Personal") }
                },
                TypeName = "Microsoft.AspNet.OData.Test.Account"
            };

            IEdmModel model = GetAlternativeEdmModel();
            ODataDeserializerContext readContext = new ODataDeserializerContext() { Model = model };
            IEdmTypeReference edmTypeReference = model.GetEdmTypeReference(typeof(Account));
            IEdmStructuredTypeReference enumStructuredTypeReference = edmTypeReference.AsStructured();

            // Act
            var account = deserializer.ReadResource(new ODataResourceWrapper(resourceValue), enumStructuredTypeReference, readContext) as Account;

            // Assert
            Assert.NotNull(account);
            Assert.Equal(AccountType.Personal, account.AccountType);
        }

        private IEdmModel GetAlternativeEdmModel()
        {
            const string edmx = @"<edmx:Edmx Version=""4.0"" xmlns:edmx=""http://docs.oasis-open.org/odata/ns/edmx"">
                  <edmx:DataServices>
                    <Schema Namespace=""Microsoft.AspNet.OData.Test"" xmlns=""http://docs.oasis-open.org/odata/ns/edm"">
                      <EntityType Name=""Account"">
                        <Key>
                          <PropertyRef Name=""AccountId"" />
                        </Key>
                        <Property Name=""AccountId"" Type=""Edm.Guid"" Nullable=""false"" />
                        <Property Name=""Name"" Type=""Edm.String"" />
                        <Property Name=""AccountType"" Type=""Microsoft.AspNet.OData.Test.AccountType"" Nullable=""false"" />
                      </EntityType>
                      <EnumType Name=""AccountType"">
                        <Member Name=""Corporate"" Value=""0"" />
                        <Member Name=""Personal"" Value=""1"" />
                        <Member Name=""Unknown"" Value=""2"" />
                      </EnumType>
                    </Schema>
                    <Schema Namespace=""Default"" xmlns=""http://docs.oasis-open.org/odata/ns/edm"">
                      <EntityContainer Name=""AccountService"">
                        <EntitySet Name=""Accounts"" EntityType=""Microsoft.AspNet.OData.Test.Account"" />
                      </EntityContainer>
                    </Schema>
                  </edmx:DataServices>
                </edmx:Edmx>";

            XmlReader reader = XmlReader.Create(new System.IO.StringReader(edmx));
            return CsdlReader.Parse(reader);
        }
    }
}
