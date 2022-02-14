//-----------------------------------------------------------------------------
// <copyright file="ODataHttpContentExtensionsTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

#if !NETCORE // TODO #939: Enable these test on AspNetCore.
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.OData;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Batch
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
