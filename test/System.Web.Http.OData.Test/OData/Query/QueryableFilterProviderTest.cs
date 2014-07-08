// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Http.OData.TestCommon.Models;
using Microsoft.TestCommon;

namespace System.Web.Http.OData.Query
{
    public class QueryableFilterProviderTest
    {
        [Theory]
        [InlineData("GetQueryable")]
        [InlineData("GetGenericQueryable")]
        [InlineData("GetSingleResult")]
        [InlineData("GetSingleResultOfT")]
        [InlineData("GetQueryableWithQueryableFilterInSubclass")]
        public void GetFilters_ReturnsQueryableFilter_ForQueryableActions(string actionName)
        {
            HttpConfiguration config = new HttpConfiguration();
            HttpControllerDescriptor controllerDescriptor = new HttpControllerDescriptor(config, "FilterProviderTest", typeof(FilterProviderTestController));
            HttpActionDescriptor actionDescriptor = new ReflectedHttpActionDescriptor(controllerDescriptor, typeof(FilterProviderTestController).GetMethod(actionName));

            FilterInfo[] filters = new QueryFilterProvider(new EnableQueryAttribute()).GetFilters(config, actionDescriptor).ToArray();

            Assert.Equal(1, filters.Length);
            Assert.Equal(FilterScope.Global, filters[0].Scope);
            EnableQueryAttribute filter = Assert.IsType<EnableQueryAttribute>(filters[0].Instance);
        }

        [Theory]
        [InlineData("GetEnumerable")]
        [InlineData("GetGenericEnumerable")]
        [InlineData("GetArray")]
        [InlineData("GetList")]
        [InlineData("GetObject")]
        [InlineData("GetGenericQueryableWithODataQueryOption")]
        [InlineData("GetGenericQueryableWithODataQueryOptionOfT")]
        [InlineData("GetGenericQueryableWithODataQueryOption2")]
        public void GetFilters_ReturnsEmptyCollection_ForNonQueryableActions(string actionName)
        {
            HttpConfiguration config = new HttpConfiguration();
            HttpControllerDescriptor controllerDescriptor = new HttpControllerDescriptor(config, "FilterProviderTest", typeof(FilterProviderTestController));
            HttpActionDescriptor actionDescriptor = new ReflectedHttpActionDescriptor(controllerDescriptor, typeof(FilterProviderTestController).GetMethod(actionName));

            FilterInfo[] filters = new QueryFilterProvider(new EnableQueryAttribute()).GetFilters(config, actionDescriptor).ToArray();

            Assert.Empty(filters);
        }

        [Theory]
        [InlineData("GetQueryableWithQueryableFilterAttributeOnBase",typeof(FilterProviderTestController))]
        [InlineData("Get", typeof(ODataControllerWithQueryableFilterContainingQueryableActionController))]
        [InlineData("GetQueryableWithQueryableFilterInSubclass", typeof(DerivedFilterProviderTestController))]
        [InlineData("Get", typeof(DerivedApiControllerContainingQueryableActionControllerController))]
        public void GetFilters_ReturnsEmptyCollections_ForActionsWithQueryableAttributeApplied(
            string actionName, 
            Type controllerType)
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();
            string controllerName = controllerType.Name.Replace("Controller","");
            HttpControllerDescriptor controllerDescriptor = new HttpControllerDescriptor(
                config, 
                controllerName,
                controllerType);

            HttpActionDescriptor actionDescriptor = new ReflectedHttpActionDescriptor(
                controllerDescriptor, 
                controllerType.GetMethod(actionName));

            // Act
            FilterInfo[] filters = new QueryFilterProvider(new EnableQueryAttribute(), skipQueryableAttribute: true)
                .GetFilters(config, actionDescriptor).ToArray();

            // Assert
            Assert.Empty(filters);
        }

        [Theory]
        [InlineData(typeof(IEnumerable), false)]
        [InlineData(typeof(IQueryable), true)]
        [InlineData(typeof(IEnumerable<Customer>), false)]
        [InlineData(typeof(IQueryable<Customer>), true)]
        [InlineData(typeof(object), false)]
        [InlineData(typeof(string), false)]
        [InlineData(typeof(List<Customer>), false)]
        [InlineData(typeof(Customer[]), false)]
        public void IsIQueryable_ReturnsWhetherTypeIsIQueryable(Type type, bool isIQueryable)
        {
            Assert.Equal(isIQueryable, QueryFilterProvider.IsIQueryable(type));
        }
    }

    public class FilterProviderTestController : ODataController
    {
        public IEnumerable GetEnumerable()
        {
            return null;
        }

        public IQueryable GetQueryable()
        {
            return null;
        }

        public IEnumerable<string> GetGenericEnumerable()
        {
            return null;
        }

        public IQueryable<string> GetGenericQueryable()
        {
            return null;
        }

        public string[] GetArray()
        {
            return null;
        }

        public List<string> GetList()
        {
            return null;
        }

        public object GetObject()
        {
            return null;
        }

        public IQueryable<string> GetGenericQueryableWithODataQueryOption(ODataQueryOptions queryOptions)
        {
            return null;
        }

        public IQueryable GetGenericQueryableWithODataQueryOptionOfT(ODataQueryOptions<string> queryOptions)
        {
            return null;
        }

        public IQueryable<string> GetGenericQueryableWithODataQueryOption2(string s, ODataQueryOptions queryOptions)
        {
            return null;
        }

        [EnableQuery(PageSize = 100)]
        public IQueryable GetQueryableWithFilterAttribute()
        {
            return null;
        }

        // Disable obsolete warning for QueryableAttribute
        #pragma warning disable 0618
        [Queryable(PageSize = 100)]
        #pragma warning restore 0618
        public virtual IQueryable GetQueryableWithQueryableFilterAttributeOnBase()
        {
            return null;
        }

        public virtual IQueryable GetQueryableWithQueryableFilterInSubclass()
        {
            return null;
        }

        public SingleResult GetSingleResult()
        {
            return null;
        }

        public SingleResult<int> GetSingleResultOfT()
        {
            return null;
        }
    }

    public class DerivedFilterProviderTestController : FilterProviderTestController
    {
        public override IQueryable GetQueryableWithQueryableFilterAttributeOnBase()
        {
            return null;
        }

        // Disable obsolete warning for QueryableAttribute
        #pragma warning disable 0618
        [Queryable]
        #pragma warning restore 0618
        public override IQueryable GetQueryableWithQueryableFilterInSubclass()
        {
            return null;
        }
    }

    // Disable obsolete warning for QueryableAttribute
    #pragma warning disable 0618
    [Queryable]
    #pragma warning restore 0618
    public class ODataControllerWithQueryableFilterContainingQueryableActionController : ODataController
    {
        public IQueryable Get()
        {
            return null;
        }
    }

    public class ApiControllerContainingQueryableActionController : ApiController
    {
        public virtual IQueryable Get()
        {
            return null;
        }
    }

    // Disable obsolete warning for QueryableAttribute
    #pragma warning disable 0618
    [Queryable]
    #pragma warning restore 0618
    public class DerivedApiControllerContainingQueryableActionControllerController : 
        ApiControllerContainingQueryableActionController
    {
    }
}
