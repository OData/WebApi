//-----------------------------------------------------------------------------
// <copyright file="NonEdmTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

#if NETCOREAPP3_1_OR_GREATER
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.NonEdm
{
    public class NonEdmTests : WebHostTestBase
    {
        public NonEdmTests(WebHostTestFixture fixture)
            : base(fixture)
        {
        }

        protected override void UpdateConfiguration(WebRouteConfiguration configuration)
        {
            configuration.AddControllers(typeof(CustomersController));
            configuration.Select().Filter().OrderBy().Expand().Count().MaxTop(null);

            // Force use of System.Text.Json output formatter
            configuration.MvcOptionsActions.Add(options =>
            {
                options.OutputFormatters.Insert(0, new SystemTextJsonOutputFormatter(new JsonSerializerOptions()));
            });

            configuration.EnableDependencyInjection();
        }

        public static IEnumerable<object[]> GetSelectExpandTheoryData()
        {
            yield return new object[]
            {
                "Customers?$select=Name",
                "[{\"Name\":\"Customer 1\"},{\"Name\":\"Customer 2\"}]"
            };

            yield return new object[]
            {
                "Customers?$select=*",
                "[{\"Id\":1,\"Name\":\"Customer 1\"},{\"Id\":2,\"Name\":\"Customer 2\"}]"
            };

            yield return new object[]
            {
                "Customers?$select=Name&$expand=RelationshipManager",
                "[{\"RelationshipManager\":{\"Id\":1,\"Name\":\"Employee 1\"},\"Name\":\"Customer 1\"},{\"RelationshipManager\":{\"Id\":3,\"Name\":\"Employee 3\"},\"Name\":\"Customer 2\"}]"
            };

            yield return new object[]
            {
                "Customers?$select=Name&$expand=*",
                "[{\"RelationshipManager\":{\"Id\":1,\"Name\":\"Employee 1\"},\"Name\":\"Customer 1\"},{\"RelationshipManager\":{\"Id\":3,\"Name\":\"Employee 3\"},\"Name\":\"Customer 2\"}]"
            };

            yield return new object[]
            {
                "Customers?$select=Name&$orderby=Name desc",
                "[{\"Name\":\"Customer 2\"},{\"Name\":\"Customer 1\"}]"
            };

            yield return new object[]
            {
                "Customers?$select=Name&$top=1",
                "[{\"Name\":\"Customer 1\"}]"
            };

            yield return new object[]
            {
                "Customers?$select=Name&$orderby=Id desc&$skip=1&$top=1",
                "[{\"Name\":\"Customer 1\"}]"
            };

            yield return new object[]
            {
                "Customers?$filter=Id gt 1&$select=Id,Name",
                "[{\"Id\":2,\"Name\":\"Customer 2\"}]"
            };

            yield return new object[]
            {
                "Customers/Microsoft.Test.E2E.AspNet.OData.NonEdm.EnterpriseCustomer?$select=Name,Id",
                "[{\"Name\":\"Customer 2\",\"Id\":2}]"
            };

            yield return new object[]
            {
                "Customers/Microsoft.Test.E2E.AspNet.OData.NonEdm.EnterpriseCustomer?$select=Name&$expand=AccountManager",
                "[{\"AccountManager\":{\"Id\":2,\"Name\":\"Employee 2\"},\"Name\":\"Customer 2\"}]"
            };

            yield return new object[]
            {
                "Customers/Microsoft.Test.E2E.AspNet.OData.NonEdm.EnterpriseCustomer?$select=Name&$expand=*",
                "[{\"AccountManager\":{\"Id\":2,\"Name\":\"Employee 2\"},\"RelationshipManager\":{\"Id\":3,\"Name\":\"Employee 3\"},\"Name\":\"Customer 2\"}]"
            };

            yield return new object[]
            {
                "Customers/1?$select=Name",
                "{\"Name\":\"Customer 1\"}"
            };

            yield return new object[]
            {
                "Customers/1?$select=*&$expand=RelationshipManager",
                "{\"RelationshipManager\":{\"Id\":1,\"Name\":\"Employee 1\"},\"Id\":1,\"Name\":\"Customer 1\"}"
            };

            yield return new object[]
            {
                "Customers/2?$select=Name&$expand=Microsoft.Test.E2E.AspNet.OData.NonEdm.EnterpriseCustomer/AccountManager",
                "{\"AccountManager\":{\"Id\":2,\"Name\":\"Employee 2\"},\"Name\":\"Customer 2\"}"
            };

            yield return new object[]
            {
                "Customers/2?$select=Name&$expand=*",
                "{\"AccountManager\":{\"Id\":2,\"Name\":\"Employee 2\"},\"RelationshipManager\":{\"Id\":3,\"Name\":\"Employee 3\"},\"Name\":\"Customer 2\"}"
            };
        }

        [Theory]
        [MemberData(nameof(GetSelectExpandTheoryData))]
        public async Task SerializationWorksForQueryOptionsOnQueryStringInNonEdmScenarioUsingSystemTextJson(string odataPath, string expected)
        {
            // Arrange
            var requestUri = string.Format("{0}/api/{1}", BaseAddress, odataPath);
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
            var client = new HttpClient();

            // Act
            var response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();

            Assert.Equal(expected, content);
        }
    }
}
#endif
