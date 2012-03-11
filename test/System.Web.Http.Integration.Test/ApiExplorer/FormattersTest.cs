using System.Linq;
using System.Web.Http.Description;
using System.Web.Http.Dispatcher;
using Xunit;

namespace System.Web.Http.ApiExplorer
{
    public class FormattersTest
    {
        [Fact]
        public void CustomRequestBodyFormatters_ShowUpOnDescription()
        {
            HttpConfiguration config = new HttpConfiguration();
            config.Routes.MapHttpRoute("Default", "{controller}/{id}", new { id = RouteParameter.Optional });
            ItemFormatter customFormatter = new ItemFormatter();
            config.Formatters.Add(customFormatter);

            DefaultHttpControllerFactory controllerFactory = ApiExplorerHelper.GetStrictControllerFactory(config, typeof(ItemController));
            config.ServiceResolver.SetService(typeof(IHttpControllerFactory), controllerFactory);

            IApiExplorer explorer = config.ServiceResolver.GetApiExplorer();
            ApiDescription description = explorer.ApiDescriptions.FirstOrDefault(desc => desc.ActionDescriptor.ActionName == "PostItem");
            Assert.True(description.SupportedRequestBodyFormatters.Any(formatter => formatter == customFormatter), "Did not find the custom formatter on the SupportedRequestBodyFormatters.");
        }

        [Fact]
        public void CustomResponseFormatters_ShowUpOnDescription()
        {
            HttpConfiguration config = new HttpConfiguration();
            config.Routes.MapHttpRoute("Default", "{controller}/{id}", new { id = RouteParameter.Optional });
            ItemFormatter customFormatter = new ItemFormatter();
            config.Formatters.Add(customFormatter);

            DefaultHttpControllerFactory controllerFactory = ApiExplorerHelper.GetStrictControllerFactory(config, typeof(ItemController));
            config.ServiceResolver.SetService(typeof(IHttpControllerFactory), controllerFactory);

            IApiExplorer explorer = config.ServiceResolver.GetApiExplorer();
            ApiDescription description = explorer.ApiDescriptions.FirstOrDefault(desc => desc.ActionDescriptor.ActionName == "PostItem");
            Assert.True(description.SupportedResponseFormatters.Any(formatter => formatter == customFormatter), "Did not find the custom formatter on the SupportedResponseFormatters.");
        }
    }
}
