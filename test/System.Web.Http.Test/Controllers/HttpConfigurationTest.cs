using Microsoft.TestCommon;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Http
{
    public class HttpConfigurationTest
    {
        [Fact]
        public void TypeIsCorrect()
        {
            Assert.Type.HasProperties<HttpConfiguration>(TypeAssert.TypeProperties.IsPublicVisibleClass | TypeAssert.TypeProperties.IsDisposable);
        }

        [Fact]
        public void Default_Constructor()
        {
            HttpConfiguration configuration = new HttpConfiguration();

            Assert.Empty(configuration.Filters);
            Assert.NotEmpty(configuration.Formatters);
            Assert.Empty(configuration.MessageHandlers);
            Assert.Empty(configuration.Properties);
            Assert.Empty(configuration.Routes);
            Assert.NotNull(configuration.ServiceResolver);
            Assert.Equal("/", configuration.VirtualPathRoot);
        }

        [Fact]
        public void Parameter_Constructor()
        {
            string path = "/some/path";
            HttpRouteCollection routes = new HttpRouteCollection(path);
            HttpConfiguration configuration = new HttpConfiguration(routes);

            Assert.Same(routes, configuration.Routes);
            Assert.Equal(path, configuration.VirtualPathRoot);
        }

        [Fact]
        public void Dispose_Idempotent()
        {
            HttpConfiguration configuration = new HttpConfiguration();
            configuration.Dispose();
            configuration.Dispose();
        }
    }
}
