//-----------------------------------------------------------------------------
// <copyright file="CustomizeSkipTokenImplementationTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Formatter.Serialization;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.OData;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.DependencyInjection
{
    public class CustomizeSkipTokenImplementationTest : WebHostTestBase
    {
        private const string CustomerBaseUrl = "{0}/customskiptoken/Customers";
        private const string OrderBaseUrl = "{0}/customskiptoken/Orders";

        public CustomizeSkipTokenImplementationTest(WebHostTestFixture fixture)
            : base(fixture)
        {
        }

        protected override void UpdateConfiguration(WebRouteConfiguration configuration)
        {
            configuration.AddControllers(typeof(CustomersController), typeof(OrdersController));
            configuration.JsonReferenceLoopHandling =
                Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            configuration.MaxTop(10).Expand().Filter().OrderBy().SkipToken();
            configuration.MapODataServiceRoute("customskiptoken", "customskiptoken", builder =>
                builder.AddService(ServiceLifetime.Singleton, sp => EdmModel.GetEdmModel(configuration))
                       .AddService<IEnumerable<IODataRoutingConvention>>(ServiceLifetime.Singleton, sp =>
                           ODataRoutingConventions.CreateDefaultWithAttributeRouting("customskiptoken", configuration))
                        .AddService<SkipTokenHandler, SkipTopNextLinkGenerator>(ServiceLifetime.Singleton));
        }

        [Theory]
        [InlineData(CustomerBaseUrl, "$skip=2")]
        [InlineData(CustomerBaseUrl + "?$skip=2", "$skip=4")]
        public async Task CustomizeSkipToken(string entitySetUrl, string expected)
        {
            string queryUrl =
                string.Format(
                    entitySetUrl,
                    BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();

            HttpResponseMessage response = await client.SendAsync(request);
            string result = await response.Content.ReadAsStringAsync();
            Assert.Contains(expected, result);
        }
    }

    public class SkipTopNextLinkGenerator : DefaultSkipTokenHandler
    {
        public override Uri GenerateNextPageLink(Uri baseUri, int pageSize, object instance, ODataSerializerContext context)
        {
            return base.GenerateNextPageLink(baseUri, pageSize, null, context);
        }
    }
}
