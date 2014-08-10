// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Web.OData.Formatter.Serialization.Models;
using Microsoft.OData.Core;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.OData.Formatter.Serialization
{
    public class ODataCollectionSerializerTests
    {
        IEdmModel _model;
        IEdmEntitySet _customerSet;
        Customer _customer;
        ODataCollectionSerializer _serializer;
        IEdmPrimitiveTypeReference _edmIntType;
        IEdmCollectionTypeReference _collectionType;

        public ODataCollectionSerializerTests()
        {
            _model = SerializationTestsHelpers.SimpleCustomerOrderModel();
            _customerSet = _model.EntityContainer.FindEntitySet("Customers");
            _edmIntType = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Int32, isNullable: false);
            _customer = new Customer()
            {
                FirstName = "Foo",
                LastName = "Bar",
                ID = 10,
            };

            ODataSerializerProvider serializerProvider = new DefaultODataSerializerProvider();
            _collectionType = new EdmCollectionTypeReference(new EdmCollectionType(_edmIntType));
            _serializer = new ODataCollectionSerializer(serializerProvider);
        }

        [Fact]
        public void Ctor_ThrowsArgumentNull_SerializerProvider()
        {
            Assert.ThrowsArgumentNull(() => new ODataCollectionSerializer(serializerProvider: null), "serializerProvider");
        }

        [Fact]
        public void WriteObject_Throws_ArgumentNull_MessageWriter()
        {
            Assert.ThrowsArgumentNull(
                () => _serializer.WriteObject(graph: null, type: typeof(int[]), messageWriter: null, writeContext: null),
                "messageWriter");
        }

        [Fact]
        public void WriteObject_Throws_ArgumentNull_WriteContext()
        {
            Assert.ThrowsArgumentNull(
                () => _serializer.WriteObject(graph: null, type: typeof(int[]),
                    messageWriter: ODataTestUtil.GetMockODataMessageWriter(), writeContext: null),
                "writeContext");
        }

        [Fact]
        public void WriteObject_WritesValueReturnedFrom_CreateODataCollectionValue()
        {
            // Arrange
            MemoryStream stream = new MemoryStream();
            IODataResponseMessage message = new ODataMessageWrapper(stream);
            ODataMessageWriterSettings settings = new ODataMessageWriterSettings()
            {
                ODataUri = new ODataUri { ServiceRoot = new Uri("http://any/") }
            };

            settings.SetContentType(ODataFormat.Json);

            ODataMessageWriter messageWriter = new ODataMessageWriter(message, settings);
            Mock<ODataCollectionSerializer> serializer = new Mock<ODataCollectionSerializer>(new DefaultODataSerializerProvider());
            ODataSerializerContext writeContext = new ODataSerializerContext { RootElementName = "CollectionName", Model = _model };
            IEnumerable enumerable = new object[0];
            ODataCollectionValue collectionValue = new ODataCollectionValue { TypeName = "NS.Name", Items = new[] { 0, 1, 2 } };

            serializer.CallBase = true;
            serializer
                .Setup(s => s.CreateODataCollectionValue(enumerable, It.Is<IEdmTypeReference>(e => e.Definition == _edmIntType.Definition), writeContext))
                .Returns(collectionValue).Verifiable();

            // Act
            serializer.Object.WriteObject(enumerable, typeof(int[]), messageWriter, writeContext);

            // Assert
            serializer.Verify();
            stream.Seek(0, SeekOrigin.Begin);
            string result = new StreamReader(stream).ReadToEnd();
            Assert.Equal("{\"@odata.context\":\"http://any/$metadata#Collection(Edm.Int32)\",\"value\":[0,1,2]}", result);
        }

        [Fact]
        public void CreateODataCollectionValue_ThrowsArgumentNull_WriteContext()
        {
            Assert.ThrowsArgumentNull(
                () => _serializer.CreateODataCollectionValue(enumerable: null, elementType: _edmIntType, writeContext: null),
                "writeContext");
        }

        [Fact]
        public void CreateODataValue_ThrowsArgument_IfGraphIsNull()
        {
            // Arrange
            IEnumerable nullEnumerable = null;
            var serializerProvider = new Mock<ODataSerializerProvider>();
            var serializer = new ODataCollectionSerializer(serializerProvider.Object);
            serializerProvider.Setup(s => s.GetEdmTypeSerializer(It.IsAny<IEdmTypeReference>())).Returns<IEdmTypeReference>(null);

            // Act and Assert
            Assert.Throws<SerializationException>(
                () => serializer.CreateODataValue(graph: nullEnumerable, expectedType: _collectionType, writeContext: new ODataSerializerContext()),
                "Null collections cannot be serialized.");
        }

        [Fact]
        public void CreateODataValue_ThrowsArgument_IfGraphIsNotEnumerable()
        {
            object nonEnumerable = new object();
            Mock<ODataSerializerProvider> serializerProvider = new Mock<ODataSerializerProvider>();
            var serializer = new ODataCollectionSerializer(serializerProvider.Object);
            serializerProvider.Setup(s => s.GetEdmTypeSerializer(It.IsAny<IEdmTypeReference>())).Returns<IEdmTypeReference>(null);

            Assert.ThrowsArgument(
                () => serializer.CreateODataValue(graph: nonEnumerable, expectedType: _collectionType, writeContext: new ODataSerializerContext()),
                "graph",
                "The argument must be of type 'IEnumerable'.");
        }

        [Fact]
        public void CreateODataCollectionValue_ThrowsSerializationException_TypeCannotBeSerialized()
        {
            IEnumerable enumerable = new[] { 0 };
            Mock<ODataSerializerProvider> serializerProvider = new Mock<ODataSerializerProvider>();
            var serializer = new ODataCollectionSerializer(serializerProvider.Object);
            serializerProvider.Setup(s => s.GetEdmTypeSerializer(It.IsAny<IEdmTypeReference>())).Returns<IEdmTypeReference>(null);

            Assert.Throws<SerializationException>(
                () => serializer.CreateODataCollectionValue(enumerable, _edmIntType, new ODataSerializerContext { Model = _model }),
                "'Edm.Int32' cannot be serialized using the ODataMediaTypeFormatter.");
        }

        [Fact]
        public void CreateODataValue_Calls_CreateODataCollectionValue()
        {
            // Arrange
            ODataCollectionValue oDataCollectionValue = new ODataCollectionValue();
            var collection = new object[0];
            Mock<ODataCollectionSerializer> serializer = new Mock<ODataCollectionSerializer>(new DefaultODataSerializerProvider());
            ODataSerializerContext writeContext = new ODataSerializerContext();
            serializer.CallBase = true;
            serializer
                .Setup(s => s.CreateODataCollectionValue(collection, _edmIntType, writeContext))
                .Returns(oDataCollectionValue)
                .Verifiable();

            // Act
            ODataValue value = serializer.Object.CreateODataValue(collection, _collectionType, writeContext);

            // Assert
            serializer.Verify();
            Assert.Same(oDataCollectionValue, value);
        }

        [Fact]
        public void CreateODataCollectionValue_Serializes_AllElementsInTheCollection()
        {
            ODataSerializerContext writeContext = new ODataSerializerContext { Model = _model };
            var oDataValue = _serializer.CreateODataCollectionValue(new int[] { 1, 2, 3 }, _edmIntType, writeContext);

            var values = Assert.IsType<ODataCollectionValue>(oDataValue);

            List<int> elements = new List<int>();
            foreach (var item in values.Items)
            {
                elements.Add(Assert.IsType<int>(item));
            }

            Assert.Equal(elements, new int[] { 1, 2, 3 });
        }

        [Fact]
        public void CreateODataCollectionValue_CanSerialize_IEdmObjects()
        {
            // Arrange
            Mock<IEdmComplexObject> edmComplexObject = new Mock<IEdmComplexObject>();
            IEdmComplexObject[] collection = new IEdmComplexObject[] { edmComplexObject.Object };
            ODataSerializerContext serializerContext = new ODataSerializerContext();
            IEdmComplexTypeReference elementType = new EdmComplexTypeReference(new EdmComplexType("NS", "ComplexType"), isNullable: true);
            edmComplexObject.Setup(s => s.GetEdmType()).Returns(elementType);
            IEdmCollectionTypeReference collectionType = new EdmCollectionTypeReference(new EdmCollectionType(elementType));

            Mock<ODataSerializerProvider> serializerProvider = new Mock<ODataSerializerProvider>();
            Mock<ODataComplexTypeSerializer> elementSerializer = new Mock<ODataComplexTypeSerializer>(MockBehavior.Strict, serializerProvider.Object);
            serializerProvider.Setup(s => s.GetEdmTypeSerializer(elementType)).Returns(elementSerializer.Object);
            elementSerializer.Setup(s => s.CreateODataComplexValue(collection[0], elementType, serializerContext)).Returns(new ODataComplexValue()).Verifiable();

            ODataCollectionSerializer serializer = new ODataCollectionSerializer(serializerProvider.Object);

            // Act
            var result = serializer.CreateODataCollectionValue(collection, elementType, serializerContext);

            // Assert
            elementSerializer.Verify();
        }

        [Fact]
        public void CreateODataCollectionValue_Returns_EmptyODataCollectionValue_ForNull()
        {
            var oDataValue = _serializer.CreateODataCollectionValue(null, _edmIntType, new ODataSerializerContext());

            Assert.NotNull(oDataValue);
            ODataCollectionValue collection = Assert.IsType<ODataCollectionValue>(oDataValue);
            Assert.Empty(collection.Items);
        }

        [Fact]
        public void CreateODataCollectionValue_SetsTypeName()
        {
            // Arrange
            IEnumerable enumerable = new int[] { 1, 2, 3 };
            ODataSerializerContext context = new ODataSerializerContext { Model = _model };

            // Act
            ODataValue oDataValue = _serializer.CreateODataCollectionValue(enumerable, _edmIntType, context);

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
            ODataCollectionSerializer.AddTypeNameAnnotationAsNeeded(value, ODataMetadataLevel.MinimalMetadata);

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

        [Fact]
        public void AddTypeNameAnnotationAsNeeded_AddsAnnotationWithNull_InJsonLightNoMetadataMode()
        {
            // Arrange
            string expectedTypeName = "TypeName";
            ODataCollectionValue value = new ODataCollectionValue
            {
                TypeName = expectedTypeName
            };

            // Act
            ODataCollectionSerializer.AddTypeNameAnnotationAsNeeded(value, ODataMetadataLevel.NoMetadata);

            // Assert
            SerializationTypeNameAnnotation annotation = value.GetAnnotation<SerializationTypeNameAnnotation>();
            Assert.NotNull(annotation); // Guard
            Assert.Null(annotation.TypeName);
        }

        [Theory]
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
