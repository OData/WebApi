// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http;
using Microsoft.OData;
using Microsoft.OData.Edm;
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
            // Arrange
            ODataDeserializerProvider deserializerProvider = new DefaultODataDeserializerProvider();
            HttpRequestMessage request = new HttpRequestMessage();

            // Act
            ODataDeserializer deserializer = deserializerProvider.GetODataDeserializer(_edmModel, typeof(Uri), request);

            // Assert
            Assert.NotNull(deserializer);
            var referenceLinkDeserializer = Assert.IsType<ODataEntityReferenceLinkDeserializer>(deserializer);
            Assert.Equal(ODataPayloadKind.EntityReferenceLink, referenceLinkDeserializer.ODataPayloadKind);
        }

        [Theory]
        [InlineData(typeof(Int16), EdmPrimitiveTypeKind.Int16)]
        [InlineData(typeof(int), EdmPrimitiveTypeKind.Int32)]
        [InlineData(typeof(Decimal), EdmPrimitiveTypeKind.Decimal)]
        [InlineData(typeof(DateTimeOffset), EdmPrimitiveTypeKind.DateTimeOffset)]
        [InlineData(typeof(DateTime), EdmPrimitiveTypeKind.DateTimeOffset)]
        [InlineData(typeof(Date), EdmPrimitiveTypeKind.Date)]
        [InlineData(typeof(TimeOfDay), EdmPrimitiveTypeKind.TimeOfDay)]
        [InlineData(typeof(double), EdmPrimitiveTypeKind.Double)]
        [InlineData(typeof(byte[]), EdmPrimitiveTypeKind.Binary)]
        [InlineData(typeof(bool), EdmPrimitiveTypeKind.Boolean)]
        [InlineData(typeof(int?), EdmPrimitiveTypeKind.Int32)]
        public void GetODataDeserializer_Primitive(Type type, EdmPrimitiveTypeKind primitiveKind)
        {
            // Arrange
            ODataDeserializerProvider deserializerProvider = new DefaultODataDeserializerProvider();
            HttpRequestMessage request = new HttpRequestMessage();

            // Act
            ODataDeserializer deserializer = deserializerProvider.GetODataDeserializer(_edmModel, type, request);

            // Assert
            Assert.NotNull(deserializer);
            ODataPrimitiveDeserializer rawValueDeserializer = Assert.IsType<ODataPrimitiveDeserializer>(deserializer);
            Assert.Equal(ODataPayloadKind.Property, rawValueDeserializer.ODataPayloadKind);
        }

        [Fact]
        public void GetODataDeserializer_Resource_ForEntity()
        {
            // Arrange
            ODataDeserializerProvider deserializerProvider = new DefaultODataDeserializerProvider();
            HttpRequestMessage request = new HttpRequestMessage();

            // Act
            ODataDeserializer deserializer = deserializerProvider.GetODataDeserializer(_edmModel,
                typeof(ODataResourceDeserializerTests.Product), request);

            // Assert
            Assert.NotNull(deserializer);
            ODataResourceDeserializer entityDeserializer = Assert.IsType<ODataResourceDeserializer>(deserializer);
            Assert.Equal(deserializer.ODataPayloadKind, ODataPayloadKind.Resource);
            Assert.Equal(entityDeserializer.DeserializerProvider, deserializerProvider);
        }

        [Fact]
        public void GetODataDeserializer_Resource_ForComplex()
        {
            // Arrange
            ODataDeserializerProvider deserializerProvider = new DefaultODataDeserializerProvider();
            HttpRequestMessage request = new HttpRequestMessage();

            // Act
            ODataDeserializer deserializer = deserializerProvider.GetODataDeserializer(_edmModel,
                typeof(ODataResourceDeserializerTests.Address), request);

            // Assert
            Assert.NotNull(deserializer);
            ODataResourceDeserializer complexDeserializer = Assert.IsType<ODataResourceDeserializer>(deserializer);
            Assert.Equal(deserializer.ODataPayloadKind, ODataPayloadKind.Resource);
            Assert.Equal(complexDeserializer.DeserializerProvider, deserializerProvider);
        }

        [Theory]
        [InlineData(typeof(ODataResourceDeserializerTests.Supplier[]))]
        [InlineData(typeof(IEnumerable<ODataResourceDeserializerTests.Supplier>))]
        [InlineData(typeof(ICollection<ODataResourceDeserializerTests.Supplier>))]
        [InlineData(typeof(IList<ODataResourceDeserializerTests.Supplier>))]
        [InlineData(typeof(List<ODataResourceDeserializerTests.Supplier>))]
        [InlineData(typeof(PageResult<ODataResourceDeserializerTests.Supplier>))]
        public void GetODataDeserializer_ResourceSet_ForEntityCollection(Type collectionType)
        {
            // Arrange
            ODataDeserializerProvider deserializerProvider = new DefaultODataDeserializerProvider();
            HttpRequestMessage request = new HttpRequestMessage();

            // Act
            ODataDeserializer deserializer = deserializerProvider.GetODataDeserializer(_edmModel, collectionType, request);

            // Assert
            Assert.NotNull(deserializer);
            ODataResourceSetDeserializer resourceSetDeserializer = Assert.IsType<ODataResourceSetDeserializer>(deserializer);
            Assert.Equal(deserializer.ODataPayloadKind, ODataPayloadKind.ResourceSet);
            Assert.Equal(resourceSetDeserializer.DeserializerProvider, deserializerProvider);
        }

        [Theory]
        [InlineData(typeof(ODataResourceDeserializerTests.Address[]))]
        [InlineData(typeof(IEnumerable<ODataResourceDeserializerTests.Address>))]
        [InlineData(typeof(ICollection<ODataResourceDeserializerTests.Address>))]
        [InlineData(typeof(IList<ODataResourceDeserializerTests.Address>))]
        [InlineData(typeof(List<ODataResourceDeserializerTests.Address>))]
        [InlineData(typeof(PageResult<ODataResourceDeserializerTests.Address>))]
        public void GetODataDeserializer_ResourceSet_ForComplexCollection(Type collectionType)
        {
            // Arrange
            ODataDeserializerProvider deserializerProvider = new DefaultODataDeserializerProvider();
            HttpRequestMessage request = new HttpRequestMessage();

            // Act
            ODataDeserializer deserializer = deserializerProvider.GetODataDeserializer(_edmModel, collectionType, request);

            // Assert
            Assert.NotNull(deserializer);
            ODataResourceSetDeserializer resourceSetDeserializer = Assert.IsType<ODataResourceSetDeserializer>(deserializer);
            Assert.Equal(deserializer.ODataPayloadKind, ODataPayloadKind.ResourceSet);
            Assert.Equal(resourceSetDeserializer.DeserializerProvider, deserializerProvider);
        }

        [Fact]
        public void GetODataDeserializer_ReturnsSameDeserializer_ForSameType()
        {
            // Arrange
            ODataDeserializerProvider deserializerProvider = new DefaultODataDeserializerProvider();
            HttpRequestMessage request = new HttpRequestMessage();

            // Act
            ODataDeserializer firstCallDeserializer = deserializerProvider.GetODataDeserializer(_edmModel,
                typeof(ODataResourceDeserializerTests.Supplier), request);
            ODataDeserializer secondCallDeserializer = deserializerProvider.GetODataDeserializer(_edmModel,
                typeof(ODataResourceDeserializerTests.Supplier), request);

            // Assert
            Assert.Same(firstCallDeserializer, secondCallDeserializer);
        }

        [Theory]
        [InlineData(typeof(ODataActionParameters))]
        [InlineData(typeof(ODataUntypedActionParameters))]
        public void GetODataDeserializer_ActionPayload(Type resourceType)
        {
            // Arrange
            ODataDeserializerProvider deserializerProvider = new DefaultODataDeserializerProvider();
            HttpRequestMessage request = new HttpRequestMessage();

            // Act
            ODataActionPayloadDeserializer basicActionPayload = deserializerProvider.GetODataDeserializer(_edmModel,
                resourceType, request) as ODataActionPayloadDeserializer;

            // Assert
            Assert.NotNull(basicActionPayload);
        }

        [Fact]
        public void GetODataDeserializer_Throws_ArgumentNullForModel()
        {
            // Arrange
            DefaultODataDeserializerProvider deserializerProvider = new DefaultODataDeserializerProvider();
            HttpRequestMessage request = new HttpRequestMessage();

            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => deserializerProvider.GetODataDeserializer(model: null, type: typeof(int), request: request),
                "model");
        }

        [Fact]
        public void GetODataDeserializer_Throws_ArgumentNullForType()
        {
            // Arrange
            DefaultODataDeserializerProvider deserializerProvider = new DefaultODataDeserializerProvider();
            HttpRequestMessage request = new HttpRequestMessage();

            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => deserializerProvider.GetODataDeserializer(model: EdmCoreModel.Instance, type: null, request: request),
                "type");
        }

        [Fact]
        public void GetEdmTypeDeserializer_ThrowsArgument_EdmType()
        {
            // Arrange
            DefaultODataDeserializerProvider deserializerProvider = new DefaultODataDeserializerProvider();

            // Act & Assert
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
