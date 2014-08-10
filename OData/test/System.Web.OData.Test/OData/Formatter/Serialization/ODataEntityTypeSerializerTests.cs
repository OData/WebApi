// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Web.Http;
using System.Web.Http.Routing;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using System.Web.OData.Routing;
using System.Web.OData.Test;
using System.Web.OData.TestCommon;
using Microsoft.OData.Core;
using Microsoft.OData.Core.UriParser;
using Microsoft.OData.Core.UriParser.Semantic;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Annotations;
using Microsoft.OData.Edm.Library;
using Microsoft.TestCommon;
using Microsoft.TestCommon.Types;
using Moq;
using ODataPath = System.Web.OData.Routing.ODataPath;

namespace System.Web.OData.Formatter.Serialization
{
    public class ODataEntityTypeSerializerTests
    {
        private IEdmModel _model;
        private IEdmEntitySet _customerSet;
        private IEdmEntitySet _orderSet;
        private Customer _customer;
        private Order _order;
        private ODataEntityTypeSerializer _serializer;
        private ODataSerializerContext _writeContext;
        private EntityInstanceContext _entityInstanceContext;
        private ODataSerializerProvider _serializerProvider;
        private IEdmEntityTypeReference _customerType;
        private IEdmEntityTypeReference _orderType;
        private IEdmEntityTypeReference _specialCustomerType;
        private IEdmEntityTypeReference _specialOrderType;
        private ODataPath _path;

        public ODataEntityTypeSerializerTests()
        {
            _model = SerializationTestsHelpers.SimpleCustomerOrderModel();

            _model.SetAnnotationValue<ClrTypeAnnotation>(_model.FindType("Default.Customer"), new ClrTypeAnnotation(typeof(Customer)));
            _model.SetAnnotationValue<ClrTypeAnnotation>(_model.FindType("Default.Order"), new ClrTypeAnnotation(typeof(Order)));
            _model.SetAnnotationValue(
                _model.FindType("Default.SpecialCustomer"),
                new ClrTypeAnnotation(typeof(SpecialCustomer)));
            _model.SetAnnotationValue(
                _model.FindType("Default.SpecialOrder"),
                new ClrTypeAnnotation(typeof(SpecialOrder)));

            _customerSet = _model.EntityContainer.FindEntitySet("Customers");
            _customer = new Customer()
            {
                FirstName = "Foo",
                LastName = "Bar",
                ID = 10,
            };

            _orderSet = _model.EntityContainer.FindEntitySet("Orders");
            _order = new Order
            {
                ID = 20,
            };

            _serializerProvider = new DefaultODataSerializerProvider();
            _customerType = _model.GetEdmTypeReference(typeof(Customer)).AsEntity();
            _orderType = _model.GetEdmTypeReference(typeof(Order)).AsEntity();
            _specialCustomerType = _model.GetEdmTypeReference(typeof(SpecialCustomer)).AsEntity();
            _specialOrderType = _model.GetEdmTypeReference(typeof(SpecialOrder)).AsEntity();
            _serializer = new ODataEntityTypeSerializer(_serializerProvider);
            _path = new ODataPath(new EntitySetPathSegment(_customerSet));
            _writeContext = new ODataSerializerContext() { NavigationSource = _customerSet, Model = _model, Path = _path };
            _entityInstanceContext = new EntityInstanceContext(_writeContext, _customerSet.EntityType().AsReference(), _customer);
        }

        [Fact]
        public void Ctor_ThrowsArgumentNull_SerializerProvider()
        {
            Assert.ThrowsArgumentNull(
                () => new ODataEntityTypeSerializer(serializerProvider: null),
                "serializerProvider");
        }

        [Fact]
        public void WriteObject_ThrowsArgumentNull_MessageWriter()
        {
            Assert.ThrowsArgumentNull(
                () => _serializer.WriteObject(graph: _customer, type: typeof(Customer), messageWriter: null, writeContext: null),
                "messageWriter");
        }

        [Fact]
        public void WriteObject_ThrowsArgumentNull_WriteContext()
        {
            ODataMessageWriter messageWriter = new ODataMessageWriter(new Mock<IODataRequestMessage>().Object);
            Assert.ThrowsArgumentNull(
                () => _serializer.WriteObject(graph: _customer, type: typeof(Customer), messageWriter: messageWriter, writeContext: null),
                "writeContext");
        }

        [Fact]
        public void WriteObject_ThrowsSerializationException_WhenEntitySetIsMissingInWriteContext()
        {
            ODataMessageWriter messageWriter = new ODataMessageWriter(new Mock<IODataRequestMessage>().Object);
            Assert.Throws<SerializationException>(
                () => _serializer.WriteObject(graph: _customer, type: typeof(Customer), messageWriter: messageWriter, writeContext: new ODataSerializerContext()),
                "The related entity set or singleton cannot be found from the OData path. The related entity set or singleton is required to serialize the payload.");
        }

        [Fact]
        public void WriteObject_Calls_WriteObjectInline_WithRightEntityType()
        {
            // Arrange
            Mock<ODataEntityTypeSerializer> serializer = new Mock<ODataEntityTypeSerializer>(new DefaultODataSerializerProvider());
            serializer
                .Setup(s => s.WriteObjectInline(_customer, It.Is<IEdmTypeReference>(e => _customerType.Definition == e.Definition),
                    It.IsAny<ODataWriter>(), _writeContext))
                .Verifiable();
            serializer.Setup(s => s.CreateSelectExpandNode(It.IsAny<EntityInstanceContext>())).Returns(new SelectExpandNode());
            serializer.CallBase = true;

            // Act
            serializer.Object.WriteObject(_customer, typeof(int), ODataTestUtil.GetMockODataMessageWriter(), _writeContext);

            // Assert
            serializer.Verify();
        }

        [Fact]
        public void WriteObjectInline_ThrowsArgumentNull_Writer()
        {
            Assert.ThrowsArgumentNull(
                () => _serializer.WriteObjectInline(graph: null, expectedType: null, writer: null, writeContext: new ODataSerializerContext()),
                "writer");
        }

        [Fact]
        public void WriteObjectInline_ThrowsArgumentNull_WriteContext()
        {
            Assert.ThrowsArgumentNull(
                () => _serializer.WriteObjectInline(graph: null, expectedType: null, writer: new Mock<ODataWriter>().Object, writeContext: null),
                "writeContext");
        }

        [Fact]
        public void WriteObjectInline_ThrowsSerializationException_WhenGraphIsNull()
        {
            ODataWriter messageWriter = new Mock<ODataWriter>().Object;
            Assert.Throws<SerializationException>(
                () => _serializer.WriteObjectInline(graph: null, expectedType: null, writer: messageWriter, writeContext: new ODataSerializerContext()),
                "Cannot serialize a null 'entry'.");
        }

        [Fact]
        public void WriteObjectInline_Calls_CreateSelectExpandNode()
        {
            // Arrange
            Mock<ODataEntityTypeSerializer> serializer = new Mock<ODataEntityTypeSerializer>(_serializerProvider);
            ODataWriter writer = new Mock<ODataWriter>().Object;

            serializer.Setup(s => s.CreateSelectExpandNode(It.Is<EntityInstanceContext>(e => Verify(e, _customer, _writeContext)))).Verifiable();
            serializer.CallBase = true;

            // Act
            serializer.Object.WriteObjectInline(_customer, _customerType, writer, _writeContext);

            // Assert
            serializer.Verify();
        }

        [Fact]
        public void WriteObjectInline_Calls_CreateEntry()
        {
            // Arrange
            SelectExpandNode selectExpandNode = new SelectExpandNode();
            Mock<ODataEntityTypeSerializer> serializer = new Mock<ODataEntityTypeSerializer>(_serializerProvider);
            ODataWriter writer = new Mock<ODataWriter>().Object;

            serializer.Setup(s => s.CreateSelectExpandNode(It.IsAny<EntityInstanceContext>())).Returns(selectExpandNode);
            serializer.Setup(s => s.CreateEntry(selectExpandNode, It.Is<EntityInstanceContext>(e => Verify(e, _customer, _writeContext)))).Verifiable();
            serializer.CallBase = true;

            // Act
            serializer.Object.WriteObjectInline(_customer, _customerType, writer, _writeContext);

            // Assert
            serializer.Verify();
        }

        [Fact]
        public void WriteObjectInline_WritesODataEntryFrom_CreateEntry()
        {
            // Arrange
            ODataEntry entry = new ODataEntry();
            Mock<ODataEntityTypeSerializer> serializer = new Mock<ODataEntityTypeSerializer>(_serializerProvider);
            Mock<ODataWriter> writer = new Mock<ODataWriter>();

            serializer.Setup(s => s.CreateEntry(It.IsAny<SelectExpandNode>(), It.IsAny<EntityInstanceContext>())).Returns(entry);
            serializer.CallBase = true;

            writer.Setup(s => s.WriteStart(entry)).Verifiable();

            // Act
            serializer.Object.WriteObjectInline(_customer, _customerType, writer.Object, _writeContext);

            // Assert
            writer.Verify();
        }

        [Fact]
        public void WriteObjectInline_Calls_CreateNavigationLink_ForEachSelectedNavigationProperty()
        {
            // Arrange
            SelectExpandNode selectExpandNode = new SelectExpandNode
            {
                SelectedNavigationProperties =
                {
                    new Mock<IEdmNavigationProperty>().Object,
                    new Mock<IEdmNavigationProperty>().Object
                }
            };
            Mock<ODataWriter> writer = new Mock<ODataWriter>();
            Mock<ODataEntityTypeSerializer> serializer = new Mock<ODataEntityTypeSerializer>(_serializerProvider);
            serializer.Setup(s => s.CreateSelectExpandNode(It.IsAny<EntityInstanceContext>())).Returns(selectExpandNode);
            serializer.CallBase = true;

            serializer.Setup(s => s.CreateNavigationLink(selectExpandNode.SelectedNavigationProperties.ElementAt(0), It.IsAny<EntityInstanceContext>())).Verifiable();
            serializer.Setup(s => s.CreateNavigationLink(selectExpandNode.SelectedNavigationProperties.ElementAt(1), It.IsAny<EntityInstanceContext>())).Verifiable();

            // Act
            serializer.Object.WriteObjectInline(_customer, _customerType, writer.Object, _writeContext);

            // Assert
            serializer.Verify();
        }

