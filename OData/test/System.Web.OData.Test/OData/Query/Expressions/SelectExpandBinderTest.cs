// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web.OData.Formatter;
using System.Web.OData.Formatter.Serialization.Models;
using System.Web.OData.TestCommon;
using Microsoft.OData.Core;
using Microsoft.OData.Core.UriParser;
using Microsoft.OData.Core.UriParser.Semantic;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;
using Microsoft.TestCommon;

namespace System.Web.OData.Query.Expressions
{
    public class SelectExpandBinderTest
    {
        private readonly SelectExpandBinder _binder;
        private readonly CustomersModelWithInheritance _model;
        private readonly IQueryable<Customer> _queryable;
        private readonly ODataQueryContext _context;
        private readonly ODataQuerySettings _settings;

        public SelectExpandBinderTest()
        {
            _settings = new ODataQuerySettings { HandleNullPropagation = HandleNullPropagationOption.False };
            _model = new CustomersModelWithInheritance();
            _model.Model.SetAnnotationValue<ClrTypeAnnotation>(_model.Customer, new ClrTypeAnnotation(typeof(Customer)));
            _model.Model.SetAnnotationValue<ClrTypeAnnotation>(_model.SpecialCustomer, new ClrTypeAnnotation(typeof(SpecialCustomer)));
            _context = new ODataQueryContext(_model.Model, typeof(Customer));
            _binder = new SelectExpandBinder(_settings, new SelectExpandQueryOption("*", "", _context));

            Customer customer = new Customer();
            Order order = new Order { Customer = customer };
            customer.Orders.Add(order);

            _queryable = new[] { customer }.AsQueryable();
        }

        [Fact]
        public void Bind_ReturnsIEdmObject_WithRightEdmType()
        {
            // Arrange
            SelectExpandQueryOption selectExpand = new SelectExpandQueryOption(select: "ID", expand: null, context: _context);

            // Act
            IQueryable queryable = SelectExpandBinder.Bind(_queryable, _settings, selectExpand);

            // Assert
            Assert.NotNull(queryable);
            IEdmTypeReference edmType = _model.Model.GetEdmTypeReference(queryable.GetType());
            Assert.NotNull(edmType);
            Assert.True(edmType.IsCollection());
            Assert.Same(_model.Customer, edmType.AsCollection().ElementType().Definition);
        }

        [Fact]
        public void Bind_GeneratedExpression_ContainsExpandedObject()
        {
            // Arrange
            SelectExpandQueryOption selectExpand = new SelectExpandQueryOption("Orders", "Orders,Orders($expand=Customer)", _context);
            IPropertyMapper mapper = new IdentityPropertyMapper();
            // Act
            IQueryable queryable = SelectExpandBinder.Bind(_queryable, _settings, selectExpand);

            // Assert
            IEnumerator enumerator = queryable.GetEnumerator();
            Assert.True(enumerator.MoveNext());
            var partialCustomer = Assert.IsAssignableFrom<SelectExpandWrapper<Customer>>(enumerator.Current);
            Assert.False(enumerator.MoveNext());
            Assert.Null(partialCustomer.Instance);
            IEnumerable<SelectExpandWrapper<Order>> innerOrders = partialCustomer.Container
                .ToDictionary(mapper)["Orders"] as IEnumerable<SelectExpandWrapper<Order>>;
            Assert.NotNull(innerOrders);
            SelectExpandWrapper<Order> partialOrder = innerOrders.Single();
            Assert.Same(_queryable.First().Orders.First(), partialOrder.Instance);
            object customer = partialOrder.Container.ToDictionary(mapper)["Customer"];
            SelectExpandWrapper<Customer> innerInnerCustomer = Assert.IsAssignableFrom<SelectExpandWrapper<Customer>>(customer);
            Assert.Same(_queryable.First(), innerInnerCustomer.Instance);
        }

        [Fact]
        public void ProjectAsWrapper_NonCollection_ContainsRightInstance()
        {
            // Arrange
            Order order = new Order();
            SelectExpandClause selectExpand = new SelectExpandClause(new SelectItem[0], allSelected: true);
            Expression source = Expression.Constant(order);

            // Act
            Expression projection = _binder.ProjectAsWrapper(source, selectExpand, _model.Order);

            // Assert
            SelectExpandWrapper<Order> projectedOrder = Expression.Lambda(projection).Compile().DynamicInvoke() as SelectExpandWrapper<Order>;
            Assert.NotNull(projectedOrder);
            Assert.Same(order, projectedOrder.Instance);
        }

