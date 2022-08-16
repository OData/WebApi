//-----------------------------------------------------------------------------
// <copyright file="ODataResourceSerializerTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Threading.Tasks;
#if NETFX // Only AspNet version has UrlHelper
using System.Web.Http.Routing;
#endif
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Formatter.Serialization;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Builder;
using Microsoft.AspNet.OData.Test.Builder.TestModels;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.AspNet.OData.Test.Common.Types;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OData.UriParser;
using Moq;
using Xunit;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;

namespace Microsoft.AspNet.OData.Test.Formatter.Serialization
{
    public class ODataResourceSerializerTests
    {
        private IEdmModel _model;
        private IEdmEntitySet _customerSet;
        private IEdmEntitySet _orderSet;
        private Customer _customer;
        private Order _order;
        private ODataResourceSerializer _serializer;
        private ODataSerializerContext _writeContext;
        private ResourceContext _entityContext;
        private ODataSerializerProvider _serializerProvider;
        private IEdmEntityTypeReference _customerType;
        private IEdmEntityTypeReference _orderType;
        private IEdmEntityTypeReference _specialCustomerType;
        private IEdmEntityTypeReference _specialOrderType;
        private ODataPath _path;

        public ODataResourceSerializerTests()
        {
            _model = SerializationTestsHelpers.SimpleCustomerOrderModel();

            _model.SetAnnotationValue(_model.FindType("Default.Customer"), new ClrTypeAnnotation(typeof(Customer)));
            _model.SetAnnotationValue(_model.FindType("Default.Order"), new ClrTypeAnnotation(typeof(Order)));
            _model.SetAnnotationValue(_model.FindType("Default.SpecialCustomer"), new ClrTypeAnnotation(typeof(SpecialCustomer)));
            _model.SetAnnotationValue(_model.FindType("Default.SpecialOrder"), new ClrTypeAnnotation(typeof(SpecialOrder)));

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

            _serializerProvider = ODataSerializerProviderFactory.Create();
            _customerType = _model.GetEdmTypeReference(typeof(Customer)).AsEntity();
            _orderType = _model.GetEdmTypeReference(typeof(Order)).AsEntity();
            _specialCustomerType = _model.GetEdmTypeReference(typeof(SpecialCustomer)).AsEntity();
            _specialOrderType = _model.GetEdmTypeReference(typeof(SpecialOrder)).AsEntity();
            _serializer = new ODataResourceSerializer(_serializerProvider);
            _path = new ODataPath(new EntitySetSegment(_customerSet));
            _writeContext = new ODataSerializerContext() { NavigationSource = _customerSet, Model = _model, Path = _path };
            _entityContext = new ResourceContext(_writeContext, _customerSet.EntityType().AsReference(), _customer);
        }

        [Fact]
        public void Ctor_ThrowsArgumentNull_SerializerProvider()
        {
            ExceptionAssert.ThrowsArgumentNull(
                () => new ODataResourceSerializer(serializerProvider: null),
                "serializerProvider");
        }

        [Fact]
        public void WriteObject_ThrowsArgumentNull_MessageWriter()
        {
            ExceptionAssert.ThrowsArgumentNull(
                () => _serializer.WriteObject(graph: _customer, type: typeof(Customer), messageWriter: null, writeContext: null),
                "messageWriter");
        }

        [Fact]
        public void WriteObject_ThrowsArgumentNull_WriteContext()
        {
            ODataMessageWriter messageWriter = new ODataMessageWriter(new Mock<IODataRequestMessage>().Object);
            ExceptionAssert.ThrowsArgumentNull(
                () => _serializer.WriteObject(graph: _customer, type: typeof(Customer), messageWriter: messageWriter, writeContext: null),
                "writeContext");
        }

        [Fact]
        public void WriteObject_Calls_WriteObjectInline_WithRightEntityType()
        {
            // Arrange
            Mock<ODataResourceSerializer> serializer = new Mock<ODataResourceSerializer>(_serializerProvider);
            serializer
                .Setup(s => s.WriteObjectInline(_customer, It.Is<IEdmTypeReference>(e => _customerType.Definition == e.Definition),
                    It.IsAny<ODataWriter>(), _writeContext))
                .Verifiable();
            serializer.Setup(s => s.CreateSelectExpandNode(It.IsAny<ResourceContext>())).Returns(new SelectExpandNode());
            serializer.CallBase = true;

            // Act
            serializer.Object.WriteObject(_customer, typeof(int), ODataTestUtil.GetMockODataMessageWriter(), _writeContext);

            // Assert
            serializer.Verify();
        }

        [Fact]
        public void WriteObjectInline_ThrowsArgumentNull_Writer()
        {
            ExceptionAssert.ThrowsArgumentNull(
                () => _serializer.WriteObjectInline(graph: null, expectedType: null, writer: null, writeContext: new ODataSerializerContext()),
                "writer");
        }

        [Fact]
        public void WriteObjectInline_ThrowsArgumentNull_WriteContext()
        {
            ExceptionAssert.ThrowsArgumentNull(
                () => _serializer.WriteObjectInline(graph: null, expectedType: null, writer: new Mock<ODataWriter>().Object, writeContext: null),
                "writeContext");
        }

        [Fact]
        public void WriteObjectInline_ThrowsSerializationException_WhenGraphIsNull()
        {
            ODataWriter messageWriter = new Mock<ODataWriter>().Object;
            ExceptionAssert.Throws<SerializationException>(
                () => _serializer.WriteObjectInline(graph: null, expectedType: null, writer: messageWriter, writeContext: new ODataSerializerContext()),
                "Cannot serialize a null 'Resource'.");
        }

        [Fact]
        public void WriteObjectInline_Calls_CreateSelectExpandNode()
        {
            // Arrange
            Mock<ODataResourceSerializer> serializer = new Mock<ODataResourceSerializer>(_serializerProvider);
            ODataWriter writer = new Mock<ODataWriter>().Object;

            serializer.Setup(s => s.CreateSelectExpandNode(It.Is<ResourceContext>(e => Verify(e, _customer, _writeContext)))).Verifiable();
            serializer.CallBase = true;

            // Act
            serializer.Object.WriteObjectInline(_customer, _customerType, writer, _writeContext);

            // Assert
            serializer.Verify();
        }

        [Fact]
        public void WriteObjectInline_Calls_CreateResource()
        {
            // Arrange
            SelectExpandNode selectExpandNode = new SelectExpandNode();
            Mock<ODataResourceSerializer> serializer = new Mock<ODataResourceSerializer>(_serializerProvider);
            ODataWriter writer = new Mock<ODataWriter>().Object;

            serializer.Setup(s => s.CreateSelectExpandNode(It.IsAny<ResourceContext>())).Returns(selectExpandNode);
            serializer.Setup(s => s.CreateResource(selectExpandNode, It.Is<ResourceContext>(e => Verify(e, _customer, _writeContext)))).Verifiable();
            serializer.CallBase = true;

            // Act
            serializer.Object.WriteObjectInline(_customer, _customerType, writer, _writeContext);

            // Assert
            serializer.Verify();
        }

        [Fact]
        public void WriteObjectInline_WritesODataEntryFrom_CreateResource()
        {
            // Arrange
            ODataResource entry = new ODataResource();
            Mock<ODataResourceSerializer> serializer = new Mock<ODataResourceSerializer>(_serializerProvider);
            Mock<ODataWriter> writer = new Mock<ODataWriter>();

            serializer.Setup(s => s.CreateResource(It.IsAny<SelectExpandNode>(), It.IsAny<ResourceContext>())).Returns(entry);
            serializer.CallBase = true;

            writer.Setup(s => s.WriteStart(entry)).Verifiable();

            // Act
            serializer.Object.WriteObjectInline(_customer, _customerType, writer.Object, _writeContext);

            // Assert
            writer.Verify();
        }

        [Fact]
        public void WriteObjectInline_Calls_CreateComplexNestedResourceInfo_ForEachSelectedComplexProperty()
        {
            // Arrange
            SelectExpandNode selectExpandNode = new SelectExpandNode
            {
                SelectedComplexTypeProperties = new Dictionary<IEdmStructuralProperty, PathSelectItem>
                {
                    { new Mock<IEdmStructuralProperty>().Object, null },
                    { new Mock<IEdmStructuralProperty>().Object, null }
                }
            };

            Mock<ODataWriter> writer = new Mock<ODataWriter>();
            Mock<ODataResourceSerializer> serializer = new Mock<ODataResourceSerializer>(_serializerProvider);
            serializer.Setup(s => s.CreateSelectExpandNode(It.IsAny<ResourceContext>())).Returns(selectExpandNode);
            serializer.CallBase = true;

            serializer.Setup(s => s.CreateComplexNestedResourceInfo(selectExpandNode.SelectedComplexTypeProperties.ElementAt(0).Key, null, It.IsAny<ResourceContext>())).Verifiable();
            serializer.Setup(s => s.CreateComplexNestedResourceInfo(selectExpandNode.SelectedComplexTypeProperties.ElementAt(1).Key, null, It.IsAny<ResourceContext>())).Verifiable();

            // Act
            serializer.Object.WriteObjectInline(_customer, _customerType, writer.Object, _writeContext);

            // Assert
            serializer.Verify();
        }

        [Fact]
        public void WriteObjectInline_Calls_CreateNavigationLink_ForEachSelectedNavigationProperty()
        {
            // Arrange
            SelectExpandNode selectExpandNode = new SelectExpandNode
            {
                SelectedNavigationProperties = new HashSet<IEdmNavigationProperty>
                {
                    new Mock<IEdmNavigationProperty>().Object,
                    new Mock<IEdmNavigationProperty>().Object
                }
            };
            Mock<ODataWriter> writer = new Mock<ODataWriter>();
            Mock<ODataResourceSerializer> serializer = new Mock<ODataResourceSerializer>(_serializerProvider);
            serializer.Setup(s => s.CreateSelectExpandNode(It.IsAny<ResourceContext>())).Returns(selectExpandNode);
            serializer.CallBase = true;

            serializer.Setup(s => s.CreateNavigationLink(selectExpandNode.SelectedNavigationProperties.ElementAt(0), It.IsAny<ResourceContext>())).Verifiable();
            serializer.Setup(s => s.CreateNavigationLink(selectExpandNode.SelectedNavigationProperties.ElementAt(1), It.IsAny<ResourceContext>())).Verifiable();

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
                SelectedNavigationProperties = new HashSet<IEdmNavigationProperty>
                {
                    new Mock<IEdmNavigationProperty>().Object,
                    new Mock<IEdmNavigationProperty>().Object
                }
            };
            ODataNestedResourceInfo[] navigationLinks = new[]
            {
                new ODataNestedResourceInfo(),
                new ODataNestedResourceInfo()
            };
            Mock<ODataResourceSerializer> serializer = new Mock<ODataResourceSerializer>(_serializerProvider);
            serializer.Setup(s => s.CreateSelectExpandNode(It.IsAny<ResourceContext>())).Returns(selectExpandNode);
            serializer
                .Setup(s => s.CreateNavigationLink(selectExpandNode.SelectedNavigationProperties.ElementAt(0), It.IsAny<ResourceContext>()))
                .Returns(navigationLinks[0]);
            serializer
                .Setup(s => s.CreateNavigationLink(selectExpandNode.SelectedNavigationProperties.ElementAt(1), It.IsAny<ResourceContext>()))
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
                ExpandedProperties = new Dictionary<IEdmNavigationProperty, ExpandedNavigationSelectItem>
                {
                    { new Mock<IEdmNavigationProperty>().Object, null },
                    { new Mock<IEdmNavigationProperty>().Object, null }
                }
            };
            Mock<ODataWriter> writer = new Mock<ODataWriter>();
            Mock<ODataResourceSerializer> serializer = new Mock<ODataResourceSerializer>(_serializerProvider);
            serializer.Setup(s => s.CreateSelectExpandNode(It.IsAny<ResourceContext>())).Returns(selectExpandNode);
            var expandedNavigationProperties = selectExpandNode.ExpandedProperties.Keys;

