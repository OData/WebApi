// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Http.OData.Formatter.Serialization.Models;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Library;
using Microsoft.Data.OData;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.OData.Formatter.Serialization
{
    public class ODataCollectionSerializerTests
    {
        IEdmModel _model;
        IEdmEntitySet _customerSet;
        Customer _customer;
        ODataCollectionSerializer _serializer;
        IEdmPrimitiveType _edmIntType;

        public ODataCollectionSerializerTests()
        {
            _model = SerializationTestsHelpers.SimpleCustomerOrderModel();
            _customerSet = _model.FindDeclaredEntityContainer("Default.Container").FindEntitySet("Customers");
            _edmIntType = _model.FindType("Edm.Int32") as IEdmPrimitiveType;
            _customer = new Customer()
            {
                FirstName = "Foo",
                LastName = "Bar",
                ID = 10,
            };

            ODataSerializerProvider serializerProvider = new DefaultODataSerializerProvider();
            _serializer = new ODataCollectionSerializer(
                new EdmCollectionTypeReference(
                    new EdmCollectionType(
                        new EdmPrimitiveTypeReference(_edmIntType, isNullable: false)),
                        isNullable: false), serializerProvider);
        }

        [Fact]
        public void Ctor_ThrowsArgumentNull_EdmType()
        {
            Assert.ThrowsArgumentNull(
                () => new ODataCollectionSerializer(edmType: null, serializerProvider: null),
                "edmType");
        }

        [Fact]
        public void Ctor_ThrowsArgumentNull_SerializerProvider()
        {
            Assert.ThrowsArgumentNull(
                () => new ODataCollectionSerializer(edmType: new Mock<IEdmCollectionTypeReference>().Object, serializerProvider: null),
                "serializerProvider");
        }

        [Fact]
        public void Ctor_ThrowsArgument_EdmType_IfElementTypeIsNull()
        {
            Mock<IEdmCollectionType> collectionType = new Mock<IEdmCollectionType>();
            collectionType.Setup(c => c.ElementType).Returns<IEdmCollectionType>(null);
            IEdmCollectionTypeReference collectionTypeReference = new EdmCollectionTypeReference(collectionType.Object, isNullable: true);

            Assert.ThrowsArgument(
                () => new ODataCollectionSerializer(edmType: collectionTypeReference, serializerProvider: new DefaultODataSerializerProvider()),
                "edmType",
                "The element type of the EDM collection type '' is null. Collection types with null element type are not valid.");
        }

        [Fact]
        public void Ctor_SetsProperty_ElementType()
        {
            // Arrange
            Mock<IEdmCollectionType> collectionType = new Mock<IEdmCollectionType>();
            Mock<IEdmTypeReference> elementType = new Mock<IEdmTypeReference>();
            collectionType.Setup(c => c.ElementType).Returns(elementType.Object);
            IEdmCollectionTypeReference collectionTypeReference = new EdmCollectionTypeReference(collectionType.Object, isNullable: true);

            // Act
            var serializer = new ODataCollectionSerializer(collectionTypeReference, new Mock<ODataSerializerProvider>().Object);

            // Assert
            Assert.Equal(elementType.Object, serializer.ElementType);
        }

        [Fact]
        public void Ctor_SetsProperty_CollectionType()
        {
            // Arrange
            Mock<IEdmCollectionType> collectionType = new Mock<IEdmCollectionType>();
            Mock<IEdmTypeReference> elementType = new Mock<IEdmTypeReference>();
            collectionType.Setup(c => c.ElementType).Returns(elementType.Object);
            IEdmCollectionTypeReference collectionTypeReference = new EdmCollectionTypeReference(collectionType.Object, isNullable: true);

            // Act
            var serializer = new ODataCollectionSerializer(collectionTypeReference, new Mock<ODataSerializerProvider>().Object);

            // Assert
            Assert.Equal(collectionTypeReference, serializer.CollectionType);
        }

        [Fact]
        public void WriteObject_Throws_ArgumentNull_MessageWriter()
        {
            Assert.ThrowsArgumentNull(
                () => _serializer.WriteObject(graph: null, messageWriter: null, writeContext: null),
                "messageWriter");
        }

        [Fact]
        public void WriteObject_Throws_ArgumentNull_WriteContext()
        {
            Assert.ThrowsArgumentNull(
                () => _serializer.WriteObject(graph: null, messageWriter: ODataTestUtil.GetMockODataMessageWriter(), writeContext: null),
                "writeContext");
        }

        [Fact]
        public void CreateODataValue_Calls_CreateODataCollectionValue()
        {
            // Arrange
            ODataCollectionValue oDataCollectionValue = new ODataCollectionValue();
            var collection = new object[0];
            Mock<ODataCollectionSerializer> serializer =
                new Mock<ODataCollectionSerializer>(_serializer.CollectionType, new DefaultODataSerializerProvider());
            serializer.CallBase = true;
            serializer
                .Setup(s => s.CreateODataCollectionValue(collection, It.IsAny<ODataSerializerContext>()))
                .Returns(oDataCollectionValue)
                .Verifiable();

            // Act
            ODataValue value = serializer.Object.CreateODataValue(collection, new ODataSerializerContext());

            // Assert
            serializer.Verify();
            Assert.Same(oDataCollectionValue, value);
        }

        [Fact]
        public void CreateODataCollectionValue_Serializes_AllElementsInTheCollection()
        {
            var oDataValue = _serializer.CreateODataCollectionValue(new int[] { 1, 2, 3 }, new ODataSerializerContext());

            var values = Assert.IsType<ODataCollectionValue>(oDataValue);

            List<int> elements = new List<int>();
            foreach (var item in values.Items)
            {
                elements.Add(Assert.IsType<int>(item));
            }

            Assert.Equal(elements, new int[] { 1, 2, 3 });
        }

        [Fact]
        public void CreateODataCollectionValue_Returns_EmptyODataCollectionValue_ForNull()
        {
            var oDataValue = _serializer.CreateODataCollectionValue(null, new ODataSerializerContext());

            Assert.NotNull(oDataValue);
            ODataCollectionValue collection = Assert.IsType<ODataCollectionValue>(oDataValue);
            Assert.Empty(collection.Items);
        }

        [Fact]
        public void CreateODataCollectionValue_SetsTypeName()
        {
            // Arrange
            object graph = new int[] { 1, 2, 3 };
            ODataSerializerContext context = new ODataSerializerContext();

            // Act
            ODataValue oDataValue = _serializer.CreateODataCollectionValue(graph, context);

            // Assert
            ODataCollectionValue collection = Assert.IsType<ODataCollectionValue>(oDataValue);
            Assert.Equal("Collection(Edm.Int32)", collection.TypeName);
        }

        [Fact]
        public void AddTypeNameAnnotationAsNeeded_DoesNotAddAnnotation_InDefaultMetadataMode()
        {
            // Arrange
            ODataCollectionValue value = new ODataCollectionValue();

            // Act
            ODataCollectionSerializer.AddTypeNameAnnotationAsNeeded(value, ODataMetadataLevel.Default);

            // Assert
            Assert.Null(value.GetAnnotation<SerializationTypeNameAnnotation>());
        }

        [Fact]
        public void AddTypeNameAnnotationAsNeeded_AddsAnnotation_InJsonLightMetadataMode()
        {
            // Arrange
            string expectedTypeName = "TypeName";
            ODataCollectionValue value = new ODataCollectionValue
            {
                TypeName = expectedTypeName
            };

            // Act
            ODataCollectionSerializer.AddTypeNameAnnotationAsNeeded(value, ODataMetadataLevel.FullMetadata);

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
            bool actualResult = ODataCollectionSerializer.ShouldAddTypeNameAnnotation(
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
            bool actualResult = ODataCollectionSerializer.ShouldSuppressTypeNameSerialization(
                (ODataMetadataLevel)metadataLevel);

            // Assert
            Assert.Equal(expectedResult, actualResult);
        }
    }
}
