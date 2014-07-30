// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Web.Http.OData.Routing;
using System.Xml.Linq;
using Microsoft.Data.Edm;
using Microsoft.Data.OData;
using Microsoft.TestCommon;

namespace System.Web.Http.OData.Formatter.Serialization
{
    public class ODataEntityReferenceLinkSerializerTest
    {
        private readonly IEdmModel _model;
        private readonly IEdmEntitySet _customerSet;

        public ODataEntityReferenceLinkSerializerTest()
        {
            _model = SerializationTestsHelpers.SimpleCustomerOrderModel();
            _customerSet = _model.FindDeclaredEntityContainer("Default.Container").FindEntitySet("Customers");
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
        public void WriteObject_Throws_EntitySetMissingDuringSerialization()
        {
            ODataEntityReferenceLinkSerializer serializer = new ODataEntityReferenceLinkSerializer();
            ODataSerializerContext writeContext = new ODataSerializerContext();

            Assert.Throws<SerializationException>(
                () => serializer.WriteObject(graph: null, type: typeof(ODataEntityReferenceLink),
                    messageWriter: ODataTestUtil.GetMockODataMessageWriter(), writeContext: writeContext),
                "The related entity set could not be found from the OData path. The related entity set is required to serialize the payload.");
        }

        [Fact]
        public void WriteObject_Throws_ODataPathMissing()
        {
            ODataEntityReferenceLinkSerializer serializer = new ODataEntityReferenceLinkSerializer();
            ODataSerializerContext writeContext = new ODataSerializerContext { EntitySet = _customerSet };

            Assert.Throws<SerializationException>(
                () => serializer.WriteObject(graph: null, type: typeof(ODataEntityReferenceLink),
                    messageWriter: ODataTestUtil.GetMockODataMessageWriter(), writeContext: writeContext),
                "The operation cannot be completed because no ODataPath is available for the request.");
        }

        [Fact]
        public void WriteObject_Throws_NavigationPropertyMissingDuringSerialization()
        {
            ODataEntityReferenceLinkSerializer serializer = new ODataEntityReferenceLinkSerializer();
            ODataSerializerContext writeContext = new ODataSerializerContext { EntitySet = _customerSet, Path = new ODataPath() };

            Assert.Throws<SerializationException>(
                () => serializer.WriteObject(graph: null, type: typeof(ODataEntityReferenceLink),
                    messageWriter: ODataTestUtil.GetMockODataMessageWriter(), writeContext: writeContext),
                "The related navigation property could not be found from the OData path. The related navigation property is required to serialize the payload.");
        }

        [Fact]
        public void WriteObject_Throws_ObjectCannotBeWritten_IfGraphIsNotUri()
        {
            IEdmNavigationProperty navigationProperty = _customerSet.ElementType.NavigationProperties().First();
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
            IEdmNavigationProperty navigationProperty = _customerSet.ElementType.NavigationProperties().First();
            ODataPath path = new ODataPath(new NavigationPathSegment(navigationProperty));
            ODataSerializerContext writeContext = new ODataSerializerContext { EntitySet = _customerSet, Path = path };
            MemoryStream stream = new MemoryStream();
            IODataResponseMessage message = new ODataMessageWrapper(stream);

            // Act
            serializer.WriteObject(link, typeof(ODataEntityReferenceLink), new ODataMessageWriter(message), writeContext);

            // Assert
            stream.Seek(0, SeekOrigin.Begin);
            XElement element = XElement.Load(stream);
            Assert.Equal("http://sampleuri/", element.Value);
        }
    }
}
