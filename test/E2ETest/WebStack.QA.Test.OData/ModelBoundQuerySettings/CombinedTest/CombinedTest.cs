using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.OData.Extensions;
using Nuwa;
using WebStack.QA.Test.OData.Common;
using Xunit;
using Xunit.Extensions;

namespace WebStack.QA.Test.OData.ModelBoundQuerySettings.CombinedTest
{
    public class CombinedTest : ODataTestBase
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
            configuration.MapODataServiceRoute("enablequery", "enablequery",
                CombinedEdmModel.GetEdmModel());
            configuration.MapODataServiceRoute("modelboundapi", "modelboundapi",
                CombinedEdmModel.GetEdmModelByModelBoundAPI());
        }

        [Theory]
        [InlineData(CustomerBaseUrl + "?$expand=NoExpandOrders", "expand")]
        [InlineData(CustomerBaseUrl + "?$count=true", "count")]
        [InlineData(CustomerBaseUrl + "?$filter=Id eq 1", "filter")]
        [InlineData(CustomerBaseUrl + "?$orderby=Id", "orderby")]
        [InlineData(OrderBaseUrl + "?$top=3", "top")]
        [InlineData(ModelBoundCustomerBaseUrl + "?$expand=NoExpandOrders", "expand")]
        [InlineData(ModelBoundCustomerBaseUrl + "?$count=true", "count")]
        [InlineData(ModelBoundCustomerBaseUrl + "?$filter=Id eq 1", "filter")]
        [InlineData(ModelBoundCustomerBaseUrl + "?$orderby=Id", "orderby")]
        [InlineData(ModelBoundOrderBaseUrl + "?$top=1", "top")]
        public void DefaultQuerySettings(string url, string error)
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

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains(error, result);
        }

        [Theory]
        [InlineData(CustomerBaseUrl, "Orders($count=true)", "count")]
        [InlineData(CustomerBaseUrl, "Orders($filter=Id eq 1)", "filter")]
        [InlineData(CustomerBaseUrl, "Orders($orderby=Id)", "orderby")]
        [InlineData(CustomerBaseUrl, "Orders($top=3)", "top")]
        [InlineData(ModelBoundCustomerBaseUrl, "Orders($count=true)", "count")]
        [InlineData(ModelBoundCustomerBaseUrl, "Orders($filter=Id eq 1)", "filter")]
        [InlineData(ModelBoundCustomerBaseUrl, "Orders($orderby=Id)", "orderby")]
        [InlineData(ModelBoundCustomerBaseUrl, "Orders($top=3)", "top")]
        public void QueryAttributeOnEntityTypeNegative(string entitySetUrl, string expandOption, string error)
        {
            string queryUrl =
                string.Format(
                    entitySetUrl + "?$expand=" + expandOption,
                    BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();

            HttpResponseMessage response = client.SendAsync(request).Result;
            string result = response.Content.ReadAsStringAsync().Result;
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains(error, result);
        }

        [Theory]
        [InlineData(CustomerBaseUrl, "CountableOrders($count=true)")]
        [InlineData(CustomerBaseUrl, "Orders($filter=Name eq 'test')")]
        [InlineData(CustomerBaseUrl, "Orders($orderby=Name)")]
        [InlineData(CustomerBaseUrl, "Orders($top=2)")]
        [InlineData(ModelBoundCustomerBaseUrl, "CountableOrders($count=true)")]
        [InlineData(ModelBoundCustomerBaseUrl, "Orders($filter=Name eq 'test')")]
        [InlineData(ModelBoundCustomerBaseUrl, "Orders($orderby=Name)")]
        [InlineData(ModelBoundCustomerBaseUrl, "Orders($top=2)")]
        public void QueryAttributeOnEntityTypePositive(string entitySetUrl, string expandOption)
        {
            string queryUrl =
                string.Format(
                    entitySetUrl + "?$expand=" + expandOption,
                    BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();

            HttpResponseMessage response = client.SendAsync(request).Result;
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Theory]
        [InlineData(CustomerBaseUrl, "?$expand=Orders($expand=Customers($count=true))", "count")]
        [InlineData(OrderBaseUrl, "?$expand=Customers2($expand=Order($top=2))", "top")]
        [InlineData(ModelBoundCustomerBaseUrl, "?$expand=Orders($expand=Customers($count=true))", "count")]
        [InlineData(ModelBoundOrderBaseUrl, "?$expand=Customers2($expand=Order($top=2))", "top")]
        public void QuerySettingsOnPropertyNegative(string entitySetUrl, string url, string error)
        {
            string queryUrl =
                string.Format(
                    entitySetUrl + url,
                    BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();

            HttpResponseMessage response = client.SendAsync(request).Result;
            string result = response.Content.ReadAsStringAsync().Result;
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains(error, result);
        }

        [Theory]
        [InlineData(OrderBaseUrl, "?$expand=Customers2($count=true)")]
        [InlineData(OrderBaseUrl, "?$expand=Customers2($top=1)")]
        [InlineData(ModelBoundOrderBaseUrl, "?$expand=Customers2($count=true)")]
        [InlineData(ModelBoundOrderBaseUrl, "?$expand=Customers2($top=1)")]
        public void QuerySettingsOnPropertyPositive(string entitySetUrl, string url)
        {
            string queryUrl =
                string.Format(
                    entitySetUrl + url,
                    BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();

            HttpResponseMessage response = client.SendAsync(request).Result;
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}