// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using System.Web.OData.Query;
using System.Web.OData.TestCommon;
using Microsoft.OData.Edm;
using Microsoft.TestCommon;

namespace System.Web.OData.Test
{
    public class EnableQueryTests
    {
        // Other not allowed query options like $orderby, $top, etc.
        public static TheoryDataSet<string, string> OtherQueryOptionsTestData
        {
            get
            {
                return new TheoryDataSet<string, string>
                {
                    {"?$orderby=Id", "OrderBy"},
                    {"?$top=5", "Top"},
                    {"?$skip=10", "Skip"},
                    {"?$count=true", "Count"},
                    {"?$select=Id", "Select"},
                    {"?$expand=Orders", "Expand"},
                };
            }
        }

        // Other unsupported query options
        public static TheoryDataSet<string, string> OtherUnsupportedQueryOptionsTestData
        {
            get
            {
                return new TheoryDataSet<string, string>
                {
                    {"?$skiptoken=5", "SkipToken"},
                };
            }
        }

        public static TheoryDataSet<string, string> LogicalOperatorsTestData
        {
            get
            {
                return new TheoryDataSet<string, string>
                {
                    // And and or operators
                    {"?$filter=Adult or false", "'Or'"},
                    {"?$filter=true and Adult", "'And'"},

                    // Logical operators with simple property
                    {"?$filter=Id ne 5", "'NotEqual'"},
                    {"?$filter=Id gt 5", "'GreaterThan'"},
                    {"?$filter=Id ge 5", "'GreaterThanOrEqual'"},
                    {"?$filter=Id lt 5", "'LessThan'"},
                    {"?$filter=Id le 5", "'LessThanOrEqual'"},

                    // Logical operators with property in a complex type property
                    {"?$filter=Address/ZipCode ne 5", "'NotEqual'"},
                    {"?$filter=Address/ZipCode gt 5", "'GreaterThan'"},
                    {"?$filter=Address/ZipCode ge 5", "'GreaterThanOrEqual'"},
                    {"?$filter=Address/ZipCode lt 5", "'LessThan'"},
                    {"?$filter=Address/ZipCode le 5", "'LessThanOrEqual'"},

                    // Logical operators with property in a single valued navigation property
                    {"?$filter=Category/Id ne 5", "'NotEqual'"},
                    {"?$filter=Category/Id gt 5", "'GreaterThan'"},
                    {"?$filter=Category/Id ge 5", "'GreaterThanOrEqual'"},
                    {"?$filter=Category/Id lt 5", "'LessThan'"},
                    {"?$filter=Category/Id le 5", "'LessThanOrEqual'"},

                    // Logical operators with property in a derived type in a single valued navigation property
                    {"?$filter=Category/System.Web.OData.Test.PremiumEnableQueryCategory/PremiumLevel ne 5", "NotEqual'"},
                    {"?$filter=Category/System.Web.OData.Test.PremiumEnableQueryCategory/PremiumLevel gt 5", "GreaterThan'"},
                    {"?$filter=Category/System.Web.OData.Test.PremiumEnableQueryCategory/PremiumLevel ge 5", "GreaterThanOrEqual'"},
                    {"?$filter=Category/System.Web.OData.Test.PremiumEnableQueryCategory/PremiumLevel lt 5", "LessThan'"},
                    {"?$filter=Category/System.Web.OData.Test.PremiumEnableQueryCategory/PremiumLevel le 5", "LessThanOrEqual'"},

                    // not operator
                    {"?$filter=not Adult", "'Not'"},
                };
            }
        }

        public static TheoryDataSet<string, string> EqualsOperatorTestData
        {
            get
            {
                return new TheoryDataSet<string, string>
                {
                    {"?$filter=Id eq 5", "Equal"},
                    {"?$filter=Address/ZipCode eq 5", "Equal"},
                    {"?$filter=Category/Id eq 5", "Equal"},
                    {"?$filter=Category/System.Web.OData.Test.PremiumEnableQueryCategory/PremiumLevel eq 5", "Equal"},
                };
            }
        }

