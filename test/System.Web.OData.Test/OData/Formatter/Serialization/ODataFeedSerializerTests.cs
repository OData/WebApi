// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using System.Web.OData.Formatter.Serialization.Models;
using System.Web.OData.Query;
using System.Web.OData.TestCommon;
using Microsoft.OData.Core;
using Microsoft.OData.Core.UriParser.Semantic;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.OData.Formatter.Serialization
{
    public class ODataFeedSerializerTests
    {
        IEdmModel _model;
        IEdmEntitySet _customerSet;
        Customer[] _customers;
        ODataFeedSerializer _serializer;
        IEdmCollectionTypeReference _customersType;
        ODataSerializerContext _writeContext;

        public ODataFeedSerializerTests()
        {
            _model = SerializationTestsHelpers.SimpleCustomerOrderModel();
            _customerSet = _model.EntityContainer.FindEntitySet("Customers");
            _model.SetAnnotationValue(_customerSet.EntityType(), new ClrTypeAnnotation(typeof(Customer)));
            _customers = new[] {
                new Customer()
                {
                    FirstName = "Foo",
                    LastName = "Bar",
                    ID = 10,
                },
                new Customer()
                {
                    FirstName = "Foo",
                    LastName = "Bar",
                    ID = 42,
                }
            };

            _customersType = _model.GetEdmTypeReference(typeof(Customer[])).AsCollection();

            _writeContext = new ODataSerializerContext() { NavigationSource = _customerSet, Model = _model };
        }

        [Fact]
        public void Ctor_ThrowsArgumentNull_SerializerProvider()
        {
            Assert.ThrowsArgumentNull(
                () => new ODataFeedSerializer(serializerProvider: null),
                "serializerProvider");
        }

        [Fact]
        public void WriteObject_ThrowsArgumentNull_MessageWriter()
        {
            ODataFeedSerializer serializer = new ODataFeedSerializer(new DefaultODataSerializerProvider());
            Assert.ThrowsArgumentNull(
                () => serializer.WriteObject(graph: null, type: null, messageWriter: null, writeContext: new ODataSerializerContext()),
                "messageWriter");
        }

        [Fact]
        public void WriteObject_ThrowsArgumentNull_WriteContext()
        {
            ODataFeedSerializer serializer = new ODataFeedSerializer(new DefaultODataSerializerProvider());
            Assert.ThrowsArgumentNull(
                () => serializer.WriteObject(graph: null, type: null, messageWriter: ODataTestUtil.GetMockODataMessageWriter(), writeContext: null),
                "writeContext");
        }

        [Fact]
        public void WriteObject_ThrowsEntitySetMissingDuringSerialization()
        {
            ODataFeedSerializer serializer = new ODataFeedSerializer(new DefaultODataSerializerProvider());
            Assert.Throws<SerializationException>(
                () => serializer.WriteObject(graph: null, type: null, messageWriter: ODataTestUtil.GetMockODataMessageWriter(), writeContext: new ODataSerializerContext()),
                "The related entity set could not be found from the OData path. The related entity set is required to serialize the payload.");
        }

        [Fact]
        public void WriteObject_Calls_WriteObjectInline()
        {
            // Arrange
            object graph = new object();
            Mock<ODataFeedSerializer> serializer = new Mock<ODataFeedSerializer>(new DefaultODataSerializerProvider());
            serializer.CallBase = true;
            serializer
                .Setup(s => s.WriteObjectInline(graph, It.Is<IEdmTypeReference>(e => _customersType.IsEquivalentTo(e)),
                    It.IsAny<ODataWriter>(), _writeContext))
                .Verifiable();

            // Act
            serializer.Object.WriteObject(graph, typeof(Customer[]), ODataTestUtil.GetMockODataMessageWriter(), _writeContext);

            // Assert
            serializer.Verify();
        }

        [Fact]
        public void WriteObjectInline_ThrowsArgumentNull_Writer()
        {
            ODataFeedSerializer serializer = new ODataFeedSerializer(new DefaultODataSerializerProvider());
            Assert.ThrowsArgumentNull(
                () => serializer.WriteObjectInline(graph: null, expectedType: null, writer: null, writeContext: new ODataSerializerContext()),
                "writer");
        }

        [Fact]
        public void WriteObjectInline_ThrowsArgumentNull_WriteContext()
        {
            ODataFeedSerializer serializer = new ODataFeedSerializer(new DefaultODataSerializerProvider());
            Assert.ThrowsArgumentNull(
                () => serializer.WriteObjectInline(graph: null, expectedType: null, writer: new Mock<ODataWriter>().Object, writeContext: null),
                "writeContext");
        }

        [Fact]
        public void WriteObjectInline_ThrowsSerializationException_CannotSerializerNull()
        {
            ODataFeedSerializer serializer = new ODataFeedSerializer(new DefaultODataSerializerProvider());
            Assert.Throws<SerializationException>(
                () => serializer.WriteObjectInline(graph: null, expectedType: _customersType,
                    writer: new Mock<ODataWriter>().Object, writeContext: _writeContext),
                "Cannot serialize a null 'feed'.");
        }

        [Fact]
        public void WriteObjectInline_ThrowsSerializationException_IfGraphIsNotEnumerable()
        {
            ODataFeedSerializer serializer = new ODataFeedSerializer(new DefaultODataSerializerProvider());
            Assert.Throws<SerializationException>(
                () => serializer.WriteObjectInline(graph: 42, expectedType: _customersType,
                    writer: new Mock<ODataWriter>().Object, writeContext: _writeContext),
                "ODataFeedSerializer cannot write an object of type 'System.Int32'.");
        }

        [Fact]
        public void WriteObjectInline_Throws_NullElementInCollection_IfFeedContainsNullElement()
        {
            // Arrange
            IEnumerable instance = new object[] { null };
            ODataFeedSerializer serializer = new ODataFeedSerializer(new DefaultODataSerializerProvider());

            // Act
            Assert.Throws<SerializationException>(
                () => serializer.WriteObjectInline(instance, _customersType, new Mock<ODataWriter>().Object, _writeContext),
                "Collections cannot contain null elements.");
        }

        [Fact]
        public void WriteObjectInline_Throws_TypeCannotBeSerialized_IfFeedContainsEntityThatCannotBeSerialized()
        {
            // Arrange
            Mock<ODataSerializerProvider> serializerProvider = new Mock<ODataSerializerProvider>();
            HttpRequestMessage request = new HttpRequestMessage();
            serializerProvider.Setup(s => s.GetODataPayloadSerializer(_model, typeof(int), request)).Returns<ODataSerializer>(null);
            IEnumerable instance = new object[] { 42 };
            ODataFeedSerializer serializer = new ODataFeedSerializer(serializerProvider.Object);

            // Act
            Assert.Throws<SerializationException>(
                () => serializer.WriteObjectInline(instance, _customersType, new Mock<ODataWriter>().Object, _writeContext),
                "'Default.Customer' cannot be serialized using the ODataMediaTypeFormatter.");
        }

        [Fact]
        public void WriteObjectInline_Calls_CreateODataFeed()
        {
            // Arrange
            IEnumerable instance = new object[0];
            Mock<ODataFeedSerializer> serializer = new Mock<ODataFeedSerializer>(new DefaultODataSerializerProvider());
            serializer.CallBase = true;
            serializer.Setup(s => s.CreateODataFeed(instance, _customersType, _writeContext)).Returns(new ODataFeed()).Verifiable();

            // Act
            serializer.Object.WriteObjectInline(instance, _customersType, new Mock<ODataWriter>().Object, _writeContext);

            // Assert
            serializer.Verify();
        }

        [Fact]
        public void WriteObjectInline_Throws_CannotSerializerNull_IfCreateODataFeedReturnsNull()
        {
            // Arrange
            IEnumerable instance = new object[0];
            Mock<ODataFeedSerializer> serializer = new Mock<ODataFeedSerializer>(new DefaultODataSerializerProvider());
            serializer.CallBase = true;
            serializer.Setup(s => s.CreateODataFeed(instance, _customersType, _writeContext)).Returns<ODataFeed>(null);
            ODataWriter writer = new Mock<ODataWriter>().Object;

            // Act & Assert
            Assert.Throws<SerializationException>(
                () => serializer.Object.WriteObjectInline(instance, _customersType, writer, _writeContext),
                "Cannot serialize a null 'feed'.");
        }

        [Fact]
        public void WriteObjectInline_Writes_CreateODataFeedOutput()
        {
            // Arrange
            IEnumerable instance = new object[0];
            ODataFeed feed = new ODataFeed();
            Mock<ODataFeedSerializer> serializer = new Mock<ODataFeedSerializer>(new DefaultODataSerializerProvider());
            serializer.CallBase = true;
            serializer.Setup(s => s.CreateODataFeed(instance, _customersType, _writeContext)).Returns(feed);
            Mock<ODataWriter> writer = new Mock<ODataWriter>();
            writer.Setup(s => s.WriteStart(feed)).Verifiable();

            // Act
            serializer.Object.WriteObjectInline(instance, _customersType, writer.Object, _writeContext);

            // Assert
            writer.Verify();
        }

        [Fact]
        public void WriteObjectInline_WritesEachEntityInstance()
        {
            // Arrange
            Mock<ODataEdmTypeSerializer> customerSerializer = new Mock<ODataEdmTypeSerializer>(ODataPayloadKind.Entry);
            ODataSerializerProvider provider = ODataTestUtil.GetMockODataSerializerProvider(customerSerializer.Object);
            var mockWriter = new Mock<ODataWriter>();

            customerSerializer.Setup(s => s.WriteObjectInline(_customers[0], _customersType.ElementType(), mockWriter.Object, _writeContext)).Verifiable();
            customerSerializer.Setup(s => s.WriteObjectInline(_customers[1], _customersType.ElementType(), mockWriter.Object, _writeContext)).Verifiable();

            _serializer = new ODataFeedSerializer(provider);

            // Act
            _serializer.WriteObjectInline(_customers, _customersType, mockWriter.Object, _writeContext);

            // Assert
            customerSerializer.Verify();
        }

        [Fact]
        public void WriteObjectInline_Can_WriteCollectionOfIEdmObjects()
        {
            // Arrange
            IEdmTypeReference edmType = new EdmEntityTypeReference(new EdmEntityType("NS", "Name"), isNullable: false);
            IEdmCollectionTypeReference feedType = new EdmCollectionTypeReference(new EdmCollectionType(edmType));
            Mock<IEdmObject> edmObject = new Mock<IEdmObject>();
            edmObject.Setup(e => e.GetEdmType()).Returns(edmType);

            var mockWriter = new Mock<ODataWriter>();

            Mock<ODataEdmTypeSerializer> customSerializer = new Mock<ODataEdmTypeSerializer>(ODataPayloadKind.Entry);
            customSerializer.Setup(s => s.WriteObjectInline(edmObject.Object, edmType, mockWriter.Object, _writeContext)).Verifiable();

            Mock<ODataSerializerProvider> serializerProvider = new Mock<ODataSerializerProvider>();
            serializerProvider.Setup(s => s.GetEdmTypeSerializer(edmType)).Returns(customSerializer.Object);

            ODataFeedSerializer serializer = new ODataFeedSerializer(serializerProvider.Object);

            // Act
            serializer.WriteObjectInline(new[] { edmObject.Object }, feedType, mockWriter.Object, _writeContext);

            // Assert
            customSerializer.Verify();
        }

        [Fact]
        public void WriteObjectInline_Sets_CountQueryOption_OnWriteStart()
        {
            // Arrange
            IEnumerable instance = new object[0];
            ODataFeed feed = new ODataFeed { Count = 1000 };
            Mock<ODataFeedSerializer> serializer = new Mock<ODataFeedSerializer>(new DefaultODataSerializerProvider());
            serializer.CallBase = true;
            serializer.Setup(s => s.CreateODataFeed(instance, _customersType, _writeContext)).Returns(feed);
            var mockWriter = new Mock<ODataWriter>();

            mockWriter.Setup(m => m.WriteStart(It.Is<ODataFeed>(f => f.Count == 1000))).Verifiable();

            // Act
            serializer.Object.WriteObjectInline(instance, _customersType, mockWriter.Object, _writeContext);

            // Assert
            mockWriter.Verify();
        }

        [Fact]
        public void WriteObjectInline_Sets_NextPageLink_OnWriteEnd()
        {
            // Arrange
            IEnumerable instance = new object[0];
            ODataFeed feed = new ODataFeed { NextPageLink = new Uri("http://nextlink.com/") };
            Mock<ODataFeedSerializer> serializer = new Mock<ODataFeedSerializer>(new DefaultODataSerializerProvider());
            serializer.CallBase = true;
            serializer.Setup(s => s.CreateODataFeed(instance, _customersType, _writeContext)).Returns(feed);
            var mockWriter = new Mock<ODataWriter>();

            mockWriter.Setup(m => m.WriteStart(It.Is<ODataFeed>(f => f.NextPageLink == null))).Verifiable();
            mockWriter
                .Setup(m => m.WriteEnd())
                .Callback(() =>
                {
                    Assert.Equal("http://nextlink.com/", feed.NextPageLink.AbsoluteUri);
                })
                .Verifiable();

            // Act
            serializer.Object.WriteObjectInline(instance, _customersType, mockWriter.Object, _writeContext);

            // Assert
            mockWriter.Verify();
        }

        [Fact]
        public void CreateODataFeed_Sets_CountValueForPageResult()
        {
            // Arrange
            ODataFeedSerializer serializer = new ODataFeedSerializer(new DefaultODataSerializerProvider());
            Uri expectedNextLink = new Uri("http://nextlink.com");
            const long ExpectedCountValue = 1000;

            var result = new PageResult<Customer>(_customers, expectedNextLink, ExpectedCountValue);

            // Act
            ODataFeed feed = serializer.CreateODataFeed(result, _customersType, new ODataSerializerContext());

            // Assert
            Assert.Equal(ExpectedCountValue, feed.Count);
        }

        [Fact]
        public void CreateODataFeed_Sets_NextPageLinkForPageResult()
        {
            // Arrange
            ODataFeedSerializer serializer = new ODataFeedSerializer(new DefaultODataSerializerProvider());
            Uri expectedNextLink = new Uri("http://nextlink.com");
            const long ExpectedCountValue = 1000;

            var result = new PageResult<Customer>(_customers, expectedNextLink, ExpectedCountValue);

            // Act
            ODataFeed feed = serializer.CreateODataFeed(result, _customersType, new ODataSerializerContext());

            // Assert
            Assert.Equal(expectedNextLink, feed.NextPageLink);
        }

        [Fact]
        public void CreateODataFeed_Sets_CountValueFromContext()
        {
            // Arrange
            ODataFeedSerializer serializer = new ODataFeedSerializer(new DefaultODataSerializerProvider());
            const long ExpectedCountValue = 1000;
            HttpRequestMessage request = new HttpRequestMessage();
            request.ODataProperties().TotalCount = ExpectedCountValue;
            var result = new object[0];

            // Act
            ODataFeed feed = serializer.CreateODataFeed(result, _customersType, new ODataSerializerContext { Request = request });

            // Assert
            Assert.Equal(ExpectedCountValue, feed.Count);
        }

        [Fact]
        public void CreateODataFeed_Sets_NextPageLinkFromContext()
        {
            // Arrange
            ODataFeedSerializer serializer = new ODataFeedSerializer(new DefaultODataSerializerProvider());
            Uri expectedNextLink = new Uri("http://nextlink.com");
            HttpRequestMessage request = new HttpRequestMessage();
            request.ODataProperties().NextLink = expectedNextLink;
            var result = new object[0];

            // Act
            ODataFeed feed = serializer.CreateODataFeed(result, _customersType, new ODataSerializerContext { Request = request });

            // Assert
            Assert.Equal(expectedNextLink, feed.NextPageLink);
        }

        [Fact]
        public void CreateODataFeed_Ignores_NextPageLink_ForInnerFeeds()
        {
            // Arrange
            ODataFeedSerializer serializer = new ODataFeedSerializer(new DefaultODataSerializerProvider());
            Uri nextLink = new Uri("http://somelink");
            HttpRequestMessage request = new HttpRequestMessage();
            request.ODataProperties().NextLink = nextLink;
            var result = new object[0];
            IEdmNavigationProperty navProp = _customerSet.EntityType().NavigationProperties().First();
            SelectExpandClause selectExpandClause = new SelectExpandClause(new SelectItem[0], allSelected: true);
            EntityInstanceContext entity = new EntityInstanceContext
            {
                SerializerContext =
                    new ODataSerializerContext { Request = request, NavigationSource = _customerSet, Model = _model }
            };
            ODataSerializerContext nestedContext = new ODataSerializerContext(entity, selectExpandClause, navProp);

            // Act
            ODataFeed feed = serializer.CreateODataFeed(result, _customersType, nestedContext);

            // Assert
            Assert.Null(feed.NextPageLink);
        }

        [Fact]
        public void CreateODataFeed_Ignores_CountValue_ForInnerFeeds()
        {
            // Arrange
            ODataFeedSerializer serializer = new ODataFeedSerializer(new DefaultODataSerializerProvider());
            HttpRequestMessage request = new HttpRequestMessage();
            request.ODataProperties().TotalCount = 42;
            var result = new object[0];
            IEdmNavigationProperty navProp = _customerSet.EntityType().NavigationProperties().First();
            SelectExpandClause selectExpandClause = new SelectExpandClause(new SelectItem[0], allSelected: true);
            EntityInstanceContext entity = new EntityInstanceContext
            {
                SerializerContext =
                    new ODataSerializerContext { Request = request, NavigationSource = _customerSet, Model = _model }
            };
            ODataSerializerContext nestedContext = new ODataSerializerContext(entity, selectExpandClause, navProp);

            // Act
            ODataFeed feed = serializer.CreateODataFeed(result, _customersType, nestedContext);

            // Assert
            Assert.Null(feed.Count);
        }

        [Fact]
        public void CreateODataFeed_SetsNextPageLink_WhenWritingTruncatedCollection_ForExpandedProperties()
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            IEdmCollectionTypeReference customersType = new EdmCollectionTypeReference(new EdmCollectionType(model.Customer.AsReference()));
            ODataFeedSerializer serializer = new ODataFeedSerializer(new DefaultODataSerializerProvider());
            SelectExpandClause selectExpandClause = new SelectExpandClause(new SelectItem[0], allSelected: true);
            IEdmNavigationProperty ordersProperty = model.Customer.NavigationProperties().First();
            EntityInstanceContext entity = new EntityInstanceContext
            {
                SerializerContext = new ODataSerializerContext { NavigationSource = model.Customers, Model = model.Model }
            };
            ODataSerializerContext nestedContext = new ODataSerializerContext(entity, selectExpandClause, ordersProperty);
            TruncatedCollection<Order> orders = new TruncatedCollection<Order>(new[] { new Order(), new Order() }, pageSize: 1);

            Mock<NavigationSourceLinkBuilderAnnotation> linkBuilder = new Mock<NavigationSourceLinkBuilderAnnotation>();
            linkBuilder.Setup(l => l.BuildNavigationLink(entity, ordersProperty, ODataMetadataLevel.Default)).Returns(new Uri("http://navigation-link/"));
            model.Model.SetNavigationSourceLinkBuilder(model.Customers, linkBuilder.Object);
            model.Model.SetNavigationSourceLinkBuilder(model.Orders, new NavigationSourceLinkBuilderAnnotation());

            // Act
            ODataFeed feed = serializer.CreateODataFeed(orders, _customersType, nestedContext);

            // Assert
            Assert.Equal("http://navigation-link/?$skip=1", feed.NextPageLink.AbsoluteUri);
        }
    }
}
