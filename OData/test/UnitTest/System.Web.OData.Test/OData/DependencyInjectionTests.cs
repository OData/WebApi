// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using System.Web.OData.Routing.Conventions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.TestCommon;
using Newtonsoft.Json.Linq;
using ServiceLifetime = Microsoft.OData.ServiceLifetime;

namespace System.Web.OData
{
    public class DependencyInjectionTests
    {
        [Fact]
        public void CanAccessContainer_InODataController()
        {
            // Arrange
            const string Uri = "http://localhost/odata/DependencyInjectionModels";
            int randomId = new Random().Next();
            DependencyInjectionModel instance = new DependencyInjectionModel { Id = randomId };
            HttpClient client = GetClient(instance);

            // Act
            HttpResponseMessage response = client.GetAsync(Uri).Result;

            // Assert
            Assert.True(response.IsSuccessStatusCode);

            var result = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            Assert.Equal("http://localhost/odata/$metadata#DependencyInjectionModels/$entity", result["@odata.context"]);
            Assert.Equal(randomId, result["Id"]);
        }

        private static HttpClient GetClient(DependencyInjectionModel instance)
        {
            HttpConfiguration config = new[] { typeof(DependencyInjectionModelsController) }.GetHttpConfiguration();
            IEdmModel model = GetEdmModel();
            config.MapODataServiceRoute("odata", "odata", builder =>
                builder.AddService(ServiceLifetime.Singleton, sp => instance)
                       .AddService(ServiceLifetime.Singleton, sp => model)
                       .AddService<IEnumerable<IODataRoutingConvention>>(ServiceLifetime.Singleton, sp =>
                            ODataRoutingConventions.CreateDefaultWithAttributeRouting("odata", config, model)));
            return new HttpClient(new HttpServer(config));
        }

        private static IEdmModel GetEdmModel()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<DependencyInjectionModel>("DependencyInjectionModels");
            return builder.GetEdmModel();
        }
    }

    public class DependencyInjectionModelsController : ODataController
    {
        [EnableQuery]
        public IHttpActionResult Get()
        {
            IServiceProvider requestContainer = this.Request.GetRequestContainer();
            return Ok(requestContainer.GetRequiredService<DependencyInjectionModel>());
        }
    }

    public class DependencyInjectionModel
    {
        public int Id { get; set; }
    }
}