        public static TheoryDataSet<string, string> ArithmeticOperatorsTestData
        {
            get
            {
                return new TheoryDataSet<string, string>
                {
                    // Arithmetic operators with simple property
                    {"?$filter=1 eq (3 add Id)", "Add"},
                    {"?$filter=1 eq (3 sub Id)", "Subtract"},
                    {"?$filter=1 eq (1 mul Id)", "Multiply"},
                    {"?$filter=1 eq (Id div 1)", "Divide"},
                    {"?$filter=1 eq (Id mod 1)", "Modulo"},

                    // Arithmetic operators with property in a complex type property
                    {"?$filter=1 eq (3 add Address/ZipCode)", "Add"},
                    {"?$filter=1 eq (3 sub Address/ZipCode)", "Subtract"},
                    {"?$filter=1 eq (1 mul Address/ZipCode)", "Multiply"},
                    {"?$filter=1 eq (Address/ZipCode div 1)", "Divide"},
                    {"?$filter=1 eq (Address/ZipCode mod 1)", "Modulo"},

                    // Arithmetic operators with property in a single valued navigation property
                    {"?$filter=1 eq (3 add Category/Id)", "Add"},
                    {"?$filter=1 eq (3 sub Category/Id)", "Subtract"},
                    {"?$filter=1 eq (1 mul Category/Id)", "Multiply"},
                    {"?$filter=1 eq (Category/Id div 1)", "Divide"},
                    {"?$filter=1 eq (Category/Id mod 1)", "Modulo"},

                    // Arithmetic operators with property in a derived type in a single valued navigation property
                    {"?$filter=1 eq (3 add Category/System.Web.OData.Test.PremiumEnableQueryCategory/PremiumLevel)", "Add"},
                    {"?$filter=1 eq (3 sub Category/System.Web.OData.Test.PremiumEnableQueryCategory/PremiumLevel)", "Subtract"},
                    {"?$filter=1 eq (1 mul Category/System.Web.OData.Test.PremiumEnableQueryCategory/PremiumLevel)", "Multiply"},
                    {"?$filter=1 eq (Category/System.Web.OData.Test.PremiumEnableQueryCategory/PremiumLevel div 1)", "Divide"},
                    {"?$filter=1 eq (Category/System.Web.OData.Test.PremiumEnableQueryCategory/PremiumLevel mod 1)", "Modulo"},
                };
            }
        }

        public static TheoryDataSet<string, string> AnyAndAllFunctionsTestData
        {
            get
            {
                return new TheoryDataSet<string, string>
                {
                    // Primitive collection property
                    {"?$filter=Points/any()", "any"},
                    {"?$filter=Points/any(p: p eq 1)", "any"},
                    {"?$filter=Points/all(p: p eq 1)", "all"},

                    // Complex type collection property
                    {"?$filter=Addresses/any()", "any"},
                    {"?$filter=Addresses/any(a: a/ZipCode eq 1)", "any"},
                    {"?$filter=Addresses/all(a: a/ZipCode eq 1)", "all"},

                    // Collection navigation property
                    {"?$filter=Orders/any()", "any"},
                    {"?$filter=Orders/any(o: o/Id eq 1)", "any"},
                    {"?$filter=Orders/all(o: o/Id eq 1)", "all"},

                    // Collection navigation property with casts
                    {"?$filter=Orders/any(o: o/System.Web.OData.Test.DiscountedEnableQueryOrder/Discount eq 1)", "any"},
                    {"?$filter=Orders/all(o: o/System.Web.OData.Test.DiscountedEnableQueryOrder/Discount eq 1)", "all"},
                    {"?$filter=Orders/System.Web.OData.Test.DiscountedEnableQueryOrder/any()", "any"},
                    {"?$filter=Orders/System.Web.OData.Test.DiscountedEnableQueryOrder/any(o: o/Discount eq 1)", "any"},
                    {"?$filter=Orders/System.Web.OData.Test.DiscountedEnableQueryOrder/all(o: o/Discount eq 1)", "all"},
                };

            }
        }

        public static TheoryDataSet<string, string> CastFunctionTestData
        {
            get
            {
                return new TheoryDataSet<string, string>
                {
                    // Entity type casts
                    {"?$filter=cast(Category,'System.Web.OData.Test.PremiumEnableQueryCategory') eq null", "cast"},
                    {"?$filter=cast(Id, Edm.Double) eq 2", "cast"},
                    {"?$filter=cast(Id, 'Edm.Double') eq 2", "cast"},
                    {"?$filter=cast('System.Web.OData.Test.PremiumEnableQueryCustomer') eq null", "cast"},
                };
            }
        }

