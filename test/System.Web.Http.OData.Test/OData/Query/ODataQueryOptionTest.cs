// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Builder.TestModels;
using Microsoft.Data.OData;
using Microsoft.TestCommon;

namespace System.Web.Http.OData.Query
{
    public class ODataQueryOptionTest
    {
        public static TheoryDataSet<string, string> QueryTestData
        {
            get
            {
                return new TheoryDataSet<string, string>
                {
                     { "$filter", null },
                     { "$filter", "''" },
                     { "$filter", "" },
                     { "$filter", " " },
                     { "$filter", "Name eq 'MSFT'" },

                     { "$orderby", null },
                     { "$orderby", "''" },
                     { "$orderby", "" },
                     { "$orderby", " " },
                     { "$orderby", "Name" },

                     { "$top", null },
                     { "$top", "''" },
                     { "$top", "" },
                     { "$top", " " },
                     { "$top", "12" },

                     { "$skip", null },
                     { "$skip", "''" },
                     { "$skip", "" },
                     { "$skip", " " },
                     { "$skip", "12" }
                };
            }
        }

        // Move items to this list from UnsupportedQueryNames as they become supported
        public static TheoryDataSet<string> SupportedQueryNames
        {
            get
            {
                return new TheoryDataSet<string>
                {
                    "$orderby",
                    "$filter",
                    "$top",
                    "$skip"
                };
            }
        }

        // Used to test modifications to $orderby when $skip or $top are present
        // and the entity type has 2 keys -- CustomerId and Name.
        // Tuple is: query expression, canUseDefaultOrderBy, expected expression
        public static TheoryDataSet<string, bool, string> SkipTopOrderByUsingKeysTestData
        {
            get
            {
                return new TheoryDataSet<string, bool, string>
                {
                    // First key present with $skip, adds 2nd key
                    { "$orderby=CustomerId&$skip=1", true,  "OrderBy(p1 => p1.CustomerId).ThenBy(p1 => p1.Name).Skip(1)" },

                    // First key present with $top, adds 2nd key
                    { "$orderby=CustomerId&$top=1", true,  "OrderBy(p1 => p1.CustomerId).ThenBy(p1 => p1.Name).Take(1)" },

                    // First key present with $skip and $top, adds 2nd key
                    { "$orderby=CustomerId&$skip=1&$top=2", true,  "OrderBy(p1 => p1.CustomerId).ThenBy(p1 => p1.Name).Skip(1).Take(2)" },

                    // First key present, no $skip or $top, no modification
                    { "$orderby=CustomerId", false, "OrderBy(p1 => p1.CustomerId)" },

                    // First key present, 'canUseDefaultOrderBy' is false, no modification
                    { "$orderby=CustomerId&$skip=1", false, "OrderBy(p1 => p1.CustomerId).Skip(1)" },

                    // Second key present, adds 1st key after 2nd
                    { "$orderby=Name&$skip=1", true, "OrderBy(p1 => p1.Name).ThenBy(p1 => p1.CustomerId).Skip(1)" },

                    // Second key plus 'asc' suffix, adds 1st key and preserves suffix
                    { "$orderby=Name asc&$skip=1", true, "OrderBy(p1 => p1.Name).ThenBy(p1 => p1.CustomerId).Skip(1)" },

                    // Second key plus 'desc' suffix, adds 1st key and preserves suffix
                    { "$orderby=Name desc&$skip=1", true, "OrderByDescending(p1 => p1.Name).ThenBy(p1 => p1.CustomerId).Skip(1)" },

                    // All keys present, no modification
                    { "$orderby=CustomerId,Name&$skip=1", true,  "OrderBy(p1 => p1.CustomerId).ThenBy(p1 => p1.Name).Skip(1)" },

                    // All keys present but in reverse order, no modification
                    { "$orderby=Name,CustomerId&$skip=1", true,  "OrderBy(p1 => p1.Name).ThenBy(p1 => p1.CustomerId).Skip(1)" },

                    // First key present but with extraneous whitespace, adds 2nd key
                    { "$orderby= CustomerId &$skip=1", true,  "OrderBy(p1 => p1.CustomerId).ThenBy(p1 => p1.Name).Skip(1)" },

                    // All keys present with extraneous whitespace, no modification
                    { "$orderby= \t CustomerId \t , Name \t desc \t &$skip=1", true,  "OrderBy(p1 => p1.CustomerId).ThenByDescending(p1 => p1.Name).Skip(1)" },

                    // Ordering on non-key property, adds all keys
                    { "$orderby=Website&$skip=1", true,  "OrderBy(p1 => p1.Website).ThenBy(p1 => p1.CustomerId).ThenBy(p1 => p1.Name).Skip(1)" },
                };
            }
        }

