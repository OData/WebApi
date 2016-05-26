using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.OData.Extensions;
using System.Web.OData.Query;
using Nuwa;
using WebStack.QA.Test.OData.Common;
using Xunit;
using Xunit.Extensions;

namespace WebStack.QA.Test.OData.ModelBoundQuerySettings.CountAttributeTest
{
    public class CountAttributeTest : ODataTestBase
    {
        private const string CustomerBaseUrl = "{0}/enablequery/Customers";
        private const string OrderBaseUrl = "{0}/enablequery/Orders";
        private const string ModelBoundCustomerBaseUrl = "{0}/modelboundapi/Customers";
        private const string ModelBoundOrderBaseUrl = "{0}/modelboundapi/Orders";

        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
        {
            configuration.Services.Replace(
                typeof (IAssembliesResolver),
                new TestAssemblyResolver(typeof(CustomersController), typeof(OrdersController)));
            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            configuration.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling =
                Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            configuration.Expand();
            configuration.MapODataServiceRoute("enablequery", "enablequery",
                CountAttributeEdmModel.GetEdmModel());
            configuration.MapODataServiceRoute("modelboundapi", "modelboundapi",
                CountAttributeEdmModel.GetEdmModelByModelBoundAPI());
        }

        [Theory]
        [InlineData(CustomerBaseUrl + "?$count=true")]
        [InlineData(CustomerBaseUrl + "(1)/Orders?$count=true")]
        [InlineData(CustomerBaseUrl + "(1)/Addresses?$count=true")]
        [InlineData(ModelBoundCustomerBaseUrl + "?$count=true")]
        [InlineData(ModelBoundCustomerBaseUrl + "(1)/Orders?$count=true")]
        [InlineData(ModelBoundCustomerBaseUrl + "(1)/Addresses?$count=true")]
        public void NonCountByDefault(string entitySetUrl)
        {
            string queryUrl =
                string.Format(
                    entitySetUrl,
                    BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();

            HttpResponseMessage response = client.SendAsync(request).Result;
            string result = response.Content.ReadAsStringAsync().Result;

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("cannot be used for $count", result);
        }

        [Theory]
        [InlineData(OrderBaseUrl + "?$count=true")]
        [InlineData(CustomerBaseUrl + "?$expand=CountableOrders($count=true)")]
        [InlineData(ModelBoundOrderBaseUrl + "?$count=true")]
        [InlineData(ModelBoundCustomerBaseUrl + "?$expand=CountableOrders($count=true)")]
        public void CountOnEntityType(string url)
        {
            string queryUrl =
                string.Format(
                    url,
                    BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();

            HttpResponseMessage response = client.SendAsync(request).Result;
            string result = response.Content.ReadAsStringAsync().Result;

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("odata.count", result);
        }

        [Theory]
        [InlineData(CustomerBaseUrl + "?$expand=Orders($count=true)")]
        [InlineData(ModelBoundCustomerBaseUrl + "?$expand=Orders($count=true)")]
        public void CountOnProperty(string url)
        {
            string queryUrl =
                string.Format(
                    url,
                    BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();

            HttpResponseMessage response = client.SendAsync(request).Result;

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}