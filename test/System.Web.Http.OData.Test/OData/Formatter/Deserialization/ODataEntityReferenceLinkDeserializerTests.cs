// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.OData;
using Microsoft.TestCommon;

namespace System.Web.Http.OData.Formatter.Deserialization
{
    public class ODataEntityReferenceLinkDeserializerTests
    {
        [Fact]
        public void Constructor()
        {
            var deserializer = new ODataEntityReferenceLinkDeserializer();

            Assert.Equal(deserializer.ODataPayloadKind, ODataPayloadKind.EntityReferenceLink);
        }

        [Fact]
        public void Read()
        {
            // Arrange
            var deserializer = new ODataEntityReferenceLinkDeserializer();
            MockODataRequestMessage requestMessage = new MockODataRequestMessage();
            ODataMessageWriter messageWriter = new ODataMessageWriter(requestMessage);
            messageWriter.WriteEntityReferenceLink(new ODataEntityReferenceLink { Url = new Uri("http://localhost/samplelink") });
            ODataMessageReader messageReader = new ODataMessageReader(new MockODataRequestMessage(requestMessage));

            // Act
            Uri uri = deserializer.Read(messageReader, new ODataDeserializerReadContext()) as Uri;

            // Assert
            Assert.NotNull(uri);
            Assert.Equal("http://localhost/samplelink", uri.AbsoluteUri);
        }
    }
}
