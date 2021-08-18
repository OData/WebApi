//-----------------------------------------------------------------------------
// <copyright file="ODataSingletonQueryOptionTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Formatter.Serialization.Models;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Query
{
    public class ODataSingletonQueryOptionTest
    {
        private HttpClient _client;

        public ODataSingletonQueryOptionTest()
        {
            var controllers = new[] { typeof(MeController) };
            var server = TestServerFactory.Create(controllers, (configuration) =>
            {
                configuration.Count().OrderBy().Filter().Expand().MaxTop(null).Select();
                configuration.MapODataServiceRoute("odata", "odata", GetEdmModel());
            });

            _client = TestServerFactory.CreateClient(server);
        }

        private static IEdmModel GetEdmModel()
        {
            ODataModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.Singleton<Customer>("Me");
            builder.EntitySet<Order>("Orders");
            builder.EntitySet<Customer>("Customers");
            return builder.GetEdmModel();
        }

        [Fact]
        public async Task ODataSingletonQueryOption_CanSelectDerivedProperty()
        {
            // Arrange
            const string expectedPayload = "{" +
                "\"@odata.context\":\"http://localhost/odata/$metadata#Me/Microsoft.AspNet.OData.Test.Formatter.Serialization.Models.SpecialCustomer(Birthday)\"," +
                "\"Birthday\":\"1991-01-12T09:03:40-00:05\"" +
                "}";

            string requestUri = "http://localhost/odata/Me/Microsoft.AspNet.OData.Test.Formatter.Serialization.Models.SpecialCustomer?$select=Birthday";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            // Act
            HttpResponseMessage response = await _client.SendAsync(request);
            string responseString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(expectedPayload, responseString);
        }

        [Fact]
        public async Task ODataSingletonQueryOption_CanExpandThenSelectProperty()
        {
            // Arrange
            string expectedPayload = Resources.SingletonSelectAndExpand;

            // Remove indentation in expect string
            expectedPayload = Regex.Replace(expectedPayload, @"\r\n\s*([""{}\]])", "$1");

            // Act
            string respsoneString = await _client.GetStringAsync(
                "http://localhost/odata/Me?$select=Orders&$expand=Orders($select=Name)");

            // Assert
            Assert.Equal(expectedPayload, respsoneString);
        }

        [Theory]
        [InlineData("$filter=Name eq 'name'")]
        [InlineData("$top=5")]
        [InlineData("$orderby=ID,Name")]
        [InlineData("$count=true")]
        [InlineData("$skip=3")]
        public async Task ODataSingletonQueryOption_Failed_OnOtherQueryOptions(string queryOption)
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/odata/Me?" + queryOption);

            // Act
            HttpResponseMessage response = await _client.SendAsync(request);
            string responseString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("The query specified in the URI is not valid. The requested resource is not a collection." +
                " Query options $filter, $orderby, $count, $skip, and $top can be applied only on collections.",
            responseString);
        }
    }

    // Controller
    public class MeController : TestODataController
    {
        private Customer me = new SpecialCustomer
        {
            ID = 1001,
            Name = "John Kanna",
            FirstName = "John",
            LastName = "Kanna",
            City = "Redmond",
            Birthday = new DateTimeOffset(1991, 1, 12, 9, 3, 40, new TimeSpan(0, -5, 0)),
            Level = 60,
            Bonus = 999.19m,
            SimpleEnum = Common.Types.SimpleEnum.Third,
            Orders = Enumerable.Range(0, 10).Select(j =>
                new Order
                {
                    ID = 100 + j,
                    Name = "Order #" + j,
                }).ToList()
        };

        [EnableQuery]
        public ITestActionResult GetFromSpecialCustomer()
        {
            return Ok((SpecialCustomer)me);
        }

        [EnableQuery]
        public ITestActionResult Get()
        {
            return Ok(me);
        }
    }
}