        // Used to test modifications to $orderby when $skip or $top are present
        // and the entity type has a no key properties.
        // Tuple is: query expression, canUseDefaultOrderBy, expected expression
        public static TheoryDataSet<string, bool, string> SkipTopOrderByWithNoKeysTestData
        {
            get
            {
                return new TheoryDataSet<string, bool, string>
                {
                    // Single property present with $skip, adds all remaining in alphabetic order
                    { "$orderby=CustomerId&$skip=1", true,  "OrderBy(p1 => p1.CustomerId).ThenBy(p1 => p1.Name).ThenBy(p1 => p1.SharePrice).ThenBy(p1 => p1.ShareSymbol).ThenBy(p1 => p1.Website).Skip(1)" },

                    // Single property present with $top, adds all remaining in alphabetic order
                    { "$orderby=CustomerId&$top=1", true,  "OrderBy(p1 => p1.CustomerId).ThenBy(p1 => p1.Name).ThenBy(p1 => p1.SharePrice).ThenBy(p1 => p1.ShareSymbol).ThenBy(p1 => p1.Website).Take(1)" },

                    // Single property present with $skip and $top, adds all remaining in alphabetic order
                    { "$orderby=CustomerId&$skip=1&$top=2", true,  "OrderBy(p1 => p1.CustomerId).ThenBy(p1 => p1.Name).ThenBy(p1 => p1.SharePrice).ThenBy(p1 => p1.ShareSymbol).ThenBy(p1 => p1.Website).Skip(1).Take(2)" },

                    // Single property present, no $skip or $top, no modification
                    { "$orderby=SharePrice", false,  "OrderBy(p1 => p1.SharePrice)" },

                    // Single property present, canUseDefaultOrderBy is false, no modification
                    { "$orderby=SharePrice&$skip=1", false,  "OrderBy(p1 => p1.SharePrice).Skip(1)" },

                    // All properties present, non-alphabetic order, no modification
                    { "$orderby=Name,SharePrice,CustomerId,Website,ShareSymbol&$skip=1", true,  "OrderBy(p1 => p1.Name).ThenBy(p1 => p1.SharePrice).ThenBy(p1 => p1.CustomerId).ThenBy(p1 => p1.Website).ThenBy(p1 => p1.ShareSymbol).Skip(1)" },

                    // All properties present, extraneous whitespace, non-alphabetic order, no modification
                    { "$orderby= \t Name \t , \t SharePrice \t , \t CustomerId \t , \t Website \t , \t ShareSymbol \t &$skip=1", true,  "OrderBy(p1 => p1.Name).ThenBy(p1 => p1.SharePrice).ThenBy(p1 => p1.CustomerId).ThenBy(p1 => p1.Website).ThenBy(p1 => p1.ShareSymbol).Skip(1)" },

                };
            }
        }

        // Move items from this list to SupportedQueryNames as they become supported
        public static TheoryDataSet<string> UnsupportedQueryNames
        {
            get
            {
                return new TheoryDataSet<string>
                {
                    "$select",
                    "$inlinecount",
                    "$expand",
                    "$skiptoken"
                };
            }
        }

        internal static IQueryable Customers = new List<Customer>().AsQueryable();

        [Fact]
        public void ConstructorNullContextThrows()
        {
            Assert.Throws<ArgumentNullException>(
                () => new ODataQueryOptions(null, new HttpRequestMessage())
            );
        }

