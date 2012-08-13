using System;
using ROOT_PROJECT_NAMESPACE.Areas.HelpPage;
using Xunit;

namespace WebApiHelpPageWebHost.UnitTest
{
    public class ImageSampleTest
    {
        [Fact]
        public void Constructor()
        {
            ImageSample sample = new ImageSample("http://host/image.png");
            Assert.Equal("http://host/image.png", sample.Src);
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ImageSample(null));
        }

        [Fact]
        public void Equals_ReturnsTrue()
        {
            ImageSample sample = new ImageSample("http://host/image.png");
            Assert.Equal(new ImageSample("http://host/image.png"), sample);
        }

        [Fact]
        public void ToString_ReturnsSrc()
        {
            ImageSample sample = new ImageSample("http://host/image.png");
            Assert.Equal("http://host/image.png", sample.ToString());
        }

        [Fact]
        public void GetHashCode_ReturnsSrcHashCode()
        {
            ImageSample sample = new ImageSample("http://host/image.png");
            Assert.Equal("http://host/image.png".GetHashCode(), sample.GetHashCode());
        }
    }
}
