using Xunit;

namespace System.Web.Mvc.Test
{
    public class HttpHeadAttributeTest
    {
        [Fact]
        public void IsValidForRequestReturnsFalseIfHttpVerbIsNotHead()
        {
            HttpVerbAttributeHelper.TestHttpVerbAttributeWithInvalidVerb<HttpHeadAttribute>("GET");
        }

        [Fact]
        public void IsValidForRequestReturnsTrueIfHttpVerbIsHead()
        {
            HttpVerbAttributeHelper.TestHttpVerbAttributeWithValidVerb<HttpHeadAttribute>("HEAD");
        }

        [Fact]
        public void IsValidForRequestThrowsIfControllerContextIsNull()
        {
            HttpVerbAttributeHelper.TestHttpVerbAttributeNullControllerContext<HttpHeadAttribute>();
        }
    }
}