        [Fact]
        public void ConstructorNullRequestThrows()
        {
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();
            Assert.Throws<ArgumentNullException>(
                () => new ODataQueryOptions(new ODataQueryContext(model, typeof(Customer)), null)
            );
        }

        [Fact]
        [Trait("ODataQueryOption", "Can bind ODataQueryOption to the uri")]
        public void CanExtractQueryOptionsCorrectly()
        {
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();

            var message = new HttpRequestMessage(
                HttpMethod.Get,
                new Uri("http://server/service/Customers/?$filter=Filter&$select=Select&$orderby=OrderBy&$expand=Expand&$top=10&$skip=20&$inlinecount=allpages&$skiptoken=SkipToken")
            );

            var queryOptions = new ODataQueryOptions(new ODataQueryContext(model, typeof(Customer), "Customers"), message);
            Assert.Equal("Filter", queryOptions.RawValues.Filter);
            Assert.NotNull(queryOptions.Filter);
            Assert.Equal("OrderBy", queryOptions.RawValues.OrderBy);
            Assert.NotNull(queryOptions.OrderBy);
            Assert.Equal("10", queryOptions.RawValues.Top);
            Assert.NotNull(queryOptions.Top);
            Assert.Equal("20", queryOptions.RawValues.Skip);
            Assert.NotNull(queryOptions.Skip);
            Assert.Equal("Expand", queryOptions.RawValues.Expand);
            Assert.Equal("Select", queryOptions.RawValues.Select);
            Assert.Equal("allpages", queryOptions.RawValues.InlineCount);
            Assert.Equal("SkipToken", queryOptions.RawValues.SkipToken);
        }

        [Fact]
        public void ApplyTo_Throws_With_Null_Queryable()
        {
            // Arrange
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();

            var message = new HttpRequestMessage(
                HttpMethod.Get,
                new Uri("http://server/service/Customers/?$filter=Filter&$select=Select&$orderby=OrderBy&$expand=Expand&$top=10&$skip=20&$inlinecount=allpages&$skiptoken=SkipToken")
            );

            var queryOptions = new ODataQueryOptions(new ODataQueryContext(model, typeof(Customer), "Customers"), message);

            // Act & Assert
            Assert.ThrowsArgumentNull(() => queryOptions.ApplyTo(null), "query");
        }

        [Fact]
        public void ApplyTo_HandleNullPropagation_Throws_With_Null_Queryable()
        {
            // Arrange
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();

            var message = new HttpRequestMessage(
                HttpMethod.Get,
                new Uri("http://server/service/Customers/?$filter=Filter&$select=Select&$orderby=OrderBy&$expand=Expand&$top=10&$skip=20&$inlinecount=allpages&$skiptoken=SkipToken")
            );

            var queryOptions = new ODataQueryOptions(new ODataQueryContext(model, typeof(Customer), "Customers"), message);

            // Act & Assert
            Assert.ThrowsArgumentNull(() => queryOptions.ApplyTo(null, handleNullPropagation: true), "query");
        }

        [Fact]
        public void ApplyTo_HandleNullPropagation_CanUseDefaultOrderBy_Throws_With_Null_Queryable()
        {
            // Arrange
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();

            var message = new HttpRequestMessage(
                HttpMethod.Get,
                new Uri("http://server/service/Customers/?$filter=Filter&$select=Select&$orderby=OrderBy&$expand=Expand&$top=10&$skip=20&$inlinecount=allpages&$skiptoken=SkipToken")
            );

            var queryOptions = new ODataQueryOptions(new ODataQueryContext(model, typeof(Customer), "Customers"), message);

            // Act & Assert
            Assert.ThrowsArgumentNull(() => queryOptions.ApplyTo(null, handleNullPropagation: true, canUseDefaultOrderBy: true), "query");
        }

