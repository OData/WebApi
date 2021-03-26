// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if NETCORE
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Interfaces;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Builder.TestModels;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.AspNet.OData.Test.Common.Types;
using Microsoft.AspNet.OData.Test.Query.Expressions;
using Microsoft.AspNet.OData.Test.Query.Validators;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Newtonsoft.Json.Linq;
using Xunit;
#else
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Interfaces;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Builder.TestModels;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.AspNet.OData.Test.Common.Types;
using Microsoft.AspNet.OData.Test.Query.Expressions;
using Microsoft.AspNet.OData.Test.Query.Validators;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Newtonsoft.Json.Linq;
using Xunit;
#endif
namespace Microsoft.AspNet.OData.Test.Query
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
                     { "$skip", "12" },

                     { "$apply", null },
                     { "$apply", "" },
                     { "$apply", " " },
                     { "$apply", "aggregate(SharePrice mul CustomerId with sum as Name)" },
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
                    "$count",
                    "$expand",
                    "$select",
                    "$format",
                    "$skiptoken",
                    "$deltatoken"
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
            var request = RequestFactory.Create();
            ExceptionAssert.Throws<ArgumentNullException>(
                () => new ODataQueryOptions(null, request)
            );
        }

        [Fact]
        public void ConstructorNullRequestThrows()
        {
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();
            ExceptionAssert.Throws<ArgumentNullException>(
                () => new ODataQueryOptions(new ODataQueryContext(model, typeof(Customer)), null)
            );
        }

        [Theory]
        [InlineData("$filter")]
        [InlineData("$count")]
        [InlineData("$orderby")]
        [InlineData("$skip")]
        [InlineData("$top")]
        public void ConstructorThrowsIfEmptyQueryOptionValue(string queryName)
        {
            // Arrange
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();
            var message = RequestFactory.Create(
                   HttpMethod.Get,
                   "http://server/service/Customers/?" + queryName + "="
               );

            // Act & Assert
            ExceptionAssert.Throws<ODataException>(() => new ODataQueryOptions(new ODataQueryContext(model, typeof(Customer)), message),
                "The value for OData query '" + queryName + "' cannot be empty.");
        }

        [Fact]
        public void CanExtractQueryOptionsCorrectly()
        {
            // Arrange
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();

            var message = RequestFactory.Create(
                HttpMethod.Get,
                "http://server/service/Customers/?$filter=Filter&$select=Select&$orderby=OrderBy&$expand=Expand&$top=10&$skip=20&$count=true&$skiptoken=SkipToken&$deltatoken=DeltaToken"
            );

            // Act
            IODataQueryOptions queryOptions = new ODataQueryOptions(new ODataQueryContext(model, typeof(Customer)), message);

            // Assert
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
            Assert.Equal("true", queryOptions.RawValues.Count);
            Assert.Equal("SkipToken", queryOptions.RawValues.SkipToken);
            Assert.Equal("DeltaToken", queryOptions.RawValues.DeltaToken);
        }

        [Theory]
        [InlineData(" $filter=Filter& $select=Select& $orderby=OrderBy& $expand=Expand& $top=10& $skip=20& $count=true& $skiptoken=SkipToken& $deltatoken=DeltaToken")]
        [InlineData("%20$filter=Filter&%20$select=Select&%20$orderby=OrderBy&%20$expand=Expand&%20$top=10&%20$skip=20&%20$count=true&%20$skiptoken=SkipToken&%20$deltatoken=DeltaToken")]
        [InlineData("$filter =Filter&$select =Select&$orderby =OrderBy&$expand =Expand&$top =10&$skip =20&$count =true&$skiptoken =SkipToken&$deltatoken =DeltaToken")]
        [InlineData("$filter%20=Filter&$select%20=Select&$orderby%20=OrderBy&$expand%20=Expand&$top%20=10&$skip%20=20&$count%20=true&$skiptoken%20=SkipToken&$deltatoken%20=DeltaToken")]
        [InlineData(" $filter =Filter& $select =Select& $orderby =OrderBy& $expand =Expand& $top =10& $skip =20& $count =true& $skiptoken =SkipToken& $deltatoken =DeltaToken")]
        public void CanExtractQueryOptionsWithExtraSpacesCorrectly(string clause)
        {
            // Arrange
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();

            var message = RequestFactory.Create(
                HttpMethod.Get,
                "http://server/service/Customers/?"+ clause
            );

            // Act
            IODataQueryOptions queryOptions = new ODataQueryOptions(new ODataQueryContext(model, typeof(Customer)), message);

            // Assert
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
            Assert.Equal("true", queryOptions.RawValues.Count);
            Assert.Equal("SkipToken", queryOptions.RawValues.SkipToken);
            Assert.Equal("DeltaToken", queryOptions.RawValues.DeltaToken);
        }

        [Fact]
        public void ApplyTo_Throws_With_Null_Queryable()
        {
            // Arrange
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();

            var message = RequestFactory.Create(
                HttpMethod.Get,
                "http://server/service/Customers/?$filter=Filter&$select=Select&$orderby=OrderBy&$expand=Expand&$top=10&$skip=20&$count=true&$skiptoken=SkipToken&$deltatoken=DeltaToken"
            );

            IODataQueryOptions queryOptions = new ODataQueryOptions(new ODataQueryContext(model, typeof(Customer)), message);

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => queryOptions.ApplyTo(null), "query");
        }

        [Fact]
        public void ApplyTo_With_QuerySettings_Throws_With_Null_Queryable()
        {
            // Arrange
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();

            var request = RequestFactory.Create(
                HttpMethod.Get,
                "http://server/service/Customers/?$filter=Filter&$select=Select&$orderby=OrderBy&$expand=Expand&$top=10&$skip=20&$count=true&$skiptoken=SkipToken&$deltatoken=DeltaToken"
            );

            IODataQueryOptions queryOptions = new ODataQueryOptions(new ODataQueryContext(model, typeof(Customer)), request);

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => queryOptions.ApplyTo(null, new ODataQuerySettings()), "query");
        }

        [Fact]
        public void ApplyTo_Throws_With_Null_QuerySettings()
        {
            // Arrange
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();

            var request = RequestFactory.Create(
                HttpMethod.Get,
                "http://server/service/Customers/?$filter=Filter&$select=Select&$orderby=OrderBy&$expand=Expand&$top=10&$skip=20&$count=true&$skiptoken=SkipToken&$deltatoken=DeltaToken"
            );

            IODataQueryOptions queryOptions = new ODataQueryOptions(new ODataQueryContext(model, typeof(Customer)), request);

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => queryOptions.ApplyTo(new Customer[0].AsQueryable(), null), "querySettings");
        }

        [Theory]
        [MemberData(nameof(SkipTopOrderByUsingKeysTestData))]
        public void ApplyTo_Adds_Missing_Keys_To_OrderBy(string oDataQuery, bool ensureStableOrdering, string expectedExpression)
        {
            // Arrange
            var model = new ODataModelBuilder()
                .Add_Customers_With_Keys_EntitySet(c => new { c.CustomerId, c.Name }).GetEdmModel();

            var request = RequestFactory.Create(
                HttpMethod.Get,
                "http://server/service/Customers?" + oDataQuery
            );

            IODataQueryOptions queryOptions = new ODataQueryOptions(new ODataQueryContext(model, typeof(Customer)), request);

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
        [MemberData(nameof(SkipTopOrderByWithNoKeysTestData))]
        public void ApplyTo_Adds_Missing_NonKey_Properties_To_OrderBy(string oDataQuery, bool ensureStableOrdering, string expectedExpression)
        {
            // Arrange
            var model = ODataModelBuilderMocks.GetModelBuilderMock<ODataModelBuilder>()
                .Add_Customers_No_Keys_EntitySet().GetEdmModel();

            var request = RequestFactory.Create(
                HttpMethod.Get,
                "http://server/service/Customers?" + oDataQuery
            );

            IODataQueryOptions queryOptions = new ODataQueryOptions(new ODataQueryContext(model, typeof(Customer)), request);

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
            var model = ODataModelBuilderMocks.GetModelBuilderMock<ODataModelBuilder>()
                            .Add_Customers_No_Keys_EntitySet().GetEdmModel();

            var request = RequestFactory.Create(
                HttpMethod.Get,
                "http://server/service/Customers?$orderby=Name"
            );

            // Act
            IODataQueryOptions queryOptions = new ODataQueryOptions(new ODataQueryContext(model, typeof(Customer)), request);
            OrderByQueryOption originalOption = queryOptions.OrderBy;
            ODataQuerySettings querySettings = new ODataQuerySettings();

            IQueryable finalQuery = queryOptions.ApplyTo(new Customer[0].AsQueryable(), querySettings);

            // Assert
            Assert.Same(originalOption, queryOptions.OrderBy);
        }

#if !NETCORE // TODO #939: Enable this test on AspNetCore.
        [Fact]
        public void ApplyTo_SetsRequestSelectExpandClause_IfSelectExpandIsNotNull()
        {
            // Arrange
            var model = new ODataModelBuilder().Add_Customers_EntitySet().GetEdmModel();
            var request = RequestFactory.Create(HttpMethod.Get, "http://server/service/Customers?$select=Name");
            ODataQueryOptions queryOptions = new ODataQueryOptions(new ODataQueryContext(model, typeof(Customer)), request);

            // Act
            queryOptions.ApplyTo(Enumerable.Empty<Customer>().AsQueryable());

            // Assert
            Assert.NotNull(request.ODataProperties().SelectExpandClause);
        }
#endif

        [Fact]
        [Trait("ODataQueryOption", "Can bind a typed ODataQueryOption to the request uri without any query")]
        public void ContextPropertyGetter()
        {
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();

            var request = RequestFactory.Create(
                HttpMethod.Get,
                "http://server/service/Customers"
            );
            IODataQueryOptions queryOptions = new ODataQueryOptions(new ODataQueryContext(model, typeof(Customer)), request);
            var entityType = queryOptions.Context.ElementClrType;
            Assert.NotNull(entityType);
            Assert.Equal(typeof(Customer).FullName, entityType.Namespace + "." + entityType.Name);
        }

        [Theory]
        [MemberData(nameof(QueryTestData))]
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

            var request = RequestFactory.Create(HttpMethod.Get, uri);

            // Act && Assert
            if (String.IsNullOrWhiteSpace(queryValue))
            {
                ExceptionAssert.Throws<ODataException>(() =>
                    new ODataQueryOptions(new ODataQueryContext(model, typeof(Customer)), request));
            }
            else
            {
                IODataQueryOptions queryOptions = new ODataQueryOptions(new ODataQueryContext(model, typeof(Customer)), request);

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
                else if (queryName == "$apply")
                {
                    Assert.Equal(queryValue, queryOptions.RawValues.Apply);
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
            var message = RequestFactory.Create(HttpMethod.Get, uri);

            // Act
            IODataQueryOptions queryOptions = new ODataQueryOptions(new ODataQueryContext(model, typeof(Customer)), message);

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

            var message = RequestFactory.Create(
                HttpMethod.Get,
                "http://server/service/Customers?$orderby=UnknownProperty"
            );

            IODataQueryOptions option = new ODataQueryOptions(new ODataQueryContext(model, typeof(Customer)), message);
            ExceptionAssert.Throws<ODataException>(() =>
            {
                option.ApplyTo(new List<Customer>().AsQueryable());
            });
        }

        [Fact]
        public void CannotConvertBadTopQueryThrows()
        {
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();

            var message = RequestFactory.Create(
                HttpMethod.Get,
                "http://server/service/Customers?$top=NotANumber"
            );

            IODataQueryOptions options = new ODataQueryOptions(new ODataQueryContext(model, typeof(Customer)), message);
            ExceptionAssert.Throws<ODataException>(() =>
                 options.ApplyTo(Customers),
                 "Invalid value 'NotANumber' for $top query option found. " +
                 "The $top query option requires a non-negative integer value.");

            message = RequestFactory.Create(
                HttpMethod.Get,
                "http://server/service/Customers?$top=''"
            );

            options = new ODataQueryOptions(new ODataQueryContext(model, typeof(Customer)), message);
            ExceptionAssert.Throws<ODataException>(() =>
                 options.ApplyTo(Customers),
                 "Invalid value '''' for $top query option found. " +
                 "The $top query option requires a non-negative integer value.");
        }

        [Fact]
        public void CannotConvertBadSkipQueryThrows()
        {
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();

            var message = RequestFactory.Create(
                HttpMethod.Get,
                "http://server/service/Customers?$skip=NotANumber"
            );

            IODataQueryOptions options = new ODataQueryOptions(new ODataQueryContext(model, typeof(Customer)), message);
            ExceptionAssert.Throws<ODataException>(() =>
                 options.ApplyTo(Customers),
                 "Invalid value 'NotANumber' for $skip query option found. " +
                 "The $skip query option requires a non-negative integer value.");

            message = RequestFactory.Create(
                HttpMethod.Get,
                "http://server/service/Customers?$skip=''"
            );

            options = new ODataQueryOptions(new ODataQueryContext(model, typeof(Customer)), message);
            ExceptionAssert.Throws<ODataException>(() =>
                 options.ApplyTo(Customers),
                 "Invalid value '''' for $skip query option found. " +
                 "The $skip query option requires a non-negative integer value.");
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
            var modelBuilder = ODataModelBuilderMocks.GetModelBuilderMock<ODataConventionModelBuilder>();
            modelBuilder.AddEntitySet("entityset", modelBuilder.AddEntityType(elementType));
            var model = modelBuilder.GetEdmModel();

            var message = RequestFactory.Create(
                HttpMethod.Get,
                "http://server/service/entityset?" + oDataQuery
            );

            IODataQueryOptions options = new ODataQueryOptions(new ODataQueryContext(model, elementType), message);
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

            var message = RequestFactory.Create(
                HttpMethod.Get,
                "http://server/service/Customers?" + oDataQuery
            );

            IODataQueryOptions options = new ODataQueryOptions(new ODataQueryContext(model, typeof(Customer)), message);
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

            var message = RequestFactory.Create(
                HttpMethod.Get,
                "http://server/service/Customers?" + oDataQuery
            );

            IODataQueryOptions options = new ODataQueryOptions(new ODataQueryContext(model, typeof(Customer)), message);
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

            var message = RequestFactory.Create(
                HttpMethod.Get,
                "http://server/service/Customers?" + oDataQuery
            );

            IODataQueryOptions options = new ODataQueryOptions(new ODataQueryContext(model, typeof(Customer)), message);
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
            var model = ODataModelBuilderMocks.GetModelBuilderMock<ODataModelBuilder>().Add_Customer_No_Keys_EntityType().Add_Customers_No_Keys_EntitySet().GetEdmModel();

            var message = RequestFactory.Create(
                HttpMethod.Get,
                "http://server/service/Customers?" + oDataQuery
            );

            IODataQueryOptions options = new ODataQueryOptions(new ODataQueryContext(model, typeof(Customer)), message);
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

            var message = RequestFactory.Create(
                HttpMethod.Get,
                "http://server/service/Customers?$orderby=CustomerId,Name"
            );

            IODataQueryOptions options = new ODataQueryOptions(new ODataQueryContext(model, typeof(Customer)), message);
            ODataValidationSettings validationSettings = new ODataValidationSettings { MaxOrderByNodeCount = 1 };

            // Act & Assert
            ExceptionAssert.Throws<ODataException>(() => options.Validate(validationSettings),
                "The number of clauses in $orderby query option exceeded the maximum number allowed. The maximum number of $orderby clauses allowed is 1.");
        }

        [Theory]
        [MemberData(nameof(SystemQueryOptionNames))]
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
            IQueryable<Customer> result = ODataQueryOptions.LimitResults(queryable, limit, false, out resultsLimited) as IQueryable<Customer>;

            Assert.Equal(Math.Min(limit, 4), result.Count());
            Assert.Equal(resultsLimitedExpected, resultsLimited);
        }

        [Fact]
        public void CanTurnOffAllValidation()
        {
            // Arrange
            var message = RequestFactory.Create(
                HttpMethod.Get,
                "http://localhost/?$filter=Name eq 'abc'"
            );

            ODataQueryContext context = ValidationTestHelper.CreateCustomerContext(false);
            IODataQueryOptions option = new ODataQueryOptions(context, message);
            ODataValidationSettings settings = new ODataValidationSettings()
            {
                AllowedQueryOptions = AllowedQueryOptions.OrderBy
            };

            // Act & Assert
            ExceptionAssert.Throws<ODataException>(() => option.Validate(settings),
                "Query option 'Filter' is not allowed. To allow it, set the 'AllowedQueryOptions' property on EnableQueryAttribute or QueryValidationSettings.");

            option.Validator = null;
            ExceptionAssert.DoesNotThrow(() => option.Validate(settings));

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
                    { e.Select(i => new DateTimeOffset(new DateTime(i, 1, 1), TimeSpan.Zero)), "$filter=year($it) ge 5&$orderby=$it desc&$skip=3&$top=1", new DateTimeOffset(new DateTime(year: 6, month: 1, day: 1), TimeSpan.Zero) },
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
                };
            }
        }

        public static TheoryDataSet<IQueryable, string, object> Querying_Enum_Collections_Data
        {
            get
            {
                IQueryable<int> e = Enumerable.Range(1, 9).AsQueryable();
                return new TheoryDataSet<IQueryable, string, object>
                {
                    { e.Select(i => (SimpleEnum)(i%3)), "$filter=$it eq Microsoft.AspNet.OData.Test.Common.Types.SimpleEnum'First'&$orderby=$it desc&$skip=1&$top=1", SimpleEnum.First },
                    { e.Select(i => (SimpleEnum?)null), "$filter=$it eq null&$orderby=$it desc&$skip=1&$top=1", null },
                };
            }
        }

        [Theory]
        [MemberData(nameof(Querying_Primitive_Collections_Data))]
        public void Querying_Primitive_Collections(IQueryable queryable, string query, object result)
        {
            var request = RequestFactory.Create(HttpMethod.Get, "http://localhost/?" + query);
            ODataQueryContext context = new ODataQueryContext(EdmCoreModel.Instance, queryable.ElementType);
            IODataQueryOptions options = new ODataQueryOptions(context, request);

            queryable = options.ApplyTo(queryable);

            IEnumerator enumerator = queryable.GetEnumerator();
            Assert.True(enumerator.MoveNext());
            Assert.Equal(result, enumerator.Current);
        }

