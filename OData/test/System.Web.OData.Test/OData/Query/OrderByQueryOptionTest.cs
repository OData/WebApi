// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Web.OData.Builder;
using System.Web.OData.Builder.TestModels;
using System.Web.OData.Query.Validators;
using System.Web.OData.TestCommon;
using Microsoft.OData.Core;
using Microsoft.OData.Core.UriParser;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.OData.Query
{
    public class OrderByQueryOptionTest
    {
        [Fact]
        public void ConstructorNullContextThrows()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new OrderByQueryOption("Name", null));
        }

        [Fact]
        public void ConstructorNullRawValueThrows()
        {
            // Arrange
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();

            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                new OrderByQueryOption(null, new ODataQueryContext(model, typeof(Customer))));
        }

        [Fact]
        public void ConstructorEmptyRawValueThrows()
        {
            // Arrange
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();

            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                new OrderByQueryOption(string.Empty, new ODataQueryContext(model, typeof(Customer))));
        }

        [Fact]
        public void ConstructorNullQueryOptionParserThrows()
        {
            // Arrange
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();

            // Act & Assert
            Assert.ThrowsArgumentNull(() =>
                new OrderByQueryOption("test", new ODataQueryContext(model, typeof(Customer)), queryOptionParser: null),
                "queryOptionParser");
        }

        [Theory]
        [InlineData("Name")]
        [InlineData("''")]
        public void CanConstructValidFilterQuery(string orderbyValue)
        {
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();
            var context = new ODataQueryContext(model, typeof(Customer));
            var orderby = new OrderByQueryOption(orderbyValue, context);

            Assert.Same(context, orderby.Context);
            Assert.Equal(orderbyValue, orderby.RawValue);
        }

        [Fact]
        public void PropertyNodes_Getter_Parses_Query()
        {
            // Arrange
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();
            var context = new ODataQueryContext(model, typeof(Customer));
            var orderby = new OrderByQueryOption("Name,Website", context);

            ICollection<OrderByNode> nodes = orderby.OrderByNodes;

            // Assert
            Assert.False(nodes.OfType<OrderByItNode>().Any());
            IEnumerable<OrderByPropertyNode> propertyNodes = nodes.OfType<OrderByPropertyNode>();
            Assert.NotNull(propertyNodes);
            Assert.Equal(2, propertyNodes.Count());
            Assert.Equal("Name", propertyNodes.First().Property.Name);
            Assert.Equal("Website", propertyNodes.Last().Property.Name);
        }

        [Theory]
        [InlineData("BadPropertyName")]
        [InlineData("''")]
        [InlineData(" ")]
        [InlineData("customerid")]
        [InlineData("CustomerId,CustomerId")]
        [InlineData("CustomerId,Name,CustomerId")]
        public void ApplyInValidOrderbyQueryThrows(string orderbyValue)
        {
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();
            var context = new ODataQueryContext(model, typeof(Customer));
            var orderby = new OrderByQueryOption(orderbyValue, context);

            Assert.Throws<ODataException>(() =>
                orderby.ApplyTo(ODataQueryOptionTest.Customers));
        }

        [Fact]
        [Trait("Description", "Can apply an orderby")]
        public void CanApplyOrderBy()
        {
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetServiceModel();
            var orderByOption = new OrderByQueryOption("Name", new ODataQueryContext(model, typeof(Customer)));

            var customers = (new List<Customer>{
                new Customer { CustomerId = 1, Name = "Andy" },
                new Customer { CustomerId = 2, Name = "Aaron" },
                new Customer { CustomerId = 3, Name = "Alex" }
            }).AsQueryable();

            var results = orderByOption.ApplyTo(customers).ToArray();
            Assert.Equal(2, results[0].CustomerId);
            Assert.Equal(3, results[1].CustomerId);
            Assert.Equal(1, results[2].CustomerId);
        }

        [Fact]
        [Trait("Description", "Can apply an orderby")]
        public void CanApplyOrderByAsc()
        {
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetServiceModel();
            var orderByOption = new OrderByQueryOption("Name asc", new ODataQueryContext(model, typeof(Customer)));

            var customers = (new List<Customer>{
                new Customer { CustomerId = 1, Name = "Andy" },
                new Customer { CustomerId = 2, Name = "Aaron" },
                new Customer { CustomerId = 3, Name = "Alex" }
            }).AsQueryable();

            var results = orderByOption.ApplyTo(customers).ToArray();
            Assert.Equal(2, results[0].CustomerId);
            Assert.Equal(3, results[1].CustomerId);
            Assert.Equal(1, results[2].CustomerId);
        }

        [Fact]
        [Trait("Description", "Can apply an orderby descending")]
        public void CanApplyOrderByDescending()
        {
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetServiceModel();
            var orderByOption = new OrderByQueryOption("Name desc", new ODataQueryContext(model, typeof(Customer)));

            var customers = (new List<Customer>{
                new Customer { CustomerId = 1, Name = "Andy" },
                new Customer { CustomerId = 2, Name = "Aaron" },
                new Customer { CustomerId = 3, Name = "Alex" }
            }).AsQueryable();

            var results = orderByOption.ApplyTo(customers).ToArray();
            Assert.Equal(1, results[0].CustomerId);
            Assert.Equal(3, results[1].CustomerId);
            Assert.Equal(2, results[2].CustomerId);
        }

        [Fact]
        [Trait("Description", "Can apply a compound orderby")]
        public void CanApplyOrderByThenBy()
        {
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetServiceModel();
            var orderByOption = new OrderByQueryOption("Name,Website", new ODataQueryContext(model, typeof(Customer)));

            var customers = (new List<Customer>{
                new Customer { CustomerId = 1, Name = "ACME", Website = "http://www.acme.net" },
                new Customer { CustomerId = 2, Name = "AAAA", Website = "http://www.aaaa.com" },
                new Customer { CustomerId = 3, Name = "ACME", Website = "http://www.acme.com" }
            }).AsQueryable();

            var results = orderByOption.ApplyTo(customers).ToArray();
            Assert.Equal(2, results[0].CustomerId);
            Assert.Equal(3, results[1].CustomerId);
            Assert.Equal(1, results[2].CustomerId);
        }

        [Fact]
        [Trait("Description", "Can apply a OrderByDescending followed by ThenBy")]
        public void CanApplyOrderByDescThenBy()
        {
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetServiceModel();
            var orderByOption = new OrderByQueryOption("Name desc,Website", new ODataQueryContext(model, typeof(Customer)));

            var customers = (new List<Customer>{
                new Customer { CustomerId = 1, Name = "ACME", Website = "http://www.acme.net" },
                new Customer { CustomerId = 2, Name = "AAAA", Website = "http://www.aaaa.com" },
                new Customer { CustomerId = 3, Name = "ACME", Website = "http://www.acme.com" }
            }).AsQueryable();

            var results = orderByOption.ApplyTo(customers).ToArray();
            Assert.Equal(3, results[0].CustomerId);
            Assert.Equal(1, results[1].CustomerId);
            Assert.Equal(2, results[2].CustomerId);
        }

        [Fact]
        [Trait("Description", "Can apply a OrderByDescending followed by ThenBy")]
        public void CanApplyOrderByDescThenByDesc()
        {
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetServiceModel();
            var orderByOption = new OrderByQueryOption("Name desc,Website desc", new ODataQueryContext(model, typeof(Customer)));

            var customers = (new List<Customer>{
                new Customer { CustomerId = 1, Name = "ACME", Website = "http://www.acme.net" },
                new Customer { CustomerId = 2, Name = "AAAA", Website = "http://www.aaaa.com" },
                new Customer { CustomerId = 3, Name = "ACME", Website = "http://www.acme.com" }
            }).AsQueryable();

            var results = orderByOption.ApplyTo(customers).ToArray();
            Assert.Equal(1, results[0].CustomerId);
            Assert.Equal(3, results[1].CustomerId);
            Assert.Equal(2, results[2].CustomerId);
        }

        [Fact]
        public void ApplyToEnums_ReturnsCorrectQueryable()
        {
            // Arrange
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<EnumModel>("EnumModels");
            var model = builder.GetEdmModel();

            var context = new ODataQueryContext(model, typeof(EnumModel));
            var orderbyOption = new OrderByQueryOption("Flag", context);
            IEnumerable<EnumModel> enumModels = FilterQueryOptionTest.EnumModelTestData;

            // Act
            IQueryable queryable = orderbyOption.ApplyTo(enumModels.AsQueryable());

            // Assert
            Assert.NotNull(queryable);
            IEnumerable<EnumModel> actualCustomers = Assert.IsAssignableFrom<IEnumerable<EnumModel>>(queryable);
            Assert.Equal(
                new int[] { 5, 2, 1, 3, 6 },
                actualCustomers.Select(enumModel => enumModel.Id));
        }

        [Theory]
        [InlineData(true, "FirstNameAlias")]
        [InlineData(false, "FirstName")]
        public void ApplyTo_PropertyAliased_IfEnabled(bool modelAliasing, string propertyName)
        {
            // Arrange
            var builder = new ODataConventionModelBuilder { ModelAliasingEnabled = modelAliasing };
            builder.EntitySet<PropertyAlias>("PropertyAliases");
            var model = builder.GetEdmModel();

            var context = new ODataQueryContext(model, typeof(PropertyAlias));
            var orderByOption = new OrderByQueryOption(propertyName, context);
            IEnumerable<PropertyAlias> propertyAliases = FilterQueryOptionTest.PropertyAliasTestData;

            // Act
            IQueryable queryable = orderByOption.ApplyTo(propertyAliases.AsQueryable());

            // Assert
            Assert.NotNull(queryable);
            IEnumerable<PropertyAlias> actualCustomers = Assert.IsAssignableFrom<IEnumerable<PropertyAlias>>(queryable);
            Assert.Equal(
                new[] { "abc", "def", "xyz" },
                actualCustomers.Select(propertyAlias => propertyAlias.FirstName));
        }

        [Theory]
        [InlineData("SharePrice add 1")]
        public void OrderBy_Throws_For_Expressions(string orderByQuery)
        {
            var model = new ODataModelBuilder().Add_Customer_EntityType_With_Address().Add_Customers_EntitySet().GetServiceModel();
            var orderByOption = new OrderByQueryOption(orderByQuery, new ODataQueryContext(model, typeof(Customer)));

            Assert.Throws<ODataException>(
                () => orderByOption.OrderByNodes.Count(),
                "Only ordering by properties is supported for non-primitive collections. Expressions are not supported.");
        }

        [Fact]
        public void CanTurnOffValidationForOrderBy()
        {
            // Arrange
            ODataQueryContext context = ValidationTestHelper.CreateCustomerContext();

            OrderByQueryOption option = new OrderByQueryOption("Name", context);
            ODataValidationSettings settings = new ODataValidationSettings();
            settings.AllowedOrderByProperties.Add("Id");

            // Act & Assert
            Assert.Throws<ODataException>(() => option.Validate(settings),
                "Order by 'Name' is not allowed. To allow it, set the 'AllowedOrderByProperties' property on EnableQueryAttribute or QueryValidationSettings.");

            option.Validator = null;
            Assert.DoesNotThrow(() => option.Validate(settings));
        }

        [Fact]
        public void OrderByDuplicatePropertyThrows()
        {
            // Arrange
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetServiceModel();

            var context = new ODataQueryContext(model, typeof(Customer));
            var orderbyOption = new OrderByQueryOption("Name, Name", context);

            // Act
            Assert.Throws<ODataException>(
                () => orderbyOption.ApplyTo(Enumerable.Empty<Customer>().AsQueryable()),
                "Duplicate property named 'Name' is not supported in '$orderby'.");
        }

        [Fact]
        public void OrderByDuplicateItThrows()
        {
            // Arrange
            var context = new ODataQueryContext(EdmCoreModel.Instance, typeof(int));
            var orderbyOption = new OrderByQueryOption("$it, $it", context);

            // Act
            Assert.Throws<ODataException>(
                () => orderbyOption.ApplyTo(Enumerable.Empty<int>().AsQueryable()),
                "Multiple '$it' nodes are not supported in '$orderby'.");
        }

        [Fact]
        public void ApplyTo_NestedProperties_Succeeds()
        {
            // Arrange
            var model = new ODataModelBuilder().Add_Customer_EntityType_With_Address().Add_Customers_EntitySet().GetServiceModel();
            var orderByOption = new OrderByQueryOption("Address/City asc", new ODataQueryContext(model, typeof(Customer)));

            var customers = (new List<Customer>{
                new Customer { CustomerId = 1, Address = new Address { City = "C" } },
                new Customer { CustomerId = 2, Address = new Address { City = "B" } },
                new Customer { CustomerId = 3, Address = new Address { City = "A" } }
            }).AsQueryable();

            // Act
            var results = orderByOption.ApplyTo(customers).ToArray();

            // Assert
            Assert.Equal(3, results[0].CustomerId);
            Assert.Equal(2, results[1].CustomerId);
            Assert.Equal(1, results[2].CustomerId);
        }

        [Fact]
        public void ApplyTo_NestedProperties_HandlesNullPropagation_Succeeds()
        {
            // Arrange
            var model = new ODataModelBuilder().Add_Customer_EntityType_With_Address().Add_Customers_EntitySet().GetServiceModel();
            var orderByOption = new OrderByQueryOption("Address/City asc", new ODataQueryContext(model, typeof(Customer)));

            var customers = (new List<Customer>{
                new Customer { CustomerId = 1, Address = null },
                new Customer { CustomerId = 2, Address = new Address { City = "B" } },
                new Customer { CustomerId = 3, Address = new Address { City = "A" } }
            }).AsQueryable();

            // Act
            var results = orderByOption.ApplyTo(customers).ToArray();

            // Assert
            Assert.Equal(1, results[0].CustomerId);
            Assert.Equal(3, results[1].CustomerId);
            Assert.Equal(2, results[2].CustomerId);
        }

        [Fact]
        public void ApplyTo_NestedProperties_DoesNotHandleNullPropagation_IfExplicitInSettings()
        {
            // Arrange
            var model = new ODataModelBuilder().Add_Customer_EntityType_With_Address().Add_Customers_EntitySet().GetServiceModel();
            var orderByOption = new OrderByQueryOption("Address/City asc", new ODataQueryContext(model, typeof(Customer)));

            var customers = (new List<Customer>{
                new Customer { CustomerId = 1, Address = null },
                new Customer { CustomerId = 2, Address = new Address { City = "B" } },
                new Customer { CustomerId = 3, Address = new Address { City = "A" } }
            }).AsQueryable();
            ODataQuerySettings settings = new ODataQuerySettings { HandleNullPropagation = HandleNullPropagationOption.False };

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => orderByOption.ApplyTo(customers, settings).ToArray());
        }

        [Fact]
        public void Property_OrderByNodes_WorksWithUnTypedContext()
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            ODataQueryContext context = new ODataQueryContext(model.Model, model.Customer);
            OrderByQueryOption orderBy = new OrderByQueryOption("ID desc", context);

            // Act & Assert
            Assert.NotNull(orderBy.OrderByNodes);
        }

        [Fact]
        public void ApplyTo_WithUnTypedContext_Throws_InvalidOperation()
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            ODataQueryContext context = new ODataQueryContext(model.Model, model.Customer);
            OrderByQueryOption orderBy = new OrderByQueryOption("ID desc", context);
            IQueryable queryable = new Mock<IQueryable>().Object;

            // Act & Assert
            Assert.Throws<NotSupportedException>(() => orderBy.ApplyTo(queryable),
                "The query option is not bound to any CLR type. 'ApplyTo' is only supported with a query option bound to a CLR type.");
        }

        [Fact]
        public void CanApplyOrderBy_WithParameterAlias()
        {
            // Arrange
            var model = new ODataModelBuilder().Add_Customer_EntityType_With_Address().Add_Address_ComplexType().GetServiceModel();

            var parser = new ODataQueryOptionParser(
                model,
                model.FindType("System.Web.OData.Builder.TestModels.Customer"),
                model.FindDeclaredNavigationSource("Default.Container.Customers"),
                new Dictionary<string, string> { { "$orderby", "@q desc,@p asc" }, { "@q", "Address/HouseNumber" }, { "@p", "CustomerId" } });

            var orderByOption = new OrderByQueryOption("@q desc,@p asc", new ODataQueryContext(model, typeof(Customer)), parser);

            var customers = (new List<Customer>{
                new Customer { CustomerId = 1, Address = new Address{HouseNumber = 2}},
                new Customer { CustomerId = 2, Address = new Address{HouseNumber = 1}},
                new Customer { CustomerId = 3, Address = new Address{HouseNumber = 3}},
                new Customer { CustomerId = 4, Address = new Address{HouseNumber = 2}},
                new Customer { CustomerId = 5, Address = new Address{HouseNumber = 1}},
            }).AsQueryable();

            // Act
            var results = orderByOption.ApplyTo(customers).ToArray();

            // Assert
            Assert.Equal(3, results[0].CustomerId);
            Assert.Equal(1, results[1].CustomerId);
            Assert.Equal(4, results[2].CustomerId);
            Assert.Equal(2, results[3].CustomerId);
            Assert.Equal(5, results[4].CustomerId);
        }

        [Fact]
        public void CanApplyOrderBy_WithNestedParameterAlias()
        {
            // Arrange
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetServiceModel();

            var parser = new ODataQueryOptionParser(
                model,
                model.FindType("System.Web.OData.Builder.TestModels.Customer"),
                model.FindDeclaredNavigationSource("Default.Container.Customers"),
                new Dictionary<string, string> { { "$orderby", "@p1" }, { "@p2", "Name" }, { "@p1", "@p2" } });

            var orderByOption = new OrderByQueryOption("@p1", new ODataQueryContext(model, typeof(Customer)), parser);

            var customers = (new List<Customer>{
                new Customer { CustomerId = 1, Name = "Andy" },
                new Customer { CustomerId = 2, Name = "Aaron" },
                new Customer { CustomerId = 3, Name = "Alex" }
            }).AsQueryable();

            // Act
            var results = orderByOption.ApplyTo(customers).ToArray();

            // Assert
            Assert.Equal(2, results[0].CustomerId);
            Assert.Equal(3, results[1].CustomerId);
            Assert.Equal(1, results[2].CustomerId);
        }

        [Fact]
        public void OrderBy_Throws_ParameterAliasNotFound()
        {
            // Arrange
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetServiceModel();
            var orderByOption = new OrderByQueryOption("@p", new ODataQueryContext(model, typeof(Customer)));

            // Act & Assert
            Assert.Throws<ODataException>(
                () => orderByOption.OrderByNodes,
                "Only ordering by properties is supported for non-primitive collections. Expressions are not supported.");
        }
    }
}
