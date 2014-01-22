// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections;
using System.Linq;
using System.Runtime.Serialization;
using System.Web.OData.Builder;
using System.Web.OData.TestCommon.Models;
using Microsoft.OData.Core;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.OData.Formatter.Deserialization
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
            _customersType = new EdmCollectionTypeReference(new EdmCollectionType(_customerType));
        }

        [Fact]
        public void Ctor_ThrowsArgumentNull_DeserializerProvider()
        {
            Assert.ThrowsArgumentNull(
                () => new ODataFeedDeserializer(deserializerProvider: null),
                "deserializerProvider");
        }

        [Fact]
        public void ReadInline_ReturnsNull_IfItemIsNull()
        {
            var deserializer = new ODataFeedDeserializer(new DefaultODataDeserializerProvider());
            Assert.Null(deserializer.ReadInline(item: null, edmType: _customersType, readContext: new ODataDeserializerContext()));
        }

        [Fact]
        public void ReadInline_Throws_ArgumentMustBeOfType()
        {
            var deserializer = new ODataFeedDeserializer(new DefaultODataDeserializerProvider());
            Assert.ThrowsArgument(
                () => deserializer.ReadInline(item: 42, edmType: _customersType, readContext: new ODataDeserializerContext()),
                "item",
                "The argument must be of type 'ODataFeedWithEntries'.");
        }

        [Fact]
        public void ReadInline_Calls_ReadFeed()
        {
            // Arrange
            ODataDeserializerProvider deserializerProvider = new DefaultODataDeserializerProvider();
            Mock<ODataFeedDeserializer> deserializer = new Mock<ODataFeedDeserializer>(deserializerProvider);
            ODataFeedWithEntries feedWrapper = new ODataFeedWithEntries(new ODataFeed());
            ODataDeserializerContext readContext = new ODataDeserializerContext();
            IEnumerable expectedResult = new object[0];

            deserializer.CallBase = true;
            deserializer.Setup(f => f.ReadFeed(feedWrapper, _customerType, readContext)).Returns(expectedResult).Verifiable();

            // Act
            var result = deserializer.Object.ReadInline(feedWrapper, _customersType, readContext);

            // Assert
            deserializer.Verify();
            Assert.Same(expectedResult, result);
        }

        [Fact]
        public void ReadFeed_Throws_TypeCannotBeDeserialized()
        {
            Mock<ODataDeserializerProvider> deserializerProvider = new Mock<ODataDeserializerProvider>();
            ODataFeedDeserializer deserializer = new ODataFeedDeserializer(deserializerProvider.Object);
            ODataFeedWithEntries feedWrapper = new ODataFeedWithEntries(new ODataFeed());
            ODataDeserializerContext readContext = new ODataDeserializerContext();

            deserializerProvider.Setup(p => p.GetEdmTypeDeserializer(_customerType)).Returns<ODataEdmTypeDeserializer>(null);

            Assert.Throws<SerializationException>(
                () => deserializer.ReadFeed(feedWrapper, _customerType, readContext).GetEnumerator().MoveNext(),
                "'System.Web.OData.TestCommon.Models.Customer' cannot be deserialized using the ODataMediaTypeFormatter.");
        }

        [Fact]
        public void ReadFeed_Calls_ReadInlineForEachEntry()
        {
            // Arrange
            Mock<ODataDeserializerProvider> deserializerProvider = new Mock<ODataDeserializerProvider>();
            Mock<ODataEdmTypeDeserializer> entityDeserializer = new Mock<ODataEdmTypeDeserializer>(ODataPayloadKind.Entry);
            ODataFeedDeserializer deserializer = new ODataFeedDeserializer(deserializerProvider.Object);
            ODataFeedWithEntries feedWrapper = new ODataFeedWithEntries(new ODataFeed());
            feedWrapper.Entries.Add(new ODataEntryWithNavigationLinks(new ODataEntry { Id = new Uri("http://a1/") }));
            feedWrapper.Entries.Add(new ODataEntryWithNavigationLinks(new ODataEntry { Id = new Uri("http://a2/") }));
            ODataDeserializerContext readContext = new ODataDeserializerContext();

            deserializerProvider.Setup(p => p.GetEdmTypeDeserializer(_customerType)).Returns(entityDeserializer.Object);
            entityDeserializer.Setup(d => d.ReadInline(feedWrapper.Entries[0], _customerType, readContext)).Returns("entry1").Verifiable();
            entityDeserializer.Setup(d => d.ReadInline(feedWrapper.Entries[1], _customerType, readContext)).Returns("entry2").Verifiable();

            // Act
            var result = deserializer.ReadFeed(feedWrapper, _customerType, readContext);

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
