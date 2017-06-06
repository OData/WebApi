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
using WebStack.QA.Test.OData.Formatter.JsonLight.Metadata.Model;
using Xunit;
using Xunit.Extensions;

namespace WebStack.QA.Test.OData.Formatter.JsonLight.Metadata
{
    [NuwaFramework]
    [NuwaHttpClientConfiguration(MessageLog = false)]
    [NuwaTrace(typeof(PlaceholderTraceWriter))]
    public class FeedMetadataTests
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
            configuration.EnableODataSupport(GetEdmModel(configuration));
            configuration.AddODataQueryFilter();
        }

        protected static IEdmModel GetEdmModel(HttpConfiguration config)
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder(config);
            var entitySet = builder.EntitySet<StubEntity>("StubEntity");
            entitySet.EntityType.Collection.Action("Paged").ReturnsCollectionFromEntitySet<StubEntity>("StubEntity");
            return builder.GetEdmModel();
        }

        [NuwaWebConfig]
        public static void UpdateWebConfig(WebConfigHelper config)
        {
            config.AddODataLibAssemblyRedirection();
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
        public void ODataCountAndNextLinkAnnotationsAppearsOnAllMetadataLevelsWhenSpecified(string acceptHeader)
        {
            //Arrange
            StubEntity[] entities = MetadataTestHelpers.CreateInstances<StubEntity[]>();
            HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Post, BaseAddress.ToLowerInvariant() + "/StubEntity/Paged");
            message.SetAcceptHeader(acceptHeader);
            string expectedNextLink = new Uri("http://differentServer:5000/StubEntity/Paged?$skip=" + entities.Length).ToString();

            //Act
            HttpResponseMessage response = Client.SendAsync(message).Result;
            JObject result = response.Content.ReadAsJObject();

            //Assert
            JsonAssert.Equal(entities.Length, "odata.count", result);
            JsonAssert.Equal(expectedNextLink, "odata.nextLink", result);
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
        public void MetadataAnnotationAppearsOnlyForFullAndMinimalMetadata(string acceptHeader)
        {
            //Arrange
            StubEntity[] entities = MetadataTestHelpers.CreateInstances<StubEntity[]>();
            HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get, BaseAddress.ToLowerInvariant() + "/StubEntity/");
            message.SetAcceptHeader(acceptHeader);
            string expectedMetadataUrl = BaseAddress.ToLowerInvariant() + "/$metadata#StubEntity";

            //Act
            HttpResponseMessage response = Client.SendAsync(message).Result;
            JObject result = response.Content.ReadAsJObject();

            //Assert
            if (acceptHeader.Contains("nometadata"))
            {
                JsonAssert.DoesNotContainProperty("odata.metadata", result);
            }
            else
            {
                JsonAssert.Equal(expectedMetadataUrl, "odata.metadata", result);
            }
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
        public void CanReturnTheWholeResultSetUsingNextLink(string acceptHeader)
        {
            //Arrange
            StubEntity[] entities = MetadataTestHelpers.CreateInstances<StubEntity[]>();
            string nextUrlToQuery = BaseAddress.ToLowerInvariant() + "/StubEntity/";
            JToken token = null;
            JArray returnedEntities = new JArray();
            JObject result = null;

            //Act
            do
            {
                HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get, nextUrlToQuery);
                message.SetAcceptHeader(acceptHeader);
                HttpResponseMessage response = Client.SendAsync(message).Result;
                result = response.Content.ReadAsJObject();
                JArray currentResults = (JArray)result["value"];
                for (int i = 0; i < currentResults.Count; i++)
                {
                    returnedEntities.Add(currentResults[i]);
                }
                if (result.TryGetValue("odata.nextLink", out token))
                {
                    nextUrlToQuery = (string)token;
                }
                else
                {
                    nextUrlToQuery = null;
                }
            }
            while (nextUrlToQuery != null);

            //Assert
            Assert.Equal(entities.Length, returnedEntities.Count);
        }
    }
}
