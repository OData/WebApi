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
    public class ODataEntityReferenceLinksSerializerTest
    {
        private readonly IEdmModel _model;
        private readonly IEdmEntitySet _customerSet;

        public ODataEntityReferenceLinksSerializerTest()
        {
            _model = SerializationTestsHelpers.SimpleCustomerOrderModel();
            _customerSet = _model.FindDeclaredEntityContainer("Default.Container").FindEntitySet("Customers");
        }

        [Fact]
        public void WriteObject_ThrowsArgumentNull_MessageWriter()
        {
            ODataEntityReferenceLinksSerializer serializer = new ODataEntityReferenceLinksSerializer();
            Assert.ThrowsArgumentNull(
                () => serializer.WriteObject(graph: null, type: typeof(ODataEntityReferenceLinks), messageWriter: null,
                    writeContext: new ODataSerializerContext()),
                "messageWriter");
        }

        [Fact]
        public void WriteObject_ThrowsArgumentNull_WriteContext()
        {
            ODataEntityReferenceLinksSerializer serializer = new ODataEntityReferenceLinksSerializer();
            Assert.ThrowsArgumentNull(
                () => serializer.WriteObject(graph: null, type: typeof(ODataEntityReferenceLinks),
                    messageWriter: ODataTestUtil.GetMockODataMessageWriter(), writeContext: null),
                "writeContext");
        }

        [Fact]
        public void WriteObject_Throws_EntitySetMissingDuringSerialization()
        {
            ODataEntityReferenceLinksSerializer serializer = new ODataEntityReferenceLinksSerializer();
            ODataSerializerContext writeContext = new ODataSerializerContext();

            Assert.Throws<SerializationException>(
                () => serializer.WriteObject(graph: null, type: typeof(ODataEntityReferenceLinks),
                    messageWriter: ODataTestUtil.GetMockODataMessageWriter(), writeContext: writeContext),
                "The related entity set could not be found from the OData path. The related entity set is required to serialize the payload.");
        }

        [Fact]
        public void WriteObject_Throws_ODataPathMissing()
        {
            ODataEntityReferenceLinksSerializer serializer = new ODataEntityReferenceLinksSerializer();
            ODataSerializerContext writeContext = new ODataSerializerContext { EntitySet = _customerSet };

            Assert.Throws<SerializationException>(
                () => serializer.WriteObject(graph: null, type: typeof(ODataEntityReferenceLinks),
                    messageWriter: ODataTestUtil.GetMockODataMessageWriter(), writeContext: writeContext),
                "The operation cannot be completed because no ODataPath is available for the request.");
        }

        [Fact]
        public void WriteObject_Throws_NavigationPropertyMissingDuringSerialization()
        {
            ODataEntityReferenceLinksSerializer serializer = new ODataEntityReferenceLinksSerializer();
            ODataSerializerContext writeContext = new ODataSerializerContext { EntitySet = _customerSet, Path = new ODataPath() };

            Assert.Throws<SerializationException>(
                () => serializer.WriteObject(graph: null, type: typeof(ODataEntityReferenceLinks),
                    messageWriter: ODataTestUtil.GetMockODataMessageWriter(), writeContext: writeContext),
                "The related navigation property could not be found from the OData path. The related navigation property is required to serialize the payload.");
        }

        [Fact]
        public void WriteObject_Throws_ObjectCannotBeWritten_IfGraphIsNotUri()
        {
            IEdmNavigationProperty navigationProperty = _customerSet.ElementType.NavigationProperties().First();
            ODataEntityReferenceLinksSerializer serializer = new ODataEntityReferenceLinksSerializer();
            ODataPath path = new ODataPath(new NavigationPathSegment(navigationProperty));
            ODataSerializerContext writeContext = new ODataSerializerContext { EntitySet = _customerSet, Path = path };

            Assert.Throws<SerializationException>(
                () => serializer.WriteObject(graph: "not uri", type: typeof(ODataEntityReferenceLinks),
                    messageWriter: ODataTestUtil.GetMockODataMessageWriter(), writeContext: writeContext),
                "ODataEntityReferenceLinksSerializer cannot write an object of type 'System.String'.");
        }

        public static TheoryDataSet<object> SerializationTestData
        {
            get
            {
                Uri uri1 = new Uri("http://uri1");
                Uri uri2 = new Uri("http://uri2");
                return new TheoryDataSet<object>
                {
                    new Uri[] { uri1, uri2 },

                    new ODataEntityReferenceLinks 
                    { 
                        Links = new ODataEntityReferenceLink[] 
                        { 
                            new ODataEntityReferenceLink{ Url = uri1 }, 
                            new ODataEntityReferenceLink{ Url = uri2 }
                        }
                    }
                };
            }
        }

        [Theory]
        [PropertyData("SerializationTestData")]
        public void ODataEntityReferenceLinkSerializer_Serializes_UrisAndEntityReferenceLinks(object uris)
        {
            // Arrange
            ODataEntityReferenceLinksSerializer serializer = new ODataEntityReferenceLinksSerializer();
            IEdmNavigationProperty navigationProperty = _customerSet.ElementType.NavigationProperties().First();
            ODataPath path = new ODataPath(new NavigationPathSegment(navigationProperty));
            ODataSerializerContext writeContext = new ODataSerializerContext { EntitySet = _customerSet, Path = path };
            MemoryStream stream = new MemoryStream();
            IODataResponseMessage message = new ODataMessageWrapper(stream);

            // Act
            serializer.WriteObject(uris, typeof(ODataEntityReferenceLinks), new ODataMessageWriter(message), writeContext);

            // Assert
            stream.Seek(0, SeekOrigin.Begin);
            XElement element = XElement.Load(stream);
            Assert.Equal(2, element.Elements().Count());
            Assert.Equal("http://uri1/", element.Elements().ElementAt(0).Value);
            Assert.Equal("http://uri2/", element.Elements().ElementAt(1).Value);
        }
    }
}
