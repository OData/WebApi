// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.OData;
using Microsoft.OData.UriParser;
using Microsoft.Test.E2E.AspNet.OData.Common;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.UriParserExtension
{
    public class UnqualifiedCallTest : WebHostTestBase<UnqualifiedCallTest>
    {
        public UnqualifiedCallTest(WebHostTestFixture<UnqualifiedCallTest> fixture)
            :base(fixture)
        {
        }

        protected static void UpdateConfigure(WebRouteConfiguration configuration)
        {
            var controllers = new[] { typeof(CustomersController), typeof(OrdersController), typeof(MetadataController) };
            configuration.AddControllers(controllers);

            configuration.Routes.Clear();

            configuration.MapODataServiceRoute("odata", "odata",
                builder =>
                    builder.AddService(ServiceLifetime.Singleton,
                        sp => UriParserExtenstionEdmModel.GetEdmModel(configuration))
                        .AddService<IEnumerable<IODataRoutingConvention>>(ServiceLifetime.Singleton, sp =>
                            ODataRoutingConventions.CreateDefaultWithAttributeRouting("odata", configuration)));

            configuration.EnsureInitialized();
        }

        public static TheoryDataSet<string, string, string> UnqualifiedCallCases
        {
            get
            {
                return new TheoryDataSet<string, string, string>()
                {
                    { "Get", "Customers(1)/Default.CalculateSalary(month=2)", "Customers(1)/CalculateSalary(month=2)" },
                    { "Post", "Customers(1)/Default.UpdateAddress()", "Customers(1)/UpdateAddress()" },
                };
            }
        }

        public static TheoryDataSet<string, string, string> UnqualifiedCallAndCaseInsensitiveCases
        {
            get
            {
                return new TheoryDataSet<string, string, string>()
                {
                    { "Get", "Customers(1)/Default.CalculateSalary(month=2)", "CuStOmErS(1)/CaLcUlAtESaLaRy(MONTH=2)" },
                    { "Post", "Customers(1)/Default.UpdateAddress()", "cUsToMeRs(1)/upDaTeAdDrEsS()" },
                };
            }
        }

        [Theory]
        [MemberData(nameof(UnqualifiedCallCases))]
        public async Task EnableUnqualifiedCallTest(string method, string qualifiedFunction, string unqualifiedFunction)
        {
            // Case sensitive
            var qualifiedFunctionUri = string.Format("{0}/odata/{1}", this.BaseAddress, qualifiedFunction);
            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod(method), qualifiedFunctionUri);
            HttpResponseMessage response = await Client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string qualifiedFunctionResponse = await response.Content.ReadAsStringAsync();

            // Case Insensitive
            var unqualifiedFunctionUri = string.Format("{0}/odata/{1}", this.BaseAddress, unqualifiedFunction);
            request = new HttpRequestMessage(new HttpMethod(method), unqualifiedFunctionUri);
            response = await Client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string unqualifiedFunctionResponse = await response.Content.ReadAsStringAsync();

            Assert.Equal(qualifiedFunctionResponse, unqualifiedFunctionResponse);
        }

        [Theory]
        [MemberData(nameof(UnqualifiedCallAndCaseInsensitiveCases))]
        public async Task EnableUnqualifiedCallAndCaseInsensitiveTest(string method, string qualifiedSensitiveFunction,
            string unqualifiedInsensitiveFunction)
        {
            // Case sensitive
            var qualifiedSensitiveFunctionUri = string.Format("{0}/odata/{1}", this.BaseAddress, qualifiedSensitiveFunction);
            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod(method), qualifiedSensitiveFunctionUri);
            HttpResponseMessage response = await Client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string qualifiedSensitiveFunctionResponse = await response.Content.ReadAsStringAsync();

            // Case Insensitive
            var unqualifiedInsensitiveFunctionUri = string.Format("{0}/odata/{1}", this.BaseAddress, unqualifiedInsensitiveFunction);
            request = new HttpRequestMessage(new HttpMethod(method), unqualifiedInsensitiveFunctionUri);
            response = await Client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string unqualifiedInsensitiveFunctionResponse = await response.Content.ReadAsStringAsync();

            Assert.Equal(qualifiedSensitiveFunctionResponse, unqualifiedInsensitiveFunctionResponse);
        }
    }
}
