// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Library;
using Microsoft.Data.OData;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.OData.Formatter.Deserialization
{
    public class DefaultODataDeserializerProviderTests
    {
        IEdmModel _edmModel = EdmTestHelpers.GetModel();

        [Fact]
        public void GetODataDeserializer_Uri()
        {
            ODataDeserializerProvider deserializerProvider = new DefaultODataDeserializerProvider();
            ODataDeserializer deserializer = deserializerProvider.GetODataDeserializer(_edmModel, typeof(Uri));

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
            ODataDeserializerProvider deserializerProvider = new DefaultODataDeserializerProvider();
            ODataDeserializer deserializer = deserializerProvider.GetODataDeserializer(_edmModel, type);

            Assert.NotNull(deserializer);
            ODataPrimitiveDeserializer rawValueDeserializer = Assert.IsType<ODataPrimitiveDeserializer>(deserializer);
            Assert.Equal(ODataPayloadKind.Property, rawValueDeserializer.ODataPayloadKind);
            Assert.Equal(primitiveKind, rawValueDeserializer.EdmType.AsPrimitive().PrimitiveDefinition().PrimitiveKind);
        }

        [Fact]
        public void GetODataDeserializer_Entity()
        {
            ODataDeserializerProvider deserializerProvider = new DefaultODataDeserializerProvider();
            ODataDeserializer deserializer = deserializerProvider.GetODataDeserializer(_edmModel, typeof(ODataEntityDeserializerTests.Product));

            Assert.NotNull(deserializer);
            ODataEntityDeserializer entityDeserializer = Assert.IsType<ODataEntityDeserializer>(deserializer);
            Assert.Equal(deserializer.ODataPayloadKind, ODataPayloadKind.Entry);
            Assert.Equal(entityDeserializer.DeserializerProvider, deserializerProvider);
            Assert.True(entityDeserializer.EntityType.IsEquivalentTo(_edmModel.GetEdmTypeReference(typeof(ODataEntityDeserializerTests.Product))));
            Assert.Equal(entityDeserializer.EdmType, entityDeserializer.EntityType);
        }

        [Fact]
        public void GetODataDeserializer_Complex()
        {
            ODataDeserializerProvider deserializerProvider = new DefaultODataDeserializerProvider();
            ODataDeserializer deserializer = deserializerProvider.GetODataDeserializer(_edmModel, typeof(ODataEntityDeserializerTests.Address));

            Assert.NotNull(deserializer);
            ODataComplexTypeDeserializer complexDeserializer = Assert.IsType<ODataComplexTypeDeserializer>(deserializer);
            Assert.Equal(deserializer.ODataPayloadKind, ODataPayloadKind.Property);
            Assert.Equal(complexDeserializer.DeserializerProvider, deserializerProvider);
            Assert.True(complexDeserializer.ComplexType.IsEquivalentTo(_edmModel.GetEdmTypeReference(typeof(ODataEntityDeserializerTests.Address))));
            Assert.Equal(complexDeserializer.EdmType, complexDeserializer.ComplexType);
        }

        [Fact]
        public void GetODataDeserializer_ReturnsSameDeserializer_ForSameType()
        {
            ODataDeserializerProvider deserializerProvider = new DefaultODataDeserializerProvider();

            ODataDeserializer firstCallDeserializer = deserializerProvider.GetODataDeserializer(_edmModel, typeof(ODataEntityDeserializerTests.Supplier));
            ODataDeserializer secondCallDeserializer = deserializerProvider.GetODataDeserializer(_edmModel, typeof(ODataEntityDeserializerTests.Supplier));

            Assert.Same(firstCallDeserializer, secondCallDeserializer);
        }

        [Fact]
        public void GetODataDeserializer_ActionPayload()
        {
            ODataDeserializerProvider deserializerProvider = new DefaultODataDeserializerProvider();
            ODataActionPayloadDeserializer basicActionPayload = deserializerProvider.GetODataDeserializer(_edmModel, typeof(ODataActionParameters)) as ODataActionPayloadDeserializer;

            Assert.NotNull(basicActionPayload);
        }

        [Fact]
        public void CreateEdmTypeDeserializer_Throws_ArgumentNullForEdmType()
        {
            DefaultODataDeserializerProvider deserializerProvider = new DefaultODataDeserializerProvider();

            Assert.ThrowsArgumentNull(
                () => deserializerProvider.CreateEdmTypeDeserializer(edmType: null),
                "edmType");
        }

        [Fact]
        public void CreateEdmTypeDeserializer_Returns_Null_ForUnsupportedType()
        {
            // Arrange
            Mock<IEdmType> unsupportedEdmType = new Mock<IEdmType>();
            unsupportedEdmType.Setup(e => e.TypeKind).Returns(EdmTypeKind.None);
            Mock<IEdmTypeReference> unsupportedEdmTypeReference = new Mock<IEdmTypeReference>();
            unsupportedEdmTypeReference.Setup(e => e.Definition).Returns(unsupportedEdmType.Object);
            DefaultODataDeserializerProvider deserializerProvider = new DefaultODataDeserializerProvider();

            // Act & Assert
            Assert.Null(deserializerProvider.CreateEdmTypeDeserializer(unsupportedEdmTypeReference.Object));
        }

        [Fact]
        public void GetODataDeserializer_Throws_ArgumentNullForModel()
        {
            DefaultODataDeserializerProvider deserializerProvider = new DefaultODataDeserializerProvider();

            Assert.ThrowsArgumentNull(
                () => deserializerProvider.GetODataDeserializer(model: null, type: typeof(int)),
                "model");
        }

        [Fact]
        public void GetODataDeserializer_Throws_ArgumentNullForType()
        {
            DefaultODataDeserializerProvider deserializerProvider = new DefaultODataDeserializerProvider();

            Assert.ThrowsArgumentNull(
                () => deserializerProvider.GetODataDeserializer(model: EdmCoreModel.Instance, type: null),
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
        public void GetEdmTypeDeserializer_Calls_CreateDeserializer_ForAnEdmType()
        {
            // Arrange
            Mock<DefaultODataDeserializerProvider> deserializerProvider = new Mock<DefaultODataDeserializerProvider> { CallBase = true };
            IEdmTypeReference edmType = new Mock<IEdmTypeReference>().Object;
            deserializerProvider.Setup(d => d.CreateEdmTypeDeserializer(edmType)).Verifiable();

            // Act
            deserializerProvider.Object.GetEdmTypeDeserializer(edmType);

            // Assert
            deserializerProvider.Verify();
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
        public void SetEdmTypeDeserializer_ThrowsArgumentNull_EdmType()
        {
            // Arrange
            DefaultODataDeserializerProvider deserializerProvider = new DefaultODataDeserializerProvider();

            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => deserializerProvider.SetEdmTypeDeserializer(edmType: null, deserializer: null),
                "edmType");
        }

        [Fact]
        public void GetEdmTypeDeserializer_Returns_SetEdmTypeDeserializerInput()
        {
            // Arrange
            DefaultODataDeserializerProvider deserializerProvider = new DefaultODataDeserializerProvider();
            IEdmTypeReference edmType = EdmCoreModel.Instance.GetInt32(isNullable: true);
            ODataEdmTypeDeserializer deserializer = new Mock<ODataEdmTypeDeserializer>(edmType, ODataPayloadKind.Property).Object;
            deserializerProvider.SetEdmTypeDeserializer(edmType, deserializer);

            // Act & Assert
            Assert.Same(deserializer, deserializerProvider.GetEdmTypeDeserializer(edmType));
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
