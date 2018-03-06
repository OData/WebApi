// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if !NETCORE // TODO #939: Enable these test on AspNetCore.
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.Test.AspNet.OData.Factories;
using Newtonsoft.Json.Linq;
using Xunit;
using ServiceLifetime = Microsoft.OData.ServiceLifetime;

namespace Microsoft.Test.AspNet.OData
{
    public class DependencyInjectionTests
    {
        [Fact]
        public async Task CanAccessContainer_InODataController()
        {
            // Arrange
            const string Uri = "http://localhost/odata/DependencyInjectionModels";
            int randomId = new Random().Next();
            DependencyInjectionModel instance = new DependencyInjectionModel { Id = randomId };
            HttpClient client = GetClient(instance);

            // Act
            HttpResponseMessage response = await client.GetAsync(Uri);

            // Assert
            Assert.True(response.IsSuccessStatusCode);

            var result = JObject.Parse(await response.Content.ReadAsStringAsync());
            Assert.Equal("http://localhost/odata/$metadata#DependencyInjectionModels/$entity", result["@odata.context"]);
            Assert.Equal(randomId, result["Id"]);
        }

        private static HttpClient GetClient(DependencyInjectionModel instance)
        {
            var config = RoutingConfigurationFactory.CreateWithTypes(new[] { typeof(DependencyInjectionModelsController) });
            IEdmModel model = GetEdmModel();
            config.MapODataServiceRoute("odata", "odata", builder =>
                builder.AddService(ServiceLifetime.Singleton, sp => instance)
                       .AddService(ServiceLifetime.Singleton, sp => model)
                       .AddService<IEnumerable<IODataRoutingConvention>>(ServiceLifetime.Singleton, sp =>
                            ODataRoutingConventions.CreateDefaultWithAttributeRouting("odata", config)));
            return new HttpClient(new HttpServer(config));
        }

        private static IEdmModel GetEdmModel()
        {
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
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
#endif