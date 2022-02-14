//-----------------------------------------------------------------------------
// <copyright file="ODataCollectionSerializerTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Formatter.Serialization;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Moq;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Formatter.Serialization
{
    public class ODataCollectionSerializerTests
    {
        IEdmModel _model;
        ODataSerializerProvider _serializerProvider;
        ODataCollectionSerializer _serializer;
        IEdmPrimitiveTypeReference _edmIntType;
        IEdmCollectionTypeReference _collectionType;

        public ODataCollectionSerializerTests()
        {
            _model = SerializationTestsHelpers.SimpleCustomerOrderModel();
            _edmIntType = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Int32, isNullable: false);

            _serializerProvider = ODataSerializerProviderFactory.Create();
            _collectionType = new EdmCollectionTypeReference(new EdmCollectionType(_edmIntType));
            _serializer = new ODataCollectionSerializer(_serializerProvider);
        }

        [Fact]
        public void Ctor_ThrowsArgumentNull_SerializerProvider()
        {
            ExceptionAssert.ThrowsArgumentNull(() => new ODataCollectionSerializer(serializerProvider: null), "serializerProvider");
        }

        [Fact]
        public void WriteObject_Throws_ArgumentNull_MessageWriter()
        {
            ExceptionAssert.ThrowsArgumentNull(
                () => _serializer.WriteObject(graph: null, type: typeof(int[]), messageWriter: null, writeContext: null),
                "messageWriter");
        }

        [Fact]
        public void WriteObject_Throws_ArgumentNull_WriteContext()
        {
            ExceptionAssert.ThrowsArgumentNull(
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
            Mock<ODataCollectionSerializer> serializer = new Mock<ODataCollectionSerializer>(_serializerProvider);
            ODataSerializerContext writeContext = new ODataSerializerContext { RootElementName = "CollectionName", Model = _model };
            IEnumerable enumerable = new object[0];
            ODataCollectionValue collectionValue = new ODataCollectionValue { TypeName = "NS.Name", Items = new object[] { 0, 1, 2 } };

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
            ExceptionAssert.ThrowsArgumentNull(
                () => _serializer.CreateODataCollectionValue(enumerable: null, elementType: _edmIntType, writeContext: null),
                "writeContext");
        }

        [Fact]
        public void CreateODataValue_ThrowsArgument_IfGraphIsNotEnumerable()
        {
            object nonEnumerable = new object();
            Mock<ODataSerializerProvider> serializerProvider = new Mock<ODataSerializerProvider>();
            var serializer = new ODataCollectionSerializer(serializerProvider.Object);
            serializerProvider.Setup(s => s.GetEdmTypeSerializer(It.IsAny<IEdmTypeReference>())).Returns<IEdmTypeReference>(null);

            ExceptionAssert.ThrowsArgument(
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

            ExceptionAssert.Throws<SerializationException>(
                () => serializer.CreateODataCollectionValue(enumerable, _edmIntType, new ODataSerializerContext { Model = _model }),
                "'Edm.Int32' cannot be serialized using the ODataMediaTypeFormatter.");
        }

        [Fact]
        public void CreateODataValue_Calls_CreateODataCollectionValue()
        {
            // Arrange
            ODataCollectionValue oDataCollectionValue = new ODataCollectionValue();
            var collection = new object[0];
            Mock<ODataCollectionSerializer> serializer = new Mock<ODataCollectionSerializer>(_serializerProvider);
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
            Mock<IEdmEnumObject> edmEnumObject = new Mock<IEdmEnumObject>();
            IEdmEnumObject[] collection = { edmEnumObject.Object };
            ODataSerializerContext serializerContext = new ODataSerializerContext();
            IEdmEnumTypeReference elementType = new EdmEnumTypeReference(new EdmEnumType("NS", "EnumType"), isNullable: true);
            edmEnumObject.Setup(s => s.GetEdmType()).Returns(elementType);

            Mock<ODataSerializerProvider> serializerProvider = new Mock<ODataSerializerProvider>();
            Mock<ODataEnumSerializer> elementSerializer = new Mock<ODataEnumSerializer>(MockBehavior.Strict, serializerProvider.Object);
            serializerProvider.Setup(s => s.GetEdmTypeSerializer(elementType)).Returns(elementSerializer.Object);
            elementSerializer.Setup(s => s.CreateODataEnumValue(collection[0], elementType, serializerContext)).Returns(new ODataEnumValue("1", "NS.EnumType")).Verifiable();

            ODataCollectionSerializer serializer = new ODataCollectionSerializer(serializerProvider.Object);

            // Act
            serializer.CreateODataCollectionValue(collection, elementType, serializerContext);

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
            Assert.Null(value.TypeAnnotation);
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
            ODataTypeAnnotation annotation = value.TypeAnnotation;
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
            ODataTypeAnnotation annotation = value.TypeAnnotation;
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
