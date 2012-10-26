// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Dispatcher;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Builder.TestModels;
using Microsoft.Data.Edm;
using Microsoft.Data.OData;
using Microsoft.Data.OData.Query;
using Microsoft.Data.OData.Query.SemanticAst;
using Microsoft.TestCommon;
using Microsoft.TestCommon.Types;

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

        // Legal filter queries usable against EnumModelTestData.
        // Tuple is: filter, expected list of customer ID's
        public static TheoryDataSet<string, int[]> EnumModelTestFilters
        {
            get
            {
                return new TheoryDataSet<string, int[]>
                {
                    // Simple Enums
                    { "Simple eq 'First'", new int[] { 1, 3 } },
                    { "'First' eq Simple", new int[] { 1, 3 } },
                    { "Simple eq null", new int[] { } },
                    { "null eq Simple", new int[] { } },
                    { "Simple eq SimpleNullable", new int[] { 1 } },
                    { "SimpleNullable eq 'First'", new int[] { 1 } },
                    { "'First' eq SimpleNullable", new int[] { 1 } },
                    { "SimpleNullable eq null", new int[] { 3 } },
                    { "null eq SimpleNullable", new int[] { 3 } },
                    
                    // Long enums
                    { "Long eq 'SecondLong'", new int[] { 2 } },

                    // Flag enums
                    { "Flag eq 'One, Four'", new int[] { 1 } },
                    { "'One, Four' eq Flag", new int[] { 1 } },
                    { "Flag eq null", new int[] { } },
                    { "null eq Flag", new int[] { } },
                    { "Flag eq FlagNullable", new int[] { 1 } },
                    { "FlagNullable eq 'One, Four'", new int[] { 1 } },
                    { "'One, Four' eq FlagNullable", new int[] { 1 } },
                    { "FlagNullable eq null", new int[] { 3 } },
                    { "null eq FlagNullable", new int[] { 3 } },

                    // Flag enums with different formats
                    { "Flag eq 'One,Four'", new int[] { 1 } },
                    { "Flag eq 'One,    Four'", new int[] { 1 } },
                    { "Flag eq 'Four, One'", new int[] { 1 } },

                    // Other expressions
                    { "Flag ne 'One, Four'", new int[] { 2, 3 } },
                    { "Flag eq FlagNullable and Simple eq SimpleNullable", new int[] { 1 } },
                    { "Simple gt 'First'", new int[] { 2 } },
                    { "Flag ge 'Four,One'", new int[] { 1, 3 } }
                };
            }
        }

        // Test data used by EnumModelTestFilters TheoryDataSet
        public static List<EnumModel> EnumModelTestData
        {
            get
            {
                return new List<EnumModel>()
                {
                    new EnumModel()
                    {
                        Id = 1,
                        Simple = SimpleEnum.First,
                        SimpleNullable = SimpleEnum.First,
                        Long = LongEnum.ThirdLong,
                        Flag = FlagsEnum.One | FlagsEnum.Four,
                        FlagNullable = FlagsEnum.One | FlagsEnum.Four
                    },
                    new EnumModel()
                    {
                        Id = 2,
                        Simple = SimpleEnum.Third,
                        SimpleNullable = SimpleEnum.Second,
                        Long = LongEnum.SecondLong,
                        Flag = FlagsEnum.One | FlagsEnum.Two,
                        FlagNullable = FlagsEnum.Two | FlagsEnum.Four
                    },
                    new EnumModel()
                    {
                        Id = 3,
                        Simple = SimpleEnum.First,
                        SimpleNullable = null,
                        Long = LongEnum.FirstLong,
                        Flag = FlagsEnum.Two | FlagsEnum.Four,
                        FlagNullable = null
                    }
                };
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

        [Fact]
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

            // There's currently a bug here. For now, the test checks for the presence of the bug (as a reminder to fix
            // the test once the bug is fixed).
            // The following asserts show the behavior with the bug and should be removed once the bug is fixed.
            Assert.Null(aParameterType);
            Assert.Equal("a", aParameter.Name);

            // TODO: Enable once Uri Parser sets parameters of Any/All bound to CollectionProperty correctly
            // The following asserts show the behavior without the bug, and should be enabled once the bug is fixed.
            //Assert.NotNull(aParameterType);
            //Assert.Equal("Address", aParameter.Name); 
        }

        [Fact]
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

            // There's currently a bug here. For now, the test checks for the presence of the bug (as a reminder to fix
            // the test once the bug is fixed).
            // The following asserts show the behavior with the bug and should be removed once the bug is fixed.
            Assert.Null(aParameterType);
            Assert.Equal("a", aParameter.Name);

            // TODO: Enable once Uri Parser sets parameters of Any/All bound to CollectionProperty correctly
            // The following asserts show the behavior without the bug, and should be enabled once the bug is fixed.
            //Assert.NotNull(aParameterType);
            //Assert.Equal("Address", aParameter.Name); 
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

        [Theory]
        [PropertyData("EnumModelTestFilters")]
        public void ApplyToEnums_ReturnsCorrectQueryable(string filter, int[] enumModelIds)
        {
            // Arrange
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<EnumModel>("EnumModels");
            var model = builder.GetEdmModel();
            
            var context = new ODataQueryContext(model, typeof(EnumModel), "EnumModels");
            var filterOption = new FilterQueryOption(filter, context);
            IEnumerable<EnumModel> enumModels = EnumModelTestData;

            // Act
            IQueryable queryable = filterOption.ApplyTo(enumModels.AsQueryable(), new ODataQuerySettings { HandleNullPropagation = HandleNullPropagationOption.True });

            // Assert
            Assert.NotNull(queryable);
            IEnumerable<EnumModel> actualCustomers = Assert.IsAssignableFrom<IEnumerable<EnumModel>>(queryable);
            Assert.Equal(
                enumModelIds,
                actualCustomers.Select(enumModel => enumModel.Id));
        }

        [Theory]
        [InlineData("length(Simple) eq 5", "The 'length' function cannot be applied to an enumeration-typed argument.")]
        [InlineData("length(SimpleNullable) eq 5", "The 'length' function cannot be applied to an enumeration-typed argument.")]
        [InlineData("length(Flag) eq 5", "The 'length' function cannot be applied to an enumeration-typed argument.")]
        [InlineData("length(FlagNullable) eq 5", "The 'length' function cannot be applied to an enumeration-typed argument.")]
        [InlineData("substringof('foo', Simple) eq true", "The 'substringof' function cannot be applied to an enumeration-typed argument.")]
        [InlineData("startswith(Simple, 'foo') eq true", "The 'startswith' function cannot be applied to an enumeration-typed argument.")]
        [InlineData("endswith(Simple, 'foo') eq true", "The 'endswith' function cannot be applied to an enumeration-typed argument.")]
        [InlineData("tolower(Simple) eq 'foo'", "The 'tolower' function cannot be applied to an enumeration-typed argument.")]
        [InlineData("toupper(Simple) eq 'foo'", "The 'toupper' function cannot be applied to an enumeration-typed argument.")]
        [InlineData("trim(Simple) eq 'foo'", "The 'trim' function cannot be applied to an enumeration-typed argument.")]
        [InlineData("indexof(Simple, 'foo') eq 2", "The 'indexof' function cannot be applied to an enumeration-typed argument.")]
        [InlineData("substring(Simple, 3) eq 'foo'", "The 'substring' function cannot be applied to an enumeration-typed argument.")]
        [InlineData("substring(Simple, 1, 3) eq 'foo'", "The 'substring' function cannot be applied to an enumeration-typed argument.")]
        [InlineData("concat(Simple, 'bar') eq 'foo'", "The 'concat' function cannot be applied to an enumeration-typed argument.")]
        public void ApplyToEnums_ThrowsNotSupported_ForStringFunctions(string filter, string exceptionMessage)
        {
            // Arrange
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<EnumModel>("EnumModels");
            var model = builder.GetEdmModel();

            var context = new ODataQueryContext(model, typeof(EnumModel), "EnumModels");
            var filterOption = new FilterQueryOption(filter, context);
            IEnumerable<EnumModel> enumModels = EnumModelTestData;

            // Act
            Assert.Throws<ODataException>(
                () => filterOption.ApplyTo(enumModels.AsQueryable(), new ODataQuerySettings { HandleNullPropagation = HandleNullPropagationOption.True }),
                exceptionMessage
            );
        }
    }
}