        public static TheoryDataSet<string, string> IsOfFunctionTestData
        {
            get
            {
                return new TheoryDataSet<string, string>
                {
                    // Entity type casts
                    {"?$filter=isof(Category,'System.Web.OData.Test.PremiumEnableQueryCategory')", "isof"},
                    {"?$filter=isof('System.Web.OData.Test.PremiumEnableQueryCustomer')", "isof"},
                };
            }
        }

        public static TheoryDataSet<string, string> StringFunctionsTestData
        {
            get
            {
                return new TheoryDataSet<string, string>
                {
                    {"?$filter=startswith(Name, 'Customer')", "startswith"},
                    {"?$filter=endswith(Name, 'Customer')", "endswith"},
                    {"?$filter=contains(Name, 'Customer')", "contains"},
                    {"?$filter=length(Name) eq 1", "length"},
                    {"?$filter=indexof(Name, 'Customer') eq 1", "indexof"},
                    {"?$filter=concat('Customer', Name) eq 'Customer'", "concat"},
                    {"?$filter=substring(Name, 3) eq 'Customer'", "substring"},
                    {"?$filter=substring(Name, 3, 3) eq 'Customer'", "substring"},
                    {"?$filter=tolower(Name) eq 'customer'", "tolower"},
                    {"?$filter=toupper(Name) eq 'CUSTOMER'", "toupper"},
                    {"?$filter=trim(Name) eq 'Customer'", "trim"},
                };
            }
        }

        public static TheoryDataSet<string, string> MathFunctionsTestData
        {
            get
            {
                return new TheoryDataSet<string, string>
                {
                    {"?$filter=round(Id) eq 1", "round"},
                    {"?$filter=floor(Id) eq 1", "floor"},
                    {"?$filter=ceiling(Id) eq 1", "ceiling"},
                };
            }
        }

        public static TheoryDataSet<string, string> SupportedDateTimeFunctionsTestData
        {
            get
            {
                return new TheoryDataSet<string, string>
                {
                    {"?$filter=year(AbsoluteBirthDate) eq 1987", "year"},
                    {"?$filter=month(AbsoluteBirthDate) eq 1987", "month"},
                    {"?$filter=day(AbsoluteBirthDate) eq 1987", "day"},
                    {"?$filter=hour(AbsoluteBirthDate) eq 1987", "hour"},
                    {"?$filter=minute(AbsoluteBirthDate) eq 1987", "minute"},
                    {"?$filter=second(AbsoluteBirthDate) eq 1987", "second"},
                };
            }
        }

        // These represent time functions that we validate but for which we don't support
        // end to end.
        public static TheoryDataSet<string, string> UnsupportedDateTimeFunctionsTestData
        {
            get
            {
                return new TheoryDataSet<string, string>
                {
                    {"?$filter=years(Time) eq 1987", "years"},
                    {"?$filter=months(Time) eq 1987", "months"},
                    {"?$filter=days(Time) eq 1987", "days"},
                    {"?$filter=hours(Time) eq 1987", "hours"},
                    {"?$filter=minutes(Time) eq 1987", "minutes"},
                    {"?$filter=seconds(Time) eq 1987", "seconds"},
                };
            }
        }

        // Other limitations like MaxSkip, MaxTop, AllowedOrderByProperties, etc.
        public static TheoryDataSet<string, string> NumericQueryLimitationsTestData
        {
            get
            {
                return new TheoryDataSet<string, string>
                {
                    {"?$orderby=Name desc, Id asc", "$orderby"},
                    {"?$skip=20", "Skip"},
                    {"?$top=20", "Top"},
                    {"?$expand=Orders($expand=OrderLines)", "$expand"},
                    {"?$filter=Orders/any(o: o/OrderLines/all(ol: ol/Id gt 0))", "MaxAnyAllExpressionDepth"},
                    {"?$filter=Orders/any(o: o/Total gt 0) and Id eq 5", "MaxNodeCount"},
                };
            }
        }