        [Fact]
        public void WriteObjectInline_WritesNavigationLinksReturnedBy_CreateNavigationLink_ForEachSelectedNavigationProperty()
        {
            // Arrange
            SelectExpandNode selectExpandNode = new SelectExpandNode
            {
                SelectedNavigationProperties =
                {
                    new Mock<IEdmNavigationProperty>().Object,
                    new Mock<IEdmNavigationProperty>().Object
                }
            };
            ODataNavigationLink[] navigationLinks = new[]
            {
                new ODataNavigationLink(),
                new ODataNavigationLink()
            };
            Mock<ODataEntityTypeSerializer> serializer = new Mock<ODataEntityTypeSerializer>(_serializerProvider);
            serializer.Setup(s => s.CreateSelectExpandNode(It.IsAny<EntityInstanceContext>())).Returns(selectExpandNode);
            serializer
                .Setup(s => s.CreateNavigationLink(selectExpandNode.SelectedNavigationProperties.ElementAt(0), It.IsAny<EntityInstanceContext>()))
                .Returns(navigationLinks[0]);
            serializer
                .Setup(s => s.CreateNavigationLink(selectExpandNode.SelectedNavigationProperties.ElementAt(1), It.IsAny<EntityInstanceContext>()))
                .Returns(navigationLinks[1]);
            serializer.CallBase = true;

            Mock<ODataWriter> writer = new Mock<ODataWriter>();
            writer.Setup(w => w.WriteStart(navigationLinks[0])).Verifiable();
            writer.Setup(w => w.WriteStart(navigationLinks[1])).Verifiable();

            // Act
            serializer.Object.WriteObjectInline(_customer, _customerType, writer.Object, _writeContext);

            // Assert
            writer.Verify();
        }

        [Fact]
        public void WriteObjectInline_Calls_CreateNavigationLink_ForEachExpandedNavigationProperty()
        {
            // Arrange
            SelectExpandNode selectExpandNode = new SelectExpandNode
            {
                ExpandedNavigationProperties =
                {
                    { new Mock<IEdmNavigationProperty>().Object, null },
                    { new Mock<IEdmNavigationProperty>().Object, null }
                }
            };
            Mock<ODataWriter> writer = new Mock<ODataWriter>();
            Mock<ODataEntityTypeSerializer> serializer = new Mock<ODataEntityTypeSerializer>(_serializerProvider);
            serializer.Setup(s => s.CreateSelectExpandNode(It.IsAny<EntityInstanceContext>())).Returns(selectExpandNode);
            var expandedNavigationProperties = selectExpandNode.ExpandedNavigationProperties.ToList();

            serializer.Setup(s => s.CreateNavigationLink(expandedNavigationProperties[0].Key, It.IsAny<EntityInstanceContext>())).Verifiable();
            serializer.Setup(s => s.CreateNavigationLink(expandedNavigationProperties[1].Key, It.IsAny<EntityInstanceContext>())).Verifiable();
            serializer.CallBase = true;

            // Act
            serializer.Object.WriteObjectInline(_customer, _customerType, writer.Object, _writeContext);

            // Assert
            serializer.Verify();
        }

        [Fact]
        public void WriteObjectInline_ExpandsUsingInnerSerializerUsingRightContext_ExpandedNavigationProperties()
        {
            // Arrange
            IEdmEntityType customerType = _customerSet.EntityType();
            IEdmNavigationProperty ordersProperty = customerType.NavigationProperties().Single(p => p.Name == "Orders");

            ODataQueryOptionParser parser = new ODataQueryOptionParser(_model, customerType, _customerSet,
                new Dictionary<string, string> { { "$select", "Orders" }, { "$expand", "Orders" } });
            SelectExpandClause selectExpandClause = parser.ParseSelectAndExpand();

            SelectExpandNode selectExpandNode = new SelectExpandNode
            {
                ExpandedNavigationProperties = 
                { 
                    { ordersProperty, selectExpandClause.SelectedItems.OfType<ExpandedNavigationSelectItem>().Single().SelectAndExpand }
                }
            };
            Mock<ODataWriter> writer = new Mock<ODataWriter>();

            Mock<ODataEdmTypeSerializer> innerSerializer = new Mock<ODataEdmTypeSerializer>(ODataPayloadKind.Entry);
            innerSerializer
                .Setup(s => s.WriteObjectInline(_customer.Orders, ordersProperty.Type, writer.Object, It.IsAny<ODataSerializerContext>()))
                .Callback((object o, IEdmTypeReference t, ODataWriter w, ODataSerializerContext context) =>
                    {
                        Assert.Same(context.NavigationSource.Name, "Orders");
                        Assert.Same(context.SelectExpandClause, selectExpandNode.ExpandedNavigationProperties.Single().Value);
                    })
                .Verifiable();

            Mock<ODataSerializerProvider> serializerProvider = new Mock<ODataSerializerProvider>();
            serializerProvider.Setup(p => p.GetEdmTypeSerializer(ordersProperty.Type))
                .Returns(innerSerializer.Object);
            Mock<ODataEntityTypeSerializer> serializer = new Mock<ODataEntityTypeSerializer>(serializerProvider.Object);
            serializer.Setup(s => s.CreateSelectExpandNode(It.IsAny<EntityInstanceContext>())).Returns(selectExpandNode);
            serializer.CallBase = true;
            _writeContext.SelectExpandClause = selectExpandClause;

            // Act
            serializer.Object.WriteObjectInline(_customer, _customerType, writer.Object, _writeContext);

            // Assert
            innerSerializer.Verify();
            // check that the context is rolled back
            Assert.Same(_writeContext.NavigationSource.Name, "Customers");
            Assert.Same(_writeContext.SelectExpandClause, selectExpandClause);
        }

        [Fact]
        public void WriteObjectInline_CanExpandNavigationProperty_ContainingEdmObject()
        {
            // Arrange
            IEdmEntityType customerType = _customerSet.EntityType();
            IEdmNavigationProperty ordersProperty = customerType.NavigationProperties().Single(p => p.Name == "Orders");

            Mock<IEdmObject> orders = new Mock<IEdmObject>();
            orders.Setup(o => o.GetEdmType()).Returns(ordersProperty.Type);
            object ordersValue = orders.Object;

            Mock<IEdmEntityObject> customer = new Mock<IEdmEntityObject>();
            customer.Setup(c => c.TryGetPropertyValue("Orders", out ordersValue)).Returns(true);
            customer.Setup(c => c.GetEdmType()).Returns(customerType.AsReference());

            ODataQueryOptionParser parser = new ODataQueryOptionParser(_model, customerType, _customerSet,
                new Dictionary<string, string> { { "$select", "Orders" }, { "$expand", "Orders" } });
            SelectExpandClause selectExpandClause = parser.ParseSelectAndExpand();

            SelectExpandNode selectExpandNode = new SelectExpandNode();
            selectExpandNode.ExpandedNavigationProperties[ordersProperty] = selectExpandClause.SelectedItems.OfType<ExpandedNavigationSelectItem>().Single().SelectAndExpand;

            Mock<ODataWriter> writer = new Mock<ODataWriter>();

            Mock<ODataEdmTypeSerializer> ordersSerializer = new Mock<ODataEdmTypeSerializer>(ODataPayloadKind.Entry);
            ordersSerializer.Setup(s => s.WriteObjectInline(ordersValue, ordersProperty.Type, writer.Object, It.IsAny<ODataSerializerContext>())).Verifiable();

            Mock<ODataSerializerProvider> serializerProvider = new Mock<ODataSerializerProvider>();
            serializerProvider.Setup(p => p.GetEdmTypeSerializer(ordersProperty.Type)).Returns(ordersSerializer.Object);

            Mock<ODataEntityTypeSerializer> serializer = new Mock<ODataEntityTypeSerializer>(serializerProvider.Object);
            serializer.Setup(s => s.CreateSelectExpandNode(It.IsAny<EntityInstanceContext>())).Returns(selectExpandNode);
            serializer.CallBase = true;

            // Act
            serializer.Object.WriteObjectInline(customer.Object, _customerType, writer.Object, _writeContext);

            //Assert
            ordersSerializer.Verify();
        }

        [Fact]
        public void WriteObjectInline_CanWriteExpandedNavigationProperty_ExpandedCollectionValuedNavigationPropertyIsNull()
        {
            // Arrange
            IEdmEntityType customerType = _customerSet.EntityType();
            IEdmNavigationProperty ordersProperty = customerType.NavigationProperties().Single(p => p.Name == "Orders");

            Mock<IEdmEntityObject> customer = new Mock<IEdmEntityObject>();
            object ordersValue = null;
            customer.Setup(c => c.TryGetPropertyValue("Orders", out ordersValue)).Returns(true);
            customer.Setup(c => c.GetEdmType()).Returns(customerType.AsReference());

            ODataQueryOptionParser parser = new ODataQueryOptionParser(_model, customerType, _customerSet,
                new Dictionary<string, string> { { "$select", "Orders" }, { "$expand", "Orders" } });
            SelectExpandClause selectExpandClause = parser.ParseSelectAndExpand();

            SelectExpandNode selectExpandNode = new SelectExpandNode();
            selectExpandNode.ExpandedNavigationProperties[ordersProperty] =
                selectExpandClause.SelectedItems.OfType<ExpandedNavigationSelectItem>().Single().SelectAndExpand;

            Mock<ODataWriter> writer = new Mock<ODataWriter>();
            writer.Setup(w => w.WriteStart(It.IsAny<ODataFeed>())).Callback(
                (ODataFeed feed) =>
                {
                    Assert.Null(feed.Count);
                    Assert.Null(feed.DeltaLink);
                    Assert.Null(feed.Id);
                    Assert.Empty(feed.InstanceAnnotations);
                    Assert.Null(feed.NextPageLink);
                }).Verifiable();
            Mock<ODataEdmTypeSerializer> ordersSerializer = new Mock<ODataEdmTypeSerializer>(ODataPayloadKind.Entry);
            Mock<ODataSerializerProvider> serializerProvider = new Mock<ODataSerializerProvider>();
            serializerProvider.Setup(p => p.GetEdmTypeSerializer(ordersProperty.Type)).Returns(ordersSerializer.Object);

            Mock<ODataEntityTypeSerializer> serializer = new Mock<ODataEntityTypeSerializer>(serializerProvider.Object);
            serializer.Setup(s => s.CreateSelectExpandNode(It.IsAny<EntityInstanceContext>())).Returns(selectExpandNode);
            serializer.CallBase = true;

            // Act
            serializer.Object.WriteObjectInline(customer.Object, _customerType, writer.Object, _writeContext);

            // Assert
            writer.Verify();
        }

