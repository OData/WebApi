//-----------------------------------------------------------------------------
// <copyright file="ODataBatchResponseItemTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

#if !NETCORE // TODO #939: Enable these test on AspNetCore.
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Batch;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.OData;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Batch
{
    public class ODataBatchResponseItemTest
    {
        [Fact]
        public async Task WriteMessageAsync_NullWriter_Throws()
        {
            await ExceptionAssert.ThrowsArgumentNullAsync(
                () => ODataBatchResponseItem.WriteMessageAsync(null, new HttpResponseMessage(), CancellationToken.None),
                "writer");
        }

        [Fact]
        public async Task WriteMessageAsync_NullResponse_Throws()
        {
            HttpContent content = new StringContent(String.Empty, Encoding.UTF8, "multipart/mixed");
            content.Headers.ContentType.Parameters.Add(new NameValueHeaderValue("boundary", Guid.NewGuid().ToString()));
            IODataResponseMessage odataResponse = ODataMessageWrapperHelper.Create(new MemoryStream(), content.Headers);
            ODataMessageWriter messageWriter = new ODataMessageWriter(odataResponse);

            await ExceptionAssert.ThrowsArgumentNullAsync(
                () => ODataBatchResponseItem.WriteMessageAsync(messageWriter.CreateODataBatchWriter(), null, CancellationToken.None),
                "response");
        }

        [Fact]
        public async Task WriteMessage_SynchronouslyWritesResponseMessage()
        {
            MemoryStream ms = new MemoryStream();
            HttpContent content = new StringContent(String.Empty, Encoding.UTF8, "multipart/mixed");
            content.Headers.ContentType.Parameters.Add(new NameValueHeaderValue("boundary", Guid.NewGuid().ToString()));
            IODataResponseMessage odataResponse = ODataMessageWrapperHelper.Create(ms, content.Headers);
            var batchWriter = new ODataMessageWriter(odataResponse).CreateODataBatchWriter();
            HttpResponseMessage response = new HttpResponseMessage()
            {
                Content = new StringContent("example content", Encoding.UTF8, "text/example")
            };
            response.Headers.Add("customHeader", "bar");

            batchWriter.WriteStartBatch();
            // For backward compatibility, default is to write to a synchronous batchWriter
            await ODataBatchResponseItem.WriteMessageAsync(batchWriter, response, CancellationToken.None);
            batchWriter.WriteEndBatch();

            ms.Position = 0;
            string result = new StreamReader(ms).ReadToEnd();

            Assert.Contains("example content", result);
            Assert.Contains("text/example", result);
            Assert.Contains("customHeader", result);
            Assert.Contains("bar", result);
        }

        [Fact]
        public async Task WriteMessageAsync_AsynchronouslyWritesResponseMessage()
        {
            MemoryStream ms = new MemoryStream();
            HttpContent content = new StringContent(String.Empty, Encoding.UTF8, "multipart/mixed");
            content.Headers.ContentType.Parameters.Add(new NameValueHeaderValue("boundary", Guid.NewGuid().ToString()));
            IODataResponseMessage odataResponse = ODataMessageWrapperHelper.Create(ms, content.Headers);
            var batchWriter = await new ODataMessageWriter(odataResponse).CreateODataBatchWriterAsync();
            HttpResponseMessage response = new HttpResponseMessage()
            {
                Content = new StringContent("example content", Encoding.UTF8, "text/example")
            };
            response.Headers.Add("customHeader", "bar");

            await batchWriter.WriteStartBatchAsync();
            await ODataBatchResponseItem.WriteMessageAsync(batchWriter, response, CancellationToken.None, /*asyncWriter*/ true);
            await batchWriter.WriteEndBatchAsync();

            ms.Position = 0;
            string result = new StreamReader(ms).ReadToEnd();

            Assert.Contains("example content", result);
            Assert.Contains("text/example", result);
            Assert.Contains("customHeader", result);
            Assert.Contains("bar", result);
        }

        [Fact]
        public async Task WriteMessageAsync_SynchronousResponseContainsContentId_IfHasContentIdInRequestChangeSet()
        {
            MemoryStream ms = new MemoryStream();
            HttpContent content = new StringContent(String.Empty, Encoding.UTF8, "multipart/mixed");
            content.Headers.ContentType.Parameters.Add(new NameValueHeaderValue("boundary", Guid.NewGuid().ToString()));
            IODataResponseMessage odataResponse = ODataMessageWrapperHelper.Create(ms, content.Headers);
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
            await ODataBatchResponseItem.WriteMessageAsync(batchWriter, response, CancellationToken.None);
            batchWriter.WriteEndChangeset();
            batchWriter.WriteEndBatch();

            ms.Position = 0;
            string result = new StreamReader(ms).ReadToEnd();

            Assert.Contains("any", result);
            Assert.Contains("text/example", result);
            Assert.Contains("Content-ID", result);
            Assert.Contains(contentId, result);
        }

        [Fact]
        public async Task WriteMessageAsync_ResponseContainsContentId_IfHasContentIdInRequestChangeSet()
        {
            MemoryStream ms = new MemoryStream();
            HttpContent content = new StringContent(String.Empty, Encoding.UTF8, "multipart/mixed");
            content.Headers.ContentType.Parameters.Add(new NameValueHeaderValue("boundary", Guid.NewGuid().ToString()));
            IODataResponseMessage odataResponse = ODataMessageWrapperHelper.Create(ms, content.Headers);
            var batchWriter = await new ODataMessageWriter(odataResponse).CreateODataBatchWriterAsync();
            HttpResponseMessage response = new HttpResponseMessage
            {
                Content = new StringContent("any", Encoding.UTF8, "text/example")
            };
            var request = new HttpRequestMessage();
            var contentId = Guid.NewGuid().ToString();
            request.SetODataContentId(contentId);
            response.RequestMessage = request;

            await batchWriter.WriteStartBatchAsync();
            await batchWriter.WriteStartChangesetAsync();
            await ODataBatchResponseItem.WriteMessageAsync(batchWriter, response, CancellationToken.None, /*asyncWriter*/ true);
            await batchWriter.WriteEndChangesetAsync();
            await batchWriter.WriteEndBatchAsync();

            ms.Position = 0;
            string result = new StreamReader(ms).ReadToEnd();

            Assert.Contains("any", result);
            Assert.Contains("text/example", result);
            Assert.Contains("Content-ID", result);
            Assert.Contains(contentId, result);
        }
    }
}
#endif