            serializer.Setup(s => s.CreateNavigationLink(expandedNavigationProperties.First(), It.IsAny<ResourceContext>())).Verifiable();
            serializer.Setup(s => s.CreateNavigationLink(expandedNavigationProperties.Last(), It.IsAny<ResourceContext>())).Verifiable();
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
                ExpandedProperties = new Dictionary<IEdmNavigationProperty, ExpandedNavigationSelectItem>
                {
                    { ordersProperty, selectExpandClause.SelectedItems.OfType<ExpandedNavigationSelectItem>().Single() }
                },
            };
            Mock<ODataWriter> writer = new Mock<ODataWriter>();

            Mock<ODataEdmTypeSerializer> innerSerializer = new Mock<ODataEdmTypeSerializer>(ODataPayloadKind.Resource);
            innerSerializer
                .Setup(s => s.WriteObjectInline(_customer.Orders, ordersProperty.Type, writer.Object, It.IsAny<ODataSerializerContext>()))
                .Callback((object o, IEdmTypeReference t, ODataWriter w, ODataSerializerContext context) =>
                    {
                        Assert.Same(context.NavigationSource.Name, "Orders");
                        Assert.Same(context.SelectExpandClause, selectExpandNode.ExpandedProperties.Single().Value.SelectAndExpand);
                    })
                .Verifiable();

            Mock<ODataSerializerProvider> serializerProvider = new Mock<ODataSerializerProvider>();
            serializerProvider.Setup(p => p.GetEdmTypeSerializer(ordersProperty.Type))
                .Returns(innerSerializer.Object);
            Mock<ODataResourceSerializer> serializer = new Mock<ODataResourceSerializer>(serializerProvider.Object);
            serializer.Setup(s => s.CreateSelectExpandNode(It.IsAny<ResourceContext>())).Returns(selectExpandNode);
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

            SelectExpandNode selectExpandNode = new SelectExpandNode
            {
                ExpandedProperties = new Dictionary<IEdmNavigationProperty, ExpandedNavigationSelectItem>()
            };
            selectExpandNode.ExpandedProperties[ordersProperty] = selectExpandClause.SelectedItems.OfType<ExpandedNavigationSelectItem>().Single();

            Mock<ODataWriter> writer = new Mock<ODataWriter>();

            Mock<ODataEdmTypeSerializer> ordersSerializer = new Mock<ODataEdmTypeSerializer>(ODataPayloadKind.Resource);
            ordersSerializer.Setup(s => s.WriteObjectInline(ordersValue, ordersProperty.Type, writer.Object, It.IsAny<ODataSerializerContext>())).Verifiable();

            Mock<ODataSerializerProvider> serializerProvider = new Mock<ODataSerializerProvider>();
            serializerProvider.Setup(p => p.GetEdmTypeSerializer(ordersProperty.Type)).Returns(ordersSerializer.Object);

            Mock<ODataResourceSerializer> serializer = new Mock<ODataResourceSerializer>(serializerProvider.Object);
            serializer.Setup(s => s.CreateSelectExpandNode(It.IsAny<ResourceContext>())).Returns(selectExpandNode);
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

            SelectExpandNode selectExpandNode = new SelectExpandNode
            {
                ExpandedProperties = new Dictionary<IEdmNavigationProperty, ExpandedNavigationSelectItem>()
            };
            selectExpandNode.ExpandedProperties[ordersProperty] =
                selectExpandClause.SelectedItems.OfType<ExpandedNavigationSelectItem>().Single();

            Mock<ODataWriter> writer = new Mock<ODataWriter>();
            writer.Setup(w => w.WriteStart(It.IsAny<ODataResourceSet>())).Callback(
                (ODataResourceSet feed) =>
                {
                    Assert.Null(feed.Count);
                    Assert.Null(feed.DeltaLink);
                    Assert.Null(feed.Id);
                    Assert.Empty(feed.InstanceAnnotations);
                    Assert.Null(feed.NextPageLink);
                }).Verifiable();
            Mock<ODataEdmTypeSerializer> ordersSerializer = new Mock<ODataEdmTypeSerializer>(ODataPayloadKind.Resource);
            Mock<ODataSerializerProvider> serializerProvider = new Mock<ODataSerializerProvider>();
            serializerProvider.Setup(p => p.GetEdmTypeSerializer(ordersProperty.Type)).Returns(ordersSerializer.Object);

            Mock<ODataResourceSerializer> serializer = new Mock<ODataResourceSerializer>(serializerProvider.Object);
            serializer.Setup(s => s.CreateSelectExpandNode(It.IsAny<ResourceContext>())).Returns(selectExpandNode);
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

            SelectExpandNode selectExpandNode = new SelectExpandNode
            {
                ExpandedProperties = new Dictionary<IEdmNavigationProperty, ExpandedNavigationSelectItem>()
            };
            selectExpandNode.ExpandedProperties[customerProperty] =
                selectExpandClause.SelectedItems.OfType<ExpandedNavigationSelectItem>().Single();

            Mock<ODataWriter> writer = new Mock<ODataWriter>();

            writer.Setup(w => w.WriteStart(null as ODataResource)).Verifiable();
            Mock<ODataEdmTypeSerializer> ordersSerializer = new Mock<ODataEdmTypeSerializer>(ODataPayloadKind.Resource);
            Mock<ODataSerializerProvider> serializerProvider = new Mock<ODataSerializerProvider>();
            serializerProvider.Setup(p => p.GetEdmTypeSerializer(customerProperty.Type))
                .Returns(ordersSerializer.Object);

            Mock<ODataResourceSerializer> serializer = new Mock<ODataResourceSerializer>(serializerProvider.Object);
            serializer.Setup(s => s.CreateSelectExpandNode(It.IsAny<ResourceContext>())).Returns(selectExpandNode);
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

            SelectExpandNode selectExpandNode = new SelectExpandNode
            {
                ExpandedProperties = new Dictionary<IEdmNavigationProperty, ExpandedNavigationSelectItem>()
            };
            selectExpandNode.ExpandedProperties[specialOrdersProperty] =
                selectExpandClause.SelectedItems.OfType<ExpandedNavigationSelectItem>().Single();

            Mock<ODataWriter> writer = new Mock<ODataWriter>();
            writer.Setup(w => w.WriteStart(It.IsAny<ODataResourceSet>())).Callback(
                (ODataResourceSet feed) =>
                {
                    Assert.Null(feed.Count);
                    Assert.Null(feed.DeltaLink);
                    Assert.Null(feed.Id);
                    Assert.Empty(feed.InstanceAnnotations);
                    Assert.Null(feed.NextPageLink);
                }).Verifiable();
            Mock<ODataEdmTypeSerializer> ordersSerializer = new Mock<ODataEdmTypeSerializer>(ODataPayloadKind.Resource);
            Mock<ODataSerializerProvider> serializerProvider = new Mock<ODataSerializerProvider>();
            serializerProvider.Setup(p => p.GetEdmTypeSerializer(specialOrdersProperty.Type))
                .Returns(ordersSerializer.Object);

            Mock<ODataResourceSerializer> serializer = new Mock<ODataResourceSerializer>(serializerProvider.Object);
            serializer.Setup(s => s.CreateSelectExpandNode(It.IsAny<ResourceContext>())).Returns(selectExpandNode);
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

            SelectExpandNode selectExpandNode = new SelectExpandNode
            {
                ExpandedProperties = new Dictionary<IEdmNavigationProperty, ExpandedNavigationSelectItem>()
            };
            selectExpandNode.ExpandedProperties[customerProperty] =
                selectExpandClause.SelectedItems.OfType<ExpandedNavigationSelectItem>().Single();

            Mock<ODataWriter> writer = new Mock<ODataWriter>();

            writer.Setup(w => w.WriteStart(null as ODataResource)).Verifiable();
            Mock<ODataEdmTypeSerializer> ordersSerializer = new Mock<ODataEdmTypeSerializer>(ODataPayloadKind.Resource);
            Mock<ODataSerializerProvider> serializerProvider = new Mock<ODataSerializerProvider>();
            serializerProvider.Setup(p => p.GetEdmTypeSerializer(customerProperty.Type))
                .Returns(ordersSerializer.Object);

            Mock<ODataResourceSerializer> serializer = new Mock<ODataResourceSerializer>(serializerProvider.Object);
            serializer.Setup(s => s.CreateSelectExpandNode(It.IsAny<ResourceContext>())).Returns(selectExpandNode);
            serializer.CallBase = true;

            // Act
            serializer.Object.WriteObjectInline(order.Object, _orderType, writer.Object, _writeContext);

            // Assert
            writer.Verify();
        }

        [Fact]
        public void CreateResource_ThrowsArgumentNull_SelectExpandNode()
        {
            ExceptionAssert.ThrowsArgumentNull(
                () => _serializer.CreateResource(selectExpandNode: null, resourceContext: _entityContext),
                "selectExpandNode");
        }

        [Fact]
        public void CreateResource_ThrowsArgumentNull_EntityContext()
        {
            ExceptionAssert.ThrowsArgumentNull(
                () => _serializer.CreateResource(new SelectExpandNode(), resourceContext: null),
                "resourceContext");
        }

        [Fact]
        public void CreateResource_Calls_CreateStructuralProperty_ForEachSelectedStructuralProperty()
        {
            // Arrange
            SelectExpandNode selectExpandNode = new SelectExpandNode
            {
                SelectedStructuralProperties = new HashSet<IEdmStructuralProperty>
                {
                    new Mock<IEdmStructuralProperty>().Object, new Mock<IEdmStructuralProperty>().Object
                }
            };
            ODataProperty[] properties = new ODataProperty[] { new ODataProperty(), new ODataProperty() };
            Mock<ODataResourceSerializer> serializer = new Mock<ODataResourceSerializer>(_serializerProvider);
            serializer.CallBase = true;

            serializer
                .Setup(s => s.CreateStructuralProperty(selectExpandNode.SelectedStructuralProperties.ElementAt(0), _entityContext))
                .Returns(properties[0])
                .Verifiable();
            serializer
                .Setup(s => s.CreateStructuralProperty(selectExpandNode.SelectedStructuralProperties.ElementAt(1), _entityContext))
                .Returns(properties[1])
                .Verifiable();

            // Act
            ODataResource entry = serializer.Object.CreateResource(selectExpandNode, _entityContext);

            // Assert
            serializer.Verify();
            Assert.Equal(properties, entry.Properties);
        }

        [Fact]
        public void CreateResource_SetsETagToNull_IfRequestIsNull()
        {
            // Arrange
            SelectExpandNode selectExpandNode = new SelectExpandNode
            {
                SelectedStructuralProperties = new HashSet<IEdmStructuralProperty>
                {
                    new Mock<IEdmStructuralProperty>().Object, new Mock<IEdmStructuralProperty>().Object
                }
            };
            ODataProperty[] properties = new[] { new ODataProperty(), new ODataProperty() };
            Mock<ODataResourceSerializer> serializer = new Mock<ODataResourceSerializer>(_serializerProvider);
            serializer.CallBase = true;

            serializer
                .Setup(s => s.CreateStructuralProperty(selectExpandNode.SelectedStructuralProperties.ElementAt(0), _entityContext))
                .Returns(properties[0]);
            serializer
                .Setup(s => s.CreateStructuralProperty(selectExpandNode.SelectedStructuralProperties.ElementAt(1), _entityContext))
                .Returns(properties[1]);

            // Act
            ODataResource entry = serializer.Object.CreateResource(selectExpandNode, _entityContext);

            // Assert
            Assert.Null(entry.ETag);
        }

        [Fact]
        public void CreateResource_SetsETagToNull_IfModelDontHaveConcurrencyProperty()
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
            _entityContext = new ResourceContext(_writeContext, orderSet.EntityType().AsReference(), order);

            SelectExpandNode selectExpandNode = new SelectExpandNode
            {
                SelectedStructuralProperties = new HashSet<IEdmStructuralProperty>
                {
                    new Mock<IEdmStructuralProperty>().Object, new Mock<IEdmStructuralProperty>().Object
                }
            };
            ODataProperty[] properties = new[] { new ODataProperty(), new ODataProperty() };
            Mock<ODataResourceSerializer> serializer = new Mock<ODataResourceSerializer>(_serializerProvider);
            serializer.CallBase = true;

            serializer
                .Setup(s => s.CreateStructuralProperty(selectExpandNode.SelectedStructuralProperties.ElementAt(0), _entityContext))
                .Returns(properties[0]);
            serializer
                .Setup(s => s.CreateStructuralProperty(selectExpandNode.SelectedStructuralProperties.ElementAt(1), _entityContext))
                .Returns(properties[1]);

            var config = RoutingConfigurationFactory.CreateWithRootContainer("Route");
            var request = RequestFactory.Create(config, "Route");
            _entityContext.Request = request;

            // Act
            ODataResource entry = serializer.Object.CreateResource(selectExpandNode, _entityContext);

            // Assert
            Assert.Null(entry.ETag);
        }

        [Fact]
        public void CreateResource_SetsEtagToNotNull_IfWithConcurrencyProperty()
        {
            // Arrange
            Mock<IEdmStructuralProperty> mockConcurrencyProperty = new Mock<IEdmStructuralProperty>();
            mockConcurrencyProperty.SetupGet(s => s.Name).Returns("City");
            SelectExpandNode selectExpandNode = new SelectExpandNode
            {
                SelectedStructuralProperties = new HashSet<IEdmStructuralProperty> { new Mock<IEdmStructuralProperty>().Object, mockConcurrencyProperty.Object }
            };
            ODataProperty[] properties = new[] { new ODataProperty(), new ODataProperty() };
            Mock<ODataResourceSerializer> serializer = new Mock<ODataResourceSerializer>(_serializerProvider);
            serializer.CallBase = true;
            serializer
                .Setup(s => s.CreateStructuralProperty(selectExpandNode.SelectedStructuralProperties.ElementAt(0), _entityContext))
                .Returns(properties[0]);
            serializer
                .Setup(s => s.CreateStructuralProperty(selectExpandNode.SelectedStructuralProperties.ElementAt(1), _entityContext))
                .Returns(properties[1]);

            Mock<IETagHandler> mockETagHandler = new Mock<IETagHandler>();
            string tag = "\"'anycity'\"";
            EntityTagHeaderValue etagHeaderValue = new EntityTagHeaderValue(tag, isWeak: true);
            mockETagHandler.Setup(e => e.CreateETag(It.IsAny<IDictionary<string, object>>())).Returns(etagHeaderValue);

            var configuration = RoutingConfigurationFactory.CreateWithRootContainer("Route", (config) =>
            {
#if NETCORE
                config.AddService<IETagHandler>(ServiceLifetime.Singleton, (services) => mockETagHandler.Object);
            });
#else
            });

            configuration.SetETagHandler(mockETagHandler.Object);
