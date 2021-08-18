//-----------------------------------------------------------------------------
// <copyright file="UriResolverDependencyTestWithOldDefaultConfig.cs" company=".NET Foundation">
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
    public class UnqualifiedCallTestWithOldDefaultConfig : WebHostTestBase
    {
        public UnqualifiedCallTestWithOldDefaultConfig(WebHostTestFixture fixture)
            : base(fixture)
        {
        }

        protected override void UpdateConfiguration(WebRouteConfiguration configuration)
        {
            var controllers = new[] { typeof(CustomersController), typeof(OrdersController), typeof(MetadataController) };
            configuration.AddControllers(controllers);

            configuration.Routes.Clear();

            configuration.MapODataServiceRoute("odata", "odata",
                builder =>
                    builder.AddService(ServiceLifetime.Singleton, sp => UriParserExtenstionEdmModel.GetEdmModel(configuration))
                        .AddService<IEnumerable<IODataRoutingConvention>>(ServiceLifetime.Singleton, sp =>
                            ODataRoutingConventions.CreateDefaultWithAttributeRouting("odata", configuration))
                        .AddService<ODataUriResolver>(ServiceLifetime.Singleton, sp => new ODataUriResolver()));

            configuration.EnsureInitialized();
        }

        public static TheoryDataSet<string, string, HttpStatusCode> urisForOldDefaultConfig
        {
            get
            {
                return new TheoryDataSet<string, string, HttpStatusCode>()
                {
                    // bad cases
                    { "Get", "Customers(1)/CalculateSalary(month=2)", HttpStatusCode.NotFound },
                    { "Post", "Customers(1)/UpdateAddress()", HttpStatusCode.NotFound },
                    { "Get", "CuStOmRrS(1)/Default.CaLcUlAtESaLaRy(MoNtH=2)", HttpStatusCode.NotFound },
                    { "Post", "CuUtOmRrS(1)/Default.UpDaTeAdDrEsS()", HttpStatusCode.NotFound },

                    // good cases
                    { "Get", "Customers(1)/Default.CalculateSalary(month=2)", HttpStatusCode.OK },
                    { "Post", "Customers(1)/Default.UpdateAddress()", HttpStatusCode.OK },
                };
            }
        }

        [Theory]
        [MemberData(nameof(urisForOldDefaultConfig))]
        public async Task InvalidUriWithOldDefaultRestored(string method, string uri, HttpStatusCode expectedStatusCode)
        {
            // Case Insensitive
            var caseInsensitiveUri = string.Format("{0}/odata/{1}", this.BaseAddress, uri);
            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod(method), caseInsensitiveUri);
            HttpResponseMessage response = await Client.SendAsync(request);

            Assert.Equal(expectedStatusCode, response.StatusCode);
        }
    }
}
