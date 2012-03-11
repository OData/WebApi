using Xunit;

namespace System.Web.Mvc.Test
{
    public class UrlParameterTest
    {
        [Fact]
        public void UrlParameterOptionalToStringReturnsEmptyString()
        {
            // Act & Assert
            Assert.Empty(UrlParameter.Optional.ToString());
        }
    }
}
