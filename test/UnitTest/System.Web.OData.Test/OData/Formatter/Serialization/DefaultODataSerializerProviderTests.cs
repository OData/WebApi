// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Web.OData.Builder.TestModels;
using System.Web.OData.Extensions;
using System.Web.OData.Formatter.Deserialization;
using System.Web.OData.Routing;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Microsoft.TestCommon;
using Moq;
using ODataPath = System.Web.OData.Routing.ODataPath;

namespace System.Web.OData.Formatter.Serialization
{
    public class DefaultODataSerializerProviderTests
    {
        private ODataSerializerProvider _serializerProvider =
            DependencyInjectionHelper.GetDefaultODataSerializerProvider();
        private IEdmModel _edmModel = EdmTestHelpers.GetModel();

        public static TheoryDataSet<Type, EdmPrimitiveTypeKind> EdmPrimitiveMappingData
        {
            get
            {
                return new TheoryDataSet<Type, EdmPrimitiveTypeKind>
                {
                    { typeof(byte[]), EdmPrimitiveTypeKind.Binary },
                    { typeof(bool), EdmPrimitiveTypeKind.Boolean },
                    { typeof(byte), EdmPrimitiveTypeKind.Byte },
                    { typeof(DateTime), EdmPrimitiveTypeKind.DateTimeOffset },
                    { typeof(DateTimeOffset), EdmPrimitiveTypeKind.DateTimeOffset },
                    { typeof(Date), EdmPrimitiveTypeKind.Date },
                    { typeof(TimeOfDay), EdmPrimitiveTypeKind.TimeOfDay },
                    { typeof(decimal), EdmPrimitiveTypeKind.Decimal },
                    { typeof(double), EdmPrimitiveTypeKind.Double },
                    { typeof(Guid), EdmPrimitiveTypeKind.Guid },
                    { typeof(short), EdmPrimitiveTypeKind.Int16 },
                    { typeof(int), EdmPrimitiveTypeKind.Int32 },
                    { typeof(long), EdmPrimitiveTypeKind.Int64 },
                    { typeof(sbyte), EdmPrimitiveTypeKind.SByte },
                    { typeof(float), EdmPrimitiveTypeKind.Single },
                    { typeof(Stream), EdmPrimitiveTypeKind.Stream },
                    { typeof(string), EdmPrimitiveTypeKind.String },
                    { typeof(TimeSpan), EdmPrimitiveTypeKind.Duration },
                };
            }
        }

