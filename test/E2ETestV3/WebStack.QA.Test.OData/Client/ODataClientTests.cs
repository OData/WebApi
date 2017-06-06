using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.OData.Builder;
using Microsoft.Data.Edm;
using Nuwa;
using WebStack.QA.Js.Client;
using WebStack.QA.Js.Server;
using WebStack.QA.Test.OData.Common;
using WebStack.QA.Test.OData.Common.Controllers;
using WebStack.QA.Test.OData.Common.Models.ProductFamilies;
using Xunit;

namespace WebStack.QA.Test.OData.Client
{
    public class ODataClientTests_ProductsController : InMemoryEntitySetController<Product, int>
    {
        public ODataClientTests_ProductsController()
            : base("ID")
        {
        }
    }

    public class ODataClientTests : ODataTestBase
    {
        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
        {
            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            JsServerSettings settings = new JsServerSettings();
            settings.Builder = new QUnitPageBuilder();
            settings.Builder.ScriptReferences.Add("http://ajax.googleapis.com/ajax/libs/jquery/1.9.0/jquery.min.js");
            settings.Builder.ScriptCode.Add(
@"
if (!String.prototype.format) {
    String.prototype.format = function () {
        var args = arguments;
        return this.replace(/{(\d+)}/g, function (match, number) {
            return typeof args[number] != 'undefined'
          ? args[number]
          : match;
        });
    };
}
$.ajaxSetup({
    async:false
});
");
            settings.ResourceLoadFrom.Add(typeof(ODataClientTests).Assembly);
            configuration.SetupJsTestServer("js", settings);

            configuration.EnableODataSupport(GetEdmModel(configuration));
        }

        [NuwaWebConfig]
        public static void UpdateWebConfig(WebStack.QA.Common.WebHost.WebConfigHelper config)
        {
            config.AddODataLibAssemblyRedirection();
        }

        private static IEdmModel GetEdmModel(HttpConfiguration configuration)
        {
            var mb = new ODataConventionModelBuilder(configuration);
            mb.EntitySet<Product>("ODataClientTests_Products").EntityType.Ignore(p => p.Family);
            return mb.GetEdmModel();
        }

        [Fact]
        public void ExcelRequestWithoutAcceptHeaderShouldReturnAtomResponse()
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, this.BaseAddress + "/");
            request.Headers.UserAgent.Add(new ProductInfoHeaderValue("PowerPivot", null));
            request.Headers.Connection.Add("Keep-Alive");
            var response = this.Client.SendAsync(request).Result;
            Assert.Equal("application/atomsvc+xml", response.Content.Headers.ContentType.MediaType);
        }
    }
}
