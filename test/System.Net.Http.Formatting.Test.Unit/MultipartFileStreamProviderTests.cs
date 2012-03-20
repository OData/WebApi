using System.IO;
using System.Linq;
using Microsoft.TestCommon;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Net.Http
{
    public class MultipartFileStreamProviderTests
    {
        const int defaultBufferSize = 0x1000;
        const string validPath = @"c:\some\path";

        [Fact]
        [Trait("Description", "MultipartFileStreamProvider is public, visible type.")]
        public void TypeIsCorrect()
        {
            Assert.Type.HasProperties(
                typeof(MultipartFileStreamProvider),
                TypeAssert.TypeProperties.IsPublicVisibleClass,
                typeof(IMultipartStreamProvider));
        }

        [Fact]
        [Trait("Description", "MultipartFileStreamProvider ctor with invalid root paths.")]
        public void ConstructorInvalidRootPath()
        {
            Assert.ThrowsArgumentNull(() => { new MultipartFileStreamProvider(null); }, "rootPath");

            foreach (string path in TestData.NotSupportedFilePaths)
            {
                Assert.Throws<NotSupportedException>(() => new MultipartFileStreamProvider(path, defaultBufferSize));
            }

            foreach (string path in TestData.InvalidNonNullFilePaths)
            {
                // Note: Path.GetFileName doesn't set the argument name when throwing.
                Assert.ThrowsArgument(() => { new MultipartFileStreamProvider(path, defaultBufferSize); }, null, allowDerivedExceptions: true);
            }
        }

        [Fact]
        [Trait("Description", "MultipartFileStreamProvider ctor with null path.")]
        public void ConstructorInvalidBufferSize()
        {
            Assert.ThrowsArgumentOutOfRange(() => { new MultipartFileStreamProvider(validPath, -1); }, "bufferSize", exceptionMessage: null);
            Assert.ThrowsArgumentOutOfRange(() => { new MultipartFileStreamProvider(validPath, 0); }, "bufferSize", exceptionMessage: null);
        }



        [Fact]
        [Trait("Description", "BodyPartFileNames empty.")]
        public void EmptyBodyPartFileNames()
        {
            MultipartFileStreamProvider instance = new MultipartFileStreamProvider(Path.GetTempPath());
            Assert.NotNull(instance.BodyPartFileNames);
            Assert.Equal(0, instance.BodyPartFileNames.Count());
        }

        [Fact]
        [Trait("Description", "GetStream(HttpContentHeaders) throws on null.")]
        public void GetStreamThrowsOnNull()
        {
            MultipartFileStreamProvider instance = new MultipartFileStreamProvider(Path.GetTempPath());
            Assert.ThrowsArgumentNull(() => { instance.GetStream(null); }, "headers");
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

                MultipartFileStreamProvider instance = new MultipartFileStreamProvider(Path.GetTempPath());
                stream0 = instance.GetStream(content.ElementAt(0).Headers);
                Assert.IsType<FileStream>(stream0);
                stream1 = instance.GetStream(content.ElementAt(1).Headers);
                Assert.IsType<FileStream>(stream1);

                Assert.Equal(2, instance.BodyPartFileNames.Count());
                Assert.Contains("BodyPart", instance.BodyPartFileNames.ElementAt(0));
                Assert.Contains("BodyPart", instance.BodyPartFileNames.ElementAt(1));
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
