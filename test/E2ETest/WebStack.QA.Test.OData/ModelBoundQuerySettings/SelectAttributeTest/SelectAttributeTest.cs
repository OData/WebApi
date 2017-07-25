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

namespace WebStack.QA.Test.OData.ModelBoundQuerySettings.SelectAttributeTest
{
    public class SelectAttributeTest : ODataTestBase
    {
        private const string CustomerBaseUrl = "{0}/enablequery/Customers";
        private const string OrderBaseUrl = "{0}/enablequery/Orders";
        private const string CarBaseUrl = "{0}/enablequery/Cars";
        private const string AutoSelectCustomerBaseUrl = "{0}/enablequery/AutoSelectCustomers";
        private const string ModelBoundCustomerBaseUrl = "{0}/modelboundapi/Customers";
        private const string ModelBoundOrderBaseUrl = "{0}/modelboundapi/Orders";
        private const string ModelBoundCarBaseUrl = "{0}/modelboundapi/Cars";
        private const string ModelBoundAutoSelectCustomerBaseUrl = "{0}/modelboundapi/AutoSelectCustomers";

        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
        {
            configuration.Services.Replace(
                typeof (IAssembliesResolver),
                new TestAssemblyResolver(typeof(CustomersController), typeof(OrdersController),
                    typeof(CarsController), typeof(AutoSelectCustomersController)));
            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            configuration.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling =
                Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            configuration.Expand();
            configuration.MapODataServiceRoute("enablequery", "enablequery",
                SelectAttributeEdmModel.GetEdmModel());
            configuration.MapODataServiceRoute("modelboundapi", "modelboundapi",
                SelectAttributeEdmModel.GetEdmModelByModelBoundAPI());
        }

        [Theory]
        [InlineData(CustomerBaseUrl + "?$select=*")]
        [InlineData(CustomerBaseUrl + "?$select=Id")]
        [InlineData(CustomerBaseUrl + "?$select=Id,Name")]
        [InlineData(ModelBoundCustomerBaseUrl + "?$select=*")]
        [InlineData(ModelBoundCustomerBaseUrl + "?$select=Id")]
        [InlineData(ModelBoundCustomerBaseUrl + "?$select=Id,Name")]
        public void NoSelectableByDefault(string url)
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
            Assert.Contains("cannot be used in the $select query option.", result);
        }

