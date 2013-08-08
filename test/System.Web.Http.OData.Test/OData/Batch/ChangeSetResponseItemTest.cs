// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Web.Http.OData.Batch;
using System.Web.Http.OData.Formatter;
using Microsoft.Data.OData;
using Microsoft.TestCommon;

namespace System.Web.Http
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