﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.OData.Edm;
using Newtonsoft.Json.Linq;
using Nuwa;
using WebStack.QA.Common.XUnit;
using WebStack.QA.Test.OData.Common;
using Xunit;
using Xunit.Extensions;

namespace WebStack.QA.Test.OData.QueryComposition.IsOf
{
    [NuwaFramework]
    [NuwaTrace(NuwaTraceAttribute.Tag.Off)]
    public class IsofFunctionTests
    {
        private static readonly string[] DataSourceTypes = new string[] {"IM", "EF"}; // In Memory or Entity Framework

        [NuwaBaseAddress]
        public string BaseAddress { get; set; }

        [NuwaHttpClient]
        public HttpClient Client { get; set; }

        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration config)
        {
            var controllers = new[]
            {typeof (BillingCustomersController), typeof (BillingsController), typeof (MetadataController)};
            TestAssemblyResolver resolver = new TestAssemblyResolver(new TypesInjectionAssembly(controllers));

            config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            config.Services.Replace(typeof (IAssembliesResolver), resolver);
            config.Routes.Clear();
            config.Count().Filter().OrderBy().Expand().MaxTop(null);

            IEdmModel model = IsofEdmModel.GetEdmModel();
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
                    {"?$filter=isof(CustomerType,'WebStack.QA.Test.OData.QueryComposition.IsOf.CustomerType')", "1,2,3"},
                    {"?$filter=isof(CustomerType,'WebStack.QA.Test.OData.QueryComposition.IsOf.CardType')", null},
                };
            }
        }

        public static TheoryDataSet<string, string> ComplexPropertyFilters
        {
            get
            {
                return new TheoryDataSet<string, string>
                {
                    {"?$filter=isof(Address,'WebStack.QA.Test.OData.QueryComposition.IsOf.BillingAddress')", "1,2,3"},
                    {"?$filter=isof(Address,'WebStack.QA.Test.OData.QueryComposition.IsOf.BillingCnAddress')", "1,3"},
                    {"?$filter=isof(Address,'WebStack.QA.Test.OData.QueryComposition.IsOf.BillingUsAddress')", "2"},
                    {"?$filter=isof(Address,'WebStack.QA.Test.OData.QueryComposition.IsOf.BillingDetail')", null},
                };
            }
        }

        public static TheoryDataSet<string, string> EntityPropertyFilters
        {
            get
            {
                return new TheoryDataSet<string, string>
                {
                    {"?$filter=isof(Billing,'WebStack.QA.Test.OData.QueryComposition.IsOf.BillingDetail')", "1,2,3"},
                    {"?$filter=isof(Billing,'WebStack.QA.Test.OData.QueryComposition.IsOf.CreditCard')", "2"},
                    {"?$filter=isof(Billing,'WebStack.QA.Test.OData.QueryComposition.IsOf.BankAccount')", "3"},
                    
                };
            }
        }

        [Theory]
        [PropertyData("PrimitivePropertyFilters")]
        [PropertyData("EnumPropertyFilters")]
        [PropertyData("ComplexPropertyFilters")]
        [PropertyData("EntityPropertyFilters")]
        [InlineData("?$filter=isof(Billing,'WebStack.QA.Test.OData.QueryComposition.IsOf.BillingAddress')", null)]
        public async Task QueryEntitySetUsingProperty_UsingInMemoryData(string filter, string expected)
        {
            // Arrange
            var requestUri = string.Format("{0}/IM/BillingCustomers{1}", this.BaseAddress, filter);

            // Act
            HttpResponseMessage response = await Client.GetAsync(requestUri);
            JObject responseString = await response.Content.ReadAsAsync<JObject>();

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
        [PropertyData("PrimitivePropertyFilters")]
        [PropertyData("EnumPropertyFilters")]
        public async Task QueryEntitySetUsingPropertyFailed_UsingEFDataForPrimitiveAndEnum(string filter, string expected)
        {
            // Arrange
            var requestUri = string.Format("{0}/EF/BillingCustomers{1}", this.BaseAddress, filter);

            // Act
            HttpResponseMessage response = await Client.GetAsync(requestUri);

            // Assert
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);

            Assert.Contains("Only entity types and complex types are supported in LINQ to Entities queries.",
                response.Content.ReadAsStringAsync().Result);
        }

        [Theory]
        [PropertyData("EntityPropertyFilters")]
        [InlineData("?$filter=isof(Address,'WebStack.QA.Test.OData.QueryComposition.IsOf.BillingAddress')", "1,2,3")]
        public async Task QueryEntitySetUsingProperty_UsingEFData(string filter, string expected)
        {
            // Arrange
            var requestUri = string.Format("{0}/EF/BillingCustomers{1}", this.BaseAddress, filter);

            // Act
            HttpResponseMessage response = await Client.GetAsync(requestUri);
            JObject responseString = await response.Content.ReadAsAsync<JObject>();

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
            string filter = "?$filter=isof(Address,'WebStack.QA.Test.OData.QueryComposition.IsOf.BillingCnAddress')";
            var requestUri = string.Format("{0}/{1}/BillingCustomers{2}", this.BaseAddress, dataSourceMode, filter);

            // Act
            HttpResponseMessage response = await Client.GetAsync(requestUri);

            // Assert
            if (work)
            {
                Assert.True(HttpStatusCode.OK == response.StatusCode);

                JObject responseString = await response.Content.ReadAsAsync<JObject>();

                JArray value = responseString["value"] as JArray;
                Assert.NotNull(value);
                Assert.Equal(2, value.Count);

                Assert.Equal("1,3", string.Join(",", value.Select(e => (int) e["CustomerId"])));
            }
            else
            {
                Assert.True(HttpStatusCode.InternalServerError == response.StatusCode);
            }
        }

        public static TheoryDataSet<string, string> IsOfFilterOnType
        {
            get
            {
                return new TheoryDataSet<string, string>
                {
                    {"?$filter=isof('WebStack.QA.Test.OData.QueryComposition.IsOf.BankAccount')", "3"},
                    {"?$filter=isof('WebStack.QA.Test.OData.QueryComposition.IsOf.CreditCard')", "2"},
                    {"?$filter=isof('WebStack.QA.Test.OData.QueryComposition.IsOf.BillingDetail')", "1,2,3"},

                    {"?$filter=isof($it,'WebStack.QA.Test.OData.QueryComposition.IsOf.BankAccount')", "3"},
                    {"?$filter=isof($it,'WebStack.QA.Test.OData.QueryComposition.IsOf.CreditCard')", "2"},
                    {"?$filter=isof($it,'WebStack.QA.Test.OData.QueryComposition.IsOf.BillingDetail')", "1,2,3"},
                };
            }
        }

        [Theory]
        [PropertyData("IsOfFilterOnType")]
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

                JObject responseString = await response.Content.ReadAsAsync<JObject>();

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
            string filter = "?$filter=isof('WebStack.QA.Test.OData.QueryComposition.IsOf.BillingCustomer')";
            var requestUri = string.Format("{0}/{1}/Billings{2}", this.BaseAddress, dataSourceMode, filter);

            // Act
            HttpResponseMessage response = await Client.GetAsync(requestUri);

            // Assert
            if (work)
            {
                Assert.True(HttpStatusCode.OK == response.StatusCode);

                JObject responseString = await response.Content.ReadAsAsync<JObject>();

                JArray value = responseString["value"] as JArray;
                Assert.NotNull(value);
                Assert.Equal(0, value.Count);

                Assert.Equal("", string.Join(",", value.Select(e => (int)e["Id"])));
            }
            else
            {
                Assert.True(HttpStatusCode.InternalServerError == response.StatusCode);

                Assert.Contains("DbIsOfExpression requires an expression argument with a polymorphic result type that is " +
                    "compatible with the type argument.", response.Content.ReadAsStringAsync().Result);
            }
        }

        [Theory]
        [InlineData("IM")]
        [InlineData("EF")]
        public async Task IsOfFilterQueryFailedOnUnquotedType(string dataSourceMode)
        {
            // Arrange
            string filter = "?$filter=isof(WebStack.QA.Test.OData.QueryComposition.IsOf.BankAccount)";
            var requestUri = string.Format("{0}/{1}/Billings{2}", this.BaseAddress, dataSourceMode, filter);

            // Act
            HttpResponseMessage response = await Client.GetAsync(requestUri);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            Assert.Contains(
                "The query specified in the URI is not valid. Cast or IsOf Function must have a type in its arguments.",
                response.Content.ReadAsStringAsync().Result);
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
