// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if !NETCORE // TODO #939: Enable these test on AspNetCore.
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Batch;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.OData;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Batch
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
            ExceptionAssert.ThrowsArgumentNull(
                () => new OperationResponseItem(null),
                "response");
        }

        [Fact]
        public async Task WriteResponseAsync_NullWriter_Throws()
        {
            OperationResponseItem responseItem = new OperationResponseItem(new HttpResponseMessage());

            await ExceptionAssert.ThrowsArgumentNullAsync(
                () => responseItem.WriteResponseAsync(null, CancellationToken.None),
                "writer");
        }

        [Fact]
        public async Task WriteResponseAsync_SynchronouslyWritesOperation()
        {
            OperationResponseItem responseItem = new OperationResponseItem(new HttpResponseMessage(HttpStatusCode.Accepted));
            MemoryStream memoryStream = new MemoryStream();
            IODataResponseMessage responseMessage = new ODataMessageWrapper(memoryStream);
            ODataMessageWriter writer = new ODataMessageWriter(responseMessage);
            ODataBatchWriter batchWriter = writer.CreateODataBatchWriter();
            batchWriter.WriteStartBatch();

            // For backward compatibility, default is to write to use a synchronous batchWriter.
            await responseItem.WriteResponseAsync(batchWriter, CancellationToken.None);

            batchWriter.WriteEndBatch();
            memoryStream.Position = 0;
            string responseString = new StreamReader(memoryStream).ReadToEnd();

            Assert.Contains("Accepted", responseString);
        }

        [Fact]
        public async Task WriteResponseAsync_AsynchronouslyWritesOperation()
        {
            OperationResponseItem responseItem = new OperationResponseItem(new HttpResponseMessage(HttpStatusCode.Accepted));
            MemoryStream memoryStream = new MemoryStream();
            IODataResponseMessage responseMessage = new ODataMessageWrapper(memoryStream);
            ODataMessageWriter writer = new ODataMessageWriter(responseMessage);
            ODataBatchWriter batchWriter = await writer.CreateODataBatchWriterAsync();
            await batchWriter.WriteStartBatchAsync();

            await responseItem.WriteResponseAsync(batchWriter, CancellationToken.None, /*writeAsync*/ true);

            await batchWriter.WriteEndBatchAsync();
            memoryStream.Position = 0;
            string responseString = new StreamReader(memoryStream).ReadToEnd();

            Assert.Contains("Accepted", responseString);
        }

        [Fact]
        public void IsResponseSucess_TestResponse()
        {
            // Arrange
            OperationResponseItem successResponseItem = new OperationResponseItem(new HttpResponseMessage(HttpStatusCode.OK));
            OperationResponseItem errorResponseItem = new OperationResponseItem(new HttpResponseMessage(HttpStatusCode.Ambiguous));

            // Act & Assert
            Assert.True(successResponseItem.IsResponseSuccessful());
            Assert.False(errorResponseItem.IsResponseSuccessful());
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
#endif