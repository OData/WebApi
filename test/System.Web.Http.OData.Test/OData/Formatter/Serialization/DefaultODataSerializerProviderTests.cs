// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Web.Http.OData.Formatter.Deserialization;
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
                    { typeof(TimeSpan), EdmPrimitiveTypeKind.Time }
                };
            }
        }

        [Fact]
        public void CreateEdmTypeSerializer_ThrowsArgumentNull_EdmType()
        {
            DefaultODataSerializerProvider serializerProvider = new DefaultODataSerializerProvider();

            Assert.ThrowsArgumentNull(
                () => serializerProvider.CreateEdmTypeSerializer(edmType: null),
                "edmType");
        }

        [Fact]
        public void CreateEdmTypeSerializer_Returns_Null_ForUnsupportedType()
        {
            // Arrange
            Mock<IEdmType> unsupportedEdmType = new Mock<IEdmType>();
            unsupportedEdmType.Setup(e => e.TypeKind).Returns(EdmTypeKind.None);
            Mock<IEdmTypeReference> unsupportedEdmTypeReference = new Mock<IEdmTypeReference>();
            unsupportedEdmTypeReference.Setup(e => e.Definition).Returns(unsupportedEdmType.Object);
            DefaultODataSerializerProvider serializerProvider = new DefaultODataSerializerProvider();

            // Act & Assert
            Assert.Null(serializerProvider.CreateEdmTypeSerializer(unsupportedEdmTypeReference.Object));
        }

        [Fact]
        public void GetODataPayloadSerializer_ThrowsArgumentNull_Model()
        {
            DefaultODataSerializerProvider serializerProvider = new DefaultODataSerializerProvider();

            Assert.ThrowsArgumentNull(
                () => serializerProvider.GetODataPayloadSerializer(model: null, type: null),
               "model");
        }

        [Fact]
        public void GetODataPayloadSerializer_ThrowsArgumentNull_Type()
        {
            DefaultODataSerializerProvider serializerProvider = new DefaultODataSerializerProvider();

            Assert.ThrowsArgumentNull(
                () => serializerProvider.GetODataPayloadSerializer(model: EdmCoreModel.Instance, type: null),
               "type");
        }

        [Theory]
        [PropertyData("EdmPrimitiveMappingData")]
        public void GetODataSerializer_Primitive(Type type, EdmPrimitiveTypeKind edmPrimitiveTypeKind)
        {
            var serializerProvider = new DefaultODataSerializerProvider();
            var serializer = serializerProvider.GetODataPayloadSerializer(_edmModel, type);

            Assert.NotNull(serializer);
            var primitiveSerializer = Assert.IsType<ODataPrimitiveSerializer>(serializer);
            Assert.Equal(primitiveSerializer.EdmType.AsPrimitive().PrimitiveKind(), edmPrimitiveTypeKind);
            Assert.Equal(primitiveSerializer.ODataPayloadKind, ODataPayloadKind.Property);
        }

        [Fact]
        public void GetODataSerializer_Entity()
        {
            var serializerProvider = new DefaultODataSerializerProvider();
            var serializer = serializerProvider.GetODataPayloadSerializer(_edmModel, typeof(ODataEntityDeserializerTests.Product));

            Assert.NotNull(serializer);
            var entitySerializer = Assert.IsType<ODataEntityTypeSerializer>(serializer);
            Assert.True(entitySerializer.EdmType.IsEquivalentTo(_edmModel.GetEdmTypeReference(typeof(ODataEntityDeserializerTests.Product))));
            Assert.Equal(entitySerializer.SerializerProvider, serializerProvider);
            Assert.Equal(entitySerializer.ODataPayloadKind, ODataPayloadKind.Entry);
        }

        [Fact]
        public void GetODataSerializer_Complex()
        {
            var serializerProvider = new DefaultODataSerializerProvider();
            var serializer = serializerProvider.GetODataPayloadSerializer(_edmModel, typeof(ODataEntityDeserializerTests.Address));

            Assert.NotNull(serializer);
            var complexSerializer = Assert.IsType<ODataComplexTypeSerializer>(serializer);
            Assert.True(complexSerializer.EdmType.IsEquivalentTo(_edmModel.GetEdmTypeReference(typeof(ODataEntityDeserializerTests.Address))));
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
            var serializer = serializerProvider.GetODataPayloadSerializer(_edmModel, collectionType);

            Assert.NotNull(serializer);
            var feedSerializer = Assert.IsType<ODataFeedSerializer>(serializer);
            Assert.True(feedSerializer.EdmType.IsCollection());
            Assert.True(feedSerializer.EdmType.AsCollection().ElementType().IsEquivalentTo(_edmModel.GetEdmTypeReference(typeof(ODataEntityDeserializerTests.Supplier))));
        }

        [Fact]
        public void GetODataSerializer_ComplexCollection()
        {
            var serializerProvider = new DefaultODataSerializerProvider();
            var serializer = serializerProvider.GetODataPayloadSerializer(_edmModel, typeof(IEnumerable<ODataEntityDeserializerTests.Address>));

            Assert.NotNull(serializer);
            var collectionSerializer = Assert.IsType<ODataCollectionSerializer>(serializer);
            Assert.True(collectionSerializer.EdmType.IsCollection());
            Assert.True(collectionSerializer
                .EdmType.AsCollection()
                .ElementType()
                .IsEquivalentTo(_edmModel.GetEdmTypeReference(typeof(ODataEntityDeserializerTests.Address))));
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

            ODataSerializer serializer = serializerProvider.GetODataPayloadSerializer(_edmModel, payloadType);

            Assert.NotNull(serializer);
            Assert.IsType(expectedSerializerType, serializer);
        }

        [Fact]
        public void GetODataSerializer_ReturnsSameSerializer_ForSameType()
        {
            ODataSerializerProvider serializerProvider = new DefaultODataSerializerProvider();

            ODataSerializer firstCallSerializer = serializerProvider.GetODataPayloadSerializer(_edmModel, typeof(ODataEntityDeserializerTests.Supplier));
            ODataSerializer secondCallSerializer = serializerProvider.GetODataPayloadSerializer(_edmModel, typeof(ODataEntityDeserializerTests.Supplier));

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
        public void GetODataSerializer_Calls_CreateSerializer_ForAnEdmType()
        {
            // Arrange
            Mock<DefaultODataSerializerProvider> serializerProvider = new Mock<DefaultODataSerializerProvider>() { CallBase = true };
            IEdmTypeReference edmType = new Mock<IEdmTypeReference>().Object;
            serializerProvider.Setup(d => d.CreateEdmTypeSerializer(edmType)).Verifiable();

            // Act
            serializerProvider.Object.GetEdmTypeSerializer(edmType);

            // Assert
            serializerProvider.Verify();
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
        public void GetODataSerializer_ThrowsArgumentNull_EdmType()
        {
            // Arrange
            DefaultODataSerializerProvider serializerProvider = new DefaultODataSerializerProvider();

            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => serializerProvider.SetEdmTypeSerializer(edmType: null, serializer: null),
                "edmType");
        }

        [Fact]
        public void GetEdmTypeSerializer_Returns_SetEdmTypeSerializerInput()
        {
            // Arrange
            DefaultODataSerializerProvider serializerProvider = new DefaultODataSerializerProvider();
            IEdmTypeReference edmType = EdmCoreModel.Instance.GetInt32(isNullable: true);
            ODataEdmTypeSerializer serializer = new Mock<ODataEdmTypeSerializer>(edmType, ODataPayloadKind.Property).Object;
            serializerProvider.SetEdmTypeSerializer(edmType, serializer);

            // Act & Assert
            Assert.Same(serializer, serializerProvider.GetEdmTypeSerializer(edmType));
        }

        [Fact]
        public void Property_Instance_IsCached()
        {
            DefaultODataSerializerProvider instance1 = DefaultODataSerializerProvider.Instance;
            DefaultODataSerializerProvider instance2 = DefaultODataSerializerProvider.Instance;

            Assert.Same(instance1, instance2);
        }
    }
}
