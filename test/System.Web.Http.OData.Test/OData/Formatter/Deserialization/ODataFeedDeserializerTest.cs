// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections;
using System.Linq;
using System.Runtime.Serialization;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.TestCommon.Models;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Library;
using Microsoft.Data.OData;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.OData.Formatter.Deserialization
{
    public class ODataFeedDeserializerTest
    {
        private readonly IEdmModel _model;
        private readonly IEdmCollectionTypeReference _customersType;
        private readonly IEdmEntityTypeReference _customerType;

        public ODataFeedDeserializerTest()
        {
            _model = GetEdmModel();
            _customerType = _model.GetEdmTypeReference(typeof(Customer)).AsEntity();
            _customersType = new EdmCollectionTypeReference(new EdmCollectionType(_customerType), isNullable: false);
        }

        [Fact]
        public void Ctor_ThrowsArgumentNull_EdmType()
        {
            Assert.ThrowsArgumentNull(
                () => new ODataFeedDeserializer(edmType: null, deserializerProvider: new DefaultODataDeserializerProvider()),
                "edmType");
        }

        [Fact]
        public void Ctor_ThrowsArgumentNull_DeserializerProvider()
        {
            Assert.ThrowsArgumentNull(
                () => new ODataFeedDeserializer(edmType: _customersType, deserializerProvider: null),
                "deserializerProvider");
        }

        [Fact]
        public void Ctor_ThrowsArgument_IfEdmTypeIsNotEntityCollection()
        {
            IEdmCollectionTypeReference edmType =
                new EdmCollectionTypeReference(
                    new EdmCollectionType(EdmCoreModel.Instance.GetInt32(isNullable: false)),
                    isNullable: false);

            Assert.ThrowsArgument(
                () => new ODataFeedDeserializer(edmType, new DefaultODataDeserializerProvider()),
                "edmType",
                "Edm.Int32 is not a collection of type IEdmEntityType. Only entity collections are supported.");
        }

        [Fact]
        public void Ctor_SetsProperty_CollectionType()
        {
            var deserializer = new ODataFeedDeserializer(_customersType, new DefaultODataDeserializerProvider());
            Assert.Equal(_customersType, deserializer.CollectionType);
        }

        [Fact]
        public void Ctor_SetsProperty_EntityType()
        {
            var deserializer = new ODataFeedDeserializer(_customersType, new DefaultODataDeserializerProvider());
            Assert.Equal(_customerType, deserializer.EntityType);
        }

        [Fact]
        public void ReadInline_ReturnsNull_IfItemIsNull()
        {
            var deserializer = new ODataFeedDeserializer(_customersType, new DefaultODataDeserializerProvider());
            Assert.Null(deserializer.ReadInline(item: null, readContext: new ODataDeserializerContext()));
        }

        [Fact]
        public void ReadInline_Throws_ArgumentMustBeOfType()
        {
            var deserializer = new ODataFeedDeserializer(_customersType, new DefaultODataDeserializerProvider());
            Assert.ThrowsArgument(
                () => deserializer.ReadInline(item: 42, readContext: new ODataDeserializerContext()),
                "item",
                "The argument must be of type 'ODataFeedWithEntries'.");
        }

        [Fact]
        public void ReadInline_Calls_ReadFeed()
        {
            // Arrange
            ODataDeserializerProvider deserializerProvider = new DefaultODataDeserializerProvider();
            Mock<ODataFeedDeserializer> deserializer = new Mock<ODataFeedDeserializer>(_customersType, deserializerProvider);
            ODataFeedWithEntries feedWrapper = new ODataFeedWithEntries(new ODataFeed());
            ODataDeserializerContext readContext = new ODataDeserializerContext();
            IEnumerable expectedResult = new object[0];

            deserializer.CallBase = true;
            deserializer.Setup(f => f.ReadFeed(feedWrapper, readContext)).Returns(expectedResult).Verifiable();

            // Act
            var result = deserializer.Object.ReadInline(feedWrapper, readContext);

            // Assert
            deserializer.Verify();
            Assert.Same(expectedResult, result);
        }

        [Fact]
        public void ReadFeed_Throws_TypeCannotBeDeserialized()
        {
            Mock<ODataDeserializerProvider> deserializerProvider = new Mock<ODataDeserializerProvider>();
            ODataFeedDeserializer deserializer = new ODataFeedDeserializer(_customersType, deserializerProvider.Object);
            ODataFeedWithEntries feedWrapper = new ODataFeedWithEntries(new ODataFeed());
            ODataDeserializerContext readContext = new ODataDeserializerContext();

            deserializerProvider.Setup(p => p.GetEdmTypeDeserializer(_customerType)).Returns<ODataEdmTypeDeserializer>(null);

            Assert.Throws<SerializationException>(
                () => deserializer.ReadFeed(feedWrapper, readContext).GetEnumerator().MoveNext(),
                "'System.Web.Http.OData.TestCommon.Models.Customer' cannot be deserialized using the ODataMediaTypeFormatter.");
        }

        [Fact]
        public void ReadFeed_Calls_ReadInlineForEachEntry()
        {
            // Arrange
            Mock<ODataDeserializerProvider> deserializerProvider = new Mock<ODataDeserializerProvider>();
            Mock<ODataEdmTypeDeserializer> entityDeserializer = new Mock<ODataEdmTypeDeserializer>(_customerType, ODataPayloadKind.Entry);
            ODataFeedDeserializer deserializer = new ODataFeedDeserializer(_customersType, deserializerProvider.Object);
            ODataFeedWithEntries feedWrapper = new ODataFeedWithEntries(new ODataFeed());
            feedWrapper.Entries.Add(new ODataEntryWithNavigationLinks(new ODataEntry { Id = "1" }));
            feedWrapper.Entries.Add(new ODataEntryWithNavigationLinks(new ODataEntry { Id = "2" }));
            ODataDeserializerContext readContext = new ODataDeserializerContext();

            deserializerProvider.Setup(p => p.GetEdmTypeDeserializer(_customerType)).Returns(entityDeserializer.Object);
            entityDeserializer.Setup(d => d.ReadInline(feedWrapper.Entries[0], readContext)).Returns("entry1").Verifiable();
            entityDeserializer.Setup(d => d.ReadInline(feedWrapper.Entries[1], readContext)).Returns("entry2").Verifiable();

            // Act
            var result = deserializer.ReadFeed(feedWrapper, readContext);

            // Assert
            Assert.Equal(new[] { "entry1", "entry2" }, result.OfType<String>());
            entityDeserializer.Verify();
        }

        private IEdmModel GetEdmModel()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<Customer>("customers");
            return builder.GetEdmModel();
        }
    }
}
