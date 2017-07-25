﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Web.OData.Batch;
using System.Web.OData.Formatter;
using Microsoft.OData;
using Microsoft.TestCommon;

namespace System.Web.OData.Test
{
    public class ODataBatchResponseItemTest
    {
        [Fact]
        public void WriteMessageAsync_NullWriter_Throws()
        {
            Assert.ThrowsArgumentNull(
                () => ODataBatchResponseItem.WriteMessageAsync(null, new HttpResponseMessage(), CancellationToken.None)
                    .Wait(),
                "writer");
        }

        [Fact]
        public void WriteMessageAsync_NullResponse_Throws()
        {
            HttpContent content = new StringContent(String.Empty, Encoding.UTF8, "multipart/mixed");
            content.Headers.ContentType.Parameters.Add(new NameValueHeaderValue("boundary", Guid.NewGuid().ToString()));
            IODataResponseMessage odataResponse = new ODataMessageWrapper(new MemoryStream(), content.Headers);
            ODataMessageWriter messageWriter = new ODataMessageWriter(odataResponse);

            Assert.ThrowsArgumentNull(
                () => ODataBatchResponseItem.WriteMessageAsync(messageWriter.CreateODataBatchWriter(), null, CancellationToken.None)
                    .Wait(),
                "response");
        }

        [Fact]
        public void WriteMessageAsync_WritesResponseMessage()
        {
            MemoryStream ms = new MemoryStream();
            HttpContent content = new StringContent(String.Empty, Encoding.UTF8, "multipart/mixed");
            content.Headers.ContentType.Parameters.Add(new NameValueHeaderValue("boundary", Guid.NewGuid().ToString()));
            IODataResponseMessage odataResponse = new ODataMessageWrapper(ms, content.Headers);
            var batchWriter = new ODataMessageWriter(odataResponse).CreateODataBatchWriter();
            HttpResponseMessage response = new HttpResponseMessage()
            {
                Content = new StringContent("example content", Encoding.UTF8, "text/example")
            };
            response.Headers.Add("customHeader", "bar");

            batchWriter.WriteStartBatch();
            ODataBatchResponseItem.WriteMessageAsync(batchWriter, response, CancellationToken.None).Wait();
            batchWriter.WriteEndBatch();

            ms.Position = 0;
            string result = new StreamReader(ms).ReadToEnd();

            Assert.Contains("example content", result);
            Assert.Contains("text/example", result);
            Assert.Contains("customHeader", result);
            Assert.Contains("bar", result);
        }

        [Fact]
        public void WriteMessageAsync_ResponseContainsContentId_IfHasContentIdInRequestChangeSet()
        {
            MemoryStream ms = new MemoryStream();
            HttpContent content = new StringContent(String.Empty, Encoding.UTF8, "multipart/mixed");
            content.Headers.ContentType.Parameters.Add(new NameValueHeaderValue("boundary", Guid.NewGuid().ToString()));
            IODataResponseMessage odataResponse = new ODataMessageWrapper(ms, content.Headers);
            var batchWriter = new ODataMessageWriter(odataResponse).CreateODataBatchWriter();
            HttpResponseMessage response = new HttpResponseMessage
            {
                Content = new StringContent("any", Encoding.UTF8, "text/example")
            };
            var request = new HttpRequestMessage();
            var contentId = Guid.NewGuid().ToString();
            request.SetODataContentId(contentId);
            response.RequestMessage = request;

            batchWriter.WriteStartBatch();
            batchWriter.WriteStartChangeset();
            ODataBatchResponseItem.WriteMessageAsync(batchWriter, response, CancellationToken.None).Wait();
            batchWriter.WriteEndChangeset();
            batchWriter.WriteEndBatch();

            ms.Position = 0;
            string result = new StreamReader(ms).ReadToEnd();

            Assert.Contains("any", result);
            Assert.Contains("text/example", result);
            Assert.Contains("Content-ID", result);
            Assert.Contains(contentId, result);
        }
    }
}