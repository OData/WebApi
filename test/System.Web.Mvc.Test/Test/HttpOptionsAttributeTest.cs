using Xunit;

namespace System.Web.Mvc.Test
{
    public class HttpOptionsAttributeTest
    {
        [Fact]
        public void IsValidForRequestReturnsFalseIfHttpVerbIsNotOptions()
        {
            HttpVerbAttributeHelper.TestHttpVerbAttributeWithInvalidVerb<HttpOptionsAttribute>("GET");
        }

        [Fact]
        public void IsValidForRequestReturnsTrueIfHttpVerbIsOptions()
        {
            HttpVerbAttributeHelper.TestHttpVerbAttributeWithValidVerb<HttpOptionsAttribute>("OPTIONS");
        }

        [Fact]
        public void IsValidForRequestThrowsIfControllerContextIsNull()
        {
            HttpVerbAttributeHelper.TestHttpVerbAttributeNullControllerContext<HttpOptionsAttribute>();
        }
    }
}