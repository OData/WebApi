//-----------------------------------------------------------------------------
// <copyright file="DependencyInjectionTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Newtonsoft.Json.Linq;
using Xunit;
using ServiceLifetime = Microsoft.OData.ServiceLifetime;

namespace Microsoft.AspNet.OData.Test
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
            IEdmModel model = GetEdmModel();
            var controllers = new[] { typeof(DependencyInjectionModelsController) };
            var server = TestServerFactory.Create(controllers, config =>
            {
                config.MapODataServiceRoute("odata", "odata", builder =>
                builder.AddService(ServiceLifetime.Singleton, sp => instance)
                       .AddService(ServiceLifetime.Singleton, sp => model)
                       .AddService<IEnumerable<IODataRoutingConvention>>(ServiceLifetime.Singleton, sp =>
                            ODataRoutingConventions.CreateDefaultWithAttributeRouting("odata", config)));
            });

            return TestServerFactory.CreateClient(server);
        }

        private static IEdmModel GetEdmModel()
        {
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntitySet<DependencyInjectionModel>("DependencyInjectionModels");
            return builder.GetEdmModel();
        }
    }

    public class DependencyInjectionModelsController : TestODataController
    {
        [EnableQuery]
        public ITestActionResult Get()
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
