// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web.Http.Controllers;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Query.Validators;
using Microsoft.Data.OData;
using Microsoft.TestCommon;

namespace System.Web.Http.OData.Query
{
    public class QueryableAttributeTest
    {
        [Theory]
        [InlineData("Id,Address")]
        [InlineData("   Id,Address  ")]
        [InlineData(" Id , Address ")]
        [InlineData("Id, Address")]
        public void OrderByDisllowedPropertiesWithSpaces(string allowedProperties)
        {
            QueryableAttribute attribute = new QueryableAttribute();
            attribute.AllowedOrderByProperties = allowedProperties;
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/Customers/?$orderby=Id,Name");
            ODataQueryOptions queryOptions = new ODataQueryOptions(ValidationTestHelper.CreateCustomerContext(), request);

            Assert.Throws<ODataException>(() => attribute.ValidateQuery(request, queryOptions),
                "Order by 'Name' is not allowed. To allow it, set the 'AllowedOrderByProperties' property on QueryableAttribute or QueryValidationSettings.");
        }

        [Theory]
        [InlineData("Id,Name")]
        [InlineData("   Id,Name  ")]
        [InlineData(" Id , Name ")]
        [InlineData("Id, Name")]
        public void OrderByAllowedPropertiesWithSpaces(string allowedProperties)
        {
            QueryableAttribute attribute = new QueryableAttribute();
            attribute.AllowedOrderByProperties = allowedProperties;
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/Customers/?$orderby=Id,Name");
            ODataQueryOptions queryOptions = new ODataQueryOptions(ValidationTestHelper.CreateCustomerContext(), request);

            Assert.DoesNotThrow(() => attribute.ValidateQuery(request, queryOptions));
        }

        [Fact]
        public void CreateQueryContext_ReturnsQueryContext_ForNoModelOnConfiguration()
        {
            var entityClrType = typeof(QueryCompositionCustomer);
            var config = new HttpConfiguration();
            var descriptor = new ReflectedHttpActionDescriptor();
            descriptor.Configuration = config;

            var context = QueryableAttribute.CreateQueryContext(entityClrType, config, descriptor);

            Assert.NotNull(context);
            Assert.Equal(typeof(QueryCompositionCustomer), context.ElementClrType);
            Assert.Same(descriptor.Properties["MS_EdmModelSystem.Web.Http.OData.Query.QueryCompositionCustomer"], context.Model);
        }

        [Fact]
        public void CreateQueryContext_ReturnsQueryContext_ForNonMatchingModelOnConfiguration()
        {
            var builder = new ODataConventionModelBuilder();
            var model = builder.GetEdmModel();
            var entityClrType = typeof(QueryCompositionCustomer);
            var config = new HttpConfiguration();
            config.SetEdmModel(model);
            var descriptor = new ReflectedHttpActionDescriptor();
            descriptor.Configuration = config;

            var context = QueryableAttribute.CreateQueryContext(entityClrType, config, descriptor);

            Assert.NotNull(context);
            Assert.Equal(typeof(QueryCompositionCustomer), context.ElementClrType);
            Assert.Same(descriptor.Properties["MS_EdmModelSystem.Web.Http.OData.Query.QueryCompositionCustomer"], context.Model);
        }


        [Fact]
        public void CreateQueryContext_ReturnsQueryContext_ForMatchingModelOnConfiguration()
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<QueryCompositionCustomer>("customers");
            var model = builder.GetEdmModel();
            var entityClrType = typeof(QueryCompositionCustomer);
            var config = new HttpConfiguration();
            config.SetEdmModel(model);
            var descriptor = new ReflectedHttpActionDescriptor();
            descriptor.Configuration = config;

            var context = QueryableAttribute.CreateQueryContext(entityClrType, config, descriptor);

            Assert.NotNull(context);
            Assert.Equal(typeof(QueryCompositionCustomer), context.ElementClrType);
            Assert.Same(model, context.Model);
            Assert.False(descriptor.Properties.ContainsKey("MS_EdmModelSystem.Web.Http.OData.Query.QueryCompositionCustomer"));
        }
    }
}