        [Fact]
        public void WriteObjectInline_CanWriteExpandedNavigationProperty_ExpandedSingleValuedNavigationPropertyIsNull()
        {
            // Arrange
            IEdmEntityType orderType = _orderSet.EntityType();
            IEdmNavigationProperty customerProperty = orderType.NavigationProperties().Single(p => p.Name == "Customer");

            Mock<IEdmEntityObject> order = new Mock<IEdmEntityObject>();
            object customerValue = null;
            order.Setup(c => c.TryGetPropertyValue("Customer", out customerValue)).Returns(true);
            order.Setup(c => c.GetEdmType()).Returns(orderType.AsReference());

            ODataQueryOptionParser parser = new ODataQueryOptionParser(_model, orderType, _orderSet,
                new Dictionary<string, string> { { "$select", "Customer" }, { "$expand", "Customer" } });
            SelectExpandClause selectExpandClause = parser.ParseSelectAndExpand();

            SelectExpandNode selectExpandNode = new SelectExpandNode();
            selectExpandNode.ExpandedNavigationProperties[customerProperty] =
                selectExpandClause.SelectedItems.OfType<ExpandedNavigationSelectItem>().Single().SelectAndExpand;

            Mock<ODataWriter> writer = new Mock<ODataWriter>();

            writer.Setup(w => w.WriteStart(null as ODataEntry)).Verifiable();
            Mock<ODataEdmTypeSerializer> ordersSerializer = new Mock<ODataEdmTypeSerializer>(ODataPayloadKind.Entry);
            Mock<ODataSerializerProvider> serializerProvider = new Mock<ODataSerializerProvider>();
            serializerProvider.Setup(p => p.GetEdmTypeSerializer(customerProperty.Type))
                .Returns(ordersSerializer.Object);

            Mock<ODataEntityTypeSerializer> serializer = new Mock<ODataEntityTypeSerializer>(serializerProvider.Object);
            serializer.Setup(s => s.CreateSelectExpandNode(It.IsAny<EntityInstanceContext>())).Returns(selectExpandNode);
            serializer.CallBase = true;

            // Act
            serializer.Object.WriteObjectInline(order.Object, _orderType, writer.Object, _writeContext);

            // Assert
            writer.Verify();
        }

        [Fact]
        public void WriteObjectInline_CanWriteExpandedNavigationProperty_DerivedExpandedCollectionValuedNavigationPropertyIsNull()
        {
            // Arrange
            IEdmEntityType specialCustomerType = (IEdmEntityType)_specialCustomerType.Definition;
            IEdmNavigationProperty specialOrdersProperty =
                specialCustomerType.NavigationProperties().Single(p => p.Name == "SpecialOrders");

            Mock<IEdmEntityObject> customer = new Mock<IEdmEntityObject>();
            object specialOrdersValue = null;
            customer.Setup(c => c.TryGetPropertyValue("SpecialOrders", out specialOrdersValue)).Returns(true);
            customer.Setup(c => c.GetEdmType()).Returns(_specialCustomerType);

            IEdmEntityType customerType = _customerSet.EntityType();
            ODataQueryOptionParser parser = new ODataQueryOptionParser(
                _model,
                customerType,
                _customerSet,
                new Dictionary<string, string>
                {
                    { "$select", "Default.SpecialCustomer/SpecialOrders" },
                    { "$expand", "Default.SpecialCustomer/SpecialOrders" }
                });
            SelectExpandClause selectExpandClause = parser.ParseSelectAndExpand();

            SelectExpandNode selectExpandNode = new SelectExpandNode();
            selectExpandNode.ExpandedNavigationProperties[specialOrdersProperty] =
                selectExpandClause.SelectedItems.OfType<ExpandedNavigationSelectItem>().Single().SelectAndExpand;

            Mock<ODataWriter> writer = new Mock<ODataWriter>();
            writer.Setup(w => w.WriteStart(It.IsAny<ODataFeed>())).Callback(
                (ODataFeed feed) =>
                {
                    Assert.Null(feed.Count);
                    Assert.Null(feed.DeltaLink);
                    Assert.Null(feed.Id);
                    Assert.Empty(feed.InstanceAnnotations);
                    Assert.Null(feed.NextPageLink);
                }).Verifiable();
            Mock<ODataEdmTypeSerializer> ordersSerializer = new Mock<ODataEdmTypeSerializer>(ODataPayloadKind.Entry);
            Mock<ODataSerializerProvider> serializerProvider = new Mock<ODataSerializerProvider>();
            serializerProvider.Setup(p => p.GetEdmTypeSerializer(specialOrdersProperty.Type))
                .Returns(ordersSerializer.Object);

            Mock<ODataEntityTypeSerializer> serializer = new Mock<ODataEntityTypeSerializer>(serializerProvider.Object);
            serializer.Setup(s => s.CreateSelectExpandNode(It.IsAny<EntityInstanceContext>())).Returns(selectExpandNode);
            serializer.CallBase = true;

            // Act
            serializer.Object.WriteObjectInline(customer.Object, _customerType, writer.Object, _writeContext);

            // Assert
            writer.Verify();
        }

        [Fact]
        public void WriteObjectInline_CanWriteExpandedNavigationProperty_DerivedExpandedSingleValuedNavigationPropertyIsNull()
        {
            // Arrange
            IEdmEntityType specialOrderType = (IEdmEntityType)_specialOrderType.Definition;
            IEdmNavigationProperty customerProperty =
                specialOrderType.NavigationProperties().Single(p => p.Name == "SpecialCustomer");

            Mock<IEdmEntityObject> order = new Mock<IEdmEntityObject>();
            object customerValue = null;
            order.Setup(c => c.TryGetPropertyValue("SpecialCustomer", out customerValue)).Returns(true);
            order.Setup(c => c.GetEdmType()).Returns(_specialOrderType);

            IEdmEntityType orderType = (IEdmEntityType)_orderType.Definition;
            ODataQueryOptionParser parser = new ODataQueryOptionParser(
                _model,
                orderType,
                _orderSet,
                new Dictionary<string, string>
                {
                    { "$select", "Default.SpecialOrder/SpecialCustomer" },
                    { "$expand", "Default.SpecialOrder/SpecialCustomer" }
                });
            SelectExpandClause selectExpandClause = parser.ParseSelectAndExpand();

            SelectExpandNode selectExpandNode = new SelectExpandNode();
            selectExpandNode.ExpandedNavigationProperties[customerProperty] =
                selectExpandClause.SelectedItems.OfType<ExpandedNavigationSelectItem>().Single().SelectAndExpand;

            Mock<ODataWriter> writer = new Mock<ODataWriter>();

            writer.Setup(w => w.WriteStart(null as ODataEntry)).Verifiable();
            Mock<ODataEdmTypeSerializer> ordersSerializer = new Mock<ODataEdmTypeSerializer>(ODataPayloadKind.Entry);
            Mock<ODataSerializerProvider> serializerProvider = new Mock<ODataSerializerProvider>();
            serializerProvider.Setup(p => p.GetEdmTypeSerializer(customerProperty.Type))
                .Returns(ordersSerializer.Object);

            Mock<ODataEntityTypeSerializer> serializer = new Mock<ODataEntityTypeSerializer>(serializerProvider.Object);
            serializer.Setup(s => s.CreateSelectExpandNode(It.IsAny<EntityInstanceContext>())).Returns(selectExpandNode);
            serializer.CallBase = true;

            // Act
            serializer.Object.WriteObjectInline(order.Object, _orderType, writer.Object, _writeContext);

            // Assert
            writer.Verify();
        }

        [Fact]
        public void CreateEntry_ThrowsArgumentNull_SelectExpandNode()
        {
            Assert.ThrowsArgumentNull(
                () => _serializer.CreateEntry(selectExpandNode: null, entityInstanceContext: _entityInstanceContext),
                "selectExpandNode");
        }

        [Fact]
        public void CreateEntry_ThrowsArgumentNull_EntityInstanceContext()
        {
            Assert.ThrowsArgumentNull(
                () => _serializer.CreateEntry(new SelectExpandNode(), entityInstanceContext: null),
                "entityInstanceContext");
        }

        [Fact]
        public void CreateEntry_Calls_CreateStructuralProperty_ForEachSelectedStructuralProperty()
        {
            // Arrange
            SelectExpandNode selectExpandNode = new SelectExpandNode
            {
                SelectedStructuralProperties = { new Mock<IEdmStructuralProperty>().Object, new Mock<IEdmStructuralProperty>().Object }
            };
            ODataProperty[] properties = new ODataProperty[] { new ODataProperty(), new ODataProperty() };
            Mock<ODataEntityTypeSerializer> serializer = new Mock<ODataEntityTypeSerializer>(_serializerProvider);
            serializer.CallBase = true;

            serializer
                .Setup(s => s.CreateStructuralProperty(selectExpandNode.SelectedStructuralProperties.ElementAt(0), _entityInstanceContext))
                .Returns(properties[0])
                .Verifiable();
            serializer
                .Setup(s => s.CreateStructuralProperty(selectExpandNode.SelectedStructuralProperties.ElementAt(1), _entityInstanceContext))
                .Returns(properties[1])
                .Verifiable();

            // Act
            ODataEntry entry = serializer.Object.CreateEntry(selectExpandNode, _entityInstanceContext);

            // Assert
            serializer.Verify();
            Assert.Equal(properties, entry.Properties);
        }

