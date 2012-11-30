// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Web.Mvc;
using Facebook;
using Microsoft.AspNet.Mvc.Facebook.Client;

namespace Microsoft.AspNet.Mvc.Facebook.ModelBinders
{
    public class FacebookContextModelBinder : IModelBinder
    {
        private FacebookConfiguration _config;

        public FacebookContextModelBinder(FacebookConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            _config = config;
        }

        public virtual object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            FacebookClient client = _config.ClientProvider.CreateClient();
            dynamic signedRequest = FacebookRequestHelpers.GetSignedRequest(
                controllerContext.HttpContext,
                rawSignedRequest =>
                {
                    return client.ParseSignedRequest(rawSignedRequest);
                });
            if (signedRequest != null)
            {
                string accessToken = signedRequest.oauth_token;
                string userId = signedRequest.user_id;
                client.AccessToken = accessToken;
                return new FacebookContext
                {
                    Client = client,
                    SignedRequest = signedRequest,
                    AccessToken = accessToken,
                    UserId = userId,
                    Configuration = _config
                };
            }
            else
            {
                bindingContext.ModelState.AddModelError(bindingContext.ModelName, Resources.MissingSignedRequest);
            }

            return null;
        }
    }
}
