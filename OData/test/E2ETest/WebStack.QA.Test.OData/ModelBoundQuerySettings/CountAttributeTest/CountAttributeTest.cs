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
        [InlineData(CustomerBaseUrl + "?$count=true", "entity set 'Customers'")]
        [InlineData(CustomerBaseUrl + "(1)/Addresses?$count=true", "property 'Addresses'")]
        [InlineData(ModelBoundCustomerBaseUrl + "?$count=true", "entity set 'Customers'")]
        [InlineData(ModelBoundCustomerBaseUrl + "(1)/Addresses?$count=true", "property 'Addresses'")]
        public void NonCountByDefault(string entitySetUrl, string error)
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
            Assert.Contains(error + " cannot be used for $count", result);
        }

        [Theory]
        [InlineData(OrderBaseUrl + "?$count=true", HttpStatusCode.OK, "")]
        [InlineData(CustomerBaseUrl + "?$expand=CountableOrders($count=true)", HttpStatusCode.OK, "")]
        [InlineData(CustomerBaseUrl + "(1)/CountableOrders?$count=true", HttpStatusCode.OK, "")]
        [InlineData(CustomerBaseUrl + "(1)/Addresses2?$count=true", HttpStatusCode.OK, "")]
        [InlineData(OrderBaseUrl +
            "/WebStack.QA.Test.OData.ModelBoundQuerySettings.CountAttributeTest.SpecialOrder?$count=true",
            HttpStatusCode.BadRequest,
            "entity set 'Orders/WebStack.QA.Test.OData.ModelBoundQuerySettings.CountAttributeTest.SpecialOrder'")]
        [InlineData(ModelBoundOrderBaseUrl + "?$count=true", HttpStatusCode.OK, "")]
        [InlineData(ModelBoundCustomerBaseUrl + "?$expand=CountableOrders($count=true)", HttpStatusCode.OK, "")]
        [InlineData(ModelBoundCustomerBaseUrl + "(1)/CountableOrders?$count=true", HttpStatusCode.OK, "")]
        [InlineData(ModelBoundCustomerBaseUrl + "(1)/Addresses2?$count=true", HttpStatusCode.OK, "")]
        [InlineData(ModelBoundOrderBaseUrl +
            "/WebStack.QA.Test.OData.ModelBoundQuerySettings.CountAttributeTest.SpecialOrder?$count=true",
            HttpStatusCode.BadRequest,
            "entity set 'Orders/WebStack.QA.Test.OData.ModelBoundQuerySettings.CountAttributeTest.SpecialOrder'")]
        public void CountOnStructuredType(string url, HttpStatusCode statusCode, string error)
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

            Assert.Equal(statusCode, response.StatusCode);
            if (statusCode == HttpStatusCode.OK)
            {
                Assert.Contains("odata.count", result);
            }
            else
            {
                Assert.Contains(error + " cannot be used for $count", result);
            }
        }

        [Theory]
        [InlineData(CustomerBaseUrl + "(1)/Orders?$count=true", "property 'Orders'")]
        [InlineData(CustomerBaseUrl + "?$expand=Orders($count=true)", "property 'Orders'")]
        [InlineData(ModelBoundCustomerBaseUrl + "(1)/Orders?$count=true", "property 'Orders'")]
        [InlineData(ModelBoundCustomerBaseUrl + "?$expand=Orders($count=true)", "property 'Orders'")]
        public void CountOnProperty(string url, string error)
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
            Assert.Contains(error + " cannot be used for $count", result);
        }
    }
}