        [Theory]
        [PropertyData("SkipTopOrderByUsingKeysTestData")]
        public void ApplyTo_Adds_Missing_Keys_To_OrderBy(string oDataQuery, bool canUseDefaultOrderBy, string expectedExpression)
        {
            // Arrange
            var model = new ODataModelBuilder()
                .Add_Customers_With_Keys_EntitySet(c => new { c.CustomerId, c.Name }).GetEdmModel();

            var message = new HttpRequestMessage(
                HttpMethod.Get,
                new Uri("http://server/service/Customers?" + oDataQuery)
            );

            var queryOptions = new ODataQueryOptions(new ODataQueryContext(model, typeof(Customer), "Customers"), message);

            // Act
            IQueryable finalQuery = queryOptions.ApplyTo(new Customer[0].AsQueryable(), handleNullPropagation: true, canUseDefaultOrderBy: canUseDefaultOrderBy);

            // Assert
            string queryExpression = finalQuery.Expression.ToString();
            queryExpression = queryExpression.Substring(queryExpression.IndexOf("]") + 2);

            Assert.Equal(queryExpression, expectedExpression);
        }

        [Theory]
        [PropertyData("SkipTopOrderByWithNoKeysTestData")]
        public void ApplyTo_Adds_Missing_NonKey_Properties_To_OrderBy(string oDataQuery, bool canUseDefaultOrderBy, string expectedExpression)
        {
            // Arrange
            var model = new ODataModelBuilder()
                .Add_Customers_No_Keys_EntitySet().GetEdmModel();

            var message = new HttpRequestMessage(
                HttpMethod.Get,
                new Uri("http://server/service/Customers?" + oDataQuery)
            );

            var queryOptions = new ODataQueryOptions(new ODataQueryContext(model, typeof(Customer), "Customers"), message);

            // Act
            IQueryable finalQuery = queryOptions.ApplyTo(new Customer[0].AsQueryable(), handleNullPropagation: true, canUseDefaultOrderBy: canUseDefaultOrderBy);

            // Assert
            string queryExpression = finalQuery.Expression.ToString();
            queryExpression = queryExpression.Substring(queryExpression.IndexOf("]") + 2);

            Assert.Equal(queryExpression, expectedExpression);
        }

        [Fact]
        public void ApplyTo_Does_Not_Replace_Original_OrderBy_With_Missing_Keys()
        {
            // Arrange
            var model = new ODataModelBuilder()
                            .Add_Customers_No_Keys_EntitySet().GetEdmModel();

            var message = new HttpRequestMessage(
                HttpMethod.Get,
                new Uri("http://server/service/Customers?$orderby=Name")
            );

            // Act
            var queryOptions = new ODataQueryOptions(new ODataQueryContext(model, typeof(Customer), "Customers"), message);
            OrderByQueryOption originalOption = queryOptions.OrderBy;

            IQueryable finalQuery = queryOptions.ApplyTo(new Customer[0].AsQueryable(), handleNullPropagation: true, canUseDefaultOrderBy: true);

            // Assert
            Assert.ReferenceEquals(originalOption, queryOptions.OrderBy);
        }

        [Fact]
        [Trait("ODataQueryOption", "Can bind a typed ODataQueryOption to the request uri without any query")]
        public void ContextPropertyGetter()
        {
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();

            var message = new HttpRequestMessage(
                HttpMethod.Get,
                new Uri("http://server/service/Customers")
            );
            var queryOptions = new ODataQueryOptions(new ODataQueryContext(model, typeof(Customer)), message);
            var entityType = queryOptions.Context.EntityClrType;
            Assert.NotNull(entityType);
            Assert.Equal(typeof(Customer).FullName, entityType.Namespace + "." + entityType.Name);
        }

        [Theory]
        [PropertyData("QueryTestData")]
        public void QueryTest(string queryName, string queryValue)
        {
            // Arrange
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();

            string uri;
            if (queryValue == null)
            {
                // same as not passing the query - - this would work
                uri = string.Format("http://server/service/Customers?{0}=", queryName);
            }
            else
            {
                // if queryValue is invalid, such as whitespace or not a number for top and skip
                uri = string.Format("http://server/service/Customers?{0}={1}", queryName, queryValue);
            }

            var message = new HttpRequestMessage(
                HttpMethod.Get,
                new Uri(uri)
            );

            // Act && Assert
            if (String.IsNullOrWhiteSpace(queryValue))
            {
                Assert.Throws<ODataException>(() =>
                    new ODataQueryOptions(new ODataQueryContext(model, typeof(Customer), "Customers"), message));
            }
            else
            {
                var queryOptions = new ODataQueryOptions(new ODataQueryContext(model, typeof(Customer), "Customers"), message);

                if (queryName == "$filter")
                {
                    Assert.Equal(queryValue, queryOptions.RawValues.Filter);
                }
                else if (queryName == "$orderby")
                {
                    Assert.Equal(queryValue, queryOptions.RawValues.OrderBy);
                }
                else if (queryName == "$top")
                {
                    Assert.Equal(queryValue, queryOptions.RawValues.Top);
                }
                else if (queryName == "$skip")
                {
                    Assert.Equal(queryValue, queryOptions.RawValues.Skip);
                }
            }
        }

