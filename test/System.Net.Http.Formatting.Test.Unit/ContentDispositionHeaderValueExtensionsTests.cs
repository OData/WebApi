using System.Net.Http.Headers;
using Microsoft.TestCommon;
using Xunit;
using Xunit.Extensions;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Net.Http
{
    public class ContentDispositionHeaderValueExtensionsTests
    {
        [Fact]
        public void TypeIsCorrect()
        {
            Assert.Type.HasProperties(typeof(ContentDispositionHeaderValueExtensions), TypeAssert.TypeProperties.IsClass | TypeAssert.TypeProperties.IsStatic);
        }

        [Fact]
        public void ExtractLocalFileNameThrowsOnNull()
        {
            ContentDispositionHeaderValue test = null;
            Assert.ThrowsArgumentNull(() => ContentDispositionHeaderValueExtensions.ExtractLocalFileName(test), "contentDisposition");
        }

        [Theory]
        [TestDataSet(typeof(CommonUnitTestDataSets), "NonNullEmptyStrings")]
        public void ExtractLocalFileNameThrowsOnQuotedEmpty(string empty)
        {
            Assert.ThrowsArgument(
                () =>
                {
                    ContentDispositionHeaderValue contentDisposition = null;
                    ContentDispositionHeaderValue.TryParse(String.Format("formdata; filename=\"{0}\"", empty), out contentDisposition);
                    Assert.NotNull(contentDisposition.FileName);
                    ContentDispositionHeaderValueExtensions.ExtractLocalFileName(contentDisposition);
                }, "contentDisposition");
        }

        [Fact]
        public void ExtractLocalFileNamePicksFileNameStarOverFilename()
        {
            // ExtractLocalFileName picks filename* over filename.
            ContentDispositionHeaderValue contentDisposition = null;
            ContentDispositionHeaderValue.TryParse("formdata; filename=\"aaa\"; filename*=utf-8''%e2BBB", out contentDisposition);
            string localFilename = ContentDispositionHeaderValueExtensions.ExtractLocalFileName(contentDisposition);
            Assert.Equal("�BBB", localFilename);
        }
    }
}