        [Fact]
        public void CreateEntry_SetsETagToNull_IfRequestIsNull()
        {
            // Arrange
            SelectExpandNode selectExpandNode = new SelectExpandNode
            {
                SelectedStructuralProperties = { new Mock<IEdmStructuralProperty>().Object, new Mock<IEdmStructuralProperty>().Object }
            };
            ODataProperty[] properties = new[] { new ODataProperty(), new ODataProperty() };
            Mock<ODataEntityTypeSerializer> serializer = new Mock<ODataEntityTypeSerializer>(_serializerProvider);
            serializer.CallBase = true;

            serializer
                .Setup(s => s.CreateStructuralProperty(selectExpandNode.SelectedStructuralProperties.ElementAt(0), _entityInstanceContext))
                .Returns(properties[0]);
            serializer
                .Setup(s => s.CreateStructuralProperty(selectExpandNode.SelectedStructuralProperties.ElementAt(1), _entityInstanceContext))
                .Returns(properties[1]);

            // Act
            ODataEntry entry = serializer.Object.CreateEntry(selectExpandNode, _entityInstanceContext);

            // Assert
            Assert.Null(entry.ETag);
        }

        [Fact]
        public void CreateEntry_SetsETagToNull_IfModelDontHaveConcurrencyProperty()
        {
            // Arrange
            IEdmEntitySet orderSet = _model.EntityContainer.FindEntitySet("Orders");
            Order order = new Order()
            {
                Name = "Foo",
                Shipment = "Bar",
                ID = 10,
            };

            _writeContext.NavigationSource = orderSet;
            _entityInstanceContext = new EntityInstanceContext(_writeContext, orderSet.EntityType().AsReference(), order);

            SelectExpandNode selectExpandNode = new SelectExpandNode
            {
                SelectedStructuralProperties = { new Mock<IEdmStructuralProperty>().Object, new Mock<IEdmStructuralProperty>().Object }
            };
            ODataProperty[] properties = new[] { new ODataProperty(), new ODataProperty() };
            Mock<ODataEntityTypeSerializer> serializer = new Mock<ODataEntityTypeSerializer>(_serializerProvider);
            serializer.CallBase = true;

            serializer
                .Setup(s => s.CreateStructuralProperty(selectExpandNode.SelectedStructuralProperties.ElementAt(0), _entityInstanceContext))
                .Returns(properties[0]);
            serializer
                .Setup(s => s.CreateStructuralProperty(selectExpandNode.SelectedStructuralProperties.ElementAt(1), _entityInstanceContext))
                .Returns(properties[1]);

            MockHttpRequestMessage request = new MockHttpRequestMessage();
            request.SetConfiguration(new HttpConfiguration());
            _entityInstanceContext.Request = request;

            // Act
            ODataEntry entry = serializer.Object.CreateEntry(selectExpandNode, _entityInstanceContext);

            // Assert
            Assert.Null(entry.ETag);
        }

        [Fact]
        public void CreateEntry_SetsEtagToNotNull_IfWithConcurrencyProperty()
        {
            // Arrange
            Mock<IEdmStructuralProperty> mockConcurrencyProperty = new Mock<IEdmStructuralProperty>();
            mockConcurrencyProperty.SetupGet(s => s.ConcurrencyMode).Returns(EdmConcurrencyMode.Fixed);
            mockConcurrencyProperty.SetupGet(s => s.Name).Returns("City");
            SelectExpandNode selectExpandNode = new SelectExpandNode
            {
                SelectedStructuralProperties = { new Mock<IEdmStructuralProperty>().Object, mockConcurrencyProperty.Object }
            };
            ODataProperty[] properties = new[] { new ODataProperty(), new ODataProperty() };
            Mock<ODataEntityTypeSerializer> serializer = new Mock<ODataEntityTypeSerializer>(_serializerProvider);
            serializer.CallBase = true;
            serializer
                .Setup(s => s.CreateStructuralProperty(selectExpandNode.SelectedStructuralProperties.ElementAt(0), _entityInstanceContext))
                .Returns(properties[0]);
            serializer
                .Setup(s => s.CreateStructuralProperty(selectExpandNode.SelectedStructuralProperties.ElementAt(1), _entityInstanceContext))
                .Returns(properties[1]);

            MockHttpRequestMessage request = new MockHttpRequestMessage();
            HttpConfiguration configuration = new HttpConfiguration();
            Mock<IETagHandler> mockETagHandler = new Mock<IETagHandler>();
            string tag = "\"'anycity'\"";
            EntityTagHeaderValue etagHeaderValue = new EntityTagHeaderValue(tag, isWeak: true);
            mockETagHandler.Setup(e => e.CreateETag(It.IsAny<IDictionary<string, object>>())).Returns(etagHeaderValue);
            configuration.SetETagHandler(mockETagHandler.Object);
            request.SetConfiguration(configuration);
            _entityInstanceContext.Request = request;

            // Act
            ODataEntry entry = serializer.Object.CreateEntry(selectExpandNode, _entityInstanceContext);

            // Assert
            Assert.Equal(etagHeaderValue.ToString(), entry.ETag);
        }

        [Fact]
        public void CreateEntry_IgnoresProperty_IfCreateStructuralPropertyReturnsNull()
        {
            // Arrange
            SelectExpandNode selectExpandNode = new SelectExpandNode
            {
                SelectedStructuralProperties = { new Mock<IEdmStructuralProperty>().Object }
            };
            Mock<ODataEntityTypeSerializer> serializer = new Mock<ODataEntityTypeSerializer>(_serializerProvider);
            serializer.CallBase = true;

            serializer
                .Setup(s => s.CreateStructuralProperty(selectExpandNode.SelectedStructuralProperties.ElementAt(0), _entityInstanceContext))
                .Returns<ODataProperty>(null);

            // Act
            ODataEntry entry = serializer.Object.CreateEntry(selectExpandNode, _entityInstanceContext);

            // Assert
            serializer.Verify();
            Assert.Empty(entry.Properties);
        }

        [Fact]
        public void CreateEntry_Calls_CreateODataAction_ForEachSelectAction()
        {
            // Arrange
            ODataAction[] actions = new ODataAction[] { new ODataAction(), new ODataAction() };
            SelectExpandNode selectExpandNode = new SelectExpandNode
            {
                SelectedActions = { new Mock<IEdmAction>().Object, new Mock<IEdmAction>().Object }
            };
            Mock<ODataEntityTypeSerializer> serializer = new Mock<ODataEntityTypeSerializer>(_serializerProvider);
            serializer.CallBase = true;

            serializer.Setup(s => s.CreateODataAction(selectExpandNode.SelectedActions.ElementAt(0), _entityInstanceContext)).Returns(actions[0]).Verifiable();
            serializer.Setup(s => s.CreateODataAction(selectExpandNode.SelectedActions.ElementAt(1), _entityInstanceContext)).Returns(actions[1]).Verifiable();

            // Act
            ODataEntry entry = serializer.Object.CreateEntry(selectExpandNode, _entityInstanceContext);

            // Assert
            Assert.Equal(actions, entry.Actions);
            serializer.Verify();
        }

        [Fact]
        public void CreateEntry_Works_ToAppendDynamicProperties_ForOpenEntityType()
        {
            // Arrange
            IEdmModel model = SerializationTestsHelpers.SimpleOpenTypeModel();

            IEdmEntitySet customers = model.EntityContainer.FindEntitySet("Customers");

            IEdmEntityType customerType = model.FindDeclaredType("Default.Customer") as IEdmEntityType;
            Type simpleOpenCustomer = typeof(SimpleOpenCustomer);
            model.SetAnnotationValue(customerType, new ClrTypeAnnotation(simpleOpenCustomer));

            IEdmComplexType addressType = model.FindDeclaredType("Default.Address") as IEdmComplexType;
            Type simpleOpenAddress = typeof(SimpleOpenAddress);
            model.SetAnnotationValue(addressType, new ClrTypeAnnotation(simpleOpenAddress));

            IEdmEnumType enumType = model.FindDeclaredType("Default.SimpleEnum") as IEdmEnumType;
            Type simpleEnumType = typeof(SimpleEnum);
            model.SetAnnotationValue(enumType, new ClrTypeAnnotation(simpleEnumType));

            model.SetAnnotationValue(customerType, new DynamicPropertyDictionaryAnnotation(
                simpleOpenCustomer.GetProperty("CustomerProperties")));

            model.SetAnnotationValue(addressType, new DynamicPropertyDictionaryAnnotation(
                simpleOpenAddress.GetProperty("Properties")));

            ODataSerializerProvider serializerProvider = new DefaultODataSerializerProvider();
            ODataEntityTypeSerializer serializer = new ODataEntityTypeSerializer(serializerProvider);

            SelectExpandNode selectExpandNode = new SelectExpandNode(null, customerType, model);
            ODataSerializerContext writeContext = new ODataSerializerContext
            {
                Model = model,
                Path = new ODataPath(new EntitySetPathSegment(customers))
            };

            SimpleOpenCustomer customer = new SimpleOpenCustomer()
            {
                CustomerId = 991,
                Name = "Name #991",
                Address = new SimpleOpenAddress
                {
                    City = "a city",
                    Street = "a street",
                    Properties = new Dictionary<string, object> { {"ArrayProperty", new[] { "15", "14", "13" } } }
                },
                CustomerProperties = new Dictionary<string, object>()
            };
            customer.CustomerProperties.Add("EnumProperty", SimpleEnum.Fourth);
            customer.CustomerProperties.Add("GuidProperty", new Guid("181D3A20-B41A-489F-9F15-F91F0F6C9ECA"));
            customer.CustomerProperties.Add("ListProperty", new List<int>{5,4,3,2,1});

            EntityInstanceContext entityInstanceContext = new EntityInstanceContext(writeContext,
                customerType.ToEdmTypeReference(false) as IEdmEntityTypeReference, customer);

            // Act
            ODataEntry entry = serializer.CreateEntry(selectExpandNode, entityInstanceContext);

            // Assert
            Assert.Equal(entry.TypeName, "Default.Customer");
            Assert.Equal(6, entry.Properties.Count());

            // Verify the declared properties
            ODataProperty street = Assert.Single(entry.Properties.Where(p => p.Name == "CustomerId"));
            Assert.Equal(991, street.Value);

            ODataProperty city = Assert.Single(entry.Properties.Where(p => p.Name == "Name"));
            Assert.Equal("Name #991", city.Value);

            // Verify the nested open complex property
            ODataProperty address = Assert.Single(entry.Properties.Where(p => p.Name == "Address"));
            ODataComplexValue addressComplexValue = Assert.IsType<ODataComplexValue>(address.Value);
            ODataProperty addressDynamicProperty =
                Assert.Single(addressComplexValue.Properties.Where(p => p.Name == "ArrayProperty"));
            ODataCollectionValue addressCollectionValue =
                Assert.IsType<ODataCollectionValue>(addressDynamicProperty.Value);
            Assert.Equal(new[] { "15", "14", "13" }, addressCollectionValue.Items.OfType<string>().ToList());
            Assert.Equal("Collection(Edm.String)", addressCollectionValue.TypeName);

            // Verify the dynamic properties
            ODataProperty enumProperty = Assert.Single(entry.Properties.Where(p => p.Name == "EnumProperty"));
            ODataEnumValue enumValue = Assert.IsType<ODataEnumValue>(enumProperty.Value);
            Assert.Equal("Fourth", enumValue.Value);
            Assert.Equal("Default.SimpleEnum", enumValue.TypeName);

            ODataProperty guidProperty = Assert.Single(entry.Properties.Where(p => p.Name == "GuidProperty"));
            Assert.Equal(new Guid("181D3A20-B41A-489F-9F15-F91F0F6C9ECA"), guidProperty.Value);

            ODataProperty listProperty = Assert.Single(entry.Properties.Where(p => p.Name == "ListProperty"));
            ODataCollectionValue collectionValue = Assert.IsType<ODataCollectionValue>(listProperty.Value);
            Assert.Equal(new List<int>{5,4,3,2,1}, collectionValue.Items.OfType<int>().ToList());
            Assert.Equal("Collection(Edm.Int32)", collectionValue.TypeName);
        }

