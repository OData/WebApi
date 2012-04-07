// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http.Description;
using System.Web.Http.Dispatcher;
using System.Web.Http.Properties;
using Xunit;

namespace System.Web.Http.ApiExplorer
{
    public class DocumentationTest
    {
        [Fact]
        public void VerifyDefaultDocumentationMessage()
        {
            HttpConfiguration config = new HttpConfiguration();
            config.Routes.MapHttpRoute("Default", "{controller}/{id}", new { id = RouteParameter.Optional });
            ItemFormatter customFormatter = new ItemFormatter();
            config.Formatters.Add(customFormatter);

            DefaultHttpControllerSelector controllerSelector = ApiExplorerHelper.GetStrictControllerSelector(config, typeof(ItemController));
            config.Services.Replace(typeof(IHttpControllerSelector), controllerSelector);

            IApiExplorer explorer = config.Services.GetApiExplorer();
            foreach (ApiDescription description in explorer.ApiDescriptions)
            {
                Assert.Equal(
                    String.Format(SRResources.ApiExplorer_DefaultDocumentation, description.ActionDescriptor.ActionName),
                    description.Documentation);
                foreach (ApiParameterDescription param in description.ParameterDescriptions)
                {
                    Assert.Equal(
                        String.Format(SRResources.ApiExplorer_DefaultDocumentation, param.Name),
                        param.Documentation);
                }
            }
        }

        [Fact]
        public void VerifyCustomDocumentationProviderMessage()
        {
            HttpConfiguration config = new HttpConfiguration();
            config.Routes.MapHttpRoute("Default", "{controller}/{id}", new { id = RouteParameter.Optional });
            ItemFormatter customFormatter = new ItemFormatter();
            config.Formatters.Add(customFormatter);

            DefaultHttpControllerSelector controllerSelector = ApiExplorerHelper.GetStrictControllerSelector(config, typeof(DocumentationController));
            config.Services.Replace(typeof(IHttpControllerSelector), controllerSelector);

            AttributeDocumentationProvider documentationProvider = new AttributeDocumentationProvider();
            config.Services.Replace(typeof(IDocumentationProvider), documentationProvider);

            IApiExplorer explorer = config.Services.GetApiExplorer();
            foreach (ApiDescription description in explorer.ApiDescriptions)
            {
                Assert.Equal(
                    String.Format("{0} action", description.ActionDescriptor.ActionName),
                    description.Documentation);
                foreach (ApiParameterDescription param in description.ParameterDescriptions)
                {
                    Assert.Equal(
                        String.Format("{0} parameter", param.Name),
                        param.Documentation);
                }
            }
        }
    }
}
