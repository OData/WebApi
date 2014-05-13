// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using System.Web.OData.Formatter.Serialization.Models;
using Microsoft.OData.Edm;
using Microsoft.TestCommon;

namespace System.Web.OData.Query
{
    public class ODataSingletonQueryOptionTest
    {
        private HttpConfiguration _configuration;
        private HttpClient _client;

        public ODataSingletonQueryOptionTest()
        {
            var controllers = new[] { typeof(MeController) };
            _configuration = controllers.GetHttpConfiguration();
            _configuration.MapODataServiceRoute("odata", "odata", GetEdmModel());
            HttpServer server = new HttpServer(_configuration);
            _client = new HttpClient(server);
        }

        private static IEdmModel GetEdmModel()
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder();
            builder.Singleton<Customer>("Me");
            builder.EntitySet<Order>("Orders");
            builder.EntitySet<Customer>("Customers");
            return builder.GetEdmModel();
        }

        [Fact]
        public void ODataSingletonQueryOption_CanSelectDerivedProperty()
        {
            // Arrange
            const string expectedPayload = "{\r\n" +
                "  \"@odata.context\":\"http://localhost/odata/$metadata#Me(Birthday)\"," +
                "\"Birthday\":\"1991-01-12T09:03:40-00:05\"\r\n" +
                "}";

            string requestUri = "http://localhost/odata/Me/System.Web.OData.Formatter.Serialization.Models.SpecialCustomer?$select=Birthday";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            // Act
            HttpResponseMessage response = _client.SendAsync(request).Result;
            string responseString = response.Content.ReadAsStringAsync().Result;

            // Assert
            Assert.Equal(expectedPayload, responseString);
        }

        [Fact]
        public void ODataSingletonQueryOption_CanExpandThenSelectProperty()
        {
            // Arrange
            string expectedPayload = Resources.SingletonSelectAndExpand;

            // Act
            string respsoneString = _client.GetStringAsync(
                "http://localhost/odata/Me?$select=Orders&$expand=Orders($select=Name)").Result;

            // Assert
            Assert.Equal(expectedPayload, respsoneString);
        }

        [Theory]
        [InlineData("$filter=Name eq 'name'")]
        [InlineData("$top=5")]
        [InlineData("$orderby=ID,Name")]
        [InlineData("$count=true")]
        [InlineData("$skip=3")]
        public void ODataSingletonQueryOption_Failed_OnOtherQueryOptions(string queryOption)
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/odata/Me?" + queryOption);

            // Act
            HttpResponseMessage response = _client.SendAsync(request).Result;
            string responseString = response.Content.ReadAsStringAsync().Result;

            // Assert
            Assert.Equal(Net.HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("The query specified in the URI is not valid. The requested resource is not a collection." +
                " Query options $filter, $orderby, $count, $skip, and $top can be applied only on collections.",
            responseString);
        }
    }

    // Controller
    public class MeController : ODataController
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
            SimpleEnum = Microsoft.TestCommon.Types.SimpleEnum.Third,
            Orders = Enumerable.Range(0, 10).Select(j =>
                new Order
                {
                    ID = 100 + j,
                    Name = "Order #" + j,
                }).ToList()
        };

        [EnableQuery]
        public IHttpActionResult GetFromSpecialCustomer()
        {
            return Ok((SpecialCustomer)me);
        }

        [EnableQuery]
        public IHttpActionResult Get()
        {
            return Ok(me);
        }
    }
}