        [Fact]
        public void CreateStructuralProperty_ThrowsArgumentNull_StructuralProperty()
        {
            Assert.ThrowsArgumentNull(
                () => _serializer.CreateStructuralProperty(structuralProperty: null, entityInstanceContext: null),
                "structuralProperty");
        }

        [Fact]
        public void CreateStructuralProperty_ThrowsArgumentNull_EntityInstanceContext()
        {
            Mock<IEdmStructuralProperty> property = new Mock<IEdmStructuralProperty>();

            Assert.ThrowsArgumentNull(
                () => _serializer.CreateStructuralProperty(property.Object, entityInstanceContext: null),
                "entityInstanceContext");
        }

        [Fact]
        public void CreateStructuralProperty_ThrowsSerializationException_TypeCannotBeSerialized()
        {
            // Arrange
            Mock<IEdmTypeReference> propertyType = new Mock<IEdmTypeReference>();
            propertyType.Setup(t => t.Definition).Returns(new EdmEntityType("Namespace", "Name"));
            Mock<IEdmStructuralProperty> property = new Mock<IEdmStructuralProperty>();
            Mock<ODataSerializerProvider> serializerProvider = new Mock<ODataSerializerProvider>(MockBehavior.Strict);
            IEdmEntityObject entity = new Mock<IEdmEntityObject>().Object;
            property.Setup(p => p.Type).Returns(propertyType.Object);
            serializerProvider.Setup(s => s.GetEdmTypeSerializer(propertyType.Object)).Returns<ODataEdmTypeSerializer>(null);

            var serializer = new ODataEntityTypeSerializer(serializerProvider.Object);

            // Act & Assert
            Assert.Throws<SerializationException>(
                () => serializer.CreateStructuralProperty(property.Object, new EntityInstanceContext { EdmObject = entity }),
                "'Namespace.Name' cannot be serialized using the ODataMediaTypeFormatter.");
        }

        [Fact]
        public void CreateStructuralProperty_Calls_CreateODataValueOnInnerSerializer()
        {
            // Arrange
            Mock<IEdmStructuralProperty> property = new Mock<IEdmStructuralProperty>();
            property.Setup(p => p.Name).Returns("PropertyName");
            Mock<ODataSerializerProvider> serializerProvider = new Mock<ODataSerializerProvider>(MockBehavior.Strict);
            var entity = new { PropertyName = 42 };
            Mock<ODataEdmTypeSerializer> innerSerializer = new Mock<ODataEdmTypeSerializer>(ODataPayloadKind.Property);
            ODataValue propertyValue = new Mock<ODataValue>().Object;
            IEdmTypeReference propertyType = _writeContext.GetEdmType(propertyValue, typeof(int));

            property.Setup(p => p.Type).Returns(propertyType);
            serializerProvider.Setup(s => s.GetEdmTypeSerializer(propertyType)).Returns(innerSerializer.Object);
            innerSerializer.Setup(s => s.CreateODataValue(42, propertyType, _writeContext)).Returns(propertyValue).Verifiable();

            var serializer = new ODataEntityTypeSerializer(serializerProvider.Object);
            EntityInstanceContext entityInstanceContext = new EntityInstanceContext(_writeContext, _customerType, entity);

            // Act
            ODataProperty createdProperty = serializer.CreateStructuralProperty(property.Object, entityInstanceContext);

            // Assert
            innerSerializer.Verify();
            Assert.Equal("PropertyName", createdProperty.Name);
            Assert.Equal(propertyValue, createdProperty.Value);
        }

        private bool Verify(EntityInstanceContext instanceContext, object instance, ODataSerializerContext writeContext)
        {
            Assert.Same(instance, (instanceContext.EdmObject as TypedEdmEntityObject).Instance);
            Assert.Equal(writeContext.Model, instanceContext.EdmModel);
            Assert.Equal(writeContext.NavigationSource, instanceContext.NavigationSource);
            Assert.Equal(writeContext.Request, instanceContext.Request);
            Assert.Equal(writeContext.SkipExpensiveAvailabilityChecks, instanceContext.SkipExpensiveAvailabilityChecks);
            Assert.Equal(writeContext.Url, instanceContext.Url);
            return true;
        }

        [Fact]
        public void CreateNavigationLink_ThrowsArgumentNull_NavigationProperty()
        {
            Assert.ThrowsArgumentNull(
                () => _serializer.CreateNavigationLink(navigationProperty: null, entityInstanceContext: _entityInstanceContext),
                "navigationProperty");
        }

        [Fact]
        public void CreateNavigationLink_ThrowsArgumentNull_EntityInstanceContext()
        {
            IEdmNavigationProperty navigationProperty = new Mock<IEdmNavigationProperty>().Object;
            Assert.ThrowsArgumentNull(
                () => _serializer.CreateNavigationLink(navigationProperty, entityInstanceContext: null),
                "entityInstanceContext");
        }

        [Fact]
        public void CreateNavigationLink_CreatesCorrectNavigationLink()
        {
            // Arrange
            Uri navigationLinkUri = new Uri("http://navigation_link");
            IEdmNavigationProperty property1 = CreateFakeNavigationProperty("Property1", _customerType);
            NavigationSourceLinkBuilderAnnotation linkAnnotation = new MockNavigationSourceLinkBuilderAnnotation
            {
                NavigationLinkBuilder = (ctxt, property, metadataLevel) => navigationLinkUri
            };
            _model.SetNavigationSourceLinkBuilder(_customerSet, linkAnnotation);

            Mock<ODataEntityTypeSerializer> serializer = new Mock<ODataEntityTypeSerializer>(_serializerProvider);
            serializer.CallBase = true;

            // Act
            ODataNavigationLink navigationLink = serializer.Object.CreateNavigationLink(property1, _entityInstanceContext);

            // Assert
            Assert.Equal("Property1", navigationLink.Name);
            Assert.Equal(navigationLinkUri, navigationLink.Url);
        }

        [Fact]
        public void CreateEntry_UsesCorrectTypeName()
        {
            EntityInstanceContext instanceContext =
                new EntityInstanceContext { EntityType = _customerType.EntityDefinition(), SerializerContext = _writeContext };
            Mock<ODataEntityTypeSerializer> serializer = new Mock<ODataEntityTypeSerializer>(_serializerProvider);
            serializer.CallBase = true;
            SelectExpandNode selectExpandNode = new SelectExpandNode();

            // Act
            ODataEntry entry = serializer.Object.CreateEntry(selectExpandNode, instanceContext);

            // Assert
            Assert.Equal("Default.Customer", entry.TypeName);
        }

        [Fact]
        public void CreateODataAction_ThrowsArgumentNull_Action()
        {
            Assert.ThrowsArgumentNull(
                () => _serializer.CreateODataAction(action: null, entityInstanceContext: null),
                "action");
        }

        [Fact]
        public void CreateODataAction_ThrowsArgumentNull_EntityInstanceContext()
        {
            IEdmAction action = new Mock<IEdmAction>().Object;

            Assert.ThrowsArgumentNull(
                () => _serializer.CreateODataAction(action, entityInstanceContext: null),
                "entityInstanceContext");
        }

        [Fact]
        public void CreateEntry_WritesCorrectIdLink()
        {
            // Arrange
            EntityInstanceContext instanceContext = new EntityInstanceContext
            {
                SerializerContext = _writeContext,
                EntityType = _customerType.EntityDefinition()
            };

            bool customIdLinkbuilderCalled = false;
            NavigationSourceLinkBuilderAnnotation linkAnnotation = new MockNavigationSourceLinkBuilderAnnotation
            {
                IdLinkBuilder = new SelfLinkBuilder<Uri>((EntityInstanceContext context) =>
                {
                    Assert.Same(instanceContext, context);
                    customIdLinkbuilderCalled = true;
                    return new Uri("http://sample_id_link");
                },
                followsConventions: false)
            };
            _model.SetNavigationSourceLinkBuilder(_customerSet, linkAnnotation);

            Mock<ODataEntityTypeSerializer> serializer = new Mock<ODataEntityTypeSerializer>(_serializerProvider);
            serializer.CallBase = true;
            SelectExpandNode selectExpandNode = new SelectExpandNode();

            // Act
            ODataEntry entry = serializer.Object.CreateEntry(selectExpandNode, instanceContext);

            // Assert
            Assert.True(customIdLinkbuilderCalled);
        }