#if !NETCORE // TODO 939: This crashes on AspNetCore
        [Theory]
        [MemberData(nameof(Querying_Enum_Collections_Data))]
        public void Querying_Enum_Collections(IQueryable queryable, string query, object result)
        {
            // Arrange
            ODataModelBuilder odataModel = new ODataModelBuilder().Add_SimpleEnum_EnumType();
            IEdmModel model = odataModel.GetEdmModel();
            var request = RequestFactory.Create(HttpMethod.Get, "http://localhost/?" + query);
            ODataQueryContext context = new ODataQueryContext(model, queryable.ElementType);
            ODataQueryOptions options = new ODataQueryOptions(context, request);

            // Act
            queryable = options.ApplyTo(queryable);

            // Assert
            IEnumerator enumerator = queryable.GetEnumerator();
            Assert.True(enumerator.MoveNext());
            Assert.Equal(result, enumerator.Current);
        }
#endif

        [Fact]
        public void ODataQueryOptions_IgnoresUnknownOperatorStartingWithDollar()
        {
            var request = RequestFactory.Create(HttpMethod.Get, "http://localhost/?$filter=$it eq 6&$unknown=value");
            ODataQueryContext context = new ODataQueryContext(EdmCoreModel.Instance, typeof(int));
            IODataQueryOptions options = new ODataQueryOptions(context, request);

            var queryable = options.ApplyTo(Enumerable.Range(0, 10).AsQueryable());

            IEnumerator enumerator = queryable.GetEnumerator();
            Assert.True(enumerator.MoveNext());
            Assert.Equal(6, enumerator.Current);
        }

        [Fact]
        public void ApplyTo_Entity_ThrowsArgumentNull_Entity()
        {
            var message = RequestFactory.Create();

            ODataQueryContext context = new ODataQueryContext(EdmCoreModel.Instance, typeof(int));
            IODataQueryOptions queryOptions = new ODataQueryOptions(context, message);

            ExceptionAssert.ThrowsArgumentNull(
                () => queryOptions.ApplyTo(entity: null, querySettings: new ODataQuerySettings()),
                "entity");
        }

