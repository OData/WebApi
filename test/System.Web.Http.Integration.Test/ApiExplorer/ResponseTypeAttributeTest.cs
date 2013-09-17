// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Description;
using System.Web.Http.Dispatcher;
using Microsoft.TestCommon;

namespace System.Web.Http.ApiExplorer
{
    public class ResponseTypeAttributeTest
    {
        [Theory]
        [InlineData("Get", typeof(IHttpActionResult), typeof(User))]
        [InlineData("Post", typeof(HttpResponseMessage), typeof(User))]
        [InlineData("Delete", typeof(string), null)]
        public void DeclaredResponseType_AppearsOnApiDescription(string actionName, Type declaredType, Type responseType)
        {
            HttpConfiguration config = new HttpConfiguration();
            config.Routes.MapHttpRoute("Default", "{controller}/{id}", new { id = RouteParameter.Optional });
            Type controllerToTest = typeof(ResponseTypeController);
            DefaultHttpControllerSelector controllerSelector = ApiExplorerHelper.GetStrictControllerSelector(config, controllerToTest);
            config.Services.Replace(typeof(IHttpControllerSelector), controllerSelector);

            Collection<ApiDescription> apis = config.Services.GetApiExplorer().ApiDescriptions;
            ApiDescription expectedApi = apis.FirstOrDefault(api => api.ActionDescriptor.ActionName == actionName &&
                api.ResponseDescription.DeclaredType == declaredType &&
                api.ResponseDescription.ResponseType == responseType);

            Assert.NotNull(expectedApi);
        }
    }
}