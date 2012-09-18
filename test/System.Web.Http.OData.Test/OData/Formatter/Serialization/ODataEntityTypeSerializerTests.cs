// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Formatter.Deserialization;
using System.Web.Http.Routing;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Library;
using Microsoft.Data.OData;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.OData.Formatter.Serialization
{
    public class ODataEntityTypeSerializerTests
    {
        IEdmModel _model;
        IEdmEntitySet _customerSet;
        Customer _customer;
        ODataEntityTypeSerializer _serializer;
        UrlHelper _urlHelper;
        ODataSerializerContext _writeContext;

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

            ODataSerializerProvider serializerProvider = new DefaultODataSerializerProvider(_model);
            _serializer = new ODataEntityTypeSerializer(new EdmEntityTypeReference(_customerSet.ElementType, isNullable: false), serializerProvider);
            _urlHelper = new Mock<UrlHelper>(new HttpRequestMessage()).Object;
            _writeContext = new ODataSerializerContext() { EntitySet = _customerSet, UrlHelper = _urlHelper };
        }

        [Fact]
        public void WriteObjectInline_UsesCorrectTypeName()
        {
            // Arrange & Assert
            Mock<ODataWriter> writer = new Mock<ODataWriter>();
            writer
                .Setup(w => w.WriteStart(It.IsAny<ODataEntry>()))
                .Callback((ODataEntry entry) =>
                {
                    Assert.Equal("Default.Customer", entry.TypeName);
                });

            // Act
            _serializer.WriteObjectInline(_customer, writer.Object, _writeContext);
        }

        [Fact]
        public void WriteObjectInline_WritesCorrectIdLink()
        {
            // Arrange
            bool customIdLinkbuilderCalled = false;
            Mock<IEntitySetLinkBuilder> linkAnnotation = new Mock<IEntitySetLinkBuilder>();
            linkAnnotation
                .Setup(a => a.BuildIdLink(It.IsAny<EntityInstanceContext>()))
                .Callback((EntityInstanceContext context) =>
                {
                    Assert.Equal(context.EdmModel, _model);
                    Assert.Equal(context.EntityInstance, _customer);
                    Assert.Equal(context.EntitySet, _customerSet);
                    Assert.Equal(context.EntityType, _customerSet.ElementType);
                    customIdLinkbuilderCalled = true;
                })
                .Returns("sample id link");

            _model.SetEntitySetLinkBuilderAnnotation(_customerSet, linkAnnotation.Object);

            Mock<ODataWriter> writer = new Mock<ODataWriter>();
            writer
                .Setup(w => w.WriteStart(It.IsAny<ODataEntry>()))
                .Callback((ODataEntry entry) =>
                {
                    Assert.Equal(entry.Id, "sample id link");
                });

            // Act
            _serializer.WriteObjectInline(_customer, writer.Object, _writeContext);

            // Assert
            Assert.True(customIdLinkbuilderCalled);
        }

        [Fact]
        public void WriteObjectInline_WritesCorrectEditLink()
        {
            // Arrange
            bool customEditLinkbuilderCalled = false;
            Mock<IEntitySetLinkBuilder> linkAnnotation = new Mock<IEntitySetLinkBuilder>();
            linkAnnotation
                .Setup(a => a.BuildEditLink(It.IsAny<EntityInstanceContext>()))
                .Callback((EntityInstanceContext context) =>
                {
                    Assert.Equal(context.EdmModel, _model);
                    Assert.Equal(context.EntityInstance, _customer);
                    Assert.Equal(context.EntitySet, _customerSet);
                    Assert.Equal(context.EntityType, _customerSet.ElementType);
                    customEditLinkbuilderCalled = true;
                })
                .Returns(new Uri("http://sample_edit_link"));

            _model.SetEntitySetLinkBuilderAnnotation(_customerSet, linkAnnotation.Object);

            Mock<ODataWriter> writer = new Mock<ODataWriter>();
            writer
                .Setup(w => w.WriteStart(It.IsAny<ODataEntry>()))
                .Callback((ODataEntry entry) =>
                {
                    Assert.Equal(entry.EditLink, new Uri("http://sample_edit_link"));
                });

            // Act
            _serializer.WriteObjectInline(_customer, writer.Object, _writeContext);

            // Assert
            Assert.True(customEditLinkbuilderCalled);
        }

        [Fact]
        public void WriteObjectInline_WritesCorrectReadLink()
        {
            // Arrange
            bool customReadLinkbuilderCalled = false;
            Mock<IEntitySetLinkBuilder> linkAnnotation = new Mock<IEntitySetLinkBuilder>();
            linkAnnotation
                .Setup(a => a.BuildReadLink(It.IsAny<EntityInstanceContext>()))
                .Callback((EntityInstanceContext context) =>
                {
                    Assert.Equal(context.EdmModel, _model);
                    Assert.Equal(context.EntityInstance, _customer);
                    Assert.Equal(context.EntitySet, _customerSet);
                    Assert.Equal(context.EntityType, _customerSet.ElementType);
                    customReadLinkbuilderCalled = true;
                })
                .Returns(new Uri("http://sample_read_link"));

            _model.SetEntitySetLinkBuilderAnnotation(_customerSet, linkAnnotation.Object);

            Mock<ODataWriter> writer = new Mock<ODataWriter>();
            writer
                .Setup(w => w.WriteStart(It.IsAny<ODataEntry>()))
                .Callback((ODataEntry entry) =>
                {
                    Assert.Equal(entry.ReadLink, new Uri("http://sample_read_link"));
                });

            // Act
            _serializer.WriteObjectInline(_customer, writer.Object, _writeContext);

            // Assert
            Assert.True(customReadLinkbuilderCalled);
        }

        [Fact]
        public void WriteObjectInline_Respects_DollarSelect()
        {
            // Arrange
            Mock<ODataWriter> writer = new Mock<ODataWriter>();
            writer
                .Setup(w => w.WriteStart(It.IsAny<ODataEntry>()))
                .Callback((ODataEntry entry) =>
                {
                    Assert.Equal("ID", entry.Properties.Single().Name);
                });

            // Act
            ODataQueryProjectionNode rootNode = new ODataQueryProjectionNode
            {
                Name = "",
                NodeType = _customerSet.ElementType,
                Selects = 
                { 
                    new ODataQueryProjectionNode
                    {
                        Name = "ID",
                        NodeType = _model.FindType("Edm.Int32")
                    }
                }
            };
            _writeContext.CurrentProjectionNode = rootNode;
            _writeContext.RootProjectionNode = rootNode;
            _serializer.WriteObjectInline(_customer, writer.Object, _writeContext);
        }

        [Fact]
        public void WriteObjectInline_Respects_DollarExpand()
        {
            // Arrange
            bool order1Written = false;
            bool order2Written = false;
            Mock<ODataWriter> writer = new Mock<ODataWriter>();
            writer
                .Setup(w => w.WriteStart(It.IsAny<ODataEntry>()))
                .Callback((ODataEntry entry) =>
                {
                    if (entry.TypeName == "Default.Order")
                    {
                        int id = (int)entry.Properties.Where(p => p.Name == "ID").Single().Value;
                        if (id == 1)
                        {
                            order1Written = true;
                        }
                        else if (id == 2)
                        {
                            order2Written = true;
                        }
                        else
                        {
                            Assert.True(false);
                        }
                    }
                });

            ODataQueryProjectionNode rootNode = new ODataQueryProjectionNode
            {
                Name = "",
                NodeType = _customerSet.ElementType,
                Expands =
                {
                    new ODataQueryProjectionNode
                    {
                        NodeType = _model.FindDeclaredType("Default.Order"),
                        Name = "Orders"
                    }
                }
            };
            _customer.Orders.Add(new Order { ID = 1, Name = "1" });
            _customer.Orders.Add(new Order { ID = 2, Name = "2" });
            _writeContext.CurrentProjectionNode = rootNode;
            _writeContext.RootProjectionNode = rootNode;

            // Act
            _serializer.WriteObjectInline(_customer, writer.Object, _writeContext);

            // Assert
            Assert.True(order1Written);
            Assert.True(order2Written);
        }

        [Fact]
        public void WriteObjectInline_Respects_DollarExpandAndSelect()
        {
            // Arrange
            bool order1Written = false;
            bool order2Written = false;
            bool customerWritten = false;
            Mock<ODataWriter> writer = new Mock<ODataWriter>();
            writer
                .Setup(w => w.WriteStart(It.IsAny<ODataEntry>()))
                .Callback((ODataEntry entry) =>
                {
                    if (entry.TypeName == "Default.Order")
                    {
                        string id = (string)entry.Properties.Single().Value;
                        if (id == "1")
                        {
                            order1Written = true;
                        }
                        else if (id == "2")
                        {
                            order2Written = true;
                        }
                        else
                        {
                            Assert.True(false);
                        }
                    }
                    else if (entry.TypeName == "Default.Customer")
                    {
                        Assert.Equal("FirstName", entry.Properties.Single().Name);
                        customerWritten = true;
                    }
                });

            ODataQueryProjectionNode rootNode = new ODataQueryProjectionNode
            {
                Name = "",
                NodeType = _customerSet.ElementType,
                Expands =
                {
                    new ODataQueryProjectionNode
                    {
                        NodeType = _model.FindDeclaredType("Default.Order"),
                        Name = "Orders",
                        Selects = 
                        {
                            new ODataQueryProjectionNode
                            {
                                Name = "Name",
                                NodeType = _model.FindType("Edm.String")
                            }
                        }
                    }
                },
                Selects =
                {
                    new ODataQueryProjectionNode
                    {
                        Name = "FirstName",
                        NodeType = _model.FindType("Edm.String")
                    }
                }
            };
            _customer.Orders.Add(new Order { ID = 1, Name = "1" });
            _customer.Orders.Add(new Order { ID = 2, Name = "2" });
            _writeContext.CurrentProjectionNode = rootNode;
            _writeContext.RootProjectionNode = rootNode;

            // Act
            _serializer.WriteObjectInline(_customer, writer.Object, _writeContext);

            // Assert
            Assert.True(order1Written);
            Assert.True(order2Written);
            Assert.True(customerWritten);
        }

        private IEdmModel SimpleCustomerOrderModel()
        {
            var model = new EdmModel();
            var customerType = new EdmEntityType("Default", "Customer");
            customerType.AddStructuralProperty("ID", EdmPrimitiveTypeKind.Int32);
            customerType.AddStructuralProperty("FirstName", EdmPrimitiveTypeKind.String);
            customerType.AddStructuralProperty("LastName", EdmPrimitiveTypeKind.String);
            model.AddElement(customerType);

            var orderType = new EdmEntityType("Default", "Order");
            orderType.AddStructuralProperty("ID", EdmPrimitiveTypeKind.Int32);
            orderType.AddStructuralProperty("Name", EdmPrimitiveTypeKind.String);
            model.AddElement(orderType);

            // Add navigations
            customerType.AddUnidirectionalNavigation(new EdmNavigationPropertyInfo() { Name = "Orders", Target = orderType, TargetMultiplicity = EdmMultiplicity.Many });
            orderType.AddUnidirectionalNavigation(new EdmNavigationPropertyInfo() { Name = "Customer", Target = customerType, TargetMultiplicity = EdmMultiplicity.One });

            var container = new EdmEntityContainer("Default", "Container");
            var customerSet = container.AddEntitySet("Customers", customerType);
            var orderSet = container.AddEntitySet("Orders", orderType);
            customerSet.AddNavigationTarget(customerType.NavigationProperties().Single(np => np.Name == "Orders"), orderSet);
            orderSet.AddNavigationTarget(orderType.NavigationProperties().Single(np => np.Name == "Customer"), customerSet);

            Mock<IEntitySetLinkBuilder> linkAnnotation = new Mock<IEntitySetLinkBuilder>();
            model.SetEntitySetLinkBuilderAnnotation(customerSet, linkAnnotation.Object);
            model.SetEntitySetLinkBuilderAnnotation(orderSet, linkAnnotation.Object);

            model.AddElement(container);
            return model;
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
    }
}
