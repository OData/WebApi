//-----------------------------------------------------------------------------
// <copyright file="DefaultODataDeserializerProviderTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.AspNet.OData.Formatter.Deserialization;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Moq;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Formatter.Deserialization
{
    public class DefaultODataDeserializerProviderTests
    {
        ODataDeserializerProvider _deserializerProvider = ODataDeserializerProviderFactory.Create();
        IEdmModel _edmModel = EdmTestHelpers.GetModel();

        [Fact]
        public void GetODataDeserializer_Uri()
        {
            // Arrange
            var request = RequestFactory.Create();

            // Act
            ODataDeserializer deserializer = _deserializerProvider.GetODataDeserializer(typeof(Uri), request);

            // Assert
            Assert.NotNull(deserializer);
            var referenceLinkDeserializer = Assert.IsType<ODataEntityReferenceLinkDeserializer>(deserializer);
            Assert.Equal(ODataPayloadKind.EntityReferenceLink, referenceLinkDeserializer.ODataPayloadKind);
        }

        [Theory]
        [InlineData(typeof(Int16))]
        [InlineData(typeof(int))]
        [InlineData(typeof(Decimal))]
        [InlineData(typeof(DateTimeOffset))]
        [InlineData(typeof(DateTime))]
        [InlineData(typeof(Date))]
        [InlineData(typeof(TimeOfDay))]
        [InlineData(typeof(double))]
        [InlineData(typeof(byte[]))]
        [InlineData(typeof(bool))]
        [InlineData(typeof(int?))]
        public void GetODataDeserializer_Primitive(Type type)
        {
            // Arrange
            var request = RequestFactory.Create();

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
            var request = RequestFactory.CreateFromModel(_edmModel);

            // Act
            ODataDeserializer deserializer = _deserializerProvider.GetODataDeserializer(
                typeof(ODataResourceDeserializerTests.Product), request);

            // Assert
            Assert.NotNull(deserializer);
            ODataResourceDeserializer entityDeserializer = Assert.IsType<ODataResourceDeserializer>(deserializer);
            Assert.Equal(ODataPayloadKind.Resource, deserializer.ODataPayloadKind);
            Assert.Equal(entityDeserializer.DeserializerProvider, _deserializerProvider);
        }

        [Fact]
        public void GetODataDeserializer_Resource_ForComplex()
        {
            // Arrange
            var request = RequestFactory.CreateFromModel(_edmModel);

            // Act
            ODataDeserializer deserializer = _deserializerProvider.GetODataDeserializer(
                typeof(ODataResourceDeserializerTests.Address), request);

            // Assert
            Assert.NotNull(deserializer);
            ODataResourceDeserializer complexDeserializer = Assert.IsType<ODataResourceDeserializer>(deserializer);
            Assert.Equal(ODataPayloadKind.Resource, deserializer.ODataPayloadKind);
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
            var request = RequestFactory.CreateFromModel(_edmModel);

            // Act
            ODataDeserializer deserializer = _deserializerProvider.GetODataDeserializer(collectionType, request);

            // Assert
            Assert.NotNull(deserializer);
            ODataResourceSetDeserializer resourceSetDeserializer = Assert.IsType<ODataResourceSetDeserializer>(deserializer);
            Assert.Equal(ODataPayloadKind.ResourceSet, deserializer.ODataPayloadKind);
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
            var request = RequestFactory.CreateFromModel(_edmModel);

            // Act
            ODataDeserializer deserializer = _deserializerProvider.GetODataDeserializer(collectionType, request);

            // Assert
            Assert.NotNull(deserializer);
            ODataResourceSetDeserializer resourceSetDeserializer = Assert.IsType<ODataResourceSetDeserializer>(deserializer);
            Assert.Equal(ODataPayloadKind.ResourceSet, deserializer.ODataPayloadKind);
            Assert.Equal(resourceSetDeserializer.DeserializerProvider, _deserializerProvider);
        }

        [Fact]
        public void GetODataDeserializer_ReturnsSameDeserializer_ForSameType()
        {
            // Arrange
            var request = RequestFactory.CreateFromModel(_edmModel);

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
            var request = RequestFactory.Create();

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
            var request = RequestFactory.Create();

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(
                () => _deserializerProvider.GetODataDeserializer(type: null, request: request),
                "type");
        }

        [Fact]
        public void GetEdmTypeDeserializer_ThrowsArgument_EdmType()
        {
            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(
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
