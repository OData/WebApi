// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Facebook;
using Microsoft.AspNet.Mvc.Facebook.Client;
using Microsoft.AspNet.Mvc.Facebook.Models;

namespace Microsoft.AspNet.Mvc.Facebook.Providers
{
    /// <summary>
    /// Default implementation of <see cref="IFacebookPermissionService"/>.
    /// </summary>
    public class DefaultFacebookPermissionService : IFacebookPermissionService
    {
        private FacebookConfiguration _config;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultFacebookPermissionService" /> class.
        /// </summary>
        /// <param name="configuration">The <see cref="FacebookConfiguration"/>.</param>
        public DefaultFacebookPermissionService(FacebookConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            _config = configuration;
        }

        /// <summary>
        /// Gets the user permissions by calling the Facebook Graph API.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <param name="accessToken">The access token.</param>
        /// <returns>The user permissions.</returns>
        public virtual IEnumerable<string> GetUserPermissions(string userId, string accessToken)
        {
            if (userId == null)
            {
                throw new ArgumentNullException("userId");
            }

            if (accessToken == null)
            {
                throw new ArgumentNullException("accessToken");
            }

            FacebookClient client = _config.ClientProvider.CreateClient();
            client.AccessToken = accessToken;
            return client.GetCurrentUserPermissions();
        }
    }
}