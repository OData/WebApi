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

namespace WebStack.QA.Test.OData.ModelBoundQuerySettings.FilterAttributeTest
{
    public class FilterAttributeTest : ODataTestBase
    {
        private const string CustomerBaseUrl = "{0}/enablequery/Customers";
        private const string OrderBaseUrl = "{0}/enablequery/Orders";
        private const string CarBaseUrl = "{0}/enablequery/Cars";
        private const string ModelBoundCustomerBaseUrl = "{0}/modelboundapi/Customers";
        private const string ModelBoundOrderBaseUrl = "{0}/modelboundapi/Orders";
        private const string ModelBoundCarBaseUrl = "{0}/modelboundapi/Cars";

        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
        {
            configuration.Services.Replace(
                typeof (IAssembliesResolver),
                new TestAssemblyResolver(typeof(CustomersController), typeof(OrdersController),
                    typeof(CarsController)));
            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            configuration.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling =
                Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            configuration.Expand();
            configuration.MapODataServiceRoute("enablequery", "enablequery",
                FilterAttributeEdmModel.GetEdmModel());
            configuration.MapODataServiceRoute("modelboundapi", "modelboundapi",
                FilterAttributeEdmModel.GetEdmModelByModelBoundAPI());
        }

        [Theory]
        [InlineData(CustomerBaseUrl + "?$filter=Id eq 1")]
        [InlineData(CustomerBaseUrl + "?$filter=Id eq 1 and Name eq 'test'")]
        [InlineData(OrderBaseUrl + "?$expand=Customers($filter=Id eq 1)")]
        [InlineData(ModelBoundCustomerBaseUrl + "?$filter=Id eq 1")]
        [InlineData(ModelBoundCustomerBaseUrl + "?$filter=Id eq 1 and Name eq 'test'")]
        [InlineData(ModelBoundOrderBaseUrl + "?$expand=Customers($filter=Id eq 1)")]
        public void NonFilterableByDefault(string url)
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
            Assert.Contains("cannot be used in the $filter query option.", result);
        }

        [Theory]
        [InlineData(OrderBaseUrl + "?$filter=Name eq 'test'", HttpStatusCode.OK)]
        [InlineData(OrderBaseUrl + "?$filter=Id eq 1", HttpStatusCode.BadRequest)]
        [InlineData(OrderBaseUrl + "?$filter=Id eq 1 and Name eq 'test'", HttpStatusCode.BadRequest)]
        [InlineData(OrderBaseUrl +
            "/WebStack.QA.Test.OData.ModelBoundQuerySettings.FilterAttributeTest.SpecialOrder?$filter=Name eq 'test'",
            HttpStatusCode.BadRequest)]
        [InlineData(OrderBaseUrl +
            "/WebStack.QA.Test.OData.ModelBoundQuerySettings.FilterAttributeTest.SpecialOrder?$filter=Price eq 1",
            HttpStatusCode.OK)]
        [InlineData(OrderBaseUrl +
            "/WebStack.QA.Test.OData.ModelBoundQuerySettings.FilterAttributeTest.SpecialOrder?$filter=SpecialName eq 'test'",
            HttpStatusCode.OK)]
        [InlineData(OrderBaseUrl + "?$expand=Cars($filter=Id eq 1 and Name eq 'test')", HttpStatusCode.OK)]
        [InlineData(OrderBaseUrl + "?$expand=Cars($filter=CarNumber eq 1)", HttpStatusCode.BadRequest)]
        [InlineData(CarBaseUrl + "?$filter=Id eq 1 and Name eq 'test'", HttpStatusCode.OK)]
        [InlineData(CarBaseUrl + "?$filter=CarNumber eq 1", HttpStatusCode.BadRequest)]
        [InlineData(CustomerBaseUrl + "?$expand=Orders($filter=Name eq 'test')", HttpStatusCode.OK)]
        [InlineData(CustomerBaseUrl + "?$expand=Orders($filter=Id eq 1)", HttpStatusCode.BadRequest)]
        [InlineData(ModelBoundOrderBaseUrl + "?$filter=Name eq 'test'", HttpStatusCode.OK)]
        [InlineData(ModelBoundOrderBaseUrl + "?$filter=Id eq 1", HttpStatusCode.BadRequest)]
        [InlineData(ModelBoundOrderBaseUrl + "?$filter=Id eq 1 and Name eq 'test'", HttpStatusCode.BadRequest)]
        [InlineData(ModelBoundOrderBaseUrl +
            "/WebStack.QA.Test.OData.ModelBoundQuerySettings.FilterAttributeTest.SpecialOrder?$filter=Name eq 'test'",
            HttpStatusCode.BadRequest)]
        [InlineData(ModelBoundOrderBaseUrl +
            "/WebStack.QA.Test.OData.ModelBoundQuerySettings.FilterAttributeTest.SpecialOrder?$filter=Price eq 1",
            HttpStatusCode.OK)]
        [InlineData(ModelBoundOrderBaseUrl +
            "/WebStack.QA.Test.OData.ModelBoundQuerySettings.FilterAttributeTest.SpecialOrder?$filter=SpecialName eq 'test'",
            HttpStatusCode.OK)]
        [InlineData(ModelBoundOrderBaseUrl + "?$expand=Cars($filter=Id eq 1 and Name eq 'test')", HttpStatusCode.OK)]
        [InlineData(ModelBoundOrderBaseUrl + "?$expand=Cars($filter=CarNumber eq 1)", HttpStatusCode.BadRequest)]
        [InlineData(ModelBoundCarBaseUrl + "?$filter=Id eq 1 and Name eq 'test'", HttpStatusCode.OK)]
        [InlineData(ModelBoundCarBaseUrl + "?$filter=CarNumber eq 1", HttpStatusCode.BadRequest)]
        [InlineData(ModelBoundCustomerBaseUrl + "?$expand=Orders($filter=Name eq 'test')", HttpStatusCode.OK)]
        [InlineData(ModelBoundCustomerBaseUrl + "?$expand=Orders($filter=Id eq 1)", HttpStatusCode.BadRequest)]
        public void FilterOnEntityType(string entitySetUrl, HttpStatusCode statusCode)
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
                Assert.Contains("cannot be used in the $filter query option.", result);
            }
        }
    }
}