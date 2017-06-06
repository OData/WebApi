using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using System.Web.OData.Routing;
using System.Web.OData.Routing.Conventions;
using Microsoft.OData.Edm;
using Nuwa;
using WebStack.QA.Common.XUnit;
using Xunit;
using Xunit.Extensions;

namespace WebStack.QA.Test.OData.Routing
{
    [NuwaFramework]
    public class AddRelatedObjectTests
    {
        [NuwaBaseAddress]
        public string BaseAddress { get; set; }

        [NuwaHttpClient]
        public HttpClient Client { get; set; }

        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration config)
        {
            config.Routes.Clear();
            config.MapODataServiceRoute("odata", "", GetModel(), new DefaultODataPathHandler(), ODataRoutingConventions.CreateDefault());
        }

        private static IEdmModel GetModel()
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<AROCustomer>("AROCustomers");
            builder.EntityType<AROVipCustomer>().DerivesFrom<AROCustomer>();
            builder.EntitySet<AROAddress>("AROAddresses");
            builder.EntitySet<AROOrder>("Orders");
            return builder.GetEdmModel();
        }

        public static TheoryDataSet AddRelatedObjectConventionsWorkPropertyData
        {
            get
            {
                TheoryDataSet<string, string, object> dataSet = new TheoryDataSet<string, string, object>();
                dataSet.Add("POST", "/AROCustomers(5)/Orders", new AROOrder() { Id = 5 });
                dataSet.Add("POST", "/AROCustomers(5)/WebStack.QA.Test.OData.Routing.AROVipCustomer/Orders", new AROOrder() { Id = 5 });
                // ConventionRouting does not support PUT to single-value navigation property
                // dataSet.Add("PUT", "/AROCustomers(5)/WebStack.QA.Test.OData.Routing.AROVipCustomer/Address", new AROAddress() { Id = 5 });
                return dataSet;
            }
        }

        [Theory]
        [PropertyData("AddRelatedObjectConventionsWorkPropertyData")]
        public void AddRelatedObjectConventionsWork(string method, string url, object data)
        {
            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod(method), BaseAddress + url);
            request.Content = new ObjectContent(data.GetType(), data, new JsonMediaTypeFormatter());
            HttpResponseMessage response = Client.SendAsync(request).Result;
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }

    public class AROCustomer
    {
        public int Id { get; set; }
        public IList<AROOrder> Orders { get; set; }
    }

    public class AROVipCustomer : AROCustomer
    {
        public AROAddress Address { get; set; }
    }

    public class AROAddress
    {
        public int Id { get; set; }
    }

    public class AROOrder
    {
        public int Id { get; set; }
    }

    public class AROCustomersController : ODataController
    {
        public IHttpActionResult PostToOrders([FromODataUri] int key, [FromBody] AROOrder order)
        {
            if (key == 5 && order != null && order.Id == 5)
            {
                return Ok();
            }
            return BadRequest();
        }

        public IHttpActionResult PutToAddressFromAROVipCustomer([FromODataUri] int key, [FromBody] AROAddress entity)
        {
            if (key == 5 && entity != null && entity.Id == 5)
            {
                return Ok();
            }
            return BadRequest();
        }

    }
}
