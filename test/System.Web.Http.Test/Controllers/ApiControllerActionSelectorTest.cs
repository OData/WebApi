// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Web.Http.Controllers;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

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
            usersControllerDescriptor.HttpActionSelector = actionSelector;
            GetContext.ControllerDescriptor = usersControllerDescriptor;
            GetContext.Request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get
                };
            HttpControllerContext PostContext = ContextUtil.CreateControllerContext();
            usersControllerDescriptor.HttpActionSelector = actionSelector;
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
        public void SelectAction_Throws_IfContextIsNull()
        {
            ApiControllerActionSelector actionSelector = new ApiControllerActionSelector();

            Assert.ThrowsArgumentNull(
                () => actionSelector.SelectAction(null),
                "controllerContext");
        }
    }
}
