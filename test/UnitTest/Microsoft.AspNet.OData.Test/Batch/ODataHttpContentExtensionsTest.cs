// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if !NETCORE // TODO #939: Enable these test on AspNetCore.
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.OData;
using Microsoft.Test.AspNet.OData.Common;
using Xunit;

namespace Microsoft.Test.AspNet.OData.Batch
{
    public class ODataHttpContentExtensionsTest
    {
        [Fact]
        public async Task GetODataMessageReaderAsync_NullContent_Throws()
        {
            await ExceptionAssert.ThrowsArgumentNullAsync(
                () => ODataHttpContentExtensions.GetODataMessageReaderAsync(null, new ODataMessageReaderSettings(), CancellationToken.None),
                "content");
        }

        [Fact]
        public async Task GetODataMessageReaderAsync_ReturnsMessageReader()
        {
            StringContent content = new StringContent("foo", Encoding.UTF8, "multipart/mixed");

            Assert.NotNull(await content.GetODataMessageReaderAsync(new ODataMessageReaderSettings(), CancellationToken.None));
        }
    }
}
#endif