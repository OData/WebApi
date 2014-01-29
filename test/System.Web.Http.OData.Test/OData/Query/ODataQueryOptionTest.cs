// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Builder.TestModels;
using System.Web.Http.OData.Extensions;
using System.Web.Http.OData.Query.Expressions;
using System.Web.Http.OData.Query.Validators;
using System.Web.Http.TestCommon;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Library;
using Microsoft.Data.OData;
using Microsoft.TestCommon;
using Microsoft.TestCommon.Types;

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

        public static TheoryDataSet<string> SystemQueryOptionNames
        {
            get
            {
                return new TheoryDataSet<string>
                {
                    "$orderby",
                    "$filter",
                    "$top",
                    "$skip",
                    "$inlinecount",
                    "$expand",
                    "$select",
                    "$format",
                    "$skiptoken"
                };
            }
        }

        // Used to test modifications to $orderby when $skip or $top are present
        // and the entity type has 2 keys -- CustomerId and Name.
        // Tuple is: query expression, ensureStableOrdering, expected expression
        public static TheoryDataSet<string, bool, string> SkipTopOrderByUsingKeysTestData
        {
            get
            {
                return new TheoryDataSet<string, bool, string>
                {
                    // First key present with $skip, adds 2nd key
                    { "$orderby=CustomerId&$skip=1", true,  "OrderBy($it => $it.CustomerId).ThenBy($it => $it.Name).Skip(1)" },

                    // First key present with $top, adds 2nd key
                    { "$orderby=CustomerId&$top=1", true,  "OrderBy($it => $it.CustomerId).ThenBy($it => $it.Name).Take(1)" },

                    // First key present with $skip and $top, adds 2nd key
                    { "$orderby=CustomerId&$skip=1&$top=2", true,  "OrderBy($it => $it.CustomerId).ThenBy($it => $it.Name).Skip(1).Take(2)" },

                    // First key present, no $skip or $top, no modification
                    { "$orderby=CustomerId", false, "OrderBy($it => $it.CustomerId)" },

                    // First key present, 'ensureStableOrdering' is false, no modification
                    { "$orderby=CustomerId&$skip=1", false, "OrderBy($it => $it.CustomerId).Skip(1)" },

                    // Second key present, adds 1st key after 2nd
                    { "$orderby=Name&$skip=1", true, "OrderBy($it => $it.Name).ThenBy($it => $it.CustomerId).Skip(1)" },

                    // Second key plus 'asc' suffix, adds 1st key and preserves suffix
                    { "$orderby=Name asc&$skip=1", true, "OrderBy($it => $it.Name).ThenBy($it => $it.CustomerId).Skip(1)" },

                    // Second key plus 'desc' suffix, adds 1st key and preserves suffix
                    { "$orderby=Name desc&$skip=1", true, "OrderByDescending($it => $it.Name).ThenBy($it => $it.CustomerId).Skip(1)" },

                    // All keys present, no modification
                    { "$orderby=CustomerId,Name&$skip=1", true,  "OrderBy($it => $it.CustomerId).ThenBy($it => $it.Name).Skip(1)" },

                    // All keys present but in reverse order, no modification
                    { "$orderby=Name,CustomerId&$skip=1", true,  "OrderBy($it => $it.Name).ThenBy($it => $it.CustomerId).Skip(1)" },

                    // First key present but with extraneous whitespace, adds 2nd key
                    { "$orderby= CustomerId &$skip=1", true,  "OrderBy($it => $it.CustomerId).ThenBy($it => $it.Name).Skip(1)" },

                    // All keys present with extraneous whitespace, no modification
                    { "$orderby= \t CustomerId \t , Name \t desc \t &$skip=1", true,  "OrderBy($it => $it.CustomerId).ThenByDescending($it => $it.Name).Skip(1)" },

                    // Ordering on non-key property, adds all keys
                    { "$orderby=Website&$skip=1", true,  "OrderBy($it => $it.Website).ThenBy($it => $it.CustomerId).ThenBy($it => $it.Name).Skip(1)" },
                };
            }
        }

        // Used to test modifications to $orderby when $skip or $top are present
        // and the entity type has a no key properties.
        // Tuple is: query expression, ensureStableOrdering, expected expression
        public static TheoryDataSet<string, bool, string> SkipTopOrderByWithNoKeysTestData
        {
            get
            {
                return new TheoryDataSet<string, bool, string>
                {
                    // Single property present with $skip, adds all remaining in alphabetic order
                    { "$orderby=CustomerId&$skip=1", true,  "OrderBy($it => $it.CustomerId).ThenBy($it => $it.Name).ThenBy($it => $it.SharePrice).ThenBy($it => $it.ShareSymbol).ThenBy($it => $it.Website).Skip(1)" },

                    // Single property present with $top, adds all remaining in alphabetic order
                    { "$orderby=CustomerId&$top=1", true,  "OrderBy($it => $it.CustomerId).ThenBy($it => $it.Name).ThenBy($it => $it.SharePrice).ThenBy($it => $it.ShareSymbol).ThenBy($it => $it.Website).Take(1)" },

                    // Single property present with $skip and $top, adds all remaining in alphabetic order
                    { "$orderby=CustomerId&$skip=1&$top=2", true,  "OrderBy($it => $it.CustomerId).ThenBy($it => $it.Name).ThenBy($it => $it.SharePrice).ThenBy($it => $it.ShareSymbol).ThenBy($it => $it.Website).Skip(1).Take(2)" },

                    // Single property present, no $skip or $top, no modification
                    { "$orderby=SharePrice", false,  "OrderBy($it => $it.SharePrice)" },

                    // Single property present, ensureStableOrdering is false, no modification
                    { "$orderby=SharePrice&$skip=1", false,  "OrderBy($it => $it.SharePrice).Skip(1)" },

                    // All properties present, non-alphabetic order, no modification
                    { "$orderby=Name,SharePrice,CustomerId,Website,ShareSymbol&$skip=1", true,  "OrderBy($it => $it.Name).ThenBy($it => $it.SharePrice).ThenBy($it => $it.CustomerId).ThenBy($it => $it.Website).ThenBy($it => $it.ShareSymbol).Skip(1)" },

                    // All properties present, extraneous whitespace, non-alphabetic order, no modification
                    { "$orderby= \t Name \t , \t SharePrice \t , \t CustomerId \t , \t Website \t , \t ShareSymbol \t &$skip=1", true,  "OrderBy($it => $it.Name).ThenBy($it => $it.SharePrice).ThenBy($it => $it.CustomerId).ThenBy($it => $it.Website).ThenBy($it => $it.ShareSymbol).Skip(1)" },

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
        public void CanExtractQueryOptionsCorrectly()
        {
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();

            var message = new HttpRequestMessage(
                HttpMethod.Get,
                new Uri("http://server/service/Customers/?$filter=Filter&$select=Select&$orderby=OrderBy&$expand=Expand&$top=10&$skip=20&$inlinecount=allpages&$skiptoken=SkipToken")
            );

            var queryOptions = new ODataQueryOptions(new ODataQueryContext(model, typeof(Customer)), message);
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
            Assert.NotNull(queryOptions.SelectExpand);
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

            var queryOptions = new ODataQueryOptions(new ODataQueryContext(model, typeof(Customer)), message);

            // Act & Assert
            Assert.ThrowsArgumentNull(() => queryOptions.ApplyTo(null), "query");
        }

        [Fact]
        public void ApplyTo_With_QuerySettings_Throws_With_Null_Queryable()
        {
            // Arrange
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();

            var message = new HttpRequestMessage(
                HttpMethod.Get,
                new Uri("http://server/service/Customers/?$filter=Filter&$select=Select&$orderby=OrderBy&$expand=Expand&$top=10&$skip=20&$inlinecount=allpages&$skiptoken=SkipToken")
            );

            var queryOptions = new ODataQueryOptions(new ODataQueryContext(model, typeof(Customer)), message);

            // Act & Assert
            Assert.ThrowsArgumentNull(() => queryOptions.ApplyTo(null, new ODataQuerySettings()), "query");
        }

        [Fact]
        public void ApplyTo_Throws_With_Null_QuerySettings()
        {
            // Arrange
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();

            var message = new HttpRequestMessage(
                HttpMethod.Get,
                new Uri("http://server/service/Customers/?$filter=Filter&$select=Select&$orderby=OrderBy&$expand=Expand&$top=10&$skip=20&$inlinecount=allpages&$skiptoken=SkipToken")
            );

            var queryOptions = new ODataQueryOptions(new ODataQueryContext(model, typeof(Customer)), message);

            // Act & Assert
            Assert.ThrowsArgumentNull(() => queryOptions.ApplyTo(new Customer[0].AsQueryable(), null), "querySettings");
        }

        [Theory]
        [PropertyData("SkipTopOrderByUsingKeysTestData")]
        public void ApplyTo_Adds_Missing_Keys_To_OrderBy(string oDataQuery, bool ensureStableOrdering, string expectedExpression)
        {
            // Arrange
            var model = new ODataModelBuilder()
                .Add_Customers_With_Keys_EntitySet(c => new { c.CustomerId, c.Name }).GetEdmModel();

            var message = new HttpRequestMessage(
                HttpMethod.Get,
                new Uri("http://server/service/Customers?" + oDataQuery)
            );

            var queryOptions = new ODataQueryOptions(new ODataQueryContext(model, typeof(Customer)), message);
            ODataQuerySettings querySettings = new ODataQuerySettings
            {
                EnsureStableOrdering = ensureStableOrdering,
            };

            // Act
            IQueryable finalQuery = queryOptions.ApplyTo(new Customer[0].AsQueryable(), querySettings);

            // Assert
            string queryExpression = ExpressionStringBuilder.ToString(finalQuery.Expression);
            queryExpression = queryExpression.Substring(queryExpression.IndexOf("]") + 2);

            Assert.Equal(queryExpression, expectedExpression);
        }

        [Theory]
        [PropertyData("SkipTopOrderByWithNoKeysTestData")]
        public void ApplyTo_Adds_Missing_NonKey_Properties_To_OrderBy(string oDataQuery, bool ensureStableOrdering, string expectedExpression)
        {
            // Arrange
            var model = new ODataModelBuilder()
                .Add_Customers_No_Keys_EntitySet().GetEdmModel();

            var message = new HttpRequestMessage(
                HttpMethod.Get,
                new Uri("http://server/service/Customers?" + oDataQuery)
            );

            var queryOptions = new ODataQueryOptions(new ODataQueryContext(model, typeof(Customer)), message);
            ODataQuerySettings querySettings = new ODataQuerySettings
            {
                EnsureStableOrdering = ensureStableOrdering
            };

            // Act
            IQueryable finalQuery = queryOptions.ApplyTo(new Customer[0].AsQueryable(), querySettings);

            // Assert
            string queryExpression = ExpressionStringBuilder.ToString(finalQuery.Expression);
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
            var queryOptions = new ODataQueryOptions(new ODataQueryContext(model, typeof(Customer)), message);
            OrderByQueryOption originalOption = queryOptions.OrderBy;
            ODataQuerySettings querySettings = new ODataQuerySettings();

            IQueryable finalQuery = queryOptions.ApplyTo(new Customer[0].AsQueryable(), querySettings);

            // Assert
            Assert.ReferenceEquals(originalOption, queryOptions.OrderBy);
        }

        [Fact]
        public void ApplyTo_SetsRequestSelectExpandClause_IfSelectExpandIsNotNull()
        {
            // Arrange
            var model = new ODataModelBuilder().Add_Customers_EntitySet().GetEdmModel();
            var request = new HttpRequestMessage(HttpMethod.Get, new Uri("http://server/service/Customers?$select=Name"));
            ODataQueryOptions queryOptions = new ODataQueryOptions(new ODataQueryContext(model, typeof(Customer)), request);

            // Act
            queryOptions.ApplyTo(Enumerable.Empty<Customer>().AsQueryable());

            // Assert
            Assert.NotNull(request.ODataProperties().SelectExpandClause);
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
            var entityType = queryOptions.Context.ElementClrType;
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
                    new ODataQueryOptions(new ODataQueryContext(model, typeof(Customer)), message));
            }
            else
            {
                var queryOptions = new ODataQueryOptions(new ODataQueryContext(model, typeof(Customer)), message);

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
            var queryOptions = new ODataQueryOptions(new ODataQueryContext(model, typeof(Customer)), message);

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

            var option = new ODataQueryOptions(new ODataQueryContext(model, typeof(Customer)), message);
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

            var options = new ODataQueryOptions(new ODataQueryContext(model, typeof(Customer)), message);
            Assert.Throws<ODataException>(() =>
                 options.ApplyTo(Customers),
                 "Failed to convert 'NotANumber' to an integer.");

            message = new HttpRequestMessage(
                HttpMethod.Get,
                new Uri("http://server/service/Customers?$top=''")
            );

            options = new ODataQueryOptions(new ODataQueryContext(model, typeof(Customer)), message);
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

            var options = new ODataQueryOptions(new ODataQueryContext(model, typeof(Customer)), message);
            Assert.Throws<ODataException>(() =>
                 options.ApplyTo(Customers),
                 "Failed to convert 'NotANumber' to an integer.");

            message = new HttpRequestMessage(
                HttpMethod.Get,
                new Uri("http://server/service/Customers?$skip=''")
            );

            options = new ODataQueryOptions(new ODataQueryContext(model, typeof(Customer)), message);
            Assert.Throws<ODataException>(() =>
                 options.ApplyTo(Customers),
                 "Failed to convert '''' to an integer.");
        }

        [Theory]
        [InlineData("$skip=1", typeof(ODataQueryOptionTest_ComplexModel), "OrderBy($it => $it.A).ThenBy($it => $it.B).Skip(1)")]
        [InlineData("$skip=1", typeof(ODataQueryOptionTest_EntityModel), "OrderBy($it => $it.ID).Skip(1)")]
        [InlineData("$skip=1", typeof(ODataQueryOptionTest_EntityModelMultipleKeys), "OrderBy($it => $it.ID1).ThenBy($it => $it.ID2).Skip(1)")]
        [InlineData("$top=1", typeof(ODataQueryOptionTest_ComplexModel), "OrderBy($it => $it.A).ThenBy($it => $it.B).Take(1)")]
        [InlineData("$top=1", typeof(ODataQueryOptionTest_EntityModel), "OrderBy($it => $it.ID).Take(1)")]
        [InlineData("$top=1", typeof(ODataQueryOptionTest_EntityModelMultipleKeys), "OrderBy($it => $it.ID1).ThenBy($it => $it.ID2).Take(1)")]
        [InlineData("$skip=1&$top=1", typeof(ODataQueryOptionTest_ComplexModel), "OrderBy($it => $it.A).ThenBy($it => $it.B).Skip(1).Take(1)")]
        [InlineData("$skip=1&$top=1", typeof(ODataQueryOptionTest_EntityModel), "OrderBy($it => $it.ID).Skip(1).Take(1)")]
        [InlineData("$skip=1&$top=1", typeof(ODataQueryOptionTest_EntityModelMultipleKeys), "OrderBy($it => $it.ID1).ThenBy($it => $it.ID2).Skip(1).Take(1)")]
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

            var options = new ODataQueryOptions(new ODataQueryContext(model, elementType), message);
            IQueryable finalQuery = options.ApplyTo(query);

            string queryExpression = ExpressionStringBuilder.ToString(finalQuery.Expression);
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

            var options = new ODataQueryOptions(new ODataQueryContext(model, typeof(Customer)), message);
            IQueryable finalQuery = options.ApplyTo(Customers);
            string queryExpression = finalQuery.Expression.ToString();

            Assert.DoesNotContain("OrderBy", queryExpression);
        }

        [Theory]
        [InlineData("$orderby=Name", "OrderBy($it => $it.Name)")]
        [InlineData("$orderby=Website", "OrderBy($it => $it.Website)")]
        [InlineData("$orderby=Name&$skip=1", "OrderBy($it => $it.Name).ThenBy($it => $it.CustomerId).Skip(1)")]
        [InlineData("$orderby=Website&$top=1&$skip=1", "OrderBy($it => $it.Website).ThenBy($it => $it.CustomerId).Skip(1).Take(1)")]
        public void ApplyTo_DoesnotPickDefaultOrder_IfOrderByIsPresent(string oDataQuery, string expectedExpression)
        {
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();

            var message = new HttpRequestMessage(
                HttpMethod.Get,
                new Uri("http://server/service/Customers?" + oDataQuery)
            );

            var options = new ODataQueryOptions(new ODataQueryContext(model, typeof(Customer)), message);
            IQueryable finalQuery = options.ApplyTo(Customers);

            string queryExpression = ExpressionStringBuilder.ToString(finalQuery.Expression);
            queryExpression = queryExpression.Substring(queryExpression.IndexOf("OrderBy"));

            Assert.Equal(queryExpression, expectedExpression);
        }

        [Theory]
        [InlineData("$skip=1", true, "OrderBy($it => $it.CustomerId).Skip(1)")]
        [InlineData("$skip=1", false, "Skip(1)")]
        [InlineData("$filter=1 eq 1", true, "Where($it => (1 == 1))")]
        [InlineData("$filter=1 eq 1", false, "Where($it => (1 == 1))")]
        public void ApplyTo_Builds_Default_OrderBy_With_Keys(string oDataQuery, bool ensureStableOrdering, string expectedExpression)
        {
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();

            var message = new HttpRequestMessage(
                HttpMethod.Get,
                new Uri("http://server/service/Customers?" + oDataQuery)
            );

            var options = new ODataQueryOptions(new ODataQueryContext(model, typeof(Customer)), message);
            ODataQuerySettings querySettings = new ODataQuerySettings
            {
                EnsureStableOrdering = ensureStableOrdering
            };

            IQueryable finalQuery = options.ApplyTo(new Customer[0].AsQueryable(), querySettings);

            string queryExpression = ExpressionStringBuilder.ToString(finalQuery.Expression);
            queryExpression = queryExpression.Substring(queryExpression.IndexOf("]") + 2);

            Assert.Equal(queryExpression, expectedExpression);
        }

        [Theory]
        [InlineData("$skip=1", true, "OrderBy($it => $it.CustomerId).ThenBy($it => $it.Name).ThenBy($it => $it.SharePrice).ThenBy($it => $it.ShareSymbol).ThenBy($it => $it.Website).Skip(1)")]
        [InlineData("$skip=1", false, "Skip(1)")]
        [InlineData("$filter=1 eq 1", true, "Where($it => (1 == 1))")]
        [InlineData("$filter=1 eq 1", false, "Where($it => (1 == 1))")]
        public void ApplyTo_Builds_Default_OrderBy_No_Keys(string oDataQuery, bool ensureStableOrdering, string expectedExpression)
        {
            var model = new ODataModelBuilder().Add_Customer_No_Keys_EntityType().Add_Customers_No_Keys_EntitySet().GetEdmModel();

            var message = new HttpRequestMessage(
                HttpMethod.Get,
                new Uri("http://server/service/Customers?" + oDataQuery)
            );

            var options = new ODataQueryOptions(new ODataQueryContext(model, typeof(Customer)), message);
            ODataQuerySettings querySettings = new ODataQuerySettings
            {
                EnsureStableOrdering = ensureStableOrdering
            };
            IQueryable finalQuery = options.ApplyTo(new Customer[0].AsQueryable(), querySettings);

            string queryExpression = ExpressionStringBuilder.ToString(finalQuery.Expression);
            queryExpression = queryExpression.Substring(queryExpression.IndexOf("]") + 2);

            Assert.Equal(queryExpression, expectedExpression);
        }

        [Fact]
        public void Validate_ThrowsValidationErrors_ForOrderBy()
        {
            // Arrange
            var model = new ODataModelBuilder().Add_Customer_EntityType().GetEdmModel();

            var message = new HttpRequestMessage(
                HttpMethod.Get,
                new Uri("http://server/service/Customers?$orderby=CustomerId,Name")
            );

            var options = new ODataQueryOptions(new ODataQueryContext(model, typeof(Customer)), message);
            ODataValidationSettings validationSettings = new ODataValidationSettings { MaxOrderByNodeCount = 1 };

            // Act & Assert
            Assert.Throws<ODataException>(() => options.Validate(validationSettings),
                "The number of clauses in $orderby query option exceeded the maximum number allowed. The maximum number of $orderby clauses allowed is 1.");
        }

        [Theory]
        [PropertyData("SystemQueryOptionNames")]
        public void IsSystemQueryOption_Returns_True_For_All_Supported_Query_Names(string queryName)
        {
            // Arrange & Act & Assert
            Assert.True(ODataQueryOptions.IsSystemQueryOption(queryName));
        }

        [Fact]
        public void IsSystemQueryOption_Returns_False_For_Unrecognized_Query_Name()
        {
            // Arrange & Act & Assert
            Assert.False(ODataQueryOptions.IsSystemQueryOption("$invalidqueryname"));
        }

        [Theory]
        [InlineData(1, true)]
        [InlineData(2, true)]
        [InlineData(4, false)]
        [InlineData(8, false)]
        public void LimitResults_LimitsResults(int limit, bool resultsLimitedExpected)
        {
            IQueryable queryable = new List<Customer>() {
                new Customer() { CustomerId = 0 }, 
                new Customer() { CustomerId = 1 },
                new Customer() { CustomerId = 2 },
                new Customer() { CustomerId = 3 }
            }.AsQueryable();
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();
            var context = new ODataQueryContext(model, typeof(Customer));

            bool resultsLimited;
            IQueryable<Customer> result = ODataQueryOptions.LimitResults(queryable, limit, out resultsLimited) as IQueryable<Customer>;

            Assert.Equal(Math.Min(limit, 4), result.Count());
            Assert.Equal(resultsLimitedExpected, resultsLimited);
        }

        [Theory]
        [InlineData("http://localhost/Customers", 10, "http://localhost/Customers?$skip=10")]
        [InlineData("http://localhost/Customers?$filter=Age ge 18", 10, "http://localhost/Customers?$filter=Age%20ge%2018&$skip=10")]
        [InlineData("http://localhost/Customers?$top=20", 10, "http://localhost/Customers?$top=10&$skip=10")]
        [InlineData("http://localhost/Customers?$skip=5&$top=10", 2, "http://localhost/Customers?$top=8&$skip=7")]
        [InlineData("http://localhost/Customers?$filter=Age ge 18&$orderby=Name&$top=11&$skip=6", 10, "http://localhost/Customers?$filter=Age%20ge%2018&$orderby=Name&$top=1&$skip=16")]
        [InlineData("http://localhost/Customers?testkey%23%2B%3D%3F%26=testvalue%23%2B%3D%3F%26", 10, "http://localhost/Customers?testkey%23%2B%3D%3F%26=testvalue%23%2B%3D%3F%26&$skip=10")]
        public void GetNextPageLink_GetsNextPageLink(string requestUri, int pageSize, string nextPageUri)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            Uri nextPageLink = ODataQueryOptions.GetNextPageLink(request, pageSize);

            Assert.Equal(nextPageUri, nextPageLink.AbsoluteUri);
        }

        [Fact]
        public void GetNextPageLink_ThatTakesUri_GetsNextPageLink()
        {
            Uri nextPageLink = ODataQueryOptions.GetNextPageLink(new Uri("http://localhost/Customers?$filter=Age ge 18"), 10);
            Assert.Equal("http://localhost/Customers?$filter=Age%20ge%2018&$skip=10", nextPageLink.AbsoluteUri);
        }

        [Fact]
        public void CanTurnOffAllValidation()
        {
            // Arrange
            HttpRequestMessage message = new HttpRequestMessage(
                HttpMethod.Get,
                new Uri("http://localhost/?$filter=Name eq 'abc'")
            );

            ODataQueryContext context = ValidationTestHelper.CreateCustomerContext();
            ODataQueryOptions option = new ODataQueryOptions(context, message);
            ODataValidationSettings settings = new ODataValidationSettings()
            {
                AllowedQueryOptions = AllowedQueryOptions.OrderBy
            };

            // Act & Assert
            Assert.Throws<ODataException>(() => option.Validate(settings),
                "Query option 'Filter' is not allowed. To allow it, set the 'AllowedQueryOptions' property on EnableQueryAttribute or QueryValidationSettings.");

            option.Validator = null;
            Assert.DoesNotThrow(() => option.Validate(settings));

        }

        public static TheoryDataSet<IQueryable, string, object> Querying_Primitive_Collections_Data
        {
            get
            {
                IQueryable<int> e = Enumerable.Range(1, 9).AsQueryable();
                return new TheoryDataSet<IQueryable, string, object>
                {
                    { e.Select(i => (short)i), "$filter=$it ge 5&$orderby=$it desc&$skip=3&$top=1", (short)6 },
                    { e.Select(i => i), "$filter=$it ge 5&$orderby=$it desc&$skip=3&$top=1", 6 },
                    { e.Select(i => (long)i), "$filter=$it ge 5&$orderby=$it desc&$skip=3&$top=1", (long)6 },
                    { e.Select(i => (ushort)i), "$filter=$it ge 5&$orderby=$it desc&$skip=3&$top=1", (ushort)6 },
                    { e.Select(i => (uint)i), "$filter=$it ge 5&$orderby=$it desc&$skip=3&$top=1", (uint)6 },
                    { e.Select(i => (ulong)i), "$filter=$it ge 5&$orderby=$it desc&$skip=3&$top=1", (ulong)6 },
                    { e.Select(i => (float)i), "$filter=$it ge 5&$orderby=$it desc&$skip=3&$top=1", (float)6 },
                    { e.Select(i => (double)i), "$filter=$it ge 5&$orderby=$it desc&$skip=3&$top=1", (double)6 },
                    { e.Select(i => (decimal)i), "$filter=$it ge 5&$orderby=$it desc&$skip=3&$top=1", (decimal)6 },
                    { e.Select(i => (SimpleEnum)(i%3)), "$filter=$it eq 'First'&$orderby=$it desc&$skip=1&$top=1", SimpleEnum.First },
                    { e.Select(i => new DateTime(i, 1, 1)), "$filter=year($it) ge 5&$orderby=$it desc&$skip=3&$top=1", new DateTime(year: 6, month: 1, day: 1) },
                    { e.Select(i => i.ToString()), "$filter=$it ge '5'&$orderby=$it desc&$skip=3&$top=1", "6" },
                  
                    { e.Select(i => (i % 2 != 0 ? null : (short?)i)), "$filter=$it ge 5&$orderby=$it desc&$skip=1&$top=1", (short?)6 },
                    { e.Select(i => (i % 2 != 0 ? null : (int?)i)), "$filter=$it ge 5&$orderby=$it desc&$skip=1&$top=1", (int?)6 },
                    { e.Select(i => (i % 2 != 0 ? null : (long?)i)), "$filter=$it ge 5&$orderby=$it desc&$skip=1&$top=1", (long?)6 },
                    { e.Select(i => (i % 2 != 0 ? null : (ushort?)i)), "$filter=$it ge 5&$orderby=$it desc&$skip=1&$top=1", (ushort?)6 },
                    { e.Select(i => (i % 2 != 0 ? null : (uint?)i)), "$filter=$it ge 5&$orderby=$it desc&$skip=1&$top=1", (uint?)6 },
                    { e.Select(i => (i % 2 != 0 ? null : (ulong?)i)), "$filter=$it ge 5&$orderby=$it desc&$skip=1&$top=1", (ulong?)6 },
                    { e.Select(i => (i % 2 != 0 ? null : (float?)i)), "$filter=$it ge 5&$orderby=$it desc&$skip=1&$top=1", (float?)6 },
                    { e.Select(i => (i % 2 != 0 ? null : (double?)i)), "$filter=$it ge 5&$orderby=$it desc&$skip=1&$top=1", (double?)6 },
                    { e.Select(i => (i % 2 != 0 ? null : (decimal?)i)), "$filter=$it ge 5&$orderby=$it desc&$skip=1&$top=1", (decimal?)6 },
                    { e.Select(i => (SimpleEnum?)null), "$filter=$it eq null&$orderby=$it desc&$skip=1&$top=1", null },
                };
            }
        }

        [Theory]
        [PropertyData("Querying_Primitive_Collections_Data")]
        public void Querying_Primitive_Collections(IQueryable queryable, string query, object result)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/?" + query);
            ODataQueryContext context = new ODataQueryContext(EdmCoreModel.Instance, queryable.ElementType);
            ODataQueryOptions options = new ODataQueryOptions(context, request);

            queryable = options.ApplyTo(queryable);

            IEnumerator enumerator = queryable.GetEnumerator();
            Assert.True(enumerator.MoveNext());
            Assert.Equal(result, enumerator.Current);
        }

        [Fact]
        public void ODataQueryOptions_IgnoresUnknownOperatorStartingWithDollar()
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/?$filter=$it eq 6&$unknown=value");
            ODataQueryContext context = new ODataQueryContext(EdmCoreModel.Instance, typeof(int));
            ODataQueryOptions options = new ODataQueryOptions(context, request);

            var queryable = options.ApplyTo(Enumerable.Range(0, 10).AsQueryable());

            IEnumerator enumerator = queryable.GetEnumerator();
            Assert.True(enumerator.MoveNext());
            Assert.Equal(6, enumerator.Current);
        }

        [Fact]
        public void ApplyTo_Entity_ThrowsArgumentNull_Entity()
        {
            ODataQueryContext context = new ODataQueryContext(EdmCoreModel.Instance, typeof(int));
            ODataQueryOptions queryOptions = new ODataQueryOptions(context, new HttpRequestMessage());

            Assert.ThrowsArgumentNull(
                () => queryOptions.ApplyTo(entity: null, querySettings: new ODataQuerySettings()),
                "entity");
        }

        [Fact]
        public void ApplyTo_IgnoresInlineCount_IfRequestAlreadyHasInlineCount()
        {
            // Arrange
            long count = 42;
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/?$inlinecount=allpages");
            ODataQueryContext context = new ODataQueryContext(EdmCoreModel.Instance, typeof(int));
            ODataQueryOptions options = new ODataQueryOptions(context, request);
            request.ODataProperties().TotalCount = count;

            // Act
            options.ApplyTo(Enumerable.Empty<int>().AsQueryable());

            // Assert
            Assert.Equal(count, request.ODataProperties().TotalCount);
        }

        [Fact]
        public void ApplyTo_Entity_ThrowsArgumentNull_QuerySettings()
        {
            ODataQueryContext context = new ODataQueryContext(EdmCoreModel.Instance, typeof(int));
            ODataQueryOptions queryOptions = new ODataQueryOptions(context, new HttpRequestMessage());

            Assert.ThrowsArgumentNull(
                () => queryOptions.ApplyTo(entity: 42, querySettings: null),
                "querySettings");
        }

        [Theory]
        [InlineData("$filter=ID eq 1")]
        [InlineData("$orderby=ID")]
        [InlineData("$inlinecount=allpages")]
        [InlineData("$skip=1")]
        [InlineData("$top=0")]
        public void ApplyTo_Entity_ThrowsInvalidOperation_IfNonSelectExpand(string parameter)
        {
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            model.Model.SetAnnotationValue(model.Customer, new ClrTypeAnnotation(typeof(Customer)));
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost?" + parameter);
            ODataQueryContext context = new ODataQueryContext(model.Model, typeof(Customer));
            ODataQueryOptions queryOptions = new ODataQueryOptions(context, request);

            Assert.Throws<InvalidOperationException>(
                () => queryOptions.ApplyTo(42, new ODataQuerySettings()),
                "The requested resource is not a collection. Query options $filter, $orderby, $inlinecount, $skip, and $top can be applied only on collections.");
        }

        [Fact]
        public void ApplyTo_DoesnotCalculateNextPageLink_IfRequestAlreadyHasNextPageLink()
        {
            // Arrange
            Uri nextPageLink = new Uri("http://localhost/nextpagelink");
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/");
            ODataQueryContext context = new ODataQueryContext(EdmCoreModel.Instance, typeof(int));
            ODataQueryOptions options = new ODataQueryOptions(context, request);
            request.ODataProperties().NextLink = nextPageLink;

            // Act
            IQueryable result = options.ApplyTo(Enumerable.Range(0, 100).AsQueryable(), new ODataQuerySettings { PageSize = 1 });

            // Assert
            Assert.Equal(nextPageLink, request.ODataProperties().NextLink);
            Assert.Equal(1, (result as IQueryable<int>).Count());
        }

        [Fact]
        public void ODataQueryOptions_WithUnTypedContext_CanBeBuilt()
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            ODataQueryContext context = new ODataQueryContext(model.Model, model.Customer);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get,
                "http://localhost/?$filter=Id eq 42&$orderby=Id&$skip=42&$top=42&$inlinecount=allpages&$select=Id&$expand=Orders");

            // Act
            ODataQueryOptions queryOptions = new ODataQueryOptions(context, request);

            // Assert
            Assert.NotNull(queryOptions.Filter);
            Assert.NotNull(queryOptions.OrderBy);
            Assert.NotNull(queryOptions.Skip);
            Assert.NotNull(queryOptions.Top);
            Assert.NotNull(queryOptions.SelectExpand);
            Assert.NotNull(queryOptions.InlineCount);
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
