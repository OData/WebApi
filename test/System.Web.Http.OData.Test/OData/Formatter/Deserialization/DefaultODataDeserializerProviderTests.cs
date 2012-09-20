// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.Edm;
using Microsoft.Data.OData;
using Microsoft.TestCommon;

namespace System.Web.Http.OData.Formatter.Deserialization
{
    public class DefaultODataDeserializerProviderTests
    {
        IEdmModel _edmModel = EdmTestHelpers.GetModel();

        [Fact]
        public void Constructor_Throw_ArgumentNull_edmModel()
        {
            Assert.ThrowsArgumentNull(() =>
            {
                var deserializerProvider = new DefaultODataDeserializerProvider(edmModel: null);
            }, "edmModel");
        }

        [Fact]
        public void Constructor()
        {
            var deserializerProvider = new DefaultODataDeserializerProvider(_edmModel);

            Assert.Equal(_edmModel, deserializerProvider.EdmModel);
        }

        [Fact]
        public void GetODataDeserializer_Uri()
        {
            ODataDeserializerProvider deserializerProvider = new DefaultODataDeserializerProvider(_edmModel);
            ODataDeserializer deserializer = deserializerProvider.GetODataDeserializer(typeof(Uri));

            Assert.NotNull(deserializer);
            var referenceLinkDeserializer = Assert.IsType<ODataEntityReferenceLinkDeserializer>(deserializer);
            Assert.Equal(ODataPayloadKind.EntityReferenceLink, referenceLinkDeserializer.ODataPayloadKind);
        }

        [Theory]
        [InlineData(typeof(Int16), EdmPrimitiveTypeKind.Int16)]
        [InlineData(typeof(int), EdmPrimitiveTypeKind.Int32)]
        [InlineData(typeof(Decimal), EdmPrimitiveTypeKind.Decimal)]
        [InlineData(typeof(DateTime), EdmPrimitiveTypeKind.DateTime)]
        [InlineData(typeof(double), EdmPrimitiveTypeKind.Double)]
        [InlineData(typeof(byte[]), EdmPrimitiveTypeKind.Binary)]
        [InlineData(typeof(bool), EdmPrimitiveTypeKind.Boolean)]
        [InlineData(typeof(int?), EdmPrimitiveTypeKind.Int32)]
        public void GetODataDeserializer_Primitive(Type type, EdmPrimitiveTypeKind primitiveKind)
        {
            ODataDeserializerProvider deserializerProvider = new DefaultODataDeserializerProvider(_edmModel);
            ODataDeserializer deserializer = deserializerProvider.GetODataDeserializer(type);

            Assert.NotNull(deserializer);
            ODataPrimitiveDeserializer rawValueDeserializer = Assert.IsType<ODataPrimitiveDeserializer>(deserializer);
            Assert.Equal(ODataPayloadKind.Property, rawValueDeserializer.ODataPayloadKind);
            Assert.Equal(primitiveKind, rawValueDeserializer.EdmType.AsPrimitive().PrimitiveDefinition().PrimitiveKind);
        }

        [Fact]
        public void GetODataDeserializer_Entity()
        {
            ODataDeserializerProvider deserializerProvider = new DefaultODataDeserializerProvider(_edmModel);
            ODataDeserializer deserializer = deserializerProvider.GetODataDeserializer(typeof(ODataEntityDeserializerTests.Product));

            Assert.NotNull(deserializer);
            ODataEntityDeserializer entityDeserializer = Assert.IsType<ODataEntityDeserializer>(deserializer);
            Assert.Equal(deserializer.ODataPayloadKind, ODataPayloadKind.Entry);
            Assert.Equal(entityDeserializer.DeserializerProvider, deserializerProvider);
            Assert.True(entityDeserializer.EdmEntityType.IsEquivalentTo(_edmModel.GetEdmTypeReference(typeof(ODataEntityDeserializerTests.Product))));
            Assert.Equal(entityDeserializer.EdmModel, _edmModel);
            Assert.Equal(entityDeserializer.EdmType, entityDeserializer.EdmEntityType);
        }

        [Fact]
        public void GetODataDeserializer_Complex()
        {
            ODataDeserializerProvider deserializerProvider = new DefaultODataDeserializerProvider(_edmModel);
            ODataDeserializer deserializer = deserializerProvider.GetODataDeserializer(typeof(ODataEntityDeserializerTests.Address));

            Assert.NotNull(deserializer);
            ODataComplexTypeDeserializer complexDeserializer = Assert.IsType<ODataComplexTypeDeserializer>(deserializer);
            Assert.Equal(deserializer.ODataPayloadKind, ODataPayloadKind.Property);
            Assert.Equal(complexDeserializer.DeserializerProvider, deserializerProvider);
            Assert.True(complexDeserializer.EdmComplexType.IsEquivalentTo(_edmModel.GetEdmTypeReference(typeof(ODataEntityDeserializerTests.Address))));
            Assert.Equal(complexDeserializer.EdmModel, _edmModel);
            Assert.Equal(complexDeserializer.EdmType, complexDeserializer.EdmComplexType);
        }

        [Fact]
        public void GetODataSerializer_ReturnsSameSerializer_ForSameType()
        {
            ODataDeserializerProvider deserializerProvider = new DefaultODataDeserializerProvider(_edmModel);

            ODataDeserializer firstCallDeserializer = deserializerProvider.GetODataDeserializer(typeof(ODataEntityDeserializerTests.Supplier));
            ODataDeserializer secondCallDeserializer = deserializerProvider.GetODataDeserializer(typeof(ODataEntityDeserializerTests.Supplier));

            Assert.Same(firstCallDeserializer, secondCallDeserializer);
        }

        [Fact]
        public void GetODataSerializer_ActionPayload()
        {
            ODataDeserializerProvider deserializerProvider = new DefaultODataDeserializerProvider(_edmModel);
            ODataActionPayloadDeserializer basicActionPayload = deserializerProvider.GetODataDeserializer(typeof(ODataActionParameters)) as ODataActionPayloadDeserializer;

            Assert.NotNull(basicActionPayload);
        }

        [Fact]
        public void GetODataSerializer_Derived_ActionPayload()
        {
            ODataDeserializerProvider deserializerProvider = new DefaultODataDeserializerProvider(_edmModel);
            ODataActionPayloadDeserializer derivedActionPayload = deserializerProvider.GetODataDeserializer(typeof(MyActionPayload)) as ODataActionPayloadDeserializer;

            Assert.NotNull(derivedActionPayload);
        }

        public class MyActionPayload : ODataActionParameters
        { 
        }
    }
}
