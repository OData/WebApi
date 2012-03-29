using System.Globalization;
using System.Web.Http.Controllers;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Http.ValueProviders.Providers
{
    public class RouteDataValueProviderFactoryTest
    {
        private readonly RouteDataValueProviderFactory _factory = new RouteDataValueProviderFactory();

        [Fact]
        public void GetValueProvider_WhenActionContextParameterIsNull_Throws()
        {
            Assert.ThrowsArgumentNull(() => _factory.GetValueProvider(actionContext: null), "actionContext");
        }

        [Fact]
        public void GetValueProvider_ReturnsQueryStringValueProviderInstaceWithInvariantCulture()
        {
            var context = new HttpActionContext();

            IValueProvider result = _factory.GetValueProvider(context);

            var valueProvider = Assert.IsType<RouteDataValueProvider>(result);
            Assert.Equal(CultureInfo.InvariantCulture, valueProvider.Culture);
        }
    }
}
