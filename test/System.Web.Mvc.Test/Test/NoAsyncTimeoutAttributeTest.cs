using System.Threading;
using Xunit;

namespace System.Web.Mvc.Test
{
    public class NoAsyncTimeoutAttributeTest
    {
        [Fact]
        public void DurationPropertyIsZero()
        {
            // Act
            AsyncTimeoutAttribute attr = new NoAsyncTimeoutAttribute();

            // Assert
            Assert.Equal(Timeout.Infinite, attr.Duration);
        }
    }
}
