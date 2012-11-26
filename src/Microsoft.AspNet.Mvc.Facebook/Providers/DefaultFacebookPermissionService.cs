// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Facebook;
using Microsoft.AspNet.Mvc.Facebook.Client;
using Microsoft.AspNet.Mvc.Facebook.Models;

namespace Microsoft.AspNet.Mvc.Facebook.Providers
{
    public class DefaultFacebookPermissionService : IFacebookPermissionService
    {
        private FacebookConfiguration _config;

        public DefaultFacebookPermissionService(FacebookConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            _config = configuration;
        }

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

            IEnumerable<string> permissions;
            FacebookClient client = _config.ClientProvider.CreateClient();
            client.AccessToken = accessToken;
            IFacebookStorageProvider storage = _config.StorageProvider;
            if (storage != null)
            {
                UserPermissions userPermissions = storage.GetAsync(userId).Result as UserPermissions;
                if (userPermissions == null)
                {
                    userPermissions = new UserPermissions { Id = userId };
                    userPermissions.SetPermissions(client.GetCurrentUserPermissions());
                    storage.AddOrUpdateAsync(userId, userPermissions).Wait();
                }
                permissions = userPermissions.ParsePermissions();
            }
            else
            {
                permissions = client.GetCurrentUserPermissions();
            }

            return permissions;
        }
    }
}
