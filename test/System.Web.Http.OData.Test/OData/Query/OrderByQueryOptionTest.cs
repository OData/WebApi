// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Builder.TestModels;
using System.Web.Http.OData.Query.Validators;
using System.Web.Http.TestCommon;
using Microsoft.Data.Edm.Library;
using Microsoft.Data.OData;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.OData.Query
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
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();

            Assert.Throws<ArgumentException>(() =>
                new OrderByQueryOption(null, new ODataQueryContext(model, typeof(Customer))));
        }

        [Fact]
        public void ConstructorEmptyRawValueThrows()
        {
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();

            Assert.Throws<ArgumentException>(() =>
                new OrderByQueryOption(string.Empty, new ODataQueryContext(model, typeof(Customer))));
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
                new int[] { 2, 1, 3 },
                actualCustomers.Select(enumModel => enumModel.Id));
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
    }
}
