using System;
using System.Net.Http;
using System.Web.Http;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using System.Web.OData.Routing;
using System.Web.OData.Routing.Conventions;
using Microsoft.OData.Edm;
using Newtonsoft.Json.Linq;
using Nuwa;
using WebStack.QA.Test.OData.Common;
using Xunit.Extensions;
using JsonLightModel = WebStack.QA.Test.OData.Formatter.JsonLight.Metadata.Model;

namespace WebStack.QA.Test.OData.Formatter.JsonLight.Metadata
{
    [NuwaFramework]
    [NuwaHttpClientConfiguration(MessageLog = false)]
    [NuwaTrace(typeof(PlaceholderTraceWriter))]
    public class ComplexTypeTests
    {
        private string _baseAddress;

        [NuwaBaseAddress]
        public string BaseAddress
        {
            get { return _baseAddress; }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _baseAddress = value.Replace("localhost", Environment.MachineName);
                }
            }
        }

        [NuwaHttpClient]
        public HttpClient Client { get; set; }

        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
        {
            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            configuration.MapODataServiceRoute("Complex", "Complex", GetEdmModel(configuration), new DefaultODataPathHandler(), ODataRoutingConventions.CreateDefault());
            configuration.AddODataQueryFilter();
        }

        protected static IEdmModel GetEdmModel(HttpConfiguration configuration)
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder(configuration);
            var entitySet = builder.EntitySet<JsonLightModel.EntityWithComplexProperties>("EntityWithComplexProperties");
            return builder.GetEdmModel();
        }

        [Theory]
        [InlineData("application/json;odata.metadata=full")]
        [InlineData("application/json;odata.metadata=full;odata.streaming=true")]
        [InlineData("application/json;odata.metadata=full;odata.streaming=false")]
        [InlineData("application/json;odata.metadata=minimal")]
        [InlineData("application/json;odata.metadata=minimal;odata.streaming=true")]
        [InlineData("application/json;odata.metadata=minimal;odata.streaming=false")]
        [InlineData("application/json;odata.metadata=none")]
        [InlineData("application/json;odata.metadata=none;odata.streaming=true")]
        [InlineData("application/json;odata.metadata=none;odata.streaming=false")]
        [InlineData("application/json")]
        [InlineData("application/json;odata.streaming=true")]
        [InlineData("application/json;odata.streaming=false")]
        public void ODataTypeAnnotationShouldAppearForComplexTypesLikeCollectionAndUserDefinedTypes(string acceptHeader)
        {
            //Arrange
            string requestUrl = BaseAddress.ToLowerInvariant() + "/Complex/EntityWithComplexProperties/";
            JObject complexProperty;
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            request.SetAcceptHeader(acceptHeader);

            //Act
            HttpResponseMessage response = Client.SendAsync(request).Result;
            JObject result = response.Content.ReadAsAsync<JObject>().Result;

            //Assert
            JsonAssert.ContainsProperty("value", result);
            JArray returnedEntities = (JArray)result["value"];
            for (int i = 0; i < returnedEntities.Count; i++)
            {
                JObject returnedEntity = (JObject)returnedEntities[i];
                if (!acceptHeader.Contains("odata.metadata=full"))
                {
                    JsonAssert.DoesNotContainProperty("@odata.*", returnedEntity);
                }
                else
                {
                    JsonAssert.PropertyEquals("#WebStack.QA.Test.OData.Formatter.JsonLight.Metadata.Model.EntityWithComplexProperties",
                        "@odata.type", returnedEntity);
                    JsonAssert.PropertyEquals("#Collection(String)", "StringListProperty@odata.type", returnedEntity);
                    JsonAssert.ContainsProperty("ComplexProperty", returnedEntity);
                    complexProperty = (JObject)returnedEntity["ComplexProperty"];
                    JsonAssert.PropertyEquals("#WebStack.QA.Test.OData.Formatter.JsonLight.Metadata.Model.ComplexType",
                        "@odata.type", complexProperty);
                    JsonAssert.PropertyEquals("#Collection(String)", "StringListProperty@odata.type", complexProperty);
                }
            }
        }
    }
}