        [Theory]
        [InlineData(OrderBaseUrl + "?$select=Name", HttpStatusCode.OK)]
        [InlineData(OrderBaseUrl + "?$select=Id", HttpStatusCode.BadRequest)]
        [InlineData(OrderBaseUrl + "?$select=Id,Name", HttpStatusCode.BadRequest)]
        [InlineData(OrderBaseUrl +
            "/WebStack.QA.Test.OData.ModelBoundQuerySettings.SelectAttributeTest.SpecialOrder?$select=Name",
            HttpStatusCode.BadRequest)]
        [InlineData(OrderBaseUrl +
            "/WebStack.QA.Test.OData.ModelBoundQuerySettings.SelectAttributeTest.SpecialOrder?$select=Id",
            HttpStatusCode.BadRequest)]
        [InlineData(OrderBaseUrl +
            "/WebStack.QA.Test.OData.ModelBoundQuerySettings.SelectAttributeTest.SpecialOrder?$select=Price",
            HttpStatusCode.OK)]
        [InlineData(OrderBaseUrl +
            "/WebStack.QA.Test.OData.ModelBoundQuerySettings.SelectAttributeTest.SpecialOrder?$select=SpecialName",
            HttpStatusCode.OK)]
        [InlineData(OrderBaseUrl + "?$expand=Cars($select=Id,Name)", HttpStatusCode.OK)]
        [InlineData(OrderBaseUrl + "?$expand=Cars($select=CarNumber)", HttpStatusCode.BadRequest)]
        [InlineData(CarBaseUrl + "?$select=Id,Name", HttpStatusCode.OK)]
        [InlineData(CarBaseUrl + "?$select=CarNumber", HttpStatusCode.BadRequest)]
        [InlineData(ModelBoundOrderBaseUrl + "?$select=Name", HttpStatusCode.OK)]
        [InlineData(ModelBoundOrderBaseUrl + "?$select=Id", HttpStatusCode.BadRequest)]
        [InlineData(ModelBoundOrderBaseUrl + "?$select=Id,Name", HttpStatusCode.BadRequest)]
        [InlineData(ModelBoundOrderBaseUrl +
            "/WebStack.QA.Test.OData.ModelBoundQuerySettings.SelectAttributeTest.SpecialOrder?$select=Name",
            HttpStatusCode.BadRequest)]
        [InlineData(ModelBoundOrderBaseUrl +
            "/WebStack.QA.Test.OData.ModelBoundQuerySettings.SelectAttributeTest.SpecialOrder?$select=Id",
            HttpStatusCode.BadRequest)]
        [InlineData(ModelBoundOrderBaseUrl +
            "/WebStack.QA.Test.OData.ModelBoundQuerySettings.SelectAttributeTest.SpecialOrder?$select=Price",
            HttpStatusCode.OK)]
        [InlineData(ModelBoundOrderBaseUrl +
            "/WebStack.QA.Test.OData.ModelBoundQuerySettings.SelectAttributeTest.SpecialOrder?$select=SpecialName",
            HttpStatusCode.OK)]
        [InlineData(ModelBoundOrderBaseUrl + "?$expand=Cars($select=Id,Name)", HttpStatusCode.OK)]
        [InlineData(ModelBoundOrderBaseUrl + "?$expand=Cars($select=CarNumber)", HttpStatusCode.BadRequest)]
        [InlineData(ModelBoundCarBaseUrl + "?$select=Id,Name", HttpStatusCode.OK)]
        [InlineData(ModelBoundCarBaseUrl + "?$select=CarNumber", HttpStatusCode.BadRequest)]
        public void SelectOnEntityType(string entitySetUrl, HttpStatusCode statusCode)
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

