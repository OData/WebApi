using Microsoft.Data.Edm;
using Nuwa;
using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Extensions;
using WebStack.QA.Common.WebHost;
using WebStack.QA.Test.OData.Common;
using Xunit;
using Xunit.Extensions;

namespace WebStack.QA.Test.OData.Routing
{
    [NuwaFramework]
    [NuwaHttpClientConfiguration(MessageLog = false)]
    [NuwaTrace(typeof(PlaceholderTraceWriter))]
    public class ODataRouteTests
    {
        private string baseAddress = null;

        [NuwaBaseAddress]
        public string BaseAddress
        {
            get
            {
                return baseAddress;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    this.baseAddress = value.Replace("localhost", Environment.MachineName);
                }
            }
        }

        [NuwaHttpClient]
        public HttpClient Client { get; set; }

        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
        {
            HttpServer server = configuration.Properties["Nuwa.HttpServerKey"] as HttpServer;

            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            configuration.Routes.MapODataServiceRoute("noPrefix", "", GetEdmModel());
            configuration.Routes.MapODataServiceRoute("prefix", "prefix", GetEdmModel());
            configuration.Routes.MapODataServiceRoute("oneParameterInPrefix", "{a}", GetEdmModel());
        }


        protected static IEdmModel GetEdmModel()
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder();
            return builder.GetEdmModel();
        }

        [NuwaWebConfig]
        public static void UpdateWebConfig(WebConfigHelper webConfig)
        {
            webConfig.AddAppSection("aspnet:UseTaskFriendlySynchronizationContext", "true");
        }

        [Theory]
        [InlineData("")]
        [InlineData("prefix")]
        [InlineData("parameter")]
        public void UrlsGeneratedByFastPathAreConsistentWithUrlsGeneratedWithSlowPath(string requestPath)
        {
            Uri serviceUrl = new Uri(BaseAddress + "/" + requestPath);
            var response = Client.SendAsync(new HttpRequestMessage(HttpMethod.Get, serviceUrl)).Result;
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string content = response.Content.ReadAsStringAsync().Result;

            Assert.Contains("xml:base=\"" + serviceUrl + "\"", content);
        }
    }
}
