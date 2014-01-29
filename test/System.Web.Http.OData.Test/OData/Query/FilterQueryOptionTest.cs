// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Dispatcher;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Builder.TestModels;
using System.Web.Http.OData.Query.Validators;
using System.Web.Http.OData.TestCommon;
using System.Web.Http.TestCommon;
using Microsoft.Data.Edm;
using Microsoft.Data.OData;
using Microsoft.Data.OData.Query;
using Microsoft.Data.OData.Query.SemanticAst;
using Microsoft.TestCommon;
using Microsoft.TestCommon.Types;
using Moq;

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
                    Aliases = new List<string> { "alias2", "alias2" }
                };
                customerList.Add(c);

                c = new Customer
                {
                    CustomerId = 3,
                    Name = "Middle",
                    Address = new Address { City = "hobart" },
                    Aliases = new List<string> { "alias2", "alias34", "alias31" }
                };
                customerList.Add(c);

                c = new Customer
                {
                    CustomerId = 4,
                    Name = "NewLow",
                    Aliases = new List<string> { "alias34", "alias4" }
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
                new FilterQueryOption(null, new ODataQueryContext(model, typeof(Customer))));
        }

        [Fact]
        public void ConstructorEmptyRawValueThrows()
        {
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();

            Assert.Throws<ArgumentException>(() =>
                new FilterQueryOption(string.Empty, new ODataQueryContext(model, typeof(Customer))));
        }

        [Theory]
        [InlineData("Name eq 'MSFT'")]
        [InlineData("''")]
        public void CanConstructValidFilterQuery(string filterValue)
        {
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();
            var context = new ODataQueryContext(model, typeof(Customer));
            var filter = new FilterQueryOption(filterValue, context);

            Assert.Same(context, filter.Context);
            Assert.Equal(filterValue, filter.RawValue);
        }

        [Fact]
        public void GetQueryNodeParsesQuery()
        {
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();
            var context = new ODataQueryContext(model, typeof(Customer));
            var filter = new FilterQueryOption("Name eq 'MSFT'", context);
            var node = filter.FilterClause;

            Assert.Equal(QueryNodeKind.BinaryOperator, node.Expression.Kind);
            var binaryNode = node.Expression as BinaryOperatorNode;
            Assert.Equal(BinaryOperatorKind.Equal, binaryNode.OperatorKind);
            Assert.Equal(QueryNodeKind.Constant, binaryNode.Right.Kind);
            Assert.Equal("MSFT", ((ConstantNode)binaryNode.Right).Value);
            Assert.Equal(QueryNodeKind.SingleValuePropertyAccess, binaryNode.Left.Kind);
            var propertyAccessNode = binaryNode.Left as SingleValuePropertyAccessNode;
            Assert.Equal("Name", propertyAccessNode.Property.Name);
        }

        [Fact]
        public void CanConstructValidAnyQueryOverPrimitiveCollectionProperty()
        {
            var model = new ODataModelBuilder().Add_Customer_EntityType_With_CollectionProperties().Add_Customers_EntitySet().GetEdmModel();
            var context = new ODataQueryContext(model, typeof(Customer));
            var filter = new FilterQueryOption("Aliases/any(a: a eq 'alias')", context);
            var node = filter.FilterClause;
            var anyNode = node.Expression as AnyNode;
            var aParameter = anyNode.RangeVariables.SingleOrDefault(p => p.Name == "a");
            var aParameterType = aParameter.TypeReference.Definition as IEdmPrimitiveType;

            Assert.NotNull(aParameter);

            Assert.NotNull(aParameterType);
            Assert.Equal("a", aParameter.Name);
        }

        [Fact]
        public void CanConstructValidAnyQueryOverComplexCollectionProperty()
        {
            var model = new ODataModelBuilder().Add_Customer_EntityType_With_CollectionProperties().Add_Customers_EntitySet().Add_Address_ComplexType().GetEdmModel();
            var context = new ODataQueryContext(model, typeof(Customer));
            var filter = new FilterQueryOption("Addresses/any(a: a/HouseNumber eq 1)", context);
            var node = filter.FilterClause;
            var anyNode = node.Expression as AnyNode;
            var aParameter = anyNode.RangeVariables.SingleOrDefault(p => p.Name == "a");
            var aParameterType = aParameter.TypeReference.Definition as IEdmComplexType;

            Assert.NotNull(aParameter);

            Assert.NotNull(aParameterType);
            Assert.Equal("a", aParameter.Name);
        }

        [Fact]
        public void CanTurnOffValidationForFilter()
        {
            ODataValidationSettings settings = new ODataValidationSettings() { AllowedFunctions = AllowedFunctions.AllDateTimeFunctions };
            ODataQueryContext context = ValidationTestHelper.CreateCustomerContext();
            FilterQueryOption option = new FilterQueryOption("substring(Name,8,1) eq '7'", context);

            Assert.Throws<ODataException>(() =>
                option.Validate(settings),
                "Function 'substring' is not allowed. To allow it, set the 'AllowedFunctions' property on EnableQueryAttribute or QueryValidationSettings.");

            option.Validator = null;
            Assert.DoesNotThrow(() => option.Validate(settings));
        }

        [Fact]
        public void ApplyTo_Throws_Null_Query()
        {
            // Arrange
            var model = new ODataModelBuilder().Add_Customer_EntityType_With_CollectionProperties().Add_Customers_EntitySet().Add_Address_ComplexType().GetEdmModel();
            var context = new ODataQueryContext(model, typeof(Customer));
            var filter = new FilterQueryOption("Addresses/any(a: a/HouseNumber eq 1)", context);

            // Act & Assert
            Assert.ThrowsArgumentNull(() => filter.ApplyTo(null, new ODataQuerySettings()), "query");
        }

        [Fact]
        public void ApplyTo_Throws_Null_QuerySettings()
        {
            // Arrange
            var model = new ODataModelBuilder().Add_Customer_EntityType_With_CollectionProperties().Add_Customers_EntitySet().Add_Address_ComplexType().GetEdmModel();
            var context = new ODataQueryContext(model, typeof(Customer));
            var filter = new FilterQueryOption("Addresses/any(a: a/HouseNumber eq 1)", context);

            // Act & Assert
            Assert.ThrowsArgumentNull(() => filter.ApplyTo(new Customer[0].AsQueryable(), null), "querySettings");
        }

        [Fact]
        public void ApplyTo_Throws_Null_AssembliesResolver()
        {
            // Arrange
            var model = new ODataModelBuilder().Add_Customer_EntityType_With_CollectionProperties().Add_Customers_EntitySet().Add_Address_ComplexType().GetEdmModel();
            var context = new ODataQueryContext(model, typeof(Customer));
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
            var context = new ODataQueryContext(model, typeof(Customer));
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
            var model = GetEnumModel();
            var context = new ODataQueryContext(model, typeof(EnumModel));
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
            var model = GetEnumModel();
            var context = new ODataQueryContext(model, typeof(EnumModel));
            var filterOption = new FilterQueryOption(filter, context);
            IEnumerable<EnumModel> enumModels = EnumModelTestData;

            // Act
            Assert.Throws<ODataException>(
                () => filterOption.ApplyTo(enumModels.AsQueryable(), new ODataQuerySettings { HandleNullPropagation = HandleNullPropagationOption.True }),
                exceptionMessage
            );
        }

        [Fact]
        public void Property_FilterClause_WorksWithUnTypedContext()
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            ODataQueryContext context = new ODataQueryContext(model.Model, model.Customer);
            FilterQueryOption filter = new FilterQueryOption("ID eq 42", context);

            // Act & Assert
            Assert.NotNull(filter.FilterClause);
        }

        [Fact]
        public void ApplyTo_WithUnTypedContext_Throws_InvalidOperation()
        {
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            ODataQueryContext context = new ODataQueryContext(model.Model, model.Customer);
            FilterQueryOption filter = new FilterQueryOption("Id eq 42", context);
            IQueryable queryable = new Mock<IQueryable>().Object;

            Assert.Throws<NotSupportedException>(() => filter.ApplyTo(queryable, new ODataQuerySettings()),
                "The query option is not bound to any CLR type. 'ApplyTo' is only supported with a query option bound to a CLR type.");
        }

        private static IEdmModel GetEnumModel()
        {
            HttpConfiguration config = new HttpConfiguration();
            config.Services.Replace(typeof(IAssembliesResolver), new TestAssemblyResolver(typeof(EnumModel)));
            var builder = new ODataConventionModelBuilder(config);
            builder.EntitySet<EnumModel>("EnumModels");
            return builder.GetEdmModel();
        }
    }
}
