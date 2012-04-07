// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

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

            DefaultHttpControllerSelector controllerSelector = ApiExplorerHelper.GetStrictControllerSelector(config, typeof(ItemController));
            config.Services.Replace(typeof(IHttpControllerSelector), controllerSelector);

            IApiExplorer explorer = config.Services.GetApiExplorer();
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

            DefaultHttpControllerSelector controllerSelector = ApiExplorerHelper.GetStrictControllerSelector(config, typeof(ItemController));
            config.Services.Replace(typeof(IHttpControllerSelector), controllerSelector);

            IApiExplorer explorer = config.Services.GetApiExplorer();
            ApiDescription description = explorer.ApiDescriptions.FirstOrDefault(desc => desc.ActionDescriptor.ActionName == "PostItem");
            Assert.True(description.SupportedResponseFormatters.Any(formatter => formatter == customFormatter), "Did not find the custom formatter on the SupportedResponseFormatters.");
        }
    }
}
