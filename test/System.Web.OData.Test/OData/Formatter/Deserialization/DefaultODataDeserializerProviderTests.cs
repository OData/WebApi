// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using Microsoft.OData.Core;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.OData.Formatter.Deserialization
{
    public class DefaultODataDeserializerProviderTests
    {
        IEdmModel _edmModel = EdmTestHelpers.GetModel();

        [Fact]
        public void GetODataDeserializer_Uri()
        {
            ODataDeserializerProvider deserializerProvider = new DefaultODataDeserializerProvider();
            HttpRequestMessage request = new HttpRequestMessage();

            ODataDeserializer deserializer = deserializerProvider.GetODataDeserializer(_edmModel, typeof(Uri), request);

            Assert.NotNull(deserializer);
            var referenceLinkDeserializer = Assert.IsType<ODataEntityReferenceLinkDeserializer>(deserializer);
            Assert.Equal(ODataPayloadKind.EntityReferenceLink, referenceLinkDeserializer.ODataPayloadKind);
        }

        [Theory]
        [InlineData(typeof(Int16), EdmPrimitiveTypeKind.Int16)]
        [InlineData(typeof(int), EdmPrimitiveTypeKind.Int32)]
        [InlineData(typeof(Decimal), EdmPrimitiveTypeKind.Decimal)]
        [InlineData(typeof(DateTimeOffset), EdmPrimitiveTypeKind.DateTimeOffset)]
        [InlineData(typeof(double), EdmPrimitiveTypeKind.Double)]
        [InlineData(typeof(byte[]), EdmPrimitiveTypeKind.Binary)]
        [InlineData(typeof(bool), EdmPrimitiveTypeKind.Boolean)]
        [InlineData(typeof(int?), EdmPrimitiveTypeKind.Int32)]
        public void GetODataDeserializer_Primitive(Type type, EdmPrimitiveTypeKind primitiveKind)
        {
            ODataDeserializerProvider deserializerProvider = new DefaultODataDeserializerProvider();
            HttpRequestMessage request = new HttpRequestMessage();

            ODataDeserializer deserializer = deserializerProvider.GetODataDeserializer(_edmModel, type, request);

            Assert.NotNull(deserializer);
            ODataPrimitiveDeserializer rawValueDeserializer = Assert.IsType<ODataPrimitiveDeserializer>(deserializer);
            Assert.Equal(ODataPayloadKind.Property, rawValueDeserializer.ODataPayloadKind);
        }

        [Fact]
        public void GetODataDeserializer_Entity()
        {
            ODataDeserializerProvider deserializerProvider = new DefaultODataDeserializerProvider();
            HttpRequestMessage request = new HttpRequestMessage();

            ODataDeserializer deserializer = deserializerProvider.GetODataDeserializer(_edmModel,
                typeof(ODataEntityDeserializerTests.Product), request);

            Assert.NotNull(deserializer);
            ODataEntityDeserializer entityDeserializer = Assert.IsType<ODataEntityDeserializer>(deserializer);
            Assert.Equal(deserializer.ODataPayloadKind, ODataPayloadKind.Entry);
            Assert.Equal(entityDeserializer.DeserializerProvider, deserializerProvider);
        }

        [Fact]
        public void GetODataDeserializer_Complex()
        {
            ODataDeserializerProvider deserializerProvider = new DefaultODataDeserializerProvider();
            HttpRequestMessage request = new HttpRequestMessage();

            ODataDeserializer deserializer = deserializerProvider.GetODataDeserializer(_edmModel,
                typeof(ODataEntityDeserializerTests.Address), request);

            Assert.NotNull(deserializer);
            ODataComplexTypeDeserializer complexDeserializer = Assert.IsType<ODataComplexTypeDeserializer>(deserializer);
            Assert.Equal(deserializer.ODataPayloadKind, ODataPayloadKind.Property);
            Assert.Equal(complexDeserializer.DeserializerProvider, deserializerProvider);
        }

        [Fact]
        public void GetODataDeserializer_ReturnsSameDeserializer_ForSameType()
        {
            ODataDeserializerProvider deserializerProvider = new DefaultODataDeserializerProvider();
            HttpRequestMessage request = new HttpRequestMessage();

            ODataDeserializer firstCallDeserializer = deserializerProvider.GetODataDeserializer(_edmModel,
                typeof(ODataEntityDeserializerTests.Supplier), request);
            ODataDeserializer secondCallDeserializer = deserializerProvider.GetODataDeserializer(_edmModel,
                typeof(ODataEntityDeserializerTests.Supplier), request);

            Assert.Same(firstCallDeserializer, secondCallDeserializer);
        }

        [Theory]
        [InlineData(typeof(ODataActionParameters))]
        [InlineData(typeof(ODataUntypedActionParameters))]
        public void GetODataDeserializer_ActionPayload(Type resourceType)
        {
            ODataDeserializerProvider deserializerProvider = new DefaultODataDeserializerProvider();
            HttpRequestMessage request = new HttpRequestMessage();

            ODataActionPayloadDeserializer basicActionPayload = deserializerProvider.GetODataDeserializer(_edmModel,
                resourceType, request) as ODataActionPayloadDeserializer;

            Assert.NotNull(basicActionPayload);
        }

        [Fact]
        public void GetODataDeserializer_Throws_ArgumentNullForModel()
        {
            DefaultODataDeserializerProvider deserializerProvider = new DefaultODataDeserializerProvider();
            HttpRequestMessage request = new HttpRequestMessage();

            Assert.ThrowsArgumentNull(
                () => deserializerProvider.GetODataDeserializer(model: null, type: typeof(int), request: request),
                "model");
        }

        [Fact]
        public void GetODataDeserializer_Throws_ArgumentNullForType()
        {
            DefaultODataDeserializerProvider deserializerProvider = new DefaultODataDeserializerProvider();
            HttpRequestMessage request = new HttpRequestMessage();

            Assert.ThrowsArgumentNull(
                () => deserializerProvider.GetODataDeserializer(model: EdmCoreModel.Instance, type: null, request: request),
                "type");
        }

        [Fact]
        public void GetEdmTypeDeserializer_ThrowsArgument_EdmType()
        {
            DefaultODataDeserializerProvider deserializerProvider = new DefaultODataDeserializerProvider();

            Assert.ThrowsArgumentNull(
                () => deserializerProvider.GetEdmTypeDeserializer(edmType: null),
                "edmType");
        }

        [Fact]
        public void GetEdmTypeDeserializer_Caches_CreateDeserializerOutput()
        {
            // Arrange
            DefaultODataDeserializerProvider deserializerProvider = new DefaultODataDeserializerProvider();
            IEdmTypeReference edmType = new Mock<IEdmTypeReference>().Object;

            // Act
            var deserializer1 = deserializerProvider.GetEdmTypeDeserializer(edmType);
            var deserializer2 = deserializerProvider.GetEdmTypeDeserializer(edmType);

            // Assert
            Assert.Same(deserializer1, deserializer2);
        }

        [Fact]
        public void Property_Instance_IsCached()
        {
            DefaultODataDeserializerProvider instance1 = DefaultODataDeserializerProvider.Instance;
            DefaultODataDeserializerProvider instance2 = DefaultODataDeserializerProvider.Instance;

            Assert.Same(instance1, instance2);
        }

        public class MyActionPayload : ODataActionParameters
        {
        }
    }
}
