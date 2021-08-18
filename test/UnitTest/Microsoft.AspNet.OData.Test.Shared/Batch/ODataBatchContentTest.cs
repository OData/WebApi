//-----------------------------------------------------------------------------
// <copyright file="ODataBatchContentTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Batch;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Common;
#if NETCORE
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
#endif
using Xunit;

namespace Microsoft.AspNet.OData.Test.Batch
{
    public class ODataBatchContentTest
    {
#if NETCORE
        [Fact]
        public void Parameter_Constructor()
        {
            const string boundaryHeader = "boundary";
            ODataBatchContent batchContent = CreateBatchContent(new ODataBatchResponseItem[0]);
            string contentTypeHeader = batchContent.Headers[HeaderNames.ContentType].FirstOrDefault();
            string mediaType = contentTypeHeader.Substring(0, contentTypeHeader.IndexOf(';'));
            int boundaryParamStart = contentTypeHeader.IndexOf(boundaryHeader);
            string boundary = contentTypeHeader.Substring(boundaryParamStart + boundaryHeader.Length);
            var odataVersion = batchContent.Headers.FirstOrDefault(h => String.Equals(h.Key, ODataVersionConstraint.ODataServiceVersionHeader, StringComparison.OrdinalIgnoreCase));

            Assert.NotEmpty(boundary);
            Assert.NotEmpty(odataVersion.Value);
            Assert.Equal("multipart/mixed", mediaType);
        }

#else
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
#endif

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

#if NETCORE
        [Fact]
        public async Task SerializeToStreamAsync_WritesODataBatchResponseItems()
        {
            HttpContext okContext = new DefaultHttpContext();
            okContext.Response.StatusCode = (int)HttpStatusCode.OK;
            HttpContext acceptedContext = new DefaultHttpContext();
            acceptedContext.Response.StatusCode = (int)HttpStatusCode.Accepted;
            HttpContext badRequestContext = new DefaultHttpContext();
            badRequestContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;

            ODataBatchContent batchContent = CreateBatchContent(new ODataBatchResponseItem[]
            {
                new OperationResponseItem(okContext),
                new ChangeSetResponseItem(new HttpContext[]
                {
                    acceptedContext,
                    badRequestContext
                })
            });

            MemoryStream stream = new MemoryStream();
            await batchContent.SerializeToStreamAsync(stream);
            stream.Position = 0;
            string responseString = await new StreamReader(stream).ReadToEndAsync();

            Assert.Contains("changesetresponse", responseString);
            Assert.Contains("OK", responseString);
            Assert.Contains("Accepted", responseString);
            Assert.Contains("Bad Request", responseString);
        }
#else
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
#endif

        private static ODataBatchContent CreateBatchContent(IEnumerable<ODataBatchResponseItem> responses)
        {
            return new ODataBatchContent(responses, new MockContainer());
        }
    }
}
