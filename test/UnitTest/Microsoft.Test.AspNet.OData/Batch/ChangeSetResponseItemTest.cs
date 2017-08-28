// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using Microsoft.AspNet.OData.Batch;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.OData;
using Microsoft.Test.AspNet.OData.TestCommon;

namespace Microsoft.Test.AspNet.OData.Batch
{
    public class ChangeSetResponseItemTest
    {
        [Fact]
        public void Parameter_Constructor()
        {
            HttpResponseMessage[] responses = new HttpResponseMessage[0];
            ChangeSetResponseItem responseItem = new ChangeSetResponseItem(responses);

            Assert.Same(responses, responseItem.Responses);
        }

        [Fact]
        public void Constructor_NullResponses_Throws()
        {
            Assert.ThrowsArgumentNull(
                () => new ChangeSetResponseItem(null),
                "responses");
        }

        [Fact]
        public void WriteResponseAsync_NullWriter_Throws()
        {
            ChangeSetResponseItem responseItem = new ChangeSetResponseItem(new HttpResponseMessage[0]);

            Assert.ThrowsArgumentNull(
                () => responseItem.WriteResponseAsync(null, CancellationToken.None).Wait(),
                "writer");
        }

        [Fact]
        public void WriteResponseAsync_WritesChangeSet()
        {
            HttpResponseMessage[] responses = new HttpResponseMessage[]
                {
                    new HttpResponseMessage(HttpStatusCode.Accepted),
                    new HttpResponseMessage(HttpStatusCode.NoContent)
                };
            ChangeSetResponseItem responseItem = new ChangeSetResponseItem(responses);
            MemoryStream memoryStream = new MemoryStream();
            IODataResponseMessage responseMessage = new ODataMessageWrapper(memoryStream);
            ODataMessageWriter writer = new ODataMessageWriter(responseMessage);
            ODataBatchWriter batchWriter = writer.CreateODataBatchWriter();
            batchWriter.WriteStartBatch();

            responseItem.WriteResponseAsync(batchWriter, CancellationToken.None).Wait();

            batchWriter.WriteEndBatch();
            memoryStream.Position = 0;
            string responseString = new StreamReader(memoryStream).ReadToEnd();

            Assert.Contains("changesetresponse", responseString);
            Assert.Contains("Accepted", responseString);
            Assert.Contains("No Content", responseString);
        }

        [Fact]
        public void IsResponseSuccessful_TestResponse()
        {
            // Arrange
            HttpResponseMessage[] successResponses = new HttpResponseMessage[]
            {
                new HttpResponseMessage(HttpStatusCode.Accepted),
                new HttpResponseMessage(HttpStatusCode.Created),
                new HttpResponseMessage(HttpStatusCode.OK)
            };
            HttpResponseMessage[] errorResponses = new HttpResponseMessage[]
            {
                new HttpResponseMessage(HttpStatusCode.Created),
                new HttpResponseMessage(HttpStatusCode.BadGateway),
                new HttpResponseMessage(HttpStatusCode.Ambiguous)
            };
            ChangeSetResponseItem successResponseItem = new ChangeSetResponseItem(successResponses);
            ChangeSetResponseItem errorResponseItem = new ChangeSetResponseItem(errorResponses);

            // Act & Assert
            Assert.True(successResponseItem.IsResponseSuccessful());
            Assert.False(errorResponseItem.IsResponseSuccessful());
        }

        [Fact]
        public void Dispose_DisposesAllHttpResponseMessages()
        {
            ChangeSetResponseItem responseItem = new ChangeSetResponseItem(new MockHttpResponseMessage[]
            {
                new MockHttpResponseMessage(),
                new MockHttpResponseMessage(),
                new MockHttpResponseMessage()
            });

            responseItem.Dispose();

            Assert.Equal(3, responseItem.Responses.Count());
            foreach (var response in responseItem.Responses)
            {
                Assert.True(((MockHttpResponseMessage)response).IsDisposed);
            }
        }
    }
}