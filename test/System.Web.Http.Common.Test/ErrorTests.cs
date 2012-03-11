using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Http.Common
{
    public class ErrorTests
    {
        [Fact]
        public void Format()
        {
            // Arrange
            string expected = "The formatted message";

            // Act
            string actual = Error.Format("The {0} message", "formatted");

            // Assert
            Assert.Equal(expected, actual);
        }
    }
}
