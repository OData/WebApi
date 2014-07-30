// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Web.OData.Batch;
using Microsoft.OData.Core;
using Microsoft.TestCommon;

namespace System.Web.OData.Test
{
    public class ODataBatchReaderExtensionsTest
    {
        [Fact]
        public void ReadChangeSetRequest_NullReader_Throws()
        {
            Assert.ThrowsArgumentNull(
                () => ODataBatchReaderExtensions.ReadChangeSetRequestAsync(null, Guid.NewGuid()).Wait(),
                "reader");
        }

        [Fact]
        public void ReadChangeSetRequest_InvalidState_Throws()
        {
            var httpContent = new StringContent(String.Empty, Encoding.UTF8, "multipart/mixed");
            httpContent.Headers.ContentType.Parameters.Add(new NameValueHeaderValue("boundary", Guid.NewGuid().ToString()));
            var reader = httpContent.GetODataMessageReaderAsync(new ODataMessageReaderSettings(), CancellationToken.None).Result;
            Assert.Throws<InvalidOperationException>(
                () => ODataBatchReaderExtensions.ReadChangeSetRequestAsync(reader.CreateODataBatchReader(), Guid.NewGuid(),
                    CancellationToken.None).Wait(),
                "The current batch reader state 'Initial' is invalid. The expected state is 'ChangesetStart'.");
        }

        [Fact]
        public void ReadOperationRequest_NullReader_Throws()
        {
            Assert.ThrowsArgumentNull(
                () => ODataBatchReaderExtensions.ReadOperationRequestAsync(null, Guid.NewGuid(), false),
                "reader");
        }

        [Fact]
        public void ReadOperationRequest_InvalidState_Throws()
        {
            var httpContent = new StringContent(String.Empty, Encoding.UTF8, "multipart/mixed");
            httpContent.Headers.ContentType.Parameters.Add(new NameValueHeaderValue("boundary", Guid.NewGuid().ToString()));
            var reader = httpContent.GetODataMessageReaderAsync(new ODataMessageReaderSettings(), CancellationToken.None).Result;
            Assert.Throws<InvalidOperationException>(
                () => ODataBatchReaderExtensions.ReadOperationRequestAsync(reader.CreateODataBatchReader(), Guid.NewGuid(),
                    false, CancellationToken.None),
                "The current batch reader state 'Initial' is invalid. The expected state is 'Operation'.");
        }

        [Fact]
        public void ReadChangeSetOperationRequest_NullReader_Throws()
        {
            Assert.ThrowsArgumentNull(
                () => ODataBatchReaderExtensions.ReadChangeSetOperationRequestAsync(null, Guid.NewGuid(), Guid.NewGuid(), false),
                "reader");
        }

        [Fact]
        public void ReadChangeSetOperationRequest_InvalidState_Throws()
        {
            var httpContent = new StringContent(String.Empty, Encoding.UTF8, "multipart/mixed");
            httpContent.Headers.ContentType.Parameters.Add(new NameValueHeaderValue("boundary", Guid.NewGuid().ToString()));
            var reader = httpContent.GetODataMessageReaderAsync(new ODataMessageReaderSettings(), CancellationToken.None).Result;
            Assert.Throws<InvalidOperationException>(
                () => ODataBatchReaderExtensions.ReadChangeSetOperationRequestAsync(reader.CreateODataBatchReader(),
                    Guid.NewGuid(), Guid.NewGuid(), false, CancellationToken.None),
                "The current batch reader state 'Initial' is invalid. The expected state is 'Operation'.");
        }
    }
}