// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Routing;
using System.Web.Http.Routing;
using System.Web.Http.TestCommon;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Annotations;
using Microsoft.Data.Edm.Library;
using Microsoft.Data.OData;
using Microsoft.Data.OData.Query;
using Microsoft.Data.OData.Query.SemanticAst;
using Microsoft.TestCommon;
using Moq;
using ODataPath = System.Web.Http.OData.Routing.ODataPath;

namespace System.Web.Http.OData.Formatter.Serialization
{
    public class ODataEntityTypeSerializerTests
    {
        private IEdmModel _model;
        private IEdmEntitySet _customerSet;
        private Customer _customer;
        private ODataEntityTypeSerializer _serializer;
        private ODataSerializerContext _writeContext;
        private EntityInstanceContext _entityInstanceContext;
        private ODataSerializerProvider _serializerProvider;
        private IEdmEntityTypeReference _customerType;
        private ODataPath _path;

        public ODataEntityTypeSerializerTests()
        {
            _model = SerializationTestsHelpers.SimpleCustomerOrderModel();

            _model.SetAnnotationValue<ClrTypeAnnotation>(_model.FindType("Default.Customer"), new ClrTypeAnnotation(typeof(Customer)));
            _model.SetAnnotationValue<ClrTypeAnnotation>(_model.FindType("Default.Order"), new ClrTypeAnnotation(typeof(Order)));

            _customerSet = _model.FindDeclaredEntityContainer("Default.Container").FindEntitySet("Customers");
            _customer = new Customer()
            {
                FirstName = "Foo",
                LastName = "Bar",
                ID = 10,
            };

            _serializerProvider = new DefaultODataSerializerProvider();
            _customerType = _model.GetEdmTypeReference(typeof(Customer)).AsEntity();
            _serializer = new ODataEntityTypeSerializer(_serializerProvider);
            _path = new ODataPath(new EntitySetPathSegment(_customerSet));
            _writeContext = new ODataSerializerContext() { EntitySet = _customerSet, Model = _model, Path = _path };
            _entityInstanceContext = new EntityInstanceContext(_writeContext, _customerSet.ElementType.AsReference(), _customer);
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
                "The related entity set could not be found from the OData path. The related entity set is required to serialize the payload.");
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
            IEdmEntityType customerType = _customerSet.ElementType;
            IEdmNavigationProperty ordersProperty = customerType.NavigationProperties().Single(p => p.Name == "Orders");
            SelectExpandClause selectExpandClause = new ODataUriParser(_model, serviceRoot: null).ParseSelectAndExpand("Orders", "Orders", customerType, _customerSet);
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
                        Assert.Same(context.EntitySet.Name, "Orders");
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
            Assert.Same(_writeContext.EntitySet.Name, "Customers");
            Assert.Same(_writeContext.SelectExpandClause, selectExpandClause);
        }

