// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Dispatcher;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Builder.TestModels;
using Microsoft.Data.Edm;
using Microsoft.Data.OData.Query;
using Microsoft.Data.OData.Query.SemanticAst;
using Microsoft.TestCommon;

namespace System.Web.Http.OData.Query
{
    public class FilterQueryOptionTest
    {
        // Legal filter queries usable against CustomerFilterTestData.
        // Tuple is: filter, expected list of customer ID's
        public static TheoryDataSet<string, int[]> CustomerTestFilters
        {
            get
            {
                return new TheoryDataSet<string, int[]>
                {
                    // Primitive properties
                    { "Name eq 'Highest'", new int[] { 2 } },
                    { "endswith(Name, 'est')", new int[] { 1, 2 } },

                    // Complex properties
                    { "Address/City eq 'redmond'", new int[] { 1 } },
                    { "substringof('e', Address/City)", new int[] { 1, 2 } },

                    // Primitive property collections
                    { "Aliases/any(alias: alias eq 'alias34')", new int[] { 3, 4 } },
                    { "Aliases/any(alias: alias eq 'alias4')", new int[] { 4 } },
                    { "Aliases/all(alias: alias eq 'alias2')", new int[] { 2 } },

                    // Navigational properties
                    { "Orders/any(order: order/OrderId eq 12)", new int[] { 1 } },
                };
            }
        }

        // Test data used by CustomerTestFilters TheoryDataSet
        public static List<Customer> CustomerFilterTestData
        {
            get
            {
                List<Customer> customerList = new List<Customer>();

                Customer c = new Customer 
                { 
                    CustomerId = 1, 
                    Name = "Lowest", 
                    Address = new Address { City = "redmond" }, 
                };
                c.Orders = new List<Order>
                {
                    new Order { OrderId = 11, Customer = c },
                    new Order { OrderId = 12, Customer = c },
                };
                customerList.Add(c);

                c = new Customer 
                { 
                    CustomerId = 2, 
                    Name = "Highest", 
                    Address = new Address { City = "seattle" },
                    Aliases = new List<string> {"alias2", "alias2"} 
                };
                customerList.Add(c);

                c = new Customer 
                { 
                    CustomerId = 3, 
                    Name = "Middle",
                    Address = new Address { City = "hobart" },
                    Aliases = new List<string> {"alias2", "alias34", "alias31"} 
                };
                customerList.Add(c);

                c = new Customer 
                { 
                    CustomerId = 4, 
                    Name = "NewLow", 
                    Aliases = new List<string> {"alias34", "alias4"} 
                };
                customerList.Add(c);

                return customerList;
            }
        }