        [Theory]
        [PropertyData("LogicalOperatorsTestData")]
        [PropertyData("ArithmeticOperatorsTestData")]
        [PropertyData("StringFunctionsTestData")]
        [PropertyData("MathFunctionsTestData")]
        [PropertyData("SupportedDateTimeFunctionsTestData")]
        [PropertyData("UnsupportedDateTimeFunctionsTestData")]
        [PropertyData("AnyAndAllFunctionsTestData")]
        [PropertyData("OtherQueryOptionsTestData")]
        [PropertyData("OtherUnsupportedQueryOptionsTestData")]
        public void EnableQuery_Blocks_NotAllowedQueries(string queryString, string expectedElement)
        {
            // Arrange
            string url = "http://localhost/odata/OnlyFilterAndEqualsAllowedCustomers";
            HttpServer server = CreateServer("OnlyFilterAndEqualsAllowedCustomers");
            HttpClient client = new HttpClient(server);

            // Act
            HttpResponseMessage response = client.GetAsync(url + queryString).Result;
            string errorMessage = response.Content.ReadAsStringAsync().Result;

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("not allowed", errorMessage);
            Assert.Contains(expectedElement, errorMessage);
        }

        [Theory]
        [PropertyData("LogicalOperatorsTestData")]
        [PropertyData("ArithmeticOperatorsTestData")]
        [PropertyData("StringFunctionsTestData")]
        [PropertyData("MathFunctionsTestData")]
        [PropertyData("SupportedDateTimeFunctionsTestData")]
        [PropertyData("UnsupportedDateTimeFunctionsTestData")]
        [PropertyData("AnyAndAllFunctionsTestData")]
        public void EnableQuery_BlocksFilter_WhenNotAllowed(string queryString, string unused)
        {
            // Arrange
            string url = "http://localhost/odata/FilterDisabledCustomers";
            HttpServer server = CreateServer("FilterDisabledCustomers");
            HttpClient client = new HttpClient(server);

            // Act
            HttpResponseMessage response = client.GetAsync(url + queryString).Result;
            string errorMessage = response.Content.ReadAsStringAsync().Result;

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("not allowed", errorMessage);
            Assert.Contains("Filter", errorMessage);
        }

        [Theory]
        [PropertyData("OtherUnsupportedQueryOptionsTestData")]
        public void EnableQuery_ReturnsBadRequest_ForUnsupportedQueryOptions(string queryString, string expectedElement)
        {
            // Arrange
            string url = "http://localhost/odata/EverythingAllowedCustomers";
            HttpServer server = CreateServer("EverythingAllowedCustomers");
            HttpClient client = new HttpClient(server);

            // Act
            HttpResponseMessage response = client.GetAsync(url + queryString).Result;
            string errorMessage = response.Content.ReadAsStringAsync().Result;

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("not allowed", errorMessage);
            Assert.Contains(expectedElement, errorMessage);
        }

        // We check equals separately because we need to use it in the rest of the
        // tests to produce valid filter expressions in other cases, so we need to
        // enable it in those tests and this test only makes sure it covers the case
        // when everything is disabled
        [Theory]
        [PropertyData("EqualsOperatorTestData")]
        public void EnableQuery_BlocksEquals_WhenNotAllowed(string queryString, string expectedElement)
        {
            // Arrange
            string url = "http://localhost/odata/OnlyFilterAllowedCustomers";
            HttpServer server = CreateServer("OnlyFilterAllowedCustomers");
            HttpClient client = new HttpClient(server);

            // Act
            HttpResponseMessage response = client.GetAsync(url + queryString).Result;
            string errorMessage = response.Content.ReadAsStringAsync().Result;

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("not allowed", errorMessage);
            Assert.Contains(expectedElement, errorMessage);
        }

        [Theory]
        [PropertyData("LogicalOperatorsTestData")]
        [PropertyData("ArithmeticOperatorsTestData")]
        [PropertyData("EqualsOperatorTestData")]
        [PropertyData("OtherQueryOptionsTestData")]
        [PropertyData("StringFunctionsTestData")]
        [PropertyData("MathFunctionsTestData")]
        [PropertyData("SupportedDateTimeFunctionsTestData")]
        [PropertyData("AnyAndAllFunctionsTestData")]
        [PropertyData("CastFunctionTestData")]
        public void EnableQuery_DoesNotBlockQueries_WhenEverythingIsAllowed(string queryString, string unused)
        {
            // Arrange
            string url = "http://localhost/odata/EverythingAllowedCustomers";
            HttpServer server = CreateServer("EverythingAllowedCustomers");
            HttpClient client = new HttpClient(server);

            // Act
            HttpResponseMessage response = client.GetAsync(url + queryString).Result;

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Theory]
        [PropertyData("UnsupportedDateTimeFunctionsTestData")]
        public void EnableQuery_ReturnsBadRequest_ForUnsupportedFunctions(string queryString, string expectedElement)
        {
            // Arrange
            string url = "http://localhost/odata/EverythingAllowedCustomers";
            HttpServer server = CreateServer("EverythingAllowedCustomers");
            HttpClient client = new HttpClient(server);

            // Act
            HttpResponseMessage response = client.GetAsync(url + queryString).Result;
            string errorMessage = response.Content.ReadAsStringAsync().Result;

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("unknown function", errorMessage);
            Assert.Contains(expectedElement, errorMessage);
        }

