// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.OData.Routing;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Microsoft.TestCommon;
using ODataPath = System.Web.OData.Routing.ODataPath;

namespace System.Web.OData.Query
{
    public class ODataQueryOptionParserExtensionTest
    {
        [Fact]
        public void Filter_Works_QueryOptionCaseInsensitive()
        {
            // Arrange
            const string filter = "$FiLtEr=name eQ 'nba'";

            // Act
            ODataQueryOptions queryOptions = GetQueryOptions(filter);

            // Assert
            Assert.NotNull(queryOptions.Filter);
            FilterClause filterClause = queryOptions.Filter.FilterClause;
            BinaryOperatorNode node = Assert.IsType<BinaryOperatorNode>(filterClause.Expression);
            Assert.Equal(BinaryOperatorKind.Equal, node.OperatorKind);
        }

        [Fact]
        public void OrderBy_Works_QueryOptionCaseInsensitive()
        {
            // Arrange
            const string orderBy = "$oRdeRby=naMe";

            // Act
            ODataQueryOptions queryOptions = GetQueryOptions(orderBy);

            // Assert
            Assert.NotNull(queryOptions.OrderBy);
            OrderByClause orderByClause = queryOptions.OrderBy.OrderByClause;
            SingleValuePropertyAccessNode node = Assert.IsType<SingleValuePropertyAccessNode>(orderByClause.Expression);
            Assert.Equal("Name", node.Property.Name);
        }

        [Fact]
        public void Select_Works_QueryOptionCaseInsensitive()
        {
            // Arrange
            const string select = "$SeLecT=naMe";

            // Act
            ODataQueryOptions queryOptions = GetQueryOptions(select);

            // Assert
            Assert.NotNull(queryOptions.SelectExpand);
            SelectExpandClause selectClause = queryOptions.SelectExpand.SelectExpandClause;
            SelectItem selectItem = Assert.Single(selectClause.SelectedItems);
            PathSelectItem pathSelectItem = Assert.IsType<PathSelectItem>(selectItem);
            Assert.NotNull(pathSelectItem.SelectedPath);
            PropertySegment segment = Assert.IsType<PropertySegment>(pathSelectItem.SelectedPath.FirstSegment);
            Assert.Equal("Name", segment.Property.Name);
        }

        [Fact]
        public void Expand_Works_QueryOptionCaseInsensitive()
        {
            // Arrange
            const string expand = "$ExPAnd=ProdUCts";

            // Act
            ODataQueryOptions queryOptions = GetQueryOptions(expand);

            // Assert
            Assert.NotNull(queryOptions.SelectExpand);
            SelectExpandClause expandClause = queryOptions.SelectExpand.SelectExpandClause;
            ExpandedNavigationSelectItem expandItem = Assert.IsType<ExpandedNavigationSelectItem>(
                Assert.Single(expandClause.SelectedItems));
            NavigationPropertySegment segment =
                Assert.IsType<NavigationPropertySegment>(expandItem.PathToNavigationProperty.FirstSegment);
            Assert.Equal("Products", segment.NavigationProperty.Name);
        }

        private static ODataQueryOptions GetQueryOptions(string queryOption)
        {
            string uri = "Http://localhost/RoutingCustomers?" + queryOption;

            HttpConfiguration configuration = new HttpConfiguration();
            ODataUriResolver resolver = new ODataUriResolver
            {
                EnableCaseInsensitive = true
            };

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.SetConfiguration(configuration);
            request.EnableHttpDependencyInjectionSupport(b => b.AddService(ServiceLifetime.Singleton, sp => resolver));

            IEdmModel model = ODataRoutingModel.GetModel();

            IEdmEntitySet entityset = model.EntityContainer.FindEntitySet("RoutingCustomers");
            IEdmEntityType entityType =
                model.SchemaElements.OfType<IEdmEntityType>().Single(e => e.Name == "RoutingCustomer");

            ODataPath path = new ODataPath(new[] { new EntitySetSegment(entityset) });
            ODataQueryContext context = new ODataQueryContext(model, entityType, path);
            return new ODataQueryOptions(context, request);
        }
    }
}