#if !NETCORE // TODO #939: Enable this test on AspNetCore.
        [Fact]
        public void ApplyTo_IgnoresCount_IfRequestAlreadyHasCount()
        {
            // Arrange
            long count = 42;
            var request = RequestFactory.Create(HttpMethod.Get, "http://localhost/?$count=true");
            ODataQueryContext context = new ODataQueryContext(EdmCoreModel.Instance, typeof(int));
            ODataQueryOptions options = new ODataQueryOptions(context, request);
            request.ODataProperties().TotalCount = count;

            // Act
            options.ApplyTo(Enumerable.Empty<int>().AsQueryable());

            // Assert
            Assert.Equal(count, request.ODataProperties().TotalCount);
        }
#endif
        [Fact]
        public void ApplyTo_Entity_ThrowsArgumentNull_QuerySettings()
        {
            var message = RequestFactory.Create();

            ODataQueryContext context = new ODataQueryContext(EdmCoreModel.Instance, typeof(int));
            IODataQueryOptions queryOptions = new ODataQueryOptions(context, message);

            ExceptionAssert.ThrowsArgumentNull(
                () => queryOptions.ApplyTo(entity: 42, querySettings: null),
                "querySettings");
        }

        [Theory]
        [InlineData("$filter=ID eq 1")]
        [InlineData("$orderby=ID")]
        [InlineData("$count=true")]
        [InlineData("$skip=1")]
        [InlineData("$top=0")]
        public void ApplyTo_Entity_ThrowsInvalidOperation_IfNonSelectExpand(string parameter)
        {
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            model.Model.SetAnnotationValue(model.Customer, new ClrTypeAnnotation(typeof(Customer)));
            var request = RequestFactory.Create(HttpMethod.Get, "http://localhost?" + parameter);
            ODataQueryContext context = new ODataQueryContext(model.Model, typeof(Customer));
            IODataQueryOptions queryOptions = new ODataQueryOptions(context, request);

            ExceptionAssert.Throws<InvalidOperationException>(
                () => queryOptions.ApplyTo(42, new ODataQuerySettings()),
                "The requested resource is not a collection. Query options $filter, $orderby, $count, $skip, and $top can be applied only on collections.");
        }

        [Theory]
        [InlineData("?$select=Orders/OrderId", AllowedQueryOptions.Select)]
        [InlineData("?$expand=Orders", AllowedQueryOptions.Expand)]
        public void ApplyTo_Entity_DoesnotApply_IfSetApplied(string queryOption, AllowedQueryOptions appliedQueryOptions)
        {
            // Arrange
            var request = RequestFactory.Create(HttpMethod.Get, "http://localhost" + queryOption);
            var builder = ODataConventionModelBuilderFactory.Create();
            builder.EntitySet<Customer>("Customers");
            ODataQueryContext context = new ODataQueryContext(builder.GetEdmModel(), typeof(Customer));
            IODataQueryOptions options = new ODataQueryOptions(context, request);
            Customer customer = new Customer
            {
                CustomerId = 1,
                Orders = new List<Order>
                {
                    new Order {OrderId = 1}
                }
            };

            // Act
            object result = options.ApplyTo(customer, new ODataQuerySettings(), appliedQueryOptions);

            // Assert
            Assert.Equal(customer, (result as Customer));
        }

        [Theory]
        [InlineData("?$filter=CustomerId eq 1", AllowedQueryOptions.Filter)]
        [InlineData("?$orderby=CustomerId", AllowedQueryOptions.OrderBy)]
        [InlineData("?$count=true", AllowedQueryOptions.Count)]
        [InlineData("?$skip=1", AllowedQueryOptions.Skip)]
        [InlineData("?$top=1", AllowedQueryOptions.Top)]
        [InlineData("?$select=CustomerId", AllowedQueryOptions.Select)]
        [InlineData("?$expand=Orders", AllowedQueryOptions.Expand)]
        public void ApplyTo_DoesnotApply_IfSetApplied(string queryOption, AllowedQueryOptions appliedQueryOptions)
        {
            // Arrange
            var request = RequestFactory.Create(HttpMethod.Get, "http://localhost" + queryOption);
            var builder = ODataConventionModelBuilderFactory.Create();
            builder.EntitySet<Customer>("Customers");
            ODataQueryContext context = new ODataQueryContext(builder.GetEdmModel(), typeof(Customer));
            IODataQueryOptions options = new ODataQueryOptions(context, request);
            IQueryable<Customer> customers =
                Enumerable.Range(1, 10).Select(
                    i => new Customer
                    {
                        CustomerId = i,
                        Orders = new List<Order>
                        {
                            new Order {OrderId = i}
                        }
                    })
                .AsQueryable();

            // Act
            IQueryable result = options.ApplyTo(customers, new ODataQuerySettings(), appliedQueryOptions);

            // Assert
            Assert.Equal(10, (result as IQueryable<Customer>).Count());
        }

