// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Web.Http.OData.Batch;
using System.Web.Http.OData.Formatter;
using Microsoft.Data.OData;
using Microsoft.TestCommon;

namespace System.Web.Http
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
    }
}