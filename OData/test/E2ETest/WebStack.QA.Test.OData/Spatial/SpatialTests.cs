// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.OData;
using System.Web.OData.Extensions;
using Microsoft.OData.Core;
using Microsoft.OData.Edm;
using Newtonsoft.Json.Linq;
using Nuwa;
using WebStack.QA.Test.OData.Common;
using WebStack.QA.Test.OData.ModelBuilder;
using Xunit;

namespace WebStack.QA.Test.OData.Spatial
{
    [NuwaFramework]
    [NuwaTrace(NuwaTraceAttribute.Tag.Off)]
    public class SpatialTests
    {
        [NuwaBaseAddress]
        public string BaseAddress { get; set; }

        [NuwaHttpClient]
        public HttpClient Client { get; set; }

        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration config)
        {
            var controllers = new[] { typeof(SpatialCustomersController), typeof(MetadataController) };
            TestAssemblyResolver resolver = new TestAssemblyResolver(new TypesInjectionAssembly(controllers));

            config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            config.Services.Replace(typeof (IAssembliesResolver), resolver);
            config.Routes.Clear();

            IEdmModel model = IsofEdmModel.GetEdmModel();
            config.MapODataServiceRoute("odata", "odata", model);
        }

        [Fact]
        public async Task SpatialModelMetadataTest()
        {
            string requestUri = string.Format("{0}/odata/$metadata", this.BaseAddress);

            HttpResponseMessage response = await this.Client.GetAsync(requestUri);

            var stream = await response.Content.ReadAsStreamAsync();
            IODataResponseMessage message = new ODataMessageWrapper(stream, response.Content.Headers);
            var reader = new ODataMessageReader(message);
            var edmModel = reader.ReadMetadataDocument();

            var customer = edmModel.SchemaElements.OfType<IEdmEntityType>().Single(et => et.Name == "SpatialCustomer");
            Assert.Equal(5, customer.Properties().Count());

            var locationProperty = customer.DeclaredProperties.Single(p => p.Name == "Location");
            Assert.Equal("Edm.GeographyPoint", locationProperty.Type.FullName());

            var regionProperty = customer.DeclaredProperties.Single(p => p.Name == "Region");
            Assert.Equal("Edm.GeographyLineString", regionProperty.Type.FullName());

            var homePointProperty = customer.DeclaredProperties.Single(p => p.Name == "HomePoint");
            Assert.Equal("Edm.GeometryPoint", homePointProperty.Type.FullName());
        }

        [Fact]
        public async Task QuerySpatialEntity()
        {
            // Arrange
            var requestUri = string.Format("{0}/odata/SpatialCustomers(2)", this.BaseAddress);

            // Act
            HttpResponseMessage response = await Client.GetAsync(requestUri);
            JObject responseString = await response.Content.ReadAsAsync<JObject>();

            // Assert
            Assert.True(HttpStatusCode.OK == response.StatusCode);

            JArray regionData = responseString["Region"]["coordinates"] as JArray;
            Assert.NotNull(regionData);
            Assert.Equal(2, regionData.Count);

            Assert.Equal(66.0, regionData[0][0]);
            Assert.Equal(55.0, regionData[0][1]);
            Assert.Equal(JValue.CreateNull(), regionData[0][2]);
            Assert.Equal(0.0, regionData[0][3]);

            Assert.Equal(44.0, regionData[1][0]);
            Assert.Equal(33, regionData[1][1]);
            Assert.Equal(JValue.CreateNull(), regionData[1][2]);
            Assert.Equal(12.3, regionData[1][3]);
        }

        [Fact]
        public async Task PostSpatialEntity()
        {
            // Arrange
            var requestUri = string.Format("{0}/odata/SpatialCustomers", this.BaseAddress);

            const string payload = @"{
  ""Location"":{
    ""type"":""Point"",""coordinates"":[
      7,8,9,10
    ],""crs"":{
      ""type"":""name"",""properties"":{
        ""name"":""EPSG:4326""
      }
    }
  },""Region"":{
    ""type"":""LineString"",""coordinates"":[
      [
        1.0,1.0
      ],[
        3.0,3.0
      ],[
        4.0,4.0
      ],[
        0.0,0.0
      ]
    ],""crs"":{
      ""type"":""name"",""properties"":{
        ""name"":""EPSG:4326""
      }
    }
  },
  ""Name"":""Sam"",
  ""HomePoint"": {
    ""type"": ""Point"",
    ""coordinates"": [
      4.0,
      10.0
    ],
    ""crs"": {
      ""type"": ""name"",
      ""properties"": {
        ""name"": ""EPSG:0""
      }
    }
  }
}";

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            request.Content = new StringContent(payload);
            request.Content.Headers.ContentType = MediaTypeWithQualityHeaderValue.Parse("application/json");
            HttpResponseMessage response = await Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task UpdateSpatialEntity()
        {
            // Arrange
            var requestUri = string.Format("{0}/odata/SpatialCustomers(3)", this.BaseAddress);

            const string payload = @"{
  ""Location"":{
    ""type"":""Point"",""coordinates"":[
      7,8,9,10
    ],""crs"":{
      ""type"":""name"",""properties"":{
        ""name"":""EPSG:4326""
      }
    }
  }
}";

            // Act
            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("Patch"), requestUri);
            request.Content = new StringContent(payload);
            request.Content.Headers.ContentType = MediaTypeWithQualityHeaderValue.Parse("application/json");
            HttpResponseMessage response = await Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
