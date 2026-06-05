// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if NETCORE
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.Test.E2E.AspNet.OData.Common;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.ReceivedMessageSize
{
    public class MessageSizeLimitConfiguredTests : WebHostTestBase
    {
        private const int ConfiguredMaxMessageSize = 1024; // 1 KB

        public MessageSizeLimitConfiguredTests(WebHostTestFixture fixture)
            : base(fixture)
        {
        }

        protected override void UpdateConfiguration(WebRouteConfiguration configuration)
        {
            // Set Kestrel limit high so it doesn't interfere with OData-level testing.
            configuration.MaxReceivedMessageSize = int.MaxValue;

            // Set OData-level message size limit.
            var odataOptions = configuration.GetDefaultODataOptions();
            odataOptions.MaxReceivedMessageSize = ConfiguredMaxMessageSize;

            var builder = configuration.CreateConventionModelBuilder();
            builder.EntitySet<MessageSizeItem>("MessageSizeItems");
            configuration.EnableODataSupport(builder.GetEdmModel());
        }

        [Fact]
        public async Task Post_PayloadUnderConfiguredLimit_Succeeds()
        {
            // ~500 bytes, well under 1 KB limit
            string payload = new string('X', 500);
            string json = "{\"Id\":1,\"Payload\":\"" + payload + "\"}";
            var content = new StringContent(json, Encoding.UTF8);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, BaseAddress + "/MessageSizeItems")
            {
                Content = content
            };

            HttpResponseMessage response = await Client.SendAsync(request);

            Assert.True(response.IsSuccessStatusCode,
                $"Expected success but got {response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
        }

        [Fact]
        public async Task Post_PayloadExceedingConfiguredLimit_IsRejected()
        {
            // ~2 KB exceeds 1 KB limit
            string payload = new string('X', 2048);
            string json = "{\"Id\":1,\"Payload\":\"" + payload + "\"}";
            var content = new StringContent(json, Encoding.UTF8);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, BaseAddress + "/MessageSizeItems")
            {
                Content = content
            };

            HttpResponseMessage response = await Client.SendAsync(request);

            Assert.False(response.IsSuccessStatusCode,
                $"Expected non-success for payload exceeding configured limit. Got {response.StatusCode}.");
        }
    }
}
#endif
