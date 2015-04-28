using Microsoft.Data.Edm;
using Nuwa;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.OData;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Extensions;
using WebStack.QA.Common.WebHost;
using Xunit;

namespace WebStack.QA.Test.OData.Formatter.JsonLight.Metadata
{
    [NuwaFramework]
    public class MinimalMetadataSpecificTests
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

        [NuwaWebConfig]
        public static void UpdateWebConfig(WebConfigHelper webConfig)
        {
            webConfig.AddRAMFAR(true);
        }

        private static IEdmModel GetModel()
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder();
            var pets = builder.EntitySet<Pet>("Pets");
            builder.Entity<BigPet>();
            return builder.GetEdmModel();
        }

        [Fact]
        public void QueryWithCastDoesntContainODataType()
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, BaseAddress + "/odata/Pets(5)/WebStack.QA.Test.OData.Formatter.JsonLight.Metadata.BigPet");
            HttpResponseMessage response = Client.SendAsync(request).Result;
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string payload = response.Content.ReadAsStringAsync().Result;
            Assert.DoesNotContain("odata.type", payload);
            Assert.DoesNotContain("WebStack.QA.Test.OData.Formatter.JsonLight.Metadata.BigPet", payload);
        }

        [Fact]
        public void QueryWithoutCastContainsODataType()
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, BaseAddress + "/odata/Pets(5)");
            HttpResponseMessage response = Client.SendAsync(request).Result;
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string payload = response.Content.ReadAsStringAsync().Result;
            Assert.Contains("odata.type", payload);
            Assert.Contains("WebStack.QA.Test.OData.Formatter.JsonLight.Metadata.BigPet", payload);
        }
    }

    public class Pet
    {
        public int Id { get; set; }
    }

    public class BigPet : Pet
    {
    }

    public class PetsController : ODataController
    {
        [EnableQuery(PageSize = 10, MaxExpansionDepth = 2)]
        public IHttpActionResult Get()
        {
            return Ok(Enumerable.Range(0, 10).Select(i =>
            {
                if (i % 2 == 0)
                    return new Pet { Id = i };
                else
                    return new BigPet { Id = i };
            }));
        }

        [EnableQuery(PageSize = 10, MaxExpansionDepth = 2)]
        public IHttpActionResult Get([FromODataUri] int key)
        {
            return Ok(new BigPet { Id = key });
        }
    }
}
