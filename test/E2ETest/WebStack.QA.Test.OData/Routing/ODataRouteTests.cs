using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using System.Web.OData.Routing;
using System.Web.OData.Routing.Conventions;
using Microsoft.OData.Edm;
using Nuwa;
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
            configuration.MapODataServiceRoute("noPrefix", "", GetEdmModel(), new DefaultODataPathHandler(), ODataRoutingConventions.CreateDefault());
            configuration.MapODataServiceRoute("prefix", "prefix", GetEdmModel(), new DefaultODataPathHandler(), ODataRoutingConventions.CreateDefault());
            configuration.MapODataServiceRoute("oneParameterInPrefix", "{a}", GetEdmModel(), new DefaultODataPathHandler(), ODataRoutingConventions.CreateDefault());
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
        [InlineData("prefix/")]
        [InlineData("parameter/")]
        public void UrlsGeneratedByFastPathAreConsistentWithUrlsGeneratedWithSlowPath(string requestPath)
        {
            Uri serviceUrl = new Uri(BaseAddress + "/" + requestPath);
            var request = new HttpRequestMessage(HttpMethod.Get, serviceUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=full"));
            var response = Client.SendAsync(request).Result;
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string content = response.Content.ReadAsStringAsync().Result;

            Assert.Contains("odata.context\":\"" + serviceUrl + "$metadata\"", content);
        }
    }
}
