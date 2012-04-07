// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.IO;
using Microsoft.TestCommon;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Net.Http
{
    public class MultipartMemoryStreamProviderTests
    {
        [Fact]
        [Trait("Description", "MultipartMemoryStreamProvider is internal type.")]
        public void TypeIsCorrect()
        {
            Assert.Type.HasProperties(
                typeof(MultipartMemoryStreamProvider),
                TypeAssert.TypeProperties.IsClass,
                typeof(IMultipartStreamProvider));
        }

        [Fact]
        [Trait("Description", "MultipartMemoryStreamProvider default ctor.")]
        public void DefaultConstructor()
        {
            MultipartMemoryStreamProvider instance = MultipartMemoryStreamProvider.Instance;
            Assert.NotNull(instance);
        }

        [Fact]
        [Trait("Description", "GetStream(HttpContentHeaders) throws on null.")]
        public void GetStreamThrowsOnNull()
        {
            MultipartMemoryStreamProvider instance = MultipartMemoryStreamProvider.Instance;
            Assert.ThrowsArgumentNull(() => { instance.GetStream(null); }, "headers");
        }

        [Fact]
        [Trait("Description", "GetStream(HttpContentHeaders) throws on no Content-Disposition header.")]
        public void GetStreamReturnsMemoryStream()
        {
            MultipartMemoryStreamProvider instance = MultipartMemoryStreamProvider.Instance;
            HttpContent content = new StringContent("text");

            Stream stream = instance.GetStream(content.Headers);
            Assert.NotNull(stream);

            MemoryStream memStream = stream as MemoryStream;
            Assert.NotNull(stream);

            Assert.Equal(0, stream.Length);
            Assert.Equal(0, stream.Position);

            Assert.NotSame(memStream, instance.GetStream(content.Headers));
        }
    }
}
