// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Web.Http.OData.Batch;
using System.Web.Http.OData.Formatter;
using Microsoft.Data.OData;
using Microsoft.TestCommon;

namespace System.Web.Http
{
    public class OperationResponseItemTest
    {
        [Fact]
        public void Parameter_Constructor()
        {
            HttpResponseMessage response = new HttpResponseMessage();
            OperationResponseItem responseItem = new OperationResponseItem(response);

            Assert.Same(response, responseItem.Response);
        }

        [Fact]
        public void Constructor_NullResponse_Throws()
        {
            Assert.ThrowsArgumentNull(
                () => new OperationResponseItem(null),
                "response");
        }

        [Fact]
        public void WriteResponseAsync_NullWriter_Throws()
        {
            OperationResponseItem responseItem = new OperationResponseItem(new HttpResponseMessage());

            Assert.ThrowsArgumentNull(
                () => responseItem.WriteResponseAsync(null, CancellationToken.None).Wait(),
                "writer");
        }

        [Fact]
        public void WriteResponseAsync_WritesOperation()
        {
            OperationResponseItem responseItem = new OperationResponseItem(new HttpResponseMessage(HttpStatusCode.Accepted));
            MemoryStream memoryStream = new MemoryStream();
            IODataResponseMessage responseMessage = new ODataMessageWrapper(memoryStream);
            ODataMessageWriter writer = new ODataMessageWriter(responseMessage);
            ODataBatchWriter batchWriter = writer.CreateODataBatchWriter();
            batchWriter.WriteStartBatch();

            responseItem.WriteResponseAsync(batchWriter, CancellationToken.None).Wait();

            batchWriter.WriteEndBatch();
            memoryStream.Position = 0;
            string responseString = new StreamReader(memoryStream).ReadToEnd();

            Assert.Contains("Accepted", responseString);
        }

        [Fact]
        public void Dispose_DisposesHttpResponseMessage()
        {
            OperationResponseItem responseItem = new OperationResponseItem(new MockHttpResponseMessage());

            responseItem.Dispose();

            Assert.True(((MockHttpResponseMessage)responseItem.Response).IsDisposed);
        }
    }
}