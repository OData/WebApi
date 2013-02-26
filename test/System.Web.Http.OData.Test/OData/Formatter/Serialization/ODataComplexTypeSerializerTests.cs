// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Web.Http.OData.Formatter.Serialization.Models;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Library;
using Microsoft.Data.OData;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.OData.Formatter.Serialization
{
    public class ODataComplexTypeSerializerTests
    {
        IEdmModel _model;
        Address _address;
        ODataComplexTypeSerializer _serializer;
        IEdmComplexType _addressType;

        public ODataComplexTypeSerializerTests()
        {
            _model = SerializationTestsHelpers.SimpleCustomerOrderModel();
            _address = new Address()
            {
                Street = "One Microsoft Way",
                City = "Redmond",
                State = "Washington",
                Country = "United States",
                ZipCode = "98052"
            };

            _addressType = _model.FindDeclaredType("Default.Address") as IEdmComplexType;

            ODataSerializerProvider serializerProvider = new DefaultODataSerializerProvider();
            _serializer = new ODataComplexTypeSerializer(new EdmComplexTypeReference(_addressType, isNullable: false), serializerProvider);
        }

        [Fact]
        public void Ctor_ThrowsArgumentNull_EdmComplexType()
        {
            Assert.ThrowsArgumentNull(
                () => new ODataComplexTypeSerializer(edmType: null, serializerProvider: null),
                "edmType");
        }

        [Fact]
        public void Ctor_ThrowsArgumentNull_SerializerProvider()
        {
            Assert.ThrowsArgumentNull(
                () => new ODataComplexTypeSerializer(edmType: new Mock<IEdmComplexTypeReference>().Object, serializerProvider: null),
                "serializerProvider");
        }

        [Fact]
        public void Ctor_SetsProperty_ComplexType()
        {
            IEdmComplexTypeReference complexType = new Mock<IEdmComplexTypeReference>().Object;

            var serializer = new ODataComplexTypeSerializer(complexType, new DefaultODataSerializerProvider());

            Assert.Equal(complexType, serializer.ComplexType);
        }

        [Fact]
        public void CreateODataValue_Calls_CreateODataComplexValue()
        {
            // Arrange
            var oDataComplexValue = new ODataComplexValue();
            var complexObject = new object();
            Mock<ODataComplexTypeSerializer> serializer =
                new Mock<ODataComplexTypeSerializer>(_serializer.ComplexType, new DefaultODataSerializerProvider());
            serializer.CallBase = true;
            serializer
                .Setup(s => s.CreateODataComplexValue(complexObject, It.IsAny<ODataSerializerContext>()))
                .Returns(oDataComplexValue)
                .Verifiable();

            // Act
            ODataValue value = serializer.Object.CreateODataValue(complexObject, new ODataSerializerContext());

            // Assert
            serializer.Verify();
            Assert.Same(oDataComplexValue, value);
        }

        [Fact]
        public void CreateODataValue_ReturnsODataNullValue_ForNullValue()
        {
            var odataValue = _serializer.CreateODataValue(null, new ODataSerializerContext());

            Assert.IsType<ODataNullValue>(odataValue);
        }

        [Fact]
        public void CreateODataComplexValue_WritesAllDeclaredProperties()
        {
            var odataValue = _serializer.CreateODataComplexValue(_address, new ODataSerializerContext());

            ODataComplexValue complexValue = Assert.IsType<ODataComplexValue>(odataValue);

            Assert.Equal(complexValue.TypeName, "Default.Address");
            Assert.Equal(
                complexValue.Properties.Select(p => Tuple.Create(p.Name, p.Value as string)),
                new[] { 
                    Tuple.Create("Street","One Microsoft Way"), 
                    Tuple.Create("City","Redmond"),
                    Tuple.Create("State","Washington"),
                    Tuple.Create("Country", "United States"),
                    Tuple.Create("ZipCode","98052") });
        }

        [Fact]
        public void CreateODataComplexValue_ReturnsNull_ForNullValue()
        {
            var odataValue = _serializer.CreateODataComplexValue(null, new ODataSerializerContext());

            Assert.Null(odataValue);
        }

        [Fact]
        public void CreateODataComplexValue_ThrowsTypeCannotBeSerialized()
        {
            IEdmPrimitiveTypeReference stringType = EdmCoreModel.Instance.GetString(isNullable: true);
            Mock<ODataSerializerProvider> serializerProvider = new Mock<ODataSerializerProvider>();
            serializerProvider.Setup(s => s.GetEdmTypeSerializer(stringType)).Returns<ODataEntrySerializer>(null);
            ODataComplexTypeSerializer serializer = new ODataComplexTypeSerializer(_serializer.ComplexType, serializerProvider.Object);

            Assert.Throws<NotSupportedException>(
                () => serializer.CreateODataComplexValue(_address, new ODataSerializerContext()),
                "'Edm.String' cannot be serialized using the ODataMediaTypeFormatter.");
        }

        [Fact]
        public void AddTypeNameAnnotationAsNeeded_DoesNotAddAnnotation_InDefaultMetadataMode()
        {
            // Arrange
            ODataComplexValue value = new ODataComplexValue();

            // Act
            ODataComplexTypeSerializer.AddTypeNameAnnotationAsNeeded(value, ODataMetadataLevel.Default);

            // Assert
            Assert.Null(value.GetAnnotation<SerializationTypeNameAnnotation>());
        }

        [Fact]
        public void AddTypeNameAnnotationAsNeeded_AddsAnnotation_InJsonLightMetadataMode()
        {
            // Arrange
            string expectedTypeName = "TypeName";
            ODataComplexValue value = new ODataComplexValue
            {
                TypeName = expectedTypeName
            };

            // Act
            ODataComplexTypeSerializer.AddTypeNameAnnotationAsNeeded(value, ODataMetadataLevel.FullMetadata);

            // Assert
            SerializationTypeNameAnnotation annotation = value.GetAnnotation<SerializationTypeNameAnnotation>();
            Assert.NotNull(annotation); // Guard
            Assert.Equal(expectedTypeName, annotation.TypeName);
        }

        [Theory]
        [InlineData(TestODataMetadataLevel.Default, false)]
        [InlineData(TestODataMetadataLevel.FullMetadata, true)]
        [InlineData(TestODataMetadataLevel.MinimalMetadata, false)]
        [InlineData(TestODataMetadataLevel.NoMetadata, true)]
        public void ShouldAddTypeNameAnnotation(TestODataMetadataLevel metadataLevel, bool expectedResult)
        {
            // Act
            bool actualResult = ODataComplexTypeSerializer.ShouldAddTypeNameAnnotation(
                (ODataMetadataLevel)metadataLevel);

            // Assert
            Assert.Equal(expectedResult, actualResult);
        }

        [Theory]
        [InlineData(TestODataMetadataLevel.FullMetadata, false)]
        [InlineData(TestODataMetadataLevel.NoMetadata, true)]
        public void ShouldSuppressTypeNameSerialization(TestODataMetadataLevel metadataLevel, bool expectedResult)
        {
            // Act
            bool actualResult = ODataComplexTypeSerializer.ShouldSuppressTypeNameSerialization(
                (ODataMetadataLevel)metadataLevel);

            // Assert
            Assert.Equal(expectedResult, actualResult);
        }
    }
}
