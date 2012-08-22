// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Web.Http.OData.Formatter.Deserialization;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Library;
using Microsoft.Data.OData;
using Microsoft.TestCommon;

namespace System.Web.Http.OData.Formatter.Serialization
{
    public class DefaultODataSerializerProviderTests
    {
        private IEdmModel _edmModel = EdmTestHelpers.GetModel();

        [Fact]
        public void Constructor_ThrowsArgumentNull_edmModel()
        {
            Assert.ThrowsArgumentNull(() =>
            {
                var serializerProvider = new DefaultODataSerializerProvider(edmModel: null);
            }, "edmModel");
        }

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

        [Theory]
        [PropertyData("EdmPrimitiveMappingData")]
        public void GetODataSerializer_Primitive(Type type, EdmPrimitiveTypeKind edmPrimitiveTypeKind)
        {
            var serializerProvider = new DefaultODataSerializerProvider(_edmModel);
            var serializer = serializerProvider.GetODataPayloadSerializer(type);

            Assert.NotNull(serializer);
            var primitiveSerializer = Assert.IsType<ODataPrimitiveSerializer>(serializer);
            Assert.Equal(primitiveSerializer.EdmType.AsPrimitive().PrimitiveKind(), edmPrimitiveTypeKind);
            Assert.Equal(primitiveSerializer.ODataPayloadKind, ODataPayloadKind.Property);
        }

        [Fact]
        public void GetODataSerializer_Entity()
        {
            var serializerProvider = new DefaultODataSerializerProvider(_edmModel);
            var serializer = serializerProvider.GetODataPayloadSerializer(typeof(ODataEntityDeserializerTests.Product));

            Assert.NotNull(serializer);
            var entitySerializer = Assert.IsType<ODataEntityTypeSerializer>(serializer);
            Assert.True(entitySerializer.EdmType.IsEquivalentTo(_edmModel.GetEdmTypeReference(typeof(ODataEntityDeserializerTests.Product))));
            Assert.Equal(entitySerializer.SerializerProvider, serializerProvider);
            Assert.Equal(entitySerializer.ODataPayloadKind, ODataPayloadKind.Entry);
        }

        [Fact]
        public void GetODataSerializer_Complex()
        {
            var serializerProvider = new DefaultODataSerializerProvider(_edmModel);
            var serializer = serializerProvider.GetODataPayloadSerializer(typeof(ODataEntityDeserializerTests.Address));

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
        [InlineData(typeof(ODataResult<ODataEntityDeserializerTests.Supplier>))]
        public void GetODataSerializer_Feed(Type collectionType)
        {
            var serializerProvider = new DefaultODataSerializerProvider(_edmModel);
            var serializer = serializerProvider.GetODataPayloadSerializer(collectionType);

            Assert.NotNull(serializer);
            var feedSerializer = Assert.IsType<ODataFeedSerializer>(serializer);
            Assert.True(feedSerializer.EdmType.IsCollection());
            Assert.True(feedSerializer.EdmType.AsCollection().ElementType().IsEquivalentTo(_edmModel.GetEdmTypeReference(typeof(ODataEntityDeserializerTests.Supplier))));
        }

        [Fact]
        public void GetODataSerializer_ComplexCollection()
        {
            var serializerProvider = new DefaultODataSerializerProvider(_edmModel);
            var serializer = serializerProvider.GetODataPayloadSerializer(typeof(IEnumerable<ODataEntityDeserializerTests.Address>));

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

        [Fact]
        public void GetODataSerializer_Enum()
        {
            var serializerProvider = new DefaultODataSerializerProvider(_edmModel);
            IEdmTypeReference edmEnumType = new EdmEnumTypeReference(new EdmEnumType("ODataDemo", "SupplierRating"), isNullable: false);
            var serializer = serializerProvider.GetEdmTypeSerializer(edmEnumType);

            Assert.NotNull(serializer);
            var enumSerializer = Assert.IsType<ODataEnumSerializer>(serializer);
            Assert.Equal(enumSerializer.EdmType, edmEnumType);
            Assert.Equal(enumSerializer.ODataPayloadKind, ODataPayloadKind.Property);
            Assert.Equal(enumSerializer.SerializerProvider, serializerProvider);
        }

        [Fact]
        public void GetODataSerializer_ODataError()
        {
            ODataSerializerProvider serializerProvider = new DefaultODataSerializerProvider(_edmModel);

            ODataSerializer serializer = serializerProvider.GetODataPayloadSerializer(typeof(ODataError));
            Assert.NotNull(serializer);
            Assert.Equal(typeof(ODataErrorSerializer), serializer.GetType());
            Assert.Equal(ODataPayloadKind.Error, serializer.ODataPayloadKind);
        }

        [Fact]
        public void GetODataSerializer_ReturnsSameSerializer_ForSameType()
        {
            ODataSerializerProvider serializerProvider = new DefaultODataSerializerProvider(_edmModel);

            ODataSerializer firstCallSerializer = serializerProvider.GetODataPayloadSerializer(typeof(ODataEntityDeserializerTests.Supplier));
            ODataSerializer secondCallSerializer = serializerProvider.GetODataPayloadSerializer(typeof(ODataEntityDeserializerTests.Supplier));

            Assert.Same(firstCallSerializer, secondCallSerializer);
        }
    }
}
