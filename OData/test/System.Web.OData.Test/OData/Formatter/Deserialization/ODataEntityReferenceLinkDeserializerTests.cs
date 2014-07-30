// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Net.Http;
using System.Web.OData.Builder;
using System.Web.OData.Routing;
using System.Web.OData.TestCommon;
using Microsoft.OData.Core;
using Microsoft.OData.Edm;
using Microsoft.TestCommon;

namespace System.Web.OData.Formatter.Deserialization
{
    public class ODataEntityReferenceLinkDeserializerTests
    {
        [Fact]
        public void Ctor_DoesnotThrow()
        {
            var deserializer = new ODataEntityReferenceLinkDeserializer();

            Assert.Equal(deserializer.ODataPayloadKind, ODataPayloadKind.EntityReferenceLink);
        }

        [Fact]
        public void Read_ThrowsArgumentNull_MessageReader()
        {
            var deserializer = new ODataEntityReferenceLinkDeserializer();

            Assert.ThrowsArgumentNull(
                () => deserializer.Read(messageReader: null, type: null, readContext: new ODataDeserializerContext()),
                "messageReader");
        }

        [Fact]
        public void Read_ThrowsArgumentNull_ReadContext()
        {
            var deserializer = new ODataEntityReferenceLinkDeserializer();
            ODataMessageReader messageReader = ODataTestUtil.GetMockODataMessageReader();

            Assert.ThrowsArgumentNull(
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
                Path = new ODataPath(new NavigationPathSegment(GetNavigationProperty(model)))
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
                Path = new ODataPath(new NavigationPathSegment(navigationProperty))
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