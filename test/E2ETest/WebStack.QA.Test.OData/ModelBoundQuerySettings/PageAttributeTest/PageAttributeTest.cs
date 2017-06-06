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

namespace WebStack.QA.Test.OData.ModelBoundQuerySettings.PageAttributeTest
{
    public class PageAttributeTest : ODataTestBase
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
            configuration.MaxTop(2).Expand();
            configuration.MapODataServiceRoute("enablequery", "enablequery",
                PageAttributeEdmModel.GetEdmModel());
            configuration.MapODataServiceRoute("modelboundapi", "modelboundapi",
                PageAttributeEdmModel.GetEdmModelByModelBoundAPI());
        }

        [Theory]
        [InlineData(OrderBaseUrl + "?$top=3", 2)]
        [InlineData(ModelBoundOrderBaseUrl + "?$top=3", 2)]
        public void DefaultMaxTop(string url, int maxTop)
        {
            // If there is no attribute on type then the page is disabled, 
            // MaxTop is 0 or the value set in DefaultQuerySetting.
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
            Assert.Contains(string.Format("The limit of '{0}' for Top query has been exceeded", maxTop), result);
        }

        [Theory]
        [InlineData(CustomerBaseUrl + "?$top=10")]
        [InlineData(OrderBaseUrl + "/WebStack.QA.Test.OData.ModelBoundQuerySettings.PageAttributeTest.SpecialOrder?$top=10")]
        [InlineData(ModelBoundCustomerBaseUrl + "?$top=10")]
        [InlineData(ModelBoundOrderBaseUrl + "/WebStack.QA.Test.OData.ModelBoundQuerySettings.PageAttributeTest.SpecialOrder?$top=10")]
        public void MaxTopOnEnitityType(string url)
        {
            // MaxTop on entity type
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
            Assert.Contains("The limit of '5' for Top query has been exceeded", result);
        }

        [Theory]
        [InlineData(CustomerBaseUrl + "?$expand=Orders($top=3)", HttpStatusCode.BadRequest)]
        [InlineData(OrderBaseUrl + "?$expand=Customers($top=10)", HttpStatusCode.OK)]
        [InlineData(CustomerBaseUrl + "(1)/Orders?$top=3", HttpStatusCode.BadRequest)]
        [InlineData(OrderBaseUrl + "(1)/Customers?$top=10", HttpStatusCode.OK)]
        [InlineData(ModelBoundCustomerBaseUrl + "?$expand=Orders($top=3)", HttpStatusCode.BadRequest)]
        [InlineData(ModelBoundOrderBaseUrl + "?$expand=Customers($top=10)", HttpStatusCode.OK)]
        [InlineData(ModelBoundCustomerBaseUrl + "(1)/Orders?$top=3", HttpStatusCode.BadRequest)]
        [InlineData(ModelBoundOrderBaseUrl + "(1)/Customers?$top=10", HttpStatusCode.OK)]
        public void MaxTopOnProperty(string url, HttpStatusCode statusCode)
        {
            // MaxTop on property override on entity type
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
            if (statusCode == HttpStatusCode.BadRequest)
            {
                Assert.Contains("The limit of '2' for Top query has been exceeded", result);
            }
        }

        [Theory]
        [InlineData(CustomerBaseUrl)]
        [InlineData(ModelBoundCustomerBaseUrl)]
        public void PageSizeOnEntityType(string url)
        {
            string queryUrl = string.Format(url, BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=full"));
            HttpClient client = new HttpClient();

            HttpResponseMessage response = client.SendAsync(request).Result;
            string result = response.Content.ReadAsStringAsync().Result;

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains(string.Format(url, "") + "?$skip=1", result);
        }

        [Theory]
        [InlineData(CustomerBaseUrl, "?$expand=Orders")]
        [InlineData(CustomerBaseUrl, "(1)/Orders")]
        [InlineData(ModelBoundCustomerBaseUrl, "?$expand=Orders")]
        [InlineData(ModelBoundCustomerBaseUrl, "(1)/Orders")]
        public void PageSizeOnProperty(string url, string expand)
        {
            string queryUrl = string.Format(url + expand, BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=full"));
            HttpClient client = new HttpClient();

            HttpResponseMessage response = client.SendAsync(request).Result;
            string result = response.Content.ReadAsStringAsync().Result;

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("Orders?$skip=1", result);
        }
    }
}