#if !NETCORE // TODO #939: Enable this test on AspNetCore.
        [Fact]
        public void ApplyTo_DoesnotCalculateNextPageLink_IfRequestAlreadyHasNextPageLink()
        {
            // Arrange
            Uri nextPageLink = new Uri("http://localhost/nextpagelink");
            var request = RequestFactory.Create(HttpMethod.Get, "http://localhost/");
            ODataQueryContext context = new ODataQueryContext(EdmCoreModel.Instance, typeof(int));
            ODataQueryOptions options = new ODataQueryOptions(context, request);
            request.ODataProperties().NextLink = nextPageLink;

            // Act
            IQueryable result = options.ApplyTo(Enumerable.Range(0, 100).AsQueryable(), new ODataQuerySettings { PageSize = 1 });

            // Assert
            Assert.Equal(nextPageLink, request.ODataProperties().NextLink);
            Assert.Single((result as IQueryable<int>));
        }
#endif
        [Fact]
        public void ODataQueryOptions_WithUnTypedContext_CanBeBuilt()
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            ODataQueryContext context = new ODataQueryContext(model.Model, model.Customer);
            var request = RequestFactory.Create(HttpMethod.Get,
                "http://localhost/?$filter=Id eq 42&$orderby=Id&$skip=42&$top=42&$count=true&$select=Id&$expand=Orders");

            // Act
            IODataQueryOptions queryOptions = new ODataQueryOptions(context, request);

            // Assert
            Assert.NotNull(queryOptions.Filter);
            Assert.NotNull(queryOptions.OrderBy);
            Assert.NotNull(queryOptions.Skip);
            Assert.NotNull(queryOptions.Top);
            Assert.NotNull(queryOptions.SelectExpand);
            Assert.NotNull(queryOptions.Count);
        }

        [Fact]
        public async Task ODataQueryOptions_SetToApplied()
        {
            // Arrange
            string url = "http://localhost/odata/EntityModels?$filter=ID eq 1&$skip=1&$select=A&$expand=ExpandProp";
            HttpClient client = CreateClient();

            // Act
            HttpResponseMessage response = await client.GetAsync(url);
            var responseString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("Property", responseString);
            Assert.DoesNotContain("1", responseString);
            Assert.DoesNotContain("ExpandProperty", responseString);
        }

        [Theory]
        [InlineData("ExpandProp1")]
        [InlineData("ExpandProp2")]
        public async Task ODataQueryOptions_ApplyOrderByInExpandResult_WhenSetPageSize(string propName)
        {
            // Arrange
            string url = "http://localhost/odata/Products?$expand=" + propName;
            HttpClient client = CreateClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);

            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));

            // Act
            HttpResponseMessage response = await client.SendAsync(request);
            var responseObject = JObject.Parse(await response.Content.ReadAsStringAsync());
            var result = responseObject["value"] as JArray;

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(result);
            var expandProp = result[0][propName] as JArray;
            Assert.Equal(2, expandProp.Count);
            Assert.Equal(1, expandProp[0]["ID"]);
            Assert.Equal(2, expandProp[1]["ID"]);
        }

        [Theory]
        [InlineData("ExpandProp3")]
        [InlineData("ExpandProp4")]
        public async Task ODataQueryOptions_ApplyOrderByInExpandResult_WhenSetPageSize_MultiplyKeys(string propName)
        {
            // Arrange
            string url = "http://localhost/odata/Products?$expand=" + propName;
            HttpClient client = CreateClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));

            // Act
            HttpResponseMessage response = await client.SendAsync(request);
            var responseObject = JObject.Parse(await response.Content.ReadAsStringAsync());
            var result = responseObject["value"] as JArray;

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(result);
            var expandProp = result[0][propName] as JArray;
            Assert.Equal(2, expandProp.Count);
            Assert.Equal(1, expandProp[0]["ID1"]);
            Assert.Equal(1, expandProp[0]["ID2"]);
            Assert.Equal(2, expandProp[1]["ID1"]);
            Assert.Equal(1, expandProp[1]["ID2"]);
        }

        [Fact]
        public void DuplicateUnsupportedQueryParametersIgnored()
        {
            // Arrange
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();

            // a simple query with duplicate ignored parameters (key=test)
            string uri = "http://server/service/Customers?$top=10&test=1&test=2";
            var request = RequestFactory.Create(
                HttpMethod.Get,
                uri);
#if NETFX
            request.EnableHttpDependencyInjectionSupport();
#endif
            // Act
            IODataQueryOptions queryOptions = new ODataQueryOptions(new ODataQueryContext(model, typeof(Customer)), request);

            // Assert
            Assert.Equal("10", queryOptions.RawValues.Top);
        }

        [Fact]
        public async Task DuplicateUnsupportedQueryParametersIgnoredWithNoException()
        {
            // Arrange
            string url = "http://localhost/odata/Products?$top=1&test=1&test=2";
            HttpClient client = CreateClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private static HttpClient CreateClient()
        {
            var controllers = new[] { typeof(EntityModelsController), typeof(ProductsController) };
            ODataModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntitySet<ODataQueryOptionTest_EntityModel>("EntityModels");
            builder.EntitySet<MyProduct>("Products");
            builder.EntitySet<ODataQueryOptionTest_EntityModelMultipleKeys>("ODataQueryOptionTest_EntityModelMultipleKeys");
            IEdmModel model = builder.GetEdmModel();

            var server = TestServerFactory.Create(controllers, config =>
            {
                config.Count().OrderBy().Filter().Expand().MaxTop(null);
                config.MapODataServiceRoute("odata", "odata", model);
            });

            return TestServerFactory.CreateClient(server);
        }
    }

    public class EntityModelsController : TestControllerBase
    {
        private static readonly IQueryable<ODataQueryOptionTest_EntityModel> _entityModels;

#if NETCORE
        public IActionResult Get(ODataQueryOptions<ODataQueryOptionTest_EntityModel> queryOptions)
#else
        public IHttpActionResult Get(ODataQueryOptions<ODataQueryOptionTest_EntityModel> queryOptions)
#endif
        {
            // Don't apply Filter and Expand, but apply Select.
            var appliedQueryOptions = AllowedQueryOptions.Skip | AllowedQueryOptions.Filter | AllowedQueryOptions.Expand;
            var res = queryOptions.ApplyTo(_entityModels, appliedQueryOptions);
            return Ok(res.AsQueryable());
        }

        private static IEnumerable<ODataQueryOptionTest_EntityModel> CreateODataQueryOptionTest_EntityModel()
        {
            var entityModel = new ODataQueryOptionTest_EntityModel
            {
                ID = 1,
                A = "Property",
                ExpandProp = new ODataQueryOptionTest_EntityModelMultipleKeys
                {
                    ID1 = 2,
                    ID2 = 3,
                    A = "ExpandProperty"
                }
            };
            yield return entityModel;
        }

        static EntityModelsController()
        {
            _entityModels = CreateODataQueryOptionTest_EntityModel().AsQueryable();
        }
    }

    public class ProductsController : TestODataController
    {
        private static readonly IQueryable<MyProduct> _products;

        [EnableQuery(PageSize = 2)]
        public ITestActionResult Get()
            
        {
            return Ok(_products);
        }
        private static IEnumerable<MyProduct> CreateProducts()
        {
            var prop1 = new List<ODataQueryOptionTest_EntityModel>
            {
                new ODataQueryOptionTest_EntityModel
                {
                    ID = 2,
                    A = "",
                    ExpandProp = null
                },
                new ODataQueryOptionTest_EntityModel
                {
                    ID = 1,
                    A = "",
                    ExpandProp = null
                },
                new ODataQueryOptionTest_EntityModel
                {
                    ID = 3,
                    A = "",
                    ExpandProp = null
                }
            };
            var prop2 = new List<ODataQueryOptionTest_EntityModelMultipleKeys>
            {
                new ODataQueryOptionTest_EntityModelMultipleKeys
                {
                    ID1 = 2,
                    ID2 = 3,
                    A = ""
                },
                new ODataQueryOptionTest_EntityModelMultipleKeys
                {
                    ID1 = 1,
                    ID2 = 1,
                    A = ""
                },
                new ODataQueryOptionTest_EntityModelMultipleKeys
                {
                    ID1 = 2,
                    ID2 = 1,
                    A = ""
                }
            };
            var product = new MyProduct
            {
                Id = 1,
                ExpandProp1 = prop1,
                ExpandProp2 = prop1.AsQueryable(),
                ExpandProp3 = prop2,
                ExpandProp4 = prop2.AsQueryable()
            };
            yield return product;
        }

        static ProductsController()
        {
            _products = CreateProducts().AsQueryable();
        }
    }

    public class MyProduct
    {
        public int Id { get; set; }

        public List<ODataQueryOptionTest_EntityModel> ExpandProp1 { get; set; }

        public IQueryable<ODataQueryOptionTest_EntityModel> ExpandProp2 { get; set; }

        public List<ODataQueryOptionTest_EntityModelMultipleKeys> ExpandProp3 { get; set; }

        public IQueryable<ODataQueryOptionTest_EntityModelMultipleKeys> ExpandProp4 { get; set; }
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

        public ODataQueryOptionTest_EntityModelMultipleKeys ExpandProp { get; set; }
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
