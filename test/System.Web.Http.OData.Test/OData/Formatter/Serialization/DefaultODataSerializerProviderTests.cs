// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Web.Http.OData.Extensions;
using System.Web.Http.OData.Formatter.Deserialization;
using System.Web.Http.OData.Routing;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Library;
using Microsoft.Data.OData;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.OData.Formatter.Serialization
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
                    { typeof(DateTime), EdmPrimitiveTypeKind.DateTime },
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
                    { typeof(TimeSpan), EdmPrimitiveTypeKind.Time },
                    { typeof(TestEnum), EdmPrimitiveTypeKind.String },
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

        private enum TestEnum
        {
            FirstValue,
            SecondValue
        }
    }
}
