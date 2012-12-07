// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Specialized;
using System.Web.Mvc;
using Microsoft.AspNet.Mvc.Facebook.ModelBinders;
using Microsoft.AspNet.Mvc.Facebook.Providers;
using Microsoft.AspNet.Mvc.Facebook.Test.Helpers;
using Microsoft.TestCommon;

namespace Microsoft.AspNet.Mvc.Facebook.Test
{
    public class FacebookRedirectContextModelBinderTest
    {
        [Fact]
        public void Constructor_ThrowsArgumentNullException()
        {
            Assert.ThrowsArgumentNull(() => new FacebookContextModelBinder(null), "config");
        }

        [Fact]
        public void BindModel_ReturnsExpectedFacebookRedirectContext()
        {
            FacebookConfiguration config = new FacebookConfiguration();
            config.AppId = "123456";
            config.ClientProvider = new DefaultFacebookClientProvider(config);
            FacebookRedirectContextModelBinder redirectContextBinder = new FacebookRedirectContextModelBinder(config);
            ControllerContext controllerContext = MockHelpers.CreateControllerContext(
                null,
                new NameValueCollection
                {
                    {"originUrl", "https://apps.facebook.com/123456/home/index"},
                    {"permissions", "email,user_likes"}
                });
            ModelBindingContext modelBindingContext = new ModelBindingContext();

            FacebookRedirectContext context = Assert.IsType<FacebookRedirectContext>(redirectContextBinder.BindModel(controllerContext, modelBindingContext));

            Assert.Equal("https://apps.facebook.com/123456/home/index", context.OriginUrl);
            Assert.Equal("https://www.facebook.com/dialog/oauth?redirect_uri=https%3A%2F%2Fapps.facebook.com%2F123456%2Fhome%2Findex&client_id=123456&scope=email%2Cuser_likes", context.RedirectUrl);
            Assert.Equal(2, context.RequiredPermissions.Length);
            Assert.Equal("email", context.RequiredPermissions[0]);
            Assert.Equal("user_likes", context.RequiredPermissions[1]);
            Assert.Same(config, context.Configuration);
        }

        [Fact]
        public void BindModel_ReturnsInvalidModelState_WhenOriginUrlIsNull()
        {
            FacebookConfiguration config = new FacebookConfiguration();
            config.AppId = "123456";
            config.ClientProvider = new DefaultFacebookClientProvider(config);
            FacebookRedirectContextModelBinder redirectContextBinder = new FacebookRedirectContextModelBinder(config);
            ControllerContext controllerContext = MockHelpers.CreateControllerContext(
                null,
                new NameValueCollection
                {
                    {"permissions", "email,user_likes"}
                });
            ModelBindingContext modelBindingContext = new ModelBindingContext();

            FacebookRedirectContext context = Assert.IsType<FacebookRedirectContext>(redirectContextBinder.BindModel(controllerContext, modelBindingContext));
            Assert.False(modelBindingContext.ModelState.IsValid);
        }

        [Fact]
        public void BindModel_ReturnsInvalidModelState_WhenOriginUrlIsExternal()
        {
            FacebookConfiguration config = new FacebookConfiguration();
            config.AppId = "123456";
            config.ClientProvider = new DefaultFacebookClientProvider(config);
            FacebookRedirectContextModelBinder redirectContextBinder = new FacebookRedirectContextModelBinder(config);
            ControllerContext controllerContext = MockHelpers.CreateControllerContext(
                null,
                new NameValueCollection
                {
                    {"originUrl", "https://example.com/123456/home/index"},
                    {"permissions", "email,user_likes"}
                });
            ModelBindingContext modelBindingContext = new ModelBindingContext();

            FacebookRedirectContext context = Assert.IsType<FacebookRedirectContext>(redirectContextBinder.BindModel(controllerContext, modelBindingContext));
            Assert.False(modelBindingContext.ModelState.IsValid);
        }

        [Fact]
        public void BindModel_ReturnsInvalidModelState_WhenPermissionsIsNull()
        {
            FacebookConfiguration config = new FacebookConfiguration();
            config.AppId = "123456";
            config.ClientProvider = new DefaultFacebookClientProvider(config);
            FacebookRedirectContextModelBinder redirectContextBinder = new FacebookRedirectContextModelBinder(config);
            ControllerContext controllerContext = MockHelpers.CreateControllerContext(
                null,
                new NameValueCollection
                {
                    {"originUrl", "https://apps.facebook.com/123456/home/index"}
                });
            ModelBindingContext modelBindingContext = new ModelBindingContext();

            FacebookRedirectContext context = Assert.IsType<FacebookRedirectContext>(redirectContextBinder.BindModel(controllerContext, modelBindingContext));
            Assert.False(modelBindingContext.ModelState.IsValid);
        }
    }
}