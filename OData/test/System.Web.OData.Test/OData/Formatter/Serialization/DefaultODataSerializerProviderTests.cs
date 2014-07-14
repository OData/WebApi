// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Web.OData.Builder.TestModels;
using System.Web.OData.Extensions;
using System.Web.OData.Formatter.Deserialization;
using System.Web.OData.Routing;
using Microsoft.OData.Core;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;
using Microsoft.OData.Edm.Library.Values;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.OData.Formatter.Serialization
{
    public class DefaultODataSerializerProviderTests
    {
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
                    // TODO 1559: Investigate how to add support for DataTime in webapi.odata, ODataLib v4 does not support it.
                    { typeof(DateTimeOffset), EdmPrimitiveTypeKind.DateTimeOffset },
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
        public void GetODataPayloadSerializer_ThrowsArgumentNull_Model()
        {
            DefaultODataSerializerProvider serializerProvider = new DefaultODataSerializerProvider();
            HttpRequestMessage request = new HttpRequestMessage();

            Assert.ThrowsArgumentNull(
                () => serializerProvider.GetODataPayloadSerializer(model: null, type: typeof(int), request: request),
               "model");
        }

        [Fact]
        public void GetODataPayloadSerializer_ThrowsArgumentNull_Type()
        {
            DefaultODataSerializerProvider serializerProvider = new DefaultODataSerializerProvider();
            HttpRequestMessage request = new HttpRequestMessage();

            Assert.ThrowsArgumentNull(
                () => serializerProvider.GetODataPayloadSerializer(model: EdmCoreModel.Instance, type: null, request: request),
               "type");
        }

        [Fact]
        public void GetODataPayloadSerializer_ThrowsArgumentNull_Request()
        {
            DefaultODataSerializerProvider serializerProvider = new DefaultODataSerializerProvider();
            HttpRequestMessage request = new HttpRequestMessage();

            Assert.ThrowsArgumentNull(
                () => serializerProvider.GetODataPayloadSerializer(EdmCoreModel.Instance, typeof(int), request: null),
               "request");
        }

        [Theory]
        [PropertyData("EdmPrimitiveMappingData")]
        public void GetODataSerializer_Primitive(Type type, EdmPrimitiveTypeKind edmPrimitiveTypeKind)
        {
            var serializerProvider = new DefaultODataSerializerProvider();
            HttpRequestMessage request = new HttpRequestMessage();
            var serializer = serializerProvider.GetODataPayloadSerializer(_edmModel, type, request);

            Assert.NotNull(serializer);
            var primitiveSerializer = Assert.IsType<ODataPrimitiveSerializer>(serializer);
            Assert.Equal(primitiveSerializer.ODataPayloadKind, ODataPayloadKind.Property);
        }

        [Theory]
        [PropertyData("EdmPrimitiveMappingData")]
        public void GetODataPayloadSerializer_ReturnsRawValueSerializer_ForValueRequests(Type type, EdmPrimitiveTypeKind edmPrimitiveTypeKind)
        {
            ODataSerializerProvider serializerProvider = new DefaultODataSerializerProvider();
            HttpRequestMessage request = new HttpRequestMessage();
            request.ODataProperties().Path = new ODataPath(new ValuePathSegment());

            var serializer = serializerProvider.GetODataPayloadSerializer(_edmModel, type, request);

            Assert.NotNull(serializer);
            Assert.Equal(ODataPayloadKind.Value, serializer.ODataPayloadKind);
        }

        [Fact]
        public void GetODataSerializer_Enum()
        {
            var serializerProvider = new DefaultODataSerializerProvider();
            HttpRequestMessage request = new HttpRequestMessage();
            var serializer = serializerProvider.GetODataPayloadSerializer(GetEnumModel(), typeof(TestEnum), request);

            Assert.NotNull(serializer);
            var enumSerializer = Assert.IsType<ODataEnumSerializer>(serializer);
            Assert.Equal(ODataPayloadKind.Property, enumSerializer.ODataPayloadKind);
        }

        [Fact]
        public void GetODataPayloadSerializer_ReturnsRawValueSerializer_ForEnumValueRequests()
        {
            ODataSerializerProvider serializerProvider = new DefaultODataSerializerProvider();
            HttpRequestMessage request = new HttpRequestMessage();
            request.ODataProperties().Path = new ODataPath(new ValuePathSegment());

            var serializer = serializerProvider.GetODataPayloadSerializer(GetEnumModel(), typeof(TestEnum), request);

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
            ODataSerializerProvider serializerProvider = new DefaultODataSerializerProvider();

            // Act
            var serializer = serializerProvider.GetODataPayloadSerializer(model, type, request);

            // Assert
            Assert.NotNull(serializer);
            var rawValueSerializer = Assert.IsType<ODataRawValueSerializer>(serializer);
            Assert.Equal(ODataPayloadKind.Value, rawValueSerializer.ODataPayloadKind);
        }

        [Fact]
        public void GetODataSerializer_Entity()
        {
            var serializerProvider = new DefaultODataSerializerProvider();
            HttpRequestMessage request = new HttpRequestMessage();

            var serializer = serializerProvider.GetODataPayloadSerializer(_edmModel, typeof(ODataEntityDeserializerTests.Product), request);

            Assert.NotNull(serializer);
            var entitySerializer = Assert.IsType<ODataEntityTypeSerializer>(serializer);
            Assert.Equal(entitySerializer.SerializerProvider, serializerProvider);
            Assert.Equal(entitySerializer.ODataPayloadKind, ODataPayloadKind.Entry);
        }

        [Fact]
        public void GetODataSerializer_Complex()
        {
            var serializerProvider = new DefaultODataSerializerProvider();
            HttpRequestMessage request = new HttpRequestMessage();
            var serializer = serializerProvider.GetODataPayloadSerializer(_edmModel, typeof(ODataEntityDeserializerTests.Address), request);

            Assert.NotNull(serializer);
            var complexSerializer = Assert.IsType<ODataComplexTypeSerializer>(serializer);
            Assert.Equal(complexSerializer.SerializerProvider, serializerProvider);
            Assert.Equal(complexSerializer.ODataPayloadKind, ODataPayloadKind.Property);
        }

        [Theory]
        [InlineData(typeof(ODataEntityDeserializerTests.Supplier[]))]
        [InlineData(typeof(IEnumerable<ODataEntityDeserializerTests.Supplier>))]
        [InlineData(typeof(ICollection<ODataEntityDeserializerTests.Supplier>))]
        [InlineData(typeof(IList<ODataEntityDeserializerTests.Supplier>))]
        [InlineData(typeof(List<ODataEntityDeserializerTests.Supplier>))]
        [InlineData(typeof(PageResult<ODataEntityDeserializerTests.Supplier>))]
        public void GetODataSerializer_Feed(Type collectionType)
        {
            var serializerProvider = new DefaultODataSerializerProvider();
            HttpRequestMessage request = new HttpRequestMessage();

            var serializer = serializerProvider.GetODataPayloadSerializer(_edmModel, collectionType, request);

            Assert.NotNull(serializer);
            var feedSerializer = Assert.IsType<ODataFeedSerializer>(serializer);
            Assert.Equal(feedSerializer.ODataPayloadKind, ODataPayloadKind.Feed);
        }

        [Fact]
        public void GetODataSerializer_ComplexCollection()
        {
            var serializerProvider = new DefaultODataSerializerProvider();
            HttpRequestMessage request = new HttpRequestMessage();

            var serializer = serializerProvider.GetODataPayloadSerializer(_edmModel,
                typeof(IEnumerable<ODataEntityDeserializerTests.Address>), request);

            Assert.NotNull(serializer);
            var collectionSerializer = Assert.IsType<ODataCollectionSerializer>(serializer);
            Assert.Equal(collectionSerializer.ODataPayloadKind, ODataPayloadKind.Collection);
            Assert.Equal(collectionSerializer.SerializerProvider, serializerProvider);
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
            ODataSerializerProvider serializerProvider = new DefaultODataSerializerProvider();
            HttpRequestMessage request = new HttpRequestMessage();

            ODataSerializer serializer = serializerProvider.GetODataPayloadSerializer(_edmModel, payloadType, request);

            Assert.NotNull(serializer);
            Assert.IsType(expectedSerializerType, serializer);
        }

        [Fact]
        public void GetODataSerializer_ReturnsSameSerializer_ForSameType()
        {
            ODataSerializerProvider serializerProvider = new DefaultODataSerializerProvider();
            HttpRequestMessage request = new HttpRequestMessage();

            ODataSerializer firstCallSerializer = serializerProvider.GetODataPayloadSerializer(_edmModel,
                typeof(ODataEntityDeserializerTests.Supplier), request);
            ODataSerializer secondCallSerializer = serializerProvider.GetODataPayloadSerializer(_edmModel,
                typeof(ODataEntityDeserializerTests.Supplier), request);

            Assert.Same(firstCallSerializer, secondCallSerializer);
        }

        [Fact]
        public void GetEdmTypeSerializer_ThrowsArgumentNull_EdmType()
        {
            DefaultODataSerializerProvider serializerProvider = new DefaultODataSerializerProvider();

            Assert.ThrowsArgumentNull(
                () => serializerProvider.GetEdmTypeSerializer(edmType: null),
                "edmType");
        }

        [Fact]
        public void GetEdmTypeSerializer_Caches_CreateEdmTypeSerializerOutput()
        {
            // Arrange
            DefaultODataSerializerProvider serializerProvider = new DefaultODataSerializerProvider();
            IEdmTypeReference edmType = new Mock<IEdmTypeReference>().Object;

            // Act
            var serializer1 = serializerProvider.GetEdmTypeSerializer(edmType);
            var serializer2 = serializerProvider.GetEdmTypeSerializer(edmType);

            // Assert
            Assert.Same(serializer2, serializer1);
        }

        [Fact]
        public void Property_Instance_IsCached()
        {
            DefaultODataSerializerProvider instance1 = DefaultODataSerializerProvider.Instance;
            DefaultODataSerializerProvider instance2 = DefaultODataSerializerProvider.Instance;

            Assert.Same(instance1, instance2);
        }

        private static IEdmModel GetEnumModel()
        {
            EdmModel model = new EdmModel();

            EdmEnumType enumType = new EdmEnumType("TestModel", "TestEnum");
            enumType.AddMember(new EdmEnumMember(enumType, "FirstValue", new EdmIntegerConstant(0)));
            enumType.AddMember(new EdmEnumMember(enumType, "FirstValue", new EdmIntegerConstant(1)));
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
