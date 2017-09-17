﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web.Http.Dispatcher;
using System.Web.OData.Builder;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Microsoft.OData.UriParser.Aggregation;
using Microsoft.TestCommon;

namespace System.Web.OData.Query.Expressions
{
    public class AggregationBinderTests
    {
        private static readonly Uri _serviceBaseUri = new Uri("http://server/service/");

        private static Dictionary<Type, IEdmModel> _modelCache = new Dictionary<Type, IEdmModel>();

        [Fact]
        public void SingleGroupBy()
        {
            var filters = VerifyQueryDeserialization(
                "groupby((ProductName))",
                ".GroupBy($it => new GroupByWrapper() {GroupByContainer = new LastInChain() {Name = ProductName, Value = Convert($it.ProductName), }, })"
                + ".Select($it => new AggregationWrapper() {GroupByContainer = $it.Key.GroupByContainer, })");
        }

        [Fact]
        public void MultipleGroupBy()
        {
            var filters = VerifyQueryDeserialization(
                "groupby((ProductName, SupplierID))",
                ".GroupBy($it => new GroupByWrapper() {GroupByContainer = new AggregationPropertyContainer() {Name = SupplierID, Value = Convert($it.SupplierID), Next = new LastInChain() {Name = ProductName, Value = Convert($it.ProductName), }, }, })"
                + ".Select($it => new AggregationWrapper() {GroupByContainer = $it.Key.GroupByContainer, })");
        }

        [Fact]
        public void NavigationGroupBy()
        {
            var filters = VerifyQueryDeserialization(
                "groupby((Category/CategoryName))",
                ".GroupBy($it => new GroupByWrapper() {GroupByContainer = new NestedPropertyLastInChain() {Name = Category, NestedValue = new GroupByWrapper() {GroupByContainer = new LastInChain() {Name = CategoryName, Value = Convert($it.Category.CategoryName), }, }, }, })"
                + ".Select($it => new AggregationWrapper() {GroupByContainer = $it.Key.GroupByContainer, })");
        }

        [Fact]
        public void NavigationMultipleGroupBy()
        {
            var filters = VerifyQueryDeserialization(
                "groupby((Category/CategoryName, SupplierAddress/State))",
                ".GroupBy($it => new GroupByWrapper() {GroupByContainer = new NestedProperty() {Name = SupplierAddress, NestedValue = new GroupByWrapper() {GroupByContainer = new LastInChain() {Name = State, Value = Convert($it.SupplierAddress.State), }, }, Next = new NestedPropertyLastInChain() {Name = Category, NestedValue = new GroupByWrapper() {GroupByContainer = new LastInChain() {Name = CategoryName, Value = Convert($it.Category.CategoryName), }, }, }, }, })"
                + ".Select($it => new AggregationWrapper() {GroupByContainer = $it.Key.GroupByContainer, })");
        }

        [Fact]
        public void SingleDynamicGroupBy()
        {
            var filters = VerifyQueryDeserialization<DynamicProduct>(
                "groupby((ProductProperty))",
                ".GroupBy($it => new GroupByWrapper() {GroupByContainer = new LastInChain() {Name = ProductProperty, Value = Convert(IIF($it.ProductProperties.ContainsKey(ProductProperty), $it.ProductPropertiesProductProperty, null)), }, })"
                + ".Select($it => new AggregationWrapper() {GroupByContainer = $it.Key.GroupByContainer, })");
        }


        [Fact]
        public void SingleSum()
        {
            var filters = VerifyQueryDeserialization(
                "aggregate(SupplierID with sum as SupplierID)",
                ".GroupBy($it => new NoGroupByWrapper())"
                + ".Select($it => new NoGroupByAggregationWrapper() {Container = new LastInChain() {Name = SupplierID, Value = Convert($it.AsQueryable().Sum($it => $it.SupplierID)), }, })");
        }

        [Fact]
        public void SingleDynamicSum()
        {
            var filters = VerifyQueryDeserialization<DynamicProduct>(
                "aggregate(ProductProperty with sum as ProductProperty)",
                ".GroupBy($it => new NoGroupByWrapper())"
                + ".Select($it => new NoGroupByAggregationWrapper() {Container = new LastInChain() {Name = ProductProperty, Value = Convert(Convert($it.AsQueryable().Sum($it => IIF($it.ProductProperties.ContainsKey(ProductProperty), $it.ProductPropertiesProductProperty, null).SafeConvertToDecimal()))), }, })");
        }

        [Fact]
        public void SingleMin()
        {
            var filters = VerifyQueryDeserialization(
                "aggregate(SupplierID with min as SupplierID)",
                ".GroupBy($it => new NoGroupByWrapper())"
                + ".Select($it => new NoGroupByAggregationWrapper() {Container = new LastInChain() {Name = SupplierID, Value = Convert($it.AsQueryable().Min($it => $it.SupplierID)), }, })");
        }

        [Fact]
        public void SingleDynamicMin()
        {
            var filters = VerifyQueryDeserialization<DynamicProduct>(
                "aggregate(ProductProperty with min as MinProductProperty)",
                ".GroupBy($it => new NoGroupByWrapper())"
                + ".Select($it => new NoGroupByAggregationWrapper() {Container = new LastInChain() {Name = MinProductProperty, Value = Convert($it.AsQueryable().Min($it => IIF($it.ProductProperties.ContainsKey(ProductProperty), $it.ProductPropertiesProductProperty, null))), }, })");
        }

        [Fact]
        public void SingleMax()
        {
            var filters = VerifyQueryDeserialization(
                "aggregate(SupplierID with max as SupplierID)",
                ".GroupBy($it => new NoGroupByWrapper())"
                + ".Select($it => new NoGroupByAggregationWrapper() {Container = new LastInChain() {Name = SupplierID, Value = Convert($it.AsQueryable().Max($it => $it.SupplierID)), }, })");
        }

