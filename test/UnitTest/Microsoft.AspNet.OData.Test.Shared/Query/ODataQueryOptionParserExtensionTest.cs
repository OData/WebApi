//-----------------------------------------------------------------------------
// <copyright file="ODataQueryOptionParserExtensionTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Linq;
using System.Net.Http;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Routing;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Xunit;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;

namespace Microsoft.AspNet.OData.Test.Query
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

            ODataUriResolver resolver = new ODataUriResolver
            {
                EnableCaseInsensitive = true
            };

            var configuration = RoutingConfigurationFactory.CreateWithRootContainer("OData", b => b.AddService(ServiceLifetime.Singleton, sp => resolver));
            var request = RequestFactory.Create(HttpMethod.Get, uri, configuration, "OData");

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
