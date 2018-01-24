// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.Test.E2E.AspNet.OData.Common;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.SingleResultTest
{
    public class SingleResultTests : WebHostTestBase
    {
        private const string BaseUrl = "{0}/singleresult/Customers";

        public SingleResultTests(WebHostTestFixture fixture)
            :base(fixture)
        {
        }

        protected override void UpdateConfiguration(HttpConfiguration configuration)
        {
            configuration.Services.Replace(
                typeof(IAssembliesResolver),
                new TestAssemblyResolver(typeof (CustomersController)));
            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            configuration.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling =
                Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null);
            configuration.MapODataServiceRoute("singleresult", "singleresult",
                SingleResultEdmModel.GetEdmModel(configuration));
        }

        [Fact]
        public async Task EmptySingleResultReturnsNotFound()
        {
            // Arrange
            string queryUrl = string.Format(BaseUrl + "(100)", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
