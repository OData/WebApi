using System.IO;
using System.Linq;
using Microsoft.TestCommon;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Net.Http
{
    public class MultipartFormDataStreamProviderTests
    {
        const int defaultBufferSize = 0x1000;
        const string validPath = @"c:\some\path";

        [Fact]
        [Trait("Description", "MultipartFormDataStreamProvider is public, visible type.")]
        public void TypeIsCorrect()
        {
            Assert.Type.HasProperties(
                typeof(MultipartFormDataStreamProvider),
                TypeAssert.TypeProperties.IsPublicVisibleClass,
                typeof(IMultipartStreamProvider));
        }

        [Fact]
        [Trait("Description", "MultipartFormDataStreamProvider default ctor.")]
        public void DefaultConstructor()
        {
            MultipartFormDataStreamProvider instance = new MultipartFormDataStreamProvider();
            Assert.NotNull(instance);
        }

        [Fact]
        [Trait("Description", "MultipartFormDataStreamProvider ctor with invalid root paths.")]
        public void ConstructorInvalidRootPath()
        {
            Assert.ThrowsArgumentNull(() => { new MultipartFormDataStreamProvider(null); }, "rootPath");

            foreach (string path in TestData.NotSupportedFilePaths)
            {
                Assert.Throws<NotSupportedException>(() => new MultipartFormDataStreamProvider(path, defaultBufferSize));
            }

            foreach (string path in TestData.InvalidNonNullFilePaths)
            {
                // Note: Path.GetFileName doesn't set the argument name when throwing.
                Assert.ThrowsArgument(() => { new MultipartFormDataStreamProvider(path, defaultBufferSize); }, null, allowDerivedExceptions: true);
            }
        }

        [Fact]
        [Trait("Description", "MultipartFormDataStreamProvider ctor with null path.")]
        public void ConstructorInvalidBufferSize()
        {
            Assert.ThrowsArgumentOutOfRange(() => { new MultipartFormDataStreamProvider(validPath, -1); }, "bufferSize", exceptionMessage: null);
            Assert.ThrowsArgumentOutOfRange(() => { new MultipartFormDataStreamProvider(validPath, 0); }, "bufferSize", exceptionMessage: null);
        }

        [Fact]
        [Trait("Description", "BodyPartFileNames empty.")]
        public void EmptyBodyPartFileNames()
        {
            MultipartFormDataStreamProvider instance = new MultipartFormDataStreamProvider();
            Assert.NotNull(instance.BodyPartFileNames);
            Assert.Equal(0, instance.BodyPartFileNames.Count);
        }

        [Fact]
        [Trait("Description", "GetStream(HttpContentHeaders) throws on null.")]
        public void GetStreamThrowsOnNull()
        {
            MultipartFormDataStreamProvider instance = new MultipartFormDataStreamProvider();
            Assert.ThrowsArgumentNull(() => { instance.GetStream(null); }, "headers");
        }

        [Fact]
        [Trait("Description", "GetStream(HttpContentHeaders) throws on no Content-Disposition header.")]
        public void GetStreamThrowsOnNoContentDisposition()
        {
            MultipartFormDataStreamProvider instance = new MultipartFormDataStreamProvider();
            HttpContent content = new StringContent("text");
            Assert.Throws<IOException>(() => { instance.GetStream(content.Headers); }, RS.Format(Properties.Resources.MultipartFormDataStreamProviderNoContentDisposition, "Content-Disposition"));
        }

        [Fact]
        [Trait("Description", "GetStream(HttpContentHeaders) validation.")]
        public void GetStreamValidation()
        {
            Stream stream0 = null;
            Stream stream1 = null;

            try
            {
                MultipartFormDataContent content = new MultipartFormDataContent();
                content.Add(new StringContent("Not a file"), "notafile");
                content.Add(new StringContent("This is a file"), "file", "filename");

                MultipartFormDataStreamProvider instance = new MultipartFormDataStreamProvider();
                stream0 = instance.GetStream(content.ElementAt(0).Headers);
                Assert.IsType<MemoryStream>(stream0);
                stream1 = instance.GetStream(content.ElementAt(1).Headers);
                Assert.IsType<FileStream>(stream1);

                Assert.Equal(1, instance.BodyPartFileNames.Count);
                Assert.Equal(content.ElementAt(1).Headers.ContentDisposition.FileName, instance.BodyPartFileNames.Keys.ElementAt(0));
            }
            finally
            {
                if (stream0 != null)
                {
                    stream0.Close();
                }

                if (stream1 != null)
                {
                    stream1.Close();
                }
            }
        }
    }
}
