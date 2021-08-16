//-----------------------------------------------------------------------------
// <copyright file="CaseInsensitiveTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

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
    public class CaseInsensitiveTest : WebHostTestBase
    {
        public CaseInsensitiveTest(WebHostTestFixture fixture)
            :base(fixture)
        {
        }

        protected override void UpdateConfiguration(WebRouteConfiguration configuration)
        {
            var controllers = new[] { typeof(CustomersController), typeof(OrdersController), typeof(MetadataController) };
            configuration.AddControllers(controllers);

            configuration.Routes.Clear();

            configuration.Count().Filter().OrderBy().Expand().MaxTop(null);

            configuration.MapODataServiceRoute("odata", "odata",
                builder =>
                    builder.AddService(ServiceLifetime.Singleton, sp => UriParserExtenstionEdmModel.GetEdmModel(configuration))
                        .AddService<IEnumerable<IODataRoutingConvention>>(ServiceLifetime.Singleton, sp =>
                            ODataRoutingConventions.CreateDefaultWithAttributeRouting("odata", configuration))
                        .AddService<ODataUriResolver>(ServiceLifetime.Singleton, sp => new CaseInsensitiveResolver()));

            configuration.EnsureInitialized();
        }

        public static TheoryDataSet<string, string, string> CaseInsensitiveCases
        {
            get
            {
                return new TheoryDataSet<string, string, string>()
                {
                    // $metadata, $count, $ref, $value
                    { "Get", "$metadata", "$meTadata"},
                    { "Get", "Customers(1)/Name/$value", "Customers(1)/Name/$vAlue"},
                    { "Get", "Customers(1)/Orders/$ref", "Customers(1)/Orders/$rEf" },
                    { "Get", "Customers/$count", "Customers/$coUNt" },

                    // Metadata value
                    { "Get", "Customers", "CusTomeRs"},
                    { "Get", "Customers(2)", "CusTomeRs(2)"},
                    { "Get", "Customers(2)/Name", "CusTomeRs(2)/nAMe"},

                    { "Get", "Customers(6)/Microsoft.Test.E2E.AspNet.OData.UriParserExtension.VipCustomer/VipProperty", "Customers(6)/Microsoft.Test.E2E.AspNet.OData.UriParsereXtension.VipCustomer/vipproPERty"},
                    { "Get", "Customers(1)/Default.CalculateSalary(month=2)", "Customers(1)/deFault.calCULateSalary(month=2)" },
                    { "Post", "Customers(1)/Default.UpdateAddress()", "Customers(1)/deFault.updateaDDress()" },
                };
            }
        }

        [Theory]
        [MemberData(nameof(CaseInsensitiveCases))]
        public async Task EnableCaseInsensitiveTest(string method, string caseSensitive, string caseInsensitive)
        {
            // Case sensitive
            var caseSensitiveUri = string.Format("{0}/odata/{1}", this.BaseAddress, caseSensitive);
            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod(method), caseSensitiveUri);
            HttpResponseMessage response = await Client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string caseSensitiveResponse = await response.Content.ReadAsStringAsync();

            // Case Insensitive
            var caseInsensitiveUri = string.Format("{0}/odata/{1}", this.BaseAddress, caseInsensitive);
            request = new HttpRequestMessage(new HttpMethod(method), caseInsensitiveUri);
            response = await Client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string caseInsensitiveResponse = await response.Content.ReadAsStringAsync();

            Assert.Equal(caseSensitiveResponse, caseInsensitiveResponse);
        }
    }
}
