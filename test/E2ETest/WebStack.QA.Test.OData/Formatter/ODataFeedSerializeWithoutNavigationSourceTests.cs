using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.OData;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using System.Web.OData.Routing;
using Microsoft.OData.Edm;
using Newtonsoft.Json.Linq;
using Nuwa;
using WebStack.QA.Test.OData.Common;
using Xunit;

namespace WebStack.QA.Test.OData.Formatter
{
    [NuwaFramework]
    [NuwaTrace(NuwaTraceAttribute.Tag.Off)]
    public class ODataFeedSerializeWithoutNavigationSourceTests
    {
        [NuwaBaseAddress]
        public string BaseAddress { get; set; }

        [NuwaHttpClient]
        public HttpClient Client { get; set; }

        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration config)
        {
            var controllers = new[] { typeof(AnyController), typeof(MetadataController) };
            TestAssemblyResolver resolver = new TestAssemblyResolver(new TypesInjectionAssembly(controllers));
            config.Services.Replace(typeof(IAssembliesResolver), resolver);

            config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            config.MapODataServiceRoute("odata", "odata", GetModel());
        }

        private static IEdmModel GetModel()
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<DerivedTypeA>("SetA");
            builder.EntitySet<DerivedTypeB>("SetB");

            builder.EntityType<BaseType>(); // this line is necessary.
            builder.Function("ReturnAll").ReturnsCollection<BaseType>();

            return builder.GetEdmModel();
        }

        [Fact]
        public void CanSerializeFeedWithoutNavigationSource()
        {
            // Arrange
            string requestUri = BaseAddress + "/odata/ReturnAll";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            // Act
            var response = Client.SendAsync(request).Result;

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);

            JObject content = response.Content.ReadAsAsync<JObject>().Result;
            Assert.Contains("/odata/$metadata#Collection(WebStack.QA.Test.OData.Formatter.BaseType)", content["@odata.context"].ToString());

            Assert.Equal(2, content["value"].Count());

            // #1
            Assert.Equal("#WebStack.QA.Test.OData.Formatter.DerivedTypeA", content["value"][0]["@odata.type"].ToString());
            Assert.Equal(1, content["value"][0]["Id"]);

            // #2
            Assert.Equal("#WebStack.QA.Test.OData.Formatter.DerivedTypeB", content["value"][1]["@odata.type"].ToString());
            Assert.Equal(2, content["value"][1]["Id"]);
        }
    }

    public class AnyController : ODataController
    {
        public static IList<BaseType> Entities = new List<BaseType>();

        static AnyController()
        {
            DerivedTypeA a = new DerivedTypeA
            {
                Id = 1,
                Name = "Name #1",
                PropertyA = 88
            };
            Entities.Add(a);

            DerivedTypeB b = new DerivedTypeB
            {
                Id = 2,
                Name = "Name #2",
                PropertyB = 99.9,
            };
            Entities.Add(b);
        }

        [HttpGet]
        [ODataRoute("ReturnAll")]
        public IHttpActionResult ReturnAll()
        {
            return Ok(Entities);
        }
    }

    public abstract class BaseType
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }

    public class DerivedTypeA : BaseType
    {
        public int PropertyA { get; set; }
    }

    public class DerivedTypeB : BaseType
    {
        public double PropertyB { get; set; }
    }
}
