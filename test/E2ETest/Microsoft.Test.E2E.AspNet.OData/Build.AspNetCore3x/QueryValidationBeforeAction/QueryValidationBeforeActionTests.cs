//-----------------------------------------------------------------------------
// <copyright file="QueryValidationBeforeActionTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.QueryValidationBeforeAction
{
    /// <summary>
    /// Checks that query validations are run before action execution.
    /// </summary>
    public class QueryValidationBeforeActionTests : WebHostTestBase
    {
        public QueryValidationBeforeActionTests(WebHostTestFixture fixture)
            : base(fixture)
        {
        }

        protected override void UpdateConfiguration(WebRouteConfiguration configuration)
        {
            configuration.AddControllers(typeof(CustomersController));
            configuration.JsonReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null);
            configuration.MapODataServiceRoute("queryvalidationbeforeaction", "queryvalidationbeforeaction",
                QueryValidationBeforeActionEdmModel.GetEdmModel(configuration));

            configuration.EnableDependencyInjection();
        }

        /// <summary>
        /// For bad queries query execution should happen (and fail) before action being called.
        /// </summary>
        /// <returns>Task tracking operation.</returns>
        [Fact]
        public async Task QueryExecutionBeforeActionBadQuery()
        {
            // Arrange (Allowed top is 10, we are sending 100)
            string queryUrl = string.Format("{0}/queryvalidationbeforeaction/Customers?$top=100", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}
