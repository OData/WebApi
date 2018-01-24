// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Dispatcher;
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
    public class UnqualifiedCallTest : WebHostTestBase
    {
        public UnqualifiedCallTest(WebHostTestFixture fixture)
            :base(fixture)
        {
        }

        protected override void UpdateConfiguration(HttpConfiguration configuration)
        {
            var controllers = new[] { typeof(CustomersController), typeof(OrdersController), typeof(MetadataController) };
            TestAssemblyResolver resolver = new TestAssemblyResolver(new TypesInjectionAssembly(controllers));

            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            configuration.Services.Replace(typeof(IAssembliesResolver), resolver);

            configuration.Routes.Clear();

            configuration.MapODataServiceRoute("odata", "odata",
                builder =>
                    builder.AddService(ServiceLifetime.Singleton, sp => UriParserExtenstionEdmModel.GetEdmModel())
                        .AddService<IEnumerable<IODataRoutingConvention>>(ServiceLifetime.Singleton, sp =>
                            ODataRoutingConventions.CreateDefaultWithAttributeRouting("odata", configuration))
                        .AddService<ODataUriResolver>(ServiceLifetime.Singleton, sp => new UnqualifiedODataUriResolver()));

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

        [Theory]
        [MemberData(nameof(UnqualifiedCallCases))]
        public async Task EnableUnqualifiedCallTest(string method, string caseSensitive, string caseInsensitive)
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