        [Fact]
        public void WriteObjectInline_CanExpandNavigationProperty_ContainingEdmObject()
        {
            // Arrange
            IEdmEntityType customerType = _customerSet.ElementType;
            IEdmNavigationProperty ordersProperty = customerType.NavigationProperties().Single(p => p.Name == "Orders");

            Mock<IEdmObject> orders = new Mock<IEdmObject>();
            orders.Setup(o => o.GetEdmType()).Returns(ordersProperty.Type);
            object ordersValue = orders.Object;

            Mock<IEdmEntityObject> customer = new Mock<IEdmEntityObject>();
            customer.Setup(c => c.TryGetPropertyValue("Orders", out ordersValue)).Returns(true);
            customer.Setup(c => c.GetEdmType()).Returns(customerType.AsReference());

            SelectExpandClause selectExpandClause = new ODataUriParser(_model, serviceRoot: null).ParseSelectAndExpand("Orders", "Orders", customerType, _customerSet);
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
                SelectedActions = { new Mock<IEdmFunctionImport>().Object, new Mock<IEdmFunctionImport>().Object }
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
            Mock<IEdmTypeReference> propertyType = new Mock<IEdmTypeReference>();
            propertyType.Setup(t => t.Definition).Returns(new EdmEntityType("Namespace", "Name"));
            Mock<IEdmStructuralProperty> property = new Mock<IEdmStructuralProperty>();
            property.Setup(p => p.Name).Returns("PropertyName");
            Mock<ODataSerializerProvider> serializerProvider = new Mock<ODataSerializerProvider>(MockBehavior.Strict);
            var entity = new { PropertyName = 42 };
            Mock<ODataEdmTypeSerializer> innerSerializer = new Mock<ODataEdmTypeSerializer>(ODataPayloadKind.Property);
            ODataValue propertyValue = new Mock<ODataValue>().Object;

            property.Setup(p => p.Type).Returns(propertyType.Object);
            serializerProvider.Setup(s => s.GetEdmTypeSerializer(propertyType.Object)).Returns(innerSerializer.Object);
            innerSerializer.Setup(s => s.CreateODataValue(42, propertyType.Object, _writeContext)).Returns(propertyValue).Verifiable();

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
            Assert.Equal(writeContext.EntitySet, instanceContext.EntitySet);
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
            EntitySetLinkBuilderAnnotation linkAnnotation = new MockEntitySetLinkBuilderAnnotation
            {
                NavigationLinkBuilder = (ctxt, property, metadataLevel) => navigationLinkUri
            };
            _model.SetEntitySetLinkBuilder(_customerSet, linkAnnotation);

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
            IEdmFunctionImport action = new Mock<IEdmFunctionImport>().Object;

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
            EntitySetLinkBuilderAnnotation linkAnnotation = new MockEntitySetLinkBuilderAnnotation
            {
                IdLinkBuilder = new SelfLinkBuilder<string>((EntityInstanceContext context) =>
                {
                    Assert.Same(instanceContext, context);
                    customIdLinkbuilderCalled = true;
                    return "http://sample_id_link";
                },
                followsConventions: false)
            };
            _model.SetEntitySetLinkBuilder(_customerSet, linkAnnotation);

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
            EntitySetLinkBuilderAnnotation linkAnnotation = new MockEntitySetLinkBuilderAnnotation
            {
                EditLinkBuilder = new SelfLinkBuilder<Uri>((EntityInstanceContext context) =>
                {
                    Assert.Same(instanceContext, context);
                    customEditLinkbuilderCalled = true;
                    return new Uri("http://sample_edit_link");
                },
                followsConventions: false)
            };
            _model.SetEntitySetLinkBuilder(_customerSet, linkAnnotation);

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
            EntitySetLinkBuilderAnnotation linkAnnotation = new MockEntitySetLinkBuilderAnnotation
            {
                ReadLinkBuilder = new SelfLinkBuilder<Uri>((EntityInstanceContext context) =>
                {
                    Assert.Same(instanceContext, context);
                    customReadLinkbuilderCalled = true;
                    return new Uri("http://sample_read_link");
                },
                followsConventions: false)
            };

            _model.SetEntitySetLinkBuilder(_customerSet, linkAnnotation);

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
        public void AddTypeNameAnnotationAsNeeded_DoesNotAddAnnotation_InDefaultMetadataMode()
        {
            // Arrange
            ODataEntry entry = new ODataEntry();

            // Act
            ODataEntityTypeSerializer.AddTypeNameAnnotationAsNeeded(entry, null, ODataMetadataLevel.Default);

            // Assert
            Assert.Null(entry.GetAnnotation<SerializationTypeNameAnnotation>());
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
        [InlineData(TestODataMetadataLevel.Default, false)]
        [InlineData(TestODataMetadataLevel.FullMetadata, true)]
        [InlineData(TestODataMetadataLevel.MinimalMetadata, true)]
        [InlineData(TestODataMetadataLevel.NoMetadata, true)]
        public void ShouldAddTypeNameAnnotation(TestODataMetadataLevel metadataLevel, bool expectedResult)
        {
            // Act
            bool actualResult = ODataEntityTypeSerializer.ShouldAddTypeNameAnnotation(
                (ODataMetadataLevel)metadataLevel);

            // Assert
            Assert.Equal(expectedResult, actualResult);
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
        public void CreateODataAction_ForAtom_IncludesEverything()
        {
            // Arrange
            string expectedContainerName = "Container";
            string expectedActionName = "Action";
            string expectedTarget = "aa://Target";
            string expectedMetadataPrefix = "http://Metadata";

            IEdmEntityContainer container = CreateFakeContainer(expectedContainerName);
            IEdmFunctionImport functionImport = CreateFakeFunctionImport(container, expectedActionName,
                isBindable: true);

            ActionLinkBuilder linkBuilder = new ActionLinkBuilder((a) => new Uri(expectedTarget),
                followsConventions: true);
            IEdmDirectValueAnnotationsManager annotationsManager = CreateFakeAnnotationsManager();
            annotationsManager.SetActionLinkBuilder(functionImport, linkBuilder);
            annotationsManager.SetIsAlwaysBindable(functionImport);
            annotationsManager.SetDefaultContainer(container);
            IEdmModel model = CreateFakeModel(annotationsManager);
            UrlHelper url = CreateMetadataLinkFactory(expectedMetadataPrefix);

            EntityInstanceContext context = CreateContext(model, url);
            context.SerializerContext.MetadataLevel = ODataMetadataLevel.Default;

            // Act
            ODataAction actualAction = _serializer.CreateODataAction(functionImport, context);

            // Assert
            string expectedMetadata = expectedMetadataPrefix + "#" + expectedContainerName + "." + expectedActionName;
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
            IEdmEntityContainer container = CreateFakeContainer("IgnoreContainer");
            IEdmFunctionImport functionImport = CreateFakeFunctionImport(container, "IgnoreAction");

            ActionLinkBuilder linkBuilder = new ActionLinkBuilder((a) => null, followsConventions: false);
            IEdmDirectValueAnnotationsManager annotationsManager = CreateFakeAnnotationsManager();
            annotationsManager.SetActionLinkBuilder(functionImport, linkBuilder);

            IEdmModel model = CreateFakeModel(annotationsManager);

            EntityInstanceContext context = CreateContext(model);
            context.SerializerContext.MetadataLevel = ODataMetadataLevel.MinimalMetadata;

            // Act
            ODataAction actualAction = _serializer.CreateODataAction(functionImport, context);

            // Assert
            Assert.Null(actualAction);
        }

        [Fact]
        public void CreateODataAction_ForJsonLight_OmitsContainerName_PerCreateMetadataFragment()
        {
            // Arrange
            string expectedMetadataPrefix = "http://Metadata";
            string expectedActionName = "Action";

            IEdmEntityContainer container = CreateFakeContainer("ContainerShouldNotAppearInResult");
            IEdmFunctionImport functionImport = CreateFakeFunctionImport(container, expectedActionName);

            ActionLinkBuilder linkBuilder = new ActionLinkBuilder((a) => new Uri("aa://IgnoreTarget"),
                followsConventions: false);
            IEdmDirectValueAnnotationsManager annotationsManager = CreateFakeAnnotationsManager();
            annotationsManager.SetActionLinkBuilder(functionImport, linkBuilder);
            annotationsManager.SetDefaultContainer(container);

            IEdmModel model = CreateFakeModel(annotationsManager);
            UrlHelper url = CreateMetadataLinkFactory(expectedMetadataPrefix);

            EntityInstanceContext context = CreateContext(model, url);
            context.SerializerContext.MetadataLevel = ODataMetadataLevel.MinimalMetadata;

            // Act
            ODataAction actualAction = _serializer.CreateODataAction(functionImport, context);

            // Assert
            Assert.NotNull(actualAction);
            string expectedMetadata = expectedMetadataPrefix + "#" + expectedActionName;
            AssertEqual(new Uri(expectedMetadata), actualAction.Metadata);
        }

        [Fact]
        public void CreateODataAction_SkipsAlwaysAvailableAction_PerShouldOmitAction()
        {
            // Arrange
            IEdmFunctionImport functionImport = CreateFakeFunctionImport(true);

            ActionLinkBuilder linkBuilder = new ActionLinkBuilder((a) => new Uri("aa://IgnoreTarget"),
                followsConventions: true);
            IEdmDirectValueAnnotationsManager annotationsManager = CreateFakeAnnotationsManager();
            annotationsManager.SetActionLinkBuilder(functionImport, linkBuilder);
            annotationsManager.SetIsAlwaysBindable(functionImport);

            IEdmModel model = CreateFakeModel(annotationsManager);

            EntityInstanceContext context = CreateContext(model);
            context.SerializerContext.MetadataLevel = ODataMetadataLevel.MinimalMetadata;

            // Act
            ODataAction actualAction = _serializer.CreateODataAction(functionImport, context);

            // Assert
            Assert.Null(actualAction);
        }

        [Theory]
        [InlineData(TestODataMetadataLevel.Default)]
        [InlineData(TestODataMetadataLevel.FullMetadata)]
        public void CreateODataAction_IncludesTitle(TestODataMetadataLevel metadataLevel)
        {
            // Arrange
            string expectedActionName = "Action";

            IEdmEntityContainer container = CreateFakeContainer("IgnoreContainer");
            IEdmFunctionImport functionImport = CreateFakeFunctionImport(container, expectedActionName);

            ActionLinkBuilder linkBuilder = new ActionLinkBuilder((a) => new Uri("aa://IgnoreTarget"),
                followsConventions: false);
            IEdmDirectValueAnnotationsManager annotationsManager = CreateFakeAnnotationsManager();
            annotationsManager.SetActionLinkBuilder(functionImport, linkBuilder);

            IEdmModel model = CreateFakeModel(annotationsManager);
            UrlHelper url = CreateMetadataLinkFactory("http://IgnoreMetadataPath");

            EntityInstanceContext context = CreateContext(model, url);
            context.SerializerContext.MetadataLevel = (ODataMetadataLevel)metadataLevel;

            // Act
            ODataAction actualAction = _serializer.CreateODataAction(functionImport, context);

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
            IEdmEntityContainer container = CreateFakeContainer("IgnoreContainer");
            IEdmFunctionImport functionImport = CreateFakeFunctionImport(container, "IgnoreAction");

            ActionLinkBuilder linkBuilder = new ActionLinkBuilder((a) => new Uri("aa://Ignore"),
                followsConventions: false);
            IEdmDirectValueAnnotationsManager annotationsManager = CreateFakeAnnotationsManager();
            annotationsManager.SetActionLinkBuilder(functionImport, linkBuilder);

            IEdmModel model = CreateFakeModel(annotationsManager);
            UrlHelper url = CreateMetadataLinkFactory("http://IgnoreMetadataPath");

            EntityInstanceContext context = CreateContext(model, url);
            context.SerializerContext.MetadataLevel = (ODataMetadataLevel)metadataLevel;

            // Act
            ODataAction actualAction = _serializer.CreateODataAction(functionImport, context);

            // Assert
            Assert.NotNull(actualAction);
            Assert.Null(actualAction.Title);
        }

        [Theory]
        [InlineData(TestODataMetadataLevel.Default, false)]
        [InlineData(TestODataMetadataLevel.Default, true)]
        [InlineData(TestODataMetadataLevel.FullMetadata, false)]
        [InlineData(TestODataMetadataLevel.FullMetadata, true)]
        [InlineData(TestODataMetadataLevel.MinimalMetadata, false)]
        [InlineData(TestODataMetadataLevel.NoMetadata, false)]
        public void CreateODataAction_IncludesTarget(TestODataMetadataLevel metadataLevel, bool followsConventions)
        {
            // Arrange
            Uri expectedTarget = new Uri("aa://Target");

            IEdmEntityContainer container = CreateFakeContainer("IgnoreContainer");
            IEdmFunctionImport functionImport = CreateFakeFunctionImport(container, "IgnoreAction");

            ActionLinkBuilder linkBuilder = new ActionLinkBuilder((a) => expectedTarget, followsConventions);
            IEdmDirectValueAnnotationsManager annotationsManager = CreateFakeAnnotationsManager();
            annotationsManager.SetActionLinkBuilder(functionImport, linkBuilder);

            IEdmModel model = CreateFakeModel(annotationsManager);
            UrlHelper url = CreateMetadataLinkFactory("http://IgnoreMetadataPath");

            EntityInstanceContext context = CreateContext(model, url);
            context.SerializerContext.MetadataLevel = (ODataMetadataLevel)metadataLevel;

            // Act
            ODataAction actualAction = _serializer.CreateODataAction(functionImport, context);

            // Assert
            Assert.NotNull(actualAction);
            Assert.Equal(expectedTarget, actualAction.Target);
        }

        [Theory]
        [InlineData(TestODataMetadataLevel.MinimalMetadata)]
        [InlineData(TestODataMetadataLevel.NoMetadata)]
        public void CreateODataAction_OmitsTarget_WhenFollowingConventions(TestODataMetadataLevel metadataLevel)
        {
            // Arrange
            IEdmEntityContainer container = CreateFakeContainer("IgnoreContainer");
            IEdmFunctionImport functionImport = CreateFakeFunctionImport(container, "IgnoreAction");

            ActionLinkBuilder linkBuilder = new ActionLinkBuilder((a) => new Uri("aa://Ignore"),
                followsConventions: true);
            IEdmDirectValueAnnotationsManager annotationsManager = CreateFakeAnnotationsManager();
            annotationsManager.SetActionLinkBuilder(functionImport, linkBuilder);

            IEdmModel model = CreateFakeModel(annotationsManager);
            UrlHelper url = CreateMetadataLinkFactory("http://IgnoreMetadataPath");

            EntityInstanceContext context = CreateContext(model, url);
            context.SerializerContext.MetadataLevel = (ODataMetadataLevel)metadataLevel;

            // Act
            ODataAction actualAction = _serializer.CreateODataAction(functionImport, context);

            // Assert
            Assert.NotNull(actualAction);
            Assert.Null(actualAction.Target);
        }

        [InlineData(TestODataMetadataLevel.Default)]
        [InlineData(TestODataMetadataLevel.FullMetadata)]
        [InlineData(TestODataMetadataLevel.MinimalMetadata)]
        [InlineData(TestODataMetadataLevel.NoMetadata)]
        public void CreateMetadataFragment_IncludesNonDefaultContainerName(TestODataMetadataLevel metadataLevel)
        {
            // Arrange
            string expectedContainerName = "Container";
            string expectedActionName = "Action";

            IEdmEntityContainer container = CreateFakeContainer(expectedContainerName);
            IEdmFunctionImport action = CreateFakeFunctionImport(container, expectedActionName);

            IEdmModel model = CreateFakeModel();

            // Act
            string actualFragment = ODataEntityTypeSerializer.CreateMetadataFragment(action, model,
                (ODataMetadataLevel)metadataLevel);

            // Assert
            Assert.Equal(expectedContainerName + "." + expectedActionName, actualFragment);
        }

        [Theory]
        [InlineData(TestODataMetadataLevel.Default)]
        [InlineData(TestODataMetadataLevel.FullMetadata)]
        public void CreateMetadataFragment_IncludesDefaultContainerName(TestODataMetadataLevel metadataLevel)
        {
            // Arrange
            string expectedContainerName = "Container";
            string expectedActionName = "Action";

            IEdmEntityContainer container = CreateFakeContainer(expectedContainerName);
            IEdmFunctionImport action = CreateFakeFunctionImport(container, expectedActionName);

            IEdmDirectValueAnnotationsManager annotationsManager = CreateFakeAnnotationsManager();
            annotationsManager.SetDefaultContainer(container);
            IEdmModel model = CreateFakeModel(annotationsManager);

            // Act
            string actualFragment = ODataEntityTypeSerializer.CreateMetadataFragment(action, model,
                (ODataMetadataLevel)metadataLevel);

            // Assert
            Assert.Equal(expectedContainerName + "." + expectedActionName, actualFragment);
        }

        [Theory]
        [InlineData(TestODataMetadataLevel.MinimalMetadata)]
        [InlineData(TestODataMetadataLevel.NoMetadata)]
        public void CreateMetadataFragment_OmitsDefaultContainerName(TestODataMetadataLevel metadataLevel)
        {
            // Arrange
            string expectedActionName = "Action";

            IEdmEntityContainer container = CreateFakeContainer("ContainerShouldNotAppearInResult");
            IEdmFunctionImport action = CreateFakeFunctionImport(container, expectedActionName);

            IEdmDirectValueAnnotationsManager annotationsManager = CreateFakeAnnotationsManager();
            annotationsManager.SetDefaultContainer(container);
            IEdmModel model = CreateFakeModel(annotationsManager);

            // Act
            string actualFragment = ODataEntityTypeSerializer.CreateMetadataFragment(action, model,
                (ODataMetadataLevel)metadataLevel);

            // Assert
            Assert.Equal(expectedActionName, actualFragment);
        }

        [Theory]
        [InlineData(TestODataMetadataLevel.Default, false, false, false)]
        [InlineData(TestODataMetadataLevel.Default, false, true, false)]
        [InlineData(TestODataMetadataLevel.Default, true, false, false)]
        [InlineData(TestODataMetadataLevel.Default, true, true, false)]
        [InlineData(TestODataMetadataLevel.FullMetadata, false, false, false)]
        [InlineData(TestODataMetadataLevel.FullMetadata, false, true, false)]
        [InlineData(TestODataMetadataLevel.FullMetadata, true, false, false)]
        [InlineData(TestODataMetadataLevel.FullMetadata, true, true, false)]
        [InlineData(TestODataMetadataLevel.MinimalMetadata, false, false, false)]
        [InlineData(TestODataMetadataLevel.MinimalMetadata, false, true, false)]
        [InlineData(TestODataMetadataLevel.MinimalMetadata, true, false, false)]
        [InlineData(TestODataMetadataLevel.MinimalMetadata, true, true, true)]
        [InlineData(TestODataMetadataLevel.NoMetadata, false, false, false)]
        [InlineData(TestODataMetadataLevel.NoMetadata, false, true, false)]
        [InlineData(TestODataMetadataLevel.NoMetadata, true, false, false)]
        [InlineData(TestODataMetadataLevel.NoMetadata, true, true, true)]
        public void TestShouldOmitAction(TestODataMetadataLevel metadataLevel, bool isAlwaysAvailable,
            bool followsConventions, bool expectedResult)
        {
            // Arrange
            IEdmFunctionImport action = CreateFakeFunctionImport(true);
            IEdmDirectValueAnnotationsManager annonationsManager = CreateFakeAnnotationsManager();

            if (isAlwaysAvailable)
            {
                annonationsManager.SetIsAlwaysBindable(action);
            }

            IEdmModel model = CreateFakeModel(annonationsManager);

            ActionLinkBuilder builder = new ActionLinkBuilder((a) => { throw new NotImplementedException(); },
                followsConventions);

            // Act
            bool actualResult = ODataEntityTypeSerializer.ShouldOmitAction(action, model, builder,
                (ODataMetadataLevel)metadataLevel);

            // Assert
            Assert.Equal(expectedResult, actualResult);
        }

        [Fact]
        public void WriteObjectInline_SetsParentContext_ForExpandedNavigationProperties()
        {
            // Arrange
            ODataWriter mockWriter = new Mock<ODataWriter>().Object;
            IEdmNavigationProperty ordersProperty = _customerSet.ElementType.DeclaredNavigationProperties().Single();
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
            IEdmEntityTypeReference customerType = _customerSet.ElementType.AsReference();
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
            IEdmEntityType customerType = _customerSet.ElementType;
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
            IEdmEntityType customerType = _customerSet.ElementType;

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

        private static IEdmFunctionImport CreateFakeFunctionImport(IEdmEntityContainer container, string name)
        {
            Mock<IEdmFunctionImport> mock = new Mock<IEdmFunctionImport>();
            mock.Setup(o => o.Container).Returns(container);
            mock.Setup(o => o.Name).Returns(name);
            return mock.Object;
        }

        private static IEdmFunctionImport CreateFakeFunctionImport(IEdmEntityContainer container, string name,
            bool isBindable)
        {
            Mock<IEdmFunctionImport> mock = new Mock<IEdmFunctionImport>();
            mock.Setup(o => o.Container).Returns(container);
            mock.Setup(o => o.Name).Returns(name);
            mock.Setup(o => o.IsBindable).Returns(isBindable);
            return mock.Object;
        }

        private static IEdmFunctionImport CreateFakeFunctionImport(bool isBindable)
        {
            Mock<IEdmFunctionImport> mock = new Mock<IEdmFunctionImport>();
            mock.Setup(o => o.IsBindable).Returns(isBindable);
            return mock.Object;
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
            public IList<Order> Orders { get; private set; }
        }

        private class Order
        {
            public int ID { get; set; }
            public string Name { get; set; }
            public Customer Customer { get; set; }
        }

        private class FakeBindableProcedureFinder : BindableProcedureFinder
        {
            private IEdmFunctionImport[] _procedures;

            public FakeBindableProcedureFinder(params IEdmFunctionImport[] procedures)
                : base(EdmCoreModel.Instance)
            {
                _procedures = procedures;
            }

            public override IEnumerable<IEdmFunctionImport> FindProcedures(IEdmEntityType entityType)
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
