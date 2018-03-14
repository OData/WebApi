﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.OData.Batch;
using System.Web.OData.Extensions;
using Microsoft.OData;
using Microsoft.TestCommon;

namespace System.Web.OData.Test
{
    public class ODataBatchContentTest
    {
        [Fact]
        public void Parameter_Constructor()
        {
            ODataBatchContent batchContent = CreateBatchContent(new ODataBatchResponseItem[0]);
            var contentType = batchContent.Headers.ContentType;
            var boundary = contentType.Parameters.FirstOrDefault(p => String.Equals(p.Name, "boundary", StringComparison.OrdinalIgnoreCase));
            var odataVersion = batchContent.Headers.FirstOrDefault(h => String.Equals(h.Key, HttpRequestMessageProperties.ODataServiceVersionHeader, StringComparison.OrdinalIgnoreCase));

            Assert.NotNull(boundary);
            Assert.NotEmpty(boundary.Value);
            Assert.NotNull(odataVersion);
            Assert.NotEmpty(odataVersion.Value);
            Assert.Equal("multipart/mixed", contentType.MediaType);
        }

        [Fact]
        public void Constructor_Throws_WhenResponsesAreNull()
        {
            Assert.ThrowsArgumentNull(
                () => CreateBatchContent(null),
                "responses");
        }

        [Fact]
        public void ODataVersionInWriterSetting_IsPropagatedToTheHeader()
        {
            ODataBatchContent batchContent = CreateBatchContent(new ODataBatchResponseItem[0]);
            var odataVersion = batchContent.Headers.FirstOrDefault(h => String.Equals(h.Key, HttpRequestMessageProperties.ODataServiceVersionHeader, StringComparison.OrdinalIgnoreCase));

            Assert.NotNull(odataVersion);
            Assert.Equal("4.0", odataVersion.Value.FirstOrDefault());
        }

        [Fact]
        public void SerializeToStreamAsync_WritesODataBatchResponseItems()
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

            string responseString = response.Content.ReadAsStringAsync().Result;

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