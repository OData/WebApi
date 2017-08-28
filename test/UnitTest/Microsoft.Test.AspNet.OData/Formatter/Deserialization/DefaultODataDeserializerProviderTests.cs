// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Formatter.Deserialization;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.Test.AspNet.OData.TestCommon;
using Moq;

namespace Microsoft.Test.AspNet.OData.Formatter.Deserialization
{
    public class DefaultODataDeserializerProviderTests
    {
        ODataDeserializerProvider _deserializerProvider = DependencyInjectionHelper.GetDefaultODataDeserializerProvider();
        IEdmModel _edmModel = EdmTestHelpers.GetModel();

        [Fact]
        public void GetODataDeserializer_Uri()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();

            // Act
            ODataDeserializer deserializer = _deserializerProvider.GetODataDeserializer(typeof(Uri), request);

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
            HttpRequestMessage request = new HttpRequestMessage();
            request.EnableHttpDependencyInjectionSupport();

            // Act
            ODataDeserializer deserializer = _deserializerProvider.GetODataDeserializer(type, request);

            // Assert
            Assert.NotNull(deserializer);
            ODataPrimitiveDeserializer rawValueDeserializer = Assert.IsType<ODataPrimitiveDeserializer>(deserializer);
            Assert.Equal(ODataPayloadKind.Property, rawValueDeserializer.ODataPayloadKind);
        }

        [Fact]
        public void GetODataDeserializer_Resource_ForEntity()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            request.EnableHttpDependencyInjectionSupport(_edmModel);

            // Act
            ODataDeserializer deserializer = _deserializerProvider.GetODataDeserializer(
                typeof(ODataResourceDeserializerTests.Product), request);

            // Assert
            Assert.NotNull(deserializer);
            ODataResourceDeserializer entityDeserializer = Assert.IsType<ODataResourceDeserializer>(deserializer);
            Assert.Equal(deserializer.ODataPayloadKind, ODataPayloadKind.Resource);
            Assert.Equal(entityDeserializer.DeserializerProvider, _deserializerProvider);
        }

        [Fact]
        public void GetODataDeserializer_Resource_ForComplex()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            request.EnableHttpDependencyInjectionSupport(_edmModel);

            // Act
            ODataDeserializer deserializer = _deserializerProvider.GetODataDeserializer(
                typeof(ODataResourceDeserializerTests.Address), request);

            // Assert
            Assert.NotNull(deserializer);
            ODataResourceDeserializer complexDeserializer = Assert.IsType<ODataResourceDeserializer>(deserializer);
            Assert.Equal(deserializer.ODataPayloadKind, ODataPayloadKind.Resource);
            Assert.Equal(complexDeserializer.DeserializerProvider, _deserializerProvider);
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
            HttpRequestMessage request = new HttpRequestMessage();
            request.EnableHttpDependencyInjectionSupport(_edmModel);

            // Act
            ODataDeserializer deserializer = _deserializerProvider.GetODataDeserializer(collectionType, request);

            // Assert
            Assert.NotNull(deserializer);
            ODataResourceSetDeserializer resourceSetDeserializer = Assert.IsType<ODataResourceSetDeserializer>(deserializer);
            Assert.Equal(deserializer.ODataPayloadKind, ODataPayloadKind.ResourceSet);
            Assert.Equal(resourceSetDeserializer.DeserializerProvider, _deserializerProvider);
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
            HttpRequestMessage request = new HttpRequestMessage();
            request.EnableHttpDependencyInjectionSupport(_edmModel);

            // Act
            ODataDeserializer deserializer = _deserializerProvider.GetODataDeserializer(collectionType, request);

            // Assert
            Assert.NotNull(deserializer);
            ODataResourceSetDeserializer resourceSetDeserializer = Assert.IsType<ODataResourceSetDeserializer>(deserializer);
            Assert.Equal(deserializer.ODataPayloadKind, ODataPayloadKind.ResourceSet);
            Assert.Equal(resourceSetDeserializer.DeserializerProvider, _deserializerProvider);
        }

        [Fact]
        public void GetODataDeserializer_ReturnsSameDeserializer_ForSameType()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            request.EnableHttpDependencyInjectionSupport(_edmModel);

            // Act
            ODataDeserializer firstCallDeserializer = _deserializerProvider.GetODataDeserializer(
                typeof(ODataResourceDeserializerTests.Supplier), request);
            ODataDeserializer secondCallDeserializer = _deserializerProvider.GetODataDeserializer(
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
            HttpRequestMessage request = new HttpRequestMessage();

            // Act
            ODataActionPayloadDeserializer basicActionPayload = _deserializerProvider.GetODataDeserializer(
                resourceType, request) as ODataActionPayloadDeserializer;

            // Assert
            Assert.NotNull(basicActionPayload);
        }

        [Fact]
        public void GetODataDeserializer_Throws_ArgumentNullForType()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();

            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => _deserializerProvider.GetODataDeserializer(type: null, request: request),
                "type");
        }

        [Fact]
        public void GetEdmTypeDeserializer_ThrowsArgument_EdmType()
        {
            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => _deserializerProvider.GetEdmTypeDeserializer(edmType: null),
                "edmType");
        }

        [Fact]
        public void GetEdmTypeDeserializer_Caches_CreateDeserializerOutput()
        {
            // Arrange
            IEdmTypeReference edmType = new Mock<IEdmTypeReference>().Object;

            // Act
            var deserializer1 = _deserializerProvider.GetEdmTypeDeserializer(edmType);
            var deserializer2 = _deserializerProvider.GetEdmTypeDeserializer(edmType);

            // Assert
            Assert.Same(deserializer1, deserializer2);
        }
    }
}