#endif

            var request = RequestFactory.Create(configuration, "Route");
            _entityContext.Request = request;

            // Act
            ODataResource resource = serializer.Object.CreateResource(selectExpandNode, _entityContext);

            // Assert
            Assert.Equal(etagHeaderValue.ToString(), resource.ETag);
        }

        [Fact]
        public void CreateResource_IgnoresProperty_IfCreateStructuralPropertyReturnsNull()
        {
            // Arrange
            SelectExpandNode selectExpandNode = new SelectExpandNode
            {
                SelectedStructuralProperties = new HashSet<IEdmStructuralProperty> { new Mock<IEdmStructuralProperty>().Object }
            };
            Mock<ODataResourceSerializer> serializer = new Mock<ODataResourceSerializer>(_serializerProvider);
            serializer.CallBase = true;

            serializer
                .Setup(s => s.CreateStructuralProperty(selectExpandNode.SelectedStructuralProperties.ElementAt(0), _entityContext))
                .Returns<ODataProperty>(null);

            // Act
            ODataResource entry = serializer.Object.CreateResource(selectExpandNode, _entityContext);

            // Assert
            serializer.Verify();
            Assert.Empty(entry.Properties);
        }

        [Fact]
        public void CreateResource_Calls_CreateODataAction_ForEachSelectAction()
        {
            // Arrange
            ODataAction[] actions = new ODataAction[] { new ODataAction(), new ODataAction() };
            SelectExpandNode selectExpandNode = new SelectExpandNode
            {
                SelectedActions = new HashSet<IEdmAction> { new Mock<IEdmAction>().Object, new Mock<IEdmAction>().Object }
            };
            Mock<ODataResourceSerializer> serializer = new Mock<ODataResourceSerializer>(_serializerProvider);
            serializer.CallBase = true;

            serializer.Setup(s => s.CreateODataAction(selectExpandNode.SelectedActions.ElementAt(0), _entityContext)).Returns(actions[0]).Verifiable();
            serializer.Setup(s => s.CreateODataAction(selectExpandNode.SelectedActions.ElementAt(1), _entityContext)).Returns(actions[1]).Verifiable();

            // Act
            ODataResource entry = serializer.Object.CreateResource(selectExpandNode, _entityContext);

            // Assert
            Assert.Equal(actions, entry.Actions);
            serializer.Verify();
        }

        [Fact]
        public void CreateResource_Works_ToAppendDynamicProperties_ForOpenEntityType()
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

            ODataResourceSerializer serializer = new ODataResourceSerializer(_serializerProvider);

            SelectExpandNode selectExpandNode = new SelectExpandNode(null, customerType, model);
            ODataSerializerContext writeContext = new ODataSerializerContext
            {
                Model = model,
                Path = new ODataPath(new EntitySetSegment(customers))
            };

            SimpleOpenCustomer customer = new SimpleOpenCustomer()
            {
                CustomerId = 991,
                Name = "Name #991",
                Address = new SimpleOpenAddress
                {
                    City = "a city",
                    Street = "a street",
                    Properties = new Dictionary<string, object> { { "ArrayProperty", new[] { "15", "14", "13" } } }
                },
                CustomerProperties = new Dictionary<string, object>()
            };
            DateTime dateTime = new DateTime(2014, 10, 24, 0, 0, 0, DateTimeKind.Utc);
            customer.CustomerProperties.Add("EnumProperty", SimpleEnum.Fourth);
            customer.CustomerProperties.Add("GuidProperty", new Guid("181D3A20-B41A-489F-9F15-F91F0F6C9ECA"));
            customer.CustomerProperties.Add("ListProperty", new List<int> { 5, 4, 3, 2, 1 });
            customer.CustomerProperties.Add("DateTimeProperty", dateTime);

            ResourceContext resourceContext = new ResourceContext(writeContext,
                customerType.ToEdmTypeReference(false) as IEdmEntityTypeReference, customer);

            // Act
            ODataResource resource = serializer.CreateResource(selectExpandNode, resourceContext);

            // Assert
            Assert.Equal("Default.Customer", resource.TypeName);
            Assert.Equal(6, resource.Properties.Count());

            // Verify the declared properties
            ODataProperty street = Assert.Single(resource.Properties.Where(p => p.Name == "CustomerId"));
            Assert.Equal(991, street.Value);

            ODataProperty city = Assert.Single(resource.Properties.Where(p => p.Name == "Name"));
            Assert.Equal("Name #991", city.Value);

            // Verify the nested open complex property
            Assert.Empty(resource.Properties.Where(p => p.Name == "Address"));

            // Verify the dynamic properties
            ODataProperty enumProperty = Assert.Single(resource.Properties.Where(p => p.Name == "EnumProperty"));
            ODataEnumValue enumValue = Assert.IsType<ODataEnumValue>(enumProperty.Value);
            Assert.Equal("Fourth", enumValue.Value);
            Assert.Equal("Default.SimpleEnum", enumValue.TypeName);

            ODataProperty guidProperty = Assert.Single(resource.Properties.Where(p => p.Name == "GuidProperty"));
            Assert.Equal(new Guid("181D3A20-B41A-489F-9F15-F91F0F6C9ECA"), guidProperty.Value);

            ODataProperty listProperty = Assert.Single(resource.Properties.Where(p => p.Name == "ListProperty"));
            ODataCollectionValue collectionValue = Assert.IsType<ODataCollectionValue>(listProperty.Value);
            Assert.Equal(new List<int> { 5, 4, 3, 2, 1 }, collectionValue.Items.OfType<int>().ToList());
            Assert.Equal("Collection(Edm.Int32)", collectionValue.TypeName);

            ODataProperty dateTimeProperty = Assert.Single(resource.Properties.Where(p => p.Name == "DateTimeProperty"));
            Assert.Equal(new DateTimeOffset(dateTime), dateTimeProperty.Value);
        }

        [Theory]
        [InlineData("@NS.test1", "notcontain")]
        [InlineData("NStest1.", "notcontain")]
        [InlineData("NStest", "contain")]
        public void CreateResource_ThrowsErr_WithBadInstanceAnnotations(string annotationName, string error)
        {
            var in1 = new Dictionary<string, object>();
            in1.Add(annotationName, 123);

            var instAnn = new ODataInstanceAnnotationContainer();

            ExceptionAssert.ThrowsArgument(
               () => AddInstanceAnnotations(instAnn, in1, string.Empty), annotationName,
               error == "notcontain" ? SRResources.InstanceAnnotationNotContain : SRResources.InstanceAnnotationShouldContain);
        }
        
        [Fact]
        public void CreateResource_Works_WithInstanceAnnotations()
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

            model.SetAnnotationValue(customerType, new ODataInstanceAnnotationContainerAnnotation(
            simpleOpenCustomer.GetProperty("InstanceAnnotations")));

            var in1 = new Dictionary<string, object>();
            in1.Add("NS.test1", 123);

            var in2 = new Dictionary<string, object>();
            in2.Add("NS.test2", 345);

            var instAnn = new ODataInstanceAnnotationContainer();
            AddInstanceAnnotations(instAnn, in1, string.Empty);
            AddInstanceAnnotations(instAnn, in2, "Name");
            
            ODataResourceSerializer serializer = new ODataResourceSerializer(_serializerProvider);

            SelectExpandNode selectExpandNode = new SelectExpandNode(null, customerType, model);
            ODataSerializerContext writeContext = new ODataSerializerContext
            {
                Model = model,
                Path = new ODataPath(new EntitySetSegment(customers))
            };

            SimpleOpenCustomer customer = new SimpleOpenCustomer()
            {
                CustomerId = 991,
                Name = "Name #991",
                Address = new SimpleOpenAddress
                {
                    City = "a city",
                    Street = "a street",
                    Properties = new Dictionary<string, object> { { "ArrayProperty", new[] { "15", "14", "13" } } }
                },
                InstanceAnnotations = instAnn
            };

            ResourceContext resourceContext = new ResourceContext(writeContext,
                customerType.ToEdmTypeReference(false) as IEdmEntityTypeReference, customer);

            // Act
            ODataResource resource = serializer.CreateResource(selectExpandNode, resourceContext);

            
            // Assert
            Assert.Equal("Default.Customer", resource.TypeName);
            Assert.Equal(2, resource.Properties.Count());

            // Verify the declared properties
            ODataProperty street = Assert.Single(resource.Properties.Where(p => p.Name == "CustomerId"));
            Assert.Equal(991, street.Value);

            ODataProperty city = Assert.Single(resource.Properties.Where(p => p.Name == "Name"));
            Assert.Equal("Name #991", city.Value);

            // Verify the nested open complex property
            Assert.Empty(resource.Properties.Where(p => p.Name == "Address"));

            //Verify Instance Annotations
            Assert.Equal(1, resource.InstanceAnnotations.Count);
            Assert.Equal(1, resource.Properties.ToList().Where(x => x.Name == "Name").First().InstanceAnnotations.Count);
        }

        [Fact]
        public void CreateResource_Works_WithInstanceAnnotations_WithNullAnnotation()
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

            model.SetAnnotationValue(customerType, new ODataInstanceAnnotationContainerAnnotation(
            simpleOpenCustomer.GetProperty("InstanceAnnotations")));

            var in1 = new Dictionary<string, object>();
            in1.Add("NS.test1", 123);

            var in2 = new Dictionary<string, object>();
            in2.Add("NS.test2", null);

            var instAnn = new ODataInstanceAnnotationContainer();
            AddInstanceAnnotations(instAnn, in1, string.Empty);
            AddInstanceAnnotations(instAnn, in2, "Name");

            ODataResourceSerializer serializer = new ODataResourceSerializer(_serializerProvider);

            SelectExpandNode selectExpandNode = new SelectExpandNode(null, customerType, model);
            ODataSerializerContext writeContext = new ODataSerializerContext
            {
                Model = model,
                Path = new ODataPath(new EntitySetSegment(customers))
            };

            SimpleOpenCustomer customer = new SimpleOpenCustomer()
            {
                CustomerId = 991,
                Name = "Name #991",
                Address = new SimpleOpenAddress
                {
                    City = "a city",
                    Street = "a street",
                    Properties = new Dictionary<string, object> { { "ArrayProperty", new[] { "15", "14", "13" } } }
                },
                InstanceAnnotations = instAnn
            };

            ResourceContext resourceContext = new ResourceContext(writeContext,
                customerType.ToEdmTypeReference(false) as IEdmEntityTypeReference, customer);

            // Act
            ODataResource resource = serializer.CreateResource(selectExpandNode, resourceContext);

            // Assert
            Assert.Equal("Default.Customer", resource.TypeName);
            Assert.Equal(2, resource.Properties.Count());

            //Verify Instance Annotations
            Assert.Equal(1, resource.InstanceAnnotations.Count);
            Assert.Equal(1, resource.Properties.ToList().Where(x => x.Name == "Name").First().InstanceAnnotations.Count);
        }


        [Fact]
        public void CreateResource_Works_WithInstanceAnnotations_WithUntypedAnnotation()
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

            model.SetAnnotationValue(customerType, new ODataInstanceAnnotationContainerAnnotation(
            simpleOpenCustomer.GetProperty("InstanceAnnotations")));

            IEdmEntityType entityType = model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "Customer");
            EdmEntityObject customer1 = new EdmEntityObject(entityType);
            customer1.TrySetPropertyValue("ID", 3);
            customer1.TrySetPropertyValue("Name", "TestName");

            var in1 = new Dictionary<string, object>();
            in1.Add("NS.test1", customer1);

            var instAnn = new ODataInstanceAnnotationContainer();
            AddInstanceAnnotations(instAnn, in1, string.Empty);
         
            ODataResourceSerializer serializer = new ODataResourceSerializer(_serializerProvider);

            SelectExpandNode selectExpandNode = new SelectExpandNode(null, customerType, model);
            ODataSerializerContext writeContext = new ODataSerializerContext
            {
                Model = model,
                Path = new ODataPath(new EntitySetSegment(customers))
            };

            SimpleOpenCustomer customer = new SimpleOpenCustomer()
            {
                CustomerId = 991,
                Name = "Name #991",
                Address = new SimpleOpenAddress
                {
                    City = "a city",
                    Street = "a street",
                    Properties = new Dictionary<string, object> { { "ArrayProperty", new[] { "15", "14", "13" } } }
                },
                InstanceAnnotations = instAnn
            };

            ResourceContext resourceContext = new ResourceContext(writeContext,
                customerType.ToEdmTypeReference(false) as IEdmEntityTypeReference, customer);

            // Act
            ODataResource resource = serializer.CreateResource(selectExpandNode, resourceContext);

            // Assert
            Assert.Equal("Default.Customer", resource.TypeName);
            Assert.Equal(2, resource.Properties.Count());
            
            //Verify Instance Annotations
            Assert.Equal(1, resource.InstanceAnnotations.Count);
            Assert.Equal("NS.test1", resource.InstanceAnnotations.First().Name);
            var resVal = resource.InstanceAnnotations.First().Value as ODataResourceValue;
            Assert.NotNull(resVal);
            Assert.Equal(4, resVal.Properties.Count());
            Assert.Equal("ID", resVal.Properties.First().Name);
            Assert.Equal(3, resVal.Properties.First().Value);
        }
        private void AddInstanceAnnotations(IODataInstanceAnnotationContainer container, IDictionary<string, object> annotation, string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                foreach (var kv in annotation)
                {
                    container.AddResourceAnnotation(kv.Key, kv.Value);
                }
            }
            else
            {
                foreach (var kv in annotation)
                {
                    container.AddPropertyAnnotation(propertyName, kv.Key, kv.Value);
                }
            }
        }

        [Fact]
        public void CreateResource_Works_WithInstanceAnnotations_OfTypeCollection()
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

            model.SetAnnotationValue(customerType, new ODataInstanceAnnotationContainerAnnotation(
            simpleOpenCustomer.GetProperty("InstanceAnnotations")));

            List<int> lst = new List<int> { 1, 2, 3 };
            var in1 = new Dictionary<string, object>();
            in1.Add("NS.test1", lst);

            var in2 = new Dictionary<string, object>();
            in2.Add("NS.test2", 345);

            var instAnn = new ODataInstanceAnnotationContainer();
            AddInstanceAnnotations(instAnn, in1, null);
            AddInstanceAnnotations(instAnn, in2, "Name");

            ODataResourceSerializer serializer = new ODataResourceSerializer(_serializerProvider);

            SelectExpandNode selectExpandNode = new SelectExpandNode(null, customerType, model);
            ODataSerializerContext writeContext = new ODataSerializerContext
            {
                Model = model,
                Path = new ODataPath(new EntitySetSegment(customers))
            };

            SimpleOpenCustomer customer = new SimpleOpenCustomer()
            {
                CustomerId = 991,
                Name = "Name #991",
                Address = new SimpleOpenAddress
                {
                    City = "a city",
                    Street = "a street",
                    Properties = new Dictionary<string, object> { { "ArrayProperty", new[] { "15", "14", "13" } } }
                },
                InstanceAnnotations = instAnn
            };

            ResourceContext resourceContext = new ResourceContext(writeContext,
                customerType.ToEdmTypeReference(false) as IEdmEntityTypeReference, customer);

            // Act
            ODataResource resource = serializer.CreateResource(selectExpandNode, resourceContext);

            // Assert
            Assert.Equal("Default.Customer", resource.TypeName);
            Assert.Equal(2, resource.Properties.Count());

            // Verify the declared properties
            ODataProperty street = Assert.Single(resource.Properties.Where(p => p.Name == "CustomerId"));
            Assert.Equal(991, street.Value);

            ODataProperty city = Assert.Single(resource.Properties.Where(p => p.Name == "Name"));
            Assert.Equal("Name #991", city.Value);

            // Verify the nested open complex property
            Assert.Empty(resource.Properties.Where(p => p.Name == "Address"));

            //Verify Instance Annotations
            Assert.Equal(1, resource.InstanceAnnotations.Count);
            Assert.Equal(1, resource.Properties.ToList().Where(x => x.Name == "Name").First().InstanceAnnotations.Count);

            var collVal = resource.InstanceAnnotations.First().Value as ODataCollectionValue;
            Assert.NotNull(collVal);
            Assert.Equal<IEnumerable<int>>((IEnumerable<int> )lst, (IEnumerable<int>)collVal.Items.Select(x => (int)x));
        }

        [Fact]
        public void CreateResource_Works_WithInstanceAnnotations_OfTypeComplex()
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

            model.SetAnnotationValue(customerType, new ODataInstanceAnnotationContainerAnnotation(
            simpleOpenCustomer.GetProperty("InstanceAnnotations")));

            var addr = new SimpleOpenAddress { City = "Redmond", Street = "152nd st" };
            
            var in1 = new Dictionary<string, object>();
            in1.Add("NS.test1", addr);

            var in2 = new Dictionary<string, object>();
            in2.Add("NS.test2", 345);

            var instAnn = new ODataInstanceAnnotationContainer();
            AddInstanceAnnotations(instAnn, in1, null);
            AddInstanceAnnotations(instAnn, in2, "Name");

            ODataResourceSerializer serializer = new ODataResourceSerializer(_serializerProvider);

            SelectExpandNode selectExpandNode = new SelectExpandNode(null, customerType, model);
            ODataSerializerContext writeContext = new ODataSerializerContext
            {
                Model = model,
                Path = new ODataPath(new EntitySetSegment(customers))
            };

            SimpleOpenCustomer customer = new SimpleOpenCustomer()
            {
                CustomerId = 991,
                Name = "Name #991",
                Address = new SimpleOpenAddress
                {
                    City = "a city",
                    Street = "a street",
                    Properties = new Dictionary<string, object> { { "ArrayProperty", new[] { "15", "14", "13" } } }                    
                },
                InstanceAnnotations = instAnn
            };

            ResourceContext resourceContext = new ResourceContext(writeContext,
                customerType.ToEdmTypeReference(false) as IEdmEntityTypeReference, customer);

            // Act
            ODataResource resource = serializer.CreateResource(selectExpandNode, resourceContext);

            // Assert
            Assert.Equal("Default.Customer", resource.TypeName);
            Assert.Equal(2, resource.Properties.Count());

            // Verify the declared properties
            ODataProperty street = Assert.Single(resource.Properties.Where(p => p.Name == "CustomerId"));
            Assert.Equal(991, street.Value);

            ODataProperty city = Assert.Single(resource.Properties.Where(p => p.Name == "Name"));
            Assert.Equal("Name #991", city.Value);

            // Verify the nested open complex property
            Assert.Empty(resource.Properties.Where(p => p.Name == "Address"));

            //Verify Instance Annotations
            Assert.Equal(1, resource.InstanceAnnotations.Count);
            Assert.Equal(typeof(ODataResourceValue), resource.InstanceAnnotations.First().Value.GetType());
            Assert.Equal("Default.Address", ((ODataResourceValue)resource.InstanceAnnotations.First().Value).TypeName);
            Assert.Equal("152nd st", ((ODataResourceValue)resource.InstanceAnnotations.First().Value).Properties.ToList()[0].Value);
            Assert.Equal("Redmond", ((ODataResourceValue)resource.InstanceAnnotations.First().Value).Properties.ToList()[1].Value);
            Assert.Equal(1, resource.Properties.ToList().Where(x => x.Name == "Name").First().InstanceAnnotations.Count);
        }


        [Fact]
        public void CreateResource_Works_WithInstanceAnnotations_OfTypeComplex_Collection()
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

            model.SetAnnotationValue(customerType, new ODataInstanceAnnotationContainerAnnotation(
            simpleOpenCustomer.GetProperty("InstanceAnnotations")));

            var addr = new SimpleOpenAddress { City = "Redmond", Street = "152nd st" };

            var lstAddr = new List<SimpleOpenAddress> (){ addr };
            var in1 = new Dictionary<string, object>();
            in1.Add("NS.test1", lstAddr);

            var in2 = new Dictionary<string, object>();
            in2.Add("NS.test2", 345);

            var instAnn = new ODataInstanceAnnotationContainer();
            AddInstanceAnnotations(instAnn, in1, null);
            AddInstanceAnnotations(instAnn, in2, "Name");

            ODataResourceSerializer serializer = new ODataResourceSerializer(_serializerProvider);

            SelectExpandNode selectExpandNode = new SelectExpandNode(null, customerType, model);
            ODataSerializerContext writeContext = new ODataSerializerContext
            {
                Model = model,
                Path = new ODataPath(new EntitySetSegment(customers))
            };

            SimpleOpenCustomer customer = new SimpleOpenCustomer()
            {
                CustomerId = 991,
                Name = "Name #991",
                Address = new SimpleOpenAddress
                {
                    City = "a city",
                    Street = "a street",
                    Properties = new Dictionary<string, object> { { "ArrayProperty", new[] { "15", "14", "13" } } }
                },
                InstanceAnnotations = instAnn
            };

            ResourceContext resourceContext = new ResourceContext(writeContext,
                customerType.ToEdmTypeReference(false) as IEdmEntityTypeReference, customer);

            // Act
            ODataResource resource = serializer.CreateResource(selectExpandNode, resourceContext);

            // Assert
            Assert.Equal("Default.Customer", resource.TypeName);
            Assert.Equal(2, resource.Properties.Count());

            // Verify the declared properties
            ODataProperty street = Assert.Single(resource.Properties.Where(p => p.Name == "CustomerId"));
            Assert.Equal(991, street.Value);

            ODataProperty city = Assert.Single(resource.Properties.Where(p => p.Name == "Name"));
            Assert.Equal("Name #991", city.Value);

            // Verify the nested open complex property
            Assert.Empty(resource.Properties.Where(p => p.Name == "Address"));

            //Verify Instance Annotations
            Assert.Equal(1, resource.InstanceAnnotations.Count);
            Assert.Equal(typeof(ODataCollectionValue), resource.InstanceAnnotations.First().Value.GetType());
            Assert.True( ((ODataCollectionValue)(resource.InstanceAnnotations.First().Value)).Items.Count() ==1);           
            Assert.Equal("Collection(Default.Address)", ((ODataCollectionValue)resource.InstanceAnnotations.First().Value).TypeName);            
            Assert.Equal("152nd st", ((ODataResourceValue)((ODataCollectionValue)(resource.InstanceAnnotations.First().Value)).Items.ToList().First()).Properties.ToList()[0].Value);
            Assert.Equal("Redmond", ((ODataResourceValue)((ODataCollectionValue)(resource.InstanceAnnotations.First().Value)).Items.ToList().First()).Properties.ToList()[1].Value);
            Assert.Equal(1, resource.Properties.ToList().Where(x => x.Name == "Name").First().InstanceAnnotations.Count);
        }

        [Fact]
        public void CreateResource_Works_WithInstanceAnnotations_OnComplexType_AndPropertyInComplexType()
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

            model.SetAnnotationValue(customerType, new ODataInstanceAnnotationContainerAnnotation(
            simpleOpenCustomer.GetProperty("InstanceAnnotations")));

            model.SetAnnotationValue(addressType, new ODataInstanceAnnotationContainerAnnotation(
            simpleOpenAddress.GetProperty("InstanceAnnotations")));

            var addr = new SimpleOpenAddress { City = "Redmond", Street = "152nd st" };

            var lstAddr = new List<SimpleOpenAddress>() { addr };
            var in1 = new Dictionary<string, object>();
            in1.Add("NS.test1", lstAddr);

            var in2 = new Dictionary<string, object>();
            in2.Add("NS.test2", 345);

            var instAnn = new ODataInstanceAnnotationContainer();
            AddInstanceAnnotations(instAnn, in1, null);
            AddInstanceAnnotations(instAnn, in2, "City");

            ODataResourceSerializer serializer = new ODataResourceSerializer(_serializerProvider);

            SelectExpandNode selectExpandNode = new SelectExpandNode(null, addressType, model);
            ODataSerializerContext writeContext = new ODataSerializerContext
            {
                Model = model,
                Path = new ODataPath(new EntitySetSegment(customers))
            };
            
            SimpleOpenCustomer customer = new SimpleOpenCustomer()
            {
                CustomerId = 991,
                Name = "Name #991",
                Address = new SimpleOpenAddress
                {
                    City = "a city",
                    Street = "a street",
                    Properties = new Dictionary<string, object> { { "ArrayProperty", new[] { "15", "14", "13" } } },
                    InstanceAnnotations = instAnn
                }
                
            };

            ResourceContext resourceContext = new ResourceContext(writeContext,
                addressType.ToEdmTypeReference(false) as IEdmComplexTypeReference, customer.Address);

            // Act
            ODataResource resource = serializer.CreateResource(selectExpandNode, resourceContext);

            // Assert
            Assert.Equal("Default.Address", resource.TypeName);
            Assert.Equal(2, resource.Properties.Count());

            // Verify the declared properties
            ODataProperty street = Assert.Single(resource.Properties.Where(p => p.Name == "Street"));
            Assert.Equal("a street", street.Value);

            //Verify Instance Annotations
            Assert.Equal(1, resource.InstanceAnnotations.Count);
            Assert.Equal(typeof(ODataCollectionValue), resource.InstanceAnnotations.First().Value.GetType());
            Assert.True(((ODataCollectionValue)(resource.InstanceAnnotations.First().Value)).Items.Count() == 1);
            Assert.Equal("Collection(Default.Address)", ((ODataCollectionValue)resource.InstanceAnnotations.First().Value).TypeName);
            Assert.Equal("152nd st", ((ODataResourceValue)((ODataCollectionValue)(resource.InstanceAnnotations.First().Value)).Items.ToList().First()).Properties.ToList()[0].Value);
            Assert.Equal("Redmond", ((ODataResourceValue)((ODataCollectionValue)(resource.InstanceAnnotations.First().Value)).Items.ToList().First()).Properties.ToList()[1].Value);
            Assert.Equal(1, resource.Properties.ToList().Where(x => x.Name == "City").First().InstanceAnnotations.Count);
        }


        [Fact]
        public void CreateResource_Works_ToAppendDynamicProperties_ForOpenEntityType_WithAnnotations()
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

            model.SetAnnotationValue(customerType, new ODataInstanceAnnotationContainerAnnotation(
          simpleOpenCustomer.GetProperty("InstanceAnnotations")));

            var in1 = new Dictionary<string, object>();
            in1.Add("NS.test1", 123);

            var in2 = new Dictionary<string, object>();
            in2.Add("NS.test2", 345);

            var instAnn = new ODataInstanceAnnotationContainer();
            AddInstanceAnnotations(instAnn, in1, null);
            AddInstanceAnnotations(instAnn, in2, "Name");

            ODataResourceSerializer serializer = new ODataResourceSerializer(_serializerProvider);

            SelectExpandNode selectExpandNode = new SelectExpandNode(null, customerType, model);
            ODataSerializerContext writeContext = new ODataSerializerContext
            {
                Model = model,
                Path = new ODataPath(new EntitySetSegment(customers))
            };

            SimpleOpenCustomer customer = new SimpleOpenCustomer()
            {
                CustomerId = 991,
                Name = "Name #991",
                Address = new SimpleOpenAddress
                {
                    City = "a city",
                    Street = "a street",
                    Properties = new Dictionary<string, object> { { "ArrayProperty", new[] { "15", "14", "13" } } }
                },
                CustomerProperties = new Dictionary<string, object>(),
                InstanceAnnotations = instAnn
            };
            DateTime dateTime = new DateTime(2014, 10, 24, 0, 0, 0, DateTimeKind.Utc);
            customer.CustomerProperties.Add("EnumProperty", SimpleEnum.Fourth);
            customer.CustomerProperties.Add("GuidProperty", new Guid("181D3A20-B41A-489F-9F15-F91F0F6C9ECA"));
            customer.CustomerProperties.Add("ListProperty", new List<int> { 5, 4, 3, 2, 1 });
            customer.CustomerProperties.Add("DateTimeProperty", dateTime);

            ResourceContext resourceContext = new ResourceContext(writeContext,
                customerType.ToEdmTypeReference(false) as IEdmEntityTypeReference, customer);

            // Act
            ODataResource resource = serializer.CreateResource(selectExpandNode, resourceContext);

            // Assert
            Assert.Equal("Default.Customer", resource.TypeName);
            Assert.Equal(6, resource.Properties.Count());

            // Verify the declared properties
            ODataProperty street = Assert.Single(resource.Properties.Where(p => p.Name == "CustomerId"));
            Assert.Equal(991, street.Value);

            ODataProperty city = Assert.Single(resource.Properties.Where(p => p.Name == "Name"));
            Assert.Equal("Name #991", city.Value);

            // Verify the nested open complex property
            Assert.Empty(resource.Properties.Where(p => p.Name == "Address"));

            // Verify the dynamic properties
            ODataProperty enumProperty = Assert.Single(resource.Properties.Where(p => p.Name == "EnumProperty"));
            ODataEnumValue enumValue = Assert.IsType<ODataEnumValue>(enumProperty.Value);
            Assert.Equal("Fourth", enumValue.Value);
            Assert.Equal("Default.SimpleEnum", enumValue.TypeName);

            ODataProperty guidProperty = Assert.Single(resource.Properties.Where(p => p.Name == "GuidProperty"));
            Assert.Equal(new Guid("181D3A20-B41A-489F-9F15-F91F0F6C9ECA"), guidProperty.Value);

            ODataProperty listProperty = Assert.Single(resource.Properties.Where(p => p.Name == "ListProperty"));
            ODataCollectionValue collectionValue = Assert.IsType<ODataCollectionValue>(listProperty.Value);
            Assert.Equal(new List<int> { 5, 4, 3, 2, 1 }, collectionValue.Items.OfType<int>().ToList());
            Assert.Equal("Collection(Edm.Int32)", collectionValue.TypeName);

            ODataProperty dateTimeProperty = Assert.Single(resource.Properties.Where(p => p.Name == "DateTimeProperty"));
            Assert.Equal(new DateTimeOffset(dateTime), dateTimeProperty.Value);

            //Verify Instance Annotations
            Assert.Equal(1, resource.InstanceAnnotations.Count);
            Assert.Equal(1, resource.Properties.ToList().Where(x=>x.Name=="Name").First().InstanceAnnotations.Count);
        }

        [Theory]
        [InlineData(true, 4)]
        [InlineData(false, 3)]
        public void CreateResource_Works_ToAppendNullDynamicProperties_ForOpenEntityType(bool enableNullDynamicProperty, int count)
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

            model.SetAnnotationValue(customerType, new ODataInstanceAnnotationContainerAnnotation(
              simpleOpenCustomer.GetProperty("InstanceAnnotations")));

            ODataResourceSerializer serializer = new ODataResourceSerializer(_serializerProvider);

            var config = RoutingConfigurationFactory.CreateWithRootContainer("Route");
            config.SetSerializeNullDynamicProperty(enableNullDynamicProperty);
            var request = RequestFactory.Create(config, "Route");
            SelectExpandNode selectExpandNode = new SelectExpandNode(null, customerType, model);
            ODataSerializerContext writeContext = new ODataSerializerContext
            {
                Model = model,
                Path = new ODataPath(new EntitySetSegment(customers)),
                Request = request
            };

            var in1 = new Dictionary<string, object>();
            in1.Add("NS.test1", 123);

            var in2 = new Dictionary<string, object>();
            in2.Add("NS.test2", 345);
            var instAnn = new ODataInstanceAnnotationContainer();
            AddInstanceAnnotations(instAnn, in1, string.Empty);
            AddInstanceAnnotations(instAnn, in2, "Name");


            SimpleOpenCustomer customer = new SimpleOpenCustomer()
            {
                CustomerId = 991,
                Name = "Name #991",
                Address = new SimpleOpenAddress
                {
                    City = "a city",
                    Street = "a street",
                    Properties = new Dictionary<string, object> { { "ArrayProperty", new[] { "15", "14", "13" } } }
                },
                CustomerProperties = new Dictionary<string, object>(),
                InstanceAnnotations = instAnn
            };

            customer.CustomerProperties.Add("GuidProperty", new Guid("181D3A20-B41A-489F-9F15-F91F0F6C9ECA"));
            customer.CustomerProperties.Add("NullProperty", null);

            ResourceContext entityContext = new ResourceContext(writeContext,
                customerType.ToEdmTypeReference(false) as IEdmEntityTypeReference, customer);

            // Act
            ODataResource resource = serializer.CreateResource(selectExpandNode, entityContext);

            // Assert
            Assert.Equal("Default.Customer", resource.TypeName);
            Assert.Equal(count, resource.Properties.Count());

            // Verify the declared properties
            ODataProperty street = Assert.Single(resource.Properties.Where(p => p.Name == "CustomerId"));
            Assert.Equal(991, street.Value);

            ODataProperty city = Assert.Single(resource.Properties.Where(p => p.Name == "Name"));
            Assert.Equal("Name #991", city.Value);

            // Verify the nested open complex property
            Assert.Empty(resource.Properties.Where(p => p.Name == "Address"));

            // Verify the dynamic properties
            ODataProperty guidProperty = Assert.Single(resource.Properties.Where(p => p.Name == "GuidProperty"));
            Assert.Equal(new Guid("181D3A20-B41A-489F-9F15-F91F0F6C9ECA"), guidProperty.Value);

            ODataProperty nullProperty = resource.Properties.SingleOrDefault(p => p.Name == "NullProperty");
            if (enableNullDynamicProperty)
            {
                Assert.NotNull(nullProperty);
                Assert.Null(nullProperty.Value);
            }
            else
            {
                Assert.Null(nullProperty);
            }
        }

        [Fact]
        public void CreateStructuralProperty_ThrowsArgumentNull_StructuralProperty()
        {
            ExceptionAssert.ThrowsArgumentNull(
                () => _serializer.CreateStructuralProperty(structuralProperty: null, resourceContext: null),
                "structuralProperty");
        }

        [Fact]
        public void CreateStructuralProperty_ThrowsArgumentNull_EntityContext()
        {
            Mock<IEdmStructuralProperty> property = new Mock<IEdmStructuralProperty>();

            ExceptionAssert.ThrowsArgumentNull(
                () => _serializer.CreateStructuralProperty(property.Object, resourceContext: null),
                "resourceContext");
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

            var serializer = new ODataResourceSerializer(serializerProvider.Object);

            // Act & Assert
            ExceptionAssert.Throws<SerializationException>(
                () => serializer.CreateStructuralProperty(property.Object, new ResourceContext { EdmObject = entity }),
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

            var serializer = new ODataResourceSerializer(serializerProvider.Object);
            ResourceContext entityContext = new ResourceContext(_writeContext, _customerType, entity);

            // Act
            ODataProperty createdProperty = serializer.CreateStructuralProperty(property.Object, entityContext);

            // Assert
            innerSerializer.Verify();
            Assert.Equal("PropertyName", createdProperty.Name);
            Assert.Equal(propertyValue, createdProperty.Value);
        }

        private bool Verify(ResourceContext instanceContext, object instance, ODataSerializerContext writeContext)
        {
            Assert.Same(instance, (instanceContext.EdmObject as TypedEdmEntityObject).Instance);
            Assert.Equal(writeContext.Model, instanceContext.EdmModel);
            Assert.Equal(writeContext.NavigationSource, instanceContext.NavigationSource);
            Assert.Equal(writeContext.Request, instanceContext.Request);
            Assert.Equal(writeContext.SkipExpensiveAvailabilityChecks, instanceContext.SkipExpensiveAvailabilityChecks);
#if NETFX // Only AspNet version has Url property
            Assert.Equal(writeContext.Url, instanceContext.Url);
#endif
            return true;
        }

        [Fact]
        public void CreateNavigationLink_ThrowsArgumentNull_NavigationProperty()
        {
            ExceptionAssert.ThrowsArgumentNull(
                () => _serializer.CreateNavigationLink(navigationProperty: null, resourceContext: _entityContext),
                "navigationProperty");
        }

        [Fact]
        public void CreateNavigationLink_ThrowsArgumentNull_EntityContext()
        {
            IEdmNavigationProperty navigationProperty = new Mock<IEdmNavigationProperty>().Object;
            ExceptionAssert.ThrowsArgumentNull(
                () => _serializer.CreateNavigationLink(navigationProperty, resourceContext: null),
                "resourceContext");
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

            Mock<ODataResourceSerializer> serializer = new Mock<ODataResourceSerializer>(_serializerProvider);
            serializer.CallBase = true;

            // Act
            ODataNestedResourceInfo navigationLink = serializer.Object.CreateNavigationLink(property1, _entityContext);

            // Assert
            Assert.Equal("Property1", navigationLink.Name);
            Assert.Equal(navigationLinkUri, navigationLink.Url);
        }

        [Fact]
        public void CreateResource_UsesCorrectTypeName()
        {
            ResourceContext instanceContext =
                new ResourceContext { StructuredType = _customerType.EntityDefinition(), SerializerContext = _writeContext };
            Mock<ODataResourceSerializer> serializer = new Mock<ODataResourceSerializer>(_serializerProvider);
            serializer.CallBase = true;
            SelectExpandNode selectExpandNode = new SelectExpandNode();

            // Act
            ODataResource entry = serializer.Object.CreateResource(selectExpandNode, instanceContext);

            // Assert
            Assert.Equal("Default.Customer", entry.TypeName);
        }

        [Fact]
        public void CreateODataAction_ThrowsArgumentNull_Action()
        {
            ExceptionAssert.ThrowsArgumentNull(
                () => _serializer.CreateODataAction(action: null, resourceContext: null),
                "action");
        }

        [Fact]
        public void CreateODataAction_ThrowsArgumentNull_EntityContext()
        {
            IEdmAction action = new Mock<IEdmAction>().Object;

            ExceptionAssert.ThrowsArgumentNull(
                () => _serializer.CreateODataAction(action, resourceContext: null),
                "resourceContext");
        }

        [Fact]
        public void CreateResource_WritesCorrectIdLink()
        {
            // Arrange
            ResourceContext instanceContext = new ResourceContext
            {
                SerializerContext = _writeContext,
                StructuredType = _customerType.EntityDefinition()
            };

            bool customIdLinkbuilderCalled = false;
            NavigationSourceLinkBuilderAnnotation linkAnnotation = new MockNavigationSourceLinkBuilderAnnotation
            {
                IdLinkBuilder = new SelfLinkBuilder<Uri>((ResourceContext context) =>
                {
                    Assert.Same(instanceContext, context);
                    customIdLinkbuilderCalled = true;
                    return new Uri("http://sample_id_link");
                },
                followsConventions: false)
            };
            _model.SetNavigationSourceLinkBuilder(_customerSet, linkAnnotation);

            Mock<ODataResourceSerializer> serializer = new Mock<ODataResourceSerializer>(_serializerProvider);
            serializer.CallBase = true;
            SelectExpandNode selectExpandNode = new SelectExpandNode();

            // Act
            ODataResource entry = serializer.Object.CreateResource(selectExpandNode, instanceContext);

            // Assert
            Assert.True(customIdLinkbuilderCalled);
        }

        [Fact]
        public void WriteObjectInline_WritesCorrectEditLink()
        {
            // Arrange
            ResourceContext instanceContext = new ResourceContext
            {
                SerializerContext = _writeContext,
                StructuredType = _customerType.EntityDefinition()
            };
            bool customEditLinkbuilderCalled = false;
            NavigationSourceLinkBuilderAnnotation linkAnnotation = new MockNavigationSourceLinkBuilderAnnotation
            {
                EditLinkBuilder = new SelfLinkBuilder<Uri>((ResourceContext context) =>
                {
                    Assert.Same(instanceContext, context);
                    customEditLinkbuilderCalled = true;
                    return new Uri("http://sample_edit_link");
                },
                followsConventions: false)
            };
            _model.SetNavigationSourceLinkBuilder(_customerSet, linkAnnotation);

            Mock<ODataResourceSerializer> serializer = new Mock<ODataResourceSerializer>(_serializerProvider);
            serializer.CallBase = true;
            SelectExpandNode selectExpandNode = new SelectExpandNode();

            // Act
            ODataResource entry = serializer.Object.CreateResource(selectExpandNode, instanceContext);

            // Assert
            Assert.True(customEditLinkbuilderCalled);
        }

        [Fact]
        public void WriteObjectInline_WritesCorrectReadLink()
        {
            // Arrange
            ResourceContext instanceContext = new ResourceContext(_writeContext, _customerType, 42);
            bool customReadLinkbuilderCalled = false;
            NavigationSourceLinkBuilderAnnotation linkAnnotation = new MockNavigationSourceLinkBuilderAnnotation
            {
                ReadLinkBuilder = new SelfLinkBuilder<Uri>((ResourceContext context) =>
                {
                    Assert.Same(instanceContext, context);
                    customReadLinkbuilderCalled = true;
                    return new Uri("http://sample_read_link");
                },
                followsConventions: false)
            };

            _model.SetNavigationSourceLinkBuilder(_customerSet, linkAnnotation);

            Mock<ODataResourceSerializer> serializer = new Mock<ODataResourceSerializer>(_serializerProvider);
            serializer.CallBase = true;
            SelectExpandNode selectExpandNode = new SelectExpandNode();

            // Act
            ODataResource entry = serializer.Object.CreateResource(selectExpandNode, instanceContext);

            // Assert
            Assert.True(customReadLinkbuilderCalled);
        }

        [Fact]
        public void CreateSelectExpandNode_ThrowsArgumentNull_EntityContext()
        {
            ExceptionAssert.ThrowsArgumentNull(
                () => _serializer.CreateSelectExpandNode(resourceContext: null),
                "resourceContext");
        }

        [Fact]
        public void AddTypeNameAnnotationAsNeeded_AddsAnnotation_IfTypeOfPathDoesNotMatchEntryType()
        {
            // Arrange
            string expectedTypeName = "TypeName";
            ODataResource entry = new ODataResource
            {
                TypeName = expectedTypeName
            };

            // Act
            ODataResourceSerializer.AddTypeNameAnnotationAsNeeded(entry, _customerType.EntityDefinition(), ODataMetadataLevel.MinimalMetadata);

            // Assert
            ODataTypeAnnotation annotation = entry.TypeAnnotation;
            Assert.NotNull(annotation); // Guard
            Assert.Equal(expectedTypeName, annotation.TypeName);
        }

        [Fact] // Issue 984: Redundant type name serialization in OData JSON light minimal metadata mode
        public void AddTypeNameAnnotationAsNeeded_AddsAnnotationWithNullValue_IfTypeOfPathMatchesEntryType()
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            ODataResource entry = new ODataResource
            {
                TypeName = model.SpecialCustomer.FullName()
            };

            // Act
            ODataResourceSerializer.AddTypeNameAnnotationAsNeeded(entry, model.SpecialCustomer, ODataMetadataLevel.MinimalMetadata);

            // Assert
            ODataTypeAnnotation annotation = entry.TypeAnnotation;
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
        public void ShouldSuppressTypeNameSerialization(string resourceType, string entitySetType,
            TestODataMetadataLevel metadataLevel, bool expectedResult)
        {
            // Arrange
            ODataResource resource = new ODataResource
            {
                // The caller uses a namespace-qualified name, which this test leaves empty.
                TypeName = "NS." + resourceType
            };
            IEdmEntityType edmType = CreateEntityTypeWithName(entitySetType);

            // Act
            bool actualResult = ODataResourceSerializer.ShouldSuppressTypeNameSerialization(resource, edmType,
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

            OperationLinkBuilder linkBuilder = new OperationLinkBuilder((ResourceContext a) => new Uri(expectedTarget),
                followsConventions: false);
            IEdmDirectValueAnnotationsManager annotationsManager = CreateFakeAnnotationsManager();
            annotationsManager.SetOperationLinkBuilder(action, linkBuilder);
            annotationsManager.SetIsAlwaysBindable(action);
            IEdmModel model = CreateFakeModel(annotationsManager);

            ResourceContext context = CreateContext(model, expectedMetadataPrefix);
            context.SerializerContext.MetadataLevel = ODataMetadataLevel.FullMetadata;

            // Act
            ODataAction actualAction = _serializer.CreateODataAction(action, context);

            // Assert
            // In ASP.NET Core, it's using the global UriHelper to pick up the first Router to generate the Uri.
            // Owing that there's no router created in this case, a default OData router will be used to generate the Uri.
            // The default OData router will add '$metadata' after the route prefix.
#if NETCORE
            string expectedMetadata = expectedMetadataPrefix + "/$metadata#" + expectedNamespace + "." + expectedActionName;
#else
            string expectedMetadata = expectedMetadataPrefix + "#" + expectedNamespace + "." + expectedActionName;
#endif
            ODataAction expectedAction = new ODataAction
            {
                Metadata = new Uri(expectedMetadata),
                Target = new Uri(expectedTarget),
                Title = expectedActionName
            };

            AssertEqual(expectedAction, actualAction);
        }

        [Fact]
        public void CreateODataAction_OmitsAction_WheOperationLinkBuilderReturnsNull()
        {
            // Arrange
            IEdmAction action = CreateFakeAction("IgnoreAction");

            OperationLinkBuilder linkBuilder = new OperationLinkBuilder((ResourceContext a) => null, followsConventions: false);
            IEdmDirectValueAnnotationsManager annotationsManager = CreateFakeAnnotationsManager();
            annotationsManager.SetOperationLinkBuilder(action, linkBuilder);

            IEdmModel model = CreateFakeModel(annotationsManager);

            ResourceContext context = CreateContext(model);
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

            OperationLinkBuilder linkBuilder = new OperationLinkBuilder((ResourceContext a) => new Uri("aa://IgnoreTarget"),
                followsConventions: false);
            IEdmDirectValueAnnotationsManager annotationsManager = CreateFakeAnnotationsManager();
            annotationsManager.SetOperationLinkBuilder(action, linkBuilder);

            IEdmModel model = CreateFakeModel(annotationsManager);

            ResourceContext context = CreateContext(model, expectedMetadataPrefix);
            context.SerializerContext.MetadataLevel = ODataMetadataLevel.MinimalMetadata;

            // Act
            ODataAction actualAction = _serializer.CreateODataAction(action, context);

            // Assert
            Assert.NotNull(actualAction);
            // Assert
            // In ASP.NET Core, it's using the global UriHelper to pick up the first Router to generate the Uri.
            // Owing that there's no router created in this case, a default OData router will be used to generate the Uri.
            // The default OData router will add '$metadata' after the route prefix.
#if NETCORE
            string expectedMetadata = expectedMetadataPrefix + "/$metadata#" + expectedNamespace + "." + expectedActionName;
#else
            string expectedMetadata = expectedMetadataPrefix + "#" + expectedNamespace + "." + expectedActionName;
#endif
            AssertEqual(new Uri(expectedMetadata), actualAction.Metadata);
        }

        [Fact]
        public void CreateODataAction_SkipsAlwaysAvailableAction_PerShouldOmitAction()
        {
            // Arrange
            IEdmAction action = CreateFakeAction("action");

            OperationLinkBuilder linkBuilder = new OperationLinkBuilder((ResourceContext a) => new Uri("aa://IgnoreTarget"),
                followsConventions: true);
            IEdmDirectValueAnnotationsManager annotationsManager = CreateFakeAnnotationsManager();
            annotationsManager.SetOperationLinkBuilder(action, linkBuilder);
            annotationsManager.SetIsAlwaysBindable(action);

            IEdmModel model = CreateFakeModel(annotationsManager);

            ResourceContext context = CreateContext(model);
            context.SerializerContext.MetadataLevel = ODataMetadataLevel.MinimalMetadata;

            // Act
            ODataAction actualAction = _serializer.CreateODataAction(action, context);

            // Assert
            Assert.Null(actualAction);
        }

        [Fact]
        public void CreateODataAction_IncludesTitle()
        {
            // Arrange
            string expectedActionName = "Action";

            IEdmAction action = CreateFakeAction(expectedActionName);

            OperationLinkBuilder linkBuilder = new OperationLinkBuilder((ResourceContext a) => new Uri("aa://IgnoreTarget"),
                followsConventions: false);
            IEdmDirectValueAnnotationsManager annotationsManager = CreateFakeAnnotationsManager();
            annotationsManager.SetOperationLinkBuilder(action, linkBuilder);

            IEdmModel model = CreateFakeModel(annotationsManager);

            ResourceContext context = CreateContext(model, "http://IgnoreMetadataPath");
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

            OperationLinkBuilder linkBuilder = new OperationLinkBuilder((ResourceContext a) => new Uri("aa://Ignore"),
                followsConventions: false);
            IEdmDirectValueAnnotationsManager annotationsManager = CreateFakeAnnotationsManager();
            annotationsManager.SetOperationLinkBuilder(action, linkBuilder);

            IEdmModel model = CreateFakeModel(annotationsManager);

            ResourceContext context = CreateContext(model, "http://IgnoreMetadataPath");
            context.SerializerContext.MetadataLevel = (ODataMetadataLevel)metadataLevel;

            // Act
            ODataAction actualAction = _serializer.CreateODataAction(action, context);

            // Assert
            Assert.NotNull(actualAction);
            Assert.Null(actualAction.Title);
        }

        [Fact]
        public void CreateODataAction_IncludesTarget_IfDoesnotFollowODataConvention()
        {
            // Arrange
            Uri expectedTarget = new Uri("aa://Target");

            IEdmAction action = CreateFakeAction("IgnoreAction");

            OperationLinkBuilder linkBuilder = new OperationLinkBuilder((ResourceContext a) => expectedTarget, followsConventions: false);
            IEdmDirectValueAnnotationsManager annotationsManager = CreateFakeAnnotationsManager();
            annotationsManager.SetOperationLinkBuilder(action, linkBuilder);

            IEdmModel model = CreateFakeModel(annotationsManager);

            ResourceContext context = CreateContext(model, "http://IgnoreMetadataPath");
            context.SerializerContext.MetadataLevel = (ODataMetadataLevel)TestODataMetadataLevel.FullMetadata;

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

            OperationLinkBuilder linkBuilder = new OperationLinkBuilder((ResourceContext a) => new Uri("aa://Ignore"),
                followsConventions: true);
            IEdmDirectValueAnnotationsManager annotationsManager = CreateFakeAnnotationsManager();
            annotationsManager.SetOperationLinkBuilder(action, linkBuilder);

            IEdmModel model = CreateFakeModel(annotationsManager);

            ResourceContext context = CreateContext(model, "http://IgnoreMetadataPath");
            context.SerializerContext.MetadataLevel = (ODataMetadataLevel)metadataLevel;

            // Act
            ODataAction actualAction = _serializer.CreateODataAction(action, context);

            // Assert
            Assert.Null(actualAction);
        }

        [Fact]
        public void CreateODataFunction_IncludesEverything_ForFullMetadata()
        {
            // Arrange
            string expectedTarget = "aa://Target";
            string expectedMetadataPrefix = "http://Metadata";

            IEdmTypeReference returnType = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Boolean, isNullable: false);
            IEdmFunction function = new EdmFunction("NS", "Function", returnType, isBound: true, entitySetPathExpression: null, isComposable: false);

            OperationLinkBuilder linkBuilder = new OperationLinkBuilder((ResourceContext a) => new Uri(expectedTarget),
                followsConventions: false);
            IEdmDirectValueAnnotationsManager annotationsManager = CreateFakeAnnotationsManager();
            annotationsManager.SetOperationLinkBuilder(function, linkBuilder);
            annotationsManager.SetIsAlwaysBindable(function);
            IEdmModel model = CreateFakeModel(annotationsManager);

            ResourceContext context = CreateContext(model, expectedMetadataPrefix);
            context.SerializerContext.MetadataLevel = ODataMetadataLevel.FullMetadata;

            // Act
            ODataFunction actualFunction = _serializer.CreateODataFunction(function, context);

            // Assert
            string expectedMetadata = expectedMetadataPrefix + "#NS.Function";
            ODataFunction expectedFunction = new ODataFunction
            {
                Metadata = new Uri(expectedMetadata),
                Target = new Uri(expectedTarget),
                Title = "Function"
            };

            AssertEqual(actualFunction, actualFunction);
        }

        [Fact]
        public void CreateODataFunction_IncludesTitle()
        {
            // Arrange
            string expectedActionName = "Function";

            IEdmTypeReference returnType = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Boolean, isNullable: false);
            IEdmFunction function = new EdmFunction("NS", "Function", returnType, isBound: true, entitySetPathExpression: null, isComposable: false);

            OperationLinkBuilder linkBuilder = new OperationLinkBuilder((ResourceContext a) => new Uri("aa://IgnoreTarget"),
                followsConventions: false);
            IEdmDirectValueAnnotationsManager annotationsManager = CreateFakeAnnotationsManager();
            annotationsManager.SetOperationLinkBuilder(function, linkBuilder);

            IEdmModel model = CreateFakeModel(annotationsManager);

            ResourceContext context = CreateContext(model, "http://IgnoreMetadataPath");
            context.SerializerContext.MetadataLevel = (ODataMetadataLevel)TestODataMetadataLevel.FullMetadata;

            // Act
            ODataFunction actualFunction = _serializer.CreateODataFunction(function, context);

            // Assert
            Assert.NotNull(actualFunction);
            Assert.Equal(expectedActionName, actualFunction.Title);
        }

        [Theory]
        [InlineData(TestODataMetadataLevel.MinimalMetadata)]
        [InlineData(TestODataMetadataLevel.NoMetadata)]
        public void CreateODataFunction_OmitsTitle(TestODataMetadataLevel metadataLevel)
        {
            // Arrange
            IEdmTypeReference returnType = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Boolean, isNullable: false);
            IEdmFunction function = new EdmFunction("NS", "Function", returnType, isBound: true, entitySetPathExpression: null, isComposable: false);

            OperationLinkBuilder linkBuilder = new OperationLinkBuilder((ResourceContext a) => new Uri("aa://Ignore"),
                followsConventions: false);
            IEdmDirectValueAnnotationsManager annotationsManager = CreateFakeAnnotationsManager();
            annotationsManager.SetOperationLinkBuilder(function, linkBuilder);

            IEdmModel model = CreateFakeModel(annotationsManager);

            ResourceContext context = CreateContext(model, "http://IgnoreMetadataPath");
            context.SerializerContext.MetadataLevel = (ODataMetadataLevel)metadataLevel;

            // Act
            ODataFunction actualFunction = _serializer.CreateODataFunction(function, context);

            // Assert
            Assert.NotNull(actualFunction);
            Assert.Null(actualFunction.Title);
        }

        [Fact]
        public void CreateODataFunction_IncludesTarget_IfDoesnotFollowODataConvention()
        {
            // Arrange
            Uri expectedTarget = new Uri("aa://Target");
            IEdmTypeReference returnType = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Boolean, isNullable: false);
            IEdmFunction function = new EdmFunction("NS", "Function", returnType, isBound: true, entitySetPathExpression: null, isComposable: false);

            OperationLinkBuilder linkBuilder = new OperationLinkBuilder((ResourceContext a) => expectedTarget, followsConventions: false);
            IEdmDirectValueAnnotationsManager annotationsManager = CreateFakeAnnotationsManager();
            annotationsManager.SetOperationLinkBuilder(function, linkBuilder);

            IEdmModel model = CreateFakeModel(annotationsManager);

            ResourceContext context = CreateContext(model, "http://IgnoreMetadataPath");
            context.SerializerContext.MetadataLevel = (ODataMetadataLevel)TestODataMetadataLevel.FullMetadata;

            // Act
            ODataFunction actualFunction = _serializer.CreateODataFunction(function, context);

            // Assert
            Assert.NotNull(actualFunction);
            Assert.Equal(expectedTarget, actualFunction.Target);
        }

        [Theory]
        [InlineData(TestODataMetadataLevel.MinimalMetadata)]
        [InlineData(TestODataMetadataLevel.NoMetadata)]
        public void CreateODataFunction_OmitsAction_WhenFollowingConventions(TestODataMetadataLevel metadataLevel)
        {
            // Arrange
            IEdmTypeReference returnType = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Boolean, isNullable: false);
            IEdmFunction function = new EdmFunction("NS", "Function", returnType, isBound: true, entitySetPathExpression: null, isComposable: false);

            OperationLinkBuilder linkBuilder = new OperationLinkBuilder((ResourceContext a) => new Uri("aa://Ignore"),
                followsConventions: true);
            IEdmDirectValueAnnotationsManager annotationsManager = CreateFakeAnnotationsManager();
            annotationsManager.SetOperationLinkBuilder(function, linkBuilder);

            IEdmModel model = CreateFakeModel(annotationsManager);

            ResourceContext context = CreateContext(model, "http://IgnoreMetadataPath");
            context.SerializerContext.MetadataLevel = (ODataMetadataLevel)metadataLevel;

            // Act
            ODataFunction actualFunction = _serializer.CreateODataFunction(function, context);

            // Assert
            Assert.Null(actualFunction);
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
            string actualFragment = ODataResourceSerializer.CreateMetadataFragment(action);

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

            OperationLinkBuilder builder = new OperationLinkBuilder((ResourceContext a) => { throw new NotImplementedException(); },
                followsConventions);

            // Act
            bool actualResult = ODataResourceSerializer.ShouldOmitOperation(action.Action, builder,
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
            ODataResourceSerializer.EmitTitle(model, action.Operation, odataAction);

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
            ODataResourceSerializer.EmitTitle(model, action.Operation, odataAction);

            // Assert
            Assert.Equal(action.Operation.Name, odataAction.Title);
        }

        [Fact]
        public void WriteObjectInline_SetsParentContext_ForExpandedNavigationProperties()
        {
            // Arrange
            ODataWriter mockWriter = new Mock<ODataWriter>().Object;
            IEdmNavigationProperty ordersProperty = _customerSet.EntityType().DeclaredNavigationProperties().Single();
            Mock<ODataEdmTypeSerializer> expandedItemSerializer = new Mock<ODataEdmTypeSerializer>(ODataPayloadKind.ResourceSet);
            Mock<ODataSerializerProvider> serializerProvider = new Mock<ODataSerializerProvider>();
            serializerProvider.Setup(p => p.GetEdmTypeSerializer(ordersProperty.Type))
                .Returns(expandedItemSerializer.Object);

            SelectExpandNode selectExpandNode = new SelectExpandNode
            {
                ExpandedProperties = new Dictionary<IEdmNavigationProperty, ExpandedNavigationSelectItem>
                {
                    {ordersProperty, null }
                }
            };
            Mock<ODataResourceSerializer> serializer = new Mock<ODataResourceSerializer>(serializerProvider.Object);
            serializer.Setup(s => s.CreateSelectExpandNode(It.IsAny<ResourceContext>())).Returns(selectExpandNode);
            serializer.Setup(s => s.CreateResource(selectExpandNode, _entityContext)).Returns(new ODataResource());
            serializer.CallBase = true;

            // Act
            serializer.Object.WriteObjectInline(_customer, _customerType, mockWriter, _writeContext);

            // Assert
            expandedItemSerializer.Verify(
                s => s.WriteObjectInline(It.IsAny<object>(), ordersProperty.Type, mockWriter,
                    It.Is<ODataSerializerContext>(c => c.ExpandedResource.SerializerContext == _writeContext)));
        }

        [Fact]
        public void CreateSelectExpandNode_Caches_SelectExpandNode()
        {
            // Arrange
            IEdmEntityTypeReference customerType = _customerSet.EntityType().AsReference();
            ResourceContext entity1 = new ResourceContext(_writeContext, customerType, new Customer());
            ResourceContext entity2 = new ResourceContext(_writeContext, customerType, new Customer());

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

            ResourceContext entity1 = new ResourceContext(_writeContext, customerType.AsReference(), new Customer());
            ResourceContext entity2 = new ResourceContext(_writeContext, derivedCustomerType.AsReference(), new Customer());

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

            ResourceContext entity1 = new ResourceContext(_writeContext, customerType.AsReference(), new Customer());
            ResourceContext entity2 = new ResourceContext(_writeContext, customerType.AsReference(), new Customer());

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

        private static void AssertEqual(ODataOperation expected, ODataOperation actual)
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

        private static ResourceContext CreateContext(IEdmModel model)
        {
            return new ResourceContext
            {
                EdmModel = model
            };
        }

        private static ResourceContext CreateContext(IEdmModel model, string expectedMetadataPrefix)
        {
            var config = RoutingConfigurationFactory.CreateWithRootContainer("OData");
            return new ResourceContext
            {
                EdmModel = model,
                Request = RequestFactory.Create(HttpMethod.Get, expectedMetadataPrefix, config, "OData"),
#if NETFX // Only AspNet version has Url property
                Url = CreateMetadataLinkFactory(expectedMetadataPrefix)
#endif
            };
        }

        private static IEdmEntityType CreateEntityTypeWithName(string typeName)
        {
            IEdmEntityType entityType = new EdmEntityType("NS", typeName);
            return entityType;
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

        private static IEdmModel CreateFakeModel(IEdmDirectValueAnnotationsManager annotationsManager)
        {
            Mock<IEdmModel> model = new Mock<IEdmModel>();
            model.Setup(m => m.DirectValueAnnotationsManager).Returns(annotationsManager);
            return model.Object;
        }

#if NETFX // Only AspNet version has UrlHelper
        private static UrlHelper CreateMetadataLinkFactory(string metadataPath)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, metadataPath);
            request.EnableODataDependencyInjectionSupport();
            request.GetConfiguration().Routes.MapFakeODataRoute();
            return new UrlHelper(request);
        }
#endif

        private class Customer
        {
            public Customer()
            {
                this.Orders = new List<Order>();
                InstanceAnnotations = new ODataInstanceAnnotationContainer();
            }
            public int ID { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string City { get; set; }
            public IList<Order> Orders { get; private set; }
            public IODataInstanceAnnotationContainer InstanceAnnotations { get;  }

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

        private class FakeBindableOperationFinder : BindableOperationFinder
        {
            private IEdmOperation[] _operations;

            public FakeBindableOperationFinder(params IEdmOperation[] operations)
                : base(EdmCoreModel.Instance)
            {
                _operations = operations;
            }

            public override IEnumerable<IEdmOperation> FindOperations(IEdmEntityType entityType)
            {
                return _operations;
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
