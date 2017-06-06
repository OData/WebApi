using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.OData;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Extensions;
using Microsoft.Data.Edm;
using Nuwa;
using Xunit;

namespace WebStack.QA.Test.OData.Formatter
{
    [NuwaFramework]
    public class ErrorMessagesTests
    {
        [NuwaBaseAddress]
        public string BaseAddress { get; set; }

        [NuwaHttpClient]
        public HttpClient Client { get; set; }

        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration config)
        {
            config.Routes.Clear();
            config.Routes.MapODataServiceRoute("odata", "odata", GetModel());
        }

        private static IEdmModel GetModel()
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder();
            EntitySetConfiguration<ErrorCustomer> customers = builder.EntitySet<ErrorCustomer>("ErrorCustomers");
            return builder.GetEdmModel();
        }

        [Fact]
        public void ThrowsClearErrorMessageWhenACollectionPropertyIsNull()
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, BaseAddress + "/odata/ErrorCustomers");
            HttpResponseMessage response = Client.SendAsync(request).Result;
            Assert.NotNull(response);
            Assert.NotNull(response.Content);
            string result = response.Content.ReadAsStringAsync().Result;
            Assert.NotNull(result);
            Assert.Contains("Null collections cannot be serialized.", result);
        }
    }

    public class ErrorCustomersController : ODataController
    {
        [EnableQuery(PageSize = 10, MaxExpansionDepth = 2)]
        public IHttpActionResult Get()
        {
            return Ok(Enumerable.Range(0, 1).Select(i => new ErrorCustomer
            {
                Id = i,
                Name = "Name i",
                Numbers = null
            }));
        }
    }

    public class ErrorCustomer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<int> Numbers { get; set; }
    }
}
