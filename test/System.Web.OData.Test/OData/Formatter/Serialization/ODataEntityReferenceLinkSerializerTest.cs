// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Web.OData.Routing;
using System.Xml.Linq;
using Microsoft.OData.Core;
using Microsoft.OData.Edm;
using Microsoft.TestCommon;

namespace System.Web.OData.Formatter.Serialization
{
    public class ODataEntityReferenceLinkSerializerTest
    {
        private readonly IEdmModel _model;
        private readonly IEdmEntitySet _customerSet;

        public ODataEntityReferenceLinkSerializerTest()
        {
            _model = SerializationTestsHelpers.SimpleCustomerOrderModel();
            _customerSet = _model.EntityContainer.FindEntitySet("Customers");
        }

        [Fact]
        public void WriteObject_ThrowsArgumentNull_MessageWriter()
        {
            ODataEntityReferenceLinkSerializer serializer = new ODataEntityReferenceLinkSerializer();
            Assert.ThrowsArgumentNull(
                () => serializer.WriteObject(graph: null, type: typeof(ODataEntityReferenceLink), messageWriter: null,
                    writeContext: new ODataSerializerContext()),
                "messageWriter");
        }

        [Fact]
        public void WriteObject_ThrowsArgumentNull_WriteContext()
        {
            ODataEntityReferenceLinkSerializer serializer = new ODataEntityReferenceLinkSerializer();
            Assert.ThrowsArgumentNull(
                () => serializer.WriteObject(graph: null, type: typeof(ODataEntityReferenceLink),
                    messageWriter: ODataTestUtil.GetMockODataMessageWriter(), writeContext: null),
                "writeContext");
        }

        [Fact]
        public void WriteObject_Throws_ObjectCannotBeWritten_IfGraphIsNotUri()
        {
            IEdmNavigationProperty navigationProperty = _customerSet.EntityType().NavigationProperties().First();
            ODataEntityReferenceLinkSerializer serializer = new ODataEntityReferenceLinkSerializer();
            ODataPath path = new ODataPath(new NavigationPathSegment(navigationProperty));
            ODataSerializerContext writeContext = new ODataSerializerContext { EntitySet = _customerSet, Path = path };

            Assert.Throws<SerializationException>(
                () => serializer.WriteObject(graph: "not uri", type: typeof(ODataEntityReferenceLink),
                    messageWriter: ODataTestUtil.GetMockODataMessageWriter(), writeContext: writeContext),
                "ODataEntityReferenceLinkSerializer cannot write an object of type 'System.String'.");
        }

        public static TheoryDataSet<object> SerializationTestData
        {
            get
            {
                Uri uri = new Uri("http://sampleuri/");
                return new TheoryDataSet<object>
                {
                    uri,
                    new ODataEntityReferenceLink { Url = uri }
                };
            }
        }

        [Theory]
        [PropertyData("SerializationTestData")]
        public void ODataEntityReferenceLinkSerializer_Serializes_Uri(object link)
        {
            // Arrange
            ODataEntityReferenceLinkSerializer serializer = new ODataEntityReferenceLinkSerializer();
            IEdmNavigationProperty navigationProperty = _customerSet.EntityType().NavigationProperties().First();
            ODataPath path = new ODataPath(new NavigationPathSegment(navigationProperty));
            ODataSerializerContext writeContext = new ODataSerializerContext { EntitySet = _customerSet, Path = path };
            MemoryStream stream = new MemoryStream();
            IODataResponseMessage message = new ODataMessageWrapper(stream);
            ODataMessageWriterSettings settings = new ODataMessageWriterSettings
            {
                ODataUri = new ODataUri { ServiceRoot = new Uri("http://any/"), }
            };
            settings.SetContentType(ODataFormat.Atom);
            ODataMessageWriter writer = new ODataMessageWriter(message, settings);

            // Act
            serializer.WriteObject(link, typeof(ODataEntityReferenceLink), writer, writeContext);

            // Assert
            stream.Seek(0, SeekOrigin.Begin);
            XElement element = XElement.Load(stream);
            Assert.Equal("http://sampleuri/", element.Attribute("id").Value);
        }
    }
}
