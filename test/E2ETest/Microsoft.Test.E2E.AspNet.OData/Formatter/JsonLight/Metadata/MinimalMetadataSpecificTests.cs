// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.Formatter.JsonLight.Metadata
{
    public class MinimalMetadataSpecificTests : WebHostTestBase
    {
        public MinimalMetadataSpecificTests(WebHostTestFixture fixture)
            :base(fixture)
        {
        }

        protected override void UpdateConfiguration(HttpConfiguration config)
        {
            config.Routes.Clear();
            config.MapODataServiceRoute("odata", "odata", GetModel(), new DefaultODataPathHandler(), ODataRoutingConventions.CreateDefault());
        }

        private static IEdmModel GetModel()
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder();
            var pets = builder.EntitySet<Pet>("Pets");
            builder.EntityType<BigPet>();
            return builder.GetEdmModel();
        }

        [Fact]
        public async Task QueryWithCastDoesntContainODataType()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, BaseAddress + "/odata/Pets(5)/Microsoft.Test.E2E.AspNet.OData.Formatter.JsonLight.Metadata.BigPet");

            // Act
            HttpResponseMessage response = await Client.SendAsync(request);
            string payload = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("\"@odata.context\":", payload);
            Assert.Contains("\"Id\":5", payload);
            Assert.DoesNotContain("@odata.type", payload);
            Assert.DoesNotContain("#Microsoft.Test.E2E.AspNet.OData.Formatter.JsonLight.Metadata.BigPet", payload);
        }

        [Fact]
        public async Task QueryWithoutCastContainsODataType()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, BaseAddress + "/odata/Pets(5)");

            // Act
            HttpResponseMessage response = await Client.SendAsync(request);
            string payload = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("\"@odata.context\":", payload);
            Assert.Contains("\"@odata.type\":\"#Microsoft.Test.E2E.AspNet.OData.Formatter.JsonLight.Metadata.BigPet\"", payload);
            Assert.Contains("\"Id\":5", payload);
        }
    }

    public class Pet
    {
        public int Id { get; set; }
    }

    public class BigPet : Pet
    {
    }

    public class PetsController : ODataController
    {
        [EnableQuery(PageSize = 10, MaxExpansionDepth = 2)]
        public IHttpActionResult Get()
        {
            return Ok(Enumerable.Range(0, 10).Select(i =>
            {
                if (i % 2 == 0)
                    return new Pet { Id = i };
                else
                    return new BigPet { Id = i };
            }));
        }

        [EnableQuery(PageSize = 10, MaxExpansionDepth = 2)]
        public IHttpActionResult Get([FromODataUri] int key)
        {
            return Ok(new BigPet { Id = key });
        }
    }
}