        [Fact]
        public void WriteObjectInline_WritesCorrectEditLink()
        {
            // Arrange
            EntityInstanceContext instanceContext = new EntityInstanceContext
            {
                SerializerContext = _writeContext,
                EntityType = _customerType.EntityDefinition()
            };
            bool customEditLinkbuilderCalled = false;
            NavigationSourceLinkBuilderAnnotation linkAnnotation = new MockNavigationSourceLinkBuilderAnnotation
            {
                EditLinkBuilder = new SelfLinkBuilder<Uri>((EntityInstanceContext context) =>
                {
                    Assert.Same(instanceContext, context);
                    customEditLinkbuilderCalled = true;
                    return new Uri("http://sample_edit_link");
                },
                followsConventions: false)
            };
            _model.SetNavigationSourceLinkBuilder(_customerSet, linkAnnotation);

            Mock<ODataEntityTypeSerializer> serializer = new Mock<ODataEntityTypeSerializer>(_serializerProvider);
            serializer.CallBase = true;
            SelectExpandNode selectExpandNode = new SelectExpandNode();

            // Act
            ODataEntry entry = serializer.Object.CreateEntry(selectExpandNode, instanceContext);

            // Assert
            Assert.True(customEditLinkbuilderCalled);
        }

        [Fact]
        public void WriteObjectInline_WritesCorrectReadLink()
        {
            // Arrange
            EntityInstanceContext instanceContext = new EntityInstanceContext(_writeContext, _customerType, 42);
            bool customReadLinkbuilderCalled = false;
            NavigationSourceLinkBuilderAnnotation linkAnnotation = new MockNavigationSourceLinkBuilderAnnotation
            {
                ReadLinkBuilder = new SelfLinkBuilder<Uri>((EntityInstanceContext context) =>
                {
                    Assert.Same(instanceContext, context);
                    customReadLinkbuilderCalled = true;
                    return new Uri("http://sample_read_link");
                },
                followsConventions: false)
            };

            _model.SetNavigationSourceLinkBuilder(_customerSet, linkAnnotation);

            Mock<ODataEntityTypeSerializer> serializer = new Mock<ODataEntityTypeSerializer>(_serializerProvider);
            serializer.CallBase = true;
            SelectExpandNode selectExpandNode = new SelectExpandNode();

            // Act
            ODataEntry entry = serializer.Object.CreateEntry(selectExpandNode, instanceContext);

            // Assert
            Assert.True(customReadLinkbuilderCalled);
        }

        [Fact]
        public void CreateSelectExpandNode_ThrowsArgumentNull_EntityInstanceContext()
        {
            Assert.ThrowsArgumentNull(
                () => _serializer.CreateSelectExpandNode(entityInstanceContext: null),
                "entityInstanceContext");
        }

        [Fact]
        public void AddTypeNameAnnotationAsNeeded_AddsAnnotation_IfTypeOfPathDoesNotMatchEntryType()
        {
            // Arrange
            string expectedTypeName = "TypeName";
            ODataEntry entry = new ODataEntry
            {
                TypeName = expectedTypeName
            };

            // Act
            ODataEntityTypeSerializer.AddTypeNameAnnotationAsNeeded(entry, _customerType.EntityDefinition(), ODataMetadataLevel.MinimalMetadata);

            // Assert
            SerializationTypeNameAnnotation annotation = entry.GetAnnotation<SerializationTypeNameAnnotation>();
            Assert.NotNull(annotation); // Guard
            Assert.Equal(expectedTypeName, annotation.TypeName);
        }

        [Fact] // Issue 984: Redundant type name serialization in OData JSON light minimal metadata mode
        public void AddTypeNameAnnotationAsNeeded_AddsAnnotationWithNullValue_IfTypeOfPathMatchesEntryType()
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            ODataEntry entry = new ODataEntry
            {
                TypeName = model.SpecialCustomer.FullName()
            };

            // Act
            ODataEntityTypeSerializer.AddTypeNameAnnotationAsNeeded(entry, model.SpecialCustomer, ODataMetadataLevel.MinimalMetadata);

            // Assert
            SerializationTypeNameAnnotation annotation = entry.GetAnnotation<SerializationTypeNameAnnotation>();
            Assert.NotNull(annotation); // Guard
            Assert.Null(annotation.TypeName);
        }

        [Theory]
        [InlineData("MatchingType", "MatchingType", TestODataMetadataLevel.FullMetadata, false)]
        [InlineData("DoesNotMatch1", "DoesNotMatch2", TestODataMetadataLevel.FullMetadata, false)]
        [InlineData("MatchingType", "MatchingType", TestODataMetadataLevel.MinimalMetadata, true)]
        [InlineData("DoesNotMatch1", "DoesNotMatch2", TestODataMetadataLevel.MinimalMetadata, false)]
        [InlineData("MatchingType", "MatchingType", TestODataMetadataLevel.NoMetadata, true)]
        [InlineData("DoesNotMatch1", "DoesNotMatch2", TestODataMetadataLevel.NoMetadata, true)]
        public void ShouldSuppressTypeNameSerialization(string entryType, string entitySetType,
            TestODataMetadataLevel metadataLevel, bool expectedResult)
        {
            // Arrange
            ODataEntry entry = new ODataEntry
            {
                // The caller uses a namespace-qualified name, which this test leaves empty.
                TypeName = "." + entryType
            };
            IEdmEntityType edmType = CreateEntityTypeWithName(entitySetType);

            // Act
            bool actualResult = ODataEntityTypeSerializer.ShouldSuppressTypeNameSerialization(entry, edmType,
                (ODataMetadataLevel)metadataLevel);

            // Assert
            Assert.Equal(expectedResult, actualResult);
        }

        [Fact]
        public void CreateODataAction_IncludesEverything_ForFullMetadata()
        {
            // Arrange
            string expectedContainerName = "Container";
            string expectedNamespace = "NS";
            string expectedActionName = "Action";
            string expectedTarget = "aa://Target";
            string expectedMetadataPrefix = "http://Metadata";

            IEdmEntityContainer container = CreateFakeContainer(expectedContainerName);
            IEdmAction action = CreateFakeAction(expectedNamespace, expectedActionName, isBindable: true);

            ActionLinkBuilder linkBuilder = new ActionLinkBuilder((a) => new Uri(expectedTarget),
                followsConventions: true);
            IEdmDirectValueAnnotationsManager annotationsManager = CreateFakeAnnotationsManager();
            annotationsManager.SetActionLinkBuilder(action, linkBuilder);
            annotationsManager.SetIsAlwaysBindable(action);
            IEdmModel model = CreateFakeModel(annotationsManager);
            UrlHelper url = CreateMetadataLinkFactory(expectedMetadataPrefix);

            EntityInstanceContext context = CreateContext(model, url);
            context.SerializerContext.MetadataLevel = ODataMetadataLevel.FullMetadata;

            // Act
            ODataAction actualAction = _serializer.CreateODataAction(action, context);

            // Assert
            string expectedMetadata = expectedMetadataPrefix + "#" + expectedNamespace + "." + expectedActionName;
            ODataAction expectedAction = new ODataAction
            {
                Metadata = new Uri(expectedMetadata),
                Target = new Uri(expectedTarget),
                Title = expectedActionName
            };

            AssertEqual(expectedAction, actualAction);
        }

        [Fact]
        public void CreateODataAction_OmitsAction_WhenActionLinkBuilderReturnsNull()
        {
            // Arrange
            IEdmAction action = CreateFakeAction("IgnoreAction");

            ActionLinkBuilder linkBuilder = new ActionLinkBuilder((a) => null, followsConventions: false);
            IEdmDirectValueAnnotationsManager annotationsManager = CreateFakeAnnotationsManager();
            annotationsManager.SetActionLinkBuilder(action, linkBuilder);

            IEdmModel model = CreateFakeModel(annotationsManager);

            EntityInstanceContext context = CreateContext(model);
            context.SerializerContext.MetadataLevel = ODataMetadataLevel.MinimalMetadata;

            // Act
            ODataAction actualAction = _serializer.CreateODataAction(action, context);

            // Assert
            Assert.Null(actualAction);
        }

        [Fact]
        public void CreateODataAction_ForJsonLight_OmitsContainerName_PerCreateMetadataFragment()
        {
            // Arrange
            string expectedMetadataPrefix = "http://Metadata";
            string expectedNamespace = "NS";
            string expectedActionName = "Action";

            IEdmEntityContainer container = CreateFakeContainer("ContainerShouldNotAppearInResult");
            IEdmAction action = CreateFakeAction(expectedNamespace, expectedActionName);

            ActionLinkBuilder linkBuilder = new ActionLinkBuilder((a) => new Uri("aa://IgnoreTarget"),
                followsConventions: false);
            IEdmDirectValueAnnotationsManager annotationsManager = CreateFakeAnnotationsManager();
            annotationsManager.SetActionLinkBuilder(action, linkBuilder);

            IEdmModel model = CreateFakeModel(annotationsManager);
            UrlHelper url = CreateMetadataLinkFactory(expectedMetadataPrefix);

            EntityInstanceContext context = CreateContext(model, url);
            context.SerializerContext.MetadataLevel = ODataMetadataLevel.MinimalMetadata;

            // Act
            ODataAction actualAction = _serializer.CreateODataAction(action, context);

            // Assert
            Assert.NotNull(actualAction);
            string expectedMetadata = expectedMetadataPrefix + "#" + expectedNamespace + "." + expectedActionName;
            AssertEqual(new Uri(expectedMetadata), actualAction.Metadata);
        }

        [Fact]
        public void CreateODataAction_SkipsAlwaysAvailableAction_PerShouldOmitAction()
        {
            // Arrange
            IEdmActionImport actionImport = CreateFakeActionImport(true);

            ActionLinkBuilder linkBuilder = new ActionLinkBuilder((a) => new Uri("aa://IgnoreTarget"),
                followsConventions: true);
            IEdmDirectValueAnnotationsManager annotationsManager = CreateFakeAnnotationsManager();
            annotationsManager.SetActionLinkBuilder(actionImport, linkBuilder);
            annotationsManager.SetIsAlwaysBindable(actionImport.Action);

            IEdmModel model = CreateFakeModel(annotationsManager);

            EntityInstanceContext context = CreateContext(model);
            context.SerializerContext.MetadataLevel = ODataMetadataLevel.MinimalMetadata;

            // Act
            ODataAction actualAction = _serializer.CreateODataAction(actionImport.Action, context);

            // Assert
            Assert.Null(actualAction);
        }

