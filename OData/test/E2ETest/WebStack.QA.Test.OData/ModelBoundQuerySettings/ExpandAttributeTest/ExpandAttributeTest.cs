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

namespace WebStack.QA.Test.OData.ModelBoundQuerySettings.ExpandAttributeTest
{
    public class ExpandAttributeTest : ODataTestBase
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
                ExpandAttributeEdmModel.GetEdmModel());
            configuration.MapODataServiceRoute("modelboundapi", "modelboundapi",
                ExpandAttributeEdmModel.GetEdmModelByModelBoundAPI());
        }

        [Theory]
        [InlineData(CustomerBaseUrl + "?$expand=NoExpandOrders")]
        [InlineData(CustomerBaseUrl + "?$expand=Order")]
        [InlineData(ModelBoundCustomerBaseUrl + "?$expand=NoExpandOrders")]
        [InlineData(ModelBoundCustomerBaseUrl + "?$expand=Order")]
        public void NonExpandByDefault(string url)
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
            Assert.Contains("cannot be used in the $expand query option", result);
        }

        [Theory]
        [InlineData(CustomerBaseUrl, "Orders", HttpStatusCode.OK)]
        [InlineData(OrderBaseUrl, "Customers", HttpStatusCode.OK)]
        [InlineData(OrderBaseUrl, "Customers2", HttpStatusCode.OK)]
        [InlineData(OrderBaseUrl, "NoExpandCustomers", HttpStatusCode.BadRequest)]
        [InlineData(ModelBoundCustomerBaseUrl, "Orders", HttpStatusCode.OK)]
        [InlineData(ModelBoundOrderBaseUrl, "Customers", HttpStatusCode.OK)]
        [InlineData(ModelBoundOrderBaseUrl, "Customers2", HttpStatusCode.OK)]
        [InlineData(ModelBoundOrderBaseUrl, "NoExpandCustomers", HttpStatusCode.BadRequest)]
        public void ExpandOnEntityType(string entitySetUrl, string expandOption, HttpStatusCode statusCode)
        {
            string queryUrl =
                string.Format(
                    entitySetUrl + "?$expand=" + expandOption,
                    BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();

            HttpResponseMessage response = client.SendAsync(request).Result;
            Assert.Equal(statusCode, response.StatusCode);
        }

        [Theory]
        [InlineData(CustomerBaseUrl, "Orders($expand=Customers2)", HttpStatusCode.BadRequest)]
        [InlineData(CustomerBaseUrl, "Orders($expand=Customers)", HttpStatusCode.OK)]
        [InlineData(OrderBaseUrl, "Customers2($expand=Orders)", HttpStatusCode.BadRequest)]
        [InlineData(OrderBaseUrl, "Customers2($expand=Order)", HttpStatusCode.OK)]
        [InlineData(CustomerBaseUrl, "Order($expand=Customers2)", HttpStatusCode.BadRequest)]
        [InlineData(CustomerBaseUrl, "Order($expand=Customers)", HttpStatusCode.BadRequest)]
        [InlineData(ModelBoundCustomerBaseUrl, "Orders($expand=Customers2)", HttpStatusCode.BadRequest)]
        [InlineData(ModelBoundCustomerBaseUrl, "Orders($expand=Customers)", HttpStatusCode.OK)]
        [InlineData(ModelBoundOrderBaseUrl, "Customers2($expand=Orders)", HttpStatusCode.BadRequest)]
        [InlineData(ModelBoundOrderBaseUrl, "Customers2($expand=Order)", HttpStatusCode.OK)]
        [InlineData(ModelBoundCustomerBaseUrl, "Order($expand=Customers2)", HttpStatusCode.BadRequest)]
        [InlineData(ModelBoundCustomerBaseUrl, "Order($expand=Customers)", HttpStatusCode.BadRequest)]
        public void ExpandOnProperty(string entitySetUrl, string expandOption, HttpStatusCode statusCode)
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
            Assert.Equal(statusCode, response.StatusCode);
            if (statusCode == HttpStatusCode.BadRequest)
            {
                Assert.Contains("cannot be used in the $expand query option", result);
            }
        }

        [Theory]
        [InlineData(CustomerBaseUrl, "Orders($expand=Customers($expand=Orders($expand=Customers)))")]
        [InlineData(CustomerBaseUrl, "AutoExpandOrder($expand=RelatedOrder($levels=3)),Friend($levels=3)")]
        [InlineData(CustomerBaseUrl, "Friend($expand=Friend($expand=Friend($levels=max)))")]
        [InlineData(ModelBoundCustomerBaseUrl, "Orders($expand=Customers($expand=Orders($expand=Customers)))")]
        [InlineData(ModelBoundCustomerBaseUrl, "AutoExpandOrder($expand=RelatedOrder($levels=3)),Friend($levels=3)")]
        [InlineData(ModelBoundCustomerBaseUrl, "Friend($expand=Friend($expand=Friend($levels=max)))")]
        public void ExpandDepth(string url, string expandOption)
        {
            string queryUrl =
                string.Format(
                    url + "?$expand=" + expandOption,
                    BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();

            HttpResponseMessage response = client.SendAsync(request).Result;
            string result = response.Content.ReadAsStringAsync().Result;

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("The maximum depth allowed is " + 3, result);
        }

        [Theory]
        [InlineData(CustomerBaseUrl)]
        [InlineData(ModelBoundCustomerBaseUrl)]
        public void AutomaticExpand(string url)
        {
            string queryUrl = string.Format(url, BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();

            HttpResponseMessage response = client.SendAsync(request).Result;
            string result = response.Content.ReadAsStringAsync().Result;

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("AutoExpandOrder", result);
        }
    }
}