        [Fact]
        public void ProjectAsWrapper_NonCollection_ProjectedValueNullAndHandleNullPropagationTrue()
        {
            // Arrange
            _settings.HandleNullPropagation = HandleNullPropagationOption.True;
            ExpandedNavigationSelectItem expandItem = new ExpandedNavigationSelectItem(
                new ODataExpandPath(new NavigationPropertySegment(_model.Order.NavigationProperties().Single(), navigationSource: _model.Customers)),
                _model.Customers,
                selectExpandOption: null);
            SelectExpandClause selectExpand = new SelectExpandClause(new SelectItem[] { expandItem }, allSelected: true);
            Expression source = Expression.Constant(null, typeof(Order));

            // Act
            Expression projection = _binder.ProjectAsWrapper(source, selectExpand, _model.Order);

            // Assert
            SelectExpandWrapper<Order> projectedOrder = Expression.Lambda(projection).Compile().DynamicInvoke() as SelectExpandWrapper<Order>;
            Assert.NotNull(projectedOrder);
            Assert.Null(projectedOrder.Instance);
            Assert.Null(projectedOrder.Container.ToDictionary(new IdentityPropertyMapper())["Customer"]);
        }

        [Fact]
        public void ProjectAsWrapper_NonCollection_ProjectedValueNullAndHandleNullPropagationFalse_Throws()
        {
            // Arrange
            _settings.HandleNullPropagation = HandleNullPropagationOption.False;
            ExpandedNavigationSelectItem expandItem = new ExpandedNavigationSelectItem(
                new ODataExpandPath(new NavigationPropertySegment(_model.Order.NavigationProperties().Single(), navigationSource: _model.Customers)),
                _model.Customers,
                selectExpandOption: null);
            SelectExpandClause selectExpand = new SelectExpandClause(new SelectItem[] { expandItem }, allSelected: true);
            Expression source = Expression.Constant(null, typeof(Order));

            // Act
            Expression projection = _binder.ProjectAsWrapper(source, selectExpand, _model.Order);

            // Assert
            var e = Assert.Throws<TargetInvocationException>(
                () => Expression.Lambda(projection).Compile().DynamicInvoke());
            Assert.IsType<NullReferenceException>(e.InnerException);
        }

        [Fact]
        public void ProjectAsWrapper_Collection_ContainsRightInstance()
        {
            // Arrange
            Order[] orders = new Order[] { new Order() };
            SelectExpandClause selectExpand = new SelectExpandClause(new SelectItem[0], allSelected: true);
            Expression source = Expression.Constant(orders);

            // Act
            Expression projection = _binder.ProjectAsWrapper(source, selectExpand, _model.Order);

            // Assert
            IEnumerable<SelectExpandWrapper<Order>> projectedOrders = Expression.Lambda(projection).Compile().DynamicInvoke() as IEnumerable<SelectExpandWrapper<Order>>;
            Assert.NotNull(projectedOrders);
            Assert.Same(orders[0], projectedOrders.Single().Instance);
        }

        [Fact]
        public void ProjectAsWrapper_Collection_AppliesPageSize()
        {
            // Arrange
            int pageSize = 5;
            var orders = Enumerable.Range(0, 10).Select(i => new Order());
            SelectExpandClause selectExpand = new SelectExpandClause(new SelectItem[0], allSelected: true);
            Expression source = Expression.Constant(orders);
            _settings.PageSize = pageSize;

            // Act
            Expression projection = _binder.ProjectAsWrapper(source, selectExpand, _model.Order);

            // Assert
            IEnumerable<SelectExpandWrapper<Order>> projectedOrders = Expression.Lambda(projection).Compile().DynamicInvoke() as IEnumerable<SelectExpandWrapper<Order>>;
            Assert.NotNull(projectedOrders);
            Assert.Equal(pageSize + 1, projectedOrders.Count());
        }

        [Fact]
        public void ProjectAsWrapper_ProjectionContainsExpandedProperties()
        {
            // Arrange
            Order order = new Order();
            ExpandedNavigationSelectItem expandItem = new ExpandedNavigationSelectItem(
                new ODataExpandPath(new NavigationPropertySegment(_model.Order.NavigationProperties().Single(), navigationSource: _model.Customers)),
                _model.Customers,
                selectExpandOption: null);
            SelectExpandClause selectExpand = new SelectExpandClause(new SelectItem[] { expandItem }, allSelected: true);
            Expression source = Expression.Constant(order);

            // Act
            Expression projection = _binder.ProjectAsWrapper(source, selectExpand, _model.Order);

            // Assert
            SelectExpandWrapper<Order> projectedOrder = Expression.Lambda(projection).Compile().DynamicInvoke() as SelectExpandWrapper<Order>;
            Assert.NotNull(projectedOrder);
            Assert.Contains("Customer", projectedOrder.Container.ToDictionary(new IdentityPropertyMapper()).Keys);
        }