        [Fact]
        public void MissingQueryReturnsOriginalList()
        {
            // Arrange
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();

            // the query is completely missing - this would work
            string uri = "http://server/service/Customers";
            var message = new HttpRequestMessage(
                HttpMethod.Get,
                new Uri(uri)
            );

            // Act
            var queryOptions = new ODataQueryOptions(new ODataQueryContext(model, typeof(Customer), "Customers"), message);

            // Assert: everything is null
            Assert.Null(queryOptions.RawValues.OrderBy);
            Assert.Null(queryOptions.RawValues.Filter);
            Assert.Null(queryOptions.RawValues.Skip);
            Assert.Null(queryOptions.RawValues.Top);
        }

        [Fact]
        public void OrderbyWithUnknownPropertyThrows()
        {
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();

            var message = new HttpRequestMessage(
                HttpMethod.Get,
                new Uri("http://server/service/Customers?$orderby=UnknownProperty")
            );

            var option = new ODataQueryOptions(new ODataQueryContext(model, typeof(Customer), "Customers"), message);
            Assert.Throws<ODataException>(() =>
            {
                option.ApplyTo(new List<Customer>().AsQueryable());
            });
        }

        [Fact]
        public void CannotConvertBadTopQueryThrows()
        {
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();

            var message = new HttpRequestMessage(
                HttpMethod.Get,
                new Uri("http://server/service/Customers?$top=NotANumber")
            );

            var options = new ODataQueryOptions(new ODataQueryContext(model, typeof(Customer), "Customers"), message);
            Assert.Throws<ODataException>(() =>
                 options.ApplyTo(Customers),
                 "Failed to convert 'NotANumber' to an integer.");

            message = new HttpRequestMessage(
                HttpMethod.Get,
                new Uri("http://server/service/Customers?$top=''")
            );

            options = new ODataQueryOptions(new ODataQueryContext(model, typeof(Customer), "Customers"), message);
            Assert.Throws<ODataException>(() =>
                 options.ApplyTo(Customers),
                 "Failed to convert '''' to an integer.");
        }


        [Fact]
        public void CannotConvertBadSkipQueryThrows()
        {
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();

            var message = new HttpRequestMessage(
                HttpMethod.Get,
                new Uri("http://server/service/Customers?$skip=NotANumber")
            );

            var options = new ODataQueryOptions(new ODataQueryContext(model, typeof(Customer), "Customers"), message);
            Assert.Throws<ODataException>(() =>
                 options.ApplyTo(Customers),
                 "Failed to convert 'NotANumber' to an integer.");

            message = new HttpRequestMessage(
                HttpMethod.Get,
                new Uri("http://server/service/Customers?$skip=''")
            );

            options = new ODataQueryOptions(new ODataQueryContext(model, typeof(Customer), "Customers"), message);
            Assert.Throws<ODataException>(() =>
                 options.ApplyTo(Customers),
                 "Failed to convert '''' to an integer.");
        }

