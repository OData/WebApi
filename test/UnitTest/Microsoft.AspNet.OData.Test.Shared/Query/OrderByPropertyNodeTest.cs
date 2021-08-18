//-----------------------------------------------------------------------------
// <copyright file="OrderByPropertyNodeTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Test.Builder;
using Microsoft.AspNet.OData.Test.Builder.TestModels;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Moq;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Query
{
    public class OrderByPropertyNodeTest
    {
        [Fact]
        public void Constructor_With_Null_Throws()
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(
                () => new OrderByPropertyNode(property: null, direction: OrderByDirection.Ascending),
                "property");
        }

        [Fact]
        public void Constructor_Initializes_Correctly()
        {
            // Arrange
            Mock<IEdmProperty> mockProperty = new Mock<IEdmProperty>();

            // Act
            OrderByPropertyNode node = new OrderByPropertyNode(mockProperty.Object, OrderByDirection.Descending);

            // Assert
            Assert.Same(mockProperty.Object, node.Property);
            Assert.Equal(OrderByDirection.Descending, node.Direction);
        }

        [Fact]
        public void Ctor_TakingOrderByClause_ThrowsArgumentNull_OrderByClause()
        {
            ExceptionAssert.ThrowsArgumentNull(() => new OrderByPropertyNode(orderByClause: null),
                "orderByClause");
        }

        [Fact]
        public void Ctor_TakingOrderByClause_InitializesProperty_Property()
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            IEdmProperty property = model.Customer.FindProperty("ID");
            ResourceRangeVariable variable = new ResourceRangeVariable("it", model.Customer.AsReference(), model.Customers);
            SingleValuePropertyAccessNode node = new SingleValuePropertyAccessNode(new ResourceRangeVariableReferenceNode("it", variable), property);
            OrderByClause orderBy = new OrderByClause(thenBy: null, expression: node, direction: OrderByDirection.Ascending, rangeVariable: variable);

            // Act
            OrderByPropertyNode orderByNode = new OrderByPropertyNode(orderBy);

            // Assert
            Assert.Equal(property, orderByNode.Property);
        }

        [Fact]
        public void Ctor_TakingOrderByClause_InitializesProperty_Direction_and_PropertyPath()
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            IEdmProperty property = model.Customer.FindProperty("ID");
            OrderByDirection direction = OrderByDirection.Ascending;
            ResourceRangeVariable variable = new ResourceRangeVariable("it", model.Customer.AsReference(), model.Customers);
            SingleValuePropertyAccessNode node = new SingleValuePropertyAccessNode(new ResourceRangeVariableReferenceNode("it", variable), property);
            OrderByClause orderBy = new OrderByClause(thenBy: null, expression: node, direction: direction, rangeVariable: variable);

            // Act
            OrderByPropertyNode orderByNode = new OrderByPropertyNode(orderBy);

            // Assert
            Assert.Equal(direction, orderByNode.Direction);
            Assert.Equal("ID", orderByNode.PropertyPath);
        }

        [Fact]
        public void CreateCollection_From_OrderByNode_Succeeds()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataModelBuilderMocks.GetModelBuilderMock<ODataConventionModelBuilder>();
            builder.EntitySet<SampleClass>("entityset");

            IEdmModel model = builder.GetEdmModel();
            IEdmEntityType sampleClassEntityType = model.SchemaElements.Single(t => t.Name == "SampleClass") as IEdmEntityType;
            Assert.NotNull(sampleClassEntityType); // Guard
            IEdmEntitySet entitySet = model.EntityContainer.FindEntitySet("entityset");
            Assert.NotNull(entitySet); // Guard

            ODataQueryOptionParser parser = new ODataQueryOptionParser(model, sampleClassEntityType, entitySet,
                new Dictionary<string, string> { { "$orderby", "Property1 desc, Property2 asc" } });
            OrderByClause orderbyNode = parser.ParseOrderBy();

            // Act
            ICollection<OrderByNode> nodes = OrderByNode.CreateCollection(orderbyNode);

            // Assert
            Assert.False(nodes.OfType<OrderByItNode>().Any());
            IEnumerable<OrderByPropertyNode> propertyNodes = nodes.OfType<OrderByPropertyNode>();
            Assert.Equal(2, propertyNodes.Count());
            Assert.Equal("Property1", propertyNodes.First().Property.Name);
            Assert.Equal(OrderByDirection.Descending, propertyNodes.First().Direction);
            Assert.Equal("Property1", propertyNodes.First().PropertyPath);

            Assert.Equal("Property2", propertyNodes.Last().Property.Name);
            Assert.Equal(OrderByDirection.Ascending, nodes.Last().Direction);
            Assert.Equal("Property2", propertyNodes.Last().PropertyPath);
        }

        [Theory]
        [InlineData(true, "PropertyAlias2", "FirstNameAlias")]
        [InlineData(false, "PropertyAlias", "FirstName")]
        public void CreateCollection_PropertyAliased_IfEnabled(bool modelAliasing, string typeName, string propertyName)
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderHelper.CreateWithModelAliasing(modelAliasing);
            builder.EntitySet<PropertyAlias>("entityset");

            IEdmModel model = builder.GetEdmModel();
            IEdmEntityType entityType = model.SchemaElements.Single(t => t.Name == typeName) as IEdmEntityType;
            Assert.NotNull(entityType); // Guard
            IEdmEntitySet entitySet = model.EntityContainer.FindEntitySet("entityset");
            Assert.NotNull(entitySet); // Guard

            ODataQueryOptionParser parser = new ODataQueryOptionParser(model, entityType, entitySet,
                new Dictionary<string, string> { { "$orderby", propertyName + " desc, Id asc" } });
            OrderByClause orderbyNode = parser.ParseOrderBy();

            // Act
            ICollection<OrderByNode> nodes = OrderByNode.CreateCollection(orderbyNode);

            // Assert
            Assert.False(nodes.OfType<OrderByItNode>().Any());
            IEnumerable<OrderByPropertyNode> propertyNodes = nodes.OfType<OrderByPropertyNode>();
            Assert.Equal(2, propertyNodes.Count());
            Assert.Equal(propertyName, propertyNodes.First().Property.Name);
            Assert.Equal(OrderByDirection.Descending, propertyNodes.First().Direction);
            Assert.Equal(propertyName, propertyNodes.First().PropertyPath);

            Assert.Equal("Id", propertyNodes.Last().Property.Name);
            Assert.Equal(OrderByDirection.Ascending, nodes.Last().Direction);
            Assert.Equal("Id", propertyNodes.Last().PropertyPath);
        }

        [Fact]
        public void CreateCollection_CopmplexType_Succeeds()
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            ODataQueryOptionParser parser = new ODataQueryOptionParser(model.Model, model.Customer, model.Customers,
                new Dictionary<string, string> { { "$orderby", "Address/Street desc, Address/City asc, Account/BankAddress/City asc" } });

            OrderByClause orderByNode = parser.ParseOrderBy();

            // Act
            ICollection<OrderByNode> nodes = OrderByNode.CreateCollection(orderByNode);

            // Assert
            Assert.Equal(3, nodes.Count());
            Assert.Equal("Street", (nodes.ToList()[0] as OrderByPropertyNode).Property.Name);
            Assert.Equal(OrderByDirection.Descending, nodes.ToList()[0].Direction);
            Assert.Equal("Address/Street", nodes.ToList()[0].PropertyPath);

            Assert.Equal("City", (nodes.ToList()[1] as OrderByPropertyNode).Property.Name);
            Assert.Equal(OrderByDirection.Ascending, nodes.ToList()[1].Direction);
            Assert.Equal("Address/City", nodes.ToList()[1].PropertyPath);

            Assert.Equal("City", (nodes.ToList()[2] as OrderByPropertyNode).Property.Name);
            Assert.Equal(OrderByDirection.Ascending, nodes.ToList()[2].Direction);
            Assert.Equal("Account/BankAddress/City", nodes.ToList()[2].PropertyPath);
        }

        private class SampleClass
        {
            public string Property1 { get; set; }

            public string Property2 { get; set; }
        }
    }
}