        [Fact]
        public void ProjectAsWrapper_NullExpandedProperty_HasNullValueInProjectedWrapper()
        {
            // Arrange
            IPropertyMapper mapper = new IdentityPropertyMapper();
            Order order = new Order();
            ExpandedNavigationSelectItem expandItem = new ExpandedNavigationSelectItem(
                new ODataExpandPath(new NavigationPropertySegment(_model.Order.NavigationProperties().Single(), navigationSource: _model.Customers)),
                _model.Customers,
                selectExpandOption: null);
            SelectExpandClause selectExpand = new SelectExpandClause(new SelectItem[] { expandItem }, allSelected: true);
            Expression source = Expression.Constant(order);

            // Act
            Expression projection = _binder.ProjectAsWrapper(source, selectExpand, _model.Order);

            // Assert
            SelectExpandWrapper<Order> projectedOrder = Expression.Lambda(projection).Compile().DynamicInvoke() as SelectExpandWrapper<Order>;
            Assert.NotNull(projectedOrder);
            Assert.Contains("Customer", projectedOrder.Container.ToDictionary(mapper).Keys);
            Assert.Null(projectedOrder.Container.ToDictionary(mapper)["Customer"]);
        }

        [Fact]
        public void ProjectAsWrapper_Collection_ProjectedValueNullAndHandleNullPropagationTrue()
        {
            // Arrange
            _settings.HandleNullPropagation = HandleNullPropagationOption.True;
            SelectExpandClause selectExpand = new SelectExpandClause(new SelectItem[0], allSelected: true);
            Expression source = Expression.Constant(null, typeof(Order[]));

            // Act
            Expression projection = _binder.ProjectAsWrapper(source, selectExpand, _model.Order);

            // Assert
            IEnumerable<SelectExpandWrapper<Order>> projectedOrders = Expression.Lambda(projection).Compile().DynamicInvoke() as IEnumerable<SelectExpandWrapper<Order>>;
            Assert.Null(projectedOrders);
        }

        [Fact]
        public void ProjectAsWrapper_Collection_ProjectedValueNullAndHandleNullPropagationFalse_Throws()
        {
            // Arrange
            _settings.HandleNullPropagation = HandleNullPropagationOption.False;
            SelectExpandClause selectExpand = new SelectExpandClause(new SelectItem[0], allSelected: true);
            Expression source = Expression.Constant(null, typeof(Order[]));

            // Act
            Expression projection = _binder.ProjectAsWrapper(source, selectExpand, _model.Order);

            // Assert
            var e = Assert.Throws<TargetInvocationException>(
                () => Expression.Lambda(projection).Compile().DynamicInvoke());
            Assert.IsType<ArgumentNullException>(e.InnerException);
        }

        [Fact]
        public void ProjectAsWrapper_Element_ProjectedValueContainsModelID()
        {
            // Arrange
            Customer customer = new Customer();
            SelectExpandClause selectExpand = new SelectExpandClause(new SelectItem[0], allSelected: true);
            Expression source = Expression.Constant(customer);

            // Act
            Expression projection = _binder.ProjectAsWrapper(source, selectExpand, _model.Customer);

            // Assert
            SelectExpandWrapper<Customer> customerWrapper = Expression.Lambda(projection).Compile().DynamicInvoke() as SelectExpandWrapper<Customer>;
            Assert.NotNull(customerWrapper.ModelID);
            Assert.Same(_model.Model, ModelContainer.GetModel(customerWrapper.ModelID));
        }

