// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Net.Http;
using System.Web.Http.Controllers;
using Microsoft.TestCommon;

namespace System.Web.Http
{
    public class ApiControllerActionSelectorTest
    {
        [Fact]
        public void SelectAction_With_DifferentExecutionContexts()
        {
            ApiControllerActionSelector actionSelector = new ApiControllerActionSelector();
            HttpControllerContext GetContext = ContextUtil.CreateControllerContext();
            HttpControllerDescriptor usersControllerDescriptor = new HttpControllerDescriptor(GetContext.Configuration, "Users", typeof(UsersController));
            usersControllerDescriptor.Configuration.Services.Replace(typeof(IHttpActionSelector), actionSelector);
            GetContext.ControllerDescriptor = usersControllerDescriptor;
            GetContext.Request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get
                };
            HttpControllerContext PostContext = ContextUtil.CreateControllerContext();
            usersControllerDescriptor.Configuration.Services.Replace(typeof(IHttpActionSelector), actionSelector);
            PostContext.ControllerDescriptor = usersControllerDescriptor;
            PostContext.Request = new HttpRequestMessage
            {
                Method = HttpMethod.Post
            };

            HttpActionDescriptor getActionDescriptor = actionSelector.SelectAction(GetContext);
            HttpActionDescriptor postActionDescriptor = actionSelector.SelectAction(PostContext);

            Assert.Equal("Get", getActionDescriptor.ActionName);
            Assert.Equal("Post", postActionDescriptor.ActionName);
        }

        [Fact]
        public void SelectAction_RespectsDirectRoutes()
        {
            var actionSelector = new ApiControllerActionSelector();
            HttpControllerContext context = ContextUtil.CreateControllerContext();
            context.Request = new HttpRequestMessage { Method = HttpMethod.Get };
            var controllerDescriptor = new HttpControllerDescriptor(context.Configuration, "Users", typeof(UsersController));
            context.ControllerDescriptor = controllerDescriptor;
            ReflectedHttpActionDescriptor directRouteAction = (ReflectedHttpActionDescriptor)actionSelector.GetActionMapping(controllerDescriptor)["Delete"].First();
            context.RouteData.Route.DataTokens.Add("actions", new ReflectedHttpActionDescriptor[] { directRouteAction });

            HttpActionDescriptor actionDescriptor = actionSelector.SelectAction(context);

            Assert.Same(directRouteAction, actionDescriptor);
        }

        [Fact]
        public void SelectAction_Throws_IfContextIsNull()
        {
            ApiControllerActionSelector actionSelector = new ApiControllerActionSelector();

            Assert.ThrowsArgumentNull(
                () => actionSelector.SelectAction(null),
                "controllerContext");
        }
    }
}
