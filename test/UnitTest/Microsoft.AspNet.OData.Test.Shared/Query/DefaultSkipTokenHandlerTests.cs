// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if NETCORE
using System;
using Microsoft.AspNet.OData.Formatter.Serialization;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Xunit;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;
#else
using System;
using System.Linq;
using System.Web.Http.Routing;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Formatter.Serialization;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Xunit;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;
#endif

namespace Microsoft.AspNet.OData.Test.Query
{
    public class DefaultSkipTokenHandlerTests
    {
        [Theory]
        [InlineData("http://localhost/Customers(1)/Orders", "http://localhost/Customers(1)/Orders?$skip=10")]
        [InlineData("http://localhost/Customers?$expand=Orders", "http://localhost/Customers?$expand=Orders&$skip=10")]
        public void GetNextPageLink_ReturnsCorrectNextLink(string baseUri, string expectedUri)
        {
            // Arrange
            var context = GetContext(false);
            var nextLinkGenerator = context.QueryContext.GetSkipTokenHandler();

            // Act
            var uri = nextLinkGenerator.GenerateNextPageLink(new Uri(baseUri), 10, null, context);
            var actualUri = uri.ToString();

            // Assert
            Assert.Equal(expectedUri, actualUri);
        }

        private ODataSerializerContext GetContext(bool enableSkipToken = false)
        {
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            IEdmEntitySet entitySet = model.Customers;
            IEdmEntityType entityType = entitySet.EntityType();
            IEdmProperty edmProperty = entityType.FindProperty("Name");
            IEdmType edmType = entitySet.Type;
            ODataPath path = new ODataPath(new EntitySetSegment(entitySet));
            ODataQueryContext queryContext = new ODataQueryContext(model.Model, edmType, path);
            queryContext.DefaultQuerySettings.EnableSkipToken = enableSkipToken;

            var config = RoutingConfigurationFactory.CreateWithRootContainer("OData");
            var request = RequestFactory.Create(config, "OData");
            ResourceContext resource = new ResourceContext();
            ODataSerializerContext context = new ODataSerializerContext(resource, edmProperty, queryContext, null);
            return context;
        }        
    }
}
