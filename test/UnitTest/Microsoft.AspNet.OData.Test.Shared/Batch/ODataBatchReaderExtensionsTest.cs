// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if !NETCORE // TODO #939: Enable these test on AspNetCore.
using System;
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
    public class ODataBatchReaderExtensionsTest
    {
        [Fact]
        public async Task ReadChangeSetRequest_NullReader_Throws()
        {
            await ExceptionAssert.ThrowsArgumentNullAsync(
                () => ODataBatchReaderExtensions.ReadChangeSetRequestAsync(null, Guid.NewGuid()),
                "reader");
        }

        [Fact]
        public async Task ReadChangeSetRequest_InvalidState_Throws()
        {
            var httpContent = new StringContent(String.Empty, Encoding.UTF8, "multipart/mixed");
            httpContent.Headers.ContentType.Parameters.Add(new NameValueHeaderValue("boundary", Guid.NewGuid().ToString()));
            var reader = await httpContent.GetODataMessageReaderAsync(new ODataMessageReaderSettings(), CancellationToken.None);
            await ExceptionAssert.ThrowsAsync<InvalidOperationException>(
                () => ODataBatchReaderExtensions.ReadChangeSetRequestAsync(reader.CreateODataBatchReader(), Guid.NewGuid(),
                    CancellationToken.None),
                "The current batch reader state 'Initial' is invalid. The expected state is 'ChangesetStart'.");
        }

        [Fact]
        public async Task ReadOperationRequest_NullReader_Throws()
        {
            await ExceptionAssert.ThrowsArgumentNullAsync(
                () => ODataBatchReaderExtensions.ReadOperationRequestAsync(null, Guid.NewGuid(), false),
                "reader");
        }

        [Fact]
        public async Task ReadOperationRequest_InvalidState_Throws()
        {
            var httpContent = new StringContent(String.Empty, Encoding.UTF8, "multipart/mixed");
            httpContent.Headers.ContentType.Parameters.Add(new NameValueHeaderValue("boundary", Guid.NewGuid().ToString()));
            var reader = await httpContent.GetODataMessageReaderAsync(new ODataMessageReaderSettings(), CancellationToken.None);
            ExceptionAssert.Throws<InvalidOperationException>(
                () => ODataBatchReaderExtensions.ReadOperationRequestAsync(reader.CreateODataBatchReader(), Guid.NewGuid(),
                    false, CancellationToken.None),
                "The current batch reader state 'Initial' is invalid. The expected state is 'Operation'.");
        }

        [Fact]
        public async Task ReadChangeSetOperationRequest_NullReader_Throws()
        {
            await ExceptionAssert.ThrowsArgumentNullAsync(
                () => ODataBatchReaderExtensions.ReadChangeSetOperationRequestAsync(null, Guid.NewGuid(), Guid.NewGuid(), false),
                "reader");
        }

        [Fact]
        public async Task ReadChangeSetOperationRequest_InvalidState_Throws()
        {
            var httpContent = new StringContent(String.Empty, Encoding.UTF8, "multipart/mixed");
            httpContent.Headers.ContentType.Parameters.Add(new NameValueHeaderValue("boundary", Guid.NewGuid().ToString()));
            var reader = await httpContent.GetODataMessageReaderAsync(new ODataMessageReaderSettings(), CancellationToken.None);
            ExceptionAssert.Throws<InvalidOperationException>(
                () => ODataBatchReaderExtensions.ReadChangeSetOperationRequestAsync(reader.CreateODataBatchReader(),
                    Guid.NewGuid(), Guid.NewGuid(), false, CancellationToken.None),
                "The current batch reader state 'Initial' is invalid. The expected state is 'Operation'.");
        }
    }
}
#endif