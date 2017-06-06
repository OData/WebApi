using System;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Extensions;
using Microsoft.Data.Edm;
using Newtonsoft.Json.Linq;
using Nuwa;
using WebStack.QA.Common.WebHost;
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
            configuration.Routes.MapODataServiceRoute("Complex", "Complex", GetEdmModel(configuration));
            configuration.AddODataQueryFilter();
        }

        [NuwaWebConfig]
        public static void UpdateWebConfig(WebConfigHelper config)
        {
            config.AddODataLibAssemblyRedirection();
        }

        protected static IEdmModel GetEdmModel(HttpConfiguration configuration)
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder(configuration);
            var entitySet = builder.EntitySet<JsonLightModel.EntityWithComplexProperties>("EntityWithComplexProperties");
            return builder.GetEdmModel();
        }

        [Theory]
        [InlineData("application/json;odata=fullmetadata")]
        [InlineData("application/json;odata=fullmetadata;streaming=true")]
        [InlineData("application/json;odata=fullmetadata;streaming=false")]
        [InlineData("application/json;odata=minimalmetadata")]
        [InlineData("application/json;odata=minimalmetadata;streaming=true")]
        [InlineData("application/json;odata=minimalmetadata;streaming=false")]
        [InlineData("application/json;odata=nometadata")]
        [InlineData("application/json;odata=nometadata;streaming=true")]
        [InlineData("application/json;odata=nometadata;streaming=false")]
        [InlineData("application/json")]
        [InlineData("application/json;streaming=true")]
        [InlineData("application/json;streaming=false")]
        public void ODataTypeAnnotationShouldAppearForComplexTypesLikeCollectionAndUserDefinedTypes(string acceptHeader)
        {
            //Arrange
            string requestUrl = BaseAddress.ToLowerInvariant() + "/Complex/EntityWithComplexProperties/";
            JObject complexProperty;
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            request.SetAcceptHeader(acceptHeader);
            
            //Act
            HttpResponseMessage response = Client.SendAsync(request).Result;
            JObject result = response.Content.ReadAsJObject();

            //Assert
            JsonAssert.ContainsProperty("value", result);
            JArray returnedEntities = (JArray)result["value"];
            for (int i = 0; i < returnedEntities.Count; i++)
            {
                JObject returnedEntity = (JObject)returnedEntities[i];
                if (!acceptHeader.Contains("fullmetadata"))
                {
                    JsonAssert.DoesNotContainProperty("odata.*", returnedEntity);
                }
                else
                {
                    JsonAssert.Equal("Collection(Edm.String)", "StringListProperty@odata.type", returnedEntity);
                    JsonAssert.ContainsProperty("ComplexProperty", returnedEntity);
                    complexProperty = (JObject)returnedEntity["ComplexProperty"];
                    JsonAssert.Equal("WebStack.QA.Test.OData.Formatter.JsonLight.Metadata.Model.ComplexType", "odata.type", complexProperty);
                    JsonAssert.Equal("Collection(Edm.String)", "StringListProperty@odata.type", complexProperty); 
                }
            }
        }
    }
}
