// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if !NETCORE // TODO #939: Enable these test on AspNetCore.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Batch;
using Microsoft.AspNet.OData.Routing;
using Microsoft.Test.AspNet.OData.Common;
using Xunit;

namespace Microsoft.Test.AspNet.OData.Batch
{
    public class ODataBatchContentTest
    {
        [Fact]
        public void Parameter_Constructor()
        {
            ODataBatchContent batchContent = CreateBatchContent(new ODataBatchResponseItem[0]);
            var contentType = batchContent.Headers.ContentType;
            var boundary = contentType.Parameters.FirstOrDefault(p => String.Equals(p.Name, "boundary", StringComparison.OrdinalIgnoreCase));
            var odataVersion = batchContent.Headers.FirstOrDefault(h => String.Equals(h.Key, ODataVersionConstraint.ODataServiceVersionHeader, StringComparison.OrdinalIgnoreCase));

            Assert.NotNull(boundary);
            Assert.NotEmpty(boundary.Value);
            Assert.NotEmpty(odataVersion.Value);
            Assert.Equal("multipart/mixed", contentType.MediaType);
        }

        [Fact]
        public void Constructor_Throws_WhenResponsesAreNull()
        {
            ExceptionAssert.ThrowsArgumentNull(
                () => CreateBatchContent(null),
                "responses");
        }

        [Fact]
        public void ODataVersionInWriterSetting_IsPropagatedToTheHeader()
        {
            ODataBatchContent batchContent = CreateBatchContent(new ODataBatchResponseItem[0]);
            var odataVersion = batchContent.Headers.FirstOrDefault(h => String.Equals(h.Key, ODataVersionConstraint.ODataServiceVersionHeader, StringComparison.OrdinalIgnoreCase));

            Assert.Equal("4.0", odataVersion.Value.FirstOrDefault());
        }

        [Fact]
        public async Task SerializeToStreamAsync_WritesODataBatchResponseItems()
        {
            ODataBatchContent batchContent = CreateBatchContent(new ODataBatchResponseItem[]
            {
                new OperationResponseItem(new HttpResponseMessage(HttpStatusCode.OK)),
                new ChangeSetResponseItem(new HttpResponseMessage[]
                {
                    new HttpResponseMessage(HttpStatusCode.Accepted),
                    new HttpResponseMessage(HttpStatusCode.BadRequest)
                })
            });
            HttpResponseMessage response = new HttpResponseMessage
            {
                Content = batchContent
            };

            string responseString = await response.Content.ReadAsStringAsync();

            Assert.Contains("changesetresponse", responseString);
            Assert.Contains("OK", responseString);
            Assert.Contains("Accepted", responseString);
            Assert.Contains("Bad Request", responseString);
        }

        [Fact]
        public void Dispose_DisposesODataBatchResponseItems()
        {
            MockHttpResponseMessage[] responses = new MockHttpResponseMessage[]
            {
                new MockHttpResponseMessage(),
                new MockHttpResponseMessage(),
                new MockHttpResponseMessage()
            };
            ODataBatchContent batchContent = CreateBatchContent(new ODataBatchResponseItem[]
            {
                new OperationResponseItem(responses[0]),
                new ChangeSetResponseItem(new HttpResponseMessage[]
                {
                    responses[1],
                    responses[2]
                }),
            });
            HttpResponseMessage batchResponse = new HttpResponseMessage
            {
                Content = batchContent
            };

            batchResponse.Dispose();

            foreach (var response in responses)
            {
                Assert.True(response.IsDisposed);
            }
        }

        private static ODataBatchContent CreateBatchContent(IEnumerable<ODataBatchResponseItem> responses)
        {
            return new ODataBatchContent(responses, new MockContainer());
        }
    }
}
#endif