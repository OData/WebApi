// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Query.Expressions;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Query.Expressions
{
    public class SelectExpandBinderTest
    {
        private static IPropertyMapper PropertyMapper = new IdentityPropertyMapper();

        private readonly SelectExpandBinder _binder;
        private readonly IQueryable<QueryCustomer> _queryable;
        private readonly ODataQueryContext _context;
        private readonly ODataQuerySettings _settings;

        private readonly IEdmModel _model;
        private readonly IEdmEntityType _customer;
        private readonly IEdmEntityType _order;
        private readonly IEdmEntitySet _customers;
        private readonly IEdmEntitySet _orders;

        public SelectExpandBinderTest()
        {
            _model = GetEdmModel();
            _customer = _model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "QueryCustomer");
            _order = _model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "QueryOrder");
            _customers = _model.EntityContainer.FindEntitySet("Customers");
            _orders = _model.EntityContainer.FindEntitySet("Orders");

            _settings = new ODataQuerySettings { HandleNullPropagation = HandleNullPropagationOption.False };
            _context = new ODataQueryContext(_model, typeof(QueryCustomer)) { RequestContainer = new MockContainer() };
            _binder = new SelectExpandBinder(_settings, _context);

            QueryCustomer customer = new QueryCustomer
            {
                Orders = new List<QueryOrder>()
            };
            QueryOrder order = new QueryOrder { Customer = customer };
            customer.Orders.Add(order);

            _queryable = new[] { customer }.AsQueryable();
        }

        private static SelectExpandBinder GetBinder<T>(
            IEdmModel model,
            bool enableDeterministicSelectExpandWrapperInstance = false,
            HandleNullPropagationOption nullPropagation = HandleNullPropagationOption.False)
        {
            var settings = new ODataQuerySettings
            {
                EnableDeterministicSelectExpandWrapperInstance = enableDeterministicSelectExpandWrapperInstance,
                HandleNullPropagation = nullPropagation,
            };

            var context = new ODataQueryContext(model, typeof(T)) { RequestContainer = new MockContainer() };

            return new SelectExpandBinder(settings, context);
        }

        [Fact]
        public void Bind_ReturnsIEdmObject_WithRightEdmType2()
        {
            string csdl = Builder.MetadataTest.GetCSDL(_model);
            Console.WriteLine(csdl);
        }

        [Theory]
        [InlineData("Id")]
        [InlineData("Name")]
        [InlineData("HomeAddress")]
        public void Bind_ReturnsIEdmObject_WithRightEdmType(string select)
        {
            // Arrange
            SelectExpandQueryOption selectExpand = new SelectExpandQueryOption(select: select, expand: null, context: _context);

            // Act
            IQueryable queryable = SelectExpandBinder.Bind(_queryable, _settings, selectExpand);

            // Assert
            Assert.NotNull(queryable);
            IEdmType edmType = _model.GetEdmType(queryable.GetType());
            Assert.NotNull(edmType);
            Assert.Equal(EdmTypeKind.Collection, edmType.TypeKind);
            Assert.Same(_customer, edmType.AsElementType());
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Bind_GeneratedExpression_ContainsExpandedObject(bool enableDeterministicSelectExpandWrapperInstance)
        {
            // Arrange
            SelectExpandQueryOption selectExpand = new SelectExpandQueryOption("Orders", "Orders,Orders($expand=Customer)", _context);
            var settings = new ODataQuerySettings
            {
                EnableDeterministicSelectExpandWrapperInstance = enableDeterministicSelectExpandWrapperInstance,
                HandleNullPropagation = HandleNullPropagationOption.False,
            };

            // Act
            IQueryable queryable = SelectExpandBinder.Bind(_queryable, settings, selectExpand);

            // Assert
            IEnumerator enumerator = queryable.GetEnumerator();
            Assert.True(enumerator.MoveNext());
            var partialCustomer = Assert.IsAssignableFrom<SelectExpandWrapper<QueryCustomer>>(enumerator.Current);
            Assert.False(enumerator.MoveNext());

            if (enableDeterministicSelectExpandWrapperInstance)
            {
                Assert.Same(_queryable.Single(), partialCustomer.Instance);
                Assert.Null(partialCustomer.InstanceType);
            }
            else
            {
                Assert.Null(partialCustomer.Instance);
                Assert.Equal("Microsoft.AspNet.OData.Test.Query.Expressions.QueryCustomer", partialCustomer.InstanceType);
            }

            IEnumerable<SelectExpandWrapper<QueryOrder>> innerOrders = partialCustomer.Container
                .ToDictionary(PropertyMapper)["Orders"] as IEnumerable<SelectExpandWrapper<QueryOrder>>;
            Assert.NotNull(innerOrders);
            SelectExpandWrapper<QueryOrder> partialOrder = innerOrders.Single();
            Assert.Same(_queryable.First().Orders.First(), partialOrder.Instance);
            object customer = partialOrder.Container.ToDictionary(PropertyMapper)["Customer"];
            SelectExpandWrapper<QueryCustomer> innerInnerCustomer = Assert.IsAssignableFrom<SelectExpandWrapper<QueryCustomer>>(customer);
            Assert.Same(_queryable.First(), innerInnerCustomer.Instance);
        }

        [Fact]
        public void Bind_GeneratedExpression_CheckNullObjectWithinChainProjectionByKey()
        {
            // Arrange
            SelectExpandQueryOption selectExpand = new SelectExpandQueryOption(null, "Orders($expand=Customer($select=City))", _context);

            // Act
            IQueryable queryable = SelectExpandBinder.Bind(_queryable, _settings, selectExpand);

            // Assert
            var unaryExpression = (UnaryExpression)((MethodCallExpression)queryable.Expression).Arguments.Single(a => a is UnaryExpression);
            var expressionString = unaryExpression.Operand.ToString();
#if NETCORE
            Assert.Contains("IsNull = (Convert($it.Customer.Id, Nullable`1) == null)}", expressionString);
#else
            Assert.Contains("IsNull = (Convert($it.Customer.Id) == null)}", expressionString);
#endif
        }

        [Fact]
        public void ProjectAsWrapper_NonCollection_ContainsRightInstance()
        {
            // Arrange
            QueryOrder order = new QueryOrder();
            SelectExpandClause selectExpand = new SelectExpandClause(new SelectItem[0], allSelected: true);
            Expression source = Expression.Constant(order);

            // Act
            Expression projection = _binder.ProjectAsWrapper(source, selectExpand, _order, _orders);

            // Assert
            SelectExpandWrapper<QueryOrder> projectedOrder = Expression.Lambda(projection).Compile().DynamicInvoke() as SelectExpandWrapper<QueryOrder>;
            Assert.NotNull(projectedOrder);
            Assert.Same(order, projectedOrder.Instance);
        }

        [Fact]
        public void ProjectAsWrapper_NonCollection_ProjectedValueNullAndHandleNullPropagationTrue()
        {
            // Arrange
            _settings.HandleNullPropagation = HandleNullPropagationOption.True;

            IEdmNavigationProperty customerNav = _order.DeclaredNavigationProperties().Single(c => c.Name == "Customer");
            ExpandedNavigationSelectItem expandItem = new ExpandedNavigationSelectItem(
                new ODataExpandPath(new NavigationPropertySegment(customerNav, navigationSource: _customers)), _customers, selectExpandOption: null);
            SelectExpandClause selectExpand = new SelectExpandClause(new SelectItem[] { expandItem }, allSelected: true);
            Expression source = Expression.Constant(null, typeof(QueryOrder));

            // Act
            Expression projection = _binder.ProjectAsWrapper(source, selectExpand, _order, _orders);

            // Assert
            SelectExpandWrapper<QueryOrder> projectedOrder = Expression.Lambda(projection).Compile().DynamicInvoke() as SelectExpandWrapper<QueryOrder>;
            Assert.NotNull(projectedOrder);
            Assert.Null(projectedOrder.Instance);

            SelectExpandWrapper<QueryCustomer> projectCustomer = projectedOrder.Container.ToDictionary(PropertyMapper)["Customer"] as SelectExpandWrapper<QueryCustomer>;
            Assert.NotNull(projectCustomer);
            Assert.Null(projectCustomer.Instance);
        }

        [Fact]
        public void ProjectAsWrapper_NonCollection_ProjectedValueNullAndHandleNullPropagationFalse_Throws()
        {
            // Arrange
            _settings.HandleNullPropagation = HandleNullPropagationOption.False;
            IEdmNavigationProperty customerNav = _order.DeclaredNavigationProperties().Single(c => c.Name == "Customer");
            ExpandedNavigationSelectItem expandItem = new ExpandedNavigationSelectItem(
                new ODataExpandPath(new NavigationPropertySegment(customerNav, navigationSource: _customers)),
                _customers,
                selectExpandOption: null);
            SelectExpandClause selectExpand = new SelectExpandClause(new SelectItem[] { expandItem }, allSelected: true);
            Expression source = Expression.Constant(null, typeof(QueryOrder));

            // Act
            Expression projection = _binder.ProjectAsWrapper(source, selectExpand, _order, _orders);

            // Assert
            var e = ExceptionAssert.Throws<TargetInvocationException>(() => Expression.Lambda(projection).Compile().DynamicInvoke());
            Assert.IsType<NullReferenceException>(e.InnerException);
        }

        [Fact]
        public void ProjectAsWrapper_Collection_ContainsRightInstance()
        {
            // Arrange
            QueryOrder[] orders = new QueryOrder[] { new QueryOrder() };
            SelectExpandClause selectExpand = new SelectExpandClause(new SelectItem[0], allSelected: true);
            Expression source = Expression.Constant(orders);

            // Act
            Expression projection = _binder.ProjectAsWrapper(source, selectExpand, _order, _orders);

            // Assert
            IEnumerable<SelectExpandWrapper<QueryOrder>> projectedOrders = Expression.Lambda(projection).Compile().DynamicInvoke() as IEnumerable<SelectExpandWrapper<QueryOrder>>;
            Assert.NotNull(projectedOrders);
            Assert.Same(orders[0], projectedOrders.Single().Instance);
        }

        [Fact]
        public void ProjectAsWrapper_Collection_AppliesPageSize_AndOrderBy()
        {
            // Arrange
            int pageSize = 5;
            var orders = Enumerable.Range(0, 10).Select(i => new QueryOrder
            {
                Id = 10 - i,
            });
            SelectExpandClause selectExpand = new SelectExpandClause(new SelectItem[0], allSelected: true);
            Expression source = Expression.Constant(orders);
            _settings.PageSize = pageSize;

            // Act
            Expression projection = _binder.ProjectAsWrapper(source, selectExpand, _order, _orders);

            // Assert
            IEnumerable<SelectExpandWrapper<QueryOrder>> projectedOrders = Expression.Lambda(projection).Compile().DynamicInvoke() as IEnumerable<SelectExpandWrapper<QueryOrder>>;
            Assert.NotNull(projectedOrders);
            Assert.Equal(pageSize + 1, projectedOrders.Count());
            Assert.Equal(1, projectedOrders.First().Instance.Id);
        }

        [Fact]
        public void ProjectAsWrapper_ProjectionContainsExpandedProperties()
        {
            // Arrange
            QueryOrder order = new QueryOrder();
            IEdmNavigationProperty customerNav = _order.DeclaredNavigationProperties().Single(c => c.Name == "Customer");
            ExpandedNavigationSelectItem expandItem = new ExpandedNavigationSelectItem(
                new ODataExpandPath(new NavigationPropertySegment(customerNav, navigationSource: _customers)),
                _customers,
                selectExpandOption: null);
            SelectExpandClause selectExpand = new SelectExpandClause(new SelectItem[] { expandItem }, allSelected: true);
            Expression source = Expression.Constant(order);

            // Act
            Expression projection = _binder.ProjectAsWrapper(source, selectExpand, _order, _orders);

            // Assert
            SelectExpandWrapper<QueryOrder> projectedOrder = Expression.Lambda(projection).Compile().DynamicInvoke() as SelectExpandWrapper<QueryOrder>;
            Assert.NotNull(projectedOrder);
            Assert.Contains("Customer", projectedOrder.Container.ToDictionary(PropertyMapper).Keys);
        }

        [Fact]
        public void ProjectAsWrapper_NullExpandedProperty_HasNullValueInProjectedWrapper()
        {
            // Arrange
            QueryOrder order = new QueryOrder();
            IEdmNavigationProperty customerNav = _order.DeclaredNavigationProperties().Single(c => c.Name == "Customer");
            ExpandedNavigationSelectItem expandItem = new ExpandedNavigationSelectItem(
                new ODataExpandPath(new NavigationPropertySegment(customerNav, navigationSource: _customers)),
                _customers,
                selectExpandOption: null);
            SelectExpandClause selectExpand = new SelectExpandClause(new SelectItem[] { expandItem }, allSelected: true);
            Expression source = Expression.Constant(order);

            // Act
            Expression projection = _binder.ProjectAsWrapper(source, selectExpand, _order, _orders);

            // Assert
            SelectExpandWrapper<QueryOrder> projectedOrder = Expression.Lambda(projection).Compile().DynamicInvoke() as SelectExpandWrapper<QueryOrder>;
            Assert.NotNull(projectedOrder);
            Assert.Contains("Customer", projectedOrder.Container.ToDictionary(PropertyMapper).Keys);

            SelectExpandWrapper<QueryCustomer> projectCustomer = projectedOrder.Container.ToDictionary(PropertyMapper)["Customer"] as SelectExpandWrapper<QueryCustomer>;
            Assert.NotNull(projectCustomer);
            Assert.Null(projectCustomer.Instance);
        }

        [Fact]
        public void ProjectAsWrapper_Collection_ProjectedValueNullAndHandleNullPropagationTrue()
        {
            // Arrange
            _settings.HandleNullPropagation = HandleNullPropagationOption.True;

            SelectExpandClause selectExpand = new SelectExpandClause(new SelectItem[0], allSelected: true);
            Expression source = Expression.Constant(null, typeof(QueryOrder[]));

            // Act
            Expression projection = _binder.ProjectAsWrapper(source, selectExpand, _order, _orders);

            // Assert
            IEnumerable<SelectExpandWrapper<QueryOrder>> projectedOrders = Expression.Lambda(projection).Compile().DynamicInvoke() as IEnumerable<SelectExpandWrapper<QueryOrder>>;
            Assert.Null(projectedOrders);
        }

        [Fact]
        public void ProjectAsWrapper_Collection_ProjectedValueNullAndHandleNullPropagationFalse_Throws()
        {
            // Arrange
            _settings.HandleNullPropagation = HandleNullPropagationOption.False;

            SelectExpandClause selectExpand = new SelectExpandClause(new SelectItem[0], allSelected: true);

            Expression source = Expression.Constant(null, typeof(QueryOrder[]));

            // Act
            Expression projection = _binder.ProjectAsWrapper(source, selectExpand, _order, _orders);

            // Assert
            var e = ExceptionAssert.Throws<TargetInvocationException>(() => Expression.Lambda(projection).Compile().DynamicInvoke());
            Assert.IsType<ArgumentNullException>(e.InnerException);
        }

        [Fact]
        public void ProjectAsWrapper_Element_ProjectedValueContainsModelID()
        {
            // Arrange
            SelectExpandClause selectExpand = new SelectExpandClause(new SelectItem[0], allSelected: true);
            QueryCustomer aCustomer = new QueryCustomer();
            Expression source = Expression.Constant(aCustomer);

            // Act
            Expression projection = _binder.ProjectAsWrapper(source, selectExpand, _customer, _customers);

            // Assert
            SelectExpandWrapper<QueryCustomer> customerWrapper = Expression.Lambda(projection).Compile().DynamicInvoke() as SelectExpandWrapper<QueryCustomer>;
            Assert.NotNull(customerWrapper.ModelID);
            Assert.Same(_model, ModelContainer.GetModel(customerWrapper.ModelID));
        }

        [Fact]
        public void ProjectAsWrapper_Collection_ProjectedValueContainsSubKeys_IfDollarRefInDollarExpand()
        {
            // Arrange
            string expand = "Orders/$ref";
            QueryCustomer customer1 = new QueryCustomer
            {
                Orders = new[]
                {
                    new QueryOrder { Id = 8 },
                    new QueryVipOrder { Id = 9 }
                }
            };
            QueryCustomer customer2 = new QueryCustomer
            {
                Orders = new[]
                {
                    new QueryOrder { Id = 18 },
                    new QueryVipOrder { Id = 19 }
                }
            };
            Expression source = Expression.Constant(new[] { customer1, customer2 });

            SelectExpandClause selectExpandClause = ParseSelectExpand(null, expand, _model, _customer, _customers);
            Assert.NotNull(selectExpandClause);

            // Act
            Expression projection = _binder.ProjectAsWrapper(source, selectExpandClause, _customer, _customers);

            // Assert
            Assert.Equal(ExpressionType.Call, projection.NodeType);
            var customerWrappers = Expression.Lambda(projection).Compile().DynamicInvoke() as IEnumerable<SelectExpandWrapper<QueryCustomer>>;
            Assert.Equal(2, customerWrappers.Count());

            var orders = customerWrappers.ElementAt(0).Container.ToDictionary(PropertyMapper)["Orders"] as IEnumerable<SelectExpandWrapper<QueryOrder>>;
            Assert.NotNull(orders);
            Assert.Equal(2, orders.Count());
            Assert.Equal(8, orders.ElementAt(0).Container.ToDictionary(PropertyMapper)["Id"]);
            Assert.Equal(9, orders.ElementAt(1).Container.ToDictionary(PropertyMapper)["Id"]);

            orders = customerWrappers.ElementAt(1).Container.ToDictionary(PropertyMapper)["Orders"] as IEnumerable<SelectExpandWrapper<QueryOrder>>;
            Assert.NotNull(orders);
            Assert.Equal(2, orders.Count());
            Assert.Equal(18, orders.ElementAt(0).Container.ToDictionary(PropertyMapper)["Id"]);
            Assert.Equal(19, orders.ElementAt(1).Container.ToDictionary(PropertyMapper)["Id"]);
        }

        [Fact]
        public void ProjectAsWrapper_Collection_ProjectedValueContainsSubKeys_IfDollarRefInDollarExpand_AndNestedTopAndSkip()
        {
            // Arrange
            string expand = "Orders/$ref($top=1;$skip=1)";
            QueryCustomer customer1 = new QueryCustomer
            {
                Orders = new[]
                {
                    new QueryOrder { Id = 8 },
                    new QueryVipOrder { Id = 9 }
                }
            };
            QueryCustomer customer2 = new QueryCustomer
            {
                Orders = new[]
                {
                    new QueryOrder { Id = 18 },
                    new QueryVipOrder { Id = 19 }
                }
            };

            Expression source = Expression.Constant(new[] { customer1, customer2 });
            SelectExpandClause selectExpandClause = ParseSelectExpand(null, expand, _model, _customer, _customers);
            Assert.NotNull(selectExpandClause);

            // Act
            Expression projection = _binder.ProjectAsWrapper(source, selectExpandClause, _customer, _customers);

            // Assert
            Assert.Equal(ExpressionType.Call, projection.NodeType);
            var customerWrappers = Expression.Lambda(projection).Compile().DynamicInvoke() as IEnumerable<SelectExpandWrapper<QueryCustomer>>;
            Assert.Equal(2, customerWrappers.Count());

            var orders = customerWrappers.ElementAt(0).Container.ToDictionary(PropertyMapper)["Orders"] as IEnumerable<SelectExpandWrapper<QueryOrder>>;
            Assert.NotNull(orders);
            var order = Assert.Single(orders); // only one
            Assert.Equal(9, order.Container.ToDictionary(PropertyMapper)["Id"]);

            orders = customerWrappers.ElementAt(1).Container.ToDictionary(PropertyMapper)["Orders"] as IEnumerable<SelectExpandWrapper<QueryOrder>>;
            Assert.NotNull(orders);
            order = Assert.Single(orders);
            Assert.Equal(19, order.Container.ToDictionary(PropertyMapper)["Id"]);
        }

        [Theory]
        [InlineData("*")]
        [InlineData("Id,*")]
        [InlineData("*,Name")]
        [InlineData("*,HomeAddress/Street")]
        [InlineData("")]
        public void ProjectAsWrapper_Element_ProjectedValueContainsInstance_IfSelectionIsAll(string select)
        {
            // Arrange
            QueryCustomer aCustomer = new QueryCustomer();
            Expression source = Expression.Constant(aCustomer);

            SelectExpandClause selectExpandClause = ParseSelectExpand(select, null, _model, _customer, _customers);
            Assert.NotNull(selectExpandClause);

            // Act
            Expression projection = _binder.ProjectAsWrapper(source, selectExpandClause, _customer, _customers);

            // Assert
            Assert.Equal(ExpressionType.MemberInit, projection.NodeType);
            Assert.NotEmpty((projection as MemberInitExpression).Bindings.Where(p => p.Member.Name == "Instance"));
            SelectExpandWrapper<QueryCustomer> customerWrapper = Expression.Lambda(projection).Compile().DynamicInvoke() as SelectExpandWrapper<QueryCustomer>;
            Assert.Same(aCustomer, customerWrapper.Instance);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void ProjectAsWrapper_Element_ProjectedValueDoesNotContainInstance_IfSelectionIsPartial(bool alwaysSetSelectExpandWrapperInstance)
        {
            // Arrange
            string select = "Id,Orders";
            string expand = "Orders";
            QueryCustomer aCustomer = new QueryCustomer
            {
                Orders = new QueryOrder[0]
            };
            Expression source = Expression.Constant(aCustomer);

            SelectExpandClause selectExpandClause = ParseSelectExpand(select, expand, _model, _customer, _customers);
            Assert.NotNull(selectExpandClause);

            SelectExpandBinder binder = GetBinder<QueryCustomer>(_model, alwaysSetSelectExpandWrapperInstance);

            // Act
            Expression projection = binder.ProjectAsWrapper(source, selectExpandClause, _customer, _customers);

            // Assert
            Assert.Equal(ExpressionType.MemberInit, projection.NodeType);

            if (alwaysSetSelectExpandWrapperInstance)
            {
                Assert.NotEmpty((projection as MemberInitExpression).Bindings.Where(p => p.Member.Name == "Instance"));
                Assert.Empty((projection as MemberInitExpression).Bindings.Where(p => p.Member.Name == "InstanceType"));
            }
            else
            {
                Assert.Empty((projection as MemberInitExpression).Bindings.Where(p => p.Member.Name == "Instance"));
                Assert.NotEmpty((projection as MemberInitExpression).Bindings.Where(p => p.Member.Name == "InstanceType"));
            }

            SelectExpandWrapper<QueryCustomer> customerWrapper = Expression.Lambda(projection).Compile().DynamicInvoke() as SelectExpandWrapper<QueryCustomer>;

            if (alwaysSetSelectExpandWrapperInstance)
            {
                Assert.Same(aCustomer, customerWrapper.Instance);
                Assert.Null(customerWrapper.InstanceType);
            }
            else
            {
                Assert.Null(customerWrapper.Instance);
                Assert.Equal("Microsoft.AspNet.OData.Test.Query.Expressions.QueryCustomer", customerWrapper.InstanceType);
            }
        }

        [Theory]
        [InlineData("Name", "OData")]
        [InlineData("Age", 31)]
        public void ProjectAsWrapper_Element_ProjectedValueContains_SelectedStructuralProperties(string select, object expect)
        {
            // Arrange
            QueryCustomer aCustomer = new QueryCustomer
            {
                Name = "OData",
                Age = 31
            };
            Expression source = Expression.Constant(aCustomer);

            SelectExpandClause selectExpandClause = ParseSelectExpand(select, null, _model, _customer, _customers);
            Assert.NotNull(selectExpandClause);

            // Act
            Expression projection = _binder.ProjectAsWrapper(source, selectExpandClause, _customer, _customers);

            // Assert
            SelectExpandWrapper<QueryCustomer> customerWrapper = Expression.Lambda(projection).Compile().DynamicInvoke() as SelectExpandWrapper<QueryCustomer>;
            Assert.Equal(expect, customerWrapper.Container.ToDictionary(PropertyMapper)[select]);
        }


        [Theory]
        [InlineData("Emails", new[] { "E1", "E3", "E2" })]
        [InlineData("Emails($orderby=$it)", new[] { "E1", "E2", "E3" })]
        [InlineData("Emails($orderby=$it desc)", new[] { "E3", "E2", "E1" })]
        [InlineData("Emails($top=1)", new[] { "E1" })]
        [InlineData("Emails($top=1;$skip=1)", new[] { "E3" })]
        [InlineData("Emails($filter=$it le 'E2')", new[] { "E1", "E2" })]
        public void ProjectAsWrapper_Element_ProjectedValueContains_SelectedCollectStructuralProperties(string select, object expect)
        {
            // Arrange
            QueryCustomer aCustomer = new QueryCustomer
            {
                Emails = new [] { "E1", "E3", "E2" }
            };
            Expression source = Expression.Constant(aCustomer);
            SelectExpandClause selectExpandClause = ParseSelectExpand(select, null, _model, _customer, _customers);
            Assert.NotNull(selectExpandClause);

            // Act
            Expression projection = _binder.ProjectAsWrapper(source, selectExpandClause, _customer, _customers);

            // Assert
            SelectExpandWrapper<QueryCustomer> customerWrapper = Expression.Lambda(projection).Compile().DynamicInvoke() as SelectExpandWrapper<QueryCustomer>;
            var emails = customerWrapper.Container.ToDictionary(PropertyMapper)["Emails"];
            Assert.Equal(expect, emails);
        }

        [Theory]
        [InlineData("HomeAddress/Street,HomeAddress/Region")]
        [InlineData("HomeAddress($select=Street, Region)")]
        public void ProjectAsWrapper_Element_ProjectedValueContains_SelectedSubStructuralProperties(string select)
        {
            // Arrange
            QueryCustomer aCustomer = new QueryCustomer
            {
                HomeAddress = new QueryAddress
                {
                    Street = "148TH AVE NE",
                    Region = "Redmond"
                }
            };
            Expression source = Expression.Constant(aCustomer);
            SelectExpandClause selectExpandClause = ParseSelectExpand(select, null, _model, _customer, _customers);
            Assert.NotNull(selectExpandClause);

            // Act
            Expression projection = _binder.ProjectAsWrapper(source, selectExpandClause, _customer, _customers);

            // Assert
            SelectExpandWrapper<QueryCustomer> customerWrapper = Expression.Lambda(projection).Compile().DynamicInvoke() as SelectExpandWrapper<QueryCustomer>;
            SelectExpandWrapper<QueryAddress> addressWrapper = customerWrapper.Container.ToDictionary(PropertyMapper)["HomeAddress"] as SelectExpandWrapper<QueryAddress>;
            var addressProperties = addressWrapper.Container.ToDictionary(PropertyMapper);
            Assert.Equal(2, addressProperties.Count);
            Assert.Equal("148TH AVE NE", addressProperties["Street"]);
            Assert.Equal("Redmond", addressProperties["Region"]);
        }

        [Theory]
        [InlineData("Addresses/Street,Addresses/Region")]
        [InlineData("Addresses($select=Street,Region)")]
        public void ProjectAsWrapper_Element_ProjectedValueContains_SelectedTopCollectionWithSubStructuralProperties(string select)
        {
            // Arrange
            QueryCustomer aCustomer = new QueryCustomer
            {
                Addresses = new List<QueryAddress>
                {
                    new QueryCnAddress
                    {
                        Street = "Being Rd",
                        Region = "Region#1"
                    },
                    new QueryUsAddress
                    {
                        Street = "148TH AVE NE",
                        Region = "Region#2"
                    }
                }
            };
            Expression source = Expression.Constant(aCustomer);
            SelectExpandClause selectExpandClause = ParseSelectExpand(select, null, _model, _customer, _customers);
            Assert.NotNull(selectExpandClause);

            // Act
            Expression projection = _binder.ProjectAsWrapper(source, selectExpandClause, _customer, _customers);

            // Assert
            SelectExpandWrapper<QueryCustomer> customerWrapper = Expression.Lambda(projection).Compile().DynamicInvoke() as SelectExpandWrapper<QueryCustomer>;
            var addressesWrapper = customerWrapper.Container.ToDictionary(PropertyMapper)["Addresses"] as IEnumerable<SelectExpandWrapper<QueryAddress>>;
            Assert.Equal(2, addressesWrapper.Count());

            var properties = addressesWrapper.ElementAt(0).Container.ToDictionary(PropertyMapper);
            Assert.Equal("Being Rd", properties["Street"]);
            Assert.Equal("Region#1", properties["Region"]);

            properties = addressesWrapper.ElementAt(1).Container.ToDictionary(PropertyMapper);
            Assert.Equal("148TH AVE NE", properties["Street"]);
            Assert.Equal("Region#2", properties["Region"]);
        }

        [Fact]
        public void ProjectAsWrapper_Element_ProjectedValueContains_SelectedTopAndSubStructuralProperties()
        {
            // Arrange
            QueryCustomer aCustomer = new QueryCustomer
            {
                Name = "Peter",
                HomeAddress = new QueryAddress
                {
                    Street = "148TH AVE NE",
                    Region = "Redmond"
                }
            };
            Expression source = Expression.Constant(aCustomer);
            string select = "Name,HomeAddress/Street";
            SelectExpandClause selectExpandClause = ParseSelectExpand(select, null, _model, _customer, _customers);
            Assert.NotNull(selectExpandClause);

            // Act
            Expression projection = _binder.ProjectAsWrapper(source, selectExpandClause, _customer, _customers);

            // Assert
            SelectExpandWrapper<QueryCustomer> customerWrapper = Expression.Lambda(projection).Compile().DynamicInvoke() as SelectExpandWrapper<QueryCustomer>;

            var customerProperties = customerWrapper.Container.ToDictionary(PropertyMapper);
            Assert.Equal("Peter", customerProperties["Name"]);

            SelectExpandWrapper<QueryAddress> addressWrapper = customerProperties["HomeAddress"] as SelectExpandWrapper<QueryAddress>;
            var addressProperties = addressWrapper.Container.ToDictionary(PropertyMapper);
            var streetProperty = Assert.Single(addressProperties);
            Assert.Equal("Street", streetProperty.Key);
            Assert.Equal("148TH AVE NE", streetProperty.Value);
        }

        [Theory]
        [InlineData("HomeAddress/Codes", "C1,C4,C2")]
        [InlineData("HomeAddress($select=Codes)", "C1,C4,C2")]
        [InlineData("HomeAddress/Codes($top=2;$skip=1)", "C4,C2")]
        [InlineData("HomeAddress($select=Codes($top=1;$skip=2))", "C2")]
        [InlineData("HomeAddress/Codes($orderby=$it)", "C1,C2,C4")]
        [InlineData("HomeAddress($select=Codes($orderby=$it desc))", "C4,C2,C1")]
        [InlineData("HomeAddress($select=Codes($filter=$it eq 'C2'))", "C2")]
        public void ProjectAsWrapper_Element_ProjectedValueContains_SelectedSubCollectionStructuralProperties(string select, string expect)
        {
            // Arrange
            QueryCustomer aCustomer = new QueryCustomer
            {
                HomeAddress = new QueryAddress
                {
                    Codes = new [] { "C1", "C4" , "C2" }
                }
            };
            Expression source = Expression.Constant(aCustomer);
            SelectExpandClause selectExpandClause = ParseSelectExpand(select, null, _model, _customer, _customers);
            Assert.NotNull(selectExpandClause);

            // Act
            Expression projection = _binder.ProjectAsWrapper(source, selectExpandClause, _customer, _customers);

            // Assert
            SelectExpandWrapper<QueryCustomer> customerWrapper = Expression.Lambda(projection).Compile().DynamicInvoke() as SelectExpandWrapper<QueryCustomer>;
            SelectExpandWrapper<QueryAddress> addressWrapper = customerWrapper.Container.ToDictionary(PropertyMapper)["HomeAddress"] as SelectExpandWrapper<QueryAddress>;
            var addressProperties = addressWrapper.Container.ToDictionary(PropertyMapper);
            var codeProperty = Assert.Single(addressProperties);
            Assert.Equal("Codes", codeProperty.Key);
            var codes = codeProperty.Value as IEnumerable<string>;
            Assert.Equal(expect, string.Join(",", codes));
        }

        [Theory]
        [InlineData("Addresses/Codes", "C1,C4,C2", "C3,C6,C5")]
        [InlineData("Addresses($select=Codes)", "C1,C4,C2", "C3,C6,C5")]
        [InlineData("Addresses/Codes($top=2;$skip=1)", "C4,C2", "C6,C5")]
        [InlineData("Addresses($select=Codes($top=1;$skip=2))", "C2", "C5")]
        [InlineData("Addresses/Codes($orderby=$it)", "C1,C2,C4", "C3,C5,C6")]
        [InlineData("Addresses($select=Codes($orderby=$it desc))", "C4,C2,C1", "C6,C5,C3")]
        [InlineData("Addresses($select=Codes($filter=$it eq 'C2'))", "C2", "")]
        public void ProjectAsWrapper_Element_ProjectedValueContains_SelectedToCollectionAndSubCollectionStructuralProperties(string select, string expect1, string expect2)
        {
            // Arrange
            QueryCustomer aCustomer = new QueryCustomer
            {
                Addresses = new List<QueryAddress>
                {
                    new QueryCnAddress
                    {
                        Codes = new [] { "C1", "C4" , "C2" }
                    },
                    new QueryUsAddress
                    {
                        Codes = new [] { "C3", "C6", "C5" }
                    }
                }
            };

            Expression source = Expression.Constant(aCustomer);
            SelectExpandClause selectExpandClause = ParseSelectExpand(select, null, _model, _customer, _customers);
            Assert.NotNull(selectExpandClause);

            // Act
            Expression projection = _binder.ProjectAsWrapper(source, selectExpandClause, _customer, _customers);

            // Assert
            SelectExpandWrapper<QueryCustomer> customerWrapper = Expression.Lambda(projection).Compile().DynamicInvoke() as SelectExpandWrapper<QueryCustomer>;
            var addressesWrapper = customerWrapper.Container.ToDictionary(PropertyMapper)["Addresses"] as IEnumerable<SelectExpandWrapper<QueryAddress>>;
            Assert.Equal(2, addressesWrapper.Count());

            var properties = addressesWrapper.ElementAt(0).Container.ToDictionary(PropertyMapper);
            var codes = properties["Codes"] as IEnumerable<string>;
            Assert.Equal(expect1, string.Join(",", codes));

            properties = addressesWrapper.ElementAt(1).Container.ToDictionary(PropertyMapper);
            codes = properties["Codes"] as IEnumerable<string>;
            Assert.Equal(expect2, string.Join(",", codes));
        }


        [Fact(Skip = "ODL parses the following select as \"HomeAddress\\dynamic\\dynamic\", that's not correct.")]
        public void ProjectAsWrapper_Element_ProjectedValueContains_SelectedTypeCastSubStructuralProperties()
        {
            // Arrange
            QueryCustomer aCustomer = new QueryCustomer
            {
                HomeAddress = new QueryCnAddress
                {
                    Street = "Cn Street",
                    PostCode = "201501",
                }
            };
            Expression source = Expression.Constant(aCustomer);

            string select = "HomeAddress/Street,HomeAddress/Microsoft.AspNet.OData.Test.Query.Expressions.QueryCnAddress/PostCode";
            SelectExpandClause selectExpandClause = ParseSelectExpand(select, null, _model, _customer, _customers);
            Assert.NotNull(selectExpandClause);

            // Act
            Expression projection = _binder.ProjectAsWrapper(source, selectExpandClause, _customer, _customers);

            // Assert
            SelectExpandWrapper<QueryCustomer> customerWrapper = Expression.Lambda(projection).Compile().DynamicInvoke() as SelectExpandWrapper<QueryCustomer>;
            SelectExpandWrapper<QueryAddress> addressWrapper = customerWrapper.Container.ToDictionary(PropertyMapper)["HomeAddress"] as SelectExpandWrapper<QueryAddress>;
            var addressProperties = addressWrapper.Container.ToDictionary(PropertyMapper);
            Assert.Equal(2, addressProperties.Count);

            Assert.Equal("Cn Street", addressProperties["Street"]);
            Assert.Equal("201501", addressProperties["PostCode"]);
        }

        [Fact]
        public void ProjectAsWrapper_Element_ProjectedValueContainsSubKeys_IfDollarRefInDollarExpand()
        {
            // Arrange
            string expand = "Orders/$ref";
            QueryCustomer aCustomer = new QueryCustomer
            {
                Orders = new[]
                {
                    new QueryOrder { Id = 42 },
                    new QueryVipOrder { Id = 38 }
                }
            };
            Expression source = Expression.Constant(aCustomer);
            SelectExpandClause selectExpandClause = ParseSelectExpand(null, expand, _model, _customer, _customers);
            Assert.NotNull(selectExpandClause);

            // Act
            Expression projection = _binder.ProjectAsWrapper(source, selectExpandClause, _customer, _customers);

            // Assert
            Assert.Equal(ExpressionType.MemberInit, projection.NodeType);
            Assert.NotEmpty((projection as MemberInitExpression).Bindings.Where(p => p.Member.Name == "Instance"));
            SelectExpandWrapper<QueryCustomer> customerWrapper = Expression.Lambda(projection).Compile().DynamicInvoke() as SelectExpandWrapper<QueryCustomer>;

            var orders = customerWrapper.Container.ToDictionary(PropertyMapper)["Orders"] as IEnumerable<SelectExpandWrapper<QueryOrder>>;
            Assert.NotNull(orders);
            Assert.Equal(2, orders.Count());
            Assert.Equal(42, orders.ElementAt(0).Container.ToDictionary(PropertyMapper)["Id"]);
            Assert.Equal(38, orders.ElementAt(1).Container.ToDictionary(PropertyMapper)["Id"]);
        }

        [Fact]
        public void ProjectAsWrapper_Element_ProjectedValueContainsSubKeys_IfDollarRefInDollarExpand_AndNestedFilterClause()
        {
            // Arrange
            string expand = "Orders/$ref($filter =Title eq 'abc')";
            QueryCustomer aCustomer = new QueryCustomer
            {
                Orders = new[]
                {
                    new QueryOrder { Id = 42, Title = "xyz" },
                    new QueryVipOrder { Id = 38, Title = "abc" }
                }
            };

            Expression source = Expression.Constant(aCustomer);
            SelectExpandClause selectExpandClause = ParseSelectExpand(null, expand, _model, _customer, _customers);
            Assert.NotNull(selectExpandClause);

            // Act
            Expression projection = _binder.ProjectAsWrapper(source, selectExpandClause, _customer, _customers);

            // Assert
            Assert.Equal(ExpressionType.MemberInit, projection.NodeType);
            Assert.NotEmpty((projection as MemberInitExpression).Bindings.Where(p => p.Member.Name == "Instance"));
            SelectExpandWrapper<QueryCustomer> customerWrapper = Expression.Lambda(projection).Compile().DynamicInvoke() as SelectExpandWrapper<QueryCustomer>;

            var orders = customerWrapper.Container.ToDictionary(PropertyMapper)["Orders"] as IEnumerable<SelectExpandWrapper<QueryOrder>>;
            Assert.NotNull(orders);
            var order = Assert.Single(orders); // only one
            Assert.Equal(38, order.Container.ToDictionary(PropertyMapper)["Id"]);
        }

        [Fact]
        public void ProjectAsWrapper_Element_ProjectedValueContainsSubKeys_IfDollarRefInDollarExpandOnSubNavigationProperty()
        {
            // Arrange
            string expand = "HomeAddress/RelatedCity/$ref";
            QueryCustomer aCustomer = new QueryCustomer
            {
                HomeAddress = new QueryAddress
                {
                    Street = "156TH",
                    RelatedCity = new QueryCity
                    {
                        Id = 101
                    }
                }
            };
            Expression source = Expression.Constant(aCustomer);
            SelectExpandClause selectExpandClause = ParseSelectExpand(null, expand, _model, _customer, _customers);
            Assert.NotNull(selectExpandClause);

            // Act
            Expression projection = _binder.ProjectAsWrapper(source, selectExpandClause, _customer, _customers);

            // Assert
            Assert.Equal(ExpressionType.MemberInit, projection.NodeType);
            Assert.NotEmpty((projection as MemberInitExpression).Bindings.Where(p => p.Member.Name == "Instance"));
            SelectExpandWrapper<QueryCustomer> customerWrapper = Expression.Lambda(projection).Compile().DynamicInvoke() as SelectExpandWrapper<QueryCustomer>;

            var homeAddress = customerWrapper.Container.ToDictionary(PropertyMapper)["HomeAddress"] as SelectExpandWrapper<QueryAddress>;
            var relatedCity = homeAddress.Container.ToDictionary(PropertyMapper)["RelatedCity"] as SelectExpandWrapper<QueryCity>;
            Assert.Equal(101, relatedCity.Container.ToDictionary(PropertyMapper)["Id"]);
        }

        [Theory]
        [InlineData("Name", false, 3)] // 3 => Id, Name, ETag
        [InlineData("HomeAddress/Street", true, 3)] // 3 => ID, HomeAddress/Street, ETag
        [InlineData("Name,HomeAddress/Street", true, 4)] // 4 => ID, Name, HomeAddress/Street, ETag
        [InlineData("NS.*", false, 2)] // 2 => ID, ETag
        public void ProjectAsWrapper_ReturnsKeysAndConcurrencyProperties_EvenIfNotPresentInSelectClause(string select, bool containAddress, int count)
        {
            // Arrange
            IEdmStructuralProperty etagProperty =  _customer.DeclaredStructuralProperties().FirstOrDefault(c => c.Name == "CustomerETag");
            Assert.NotNull(etagProperty); // Guard
            ((EdmModel)_model).SetOptimisticConcurrencyAnnotation(_customers, new[] { etagProperty });

            QueryCustomer aCustomer = new QueryCustomer
            {
                Id = 42,
                Name = "Peter",
                HomeAddress = new QueryAddress
                {
                    Street = "MyStreet"
                },
                CustomerETag = 1.14926
            };
            Expression source = Expression.Constant(aCustomer);
            SelectExpandClause selectExpandClause = ParseSelectExpand(select, null, _model, _customer, _customers);
            Assert.NotNull(selectExpandClause);

            // Act
            Expression projection = _binder.ProjectAsWrapper(source, selectExpandClause, _customer, _customers);

            // Assert
            SelectExpandWrapper<QueryCustomer> customerWrapper = Expression.Lambda(projection).Compile().DynamicInvoke() as SelectExpandWrapper<QueryCustomer>;
            var customerSelectedProperties = customerWrapper.Container.ToDictionary(PropertyMapper);
            Assert.Equal(count, customerSelectedProperties.Count);

            Assert.Equal(42, customerSelectedProperties["Id"]);
            Assert.Equal(1.14926, customerSelectedProperties["CustomerETag"]);

            if (containAddress)
            {
                SelectExpandWrapper<QueryAddress> addressWrapper = customerSelectedProperties["HomeAddress"] as SelectExpandWrapper<QueryAddress>;
                var addressSelectedProperties = addressWrapper.Container.ToDictionary(PropertyMapper);
                Assert.Single(addressSelectedProperties);
                Assert.Equal("MyStreet", addressSelectedProperties["Street"]);
            }
        }

        #region GetSelectExpandProperties Tests
        [Theory]
        [InlineData("HomeAddress")] // $select=property
        [InlineData("Addresses")]
        [InlineData("Emails")]
        [InlineData("Name")]
        [InlineData("Microsoft.AspNet.OData.Test.Query.Expressions.QueryVipCustomer/Level")] // $select=typeCast/Property
        [InlineData("Microsoft.AspNet.OData.Test.Query.Expressions.QueryVipCustomer/Birthday")]
        [InlineData("Microsoft.AspNet.OData.Test.Query.Expressions.QueryVipCustomer/Taxes")]
        [InlineData("Microsoft.AspNet.OData.Test.Query.Expressions.QueryVipCustomer/VipAddress")]
        [InlineData("Microsoft.AspNet.OData.Test.Query.Expressions.QueryVipCustomer/VipAddresses")]
        public void GetSelectExpandProperties_ForDirectProperty_OutputCorrectProperties(string select)
        {
            // Arrange
            SelectExpandClause selectExpandClause = ParseSelectExpand(select, null, _model, _customer, _customers);
            Assert.NotNull(selectExpandClause);

            // Act
            IDictionary<IEdmStructuralProperty, PathSelectItem> propertiesToInclude;
            IDictionary<IEdmNavigationProperty, ExpandedReferenceSelectItem> propertiesToExpand;
            ISet<IEdmStructuralProperty> autoSelectedProperties;
            bool isContainDynamicProperty = SelectExpandBinder.GetSelectExpandProperties(_model, _customer, _customers, selectExpandClause,
                out propertiesToInclude,
                out propertiesToExpand,
                out autoSelectedProperties);

            // Assert
            Assert.False(selectExpandClause.AllSelected); // guard
            SelectItem selectItem = selectExpandClause.SelectedItems.First();
            PathSelectItem pathSelectItem = Assert.IsType<PathSelectItem>(selectItem); // Guard

            Assert.False(isContainDynamicProperty);
            Assert.Null(propertiesToExpand); // No navigation property to expand

            Assert.NotNull(autoSelectedProperties); // auto select the keys
            Assert.Equal("Id", Assert.Single(autoSelectedProperties).Name);

            Assert.NotNull(propertiesToInclude); // has one structural property to select
            var propertyToInclude = Assert.Single(propertiesToInclude);

            string[] segments = select.Split('/');
            if (segments.Length == 2)
            {
                Assert.Equal(segments[1], propertyToInclude.Key.Name);
            }
            else
            {
                Assert.Equal(segments[0], propertyToInclude.Key.Name);
            }

            Assert.NotNull(propertyToInclude.Value);
            Assert.Same(pathSelectItem, propertyToInclude.Value);
        }

        [Theory]
        [InlineData("HomeAddress,HomeAddress/Codes")]
        [InlineData("HomeAddress,HomeAddress/Codes($top=2)")]
        public void GetSelectExpandProperties_ForSelectAllAndSelectSpecialy_OutputCorrectProperties(string select)
        {
            // Arrange
            SelectExpandClause selectExpandClause = ParseSelectExpand(select, null, _model, _customer, _customers);
            Assert.NotNull(selectExpandClause);

            // Act
            IDictionary<IEdmStructuralProperty, PathSelectItem> propertiesToInclude;
            IDictionary<IEdmNavigationProperty, ExpandedReferenceSelectItem> propertiesToExpand;
            ISet<IEdmStructuralProperty> autoSelectedProperties;
            bool isContainDynamicProperty = SelectExpandBinder.GetSelectExpandProperties(_model, _customer, _customers, selectExpandClause,
                out propertiesToInclude,
                out propertiesToExpand,
                out autoSelectedProperties);

            // Assert
            Assert.False(isContainDynamicProperty);
            Assert.Null(propertiesToExpand);

            Assert.NotNull(autoSelectedProperties);
            Assert.Equal("Id", Assert.Single(autoSelectedProperties).Name); // Key and ETag

            Assert.NotNull(propertiesToInclude);
            var propertyToInclude = Assert.Single(propertiesToInclude);
            Assert.Equal("HomeAddress", propertyToInclude.Key.Name);

            Assert.NotNull(propertyToInclude.Value.SelectAndExpand);
            Assert.True(propertyToInclude.Value.SelectAndExpand.AllSelected);
            var selectItem = Assert.Single(propertyToInclude.Value.SelectAndExpand.SelectedItems);
            Assert.IsType<PathSelectItem>(selectItem);
        }

        [Theory]
        [InlineData("HomeAddress/Street,HomeAddress/Region,HomeAddress/Codes")]
        [InlineData("HomeAddress($select=Street,Region,Codes)")]
        public void GetSelectExpandProperties_ForMultipleSubPropertiesSelection_OutputCorrectProperties(string select)
        {
            // Arrange
            SelectExpandClause selectExpandClause = ParseSelectExpand(select, null, _model, _customer, _customers);
            Assert.NotNull(selectExpandClause);

            // Act
            IDictionary<IEdmStructuralProperty, PathSelectItem> propertiesToInclude;
            IDictionary<IEdmNavigationProperty, ExpandedReferenceSelectItem> propertiesToExpand;
            ISet<IEdmStructuralProperty> autoSelectedProperties;
            bool isContainDynamicProperty = SelectExpandBinder.GetSelectExpandProperties(_model, _customer, _customers, selectExpandClause,
                out propertiesToInclude,
                out propertiesToExpand,
                out autoSelectedProperties);

            // Assert
            Assert.False(isContainDynamicProperty);
            Assert.Null(propertiesToExpand); // No navigation property to expand

            Assert.NotNull(autoSelectedProperties); // auto select the keys & ETags
            Assert.Equal("Id", Assert.Single(autoSelectedProperties).Name);

            Assert.NotNull(propertiesToInclude); // has one structural property to select
            var propertyToInclude = Assert.Single(propertiesToInclude);
            Assert.Equal("HomeAddress", propertyToInclude.Key.Name);

            Assert.NotNull(propertyToInclude.Value);

            Assert.NotNull(propertyToInclude.Value.SelectAndExpand); // Sub select & expand
            Assert.False(propertyToInclude.Value.SelectAndExpand.AllSelected);

            Assert.Equal(3, propertyToInclude.Value.SelectAndExpand.SelectedItems.Count()); // Street, Region, Codes

            Assert.Equal(new [] { "Street", "Region", "Codes"},
                propertyToInclude.Value.SelectAndExpand.SelectedItems.Select(s =>
            {
                PathSelectItem subSelectItem = (PathSelectItem)s;
                PropertySegment propertySegment = subSelectItem.SelectedPath.Single() as PropertySegment;
                Assert.NotNull(propertySegment);
                return propertySegment.Property.Name;
            }));
        }

        [Theory]
        [InlineData("CustomerDynamicProperty1", true)]
        [InlineData("CustomerDynamicProperty2", true)]
        [InlineData("HomeAddress/AddressDynPriperty", false)]
        public void GetSelectExpandProperties_ForDynamicProperty_OutputCorrectBoolean(string select, bool expect)
        {
            // Arrange
            SelectExpandClause selectExpandClause = ParseSelectExpand(select, null, _model, _customer, _customers);
            Assert.NotNull(selectExpandClause);

            // Act
            IDictionary<IEdmStructuralProperty, PathSelectItem> propertiesToInclude;
            IDictionary<IEdmNavigationProperty, ExpandedReferenceSelectItem > propertiesToExpand;
            ISet<IEdmStructuralProperty> autoSelectedProperties;
            bool isContainDynamicProperty = SelectExpandBinder.GetSelectExpandProperties(_model, _customer, _customers, selectExpandClause,
                out propertiesToInclude, out propertiesToExpand, out autoSelectedProperties);

            // Assert
            if (select.StartsWith("HomeAddress"))
            {
                Assert.NotNull(propertiesToInclude);
            }
            else
            {
                Assert.Null(propertiesToInclude);
            }

            Assert.Null(propertiesToExpand);
            Assert.NotNull(autoSelectedProperties);

            Assert.False(selectExpandClause.AllSelected); // guard
            Assert.Equal(expect, isContainDynamicProperty);
        }

        [Theory]
        [InlineData("Orders")]
        [InlineData("PrivateOrder")]
        [InlineData("Microsoft.AspNet.OData.Test.Query.Expressions.QueryVipCustomer/SpecialOrder")]
        [InlineData("Microsoft.AspNet.OData.Test.Query.Expressions.QueryVipCustomer/SpecialOrders")]
        public void GetSelectExpandProperties_SkipForNavigationSelection(string select)
        {
            // Arrange
            SelectExpandClause selectExpandClause = ParseSelectExpand(select, null, _model, _customer, _customers);
            Assert.NotNull(selectExpandClause);

            // Act
            IDictionary<IEdmStructuralProperty, PathSelectItem> propertiesToInclude;
            IDictionary<IEdmNavigationProperty, ExpandedReferenceSelectItem> propertiesToExpand;
            ISet<IEdmStructuralProperty> autoSelectedProperties;
            bool isContainDynamicProperty = SelectExpandBinder.GetSelectExpandProperties(_model, _customer, _customers, selectExpandClause,
                out propertiesToInclude,
                out propertiesToExpand,
                out autoSelectedProperties);

            // Assert
            Assert.False(isContainDynamicProperty);
            Assert.Null(propertiesToInclude);
            Assert.Null(propertiesToExpand);

            Assert.NotNull(autoSelectedProperties);
        }

        [Theory]
        [InlineData("PrivateOrder")]
        [InlineData("PrivateOrder/$ref")]
        [InlineData("Orders")]
        [InlineData("Orders/$ref")]
        [InlineData("Orders($top=2;$count=true)")]
        [InlineData("Microsoft.AspNet.OData.Test.Query.Expressions.QueryVipCustomer/SpecialOrder")]
        [InlineData("Microsoft.AspNet.OData.Test.Query.Expressions.QueryVipCustomer/SpecialOrder/$ref")]
        [InlineData("Microsoft.AspNet.OData.Test.Query.Expressions.QueryVipCustomer/SpecialOrders")]
        [InlineData("Microsoft.AspNet.OData.Test.Query.Expressions.QueryVipCustomer/SpecialOrders/$ref")]
        [InlineData("Microsoft.AspNet.OData.Test.Query.Expressions.QueryVipCustomer/SpecialOrders($search=abc)")]
        public void GetSelectExpandProperties_ForDirectNavigationProperty_ReturnsProperties(string expand)
        {
            // Arrange
            SelectExpandClause selectExpandClause = ParseSelectExpand(null, expand, _model, _customer, _customers);
            Assert.NotNull(selectExpandClause);

            // Act
            IDictionary<IEdmStructuralProperty, PathSelectItem> propertiesToInclude;
            IDictionary<IEdmNavigationProperty, ExpandedReferenceSelectItem> propertiesToExpand;
            ISet<IEdmStructuralProperty> autoSelectedProperties;
            bool isContainDynamicProperty = SelectExpandBinder.GetSelectExpandProperties(_model, _customer, _customers, selectExpandClause,
                out propertiesToInclude,
                out propertiesToExpand,
                out autoSelectedProperties);

            // Assert
            var selectItem = Assert.Single(selectExpandClause.SelectedItems);
            ExpandedReferenceSelectItem expandedItem = selectItem as ExpandedReferenceSelectItem;
            Assert.NotNull(expandedItem);
            var navigationSegment = expandedItem.PathToNavigationProperty.First(p => p is NavigationPropertySegment) as NavigationPropertySegment;

            Assert.False(isContainDynamicProperty); // not container dynamic properties selection
            Assert.Null(propertiesToInclude); // no structural properties to include
            Assert.Null(autoSelectedProperties); // no auto select properties

            Assert.NotNull(propertiesToExpand);
            var propertyToExpand = Assert.Single(propertiesToExpand);

            Assert.Same(navigationSegment.NavigationProperty, propertyToExpand.Key);
            Assert.Same(expandedItem, propertyToExpand.Value);
        }

        [Theory]
        [InlineData("HomeAddress/RelatedCity")]
        [InlineData("HomeAddress/RelatedCity/$ref")]
        [InlineData("HomeAddress/Cities")]
        [InlineData("HomeAddress/Cities($top=2)")]
        [InlineData("HomeAddress/Cities/$ref")]
        [InlineData("Addresses/RelatedCity")]
        [InlineData("Addresses/RelatedCity/$ref")]
        [InlineData("Addresses/Cities")]
        [InlineData("Addresses/Cities($count=true)")]
        [InlineData("Addresses/Cities/$ref")]
        [InlineData("HomeAddress/Info/InfoCity")]
        [InlineData("Addresses/Info/InfoCity")]
        [InlineData("HomeAddress/Microsoft.AspNet.OData.Test.Query.Expressions.QueryUsAddress/UsCity")]
        [InlineData("HomeAddress/Microsoft.AspNet.OData.Test.Query.Expressions.QueryUsAddress/UsCities")]
        [InlineData("HomeAddress/Microsoft.AspNet.OData.Test.Query.Expressions.QueryUsAddress/UsCities($select=CityName)")]
        [InlineData("HomeAddress/Microsoft.AspNet.OData.Test.Query.Expressions.QueryUsAddress/UsCities/$ref")]
        [InlineData("Addresses/Microsoft.AspNet.OData.Test.Query.Expressions.QueryCnAddress/CnCity")]
        [InlineData("Addresses/Microsoft.AspNet.OData.Test.Query.Expressions.QueryCnAddress/CnCities")]
        [InlineData("Microsoft.AspNet.OData.Test.Query.Expressions.QueryVipCustomer/VipAddress/Microsoft.AspNet.OData.Test.Query.Expressions.QueryCnAddress/CnCities")]
        [InlineData("Microsoft.AspNet.OData.Test.Query.Expressions.QueryVipCustomer/VipAddresses/Microsoft.AspNet.OData.Test.Query.Expressions.QueryUsAddress/UsCities")]
        public void GetSelectExpandProperties_ForNonDirectNavigationProperty_ReturnsCorrectExpandedProperties(string expand)
        {
            // Arrange
            SelectExpandClause selectExpandClause = ParseSelectExpand(null, expand, _model, _customer, _customers);
            Assert.NotNull(selectExpandClause);

            // Act
            IDictionary<IEdmStructuralProperty, PathSelectItem> propertiesToInclude;
            IDictionary<IEdmNavigationProperty, ExpandedReferenceSelectItem> propertiesToExpand;
            ISet<IEdmStructuralProperty> autoSelectedProperties;
            bool isContainDynamicProperty = SelectExpandBinder.GetSelectExpandProperties(_model, _customer, _customers, selectExpandClause,
                out propertiesToInclude,
                out propertiesToExpand,
                out autoSelectedProperties);

            // Assert
            var selectItem = Assert.Single(selectExpandClause.SelectedItems);
            ExpandedReferenceSelectItem expandedItem = selectItem as ExpandedReferenceSelectItem;
            Assert.NotNull(expandedItem);
            var propertySegment = expandedItem.PathToNavigationProperty.First(p => p is PropertySegment) as PropertySegment;

            Assert.Null(propertiesToExpand); // nothing to expand at current level

            Assert.NotEmpty(propertiesToInclude);
            var propertyToInclude = Assert.Single(propertiesToInclude);

            Assert.Same(propertySegment.Property, propertyToInclude.Key);
            Assert.NotNull(propertyToInclude.Value);

            PathSelectItem pathItem = propertyToInclude.Value;
            Assert.NotNull(pathItem);
            Assert.NotNull(pathItem.SelectAndExpand);
            Assert.True(pathItem.SelectAndExpand.AllSelected);
            var nextLevelSelectItem = Assert.Single(pathItem.SelectAndExpand.SelectedItems);
            var nextLevelExpandedItem = nextLevelSelectItem as ExpandedReferenceSelectItem;
            Assert.NotNull(nextLevelExpandedItem);
        }

        [Fact]
        public void GetSelectExpandProperties_FoSelectAndExpand_ReturnsCorrectExpandedProperties()
        {
            // Arrange
            string select = "HomeAddress($select=Street),Addresses/Codes($top=2)";
            string expand = "HomeAddress/RelatedCity/$ref,HomeAddress/Cities($count=true),PrivateOrder";
            SelectExpandClause selectExpandClause = ParseSelectExpand(select, expand, _model, _customer, _customers);
            Assert.NotNull(selectExpandClause);

            // Act
            IDictionary<IEdmStructuralProperty, PathSelectItem> propertiesToInclude;
            IDictionary<IEdmNavigationProperty, ExpandedReferenceSelectItem> propertiesToExpand;
            ISet<IEdmStructuralProperty> autoSelectedProperties;
            bool isContainDynamicProperty = SelectExpandBinder.GetSelectExpandProperties(_model, _customer, _customers, selectExpandClause,
                out propertiesToInclude,
                out propertiesToExpand,
                out autoSelectedProperties);

            // Arrange
            Assert.False(isContainDynamicProperty);

            Assert.NotNull(selectExpandClause);
            Assert.False(selectExpandClause.AllSelected);

            // Why it's 6, because ODL includes "HomeAddress" as Selected automatic when parsing $expand=HomeAddress/Nav
            // It's an issue reported at: https://github.com/OData/odata.net/issues/1574
            Assert.Equal(6, selectExpandClause.SelectedItems.Count());

            Assert.NotNull(propertiesToInclude);
            Assert.Equal(2, propertiesToInclude.Count);
            Assert.Equal(new[] { "HomeAddress", "Addresses" }, propertiesToInclude.Keys.Select(e => e.Name));

            Assert.NotNull(propertiesToExpand);
            var propertyToExpand = Assert.Single(propertiesToExpand);
            Assert.Equal("PrivateOrder", propertyToExpand.Key.Name);

            Assert.NotNull(autoSelectedProperties);
            Assert.Equal("Id", Assert.Single(autoSelectedProperties).Name);
        }

        #endregion

        #region CreatePropertyNameExpression Tests
        [Fact]
        public void CreatePropertyNameExpression_ReturnsCorrectExpression()
        {
            // Arrange
            // Retrieve base info
            IEdmProperty baseProperty = _customer.FindProperty("PrivateOrder");
            Assert.NotNull(baseProperty); // Guard

            // Retrieve derived info
            IEdmEntityType vipCustomer = _model.SchemaElements.OfType<IEdmEntityType>().FirstOrDefault(c => c.Name == "QueryVipCustomer");
            Assert.NotNull(vipCustomer); // Guard
            IEdmProperty derivedProperty = vipCustomer.FindProperty("Birthday");
            Assert.NotNull(derivedProperty); // Guard

            Expression source = Expression.Parameter(typeof(QueryCustomer), "aCustomer");
            SelectExpandBinder binder = GetBinder<QueryCustomer>(_model);

            // Act & Assert
            // #1. Base property on base type
            Expression property = binder.CreatePropertyNameExpression(_customer, baseProperty, source);
            Assert.Equal(ExpressionType.Constant, property.NodeType);
            Assert.Equal(typeof(string), property.Type);
            Assert.Equal("PrivateOrder", (property as ConstantExpression).Value);

            // #2. Base property on derived type
            property = binder.CreatePropertyNameExpression(vipCustomer, baseProperty, source);
            Assert.Equal(ExpressionType.Constant, property.NodeType);
            Assert.Equal(typeof(string), property.Type);
            Assert.Equal("PrivateOrder", (property as ConstantExpression).Value);

            // #3. Derived property on base type
            property = binder.CreatePropertyNameExpression(_customer, derivedProperty, source);
            Assert.Equal(ExpressionType.Conditional, property.NodeType);
            Assert.Equal(typeof(string), property.Type);
            Assert.Equal("IIF((aCustomer Is QueryVipCustomer), \"Birthday\", null)", property.ToString());

            // #4. Derived property on derived type.
            property = binder.CreatePropertyNameExpression(vipCustomer, derivedProperty, source);
            Assert.Equal(ExpressionType.Constant, property.NodeType);
            Assert.Equal(typeof(string), property.Type);
            Assert.Equal("Birthday", (property as ConstantExpression).Value);
        }

        [Fact(Skip = "if (!castType.IsAssignableFrom(originalType)) retrun true?")]
        public void CreatePropertyNameExpression_ReturnsConstantExpression_IfPropertyTypeCannotAssignedToElementType()
        {
            // Arrange
            IEdmComplexType complexType = _model.SchemaElements.OfType<IEdmComplexType>().First(c => c.Name == "QueryAddress");
            Assert.False(complexType.IsOrInheritsFrom(_customer)); // make sure order has no inheritance-ship with customer.

            IEdmProperty edmProperty = complexType.FindProperty("Street");
            Assert.NotNull(edmProperty);

            Expression source = Expression.Parameter(typeof(QueryCustomer), "aCustomer");

            // Act
            Expression property = _binder.CreatePropertyNameExpression(_customer, edmProperty, source);

            // Assert
            Assert.Equal(ExpressionType.Constant, property.NodeType);
            Assert.Equal(typeof(string), property.Type);
            Assert.Equal("Street", (property as ConstantExpression).Value);
        }

        [Fact]
        public void CreatePropertyNameExpression_ThrowsODataException_IfMappingTypeIsNotFoundInModel()
        {
            // Arrange
            EdmModel model = _model as EdmModel;

            // Create a "SubCustomer" derived from "Customer", but without the CLR type in the Edm model.
            EdmEntityType subCustomer = new EdmEntityType("NS", "SubCustomer", _customer);
            EdmStructuralProperty subNameProperty = subCustomer.AddStructuralProperty("SubName", EdmPrimitiveTypeKind.String);
            model.AddElement(subCustomer);

            Expression source = Expression.Constant(new QueryCustomer());
            SelectExpandBinder binder = GetBinder<QueryCustomer>(model);

            // Act & Assert
            ExceptionAssert.Throws<ODataException>(() => binder.CreatePropertyNameExpression(_customer, subNameProperty, source),
                "The provided mapping does not contain a resource for the resource type 'NS.SubCustomer'.");
        }
        #endregion

        #region CreatePropertyValueExpression
        [Theory]
        [InlineData("PrivateOrder")]
        [InlineData("Orders")]
        public void CreatePropertyValueExpression_NonDerivedNavigationProperty_ReturnsMemberAccessExpression(string property)
        {
            // Arrange
            Expression source = Expression.Constant(new QueryCustomer());

            IEdmNavigationProperty navProperty = _customer.NavigationProperties().Single(c => c.Name == property);
            Assert.NotNull(navProperty);

            // Act
            Expression propertyValue = _binder.CreatePropertyValueExpression(_customer, navProperty, source, null);

            // Assert
            Assert.Equal(ExpressionType.MemberAccess, propertyValue.NodeType);
            Assert.Equal(typeof(QueryCustomer).GetProperty(property), (propertyValue as MemberExpression).Member);
            Assert.Equal(String.Format("{0}.{1}", source.ToString(), property), propertyValue.ToString());
        }

        [Theory]
        [InlineData("SpecialOrder")]
        [InlineData("SpecialOrders")]
        public void CreatePropertyValueExpression_DerivedNavigationProperty_ReturnsPropertyAccessExpression(string property)
        {
            // Arrange
            Expression source = Expression.Constant(new QueryCustomer());

            IEdmStructuredType vipCustomer = _model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "QueryVipCustomer");
            Assert.NotNull(vipCustomer);

            IEdmNavigationProperty specialProperty = vipCustomer.DeclaredNavigationProperties().First(c => c.Name == property);
            Assert.NotNull(specialProperty);

            // Act
            Expression propertyValue = _binder.CreatePropertyValueExpression(_customer, specialProperty, source, null);

            // Assert
            Assert.Equal(String.Format("({0} As QueryVipCustomer).{1}", source.ToString(), property), propertyValue.ToString());
        }

        [Theory]
        [InlineData("Level")]
        [InlineData("Birthday")]
        [InlineData("Bonus")]
        public void CreatePropertyValueExpression_DerivedValueProperty_ReturnsPropertyAccessExpression(string property)
        {
            // Arrange
            Expression source = Expression.Constant(new QueryCustomer());

            IEdmStructuredType vipCustomer = _model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "QueryVipCustomer");
            Assert.NotNull(vipCustomer);

            IEdmStructuralProperty edmProperty = vipCustomer.DeclaredStructuralProperties().First(c => c.Name == property);
            Assert.NotNull(vipCustomer);

            // Act
            Expression propertyValue = _binder.CreatePropertyValueExpression(_customer, edmProperty, source, null);

            // Assert
#if NETCORE
            Assert.Equal(String.Format("Convert(({0} As QueryVipCustomer).{1}, Nullable`1)", source.ToString(), property), propertyValue.ToString());
#else
            Assert.Equal(String.Format("Convert(({0} As QueryVipCustomer).{1})", source.ToString(), property), propertyValue.ToString());
#endif
        }

        [Theory]
        [InlineData("Taxes")]
        [InlineData("VipAddress")]
        [InlineData("VipAddresses")]
        public void CreatePropertyValueExpression_DerivedReferenceProperty_ReturnsPropertyAccessExpression(string property)
        {
            // Arrange
            Expression source = Expression.Constant(new QueryCustomer());

            IEdmStructuredType vipCustomer = _model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "QueryVipCustomer");
            Assert.NotNull(vipCustomer);

            IEdmStructuralProperty edmProperty = vipCustomer.DeclaredStructuralProperties().First(c => c.Name == property);
            Assert.NotNull(vipCustomer);

            // Act
            Expression propertyValue = _binder.CreatePropertyValueExpression(_customer, edmProperty, source, null);

            // Assert
            Assert.Equal(String.Format("({0} As QueryVipCustomer).{1}", source.ToString(), property), propertyValue.ToString());
        }

        [Fact]
        public void CreatePropertyValueExpression_HandleNullPropagationTrue_AddsNullCheck()
        {
            // Arrange
            _settings.HandleNullPropagation = HandleNullPropagationOption.True;
            Expression source = Expression.Constant(new QueryCustomer());
            IEdmProperty idProperty = _customer.StructuralProperties().Single(p => p.Name == "Id");

            // Act
            Expression property = _binder.CreatePropertyValueExpression(_customer, idProperty, source, null);

            // Assert
            // NetFx and NetCore differ in the way Expression is converted to a string.
            Assert.Equal(ExpressionType.Conditional, property.NodeType);
            ConditionalExpression conditionalExpression = property as ConditionalExpression;
            Assert.NotNull(conditionalExpression);
            Assert.Equal(typeof(int?), conditionalExpression.Type);

            Assert.Equal(ExpressionType.Convert, conditionalExpression.IfFalse.NodeType);
            UnaryExpression falseUnaryExpression = conditionalExpression.IfFalse as UnaryExpression;
            Assert.NotNull(falseUnaryExpression);
            Assert.Equal(String.Format("{0}.Id", source.ToString()), falseUnaryExpression.Operand.ToString());
            Assert.Equal(typeof(int?), falseUnaryExpression.Type);

            Assert.Equal(ExpressionType.Constant, conditionalExpression.IfTrue.NodeType);
            ConstantExpression trueUnaryExpression = conditionalExpression.IfTrue as ConstantExpression;
            Assert.NotNull(trueUnaryExpression);
            Assert.Equal("null", trueUnaryExpression.ToString());

            Assert.Equal(ExpressionType.Equal, conditionalExpression.Test.NodeType);
            BinaryExpression binaryExpression = conditionalExpression.Test as BinaryExpression;
            Assert.NotNull(binaryExpression);
            Assert.Equal(source.ToString(), binaryExpression.Left.ToString());
            Assert.Equal("null", binaryExpression.Right.ToString());
            Assert.Equal(typeof(bool), binaryExpression.Type);
        }

        [Fact]
        public void CreatePropertyValueExpression_HandleNullPropagationFalse_ConvertsToNullableType()
        {
            // Arrange
            _settings.HandleNullPropagation = HandleNullPropagationOption.False;
            Expression source = Expression.Constant(new QueryCustomer());
            IEdmProperty idProperty = _customer.StructuralProperties().Single(p => p.Name == "Id");

            // Act
            Expression property = _binder.CreatePropertyValueExpression(_customer, idProperty, source, filterClause: null);

            // Assert
#if NETCORE
            Assert.Equal(String.Format("Convert({0}.Id, Nullable`1)", source.ToString()), property.ToString());
#else
            Assert.Equal(String.Format("Convert({0}.Id)", source.ToString()), property.ToString());
#endif
            Assert.Equal(typeof(int?), property.Type);
        }

        [Fact]
        public void CreatePropertyValueExpression_Collection_ThrowsODataException_IfMappingTypeIsNotFoundInModel()
        {
            // Arrange
            _model.SetAnnotationValue(_order, new ClrTypeAnnotation(null));

            var source = Expression.Constant(new QueryCustomer
            {
                Orders = new[]
                {
                    new QueryOrder { Id = 1 },
                    new QueryOrder { Id = 2 }
                }
            });

            var ordersProperty = _customer.NavigationProperties().Single(p => p.Name == "Orders");

            SelectExpandClause selectExpand = ParseSelectExpand(null, "Orders($filter=Id eq 1)", _model, _customer, _customers);
            ExpandedNavigationSelectItem expandItem = Assert.Single(selectExpand.SelectedItems) as ExpandedNavigationSelectItem;
            Assert.NotNull(expandItem);
            Assert.NotNull(expandItem.FilterOption);

            // Act & Assert
            ExceptionAssert.Throws<ODataException>(
                () => _binder.CreatePropertyValueExpression(_customer, ordersProperty, source, expandItem.FilterOption),
                String.Format("The provided mapping does not contain a resource for the resource type '{0}'.",
                ordersProperty.Type.Definition.AsElementType().FullTypeName()));
        }

        [Theory]
        [InlineData(HandleNullPropagationOption.True)]
        [InlineData(HandleNullPropagationOption.False)]
        public void CreatePropertyValueExpression_Collection_Works_HandleNullPropagationOption(HandleNullPropagationOption nullOption)
        {
            // Arrange
            _settings.HandleNullPropagation = nullOption;
            var source = Expression.Constant(new QueryCustomer
            {
                Orders = new[]
                {
                    new QueryOrder { Id = 1 },
                    new QueryOrder { Id = 2 }
                }
            });

            var ordersProperty = _customer.NavigationProperties().Single(p => p.Name == "Orders");

            SelectExpandClause selectExpand = ParseSelectExpand(null, "Orders($filter=Id eq 1)", _model, _customer, _customers);
            ExpandedNavigationSelectItem expandItem = Assert.Single(selectExpand.SelectedItems) as ExpandedNavigationSelectItem;
            Assert.NotNull(expandItem);
            Assert.NotNull(expandItem.FilterOption);

            // Act
            var filterInExpand = _binder.CreatePropertyValueExpression(_customer, ordersProperty, source, expandItem.FilterOption);

            // Assert
            if (nullOption == HandleNullPropagationOption.True)
            {
                Assert.Equal(
                    string.Format(
                        "IIF((value({0}) == null), null, IIF((value({0}).Orders == null), null, " +
                        "value({0}).Orders.Where($it => ($it.Id == value({1}).TypedProperty))))",
                        source.Type,
                        "Microsoft.AspNet.OData.Query.Expressions.LinqParameterContainer+TypedLinqParameterContainer`1[System.Int32]"),
                    filterInExpand.ToString());
            }
            else
            {
                Assert.Equal(
                    string.Format(
                        "value({0}).Orders.Where($it => ($it.Id == value(" +
                        "Microsoft.AspNet.OData.Query.Expressions.LinqParameterContainer+TypedLinqParameterContainer`1[System.Int32]).TypedProperty))",
                        source.Type),
                    filterInExpand.ToString());
            }

            var orders = Expression.Lambda(filterInExpand).Compile().DynamicInvoke() as IEnumerable<QueryOrder>;
            QueryOrder order = Assert.Single(orders);
            Assert.Equal(1, order.Id);
        }

        [Fact]
        public void CreatePropertyValueExpression_Single_ThrowsODataException_IfMappingTypeIsNotFoundInModel()
        {
            // Arrange
            _model.SetAnnotationValue(_customer, new ClrTypeAnnotation(null));

            _settings.HandleReferenceNavigationPropertyExpandFilter = true;
            var source = Expression.Constant(new QueryOrder());
            var customerProperty = _order.NavigationProperties().Single(p => p.Name == "Customer");

            SelectExpandClause selectExpand = ParseSelectExpand(null, "Customer($filter=Id ne 1)", _model, _order, _orders);
            ExpandedNavigationSelectItem expandItem = Assert.Single(selectExpand.SelectedItems) as ExpandedNavigationSelectItem;
            Assert.NotNull(expandItem);
            Assert.NotNull(expandItem.FilterOption);

            // Act & Assert
            ExceptionAssert.Throws<ODataException>(
                () => _binder.CreatePropertyValueExpression(_order, customerProperty, source, expandItem.FilterOption),
                String.Format("The provided mapping does not contain a resource for the resource type '{0}'.", customerProperty.Type.FullName()));
        }

        [Fact]
        public void CreatePropertyValueExpression_Single_Works_IfSettingIsOff()
        {
            // Arrange
            _settings.HandleReferenceNavigationPropertyExpandFilter = false;
            var order = Expression.Constant(
                    new QueryOrder
                    {
                        Customer = new QueryCustomer
                        {
                            Id = 1
                        }
                    }
            );
            var customerProperty = _order.NavigationProperties().Single(p => p.Name == "Customer");

            SelectExpandClause selectExpand = ParseSelectExpand(null, "Customer($filter=Id ne 1)", _model, _order, _orders);
            Assert.True(selectExpand.AllSelected); // Guard
            ExpandedNavigationSelectItem expandItem = Assert.Single(selectExpand.SelectedItems) as ExpandedNavigationSelectItem;
            Assert.NotNull(expandItem);
            Assert.NotNull(expandItem.FilterOption);

            // Act 
            var filterInExpand = _binder.CreatePropertyValueExpression(_order, customerProperty, order, expandItem.FilterOption);

            // Assert
            var customer = Expression.Lambda(filterInExpand).Compile().DynamicInvoke() as QueryCustomer;
            Assert.NotNull(customer);
            Assert.Equal(1, customer.Id);
        }

        [Theory]
        [InlineData(HandleNullPropagationOption.True)]
        [InlineData(HandleNullPropagationOption.False)]
        public void CreatePropertyValueExpression_Single_Works_HandleNullPropagationOption(HandleNullPropagationOption nullOption)
        {
            // Arrange
            _settings.HandleReferenceNavigationPropertyExpandFilter = true;
            _settings.HandleNullPropagation = nullOption;
            var source = Expression.Constant(
                    new QueryOrder
                    {
                        Customer = new QueryCustomer
                        {
                            Id = 1
                        }
                    }
            );
            var customerProperty = _order.NavigationProperties().Single(p => p.Name == "Customer");

            SelectExpandClause selectExpand = ParseSelectExpand(null, "Customer($filter=Id ne 1)", _model, _order, _orders);
            Assert.True(selectExpand.AllSelected); // Guard
            ExpandedNavigationSelectItem expandItem = Assert.Single(selectExpand.SelectedItems) as ExpandedNavigationSelectItem;
            Assert.NotNull(expandItem);
            Assert.NotNull(expandItem.FilterOption);

            // Act
            var filterInExpand = _binder.CreatePropertyValueExpression(_order, customerProperty, source, expandItem.FilterOption);

            // Assert
            if (nullOption == HandleNullPropagationOption.True)
            {
                Assert.Equal(
                    string.Format(
                        "IIF((value({0}) == null), null, IIF((value({0}).Customer == null), null, " +
                        "IIF((value({0}).Customer.Id != value(Microsoft.AspNet.OData.Query.Expressions.LinqParameterContainer+TypedLinqParameterContainer`1[System.Int32]).TypedProperty), " +
                        "value({0}).Customer, null)))",
                        source.Type),
                    filterInExpand.ToString());
            }
            else
            {
                Assert.Equal(
                    string.Format(
                        "IIF((value({0}).Customer.Id != value(Microsoft.AspNet.OData.Query.Expressions.LinqParameterContainer+TypedLinqParameterContainer`1[System.Int32]).TypedProperty), " +
                        "value({0}).Customer, null)",
                        source.Type),
                    filterInExpand.ToString());
            }

            var customer = Expression.Lambda(filterInExpand).Compile().DynamicInvoke() as QueryCustomer;
            Assert.Null(customer);
        }
        #endregion

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
            ExceptionAssert.Throws<ODataException>(
                () => SelectExpandBinder.CreateTypeNameExpression(source, baseType, model),
                "The provided mapping does not contain a resource for the resource type 'NS.DerivedType'.");
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
                @"IIF((42 Is AAA), ""NS.AAA"", IIF((42 Is AA), ""NS.AA"", IIF((42 Is B), ""NS.B"", IIF((42 Is A), ""NS.A"", ""NS.BaseType""))))",
                result.ToString());
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CorrelatedSubqueryIncludesToListIfBufferingOptimizationIsTrue(bool enableOptimization)
        {
            // Arrange
            _settings.EnableCorrelatedSubqueryBuffering = enableOptimization;
            var source = Expression.Constant(new QueryCustomer
            {
                Orders = new[]
                {
                    new QueryOrder { Id = 1 },
                    new QueryOrder { Id = 2 }
                }
            });

            var expandClause = ParseSelectExpand(null, "Orders", _model, _customer, _customers);

            // Act
            var expand = _binder.ProjectAsWrapper(source, expandClause, _customer, _customers);

            // Assert
            Assert.True(expand.ToString().Contains("ToList") == enableOptimization);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CorrelatedSubqueryIncludesToListIfBufferingOptimizationIsTrueAndPagesizeIsSet(bool enableOptimization)
        {
            // Arrange
            _settings.EnableCorrelatedSubqueryBuffering = enableOptimization;
            _settings.PageSize = 100;
            var source = Expression.Constant(new QueryCustomer
            {
                Orders = new[]
                {
                    new QueryOrder { Id = 1 },
                    new QueryOrder { Id = 2 }
                }
            });

            var expandClause = ParseSelectExpand(null, "Orders", _model, _customer, _customers);

            // Act
            var expand = _binder.ProjectAsWrapper(source, expandClause, _customer, _customers);

            // Assert
            Assert.True(expand.ToString().Contains("ToList") == enableOptimization);
        }

        public static IEdmModel GetEdmModel()
        {
            var builder = new ODataConventionModelBuilder();
            var customer = builder.EntitySet<QueryCustomer>("Customers").EntityType;
            builder.EntitySet<QueryOrder>("Orders");
            builder.EntitySet<QueryCity>("Cities");

            customer.Collection.Function("IsUpgraded").Returns<bool>().Namespace="NS";
            customer.Collection.Action("UpgradeAll").Namespace = "NS";
            return builder.GetEdmModel();
        }

        public static SelectExpandClause ParseSelectExpand(string select, string expand, IEdmModel model, IEdmType edmType, IEdmNavigationSource navigationSource)
        {
            return new ODataQueryOptionParser(model, edmType, navigationSource,
                new Dictionary<string, string>
                {
                    { "$expand", expand == null ? "" : expand },
                    { "$select", select == null ? "" : select }
                }).ParseSelectAndExpand();
        }
    }

    public class QueryCity
    {
        public int Id { get; set; }

        public int CityName { get; set; }
    }


    public class QueryAddressInfo
    {
        public QueryCity InfoCity { get; set; }
    }

    public class QueryAddress
    {
        public string Street { get; set; }

        public string Region { get; set; }

        public IList<string> Codes { get; set; }

        public QueryAddressInfo Info { get; set; }

        public IList<int> Prices { get; set; }

        public QueryCity RelatedCity { get; set; }

        [ConcurrencyCheck] // ETag on property of complex is not fully supported.
        public double AddressETag { get; set; }

        public IList<QueryCity> Cities { get; set; }

        public IDictionary<string, object> AddressDynaicProperties { get; set; }
    }

    public class QueryUsAddress : QueryAddress
    {
        public string ZipCode { get; set; }

        public QueryCity UsCity { get; set; }

        public IList<QueryCity> UsCities { get; set; }
    }

    public class QueryCnAddress : QueryAddress
    {
        public string PostCode { get; set; }

        public QueryCity CnCity { get; set; }

        public IList<QueryCity> CnCities { get; set; }
    }

    public class QueryOrder
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public QueryCustomer Customer { get; set; }

        public IDictionary<string, object> OrderProperties { get; set; }
    }

    public class QueryCustomer
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int Age { get; set; }

        public IList<string> Emails { get; set; }

        // [ConcurrencyCheck]
        public double CustomerETag { get; set; }

        public QueryColor FarivateColor { get; set; }

        public QueryAddress HomeAddress { get; set; }

        public IList<QueryAddress> Addresses { get; set; }

        public QueryOrder PrivateOrder { get; set; }

        public IList<QueryOrder> Orders { get; set; }

        public IDictionary<string, object> CustomerProperties { get; set; }
    }

    public class QueryVipCustomer : QueryCustomer
    {
        public int Level { get; set; }

        public DateTimeOffset Birthday { get; set; }

        public IList<int> Taxes { get; set; }

        public decimal Bonus { get; set; }

        public QueryAddress VipAddress { get; set; }

        public QueryAddress[] VipAddresses { get; set; }

        public QueryVipOrder SpecialOrder { get; set; }

        public QueryVipOrder[] SpecialOrders { get; set; }
    }

    public class QueryVipOrder : QueryOrder
    {
        public QueryVipCustomer[] SpecialCustomers { get; set; }
    }

    public enum QueryColor
    {
        Red,

        Green,

        Blue
    }
}
