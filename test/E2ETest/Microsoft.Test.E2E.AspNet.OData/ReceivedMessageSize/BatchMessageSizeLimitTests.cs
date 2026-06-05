// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if NETCORE
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Batch;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.Test.E2E.AspNet.OData.Common;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.ReceivedMessageSize
{
    public class BatchMessageSizeLimitConfiguredTests : WebHostTestBase
    {
        private const int ConfiguredMaxMessageSize = 4096; // 4 KB

        public BatchMessageSizeLimitConfiguredTests(WebHostTestFixture fixture)
            : base(fixture)
        {
        }

        protected override void UpdateConfiguration(WebRouteConfiguration configuration)
        {
            // Set Kestrel limit high so it doesn't interfere.
            configuration.MaxReceivedMessageSize = int.MaxValue;

            // Set OData-level message size limit.
            var odataOptions = configuration.GetDefaultODataOptions();
            odataOptions.MaxReceivedMessageSize = ConfiguredMaxMessageSize;

            var builder = configuration.CreateConventionModelBuilder();
            builder.EntitySet<MessageSizeItem>("MessageSizeItems");

            var batchHandler = configuration.CreateDefaultODataBatchHandler();
            batchHandler.MessageQuotas.MaxReceivedMessageSize = ConfiguredMaxMessageSize;

            configuration.MapODataServiceRoute(
                "batch",
                "odata",
                builder.GetEdmModel(),
                new DefaultODataPathHandler(),
                ODataRoutingConventions.CreateDefault(),
                batchHandler);
        }

        [Fact]
        public async Task Batch_PayloadUnderConfiguredLimit_Succeeds()
        {
            var batchBoundary = "batch_36522ad7-fc75-4b56-8c71-56071383e77b";
            var changesetBoundary = "changeset_36522ad7-fc75-4b56-8c71-56071383e77b";

            var batchBody =
$@"--{batchBoundary}
Content-Type: multipart/mixed; boundary={changesetBoundary}

--{changesetBoundary}
Content-Type: application/http
Content-Transfer-Encoding: binary
Content-ID: 1

POST MessageSizeItems HTTP/1.1
Content-Type: application/json;odata.metadata=minimal
Accept: application/json;odata.metadata=minimal
Accept-Charset: UTF-8
OData-Version: 4.0

{{""Id"":1,""Payload"":""A""}}
--{changesetBoundary}--
--{batchBoundary}--
";
            var request = new HttpRequestMessage(HttpMethod.Post, BaseAddress + "/odata/$batch");
            var content = new StringContent(batchBody);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse($"multipart/mixed; boundary={batchBoundary}");
            request.Content = content;

            HttpResponseMessage response = await Client.SendAsync(request);

            string responseBody = await response.Content.ReadAsStringAsync();
            Assert.True(response.IsSuccessStatusCode,
                $"Expected success for small batch payload but got {response.StatusCode}: {responseBody}");
        }

        [Fact]
        public async Task Batch_PayloadExceedingConfiguredLimit_IsRejected()
        {
            var largePayload = new string('X', 8192);
            var batchBoundary = "batch_36522ad7-fc75-4b56-8c71-56071383e77c";
            var changesetBoundary = "changeset_36522ad7-fc75-4b56-8c71-56071383e77c";

            var batchBody =
$@"--{batchBoundary}
Content-Type: multipart/mixed; boundary={changesetBoundary}

--{changesetBoundary}
Content-Type: application/http
Content-Transfer-Encoding: binary
Content-ID: 1

POST MessageSizeItems HTTP/1.1
Content-Type: application/json;odata.metadata=minimal
Accept: application/json;odata.metadata=minimal
Accept-Charset: UTF-8
OData-Version: 4.0

{{""Id"":2,""Payload"":""{largePayload}""}}
--{changesetBoundary}--
--{batchBoundary}--
";
            var request = new HttpRequestMessage(HttpMethod.Post, BaseAddress + "/odata/$batch");
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("multipart/mixed"));
            var content = new StringContent(batchBody);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse($"multipart/mixed; boundary={batchBoundary}");
            request.Content = content;

            // When batch stream exceeds MaxReceivedMessageSize, OData reader throws an exception.
            // It may surface as an exception from SendAsync or a non-success response.
            HttpResponseMessage response = null;
            Exception exception = null;
            try
            {
                response = await Client.SendAsync(request);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // Either we got an exception, or the response is non-success.
            if (exception != null)
            {
                Assert.Contains("maximum number of bytes allowed", exception.ToString());
            }
            else
            {
                Assert.False(response.IsSuccessStatusCode,
                    $"Expected non-success for batch exceeding limit. Got {response.StatusCode}.");
            }
        }
    }
}
#endif
