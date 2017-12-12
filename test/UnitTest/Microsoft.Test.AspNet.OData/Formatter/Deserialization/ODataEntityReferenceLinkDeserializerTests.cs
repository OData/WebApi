// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Net.Http;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Formatter.Deserialization;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Microsoft.Test.AspNet.OData.TestCommon;
using Xunit;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;

namespace Microsoft.Test.AspNet.OData.Formatter.Deserialization
{
    public class ODataEntityReferenceLinkDeserializerTests
    {
        [Fact]
        public void Ctor_DoesnotThrow()
        {
            var deserializer = new ODataEntityReferenceLinkDeserializer();

            Assert.Equal(ODataPayloadKind.EntityReferenceLink, deserializer.ODataPayloadKind);
        }

        [Fact]
        public void Read_ThrowsArgumentNull_MessageReader()
        {
            var deserializer = new ODataEntityReferenceLinkDeserializer();

            ExceptionAssert.ThrowsArgumentNull(
                () => deserializer.Read(messageReader: null, type: null, readContext: new ODataDeserializerContext()),
                "messageReader");
        }

        [Fact]
        public void Read_ThrowsArgumentNull_ReadContext()
        {
            var deserializer = new ODataEntityReferenceLinkDeserializer();
            ODataMessageReader messageReader = ODataTestUtil.GetMockODataMessageReader();

            ExceptionAssert.ThrowsArgumentNull(
                () => deserializer.Read(messageReader, type: null, readContext: null),
                "readContext");
        }

        [Fact]
        public void Read_RoundTrips()
        {
            // Arrange
            IEdmModel model = CreateModel();
            var deserializer = new ODataEntityReferenceLinkDeserializer();
            MockODataRequestMessage requestMessage = new MockODataRequestMessage();
            ODataMessageWriterSettings settings = new ODataMessageWriterSettings()
            {
                ODataUri = new ODataUri { ServiceRoot = new Uri("http://any/") }
            };
            settings.SetContentType(ODataFormat.Json);

            ODataMessageWriter messageWriter = new ODataMessageWriter(requestMessage, settings);
            messageWriter.WriteEntityReferenceLink(new ODataEntityReferenceLink { Url = new Uri("http://localhost/samplelink") });

            ODataMessageReaderSettings readSettings = new ODataMessageReaderSettings();
            ODataMessageReader messageReader = new ODataMessageReader(new MockODataRequestMessage(requestMessage), readSettings, model);
            ODataDeserializerContext context = new ODataDeserializerContext
            {
                Request = new HttpRequestMessage(),
                Path = new ODataPath(new NavigationPropertySegment(GetNavigationProperty(model), navigationSource: null))
            };

            // Act
            Uri uri = deserializer.Read(messageReader, typeof(Uri), context) as Uri;

            // Assert
            Assert.NotNull(uri);
            Assert.Equal("http://localhost/samplelink", uri.AbsoluteUri);
        }

        [Fact]
        public void ReadJsonLight()
        {
            // Arrange
            var deserializer = new ODataEntityReferenceLinkDeserializer();
            MockODataRequestMessage requestMessage = new MockODataRequestMessage();
            ODataMessageWriterSettings writerSettings = new ODataMessageWriterSettings();
            writerSettings.SetContentType(ODataFormat.Json);
            IEdmModel model = CreateModel();
            ODataMessageWriter messageWriter = new ODataMessageWriter(requestMessage, writerSettings, model);
            messageWriter.WriteEntityReferenceLink(new ODataEntityReferenceLink { Url = new Uri("http://localhost/samplelink") });
            ODataMessageReader messageReader = new ODataMessageReader(new MockODataRequestMessage(requestMessage),
                new ODataMessageReaderSettings(), model);

            IEdmNavigationProperty navigationProperty = GetNavigationProperty(model);

            ODataDeserializerContext context = new ODataDeserializerContext
            {
                Request = new HttpRequestMessage(),
                Path = new ODataPath(new NavigationPropertySegment(navigationProperty, navigationSource: null))
            };

            // Act
            Uri uri = deserializer.Read(messageReader, typeof(Uri), context) as Uri;

            // Assert
            Assert.NotNull(uri);
            Assert.Equal("http://localhost/samplelink", uri.AbsoluteUri);
        }

        private static IEdmModel CreateModel()
        {
            ODataModelBuilder builder = ODataModelBuilderMocks.GetModelBuilderMock<ODataModelBuilder>();
            EntitySetConfiguration<Entity> entities = builder.EntitySet<Entity>("entities");
            builder.EntitySet<RelatedEntity>("related");
            NavigationPropertyConfiguration entityToRelated =
                entities.EntityType.HasOptional<RelatedEntity>((e) => e.Related);
            entities.HasNavigationPropertyLink(entityToRelated, (a, b) => new Uri("aa:b"), false);
            entities.HasOptionalBinding((e) => e.Related, "related");

            return builder.GetEdmModel();
        }

        private static IEdmNavigationProperty GetNavigationProperty(IEdmModel model)
        {
            return
                model.EntityContainer.EntitySets().First().NavigationPropertyBindings.Single().NavigationProperty;
        }

        private class Entity
        {
            public RelatedEntity Related { get; set; }
        }

        private class RelatedEntity
        {
        }
    }
}