        [Theory]
        [InlineData("*")]
        [InlineData("ID,*")]
        [InlineData("")]
        public void ProjectAsWrapper_Element_ProjectedValueContainsInstance_IfSelectionIsAll(string select)
        {
            // Arrange
            Customer customer = new Customer();
            ODataQueryOptionParser parser = new ODataQueryOptionParser(
                _model.Model,
                _model.Customer,
                _model.Customers,
                new Dictionary<string, string> { { "$select", select }, { "$expand", "Orders" } });
            SelectExpandClause selectExpand = parser.ParseSelectAndExpand();
            Expression source = Expression.Constant(customer);

            // Act
            Expression projection = _binder.ProjectAsWrapper(source, selectExpand, _model.Customer);

            // Assert
            Assert.Equal(ExpressionType.MemberInit, projection.NodeType);
            Assert.NotEmpty((projection as MemberInitExpression).Bindings.Where(p => p.Member.Name == "Instance"));
            SelectExpandWrapper<Customer> customerWrapper = Expression.Lambda(projection).Compile().DynamicInvoke() as SelectExpandWrapper<Customer>;
            Assert.Same(customer, customerWrapper.Instance);
        }

        [Fact]
        public void ProjectAsWrapper_Element_ProjectedValueDoesNotContainInstance_IfSelectionIsPartial()
        {
            // Arrange
            Customer customer = new Customer();
            ODataQueryOptionParser parser = new ODataQueryOptionParser(_model.Model, _model.Customer, _model.Customers,
                new Dictionary<string, string> { { "$select", "ID,Orders" }, { "$expand", "Orders" } });
            SelectExpandClause selectExpand = parser.ParseSelectAndExpand();
            Expression source = Expression.Constant(customer);

            // Act
            Expression projection = _binder.ProjectAsWrapper(source, selectExpand, _model.Customer);

            // Assert
            Assert.Equal(ExpressionType.MemberInit, projection.NodeType);
            Assert.Empty((projection as MemberInitExpression).Bindings.Where(p => p.Member.Name == "Instance"));
            SelectExpandWrapper<Customer> customerWrapper = Expression.Lambda(projection).Compile().DynamicInvoke() as SelectExpandWrapper<Customer>;
            Assert.Null(customerWrapper.Instance);
        }

        [Fact]
        public void ProjectAsWrapper_Element_ProjectedValueContains_SelectedStructuralProperties()
        {
            // Arrange
            Customer customer = new Customer { Name = "OData" };
            ODataQueryOptionParser parser = new ODataQueryOptionParser(_model.Model, _model.Customer, _model.Customers,
                new Dictionary<string, string> { { "$select", "Name,Orders" }, { "$expand", "Orders" } });
            SelectExpandClause selectExpand = parser.ParseSelectAndExpand();
            Expression source = Expression.Constant(customer);

            // Act
            Expression projection = _binder.ProjectAsWrapper(source, selectExpand, _model.Customer);

            // Assert
            SelectExpandWrapper<Customer> customerWrapper = Expression.Lambda(projection).Compile().DynamicInvoke() as SelectExpandWrapper<Customer>;
            Assert.Equal(customer.Name, customerWrapper.Container.ToDictionary(new IdentityPropertyMapper())["Name"]);
        }

        [Theory]
        [InlineData("Name")]
        [InlineData("NS.upgrade")]
        public void ProjectAsWrapper_Element_ProjectedValueContains_KeyPropertiesEvenIfNotPresentInSelectClause(string select)
        {
            // Arrange
            Customer customer = new Customer { ID = 42, FirstName = "OData" };
            ODataQueryOptionParser parser = new ODataQueryOptionParser(_model.Model, _model.Customer, _model.Customers,
                new Dictionary<string, string> { { "$select", select } });

            SelectExpandClause selectExpand = parser.ParseSelectAndExpand();
            Expression source = Expression.Constant(customer);

            // Act
            Expression projection = _binder.ProjectAsWrapper(source, selectExpand, _model.Customer);

            // Assert
            SelectExpandWrapper<Customer> customerWrapper = Expression.Lambda(projection).Compile().DynamicInvoke() as SelectExpandWrapper<Customer>;
            Assert.Equal(customer.ID, customerWrapper.Container.ToDictionary(new IdentityPropertyMapper())["ID"]);
        }

        [Theory]
        [InlineData("Name")]
        [InlineData("NS.upgrade")]
        public void ProjectAsWrapper_ProjectedValueContainsConcurrencyProperties_EvenIfNotPresentInSelectClause(string select)
        {
            // Arrange
            Customer customer = new Customer { ID = 42, City = "any" };

            ODataQueryOptionParser parser = new ODataQueryOptionParser(_model.Model, _model.Customer, _model.Customers,
                new Dictionary<string, string> { { "$select", select } });
            SelectExpandClause selectExpand = parser.ParseSelectAndExpand();
            Expression source = Expression.Constant(customer);

            // Act
            Expression projection = _binder.ProjectAsWrapper(source, selectExpand, _model.Customer);

            // Assert
            SelectExpandWrapper<Customer> customerWrapper = Expression.Lambda(projection).Compile().DynamicInvoke() as SelectExpandWrapper<Customer>;
            Assert.Equal(customer.City, customerWrapper.Container.ToDictionary(new IdentityPropertyMapper())["City"]);
        }

