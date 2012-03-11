using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Helpers.Test
{
    public class AntiForgeryTest
    {
        private static string _antiForgeryTokenCookieName = AntiForgeryData.GetAntiForgeryTokenName("/SomeAppPath");

        [Fact]
        public void GetHtml_ThrowsWhenNotCalledInWebContext()
        {
            Assert.Throws<ArgumentException>(() => AntiForgery.GetHtml(),
                                                    "An HttpContext is required to perform this operation. Check that this operation is being performed during a web request.");
        }

        [Fact]
        public void GetHtml_ThrowsOnNullContext()
        {
            Assert.ThrowsArgumentNull(() => AntiForgery.GetHtml(null, null, null, null), "httpContext");
        }

        [Fact]
        public void Validate_ThrowsWhenNotCalledInWebContext()
        {
            Assert.Throws<ArgumentException>(() => AntiForgery.Validate(),
                                                    "An HttpContext is required to perform this operation. Check that this operation is being performed during a web request.");
        }

        [Fact]
        public void Validate_ThrowsOnNullContext()
        {
            Assert.ThrowsArgumentNull(() => AntiForgery.Validate(httpContext: null, salt: null), "httpContext");
        }
    }
}