        [Fact]
        public void CreateODataAction_IncludesTitle()
        {
            // Arrange
            string expectedActionName = "Action";

            IEdmAction action = CreateFakeAction(expectedActionName);

            ActionLinkBuilder linkBuilder = new ActionLinkBuilder((a) => new Uri("aa://IgnoreTarget"),
                followsConventions: false);
            IEdmDirectValueAnnotationsManager annotationsManager = CreateFakeAnnotationsManager();
            annotationsManager.SetActionLinkBuilder(action, linkBuilder);

            IEdmModel model = CreateFakeModel(annotationsManager);
            UrlHelper url = CreateMetadataLinkFactory("http://IgnoreMetadataPath");

            EntityInstanceContext context = CreateContext(model, url);
            context.SerializerContext.MetadataLevel = (ODataMetadataLevel)TestODataMetadataLevel.FullMetadata;

            // Act
            ODataAction actualAction = _serializer.CreateODataAction(action, context);

            // Assert
            Assert.NotNull(actualAction);
            Assert.Equal(expectedActionName, actualAction.Title);
        }

        [Theory]
        [InlineData(TestODataMetadataLevel.MinimalMetadata)]
        [InlineData(TestODataMetadataLevel.NoMetadata)]
        public void CreateODataAction_OmitsTitle(TestODataMetadataLevel metadataLevel)
        {
            // Arrange
            IEdmAction action = CreateFakeAction("IgnoreAction");

            ActionLinkBuilder linkBuilder = new ActionLinkBuilder((a) => new Uri("aa://Ignore"),
                followsConventions: false);
            IEdmDirectValueAnnotationsManager annotationsManager = CreateFakeAnnotationsManager();
            annotationsManager.SetActionLinkBuilder(action, linkBuilder);

            IEdmModel model = CreateFakeModel(annotationsManager);
            UrlHelper url = CreateMetadataLinkFactory("http://IgnoreMetadataPath");

            EntityInstanceContext context = CreateContext(model, url);
            context.SerializerContext.MetadataLevel = (ODataMetadataLevel)metadataLevel;

            // Act
            ODataAction actualAction = _serializer.CreateODataAction(action, context);

            // Assert
            Assert.NotNull(actualAction);
            Assert.Null(actualAction.Title);
        }

        [Theory]
        [InlineData(TestODataMetadataLevel.FullMetadata, false)]
        [InlineData(TestODataMetadataLevel.FullMetadata, true)]
        [InlineData(TestODataMetadataLevel.MinimalMetadata, false)]
        [InlineData(TestODataMetadataLevel.NoMetadata, false)]
        public void CreateODataAction_IncludesTarget(TestODataMetadataLevel metadataLevel, bool followsConventions)
        {
            // Arrange
            Uri expectedTarget = new Uri("aa://Target");

            IEdmAction action = CreateFakeAction("IgnoreAction");

            ActionLinkBuilder linkBuilder = new ActionLinkBuilder((a) => expectedTarget, followsConventions);
            IEdmDirectValueAnnotationsManager annotationsManager = CreateFakeAnnotationsManager();
            annotationsManager.SetActionLinkBuilder(action, linkBuilder);

            IEdmModel model = CreateFakeModel(annotationsManager);
            UrlHelper url = CreateMetadataLinkFactory("http://IgnoreMetadataPath");

            EntityInstanceContext context = CreateContext(model, url);
            context.SerializerContext.MetadataLevel = (ODataMetadataLevel)metadataLevel;

            // Act
            ODataAction actualAction = _serializer.CreateODataAction(action, context);

            // Assert
            Assert.NotNull(actualAction);
            Assert.Equal(expectedTarget, actualAction.Target);
        }

        [Theory]
        [InlineData(TestODataMetadataLevel.MinimalMetadata)]
        [InlineData(TestODataMetadataLevel.NoMetadata)]
        public void CreateODataAction_OmitsAction_WhenFollowingConventions(TestODataMetadataLevel metadataLevel)
        {
            // Arrange
            IEdmAction action = CreateFakeAction("IgnoreAction", isBindable: true);

            ActionLinkBuilder linkBuilder = new ActionLinkBuilder((a) => new Uri("aa://Ignore"),
                followsConventions: true);
            IEdmDirectValueAnnotationsManager annotationsManager = CreateFakeAnnotationsManager();
            annotationsManager.SetActionLinkBuilder(action, linkBuilder);

            IEdmModel model = CreateFakeModel(annotationsManager);
            UrlHelper url = CreateMetadataLinkFactory("http://IgnoreMetadataPath");

            EntityInstanceContext context = CreateContext(model, url);
            context.SerializerContext.MetadataLevel = (ODataMetadataLevel)metadataLevel;

            // Act
            ODataAction actualAction = _serializer.CreateODataAction(action, context);

            // Assert
            Assert.Null(actualAction);
        }
        
        [Fact]
        public void CreateMetadataFragment_IncludesNamespaceAndName()
        {
            // Arrange
            string expectedActionName = "Action";
            string expectedNamespace = "NS";

            IEdmEntityContainer container = CreateFakeContainer("ContainerShouldNotAppearInResult");
            IEdmAction action = CreateFakeAction(expectedNamespace, expectedActionName);

            IEdmDirectValueAnnotationsManager annotationsManager = CreateFakeAnnotationsManager();
            IEdmModel model = CreateFakeModel(annotationsManager);

            // Act
            string actualFragment = ODataEntityTypeSerializer.CreateMetadataFragment(action);

            // Assert
            Assert.Equal(expectedNamespace + "." + expectedActionName, actualFragment);
        }

        [Theory]
        [InlineData(TestODataMetadataLevel.FullMetadata, false, false)]
        [InlineData(TestODataMetadataLevel.FullMetadata, true, false)]
        [InlineData(TestODataMetadataLevel.MinimalMetadata, false, false)]
        [InlineData(TestODataMetadataLevel.MinimalMetadata, true, true)]
        [InlineData(TestODataMetadataLevel.NoMetadata, false, false)]
        [InlineData(TestODataMetadataLevel.NoMetadata, true, true)]
        public void TestShouldOmitAction(TestODataMetadataLevel metadataLevel,
            bool followsConventions, bool expectedResult)
        {
            // Arrange
            IEdmActionImport action = CreateFakeActionImport(true);
            IEdmDirectValueAnnotationsManager annonationsManager = CreateFakeAnnotationsManager();

            IEdmModel model = CreateFakeModel(annonationsManager);

            ActionLinkBuilder builder = new ActionLinkBuilder((a) => { throw new NotImplementedException(); },
                followsConventions);

            // Act
            bool actualResult = ODataEntityTypeSerializer.ShouldOmitAction(action.Action, builder,
                (ODataMetadataLevel)metadataLevel);

            // Assert
            Assert.Equal(expectedResult, actualResult);
        }

        [Fact]
        public void TestSetTitleAnnotation()
        {
            // Arrange
            IEdmActionImport action = CreateFakeActionImport(true);
            IEdmDirectValueAnnotationsManager annonationsManager = CreateFakeAnnotationsManager();
            IEdmModel model = CreateFakeModel(annonationsManager);
            string expectedTitle = "The title";
            model.SetOperationTitleAnnotation(action.Operation, new OperationTitleAnnotation(expectedTitle));
            ODataAction odataAction = new ODataAction();

            // Act
            ODataEntityTypeSerializer.EmitTitle(model, action.Operation, odataAction);

            // Assert
            Assert.Equal(expectedTitle, odataAction.Title);
        }

        [Fact]
        public void TestSetTitleAnnotation_UsesNameIfNoTitleAnnotationIsPresent()
        {
            // Arrange
            IEdmActionImport action = CreateFakeActionImport(CreateFakeContainer("Container"), "Action");
            IEdmDirectValueAnnotationsManager annonationsManager = CreateFakeAnnotationsManager();
            IEdmModel model = CreateFakeModel(annonationsManager);
            ODataAction odataAction = new ODataAction();

            // Act
            ODataEntityTypeSerializer.EmitTitle(model, action.Operation, odataAction);

            // Assert
            Assert.Equal(action.Operation.Name, odataAction.Title);
        }

        [Fact]
        public void WriteObjectInline_SetsParentContext_ForExpandedNavigationProperties()
        {
            // Arrange
            ODataWriter mockWriter = new Mock<ODataWriter>().Object;
            IEdmNavigationProperty ordersProperty = _customerSet.EntityType().DeclaredNavigationProperties().Single();
            Mock<ODataEdmTypeSerializer> expandedItemSerializer = new Mock<ODataEdmTypeSerializer>(ODataPayloadKind.Feed);
            Mock<ODataSerializerProvider> serializerProvider = new Mock<ODataSerializerProvider>();
            serializerProvider.Setup(p => p.GetEdmTypeSerializer(ordersProperty.Type))
                .Returns(expandedItemSerializer.Object);

            SelectExpandNode selectExpandNode = new SelectExpandNode
            {
                ExpandedNavigationProperties = 
                {
                     { ordersProperty, new SelectExpandClause(new SelectItem[0], allSelected: true) }
                }
            };
            Mock<ODataEntityTypeSerializer> serializer = new Mock<ODataEntityTypeSerializer>(serializerProvider.Object);
            serializer.Setup(s => s.CreateSelectExpandNode(It.IsAny<EntityInstanceContext>())).Returns(selectExpandNode);
            serializer.Setup(s => s.CreateEntry(selectExpandNode, _entityInstanceContext)).Returns(new ODataEntry());
            serializer.CallBase = true;

            // Act
            serializer.Object.WriteObjectInline(_customer, _customerType, mockWriter, _writeContext);

            // Assert
            expandedItemSerializer.Verify(
                s => s.WriteObjectInline(It.IsAny<object>(), ordersProperty.Type, mockWriter,
                    It.Is<ODataSerializerContext>(c => c.ExpandedEntity.SerializerContext == _writeContext)));
        }

        [Fact]
        public void CreateSelectExpandNode_Caches_SelectExpandNode()
        {
            // Arrange
            IEdmEntityTypeReference customerType = _customerSet.EntityType().AsReference();
            EntityInstanceContext entity1 = new EntityInstanceContext(_writeContext, customerType, new Customer());
            EntityInstanceContext entity2 = new EntityInstanceContext(_writeContext, customerType, new Customer());

            // Act
            var selectExpandNode1 = _serializer.CreateSelectExpandNode(entity1);
            var selectExpandNode2 = _serializer.CreateSelectExpandNode(entity2);

            // Assert
            Assert.Same(selectExpandNode1, selectExpandNode2);
        }