        [Fact]
        public void CreatePropertyNameExpression_NonDerivedProperty_ReturnsConstantExpression()
        {
            Expression customer = Expression.Constant(new Customer());
            IEdmNavigationProperty ordersProperty = _model.Customer.NavigationProperties().Single();

            Expression property = _binder.CreatePropertyNameExpression(_model.Customer, ordersProperty, customer);

            Assert.Equal(ExpressionType.Constant, property.NodeType);
            Assert.Equal(ordersProperty.Name, (property as ConstantExpression).Value);
        }

        [Fact]
        public void CreatePropertyNameExpression_ThrowsODataException_IfMappingTypeIsNotFoundInModel()
        {
            _model.Model.SetAnnotationValue<ClrTypeAnnotation>(_model.SpecialCustomer, null);

            Expression customer = Expression.Constant(new Customer());
            IEdmNavigationProperty specialOrdersProperty = _model.SpecialCustomer.DeclaredNavigationProperties().Single();

            Assert.Throws<ODataException>(
                () => _binder.CreatePropertyNameExpression(_model.Customer, specialOrdersProperty, customer),
                "The provided mapping does not contain an entry for the entity type 'NS.SpecialCustomer'.");
        }

        [Fact]
        public void CreatePropertyNameExpression_DerivedProperty_ReturnsConditionalExpression()
        {
            Expression customer = Expression.Constant(new Customer());
            IEdmNavigationProperty specialOrdersProperty = _model.SpecialCustomer.DeclaredNavigationProperties().Single();

            Expression property = _binder.CreatePropertyNameExpression(_model.Customer, specialOrdersProperty, customer);

            Assert.Equal(ExpressionType.Conditional, property.NodeType);
            Assert.Equal(String.Format("IIF(({0} Is SpecialCustomer), \"SpecialOrders\", null)", customer.ToString()), property.ToString());
        }

        [Fact]
        public void CreatePropertyValueExpression_NonDerivedProperty_ReturnsMemberAccessExpression()
        {
            Expression customer = Expression.Constant(new Customer());
            IEdmNavigationProperty ordersProperty = _model.Customer.NavigationProperties().Single();

            Expression property = _binder.CreatePropertyValueExpression(_model.Customer, ordersProperty, customer);

            Assert.Equal(ExpressionType.MemberAccess, property.NodeType);
            Assert.Equal(typeof(Customer).GetProperty("Orders"), (property as MemberExpression).Member);
        }

        [Fact]
        public void CreatePropertyValueExpression_ThrowsODataException_IfMappingTypeIsNotFoundInModel()
        {
            _model.Model.SetAnnotationValue<ClrTypeAnnotation>(_model.SpecialCustomer, null);
            Expression customer = Expression.Constant(new Customer());
            IEdmNavigationProperty specialOrdersProperty = _model.SpecialCustomer.DeclaredNavigationProperties().Single();

            Assert.Throws<ODataException>(
                () => _binder.CreatePropertyValueExpression(_model.Customer, specialOrdersProperty, customer),
                "The provided mapping does not contain an entry for the entity type 'NS.SpecialCustomer'.");
        }

        [Fact]
        public void CreatePropertyValueExpression_DerivedProperty_ReturnsPropertyAccessExpression()
        {
            Expression customer = Expression.Constant(new Customer());
            IEdmNavigationProperty specialOrdersProperty = _model.SpecialCustomer.DeclaredNavigationProperties().Single();

            Expression property = _binder.CreatePropertyValueExpression(_model.Customer, specialOrdersProperty, customer);

            Assert.Equal(String.Format("({0} As SpecialCustomer).SpecialOrders", customer.ToString()), property.ToString());
        }

        [Fact]
        public void CreatePropertyValueExpression_DerivedNonNullableProperty_ReturnsPropertyAccessExpressionCastToNullable()
        {
            Expression customer = Expression.Constant(new Customer());
            IEdmStructuralProperty specialCustomerProperty = _model.SpecialCustomer.DeclaredStructuralProperties()
                .Single(s => s.Name == "SpecialCustomerProperty");

            Expression property = _binder.CreatePropertyValueExpression(_model.Customer, specialCustomerProperty, customer);

            Assert.Equal(
                String.Format("Convert(({0} As SpecialCustomer).SpecialCustomerProperty)", customer.ToString()),
                property.ToString());
        }

