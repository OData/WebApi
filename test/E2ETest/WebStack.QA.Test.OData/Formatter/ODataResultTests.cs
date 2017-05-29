using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using System.Web.OData.Query;
using System.Web.OData.Routing;
using Microsoft.OData.Client;
using Microsoft.OData.Edm;
using Nuwa;
using WebStack.QA.Test.OData.Common;
using WebStack.QA.Test.OData.Common.Controllers;
using Xunit;

namespace WebStack.QA.Test.OData.Formatter
{
    [EntitySet("ODataResult_Model1")]
    [Key("ID")]
    public class ODataResult_Model1
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public ICollection<ODataResult_Model2> Model2 { get; set; }
    }

    [EntitySet("ODataResult_Model2")]
    [Key("ID")]
    public class ODataResult_Model2
    {
        public int ID { get; set; }
        public string Name { get; set; }
    }

    public class ODataResult_Model1Controller : InMemoryODataController<ODataResult_Model1, int>
    {
        public ODataResult_Model1Controller()
            : base("ID")
        {
        }

        [HttpGet]
        public PageResult<ODataResult_Model2> GetModel2(ODataQueryOptions options, int key, int count)
        {
            var models = new List<ODataResult_Model2>();
            for (int i = 0; i < count; i++)
            {
                models.Add(new ODataResult_Model2
                {
                    ID = i,
                    Name = "Test " + i
                });
            }
            var baseUri = new Uri(this.Url.CreateODataLink());
            var uri = new Uri(this.Url.CreateODataLink(new EntitySetPathSegment("ODataResult_Model2")));
            return new PageResult<ODataResult_Model2>(models, baseUri.MakeRelativeUri(uri), count);
        }
    }

    public class ODataResultTests : ODataTestBase
    {
        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
        {
            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            configuration.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;

            configuration.EnableODataSupport(GetEdmModel());
        }

        private static IEdmModel GetEdmModel()
        {
            var mb = new ODataConventionModelBuilder();
            mb.EntitySet<ODataResult_Model1>("ODataResult_Model1");
            mb.EntitySet<ODataResult_Model2>("ODataResult_Model2");

            return mb.GetEdmModel();
        }

        [Fact]
        public async Task ODataResultWithZeroResultShouldWork()
        {
            // Arrange
            var ctx = new DataServiceContext(new Uri(this.BaseAddress), ODataProtocolVersion.V4);
            ctx.Format.UseJson(GetEdmModel());

            ctx.AddObject(
                "ODataResult_Model1",
                new ODataResult_Model1()
                {
                    ID = 1,
                    Name = "ABC"
                });
            await ctx.SaveChangesAsync();

            // Act
            var response = await Client.GetWithAcceptAsync(this.BaseAddress + "/ODataResult_Model1(1)/Model2?count=0", "application/json");
            var responseContentString = await response.Content.ReadAsStringAsync();

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Contains("\"@odata.nextLink\":\"ODataResult_Model2\"", responseContentString);
        }
    }
}