        [Fact]
        public void CreateSelectExpandNode_ReturnsDifferentSelectExpandNode_IfEntityTypeIsDifferent()
        {
            // Arrange
            IEdmEntityType customerType = _customerSet.EntityType();
            IEdmEntityType derivedCustomerType = new EdmEntityType("NS", "DerivedCustomer", customerType);

            EntityInstanceContext entity1 = new EntityInstanceContext(_writeContext, customerType.AsReference(), new Customer());
            EntityInstanceContext entity2 = new EntityInstanceContext(_writeContext, derivedCustomerType.AsReference(), new Customer());

            // Act
            var selectExpandNode1 = _serializer.CreateSelectExpandNode(entity1);
            var selectExpandNode2 = _serializer.CreateSelectExpandNode(entity2);

            // Assert
            Assert.NotSame(selectExpandNode1, selectExpandNode2);
        }

        [Fact]
        public void CreateSelectExpandNode_ReturnsDifferentSelectExpandNode_IfSelectExpandClauseIsDifferent()
        {
            // Arrange
            IEdmEntityType customerType = _customerSet.EntityType();

            EntityInstanceContext entity1 = new EntityInstanceContext(_writeContext, customerType.AsReference(), new Customer());
            EntityInstanceContext entity2 = new EntityInstanceContext(_writeContext, customerType.AsReference(), new Customer());

            // Act
            _writeContext.SelectExpandClause = new SelectExpandClause(new SelectItem[0], allSelected: true);
            var selectExpandNode1 = _serializer.CreateSelectExpandNode(entity1);
            _writeContext.SelectExpandClause = new SelectExpandClause(new SelectItem[0], allSelected: false);
            var selectExpandNode2 = _serializer.CreateSelectExpandNode(entity2);

            // Assert
            Assert.NotSame(selectExpandNode1, selectExpandNode2);
        }

        private static IEdmNavigationProperty CreateFakeNavigationProperty(string name, IEdmTypeReference type)
        {
            Mock<IEdmNavigationProperty> property = new Mock<IEdmNavigationProperty>();
            property.Setup(p => p.Name).Returns(name);
            property.Setup(p => p.Type).Returns(type);
            return property.Object;
        }

        private static void AssertEqual(ODataAction expected, ODataAction actual)
        {
            if (expected == null)
            {
                Assert.Null(actual);
                return;
            }

            Assert.NotNull(actual);
            AssertEqual(expected.Metadata, actual.Metadata);
            AssertEqual(expected.Target, actual.Target);
            Assert.Equal(expected.Title, actual.Title);
        }

        private static void AssertEqual(Uri expected, Uri actual)
        {
            if (expected == null)
            {
                Assert.Null(actual);
                return;
            }

            Assert.NotNull(actual);
            Assert.Equal(expected.AbsoluteUri, actual.AbsoluteUri);
        }

        private static EntityInstanceContext CreateContext(IEdmModel model)
        {
            return new EntityInstanceContext
            {
                EdmModel = model
            };
        }

        private static EntityInstanceContext CreateContext(IEdmModel model, UrlHelper url)
        {
            return new EntityInstanceContext
            {
                EdmModel = model,
                Url = url,
            };
        }

        private static IEdmEntityType CreateEntityTypeWithName(string typeName)
        {
            Mock<IEdmEntityType> entityTypeMock = new Mock<IEdmEntityType>();
            entityTypeMock.Setup(o => o.Name).Returns(typeName);
            return entityTypeMock.Object;
        }

        private static IEdmDirectValueAnnotationsManager CreateFakeAnnotationsManager()
        {
            return new FakeAnnotationsManager();
        }

        private static IEdmEntityContainer CreateFakeContainer(string name)
        {
            Mock<IEdmEntityContainer> mock = new Mock<IEdmEntityContainer>();
            mock.Setup(o => o.Name).Returns(name);
            return mock.Object;
        }

        private static IEdmActionImport CreateFakeActionImport(IEdmEntityContainer container, string name)
        {
            Mock<IEdmAction> mockAction = new Mock<IEdmAction>();
            mockAction.Setup(o => o.IsBound).Returns(true);
            mockAction.Setup(o => o.Name).Returns(name);
            Mock<IEdmActionImport> mock = new Mock<IEdmActionImport>();
            mock.Setup(o => o.Container).Returns(container);
            mock.Setup(o => o.Name).Returns(name);
            mock.Setup(o => o.Action).Returns(mockAction.Object);
            mock.Setup(o => o.Operation).Returns(mockAction.Object);
            return mock.Object;
        }
        
        private static IEdmActionImport CreateFakeActionImport(IEdmEntityContainer container, string name, bool isBindable)
        {
            Mock<IEdmActionImport> mock = new Mock<IEdmActionImport>();
            mock.Setup(o => o.Container).Returns(container);
            mock.Setup(o => o.Name).Returns(name);
            Mock<IEdmAction> mockAction = new Mock<IEdmAction>();
            mockAction.Setup(o => o.IsBound).Returns(isBindable);
            mock.Setup(o => o.Action).Returns(mockAction.Object);
            return mock.Object;
        }

        private static IEdmActionImport CreateFakeActionImport(bool isBindable)
        {
            Mock<IEdmActionImport> mock = new Mock<IEdmActionImport>();
            Mock<IEdmAction> mockAction = new Mock<IEdmAction>();
            mockAction.Setup(o => o.IsBound).Returns(isBindable);
            mock.Setup(o => o.Action).Returns(mockAction.Object);
            mock.Setup(o => o.Operation).Returns(mockAction.Object);
            return mock.Object;
        }

        private static IEdmAction CreateFakeAction(string name)
        {
            return CreateFakeAction(nameSpace: null, name: name, isBindable: true);
        }

        private static IEdmAction CreateFakeAction(string name, bool isBindable)
        {
            return CreateFakeAction(nameSpace: null, name: name, isBindable: isBindable);
        }

        private static IEdmAction CreateFakeAction(string nameSpace, string name)
        {
            return CreateFakeAction(nameSpace, name, isBindable: true);
        }

        private static IEdmAction CreateFakeAction(string nameSpace, string name, bool isBindable)
        {
            Mock<IEdmAction> mockAction = new Mock<IEdmAction>();
            mockAction.SetupGet(o => o.Namespace).Returns(nameSpace);
            mockAction.SetupGet(o => o.Name).Returns(name);
            mockAction.Setup(o => o.IsBound).Returns(isBindable);
            Mock<IEdmOperationParameter> mockParameter = new Mock<IEdmOperationParameter>();
            mockParameter.SetupGet(o => o.DeclaringOperation).Returns(mockAction.Object);
            Mock<IEdmEntityTypeReference> mockEntityTyeRef = new Mock<IEdmEntityTypeReference>();
            mockEntityTyeRef.Setup(o => o.Definition).Returns(new Mock<IEdmEntityType>().Object);
            mockParameter.SetupGet(o => o.Type).Returns(mockEntityTyeRef.Object);
            mockAction.SetupGet(o => o.Parameters).Returns(new[] { mockParameter.Object });
            return mockAction.Object;
        }

        private static IEdmModel CreateFakeModel()
        {
            IEdmDirectValueAnnotationsManager annotationsManager = CreateFakeAnnotationsManager();
            return CreateFakeModel(annotationsManager);
        }

        private static IEdmModel CreateFakeModel(IEdmDirectValueAnnotationsManager annotationsManager)
        {
            Mock<IEdmModel> model = new Mock<IEdmModel>();
            model.Setup(m => m.DirectValueAnnotationsManager).Returns(annotationsManager);
            return model.Object;
        }

        private static UrlHelper CreateMetadataLinkFactory(string metadataPath)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, metadataPath);
            HttpConfiguration configuration = new HttpConfiguration();
            configuration.Routes.MapFakeODataRoute();
            request.SetConfiguration(configuration);
            request.SetFakeODataRouteName();
            return new UrlHelper(request);
        }

        private class Customer
        {
            public Customer()
            {
                this.Orders = new List<Order>();
            }
            public int ID { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string City { get; set; }
            public IList<Order> Orders { get; private set; }
        }

        private class SpecialCustomer
        {
            public IList<SpecialOrder> SpecialOrders { get; private set; }
        }

        private class Order
        {
            public int ID { get; set; }
            public string Name { get; set; }
            public string Shipment { get; set; }
            public Customer Customer { get; set; }
        }

        private class SpecialOrder
        {
            public SpecialCustomer SpecialCustomer { get; set; }
        }

        private class FakeBindableProcedureFinder : BindableProcedureFinder
        {
            private IEdmOperation[] _procedures;

            public FakeBindableProcedureFinder(params IEdmOperation[] procedures)
                : base(EdmCoreModel.Instance)
            {
                _procedures = procedures;
            }

            public override IEnumerable<IEdmOperation> FindProcedures(IEdmEntityType entityType)
            {
                return _procedures;
            }
        }

        private class FakeAnnotationsManager : IEdmDirectValueAnnotationsManager
        {
            IDictionary<Tuple<IEdmElement, string, string>, object> annotations =
                new Dictionary<Tuple<IEdmElement, string, string>, object>();

            public object GetAnnotationValue(IEdmElement element, string namespaceName, string localName)
            {
                object value;

                if (!annotations.TryGetValue(CreateKey(element, namespaceName, localName), out value))
                {
                    return null;
                }

                return value;
            }

            public object[] GetAnnotationValues(IEnumerable<IEdmDirectValueAnnotationBinding> annotations)
            {
                throw new NotImplementedException();
            }

            public IEnumerable<IEdmDirectValueAnnotation> GetDirectValueAnnotations(IEdmElement element)
            {
                throw new NotImplementedException();
            }

            public void SetAnnotationValue(IEdmElement element, string namespaceName, string localName, object value)
            {
                annotations[CreateKey(element, namespaceName, localName)] = value;
            }

            public void SetAnnotationValues(IEnumerable<IEdmDirectValueAnnotationBinding> annotations)
            {
                throw new NotImplementedException();
            }

            private static Tuple<IEdmElement, string, string> CreateKey(IEdmElement element, string namespaceName,
                string localName)
            {
                return new Tuple<IEdmElement, string, string>(element, namespaceName, localName);
            }
        }

    }
}