        [Fact]
        public void SingleAverage()
        {
            var filters = VerifyQueryDeserialization(
                "aggregate(UnitPrice with average as AvgUnitPrice)",
                ".GroupBy($it => new NoGroupByWrapper())"
                + ".Select($it => new NoGroupByAggregationWrapper() {Container = new LastInChain() {Name = AvgUnitPrice, Value = Convert($it.AsQueryable().Average($it => $it.UnitPrice)), }, })");
        }

        [Fact]
        public void SingleCountDistinct()
        {
            var filters = VerifyQueryDeserialization(
                "aggregate(SupplierID with countdistinct as Count)",
                ".GroupBy($it => new NoGroupByWrapper())"
                + ".Select($it => new NoGroupByAggregationWrapper() {Container = new LastInChain() {Name = Count, Value = Convert($it.AsQueryable().Select($it => $it.SupplierID).Distinct().LongCount()), }, })");
        }

        [Fact]
        public void MultipleAggregate()
        {
            var filters = VerifyQueryDeserialization(
                "aggregate(SupplierID with sum as SupplierID, CategoryID with sum as CategoryID)",
                ".GroupBy($it => new NoGroupByWrapper())"
                + ".Select($it => new NoGroupByAggregationWrapper() {Container = new AggregationPropertyContainer() {Name = CategoryID, Value = Convert($it.AsQueryable().Sum($it => $it.CategoryID)), Next = new LastInChain() {Name = SupplierID, Value = Convert($it.AsQueryable().Sum($it => $it.SupplierID)), }, }, })");
        }

        [Fact]
        public void GroupByAndAggregate()
        {
            var filters = VerifyQueryDeserialization(
                "groupby((ProductName), aggregate(SupplierID with sum as SupplierID))",
                ".GroupBy($it => new GroupByWrapper() {GroupByContainer = new LastInChain() {Name = ProductName, Value = Convert($it.ProductName), }, })"
                + ".Select($it => new AggregationWrapper() {GroupByContainer = $it.Key.GroupByContainer, Container = new LastInChain() {Name = SupplierID, Value = Convert($it.AsQueryable().Sum($it => $it.SupplierID)), }, })");
        }

        private Expression VerifyQueryDeserialization(string filter, string expectedResult = null, Action<ODataQuerySettings> settingsCustomizer = null)
        {
            return VerifyQueryDeserialization<Product>(filter, expectedResult, settingsCustomizer);
        }

        private Expression VerifyQueryDeserialization<T>(string clauseString, string expectedResult = null, Action<ODataQuerySettings> settingsCustomizer = null) where T : class
        {
            IEdmModel model = GetModel<T>();
            ApplyClause clause = CreateApplyNode(clauseString, model, typeof(T));
            IAssembliesResolver assembliesResolver = CreateFakeAssembliesResolver();

            Func<ODataQuerySettings, ODataQuerySettings> customizeSettings = (settings) =>
            {
                if (settingsCustomizer != null)
                {
                    settingsCustomizer.Invoke(settings);
                }

                return settings;
            };

            var binder = new AggregationBinder(
                customizeSettings(new ODataQuerySettings { HandleNullPropagation = HandleNullPropagationOption.False }),
                assembliesResolver,
                typeof(T),
                model,
                clause.Transformations.First());

            var query = Enumerable.Empty<T>().AsQueryable();

            var queryResult = binder.Bind(query);

            var applyExpr = queryResult.Expression;

            VerifyExpression<T>(applyExpr, expectedResult);

            return applyExpr;
        }

        private void VerifyExpression<T>(Expression clause, string expectedExpression)
        {
            // strip off the beginning part of the expression to get to the first
            // actual query operator
            string resultExpression = ExpressionStringBuilder.ToString(clause);
            var replace = typeof(T).FullName + "[]";
            resultExpression = resultExpression.Replace(replace, string.Empty);
            Assert.True(resultExpression == expectedExpression,
                String.Format("Expected expression '{0}' but the deserializer produced '{1}'", expectedExpression, resultExpression));
        }

        private ApplyClause CreateApplyNode(string clause, IEdmModel model, Type entityType)
        {
            IEdmEntityType productType = model.SchemaElements.OfType<IEdmEntityType>().Single(t => t.Name == entityType.Name);
            Assert.NotNull(productType); // Guard

            IEdmEntitySet products = model.EntityContainer.FindEntitySet("Products");
            Assert.NotNull(products); // Guard

            ODataQueryOptionParser parser = new ODataQueryOptionParser(model, productType, products,
                new Dictionary<string, string> { { "$apply", clause } });

            return parser.ParseApply();
        }

        private IAssembliesResolver CreateFakeAssembliesResolver()
        {
            return new NoAssembliesResolver();
        }

        private IEdmModel GetModel<T>() where T : class
        {
            Type key = typeof(T);
            IEdmModel value;

            if (!_modelCache.TryGetValue(key, out value))
            {
                ODataModelBuilder model = new ODataConventionModelBuilder();
                model.EntitySet<T>("Products");
                if (key == typeof(Product))
                {
                    model.EntityType<DerivedProduct>().DerivesFrom<Product>();
                    model.EntityType<DerivedCategory>().DerivesFrom<Category>();
                }

                value = _modelCache[key] = model.GetEdmModel();
            }
            return value;
        }

        private class NoAssembliesResolver : IAssembliesResolver
        {
            public ICollection<Assembly> GetAssemblies()
            {
                return new Assembly[0];
            }
        }
    }
}
