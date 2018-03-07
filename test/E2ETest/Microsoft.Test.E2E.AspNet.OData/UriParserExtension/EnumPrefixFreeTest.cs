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
    public class EnumPrefixFreeTest : WebHostTestBase
    {
        public EnumPrefixFreeTest(WebHostTestFixture fixture)
            :base(fixture)
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
                        .AddService<ODataUriResolver>(ServiceLifetime.Singleton, sp => new StringAsEnumResolver()));

            configuration.EnsureInitialized();
        }

        public static TheoryDataSet<string, string, int> EnumPrefixFreeCases
        {
            get
            {
                return new TheoryDataSet<string, string, int>()
                {
                    { "gender=Microsoft.Test.E2E.AspNet.OData.UriParserExtension.Gender'Male'", "gender='Male'", (int)HttpStatusCode.OK },
                    { "gender=Microsoft.Test.E2E.AspNet.OData.UriParserExtension.Gender'UnknownValue'", "gender='UnknownValue'", (int)HttpStatusCode.NotFound },
                };
            }
        }

        [Theory]
        [MemberData(nameof(EnumPrefixFreeCases))]
        public async Task EnableEnumPrefixFreeTest(string prefix, string prefixFree, int statusCode)
        {
            // Enum with prefix
            var prefixUri = string.Format("{0}/odata/Customers/Default.GetCustomerByGender({1})", this.BaseAddress, prefix);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, prefixUri);
            HttpResponseMessage response = await Client.SendAsync(request);

            Assert.Equal(statusCode, (int)response.StatusCode);
            string prefixResponse = await response.Content.ReadAsStringAsync();

            // Enum prefix free
            var prefixFreeUri = string.Format("{0}/odata/Customers/Default.GetCustomerByGender({1})", this.BaseAddress, prefixFree);
            request = new HttpRequestMessage(HttpMethod.Get, prefixFreeUri);
            response = await Client.SendAsync(request);

            Assert.Equal(statusCode, (int)response.StatusCode);
            string prefixFreeResponse = await response.Content.ReadAsStringAsync();

            if (statusCode == (int)HttpStatusCode.OK)
            {
                Assert.Equal(prefixResponse, prefixFreeResponse);
            }
        }
    }
}
