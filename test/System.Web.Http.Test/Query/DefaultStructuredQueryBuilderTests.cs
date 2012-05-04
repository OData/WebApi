// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Http.Query
{
    public class DefaultStructuredQueryBuilderTests
    {
        IStructuredQueryBuilder _structuredQueryBuilder = new DefaultStructuredQueryBuilder();

        [Fact]
        public void GetStructuredQuery_ThrowsArgumentNull_uri()
        {
            Assert.ThrowsArgumentNull(() =>
            {
                _structuredQueryBuilder.GetStructuredQuery(uri: null);
            }, "uri");
        }

        [Fact]
        public void GetStructuredQuery_Filter()
        {
            StructuredQuery query = _structuredQueryBuilder.GetStructuredQuery(new Uri("http://localhost/?$filter=value"));

            IStructuredQueryPart filter = query.QueryParts.Where(queryPart => queryPart.QueryOperator == "filter").SingleOrDefault();
            Assert.NotNull(filter);
            Assert.Equal("filter", filter.QueryOperator);
            Assert.Equal("value", filter.QueryExpression);
        }

        [Fact]
        public void GetStructuredQuery_skip()
        {
            StructuredQuery query = _structuredQueryBuilder.GetStructuredQuery(new Uri("http://localhost/?$skip=value"));

            IStructuredQueryPart skip = query.QueryParts.Where(queryPart => queryPart.QueryOperator == "skip").SingleOrDefault();
            Assert.NotNull(skip);
            Assert.Equal("skip", skip.QueryOperator);
            Assert.Equal("value", skip.QueryExpression);
        }

        [Fact]
        public void GetStructuredQuery_top()
        {
            StructuredQuery query = _structuredQueryBuilder.GetStructuredQuery(new Uri("http://localhost/?$top=value"));

            IStructuredQueryPart top = query.QueryParts.Where(queryPart => queryPart.QueryOperator == "top").SingleOrDefault();
            Assert.NotNull(top);
            Assert.Equal("top", top.QueryOperator);
            Assert.Equal("value", top.QueryExpression);
        }

        [Fact]
        public void GetStructuredQuery_orderby()
        {
            StructuredQuery query = _structuredQueryBuilder.GetStructuredQuery(new Uri("http://localhost/?$orderby=value"));

            IStructuredQueryPart orderby = query.QueryParts.Where(queryPart => queryPart.QueryOperator == "orderby").SingleOrDefault();
            Assert.NotNull(orderby);
            Assert.Equal("orderby", orderby.QueryOperator);
            Assert.Equal("value", orderby.QueryExpression);
        }

        [Fact]
        public void GetStructuredQuery()
        {
            StructuredQuery query = _structuredQueryBuilder.GetStructuredQuery(new Uri("http://localhost/?$filter=filtervalue&$skip=skipvalue&$top=topvalue&$orderby=orderbyvalue"));

            Assert.Equal(4, query.QueryParts.Count());

            IStructuredQueryPart filter = query.QueryParts.Where(queryPart => queryPart.QueryOperator == "filter").SingleOrDefault();
            Assert.NotNull(filter);
            Assert.Equal("filtervalue", filter.QueryExpression);

            IStructuredQueryPart skip = query.QueryParts.Where(queryPart => queryPart.QueryOperator == "skip").SingleOrDefault();
            Assert.NotNull(skip);
            Assert.Equal("skipvalue", skip.QueryExpression);

            IStructuredQueryPart top = query.QueryParts.Where(queryPart => queryPart.QueryOperator == "top").SingleOrDefault();
            Assert.NotNull(top);
            Assert.Equal("topvalue", top.QueryExpression);

            IStructuredQueryPart orderby = query.QueryParts.Where(queryPart => queryPart.QueryOperator == "orderby").SingleOrDefault();
            Assert.NotNull(orderby);
            Assert.Equal("orderbyvalue", orderby.QueryExpression);
        }
    }
}