        [Fact]
        public void GetODataPayloadSerializer_ThrowsArgumentNull_Type()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();

            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => _serializerProvider.GetODataPayloadSerializer(type: null, request: request),
               "type");
        }

        [Fact]
        public void GetODataPayloadSerializer_ThrowsArgumentNull_Request()
        {
            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => _serializerProvider.GetODataPayloadSerializer(typeof(int), request: null),
               "request");
        }

        [Theory]
        [PropertyData("EdmPrimitiveMappingData")]
        public void GetODataSerializer_Primitive(Type type, EdmPrimitiveTypeKind edmPrimitiveTypeKind)
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            request.EnableHttpDependencyInjectionSupport();

            // Act
            var serializer = _serializerProvider.GetODataPayloadSerializer(type, request);

            // Assert
            Assert.NotNull(serializer);
            var primitiveSerializer = Assert.IsType<ODataPrimitiveSerializer>(serializer);
            Assert.Equal(primitiveSerializer.ODataPayloadKind, ODataPayloadKind.Property);
        }

        [Theory]
        [PropertyData("EdmPrimitiveMappingData")]
        public void GetODataPayloadSerializer_ReturnsRawValueSerializer_ForValueRequests(Type type, EdmPrimitiveTypeKind edmPrimitiveTypeKind)
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            request.ODataProperties().Path =
                new ODataPath(new ValueSegment(EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.Int32)));
            request.EnableHttpDependencyInjectionSupport(_edmModel);

            // Act
            var serializer = _serializerProvider.GetODataPayloadSerializer(type, request);

            // Assert
            Assert.NotNull(serializer);
            Assert.Equal(ODataPayloadKind.Value, serializer.ODataPayloadKind);
        }

        [Fact]
        public void GetODataSerializer_Enum()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            request.EnableHttpDependencyInjectionSupport(GetEnumModel());

            // Act
            var serializer = _serializerProvider.GetODataPayloadSerializer(typeof(TestEnum), request);

            // Assert
            Assert.NotNull(serializer);
            var enumSerializer = Assert.IsType<ODataEnumSerializer>(serializer);
            Assert.Equal(ODataPayloadKind.Property, enumSerializer.ODataPayloadKind);
        }

        [Fact]
        public void GetODataPayloadSerializer_ReturnsRawValueSerializer_ForEnumValueRequests()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            request.ODataProperties().Path =
                new ODataPath(new ValueSegment(EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.Int32)));
            request.EnableHttpDependencyInjectionSupport(GetEnumModel());

            // Act
            var serializer = _serializerProvider.GetODataPayloadSerializer(typeof(TestEnum), request);

            // Assert
            Assert.NotNull(serializer);
            var rawValueSerializer = Assert.IsType<ODataRawValueSerializer>(serializer);
            Assert.Equal(ODataPayloadKind.Value, rawValueSerializer.ODataPayloadKind);
        }

        [Theory]
        [InlineData("DollarCountEntities/$count", typeof(ODataCountTest.DollarCountEntity))]
        [InlineData("DollarCountEntities(5)/StringCollectionProp/$count", typeof(string))]
        [InlineData("DollarCountEntities(5)/EnumCollectionProp/$count", typeof(Color))]
        [InlineData("DollarCountEntities(5)/TimeSpanCollectionProp/$count", typeof(TimeSpan))]
        [InlineData("DollarCountEntities(5)/ComplexCollectionProp/$count", typeof(ODataCountTest.DollarCountComplex))]
        [InlineData("DollarCountEntities(5)/EntityCollectionProp/$count", typeof(ODataCountTest.DollarCountEntity))]
        [InlineData("UnboundFunctionReturnsPrimitveCollection()/$count", typeof(int))]
        [InlineData("UnboundFunctionReturnsEnumCollection()/$count", typeof(Color))]
        [InlineData("UnboundFunctionReturnsDateTimeOffsetCollection()/$count", typeof(DateTimeOffset))]
        [InlineData("UnboundFunctionReturnsDateCollection()/$count", typeof(Date))]
        [InlineData("UnboundFunctionReturnsComplexCollection()/$count", typeof(ODataCountTest.DollarCountComplex))]
        [InlineData("UnboundFunctionReturnsEntityCollection()/$count", typeof(ODataCountTest.DollarCountEntity))]
        [InlineData("DollarCountEntities/Default.BoundFunctionReturnsPrimitveCollection()/$count", typeof(DateTimeOffset))]
        [InlineData("DollarCountEntities/Default.BoundFunctionReturnsEnumCollection()/$count", typeof(Color))]
        [InlineData("DollarCountEntities/Default.BoundFunctionReturnsDateTimeOffsetCollection()/$count", typeof(DateTimeOffset))]
        [InlineData("DollarCountEntities/Default.BoundFunctionReturnsComplexCollection()/$count", typeof(ODataCountTest.DollarCountComplex))]
        [InlineData("DollarCountEntities/Default.BoundFunctionReturnsEntityCollection()/$count", typeof(ODataCountTest.DollarCountEntity))]
        public void GetODataPayloadSerializer_ReturnsRawValueSerializer_ForDollarCountRequests(string uri, Type elementType)
        {
            // Arrange
            IEdmModel model = ODataCountTest.GetEdmModel();
            Type type = typeof(ICollection<>).MakeGenericType(elementType);
            var request = new HttpRequestMessage();
            var pathHandler = new DefaultODataPathHandler();
            var path = pathHandler.Parse(model, "http://localhost/", uri);
            request.ODataProperties().Path = path;
            request.EnableHttpDependencyInjectionSupport(model);

            // Act
            var serializer = _serializerProvider.GetODataPayloadSerializer(type, request);

            // Assert
            Assert.NotNull(serializer);
            var rawValueSerializer = Assert.IsType<ODataRawValueSerializer>(serializer);
            Assert.Equal(ODataPayloadKind.Value, rawValueSerializer.ODataPayloadKind);
        }

        [Fact]
        public void GetODataSerializer_Resource_ForEntity()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            request.EnableHttpDependencyInjectionSupport(_edmModel);

            // Act
            var serializer = _serializerProvider.GetODataPayloadSerializer(typeof(ODataResourceDeserializerTests.Product), request);

            // Assert
            Assert.NotNull(serializer);
            var entitySerializer = Assert.IsType<ODataResourceSerializer>(serializer);
            Assert.Equal(entitySerializer.SerializerProvider, _serializerProvider);
            Assert.Equal(entitySerializer.ODataPayloadKind, ODataPayloadKind.Resource);
        }

        [Fact]
        public void GetODataSerializer_Resource_ForComplex()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            request.EnableHttpDependencyInjectionSupport(_edmModel);

            // Act
            var serializer = _serializerProvider.GetODataPayloadSerializer(typeof(ODataResourceDeserializerTests.Address), request);

            // Assert
            Assert.NotNull(serializer);
            var complexSerializer = Assert.IsType<ODataResourceSerializer>(serializer);
            Assert.Equal(complexSerializer.SerializerProvider, _serializerProvider);
            Assert.Equal(complexSerializer.ODataPayloadKind, ODataPayloadKind.Resource);
        }

        [Theory]
        [InlineData(typeof(ODataResourceDeserializerTests.Supplier[]))]
        [InlineData(typeof(IEnumerable<ODataResourceDeserializerTests.Supplier>))]
        [InlineData(typeof(ICollection<ODataResourceDeserializerTests.Supplier>))]
        [InlineData(typeof(IList<ODataResourceDeserializerTests.Supplier>))]
        [InlineData(typeof(List<ODataResourceDeserializerTests.Supplier>))]
        [InlineData(typeof(PageResult<ODataResourceDeserializerTests.Supplier>))]
        public void GetODataSerializer_ResourceSet_ForEntityCollection(Type collectionType)
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            request.EnableHttpDependencyInjectionSupport(_edmModel);

            // Act
            var serializer = _serializerProvider.GetODataPayloadSerializer(collectionType, request);

            // Assert
            Assert.NotNull(serializer);
            var resourceSetSerializer = Assert.IsType<ODataResourceSetSerializer>(serializer);
            Assert.Equal(resourceSetSerializer.ODataPayloadKind, ODataPayloadKind.ResourceSet);
            Assert.Same(resourceSetSerializer.SerializerProvider, _serializerProvider);
        }

        [Theory]
        [InlineData(typeof(ODataResourceDeserializerTests.Address[]))]
        [InlineData(typeof(IEnumerable<ODataResourceDeserializerTests.Address>))]
        [InlineData(typeof(ICollection<ODataResourceDeserializerTests.Address>))]
        [InlineData(typeof(IList<ODataResourceDeserializerTests.Address>))]
        [InlineData(typeof(List<ODataResourceDeserializerTests.Address>))]
        [InlineData(typeof(PageResult<ODataResourceDeserializerTests.Address>))]
        public void GetODataSerializer_ResourceSet_ForComplexCollection(Type collectionType)
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            request.EnableHttpDependencyInjectionSupport(_edmModel);

            // Act
            var serializer = _serializerProvider.GetODataPayloadSerializer(collectionType, request);

            // Assert
            Assert.NotNull(serializer);
            var resourceSetSerializer = Assert.IsType<ODataResourceSetSerializer>(serializer);
            Assert.Equal(resourceSetSerializer.ODataPayloadKind, ODataPayloadKind.ResourceSet);
            Assert.Same(resourceSetSerializer.SerializerProvider, _serializerProvider);
        }

        [Theory]
        [InlineData(typeof(ODataError), typeof(ODataErrorSerializer))]
        [InlineData(typeof(Uri), typeof(ODataEntityReferenceLinkSerializer))]
        [InlineData(typeof(ODataEntityReferenceLink), typeof(ODataEntityReferenceLinkSerializer))]
        [InlineData(typeof(Uri[]), typeof(ODataEntityReferenceLinksSerializer))]
        [InlineData(typeof(List<Uri>), typeof(ODataEntityReferenceLinksSerializer))]
        [InlineData(typeof(ODataEntityReferenceLinks), typeof(ODataEntityReferenceLinksSerializer))]
        public void GetODataSerializer_Returns_ExpectedSerializerType(Type payloadType, Type expectedSerializerType)
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();

            // Act
            ODataSerializer serializer = _serializerProvider.GetODataPayloadSerializer(payloadType, request);

            // Assert
            Assert.NotNull(serializer);
            Assert.IsType(expectedSerializerType, serializer);
        }

        [Fact]
        public void GetODataSerializer_ReturnsSameSerializer_ForSameType()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            request.EnableHttpDependencyInjectionSupport(_edmModel);

            // Act
            ODataSerializer firstCallSerializer = _serializerProvider.GetODataPayloadSerializer(
                typeof(ODataResourceDeserializerTests.Supplier), request);
            ODataSerializer secondCallSerializer = _serializerProvider.GetODataPayloadSerializer(
                typeof(ODataResourceDeserializerTests.Supplier), request);

            // Assert
            Assert.Same(firstCallSerializer, secondCallSerializer);
        }

        [Fact]
        public void GetEdmTypeSerializer_ThrowsArgumentNull_EdmType()
        {
            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => _serializerProvider.GetEdmTypeSerializer(edmType: null),
                "edmType");
        }

        [Fact]
        public void GetEdmTypeSerializer_Caches_CreateEdmTypeSerializerOutput()
        {
            // Arrange
            IEdmTypeReference edmType = new Mock<IEdmTypeReference>().Object;

            // Act
            var serializer1 = _serializerProvider.GetEdmTypeSerializer(edmType);
            var serializer2 = _serializerProvider.GetEdmTypeSerializer(edmType);

            // Assert
            Assert.Same(serializer2, serializer1);
        }

        private static IEdmModel GetEnumModel()
        {
            EdmModel model = new EdmModel();

            EdmEnumType enumType = new EdmEnumType("TestModel", "TestEnum");
            enumType.AddMember(new EdmEnumMember(enumType, "FirstValue", new EdmEnumMemberValue(0)));
            enumType.AddMember(new EdmEnumMember(enumType, "FirstValue", new EdmEnumMemberValue(1)));
            model.AddElement(enumType);

            model.SetAnnotationValue(model.FindDeclaredType("TestModel.TestEnum"), new ClrTypeAnnotation(typeof(TestEnum)));

            return model;
        }

        private enum TestEnum
        {
            FirstValue,
            SecondValue
        }
    }
}