        [Theory]
        [PropertyData("IsOfFunctionTestData")]
        public void EnableQuery_ReturnsBadRequest_ForIsOf(string queryString, string expectedElement)
        {
            // Arrange
            string url = "http://localhost/odata/EverythingAllowedCustomers";
            HttpServer server = CreateServer("EverythingAllowedCustomers");
            HttpClient client = new HttpClient(server);

            // Act
            HttpResponseMessage response = client.GetAsync(url + queryString).Result;
            string errorMessage = response.Content.ReadAsStringAsync().Result;

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("Unknown function", errorMessage);
            Assert.Contains(expectedElement, errorMessage);
        }

        [Theory]
        [PropertyData("NumericQueryLimitationsTestData")]
        public void EnableQuery_BlocksQueries_WithOtherLimitations(string queryString, string expectedElement)
        {
            // Arrange
            string url = "http://localhost/odata/OtherLimitationsCustomers";
            HttpServer server = CreateServer("OtherLimitationsCustomers");
            HttpClient client = new HttpClient(server);

            // Act
            HttpResponseMessage response = client.GetAsync(url + queryString).Result;
            string errorMessage = response.Content.ReadAsStringAsync().Result;

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains(expectedElement, errorMessage);
        }

        // This controller limits any operation except for $filter and the eq operator
        // in order to validate that all limitations work.
        public class OnlyFilterAndEqualsAllowedCustomersController : ODataController
        {
            private static readonly IQueryable<EnableQueryCustomer> _customers;

            [EnableQuery(
                AllowedFunctions = AllowedFunctions.None,
                AllowedLogicalOperators = AllowedLogicalOperators.Equal,
                AllowedQueryOptions = AllowedQueryOptions.Filter,
                AllowedArithmeticOperators = AllowedArithmeticOperators.None)]
            public IQueryable<EnableQueryCustomer> Get()
            {
                return _customers;
            }

            static OnlyFilterAndEqualsAllowedCustomersController()
            {
                _customers = CreateCustomers().AsQueryable();
            }
        }

        // This controller exposes an action that limits everything except for
        // filtering in order to verify that limiting the eq operator works.
        // We didn't limit the eq operator in other queries as we need it to
        // create valid filter queries using other limited elements and we
        // want the query to fail because of limitations imposed on other
        // elements rather than eq.
        public class OnlyFilterAllowedCustomersController : ODataController
        {
            private static readonly IQueryable<EnableQueryCustomer> _customers;

            [EnableQuery(
                AllowedFunctions = AllowedFunctions.None,
                AllowedLogicalOperators = AllowedLogicalOperators.None,
                AllowedQueryOptions = AllowedQueryOptions.Filter,
                AllowedArithmeticOperators = AllowedArithmeticOperators.None)]
            public IQueryable<EnableQueryCustomer> Get()
            {
                return _customers;
            }

            static OnlyFilterAllowedCustomersController()
            {
                _customers = CreateCustomers().AsQueryable();
            }
        }

        // This controller disables all the query options ($filter amongst them)
        public class FilterDisabledCustomersController : ODataController
        {
            private static readonly IQueryable<EnableQueryCustomer> _customers;

            [EnableQuery(AllowedQueryOptions = AllowedQueryOptions.None)]
            public IQueryable<EnableQueryCustomer> Get()
            {
                return _customers;
            }

            static FilterDisabledCustomersController()
            {
                _customers = CreateCustomers().AsQueryable();
            }
        }

        // This controller doesn't limit anything specific to ensure that the
        // requests succeed if they aren't limited.
        public class EverythingAllowedCustomersController : ODataController
        {
            private static readonly IQueryable<EnableQueryCustomer> _customers;

            [EnableQuery]
            public IQueryable<EnableQueryCustomer> Get()
            {
                return _customers;
            }

            static EverythingAllowedCustomersController()
            {
                _customers = CreateCustomers().AsQueryable();
            }
        }

