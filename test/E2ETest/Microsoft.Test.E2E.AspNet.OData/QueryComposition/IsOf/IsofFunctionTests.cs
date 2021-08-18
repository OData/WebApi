//-----------------------------------------------------------------------------
// <copyright file="IsofFunctionTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Microsoft.Test.E2E.AspNet.OData.Common.Extensions;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.QueryComposition.IsOf
{
    public class IsofFunctionTests : WebHostTestBase
    {
        private static readonly string[] DataSourceTypes = new string[] {"IM", "EF"}; // In Memory or Entity Framework

        public IsofFunctionTests(WebHostTestFixture fixture)
            :base(fixture)
        {
        }

        protected override void UpdateConfiguration(WebRouteConfiguration config)
        {
            var controllers = new[]
                {typeof (BillingCustomersController), typeof (BillingsController), typeof (MetadataController)};
            config.AddControllers(controllers);

            config.Routes.Clear();
            config.Count().Filter().OrderBy().Expand().MaxTop(null);

            IEdmModel model = IsofEdmModel.GetEdmModel(config);
            foreach (string dataSourceType in DataSourceTypes)
            {
                config.MapODataServiceRoute(dataSourceType, dataSourceType, model);
            }
        }

        public static TheoryDataSet<string, string> PrimitivePropertyFilters
        {
            get
            {
                return new TheoryDataSet<string, string>
                {
                    {"?$filter=isof(Name,Edm.String)", "1,2,3"},
                    {"?$filter=isof(Name,    Edm.String)", "1,2,3"},
                    {"?$filter=isof(Name,    Edm.String  )", "1,2,3"},
                    {"?$filter=isof(Name,'Edm.String')", "1,2,3"},
                    {"?$filter=isof(Name,'Edm.Int32')", null},

                    {"?$filter=isof(Birthday,'Edm.DateTimeOffset')", "1,2,3"},
                    {"?$filter=isof(Birthday,'Edm.Date')", null},
                    {"?$filter=isof(Birthday,'Edm.TimeOfDay')", null},

                    {"?$filter=isof(Token,'Edm.Guid')", "2"},
                    {"?$filter=isof(Token,Edm.Guid)", "2"},
                    {"?$filter=isof(Token,Edm.String)", null},
                };
            }
        }

        public static TheoryDataSet<string, string> EnumPropertyFilters
        {
            get
            {
                return new TheoryDataSet<string, string>
                {
                    {"?$filter=isof(CustomerType,'Microsoft.Test.E2E.AspNet.OData.QueryComposition.IsOf.CustomerType')", "1,2,3"},
                    {"?$filter=isof(CustomerType,'Microsoft.Test.E2E.AspNet.OData.QueryComposition.IsOf.CardType')", null},
                };
            }
        }

        public static TheoryDataSet<string, string> ComplexPropertyFilters
        {
            get
            {
                return new TheoryDataSet<string, string>
                {
                    {"?$filter=isof(Address,'Microsoft.Test.E2E.AspNet.OData.QueryComposition.IsOf.BillingAddress')", "1,2,3"},
                    {"?$filter=isof(Address,'Microsoft.Test.E2E.AspNet.OData.QueryComposition.IsOf.BillingCnAddress')", "1,3"},
                    {"?$filter=isof(Address,'Microsoft.Test.E2E.AspNet.OData.QueryComposition.IsOf.BillingUsAddress')", "2"},
                    {"?$filter=isof(Address,'Microsoft.Test.E2E.AspNet.OData.QueryComposition.IsOf.BillingDetail')", null},
                };
            }
        }

        public static TheoryDataSet<string, string> EntityPropertyFilters
        {
            get
            {
                return new TheoryDataSet<string, string>
                {
                    {"?$filter=isof(Billing,'Microsoft.Test.E2E.AspNet.OData.QueryComposition.IsOf.BillingDetail')", "1,2,3"},
                    {"?$filter=isof(Billing,'Microsoft.Test.E2E.AspNet.OData.QueryComposition.IsOf.CreditCard')", "2"},
                    {"?$filter=isof(Billing,'Microsoft.Test.E2E.AspNet.OData.QueryComposition.IsOf.BankAccount')", "3"},
                    
                };
            }
        }

        [Theory]
        [MemberData(nameof(PrimitivePropertyFilters))]
        [MemberData(nameof(EnumPropertyFilters))]
        [MemberData(nameof(ComplexPropertyFilters))]
        [MemberData(nameof(EntityPropertyFilters))]
        [InlineData("?$filter=isof(Billing,'Microsoft.Test.E2E.AspNet.OData.QueryComposition.IsOf.BillingAddress')", null)]
        public async Task QueryEntitySetUsingProperty_UsingInMemoryData(string filter, string expected)
        {
            // Arrange
            var requestUri = string.Format("{0}/IM/BillingCustomers{1}", this.BaseAddress, filter);

            // Act
            HttpResponseMessage response = await Client.GetAsync(requestUri);
            JObject responseString = await response.Content.ReadAsObject<JObject>();

            // Assert
            Assert.True(HttpStatusCode.OK == response.StatusCode);

            JArray value = responseString["value"] as JArray;
            Assert.NotNull(value);

            if (expected == null)
            {
                Assert.Empty(value);
            }
            else
            {
                Assert.True(GetCount(expected) == value.Count);
                Assert.Equal(expected, string.Join(",", value.Select(e => (int) e["CustomerId"])));
            }
        }

        [Theory]
        [MemberData(nameof(PrimitivePropertyFilters))]
        [MemberData(nameof(EnumPropertyFilters))]
        public async Task QueryEntitySetUsingPropertyFailed_UsingEFDataForPrimitiveAndEnum(string filter, string expected)
        {
            // Arrange
            var requestUri = string.Format("{0}/EF/BillingCustomers{1}", this.BaseAddress, filter);

            // Act
            HttpResponseMessage response = null;
            HttpRequestException exception = null;
            try
            {
                response = await Client.GetAsync(requestUri);
            }
            catch (HttpRequestException e)
            {
                exception = e;
            }

#if NETCORE
            // AspNetCore does not encounter the error until after the headers have been sent, at which
            // point is closes the connection.
            Assert.DoesNotContain("unused", expected);
            Assert.Null(response);
            Assert.NotNull(exception);
            Assert.Contains("Error while copying content to a stream.", exception.Message);
#else
            // AspNet catches the exception and converts it to a 500.
            Assert.DoesNotContain("unused", expected);
            Assert.Null(exception);
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);

            Assert.Contains("Only entity types and complex types are supported in LINQ to Entities queries.",
                await response.Content.ReadAsStringAsync());
#endif
        }

        [Theory]
        [MemberData(nameof(EntityPropertyFilters))]
        [InlineData("?$filter=isof(Address,'Microsoft.Test.E2E.AspNet.OData.QueryComposition.IsOf.BillingAddress')", "1,2,3")]
        public async Task QueryEntitySetUsingProperty_UsingEFData(string filter, string expected)
        {
            // Arrange
            var requestUri = string.Format("{0}/EF/BillingCustomers{1}", this.BaseAddress, filter);

            // Act
            HttpResponseMessage response = await Client.GetAsync(requestUri);
            JObject responseString = await response.Content.ReadAsObject<JObject>();

            // Assert
            Assert.True(HttpStatusCode.OK == response.StatusCode);

            JArray value = responseString["value"] as JArray;
            Assert.NotNull(value);

            if (expected == null)
            {
                Assert.Empty(value);
            }
            else
            {
                Assert.True(GetCount(expected) == value.Count);
                Assert.Equal(expected, string.Join(",", value.Select(e => (int)e["CustomerId"])));
            }
        }

        [Theory]
        [InlineData("IM", true)]
        [InlineData("EF", false)] // EF does not support complex type inheritance.
        public async Task IsOfFilterQueryWithComplexTypeProperty(string dataSourceMode, bool work)
        {
            // Arrange
            string filter = "?$filter=isof(Address,'Microsoft.Test.E2E.AspNet.OData.QueryComposition.IsOf.BillingCnAddress')";
            var requestUri = string.Format("{0}/{1}/BillingCustomers{2}", this.BaseAddress, dataSourceMode, filter);

            // Act
            HttpResponseMessage response = null;
            HttpRequestException exception = null;
            try
            {
                response = await Client.GetAsync(requestUri);
            }
            catch (HttpRequestException e)
            {
                exception = e;
            }

            // Assert
            if (work)
            {
                Assert.True(HttpStatusCode.OK == response.StatusCode);

                JObject responseString = await response.Content.ReadAsObject<JObject>();

                JArray value = responseString["value"] as JArray;
                Assert.NotNull(value);
                Assert.Equal(2, value.Count);

                Assert.Equal("1,3", string.Join(",", value.Select(e => (int) e["CustomerId"])));
            }
            else
            {
#if NETCORE
                // AspNetCore does not encounter the error until after the headers have been sent, at which
                // point is closes the connection.
                Assert.Null(response);
                Assert.NotNull(exception);
                Assert.Contains("Error while copying content to a stream.", exception.Message);
#else
                // AspNet catches the exception and converts it to a 500.
                Assert.Null(exception);
                Assert.NotNull(response);
                Assert.True(HttpStatusCode.InternalServerError == response.StatusCode);
#endif
            }
        }

        public static TheoryDataSet<string, string> IsOfFilterOnType
        {
            get
            {
                return new TheoryDataSet<string, string>
                {
                    {"?$filter=isof('Microsoft.Test.E2E.AspNet.OData.QueryComposition.IsOf.BankAccount')", "3"},
                    {"?$filter=isof('Microsoft.Test.E2E.AspNet.OData.QueryComposition.IsOf.CreditCard')", "2"},
                    {"?$filter=isof('Microsoft.Test.E2E.AspNet.OData.QueryComposition.IsOf.BillingDetail')", "1,2,3"},

                    {"?$filter=isof($it,'Microsoft.Test.E2E.AspNet.OData.QueryComposition.IsOf.BankAccount')", "3"},
                    {"?$filter=isof($it,'Microsoft.Test.E2E.AspNet.OData.QueryComposition.IsOf.CreditCard')", "2"},
                    {"?$filter=isof($it,'Microsoft.Test.E2E.AspNet.OData.QueryComposition.IsOf.BillingDetail')", "1,2,3"},
                };
            }
        }

        [Theory]
        [MemberData(nameof(IsOfFilterOnType))]
        public async Task IsOfFilterQueryOnTypeWorks(string filter, string expected)
        {
            foreach (string dataSourceMode in DataSourceTypes)
            {
                // Arrange
                var requestUri = string.Format("{0}/{1}/Billings{2}", this.BaseAddress, dataSourceMode, filter);

                // Act
                HttpResponseMessage response = await Client.GetAsync(requestUri);

                // Assert
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                JObject responseString = await response.Content.ReadAsObject<JObject>();

                JArray value = responseString["value"] as JArray;
                Assert.NotNull(value);
                Assert.Equal(GetCount(expected), value.Count);

                Assert.Equal(expected, string.Join(",", value.Select(e => (int)e["Id"])));
            }
        }

        [Theory]
        [InlineData("IM", true)]
        [InlineData("EF", false)] // EF does not work.
        public async Task IsOfFilterQueryOnTypeDifferentResultUsingDifferentData(string dataSourceMode, bool work)
        {
            // Arrange
            string filter = "?$filter=isof('Microsoft.Test.E2E.AspNet.OData.QueryComposition.IsOf.BillingCustomer')";
            var requestUri = string.Format("{0}/{1}/Billings{2}", this.BaseAddress, dataSourceMode, filter);

            // Act
            HttpResponseMessage response = null;
            HttpRequestException exception = null;
            try
            {
                response = await Client.GetAsync(requestUri);
            }
            catch (HttpRequestException e)
            {
                exception = e;
            }

            // Assert
            if (work)
            {
                Assert.True(HttpStatusCode.OK == response.StatusCode);

                JObject responseString = await response.Content.ReadAsObject<JObject>();

                JArray value = responseString["value"] as JArray;
                Assert.NotNull(value);
                Assert.Empty(value);

                Assert.Equal("", string.Join(",", value.Select(e => (int)e["Id"])));
            }
            else
            {
#if NETCORE
                // AspNetCore does not encounter the error until after the headers have been sent, at which
                // point is closes the connection.
                Assert.Null(response);
                Assert.NotNull(exception);
                Assert.Contains("Error while copying content to a stream.", exception.Message);
#else
                // AspNet catches the exception and converts it to a 500.
                Assert.Null(exception);
                Assert.NotNull(response);
                Assert.True(HttpStatusCode.InternalServerError == response.StatusCode);
                Assert.Contains("DbIsOfExpression requires an expression argument with a polymorphic result type that is " +
                    "compatible with the type argument.", await response.Content.ReadAsStringAsync());
#endif
            }
        }

        [Theory]
        [InlineData("IM")]
        [InlineData("EF")]
        public async Task IsOfFilterQueryFailedOnUnquotedType(string dataSourceMode)
        {
            // Arrange
            string filter = "?$filter=isof(Microsoft.Test.E2E.AspNet.OData.QueryComposition.IsOf.BankAccount)";
            var requestUri = string.Format("{0}/{1}/Billings{2}", this.BaseAddress, dataSourceMode, filter);

            // Act
            HttpResponseMessage response = await Client.GetAsync(requestUri);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            Assert.Contains(
                "The query specified in the URI is not valid. Cast or IsOf Function must have a type in its arguments.",
                await response.Content.ReadAsStringAsync());
        }

        private static int GetCount(string expect)
        {
            if (String.IsNullOrEmpty(expect))
            {
                return 0;
            }

            return expect.Split(',').Count();
        }
    }
}