        [Fact]
        public void ConstructorNullContextThrows()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new FilterQueryOption("Name eq 'MSFT'", null));
        }

        [Fact]
        public void ConstructorNullRawValueThrows()
        {
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();

            Assert.Throws<ArgumentException>(() =>
                new FilterQueryOption(null, new ODataQueryContext(model, typeof(Customer), "Customers")));
        }

        [Fact]
        public void ConstructorEmptyRawValueThrows()
        {
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();

            Assert.Throws<ArgumentException>(() =>
                new FilterQueryOption(string.Empty, new ODataQueryContext(model, typeof(Customer), "Customers")));
        }

        [Theory]
        [InlineData("Name eq 'MSFT'")]
        [InlineData("''")]
        public void CanConstructValidFilterQuery(string filterValue)
        {
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();
            var context = new ODataQueryContext(model, typeof(Customer), "Customers");
            var filter = new FilterQueryOption(filterValue, context);

            Assert.Same(context, filter.Context);
            Assert.Equal(filterValue, filter.RawValue);
        }

        [Fact]
        public void GetQueryNodeParsesQuery()
        {
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();
            var context = new ODataQueryContext(model, typeof(Customer), "Customers");
            var filter = new FilterQueryOption("Name eq 'MSFT'", context);
            var node = filter.QueryNode;

            Assert.Equal(QueryNodeKind.BinaryOperator, node.Expression.Kind);
            var binaryNode = node.Expression as BinaryOperatorQueryNode;
            Assert.Equal(BinaryOperatorKind.Equal, binaryNode.OperatorKind);
            Assert.Equal(QueryNodeKind.Constant, binaryNode.Right.Kind);
            Assert.Equal("MSFT", ((ConstantQueryNode)binaryNode.Right).Value);
            Assert.Equal(QueryNodeKind.PropertyAccess, binaryNode.Left.Kind);
            var propertyAccessNode = binaryNode.Left as PropertyAccessQueryNode;
            Assert.Equal("Name", propertyAccessNode.Property.Name);
        }

        [Fact(Skip="Enable once Uri Parser sets parameters of Any/All bound to CollectionProperty correctly")]
        public void CanConstructValidAnyQueryOverPrimitiveCollectionProperty()
        {
            var model = new ODataModelBuilder().Add_Customer_EntityType_With_CollectionProperties().Add_Customers_EntitySet().GetEdmModel();
            var context = new ODataQueryContext(model, typeof(Customer), "Customers");
            var filter = new FilterQueryOption("Aliases/any(a: a eq 'alias')", context);
            var node = filter.QueryNode;
            var anyNode = node.Expression as AnyQueryNode;
            var aParameter = anyNode.Parameters.SingleOrDefault(p => p.Name == "a");
            var aParameterType = aParameter.ParameterType.Definition as IEdmPrimitiveType;

            Assert.NotNull(aParameter);
            Assert.NotNull(aParameterType);
            Assert.Equal("String", aParameter.Name);            
        }

        [Fact(Skip = "Enable once Uri Parser sets parameters of Any/All bound to CollectionProperty correctly")]
        public void CanConstructValidAnyQueryOverComplexCollectionProperty()
        {
            var model = new ODataModelBuilder().Add_Customer_EntityType_With_CollectionProperties().Add_Customers_EntitySet().Add_Address_ComplexType().GetEdmModel();
            var context = new ODataQueryContext(model, typeof(Customer), "Customers");
            var filter = new FilterQueryOption("Addresses/any(a: a/HouseNumber eq 1)", context);
            var node = filter.QueryNode;
            var anyNode = node.Expression as AnyQueryNode;
            var aParameter = anyNode.Parameters.SingleOrDefault(p => p.Name == "a");
            var aParameterType = aParameter.ParameterType.Definition as IEdmComplexType;

            Assert.NotNull(aParameter);
            Assert.NotNull(aParameterType);
            Assert.Equal("Address", aParameter.Name); 
        }

        [Fact]
        public void ApplyTo_Throws_Null_Query()
        {
            // Arrange
            var model = new ODataModelBuilder().Add_Customer_EntityType_With_CollectionProperties().Add_Customers_EntitySet().Add_Address_ComplexType().GetEdmModel();
            var context = new ODataQueryContext(model, typeof(Customer), "Customers");
            var filter = new FilterQueryOption("Addresses/any(a: a/HouseNumber eq 1)", context);

            // Act & Assert
            Assert.ThrowsArgumentNull(() => filter.ApplyTo(null, new ODataQuerySettings()), "query");
        }

        [Fact]
        public void ApplyTo_Throws_Null_QuerySettings()
        {
            // Arrange
            var model = new ODataModelBuilder().Add_Customer_EntityType_With_CollectionProperties().Add_Customers_EntitySet().Add_Address_ComplexType().GetEdmModel();
            var context = new ODataQueryContext(model, typeof(Customer), "Customers");
            var filter = new FilterQueryOption("Addresses/any(a: a/HouseNumber eq 1)", context);

            // Act & Assert
            Assert.ThrowsArgumentNull(() => filter.ApplyTo(new Customer[0].AsQueryable(), null), "querySettings");
        }

        [Fact]
        public void ApplyTo_Throws_Null_AssembliesResolver()
        {
            // Arrange
            var model = new ODataModelBuilder().Add_Customer_EntityType_With_CollectionProperties().Add_Customers_EntitySet().Add_Address_ComplexType().GetEdmModel();
            var context = new ODataQueryContext(model, typeof(Customer), "Customers");
            var filter = new FilterQueryOption("Addresses/any(a: a/HouseNumber eq 1)", context);

            // Act & Assert
            Assert.ThrowsArgumentNull(() => filter.ApplyTo(new Customer[0].AsQueryable(), new ODataQuerySettings(), null), "assembliesResolver");
        }

        [Theory]
        [PropertyData("CustomerTestFilters")]
        public void ApplyTo_Returns_Correct_Queryable(string filter, int[] customerIds)
        {
            // Arrange
            var model = new ODataModelBuilder()
                            .Add_Order_EntityType()
                            .Add_Customer_EntityType_With_Address()
                            .Add_CustomerOrders_Relationship()
                            .Add_Customer_EntityType_With_CollectionProperties()
                            .Add_Customers_EntitySet()
                            .GetEdmModel();
            var context = new ODataQueryContext(model, typeof(Customer), "Customers");
            var filterOption = new FilterQueryOption(filter, context);
            IEnumerable<Customer> customers = CustomerFilterTestData;

            // Act
            IQueryable queryable = filterOption.ApplyTo(customers.AsQueryable(), new ODataQuerySettings { HandleNullPropagation = HandleNullPropagationOption.True });

            // Assert
            Assert.NotNull(queryable);
            IEnumerable<Customer> actualCustomers = Assert.IsAssignableFrom<IEnumerable<Customer>>(queryable);
            Assert.Equal(
                customerIds,
                actualCustomers.Select(customer => customer.CustomerId));
        }
    }
}
