//-----------------------------------------------------------------------------
// <copyright file="ODataEntityReferenceLinkDeserializerTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Linq;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Formatter.Deserialization;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Xunit;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;

namespace Microsoft.AspNet.OData.Test.Formatter.Deserialization
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

            var config = RoutingConfigurationFactory.CreateWithRootContainer("OData");
            var request = RequestFactory.Create(config, "OData");
            ODataMessageReaderSettings readSettings = new ODataMessageReaderSettings();
            ODataMessageReader messageReader = new ODataMessageReader(new MockODataRequestMessage(requestMessage), readSettings, model);
            ODataDeserializerContext context = new ODataDeserializerContext
            {
                Request = request,
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

            var config = RoutingConfigurationFactory.CreateWithRootContainer("OData");
            var request = RequestFactory.Create(config, "OData");
            ODataDeserializerContext context = new ODataDeserializerContext
            {
                Request = request,
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