        // This controller exposes an action that has limitations on aspects
        // other than AllowedFunctions, AllowedLogicalOperators, etc.
        public class OtherLimitationsCustomersController : ODataController
        {
            private static readonly IQueryable<EnableQueryCustomer> _customers;

            [EnableQuery(MaxNodeCount = 5,
                MaxExpansionDepth = 1,
                MaxAnyAllExpressionDepth = 1,
                MaxSkip = 5,
                MaxTop = 5,
                MaxOrderByNodeCount = 1)]
            public IQueryable<EnableQueryCustomer> Get()
            {
                return _customers;
            }

            static OtherLimitationsCustomersController()
            {
                _customers = CreateCustomers().AsQueryable();
            }
        }

        private static HttpServer CreateServer(string customersEntitySet)
        {
            HttpConfiguration configuration = new HttpConfiguration();

            // We need to do this to avoid controllers with incorrect attribute
            // routing configuration in this assembly that cause an exception to
            // be thrown at runtime. With this, we restrict the test to the following
            // set of controllers.
            configuration.Services.Replace(
                typeof(IAssembliesResolver),
                new TestAssemblyResolver(
                    typeof(OnlyFilterAllowedCustomersController),
                    typeof(OnlyFilterAndEqualsAllowedCustomersController),
                    typeof(FilterDisabledCustomersController),
                    typeof(EverythingAllowedCustomersController),
                    typeof(OtherLimitationsCustomersController)));

            ODataModelBuilder builder = new ODataConventionModelBuilder();

            builder.EntitySet<EnableQueryCustomer>(customersEntitySet);
            builder.EntityType<PremiumEnableQueryCustomer>();

            builder.EntitySet<EnableQueryCategory>("EnableQueryCategories");
            builder.EntityType<PremiumEnableQueryCategory>();

            builder.EntitySet<EnableQueryOrder>("EnableQueryOrders");
            builder.EntityType<DiscountedEnableQueryOrder>();

            builder.EntitySet<EnableQueryOrderLine>("EnableQueryOrderLines");

            builder.ComplexType<EnableQueryAddress>();

            IEdmModel model = builder.GetEdmModel();

            configuration.MapODataServiceRoute("odata", "odata", model);

            return new HttpServer(configuration);
        }

        // We need to create the data as we need the queries to succeed in one scenario.
        private static IEnumerable<EnableQueryCustomer> CreateCustomers()
        {
            PremiumEnableQueryCustomer customer = new PremiumEnableQueryCustomer();

            customer.Id = 1;
            customer.Name = "Customer 1";
            customer.Points = Enumerable.Range(1, 10).ToList();
            customer.Address = new EnableQueryAddress { ZipCode = 1 };
            customer.Addresses = Enumerable.Range(1, 10).Select(j => new EnableQueryAddress { ZipCode = j }).ToList();

            customer.Category = new PremiumEnableQueryCategory
            {
                Id = 1,
                PremiumLevel = 1,
            };

            customer.Orders = Enumerable.Range(1, 10).Select(j => new DiscountedEnableQueryOrder
            {
                Id = j,
                Total = j,
                Discount = j,
            }).ToList<EnableQueryOrder>();

            yield return customer;
        }

        public class EnableQueryCustomer
        {
            public int Id { get; set; }

            public string Name { get; set; }

            public EnableQueryCategory Category { get; set; }

            public ICollection<EnableQueryOrder> Orders { get; set; }

            public ICollection<int> Points { get; set; }

            public ICollection<EnableQueryAddress> Addresses { get; set; }

            public EnableQueryAddress Address { get; set; }

            public DateTimeOffset AbsoluteBirthDate { get; set; }

            public TimeSpan Time { get; set; }

            public bool Adult { get; set; }
        }

        public class PremiumEnableQueryCustomer : EnableQueryCustomer
        {
        }

        public class EnableQueryOrder
        {
            public int Id { get; set; }

            public double Total { get; set; }

            public ICollection<EnableQueryOrderLine> OrderLines { get; set; }
        }

        public class EnableQueryOrderLine
        {
            public int Id { get; set; }
        }

        public class DiscountedEnableQueryOrder : EnableQueryOrder
        {
            public double Discount { get; set; }
        }

        public class EnableQueryCategory
        {
            public int Id { get; set; }
        }

        public class PremiumEnableQueryCategory : EnableQueryCategory
        {
            public int PremiumLevel { get; set; }
        }

        public class EnableQueryAddress
        {
            public int ZipCode { get; set; }
        }
    }
}
