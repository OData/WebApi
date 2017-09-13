// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.OData.Edm;
using Nuwa;
using WebStack.QA.Common.WebHost;
using WebStack.QA.Test.OData.Common;
using Xunit;

namespace WebStack.QA.Test.OData.Formatter.Extensibility
{
    public class EntityWithPrimitiveAndBinaryProperty
    {
        public int Id { get; set; }
        public long LongProperty { get; set; }
        public byte[] BinaryProperty { get; set; }
        public long? NullableLongProperty { get; set; }
    }

    public class EntityWithPrimitiveAndBinaryPropertyController : ODataController
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

    [NuwaFramework]
    [NuwaHttpClientConfiguration(MessageLog = false)]
    [NuwaTrace(typeof(PlaceholderTraceWriter))]
    public class SupportDollarValueTest
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
            configuration.
                MapODataServiceRoute(
                    routeName: "RawValue",
                    routePrefix: "RawValue",
                    model: GetEdmModel(configuration), pathHandler: new DefaultODataPathHandler(),
                    routingConventions: ODataRoutingConventions.CreateDefault(),
                    defaultHandler: HttpClientFactory.CreatePipeline(innerHandler: new HttpControllerDispatcher(configuration), handlers: new[] { new ODataNullValueMessageHandler() }));
        }

        [NuwaWebConfig]
        public static void UpdateWebConfig(WebConfigHelper config)
        {
            //config.AddODataLibAssemblyRedirection();
        }

        protected static IEdmModel GetEdmModel(HttpConfiguration configuration)
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder(configuration);
            var parentSet = builder.EntitySet<EntityWithPrimitiveAndBinaryProperty>("EntityWithPrimitiveAndBinaryProperty");
            return builder.GetEdmModel();
        }

        [Fact]
        public void CanExtendTheFormatterToSupportPrimitiveRawValues()
        {
            // Arrange
            string requestUrl = BaseAddress + "/RawValue/EntityWithPrimitiveAndBinaryProperty(1)/LongProperty/$value";
            HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            message.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));

            // Act
            HttpResponseMessage response = Client.SendAsync(message).Result;
            long result = long.Parse(response.Content.ReadAsStringAsync().Result);

            // Assert
            Assert.Equal(long.MaxValue, result);
        }

        [Fact]
        public void CanExtendTheFormatterToSupportBinaryRawValues()
        {
            // Arrange
            string requestUrl = BaseAddress + "/RawValue/EntityWithPrimitiveAndBinaryProperty(1)/BinaryProperty/$value";
            HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            message.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));

            // Act
            HttpResponseMessage response = Client.SendAsync(message).Result;
            byte[] result = response.Content.ReadAsByteArrayAsync().Result;

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.True(new HashSet<byte>(Enumerable.Range(1, 10).Select(x => (byte)x)).SetEquals(result));
        }

        [Fact]
        public void CanExtendTheFormatterToSupportNullRawValues()
        {
            // Arrange
            string requestUrl = BaseAddress + "/RawValue/EntityWithPrimitiveAndBinaryProperty(1)/NullableLongProperty/$value";
            HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            message.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));

            // Act
            HttpResponseMessage response = Client.SendAsync(message).Result;

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }
    }
}