        [Theory]
        [InlineData("$skip=1", typeof(ODataQueryOptionTest_ComplexModel), "OrderBy(p1 => p1.A).ThenBy(p1 => p1.B).Skip(1)")]
        [InlineData("$skip=1", typeof(ODataQueryOptionTest_EntityModel), "OrderBy(p1 => p1.ID).Skip(1)")]
        [InlineData("$skip=1", typeof(ODataQueryOptionTest_EntityModelMultipleKeys), "OrderBy(p1 => p1.ID1).ThenBy(p1 => p1.ID2).Skip(1)")]
        [InlineData("$top=1", typeof(ODataQueryOptionTest_ComplexModel), "OrderBy(p1 => p1.A).ThenBy(p1 => p1.B).Take(1)")]
        [InlineData("$top=1", typeof(ODataQueryOptionTest_EntityModel), "OrderBy(p1 => p1.ID).Take(1)")]
        [InlineData("$top=1", typeof(ODataQueryOptionTest_EntityModelMultipleKeys), "OrderBy(p1 => p1.ID1).ThenBy(p1 => p1.ID2).Take(1)")]
        [InlineData("$skip=1&$top=1", typeof(ODataQueryOptionTest_ComplexModel), "OrderBy(p1 => p1.A).ThenBy(p1 => p1.B).Skip(1).Take(1)")]
        [InlineData("$skip=1&$top=1", typeof(ODataQueryOptionTest_EntityModel), "OrderBy(p1 => p1.ID).Skip(1).Take(1)")]
        [InlineData("$skip=1&$top=1", typeof(ODataQueryOptionTest_EntityModelMultipleKeys), "OrderBy(p1 => p1.ID1).ThenBy(p1 => p1.ID2).Skip(1).Take(1)")]
        public void ApplyTo_Picks_DefaultOrder(string oDataQuery, Type elementType, string expectedExpression)
        {
            IQueryable query = Array.CreateInstance(elementType, 0).AsQueryable();
            var modelBuilder = new ODataConventionModelBuilder();
            modelBuilder.AddEntitySet("entityset", modelBuilder.AddEntity(elementType));
            var model = modelBuilder.GetEdmModel();

            var message = new HttpRequestMessage(
                HttpMethod.Get,
                new Uri("http://server/service/entityset?" + oDataQuery)
            );

            var options = new ODataQueryOptions(new ODataQueryContext(model, elementType, "entityset"), message);
            IQueryable finalQuery = options.ApplyTo(query);

            string queryExpression = finalQuery.Expression.ToString();
            queryExpression = queryExpression.Substring(queryExpression.IndexOf("OrderBy"));

            Assert.Equal(queryExpression, expectedExpression);
        }

        [Theory]
        [InlineData("$filter=1 eq 1")]
        [InlineData("")]
        public void ApplyTo_DoesnotPickDefaultOrder_IfSkipAndTopAreNotPresent(string oDataQuery)
        {
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();

            var message = new HttpRequestMessage(
                HttpMethod.Get,
                new Uri("http://server/service/Customers?" + oDataQuery)
            );

            var options = new ODataQueryOptions(new ODataQueryContext(model, typeof(Customer), "Customers"), message);
            IQueryable finalQuery = options.ApplyTo(Customers);
            string queryExpression = finalQuery.Expression.ToString();

            Assert.DoesNotContain("OrderBy", queryExpression);
        }

        [Theory]
        [InlineData("$orderby=Name", "OrderBy(p1 => p1.Name)")]
        [InlineData("$orderby=Website", "OrderBy(p1 => p1.Website)")]
        [InlineData("$orderby=Name&$skip=1", "OrderBy(p1 => p1.Name).ThenBy(p1 => p1.CustomerId).Skip(1)")]
        [InlineData("$orderby=Website&$top=1&$skip=1", "OrderBy(p1 => p1.Website).ThenBy(p1 => p1.CustomerId).Skip(1).Take(1)")]
        public void ApplyTo_DoesnotPickDefaultOrder_IfOrderByIsPresent(string oDataQuery, string expectedExpression)
        {
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();

            var message = new HttpRequestMessage(
                HttpMethod.Get,
                new Uri("http://server/service/Customers?" + oDataQuery)
            );

            var options = new ODataQueryOptions(new ODataQueryContext(model, typeof(Customer), "Customers"), message);
            IQueryable finalQuery = options.ApplyTo(Customers);

            string queryExpression = finalQuery.Expression.ToString();
            queryExpression = queryExpression.Substring(queryExpression.IndexOf("OrderBy"));

            Assert.Equal(queryExpression, expectedExpression);
        }

