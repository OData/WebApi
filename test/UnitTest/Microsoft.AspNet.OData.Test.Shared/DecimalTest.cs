using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Builder.TestModels;
using Microsoft.OData.Edm;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.AspNet.OData.Test
{
    public class DecimalTest
    {
        [Fact]
        public async Task PrecitionOfDecimalProperty_ShouldBeCalculated_BasedOnScale()
        {
            const string Uri = "http://localhost/odata/Orders";
            using (HttpClient client = GetClient())
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, Uri);
                request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=full"));

                // Act
                HttpResponseMessage response = await client.SendAsync(request);

                // Assert
                IEnumerable<Order> orders = await ReadEntitiesFromResponse<Order>(response);
                foreach (Order order in orders)
                {
                    Assert.Equal("10.00", order.TotalPrice.ToString());
                }
            }
        }

        private static HttpClient GetClient()
        {
            var controllers = new[] { typeof(MetadataController), typeof(OrdersController) };
            var server = TestServerFactory.Create(controllers, (config) =>
            {
                config.Count().OrderBy().Filter().Expand().MaxTop(null).Select();
                config.MapODataServiceRoute("odata", "odata", GetEdmModel());

            });
            return TestServerFactory.CreateClient(server);
        }

        private static IEdmModel GetEdmModel()
        {
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntitySet<Order>("Orders");
            builder.EntityType<Order>().Property(s => s.TotalPrice).Scale = 2;
            return builder.GetEdmModel();
        }

        async Task<IEnumerable<T>> ReadEntitiesFromResponse<T>(HttpResponseMessage httpResponseMessage) where T : class
        {
            string jsonContent = await httpResponseMessage.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ODataResponse<T>>(jsonContent).Value;
        }
    }

    public class OrdersController : TestODataController
    {
        private DecimalModelContext db = new DecimalModelContext();

        [EnableQuery]
        public ITestActionResult Get()
        {
            return Ok(db.Orders);
        }
    }

    class DecimalModelContext
    {
        private static IList<Order> _orders;

        static DecimalModelContext()
        {
            _orders = new List<Order>()
            {
                new Order(){ OrderId=1,TotalPrice=10 },
                new Order(){ OrderId=2, TotalPrice=10.000m },
                new Order(){ OrderId=3, TotalPrice=10.00m }
            };
        }

        public IEnumerable<Order> Orders { get { return _orders; } }
    }

    class ODataResponse<T>
    {
        public IEnumerable<T> Value { get; set; }
    }
}