        [Fact]
        public void CreatePropertyValueExpression_HandleNullPropagationTrue_AddsNullCheck()
        {
            _settings.HandleNullPropagation = HandleNullPropagationOption.True;
            Expression customer = Expression.Constant(new Customer());
            IEdmProperty idProperty = _model.Customer.StructuralProperties().Single(p => p.Name == "ID");

            Expression property = _binder.CreatePropertyValueExpression(_model.Customer, idProperty, customer);

            Assert.Equal(ExpressionType.Conditional, property.NodeType);
            Assert.Equal(String.Format("IIF(({0} == null), null, Convert({0}.ID))", customer.ToString()), property.ToString());
        }

        [Fact]
        public void CreatePropertyValueExpression_HandleNullPropagationFalse_ConvertsToNullableType()
        {
            _settings.HandleNullPropagation = HandleNullPropagationOption.False;
            Expression customer = Expression.Constant(new Customer());
            IEdmProperty idProperty = _model.Customer.StructuralProperties().Single(p => p.Name == "ID");

            Expression property = _binder.CreatePropertyValueExpression(_model.Customer, idProperty, customer);

            Assert.Equal(String.Format("Convert({0}.ID)", customer.ToString()), property.ToString());
            Assert.Equal(typeof(int?), property.Type);
        }

        [Fact]
        public void CreateTypeNameExpression_ReturnsNull_IfTypeHasNoDerivedTypes()
        {
            // Arrange
            IEdmEntityType baseType = new EdmEntityType("NS", "BaseType");
            EdmModel model = new EdmModel();
            model.AddElement(baseType);

            Expression source = Expression.Constant(42);

            // Act
            Expression result = SelectExpandBinder.CreateTypeNameExpression(source, baseType, model);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void CreateTypeNameExpression_ThrowsODataException_IfTypeHasNoMapping()
        {
            // Arrange
            IEdmEntityType baseType = new EdmEntityType("NS", "BaseType");
            IEdmEntityType derivedType = new EdmEntityType("NS", "DerivedType", baseType);
            EdmModel model = new EdmModel();
            model.AddElement(baseType);
            model.AddElement(derivedType);

            Expression source = Expression.Constant(42);

            // Act & Assert
            Assert.Throws<ODataException>(
                () => SelectExpandBinder.CreateTypeNameExpression(source, baseType, model),
                "The provided mapping does not contain an entry for the entity type 'NS.DerivedType'.");
        }

        [Fact]
        public void CreateTypeNameExpression_ReturnsConditionalExpression_IfTypeHasDerivedTypes()
        {
            // Arrange
            IEdmEntityType baseType = new EdmEntityType("NS", "BaseType");
            IEdmEntityType typeA = new EdmEntityType("NS", "A", baseType);
            IEdmEntityType typeB = new EdmEntityType("NS", "B", baseType);
            IEdmEntityType typeAA = new EdmEntityType("NS", "AA", typeA);
            IEdmEntityType typeAAA = new EdmEntityType("NS", "AAA", typeAA);
            IEdmEntityType[] types = new[] { baseType, typeA, typeAAA, typeB, typeAA };

            EdmModel model = new EdmModel();
            foreach (var type in types)
            {
                model.AddElement(type);
                model.SetAnnotationValue(type, new ClrTypeAnnotation(new MockType(type.Name, @namespace: type.Namespace)));
            }

            Expression source = Expression.Constant(42);

            // Act
            Expression result = SelectExpandBinder.CreateTypeNameExpression(source, baseType, model);

            // Assert
            Assert.Equal(
                result.ToString(),
                @"IIF((42 Is AAA), ""NS.AAA"", IIF((42 Is AA), ""NS.AA"", IIF((42 Is B), ""NS.B"", IIF((42 Is A), ""NS.A"", ""NS.BaseType""))))");
        }

        private class SpecialCustomer : Customer
        {
            public int SpecialCustomerProperty { get; set; }

            public SpecialOrder[] SpecialOrders { get; set; }
        }

        private class SpecialOrder : Order
        {
            public SpecialCustomer[] SpecialCustomers { get; set; }
        }
    }
}