        [Theory]
        [InlineData("$skip=1", true, "OrderBy(p1 => p1.CustomerId).Skip(1)")]
        [InlineData("$skip=1", false, "Skip(1)")]
        [InlineData("$filter=1 eq 1", true, "Where($it => (1 == 1))")]
        [InlineData("$filter=1 eq 1", false, "Where($it => (1 == 1))")]
        public void ApplyTo_Builds_Default_OrderBy_With_Keys(string oDataQuery, bool canUseDefaultOrderBy, string expectedExpression)
        {
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();

            var message = new HttpRequestMessage(
                HttpMethod.Get,
                new Uri("http://server/service/Customers?" + oDataQuery)
            );

            var options = new ODataQueryOptions(new ODataQueryContext(model, typeof(Customer), "Customers"), message);
            IQueryable finalQuery = options.ApplyTo(new Customer[0].AsQueryable(), handleNullPropagation: true, canUseDefaultOrderBy: canUseDefaultOrderBy);

            string queryExpression = finalQuery.Expression.ToString();
            queryExpression = queryExpression.Substring(queryExpression.IndexOf("]") + 2);

            Assert.Equal(queryExpression, expectedExpression);
        }

        [Theory]
        [InlineData("$skip=1", true, "OrderBy(p1 => p1.CustomerId).ThenBy(p1 => p1.Name).ThenBy(p1 => p1.SharePrice).ThenBy(p1 => p1.ShareSymbol).ThenBy(p1 => p1.Website).Skip(1)")]
        [InlineData("$skip=1", false, "Skip(1)")]
        [InlineData("$filter=1 eq 1", true, "Where($it => (1 == 1))")]
        [InlineData("$filter=1 eq 1", false, "Where($it => (1 == 1))")]
        public void ApplyTo_Builds_Default_OrderBy_No_Keys(string oDataQuery, bool canUseDefaultOrderBy, string expectedExpression)
        {
            var model = new ODataModelBuilder().Add_Customer_No_Keys_EntityType().Add_Customers_No_Keys_EntitySet().GetEdmModel();

            var message = new HttpRequestMessage(
                HttpMethod.Get,
                new Uri("http://server/service/Customers?" + oDataQuery)
            );

            var options = new ODataQueryOptions(new ODataQueryContext(model, typeof(Customer), "Customers"), message);
            IQueryable finalQuery = options.ApplyTo(new Customer[0].AsQueryable(), handleNullPropagation: true, canUseDefaultOrderBy: canUseDefaultOrderBy);

            string queryExpression = finalQuery.Expression.ToString();
            queryExpression = queryExpression.Substring(queryExpression.IndexOf("]") + 2);

            Assert.Equal(queryExpression, expectedExpression);
        }

        [Theory]
        [PropertyData("SupportedQueryNames")]
        public void IsSupported_Returns_True_For_All_Supported_Query_Names(string queryName)
        {
            // Arrange & Act & Assert
            Assert.True(ODataQueryOptions.IsSupported(queryName));
        }

        [Theory]
        [PropertyData("UnsupportedQueryNames")]
        public void IsSupported_Returns_False_For_All_Unsupported_Query_Names(string queryName)
        {
            // Arrange & Act & Assert
            Assert.False(ODataQueryOptions.IsSupported(queryName));
        }

        [Fact]
        public void IsSupported_Returns_False_For_Unrecognized_Query_Name()
        {
            // Arrange & Act & Assert
            Assert.False(ODataQueryOptions.IsSupported("$invalidqueryname"));
        }
    }

    public class ODataQueryOptionTest_ComplexModel
    {
        public int A { get; set; }

        public string B { get; set; }
    }

    public class ODataQueryOptionTest_EntityModel
    {
        public int ID { get; set; }

        public string A { get; set; }
    }

    public class ODataQueryOptionTest_EntityModelMultipleKeys
    {
        [Key]
        public int ID1 { get; set; }

        [Key]
        public int ID2 { get; set; }

        public string A { get; set; }
    }
}
