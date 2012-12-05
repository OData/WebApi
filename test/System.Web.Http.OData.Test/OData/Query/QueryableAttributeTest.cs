// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
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
    }
}
