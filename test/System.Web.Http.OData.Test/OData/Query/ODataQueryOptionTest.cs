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
        [InlineData("$orderby=Name&$skip=1", "OrderBy(p1 => p1.Name).Skip(1)")]
        [InlineData("$orderby=Website&$top=1&$skip=1", "OrderBy(p1 => p1.Website).Skip(1).Take(1)")]
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
        public void ApplyTo_Understands_CanUseDefaultOrderByParameter(string oDataQuery, bool canUseDefaultOrderBy, string expectedExpression)
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
