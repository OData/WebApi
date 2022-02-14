//-----------------------------------------------------------------------------
// <copyright file="SupportDollarValue.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

#if NETCORE
using System.Linq;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;
#else
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http.Dispatcher;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Xunit;
#endif

namespace Microsoft.Test.E2E.AspNet.OData.Formatter.Extensibility
{
    public class EntityWithPrimitiveAndBinaryProperty
    {
        public int Id { get; set; }
        public long LongProperty { get; set; }
        public byte[] BinaryProperty { get; set; }
        public long? NullableLongProperty { get; set; }
    }

    public class EntityWithPrimitiveAndBinaryPropertyController : TestODataController
    {
        private static readonly EntityWithPrimitiveAndBinaryProperty ENTITY;

        static EntityWithPrimitiveAndBinaryPropertyController()
        {
            ENTITY = new EntityWithPrimitiveAndBinaryProperty
            {
                Id = 1,
                LongProperty = long.MaxValue,
                BinaryProperty = Enumerable.Range(1, 10).Select(x => (byte)x).ToArray(),
                NullableLongProperty = null
            };
        }

        public long GetLongProperty(int key)
        {
            return ENTITY.LongProperty;
        }

        public byte[] GetBinaryProperty(int key)
        {
            return ENTITY.BinaryProperty;
        }

        public long? GetNullableLongProperty(int key)
        {
            return ENTITY.NullableLongProperty;
        }
    }

#if !NETCORE // TODO #939: Enable these tests for AspNetCore
    public class SupportDollarValueTest : WebHostTestBase
    {
        public SupportDollarValueTest(WebHostTestFixture fixture)
            :base(fixture)
        {
        }

        protected override void UpdateConfiguration(WebRouteConfiguration configuration)
        {
            configuration.
                MapODataServiceRoute(
                    routeName: "RawValue",
                    routePrefix: "RawValue",
                    model: GetEdmModel(configuration), pathHandler: new DefaultODataPathHandler(),
                    routingConventions: ODataRoutingConventions.CreateDefault(),
                    defaultHandler: HttpClientFactory.CreatePipeline(innerHandler: new HttpControllerDispatcher(configuration), handlers: new[] { new ODataNullValueMessageHandler() }));
        }

        protected static IEdmModel GetEdmModel(WebRouteConfiguration configuration)
        {
            ODataModelBuilder builder = configuration.CreateConventionModelBuilder();
            var parentSet = builder.EntitySet<EntityWithPrimitiveAndBinaryProperty>("EntityWithPrimitiveAndBinaryProperty");
            return builder.GetEdmModel();
        }

        [Fact]
        public async Task CanExtendTheFormatterToSupportPrimitiveRawValues()
        {
            // Arrange
            string requestUrl = BaseAddress + "/RawValue/EntityWithPrimitiveAndBinaryProperty(1)/LongProperty/$value";
            HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            message.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));

            // Act
            HttpResponseMessage response = await Client.SendAsync(message);
            long result = long.Parse(await response.Content.ReadAsStringAsync());

            // Assert
            Assert.Equal(long.MaxValue, result);
        }

        [Fact]
        public async Task CanExtendTheFormatterToSupportBinaryRawValues()
        {
            // Arrange
            string requestUrl = BaseAddress + "/RawValue/EntityWithPrimitiveAndBinaryProperty(1)/BinaryProperty/$value";
            HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            message.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));

            // Act
            HttpResponseMessage response = await Client.SendAsync(message);
            byte[] result = await response.Content.ReadAsByteArrayAsync();

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.True(new HashSet<byte>(Enumerable.Range(1, 10).Select(x => (byte)x)).SetEquals(result));
        }

        [Fact]
        public async Task CanExtendTheFormatterToSupportNullRawValues()
        {
            // Arrange
            string requestUrl = BaseAddress + "/RawValue/EntityWithPrimitiveAndBinaryProperty(1)/NullableLongProperty/$value";
            HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            message.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));

            // Act
            HttpResponseMessage response = await Client.SendAsync(message);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }
    }
#endif
}