            Assert.Equal(statusCode, response.StatusCode);
            if (statusCode == HttpStatusCode.BadRequest)
            {
                Assert.Contains("cannot be used in the $select query option.", result);
            }
        }

        [Theory]
        [InlineData(OrderBaseUrl + "?$expand=Customers($select=Id,Name)", HttpStatusCode.OK)]
        [InlineData(OrderBaseUrl + "(1)/Customers?$select=Id,Name", HttpStatusCode.OK)]
        [InlineData(CustomerBaseUrl + "?$expand=Orders($select=Name)", HttpStatusCode.BadRequest)]
        [InlineData(CustomerBaseUrl + "(1)/Orders?$select=Name", HttpStatusCode.BadRequest)]
        [InlineData(CustomerBaseUrl + "?$expand=Orders($select=Id)", HttpStatusCode.OK)]
        [InlineData(CustomerBaseUrl + "(1)/Orders?$select=Id", HttpStatusCode.OK)]
        [InlineData(ModelBoundOrderBaseUrl + "?$expand=Customers($select=Id,Name)", HttpStatusCode.OK)]
        [InlineData(ModelBoundOrderBaseUrl + "(1)/Customers?$select=Id,Name", HttpStatusCode.OK)]
        [InlineData(ModelBoundCustomerBaseUrl + "?$expand=Orders($select=Name)", HttpStatusCode.BadRequest)]
        [InlineData(ModelBoundCustomerBaseUrl + "(1)/Orders?$select=Name", HttpStatusCode.BadRequest)]
        [InlineData(ModelBoundCustomerBaseUrl + "?$expand=Orders($select=Id)", HttpStatusCode.OK)]
        [InlineData(ModelBoundCustomerBaseUrl + "(1)/Orders?$select=Id", HttpStatusCode.OK)]
        public void SelectOnProperty(string entitySetUrl, HttpStatusCode statusCode)
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

            Assert.Equal(statusCode, response.StatusCode);
            if (statusCode == HttpStatusCode.BadRequest)
            {
                Assert.Contains("cannot be used in the $select query option.", result);
            }
        }

        [Theory]
        [InlineData(AutoSelectCustomerBaseUrl)]
        [InlineData(ModelBoundAutoSelectCustomerBaseUrl)]
        public void AutoSelectWithAutoExpand(string url)
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
            Assert.DoesNotContain("Id", result);
            Assert.Contains("Customer1", result);
            Assert.Contains("AutoExpandOrder1", result);
            Assert.Contains("Name", result);
        }

        [Theory]
        [InlineData(AutoSelectCustomerBaseUrl)]
        [InlineData(ModelBoundAutoSelectCustomerBaseUrl)]
        public void AutoSelectPropertyAccessWithAutoExpand(string url)
        {
            string queryUrl =
                string.Format(
                    url + "(1)/Order?$expand=Customer",
                    BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();

            HttpResponseMessage response = client.SendAsync(request).Result;
            string result = response.Content.ReadAsStringAsync().Result;

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.DoesNotContain("Id", result);
            Assert.Contains("Customer3", result);
            Assert.Contains("AutoExpandOrder2", result);
            Assert.Contains("AutoExpandOrder3", result);
            Assert.Contains("Name", result);
        }

        [Theory]
        [InlineData(AutoSelectCustomerBaseUrl)]
        [InlineData(ModelBoundAutoSelectCustomerBaseUrl)]
        public void AutoSelectByDefault(string url)
        {
            string queryUrl =
                string.Format(
                    url + "(1)/Car",
                    BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();

            HttpResponseMessage response = client.SendAsync(request).Result;
            string result = response.Content.ReadAsStringAsync().Result;

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.DoesNotContain("Name", result);
            Assert.Contains("2", result);
            Assert.Contains("Id", result);
        }

        [Theory(Skip = "VSTS AX: Null elimination")]
        [InlineData(AutoSelectCustomerBaseUrl)]
        [InlineData(ModelBoundAutoSelectCustomerBaseUrl)]
        public void DollarSelectGetPrecedenceWithAutoSelect(string url)
        {
            string queryUrl =
                string.Format(
                    url + "(1)/Car?$select=CarNumber",
                    BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();

            HttpResponseMessage response = client.SendAsync(request).Result;
            string result = response.Content.ReadAsStringAsync().Result;

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.DoesNotContain("Name", result);
            Assert.DoesNotContain("2", result);
            Assert.DoesNotContain("Id", result);
            Assert.Contains("CarNumber", result);
        }

        [Theory(Skip = "VSTS AX: Null elimination")]
        [InlineData(AutoSelectCustomerBaseUrl)]
        [InlineData(ModelBoundAutoSelectCustomerBaseUrl)]
        public void NestedDollarSelectGetPrecedenceWithAutoSelect(string url)
        {
            string queryUrl =
                string.Format(
                    url + "(1)?$expand=Car($select=CarNumber)",
                    BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();

            HttpResponseMessage response = client.SendAsync(request).Result;
            string result = response.Content.ReadAsStringAsync().Result;

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.DoesNotContain("Id", result);
            Assert.Contains("CarNumber", result);
        }

        [Theory]
        [InlineData(AutoSelectCustomerBaseUrl)]
        [InlineData(AutoSelectCustomerBaseUrl + "(9)")]
        [InlineData(ModelBoundAutoSelectCustomerBaseUrl)]
        [InlineData(ModelBoundAutoSelectCustomerBaseUrl + "(9)")]
        public void AutomaticSelectInDerivedType(string url)
        {
            string queryUrl = string.Format(url, BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();

            HttpResponseMessage response = client.SendAsync(request).Result;
            string result = response.Content.ReadAsStringAsync().Result;

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("VIPNumber", result);
            Assert.DoesNotContain("Id", result);
        }
    }
}